using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Doctor.Models.DrProgressModels;
using CloudClinic.Doctor.Models.CheckPatModels;
using CloudClinic.Doctor.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using CloudClinic.Doctor.Models.CheckPatModels.PatLabTestModels;
using CloudClinic.Shared;
using CloudClinic.Shared.AuthConstants;

namespace CloudClinic.Doctor.Controllers.DrStaffControllers
{
    [Route(Routes.DrLabStaffControllerRoute)]
    [ApiController]
    [Authorize(Roles = Roles.DrRole + "," + Roles.DrLabStaffRole)]
    public class DrLabStaffController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private object response;

        private CloudClinicDb db { get; set; }
        private IDrDetailsProvider drDetailsProvider { get; set; }
        private IDrClinicalDataService clinicalDataService { get; set; }
        private IDrAppointmentService drAppointmentService { get; set; }

        // for payment calculation
        private int totalLabFee { get; set; }

        public DrLabStaffController(
            CloudClinicDb Db,
            IDrDetailsProvider DrDetailsProvider,
            IDrClinicalDataService ClinicalDataService,
            IDrAppointmentService appointmentService,
            IWebHostEnvironment environment)
        {
            db = Db;
            drDetailsProvider = DrDetailsProvider;
            clinicalDataService = ClinicalDataService;
            drAppointmentService = appointmentService;
            _environment = environment;
        }

        // return referred lab tests used by labstaff....
        [HttpGet(Routes.GetReferredLabTestsRoute, Name = Routes.GetReferredLabTestsRoute)]
        public IActionResult GetReferredLabTests([FromQuery] int d, [FromQuery] int m, [FromQuery] int y, [FromQuery] string drPracTimeName)
        {
            string drId = drDetailsProvider.GetDrId(User);
            DateTime date = new DateTime(y, m, d);

            var q = (from rs in db.PatReports
                     where rs.DrId == new Guid(drId)
                     && rs.ReportDate.Date == date.Date
                     && rs.DrPracTimeName == drPracTimeName
                     && rs.IsVisible == true
                     join ts in db.PatLabTests on rs.PatReportId equals ts.PatReportId
                     join patu in db.AppUsers on rs.PatId equals patu.Id
                     select new ReferredLabTestViewModel
                     {
                         PatName = patu.FullName,
                         GuardianName = patu.GuardianName,
                         PatProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, patu.ProfilePicName),
                         PatPhoneNumber = patu.PhoneNumber,
                         LabTestId = ts.LabTestId,
                         LabTestName = ts.LabTestName,
                         LabTestResult = ts.LabTestResult,
                         LabTestFee = ts.LabTestFee,
                         AttachmentUri = HelperMethods.GenerateAttachmentUri(HttpContext, ts.AttachmentName),
                         LabTestReferredDateTime = ts.LabTestReferDateTime.ToShortDateString() +" "+ ts.LabTestReferDateTime.ToShortTimeString(),
                         LabTestSubmitDateTime = ts.LabTestSubmitDateTime.ToShortDateString() +" "+ ts.LabTestSubmitDateTime.ToShortTimeString(),
                         PatReportId = ts.PatReportId
                     });

            if (!q.Any())
            {
                response = HelperMethods.CreateResponse(
                   "404",
                   new ErrorModel { Type = ErrorTypes.NotFound, Message = "No reffered lab tests found or the session is expired" },
                   null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { allReferredLabTests = q.ToList() });

            return Ok(response);
        }

        // used by lab-staff when dr submit result and 
        // notify labStaff through signalR
        // then staff will use this to get updated labTest
        [HttpGet(Routes.GetReferredLabTestRoute, Name = Routes.GetReferredLabTestRoute)]
        public IActionResult GetReferredLabTest([FromQuery] Guid labTestId)
        {
            string drId = drDetailsProvider.GetDrId(User);

            var q = (from rs in db.PatReports
                     where rs.DrId == new Guid(drId)
                     && rs.IsVisible == true
                     join ts in db.PatLabTests on rs.PatReportId equals ts.PatReportId
                     where ts.LabTestId == labTestId
                     join patu in db.AppUsers on rs.PatId equals patu.Id
                     select new ReferredLabTestViewModel
                     {
                         PatName = patu.FullName,
                         GuardianName = patu.GuardianName,
                         PatProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, patu.ProfilePicName),
                         PatPhoneNumber = patu.PhoneNumber,
                         LabTestId = ts.LabTestId,
                         LabTestName = ts.LabTestName,
                         LabTestResult = ts.LabTestResult,
                         LabTestFee = ts.LabTestFee,
                         LabTestReferredDateTime = ts.LabTestReferDateTime.ToShortDateString() +" "+ ts.LabTestReferDateTime.ToShortTimeString(),
                         LabTestSubmitDateTime = ts.LabTestSubmitDateTime.ToShortDateString() +" "+ ts.LabTestSubmitDateTime.ToShortTimeString(),
                         AttachmentUri = HelperMethods.GenerateAttachmentUri(HttpContext, ts.AttachmentName),
                         PatReportId = ts.PatReportId
                     });

            if (!q.Any())
            {
                response = HelperMethods.CreateResponse(
                   "404",
                   new ErrorModel { Type = ErrorTypes.NotFound, Message = "No reffered lab test found" },
                   null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { labTest = q.FirstOrDefault() });

            return Ok(response);
        }

        // this is used for both create and update
        // because the lab tests are already enterd by the dr (referring labtests), 
        // staff can only add result to them..
        // ------------------------------------------------------
        // these submitted results will be saved at client side
        // so he can view it and change it on the same day only...
        [HttpPost(Routes.SubmitLabTestResultsRoute, Name = Routes.SubmitLabTestResultsRoute)]
        public IActionResult SubmitLabTestResults([FromForm] LabTestSubmitModel testsModel)
        {
            string drId = drDetailsProvider.GetDrId(User);

            //try
            //{
                PatReport patReport = db.PatReports.FirstOrDefault(r => r.PatReportId == testsModel.PatReportId
                      && r.DrId == new Guid(drId) && r.ReportDate.Date == DateTime.Now.Date);

                if (patReport is null)
                {
                    response = HelperMethods.CreateResponse(
                       "404",
                       new ErrorModel { Type = ErrorTypes.NotFound, Message = "Pat report not found or expired" },
                       null);

                    return Ok(response);
                }

                if (patReport.IsPending == false)
                {
                    response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = "this report is no more pending, contact your dr to reopen it" },
                        null);

                    return Ok(response);
                }

                PatLabTest labTest = db.PatLabTests.FirstOrDefault(t => t.LabTestId == testsModel.LabTestId && t.PatReportId == testsModel.PatReportId);

                if (labTest is null)
                {
                    response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = "This test was not found in referred tests." },
                        null);

                    return Ok(response);
                }

                // date is already checked for report
                //if (labTest.LabTestReferDateTime.Date < DateTime.Now.Date)
                //{
                //    response = HelperMethods.CreateResponse(
                //        "404",
                //        new ErrorModel { Type = ErrorTypes.NotFound, Message = "Report update session is expired." },
                //        null);

                //    return Ok(response);
                //}

                if (labTest.AttachmentName != null)
                {
                    HelperMethods.DeleteAttachment(labTest.AttachmentName, _environment);
                }

                if (testsModel.AttachmentFile != null)
                {
                    string attachmentName = HelperMethods.AddAttachment(testsModel.AttachmentFile, _environment);

                    if (attachmentName.StartsWith("error"))
                    {
                        response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = attachmentName },
                        null);

                        return Ok(response);
                    }
                    
                    labTest.AttachmentName = attachmentName;
                }

            // note old fee before update
            int oldTestFee = labTest.LabTestFee;

                labTest.LabTestResult = testsModel.LabTestResult;
                labTest.LabTestFee = testsModel.LabTestFee;
                labTest.LabTestSubmitDateTime = DateTime.Now;

                var p = (from v in db.Visits
                         where v.PatReportId == labTest.PatReportId
                         join pay in db.Payments on v.PaymentId equals pay.PaymentId
                         select pay).FirstOrDefault();

                if (p != null)
                {
                    p.LabFee = (p.LabFee == 0) ? testsModel.LabTestFee : (p.LabFee - oldTestFee) + testsModel.LabTestFee;
                }

                db.SaveChanges();

                response = HelperMethods.CreateResponse(
                        "200", null, new { 
                            submitDate = labTest.LabTestSubmitDateTime.ToShortDateString()+" "+labTest.LabTestSubmitDateTime.ToShortTimeString(),
                            attachmentUri = HelperMethods.GenerateAttachmentUri(HttpContext, labTest.AttachmentName)
                            });

                return Ok(response);
            //}
            //catch (Exception e)
            //{
            //    response = HelperMethods.CreateResponse(
            //            "400",
            //            new ErrorModel { Type = ErrorTypes.NotFound, Message = "Failed try again." },
            //            null);

            //    return Ok(response);
            //}
        }

        // used by Dr only
        // incase if he want to refresh localStorage for
        // a particular report
        [HttpGet(Routes.ViewLabTestsRoute, Name = Routes.ViewLabTestsRoute)]
        [Authorize(Roles = Roles.DrRole)]
        public IActionResult ViewLabTests([FromQuery] Guid reportId)
        {
            //var t = db.Database.BeginTransaction();

            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                var q = (from tests in db.PatLabTests
                         where tests.PatReportId == reportId
                         select new LabTestViewModel
                         {
                             LabTestId = tests.LabTestId,
                             PatReportId = tests.PatReportId,
                             LabTestName = tests.LabTestName,
                             LabTestResult = tests.LabTestResult,
                             LabTestFee = tests.LabTestFee,
                             AttachmentUri = HelperMethods.GenerateAttachmentUri(HttpContext, tests.AttachmentName),
                             LabTestReferDateTime = tests.LabTestReferDateTime.ToShortDateString() + " " + tests.LabTestReferDateTime.ToShortTimeString(),
                             LabTestSubmitDateTime = tests.LabTestSubmitDateTime.ToShortDateString() + " " + tests.LabTestSubmitDateTime.ToShortTimeString()
                         });
                //t.Commit();

                if (!q.Any())
                {
                    response = HelperMethods.CreateResponse(
                    "404", new ErrorModel { Type = "not found", Message = "labtests not found" }, null);

                    return Ok(response);
                }

                response = HelperMethods.CreateResponse("200", null, new { patLabTests = q.ToList() });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                    "400", new ErrorModel { Type = "", Message = "Some error" }, null);

                return Ok(response);
            }
        }

        // used by Dr only
        // for signalr updates from staff
        [HttpGet(Routes.ViewLabTestRoute, Name = Routes.ViewLabTestRoute)]
        [Authorize(Roles = Roles.DrRole)]
        public IActionResult ViewLabTest([FromQuery] Guid labTestId)
        {
            //var t = db.Database.BeginTransaction();

            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                var q = (from tests in db.PatLabTests
                         where tests.LabTestId == labTestId
                         select new LabTestViewModel
                         {
                             LabTestId = tests.LabTestId,
                             PatReportId = tests.PatReportId,
                             LabTestName = tests.LabTestName,
                             LabTestFee = tests.LabTestFee,
                             LabTestResult = tests.LabTestResult,
                             AttachmentUri = HelperMethods.GenerateAttachmentUri(HttpContext, tests.AttachmentName),
                             LabTestReferDateTime = tests.LabTestReferDateTime.ToShortDateString() + " " + tests.LabTestReferDateTime.ToShortTimeString(),
                             LabTestSubmitDateTime = tests.LabTestSubmitDateTime.ToShortDateString() + " " + tests.LabTestSubmitDateTime.ToShortTimeString()
                         });
                //t.Commit();

                if (!q.Any())
                {
                    response = HelperMethods.CreateResponse(
                    "404", new ErrorModel { Type = "not found", Message = "labtests not found" }, null);

                    return Ok(response);
                }

                response = HelperMethods.CreateResponse("200", null, new { patLabTest = q.First() });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                    "400", new ErrorModel { Type = "", Message = "Some error" }, null);

                return Ok(response);
            }
        }
    }
}