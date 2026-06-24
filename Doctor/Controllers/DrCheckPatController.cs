using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Doctor.Models.DrProgressModels;
using CloudClinic.Doctor.Models.CheckPatModels.PatDrugModels;
using CloudClinic.Doctor.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using CloudClinic.Doctor.Models.CheckPatModels.ReportModels;
using CloudClinic.Doctor.Models.CheckPatModels.PatLabTestModels;
using CloudClinic.Doctor.Models.CheckPatModels.PatXrayModels;
using CloudClinic.Shared;
using CloudClinic.Shared.AuthConstants;
using Microsoft.EntityFrameworkCore;

namespace CloudClinic.Doctor.Controllers
{
    [Authorize(Roles = Roles.DrRole)]
    [Route(Routes.DrCheckPatControllerRoute)]
    [ApiController]
    public class DrCheckPatController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private object response;

        private CloudClinicDb db { get; set; }
        private IDrDetailsProvider drDetailsProvider { get; set; }
        private IDrAppointmentService drAppointmentService { get; set; }

        public DrCheckPatController(
            CloudClinicDb Db,
            IDrDetailsProvider DrDetailsProvider,
            IDrAppointmentService appointmentService,
            IWebHostEnvironment environment)
        {
            db = Db;
            drDetailsProvider = DrDetailsProvider;
            drAppointmentService = appointmentService;
            _environment = environment;
        }

        // this action will gets invoked when dr click on a patient appointment from appointments list
        [HttpGet(Routes.InitPatReportRoute, Name = Routes.InitPatReportRoute)]
        public async Task<IActionResult> InitPatReport([FromQuery] string patId, [FromQuery] Guid appointId, [FromQuery] string drPracTimeName)
        {
            // unique index: appointId
            var t = await db.Database.BeginTransactionAsync();

            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                InitReportViewModel initReport;

                PatReport report = await db.PatReports.FirstOrDefaultAsync(r => r.AppointId == appointId
                    && r.DrId == new Guid(drId)
                    && r.ReportDate.Date == DateTime.Now.Date);

                if (report != null)
                {
                    initReport = new InitReportViewModel
                    {
                        PatReportId = report.PatReportId,
                        PatId = report.PatId,
                        IsPending = report.IsPending,
                        AppointId = report.AppointId,
                        DrPracTimeName = report.DrPracTimeName
                    };

                    var v = await db.Visits.FirstOrDefaultAsync(v => v.PatReportId == report.PatReportId);

                    if (v != null)
                    {
                        initReport.VisitId = v.VisitId;
                    }

                    var p = await db.Payments.FirstOrDefaultAsync(p => p.PaymentId == v.PaymentId);

                    if (p != null)
                    {
                        initReport.PaymentId = p.PaymentId;
                    }

                    // before return let make it current
                    drAppointmentService.ChangeAppointmentStatus(db, drId, appointId, "current");

                    response = HelperMethods.CreateResponse(
                        "200", null, new { initReport });

                    return Ok(response);
                }

                PatReport patReport = new PatReport
                {
                    DrId = new Guid(drId),
                    PatId = patId,
                    AppointId = appointId,
                    DrPracTimeName = drPracTimeName,
                    IsPending = true,
                    // initially it is visible
                    IsVisible = true,
                    ReportDate = DateTime.Now
                };

                await db.PatReports.AddAsync(patReport);
                await db.SaveChangesAsync();

                // first generate payment, so paymentId will be available to Visit object...
                // the payment date will be changed to current dateTime at confirm method..
                Payment payment = new Payment
                {
                    DrFee = drDetailsProvider.GetDrFee(db, drId),
                    Paid = false,
                    PaymentDate = DateTime.Now
                };

                await db.Payments.AddAsync(payment);
                await db.SaveChangesAsync();

                // create the visit and init payment...
                Visit visit = new Visit
                {
                    DrId = new Guid(drId),
                    PatId = patId,
                    PatReportId = patReport.PatReportId,
                    AppointId = appointId,
                    PaymentId = payment.PaymentId,
                    DrPracTimeName = drPracTimeName,
                    VisitDate = DateTime.Now
                };

                await db.Visits.AddAsync(visit);
                await db.SaveChangesAsync();

                // this is the current appointment
                // this value will used to determine the current sequence no for pending
                // appointments holders
                //drAppointmentService.ChangeAppointmentStatus(db, drId, appointId, "current"); // change separately from client
                
                await t.CommitAsync();

                initReport = new InitReportViewModel
                {
                    PatReportId = patReport.PatReportId,
                    IsPending = patReport.IsPending,
                    PatId = patReport.PatId,
                    VisitId = visit.VisitId,
                    PaymentId = payment.PaymentId,
                    AppointId = patReport.AppointId,
                    DrPracTimeName = patReport.DrPracTimeName
                };

                response = HelperMethods.CreateResponse("200", null, new { initReport });
                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                    "400", new ErrorModel { Type = "init report error", Message = "can't init report" + e.Message }, null);

                return Ok(response);
            }
        }

        #region GenerateReport Method
        // this method will always called after InitPatReport()
        // this can also be used for updating reports
        // reports contents will be available with dr on the client
        // when he submit, the specified report (model.reportId) will find and 
        // updated from the model data...
        // ----------------------------------------------------------
        // this method only handle pat report(e.g diagnosis), pat drugs
        // will be manipulated by separate methods by dr..
        // lab tests and xray are
        // handled(refer, submit, update, view, remove) separatly for the
        // specified report, by staff controllers(dr also has access to it).
        // -----------------------------------------
        // there will be three submits, 1. report, 2. drugs 3. tests, 4. xray each one do
        // submit and bring the view to client...separately..
        [HttpPost(Routes.GeneratePatReportRoute, Name = Routes.GeneratePatReportRoute)]
        
        public async Task<IActionResult> GeneratePatReport([FromBody] PatReportCreateModel model)
        {
            string drId = drDetailsProvider.GetDrId(User);

            var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                PatReport patReport = db.PatReports.FirstOrDefault(r => r.DrId == new Guid(drId) && r.PatReportId == model.PatReportId);

                if (patReport is null)
                {
                    response = HelperMethods.CreateResponse(
                    "200",
                    new ErrorModel { Type = "server error", Message = "Cant save report, init report not found" },
                    null);

                    return Ok(response);
                }

                if (patReport.ReportDate.Date < DateTime.Now.Date)
                {
                    response = HelperMethods.CreateResponse(
                    "200",
                    new ErrorModel { Type = "server error", Message = "This session is expired,..." },
                    null);

                    return Ok(response);
                }

                if (db.PatLabTests.Any(t => t.PatReportId == model.PatReportId && t.LabTestResult == "pending"))
                {
                    response = HelperMethods.CreateResponse(
                    "200",
                    new ErrorModel { Type = "server error", Message = "lab tests are pending." },
                    null);

                    return Ok(response);
                }

                if (db.PatXrays.Any(x => x.PatReportId == model.PatReportId && x.XrayResult == "pending"))
                {
                    response = HelperMethods.CreateResponse(
                    "200",
                    new ErrorModel { Type = "server error", Message = "xrays are pending." },
                    null);

                    return Ok(response);
                }

                patReport.Symptoms = model.Symptoms;
                patReport.Diagnosis = model.Diagnosis;
                patReport.Remarks = model.Remarks;
                patReport.ReportDate = DateTime.Now;
                patReport.IsPending = false;

                db.SaveChanges();

                //drAppointmentService.ChangeAppointmentStatus(db, drId, patReport.AppointId, "done"); // change separately from client

                await transaction.CommitAsync();

                response = HelperMethods.CreateResponse(
                       "200", null, new { 
                           reportDate = patReport.ReportDate.ToShortDateString() + " " + patReport.ReportDate.ToShortTimeString()
                       });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                "200",
                new ErrorModel { Type = "server error", Message = "failed try again" },
                null);

                return Ok(response);
            }
        }
        #endregion 

        // Reopen report for labtests results and xray results changing
        // see: Policy For Pending Reports; in sticky notes
        [HttpGet(Routes.ReOpenReportRoute, Name = Routes.ReOpenReportRoute)]
        public async Task<IActionResult> ReOpenReport([FromQuery] string reportId)
        {
            var t = await db.Database.BeginTransactionAsync();

            try
            {
                string drId = drDetailsProvider.GetDrId(User);
                PatReport patReport = db.PatReports.FirstOrDefault(r => r.PatReportId == new Guid(reportId)
                                      && r.DrId == new Guid(drId));

                if (patReport is null)
                {
                    response = HelperMethods.CreateResponse(
                        "404", new ErrorModel { Type = "ReportNotfound", Message = "Report Not found" }, null);

                    return Ok(response);
                }

                if (patReport.ReportDate.Date < DateTime.Now.Date)
                {
                    response = HelperMethods.CreateResponse(
                        "400", new ErrorModel { Type = "", Message = "This session is expired" }, null);

                    return Ok(response);
                }

                patReport.IsPending = true;

                drAppointmentService.ChangeAppointmentStatus(db, drId, patReport.AppointId, "waiting");

                await db.SaveChangesAsync();

                await t.CommitAsync();

                response = HelperMethods.CreateResponse("200", null, new { reOpened = true });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                    "400", new ErrorModel { Type = "", Message = "Failed try again" }, null);

                return Ok(response);
            }
        }

        [HttpPost(Routes.ReferLabTestRoute, Name = Routes.ReferLabTestRoute)]
        [Authorize(Roles=Roles.DrRole)]
        public IActionResult ReferLabTests([FromBody] LabTestReferModel testsModel)
        {
            string drId = drDetailsProvider.GetDrId(User);

            var t = db.Database.BeginTransaction();
            try
            {
                var r = db.PatReports.FirstOrDefault(r => r.PatReportId == testsModel.PatReportId);

                if (r is null)
                {
                    response = HelperMethods.CreateResponse(
                        "400", new ErrorModel { Type = "", Message = "Report not found to add referred tests" }, null);

                    return Ok(response);
                }

                PatLabTest labTest = new PatLabTest
                {
                    LabTestName = testsModel.LabTestName,
                    LabTestResult = "pending",
                    LabTestFee = 0,
                    LabTestReferDateTime = DateTime.Now,
                    LabTestSubmitDateTime = new DateTime(1, 1, 1),
                    PatReportId = testsModel.PatReportId,
                };

                db.PatLabTests.Add(labTest);
                db.SaveChanges();

                //drAppointmentService.ChangeAppointmentStatus(db, drId, r.AppointId, "waiting"); // change separatly on client

                r.IsPending = true;
                db.SaveChanges();

                t.Commit();

                response = HelperMethods.CreateResponse(
                    "200", null, new { 
                        labTestId = labTest.LabTestId,
                        referredDate = labTest.LabTestReferDateTime.ToShortDateString() + " " + labTest.LabTestReferDateTime.ToShortTimeString() 
                    });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                    "400", new ErrorModel { Type = "", Message = "Some thing bad happend" }, null);
                return Ok(response);
            }
        }

        [HttpGet(Routes.RemoveLabTestRoute, Name = Routes.RemoveLabTestRoute)]
        public IActionResult RemoveLabTest([FromQuery] string labTestId)
        {
            string drId = drDetailsProvider.GetDrId(User);

            PatLabTest patLabTest = db.PatLabTests.FirstOrDefault(t => t.LabTestId == new Guid(labTestId));
            
            if (patLabTest is null)
            {
                response = HelperMethods.CreateResponse(
                "404", new ErrorModel { Type = "", Message = "Labtest not found" }, null);

                return Ok(response);
            }

            db.PatLabTests.Remove(patLabTest);

            var p = (from v in db.Visits
                     where v.PatReportId == patLabTest.PatReportId
                     join pay in db.Payments on v.PaymentId equals pay.PaymentId
                     select pay).FirstOrDefault();

            if (p != null)
            {
                p.LabFee = (p.LabFee == 0) ? 0 : (p.LabFee < patLabTest.LabTestFee) ? 0 : p.LabFee - patLabTest.LabTestFee;
            }

            if (patLabTest.AttachmentName != null)
            {
                HelperMethods.DeleteAttachment(patLabTest.AttachmentName, _environment);
            }

            db.SaveChanges();
            
            response = HelperMethods.CreateResponse("200", null, new { deleted = true });
            return Ok(response);
        }

        [HttpPost(Routes.ReferXrayRoute, Name = Routes.ReferXrayRoute)]
        public IActionResult ReferXray([FromBody] XrayReferModel xrayModel)
        {
            string drId = drDetailsProvider.GetDrId(User);

            var t = db.Database.BeginTransaction();
            try
            {
                var r = db.PatReports.FirstOrDefault(r => r.PatReportId == xrayModel.PatReportId);

                if (r is null)
                {
                    response = HelperMethods.CreateResponse(
                        "404", new ErrorModel { Type = "", Message = "Report not found to add referred xray" }, null);

                    return Ok(response);
                }

                PatXray xray = new PatXray
                {
                    XrayName = xrayModel.XrayName,
                    XrayType = xrayModel.XrayType,
                    XrayResult = "pending",
                    XrayFee = 0,
                    XrayReferDateTime = DateTime.Now,
                    XraySubmitDateTime = new DateTime(1,1,1),
                    PatReportId = xrayModel.PatReportId,
                };

                db.PatXrays.Add(xray);
                db.SaveChanges();

                r.IsPending = true;
                db.SaveChanges();

                // in case if the report submitted (done) and then the refer test or xray is changed. 
                //_ = drAppointmentService.ChangeAppointmentStatus(db, drId, r.AppointId, "waiting");

                t.Commit();
                response = HelperMethods.CreateResponse(
                    "200",
                    null,
                    new 
                    { 
                        xrayId = xray.XrayId,
                        referredDateTime = xray.XrayReferDateTime.ToShortDateString() + " " + xray.XrayReferDateTime.ToShortTimeString() });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                    "400", new ErrorModel { Type = "", Message = "Bad things always bad, try again" }, null);

                return Ok(response);
            }
        }

        [HttpGet(Routes.RemoveXrayRoute, Name = Routes.RemoveXrayRoute)]
        [Authorize(Roles=Roles.DrRole)]
        public IActionResult RemoveXray([FromQuery] Guid xrayId)
        {
            string drId = drDetailsProvider.GetDrId(User);

            PatXray patXray = db.PatXrays.FirstOrDefault(x => x.XrayId == xrayId);

            if (patXray is null)
            {
                response = HelperMethods.CreateResponse(
                    "404", new ErrorModel { Type = "Notfound", Message = "xray not found" }, null);

                return Ok(response);
            }

            db.PatXrays.Remove(patXray);

            var p = (from v in db.Visits
                     where v.PatReportId == patXray.PatReportId
                     join pay in db.Payments on v.PaymentId equals pay.PaymentId
                     select pay).FirstOrDefault();

            if (p != null)
            {
                p.XrayFee = (p.XrayFee == 0) ? 0 : (p.XrayFee < patXray.XrayFee) ? 0 : p.XrayFee - patXray.XrayFee;
            }

            if (patXray.AttachmentName != null)
            {
                HelperMethods.DeleteAttachment(patXray.AttachmentName, _environment);
            }

            db.SaveChanges();

            response = HelperMethods.CreateResponse("200", null, new { deleted = true });

            return Ok(response);
        }

        // adding one by one is better than bulk add, cause availquantity can be checked easily for individual drug
        // in bulk add, if one drug is not available, then all drugs could not be added...
        // in bulk add, update will also be in bulk, if one need to be updated, all will be deleted and then 
        // each will be added again...
        [HttpPost(Routes.AddPatDrugRoute, Name = Routes.AddPatDrugRoute)]
        public async Task<IActionResult> AddPatDrug([FromBody] PatDrugCreateModel patDrugModel)
        {
            var t = await db.Database.BeginTransactionAsync();
            string[] array = new string[3];
            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                PatReport patReport = db.PatReports.FirstOrDefault(r => r.PatReportId == patDrugModel.PatReportId
                      && r.DrId == new Guid(drId));

                if (patReport is null)
                {
                    response = HelperMethods.CreateResponse(
                    "404", new ErrorModel { Type = "", Message = "Report Not found" }, null);
                    return Ok(response);
                }

                if (patReport.ReportDate.Date < DateTime.Now.Date)
                {
                    response = HelperMethods.CreateResponse(
                    "404", new ErrorModel { Type = "", Message = "This session is expired,..." }, null);

                    return Ok(response);
                }

                array = parsePatDrugName(patDrugModel.PatDrugName);

                Drug drug = db.Drugs.FirstOrDefault(d => d.DrugName == array[0] && d.DrugType == array[1]
                && d.DrugWeight == array[2] && d.DrId == new Guid(drId));

                // if it is added from his store
                if (drug != null)
                {
                    if (drug.DrugAvailQuantity < patDrugModel.DrugGivenQuantity)
                    {
                        response = HelperMethods.CreateResponse(
                           "404",
                           new ErrorModel { Type = "", Message = "Insufficient quantity" },
                           null);

                        return Ok(response);
                    }

                    drug.DrugAvailQuantity = drug.DrugAvailQuantity - patDrugModel.DrugGivenQuantity;
                    db.SaveChanges();

                    var p = (from v in db.Visits
                             where v.PatReportId == patDrugModel.PatReportId
                             join pay in db.Payments on v.PaymentId equals pay.PaymentId
                             select pay).FirstOrDefault();

                    if (p != null)
                    {
                        p.DrugsPrice += drug.DrugPurchasePrice * patDrugModel.DrugGivenQuantity;
                        db.SaveChanges();
                    }


                } // if ends

                PatDrug patDrug = new PatDrug
                {
                    PatDrugName = patDrugModel.PatDrugName,
                    DrugGivenQuantity = patDrugModel.DrugGivenQuantity,
                    PatDrugDosage = patDrugModel.PatDrugDosage,
                    PatDrugInstruction = patDrugModel.PatDrugInstruction,
                    PatDrugTime = patDrugModel.PatDrugTime,
                    PatReportId = patDrugModel.PatReportId,
                };

                db.PatDrugs.Add(patDrug);
                db.SaveChanges();

                await t.CommitAsync();

                response = HelperMethods.CreateResponse("200", null, new { patDrugId = patDrug.PatDrugId });
                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                    "400",
                    new ErrorModel { Type = "", Message = "failed try again " + patDrugModel.PatDrugName + e.Message },
                    new { array = parsePatDrugName(patDrugModel.PatDrugName) });

                return Ok(response);
            }
        }

        [HttpPost(Routes.UpdatePatDrugRoute, Name = Routes.UpdatePatDrugRoute)]
        public async Task<IActionResult> UpdatePatDrug([FromBody] PatDrugUpdateModel patDrugModel)
        {
            var t = await db.Database.BeginTransactionAsync();

            try
            {
                string PrevDrugName = "";
                int PrevDrugGivenQuantity;

                string drId = drDetailsProvider.GetDrId(User);

                PatReport patReport = db.PatReports.FirstOrDefault(r => r.PatReportId == patDrugModel.PatReportId
                                      && r.DrId == new Guid(drId));

                if (patReport is null)
                {
                    response = HelperMethods.CreateResponse(
                        "404", new ErrorModel { Type = "ReportNotfound", Message = "Report Not found" }, null);

                    return Ok(response);
                }

                if (patReport.ReportDate.Date < DateTime.Now.Date)
                {
                    response = HelperMethods.CreateResponse(
                    "400", new ErrorModel { Type = "", Message = "This session is expired" }, null);

                    return Ok(response);
                }

                PatDrug patDrug = db.PatDrugs.FirstOrDefault(pd => pd.PatDrugId == patDrugModel.PatDrugId);

                // let capture the previous quantity and name
                PrevDrugGivenQuantity = patDrug.DrugGivenQuantity;
                PrevDrugName = patDrug.PatDrugName;

                patDrug.PatDrugName = patDrugModel.PatDrugName;
                patDrug.DrugGivenQuantity = patDrugModel.DrugGivenQuantity;
                patDrug.PatDrugDosage = patDrugModel.PatDrugDosage;
                patDrug.PatDrugInstruction = patDrugModel.PatDrugInstruction;
                patDrug.PatDrugTime = patDrugModel.PatDrugTime;

                db.SaveChanges();


                var p = (from v in db.Visits
                         where v.PatReportId == patDrugModel.PatReportId
                         join pay in db.Payments on v.PaymentId equals pay.PaymentId
                         select pay).FirstOrDefault();

                // if it is from his store (for new drug)
                string[] array = parsePatDrugName(patDrugModel.PatDrugName);

                Drug newDrug = db.Drugs.FirstOrDefault(d => d.DrugName == array[0] && d.DrugType == array[1]
                && d.DrugWeight == array[2] && d.DrId == new Guid(drId));

                if (newDrug != null)
                {
                    if (newDrug.DrugAvailQuantity < patDrugModel.DrugGivenQuantity)
                    {
                        response = HelperMethods.CreateResponse(
                           "404",
                           new ErrorModel { Type = "", Message = "Insufficient quantity" },
                           null);

                        return Ok(response);
                    }

                    newDrug.DrugAvailQuantity = newDrug.DrugAvailQuantity - patDrugModel.DrugGivenQuantity;
                    db.SaveChanges();

                    p.DrugsPrice += newDrug.DrugPurchasePrice * patDrugModel.DrugGivenQuantity;
                    db.SaveChanges();
                }

                array = parsePatDrugName(PrevDrugName);

                Drug prevDrug = db.Drugs.FirstOrDefault(d => d.DrugName == array[0] && d.DrugType == array[1]
                && d.DrugWeight == array[2] && d.DrId == new Guid(drId));

                if (prevDrug != null)
                {
                    if (p != null)
                    {
                        p.DrugsPrice -= prevDrug.DrugPurchasePrice * PrevDrugGivenQuantity;
                        db.SaveChanges();
                    }


                    prevDrug.DrugAvailQuantity += PrevDrugGivenQuantity;
                    db.SaveChanges();
                }

                await t.CommitAsync();
                response = HelperMethods.CreateResponse(
                       "200", null, new { updated = true });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                       "400",
                       new ErrorModel { Type = "", Message = "Unknow error" },
                       null);

                return Ok(response);
            }
        }

        [HttpGet(Routes.RemovePatDrugRoute, Name = Routes.RemovePatDrugRoute)]
        public async Task<IActionResult> RemovePatDrug([FromQuery] Guid patDrugId)
        {
            var t = await db.Database.BeginTransactionAsync();

            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                PatDrug patDrug = db.PatDrugs.FirstOrDefault(pd => pd.PatDrugId == patDrugId);

                if (patDrug is null)
                {
                    response = HelperMethods.CreateResponse(
                    "404", new ErrorModel { Type = "", Message = "This pat drug not found" }, null);

                    return Ok(response);
                }

                PatReport patReport = db.PatReports.FirstOrDefault(r => r.PatReportId == patDrug.PatReportId
                                      && r.DrId == new Guid(drId));

                if (patReport is null)
                {
                    response = HelperMethods.CreateResponse(
                    "400", new ErrorModel { Type = "", Message = "Report is not found" }, null);

                    return Ok(response);
                }

                if (patReport.ReportDate.Date < DateTime.Now.Date)
                {
                    response = HelperMethods.CreateResponse(
                    "400", new ErrorModel { Type = "", Message = "This session is expired" }, null);

                    return Ok(response);
                }

                // if it is from his store (for new drug)
                string[] array = parsePatDrugName(patDrug.PatDrugName);

                Drug drug = db.Drugs.FirstOrDefault(d => d.DrugName == array[0] && d.DrugType == array[1]
                && d.DrugWeight == array[2] && d.DrId == new Guid(drId));

                if (drug != null)
                {
                    // add quantity back, because it was not sold...
                    drug.DrugAvailQuantity = drug.DrugAvailQuantity + patDrug.DrugGivenQuantity;
                    db.SaveChanges();

                    var p = (from v in db.Visits
                             where v.PatReportId == patDrug.PatReportId
                             join pay in db.Payments on v.PaymentId equals pay.PaymentId
                             select pay).FirstOrDefault();

                    if (p != null)
                    {
                        p.DrugsPrice =
                            (p.DrugsPrice == 0 || p.DrugsPrice < drug.DrugPurchasePrice) ? 0 : p.DrugsPrice - drug.DrugPurchasePrice * patDrug.DrugGivenQuantity;
                    }

                    db.SaveChanges();
                }


                db.PatDrugs.Remove(patDrug);
                db.SaveChanges();

                await t.CommitAsync();
                response = HelperMethods.CreateResponse("200", null, new { removed = true });
                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                    "400", new ErrorModel { Type = "", Message = "Some errors occurred!" }, null);

                return Ok(response);
            }
        }

        // remove the specified report
        [HttpGet(Routes.CancelReportRoute, Name = Routes.CancelReportRoute)]
        public IActionResult CancelReport([FromQuery] Guid reportId)
        {
            string drId = drDetailsProvider.GetDrId(User);

            PatReport patReport = db.PatReports.FirstOrDefault(r => r.PatReportId == reportId
                                  && r.DrId == new Guid(drId)
                                  && r.ReportDate.Date == DateTime.Now.Date);

            if (patReport == null)
            {
                response = HelperMethods.CreateResponse(
                    "404", new ErrorModel { Type = "", Message = "Report is not found." }, null);

                return Ok(response);
            }

            // delete every related data including attachments
            var tests = db.PatLabTests.Where(t => t.PatReportId == reportId);
            var xrays = db.PatXrays.Where(x => x.PatReportId == reportId);
            var drugs = db.PatDrugs.Where(d => d.PatReportId == reportId);
            var visit = db.Visits.FirstOrDefault(v => v.PatReportId == reportId);
            var payment = db.Payments.FirstOrDefault(p => p.PaymentId == visit.PaymentId);

            if (xrays.Any())
            {
                db.PatXrays.RemoveRange(xrays);
            }

            if (tests.Any())
            {
                db.PatLabTests.RemoveRange(tests);
            }

            if (drugs.Any())
            {
                db.PatDrugs.RemoveRange(drugs);
            }

            if (payment != null)
            {
                db.Payments.Remove(payment);
            }

            if (visit != null)
            {
                db.Visits.Remove(visit);
            }

            db.PatReports.Remove(patReport);
            db.SaveChanges();

            // mark it as pending
            // drAppointmentService.ChangeAppointmentStatus(db, drId, patReport.AppointId, "pending");

            response = HelperMethods.CreateResponse("200", null, new { removed = true });
            return Ok(response);
        }

        [HttpGet(Routes.ChangeAppointStatusRoute, Name = Routes.ChangeAppointStatusRoute)]
        public IActionResult ChangeAppointStatus([FromQuery] Guid appointId, [FromQuery] string status)
        {
            string drId = drDetailsProvider.GetDrId(User);
            // at the time when appoint is clicked report is not yet inited,
            // the changAppointStatus() will check for today date of appointment instead
            //
            //PatReport patReport = db.PatReports.FirstOrDefault(r => r.AppointId == appointId
            //                      && r.DrId == new Guid(drId)
            //                      && r.ReportDate.Date == DateTime.Now.Date);

            // also check for expiration
            var r = drAppointmentService.ChangeAppointmentStatus(db, drId, appointId, status);
            if (r is null)
            {
                response = HelperMethods.CreateResponse(
                    "400",
                    new ErrorModel { Type = "", Message = "Appointment is not found or expired." },
                    null);
                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { changed = true });
            return Ok(response);
        }

        [HttpGet(Routes.GetPatMedHistoryRoute, Name = Routes.GetPatMedHistoryRoute)]
        public IActionResult GetPatMedHistory([FromQuery] string patId, [FromQuery] int topNo = 10)
        {
            try
            {
                var q = (from r in db.PatReports
                         where r.PatId == patId
                         && r.ReportDate.Date < DateTime.Now.Date
                         && r.IsVisible == true // visisbility can be true only of none deleted rs
                         join dr in db.DrDetails on r.DrId equals dr.DrId
                         join dru in db.AppUsers on dr.UserId equals dru.Id
                         select new PatMedHistoryModel
                         {
                             DrName = dru.FullName,
                             DrSpecialty = dr.DrSpecialty,
                             DrAddress = dru.Street + ", " + dru.City + ", " + dru.Province,
                             PatId = patId,
                             Diagnosis = r.Diagnosis,
                             ReportId = r.PatReportId,
                             ReportDate = r.ReportDate.ToString()
                         });

                if (!q.Any())
                {
                    response = HelperMethods.CreateResponse(
                    "404",
                    new ErrorModel { Type = "", Message = "Not found any history or pat donot want to share his/her medical history" },
                    null);

                    return Ok(response);
                }

                response = HelperMethods.CreateResponse("200", null, new { patMedHistory = q.Take(topNo).ToList() });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                    "400", new ErrorModel { Type = "", Message = "Some error" }, null);

                return Ok(response);
            }
        }

        // retriev detail of report
        // when patReportId is provided from pat history or today reports (IsPending==true or false(done))
        [HttpGet(Routes.ViewPatReportRoute, Name = Routes.ViewPatReportRoute)]
        public IActionResult ViewPatReport([FromQuery] Guid reportId)
        {
            var t = db.Database.BeginTransaction();

            try
            {
                string drId = drDetailsProvider.GetDrId(User);
                ReportViewModel patReport;

                var reportQuery = (from rs in db.PatReports
                          where rs.PatReportId == reportId
                          && rs.IsVisible == true
                          join drs in db.DrDetails on rs.DrId equals drs.DrId
                          join dru in db.AppUsers on drs.UserId equals dru.Id
                          join pats in db.AppUsers on rs.PatId equals pats.Id
                          select new ReportViewModel
                          {
                              PatReportId = rs.PatReportId,
                              DrName = dru.FullName,
                              DrId = drs.DrId,
                              DrProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, dru.ProfilePicName),
                              DrSpecialty = drs.DrSpecialty,
                              DrAddress = dru.Street + ", " + dru.City + ", " + dru.Province,
                              PatId = rs.PatId,
                              PatName = pats.FullName,
                              PatDOB = pats.DOB.ToShortDateString(),
                              PatGender = pats.Gender,
                              PatAddress = pats.Street + ", " + pats.City + ", " + pats.Province,
                              ReportDate = rs.ReportDate.ToString(),
                              Symptoms = rs.Symptoms,
                              Diagnosis = rs.Diagnosis,
                              Remarks = rs.Remarks,
                              AppointId = rs.AppointId
                          });

                var labTestsQuery = (from tests in db.PatLabTests
                                     where tests.PatReportId == reportId
                                     select new LabTestViewModel
                                     {
                                         LabTestId = tests.LabTestId,
                                         PatReportId = tests.PatReportId,
                                         LabTestName = tests.LabTestName,
                                         LabTestResult = tests.LabTestResult,
                                         LabTestReferDateTime = tests.LabTestReferDateTime.ToString(),
                                         LabTestSubmitDateTime = tests.LabTestSubmitDateTime.ToString(),
                                         AttachmentUri = HelperMethods.GenerateAttachmentUri(HttpContext, tests.AttachmentName)
                                     });

                var xrayQuery = (from xrays in db.PatXrays
                                 where xrays.PatReportId == reportId
                                 select new XrayViewModel
                                 {
                                     XrayId = xrays.XrayId,
                                     XrayReferredDateTime = xrays.XrayReferDateTime.ToString(),
                                     XraySubmitDateTime = xrays.XraySubmitDateTime.ToString(),
                                     XrayName = xrays.XrayName,
                                     XrayType = xrays.XrayType,
                                     XrayResult = xrays.XrayResult,
                                     XrayFee = xrays.XrayFee,
                                     AttachmentUri = HelperMethods.GenerateAttachmentUri(HttpContext, xrays.AttachmentName)
                                 });


                var drugsQuery = (from pdrugs in db.PatDrugs
                                     where pdrugs.PatReportId == reportId
                                     select new PatDrugViewModel
                                     {
                                         PatDrugName = pdrugs.PatDrugName,
                                         DrugGivenQuantity = pdrugs.DrugGivenQuantity,
                                         PatDrugDosage = pdrugs.PatDrugDosage,
                                         PatDrugInstruction = pdrugs.PatDrugInstruction,
                                         PatDrugTime = pdrugs.PatDrugTime
                                     });
                t.Commit();

                if (!reportQuery.Any())
                {
                    response = HelperMethods.CreateResponse(
                    "404", new ErrorModel { Type = "", Message = "report not found" }, null);

                    return Ok(response);
                }

                patReport = reportQuery.First();

                if (labTestsQuery.Any())
                {
                    patReport.PatLabTests = labTestsQuery.ToList();
                }

                if (drugsQuery.Any())
                {
                    patReport.PatDrugs = drugsQuery.ToList();
                }

                response = HelperMethods.CreateResponse("200", null, new { patReport });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                    "400", new ErrorModel { Type = "", Message = "Some error" }, null);

                return Ok(response);
            }
        }

        private string[] parsePatDrugName(string patDrugName)
        {
            string[] array = patDrugName.Split(' ');

            string drugName = array[0].Trim();
            string type = array[1].Replace('(', ' ').Replace(')', ' ').Trim();
            string weight = array[2].Replace('(', ' ').Replace(')', ' ').Trim();

            return new string[] { drugName, type, weight};
        }
    }
}

        