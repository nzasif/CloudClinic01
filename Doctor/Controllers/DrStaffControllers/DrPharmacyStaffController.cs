using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Doctor.Models.CheckPatModels.PatDrugModels;
using CloudClinic.Doctor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using CloudClinic.Doctor.Models.PharmacyModels;
using CloudClinic.Shared.AuthConstants;
using CloudClinic.Shared;

namespace CloudClinic.Doctor.Controllers.DrStaffControllers
{
    [Route(Routes.DrPharmacyStaffControllerRoute)]
    [ApiController]
    [Authorize(Roles = Roles.DrPharmacyStaffRole + "," + Roles.DrRole)]
    public class DrPharmacyStaffController : ControllerBase
    {
        #region props and ctor
        private readonly IWebHostEnvironment _environment;
        private object response;

        private CloudClinicDb db { get; set; }
        private IDrDetailsProvider drDetailsProvider { get; set; }
        private IDrClinicalDataService clinicalDataService { get; set; }
        private IDrAppointmentService drAppointmentService { get; set; }

        public DrPharmacyStaffController(
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
        #endregion

        [HttpGet(Routes.GetAllDrugsRoute, Name = Routes.GetAllDrugsRoute)]
        public IActionResult GetAllDrugs()
        {
            string drId = drDetailsProvider.GetDrId(User);
            var drugsQuery = (from drugStore in db.Drugs
                              where drugStore.DrId == new Guid(drId)
                              select new DrugViewModel
                              {
                                  DrugId = drugStore.DrugId,
                                  DrugName = drugStore.DrugName,
                                  DrugFormula = drugStore.DrugFormula,
                                  DrugType = drugStore.DrugType,
                                  DrugPurchasePrice = drugStore.DrugPurchasePrice,
                                  DrugSalePrice = drugStore.DrugSalePrice,
                                  DrugAvailQuantity = drugStore.DrugAvailQuantity,
                                  DrugCompany = drugStore.DrugCompany,
                                  DrugExpiryDate = drugStore.DrugExpiryDate.ToShortDateString(),
                                  DrugWeight = drugStore.DrugWeight
                              });
            if (!drugsQuery.Any())
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message =  "Dugs Not found"},
                        null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { allDrugs = drugsQuery.ToList() });

            return Ok(response);
        }

        [HttpGet(Routes.SearchDrugRoute, Name = Routes.SearchDrugRoute)]
        public IActionResult SearchDrug([FromQuery] string drugName, [FromQuery] string drugType)
        {
            string drId = drDetailsProvider.GetDrId(User);

            var drugsQuery = (from drugStore in db.Drugs
                              where drugStore.DrId == new Guid(drId)
                              && drugStore.DrugName.Contains(drugName)
                              && drugStore.DrugType == drugType
                              select new DrugViewModel
                              {
                                  DrugName = drugStore.DrugName,
                                  DrugFormula = drugStore.DrugFormula,
                                  DrugType = drugStore.DrugType,
                                  DrugPurchasePrice = drugStore.DrugPurchasePrice,
                                  DrugSalePrice = drugStore.DrugSalePrice,
                                  DrugAvailQuantity = drugStore.DrugAvailQuantity,
                                  DrugCompany = drugStore.DrugCompany,
                                  DrugExpiryDate = drugStore.DrugExpiryDate.ToShortDateString(),
                                  DrugWeight = drugStore.DrugWeight
                              });
            if (!drugsQuery.Any())
            {
                response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message =  "durg not found"},
                        null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { allDrugs = drugsQuery.ToList() });

            return Ok(response);
        }

        [HttpPost(Routes.AddDrugRoute, Name = Routes.AddDrugRoute)]
        public IActionResult AddDrug([FromBody] DrugCreateModel model)
        {
            string drId = drDetailsProvider.GetDrId(User);

            Drug drg = db.Drugs.FirstOrDefault(drg => drg.DrId == new Guid(drId)
                                               && drg.DrugName == model.DrugName
                                               && drg.DrugType == model.DrugType);
            if (drg != null)
            {
                response = HelperMethods.CreateResponse(
                "400",
                new ErrorModel { Type = "Duplicate Drug", Message = "durg is already exist" },
                null);

                return Ok(response);
            }

            Drug drug = new Drug
            {
                DrugName = model.DrugName,
                DrugFormula = model.DrugFormula,
                DrugExpiryDate = model.DrugExpiryDate,
                DrugPurchasePrice = model.DrugPurchasePrice,
                DrugSalePrice = model.DrugSalePrice,
                DrugCompany = model.DrugCompany,
                DrugType = model.DrugType,
                DrugAvailQuantity = model.DrugAvailQuantity,
                DrugWeight = model.DrugWeight,
                DrId = new Guid(drId)
            };

            db.Drugs.Add(drug);

            db.SaveChanges();
            response = HelperMethods.CreateResponse(
            "200", null, new { drugId = drug.DrugId, drugExpiryDate = drug.DrugExpiryDate.ToShortDateString()}
            );

            return Ok(response);
        }

        [HttpPost(Routes.UpdateDrugRoute, Name = Routes.UpdateDrugRoute)]
        public IActionResult UpdateDrug([FromBody] DrugUpdateModel model)
        {
            string drId = drDetailsProvider.GetDrId(User);

            Drug drug = db.Drugs.FirstOrDefault(drug => drug.DrugId == model.DrugId && drug.DrId == new Guid(drId));

            if (drug is null)
            {
                response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = "durg not found" },
                        null);

                return Ok(response);
            }

            //// update only, when this drug is not given to any patient
            //if (!db.PatDrugs.Any(d => d.DrugId == model.DrugId ))
            //{
            //    drug.DrugName = model.DrugName;
            //    drug.DrugFormula = model.DrugFormula;
            //    drug.DrugType = model.DrugType;
            //}
            
            // these properties can be updated always
            drug.DrugPurchasePrice = model.DrugPurchasePrice;
            drug.DrugSalePrice = model.DrugSalePrice;
            drug.DrugAvailQuantity = model.DrugAvailQuantity;
            drug.DrugCompany = model.DrugCompany;
            drug.DrugExpiryDate = model.DrugExpiryDate;
            drug.DrugWeight = model.DrugWeight;

            db.SaveChanges();

            response = HelperMethods.CreateResponse(
            "200", null, new { updated = true, drugExpiryDate = drug.DrugExpiryDate.ToShortDateString() }
            ) ;

            return Ok(response);
        }

        [HttpGet(Routes.DeleteDrugRoute, Name = Routes.DeleteDrugRoute)]
        public IActionResult DeleteDrug([FromQuery] string drugId)
        {
            string drId = drDetailsProvider.GetDrId(User);

            Drug drug = db.Drugs.FirstOrDefault(drug =>
            drug.DrugId == new Guid(drugId) && drug.DrId == new Guid(drId));

            if (drug is null)
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = "durg not found" },
                        null);

                return Ok(response);
            }

            db.Drugs.Remove(drug);

            db.SaveChanges();
            
            response = HelperMethods.CreateResponse(
            "200", null, new { deleted = true }
            );

            return Ok(response);
        }

        // when patReportId is provided from created reports..
        [HttpGet(Routes.ViewPatDrugsRoute, Name = Routes.ViewPatDrugsRoute)]
        public IActionResult ViewPatDrugs([FromQuery] Guid reportId)
        {
            string drId = drDetailsProvider.GetDrId(User);

            var t = db.Database.BeginTransaction();

            //try
            //{
                var patDrugsQuery = (from pdrugs in db.PatDrugs
                                     where pdrugs.PatReportId == reportId
                                     select new PatDrugViewModel
                                     {
                                         PatDrugId = pdrugs.PatDrugId,
                                         PatReportId = reportId,
                                         PatDrugName = pdrugs.PatDrugName,
                                         DrugGivenQuantity = pdrugs.DrugGivenQuantity,
                                         PatDrugDosage = pdrugs.PatDrugDosage,
                                         PatDrugInstruction = pdrugs.PatDrugInstruction,
                                         PatDrugTime = pdrugs.PatDrugTime
                                     });
                t.Commit();
                
                if(!patDrugsQuery.Any())
                {
                    response = HelperMethods.CreateResponse(
                            "404",
                            new ErrorModel { Type = ErrorTypes.NotFound, Message = "patient durgs not found" },
                            null);

                    return Ok(response);
                }

                response = HelperMethods.CreateResponse("200", null, new { patDrugs = patDrugsQuery.ToList() });

                return Ok(response);
            //}
            //catch (Exception e)
            //{
               // response = HelperMethods.CreateResponse(
               //"400",
               //new ErrorModel { Type = ErrorTypes.ServerError, Message = "failed try again" },
               //null);

               // return Ok(response);
            //}
        }

        // when patReportId is provided from created reports..
        // or sent by dr through signalr
        [HttpGet(Routes.ViewPrescriptionRoute, Name = Routes.ViewPrescriptionRoute)]
        public IActionResult ViewPrescription([FromQuery] Guid reportId)
        {
            string drId = drDetailsProvider.GetDrId(User);

            var t = db.Database.BeginTransaction();

            //try
            //{
            var q = (from r in db.PatReports
                     where r.PatReportId == reportId &&
                     r.IsPending == false && r.IsVisible == true
                     join v in db.Visits on r.PatReportId equals v.PatReportId
                     join p in db.Payments on v.PaymentId equals p.PaymentId
                     join pat in db.AppUsers on r.PatId equals pat.Id
                     select new Prescription
                     {
                         ReportId = reportId,
                         ReportDate = $"{r.ReportDate.Year}-{r.ReportDate.Month}-{r.ReportDate.Day}",
                         PracTimeName = r.DrPracTimeName,
                         PatName = pat.FullName,
                         GuardianName = pat.GuardianName,
                         PhoneNubmer = pat.PhoneNumber,
                         ProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, pat.ProfilePicName),
                         Address = $"{pat.Street}, {pat.City}, {pat.Province}",
                         DrugsPrice = p.DrugsPrice,     
                         PatDrugs = (from pDrugs in db.PatDrugs
                                     where pDrugs.PatReportId == reportId
                                     select new PatDrugModel
                                     {
                                         PatDrugId = pDrugs.PatDrugId,
                                         PatDrugName = pDrugs.PatDrugName,
                                         DrugGivenQuantity = pDrugs.DrugGivenQuantity,
                                         PatDrugDosage = pDrugs.PatDrugDosage,
                                         PatDrugInstruction = pDrugs.PatDrugInstruction,
                                         PatDrugTime = pDrugs.PatDrugTime
                                     }).ToList()
                     });
            t.Commit();

            if (!q.Any())
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = "patient prescription not found" },
                        null);

                return Ok(response);
            }

            var prescription = q.First();

            response = HelperMethods.CreateResponse("200", null, new { prescription });

            return Ok(response);
            //}
            //catch (Exception e)
            //{
            // response = HelperMethods.CreateResponse(
            //"400",
            //new ErrorModel { Type = ErrorTypes.ServerError, Message = "failed try again" },
            //null);

            // return Ok(response);
            //}
        }

        [HttpGet(Routes.ViewAllPrescriptionsRoute, Name = Routes.ViewAllPrescriptionsRoute)]
        public IActionResult ViewAllPrescriptions(
            [FromQuery] int d, [FromQuery] int m, [FromQuery] int y, [FromQuery] string drPracTimeName)
        {
            string drId = drDetailsProvider.GetDrId(User);
            DateTime date = new DateTime(y, m, d);
            var t = db.Database.BeginTransaction();

            //try
            //{
            var q = (from r in db.PatReports
                     where r.DrId == new Guid(drId) &&
                     r.IsPending == false &&
                     r.IsVisible == true &&
                     r.ReportDate.Date == date.Date &&
                     r.DrPracTimeName == drPracTimeName
                     join v in db.Visits on r.PatReportId equals v.PatReportId
                     join p in db.Payments on v.PaymentId equals p.PaymentId
                     join pat in db.AppUsers on r.PatId equals pat.Id
                     select new Prescription
                     {
                         ReportId = r.PatReportId,
                         ReportDate = $"{r.ReportDate.Year}-{r.ReportDate.Month}-{r.ReportDate.Day}",
                         PracTimeName = r.DrPracTimeName,
                         PatName = pat.FullName,
                         GuardianName = pat.GuardianName,
                         PhoneNubmer = pat.PhoneNumber,
                         ProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, pat.ProfilePicName),
                         Address = $"{pat.Street}, {pat.City}, {pat.Province}",
                         DrugsPrice = p.DrugsPrice,
                         PatDrugs = (from pDrugs in db.PatDrugs
                                     where pDrugs.PatReportId == r.PatReportId
                                     select new PatDrugModel
                                     {
                                         PatDrugId = pDrugs.PatDrugId,
                                         PatDrugName = pDrugs.PatDrugName,
                                         DrugGivenQuantity = pDrugs.DrugGivenQuantity,
                                         PatDrugDosage = pDrugs.PatDrugDosage,
                                         PatDrugInstruction = pDrugs.PatDrugInstruction,
                                         PatDrugTime = pDrugs.PatDrugTime
                                     }).ToList()
                     });

            t.Commit();

            if (!q.Any())
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = "no prescriptions found" },
                        null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { allPrescriptions = q.ToList() });

            return Ok(response);
            //}
            //catch (Exception e)
            //{
            // response = HelperMethods.CreateResponse(
            //"400",
            //new ErrorModel { Type = ErrorTypes.ServerError, Message = "failed try again" },
            //null);

            // return Ok(response);
            //}
        }
    }
}