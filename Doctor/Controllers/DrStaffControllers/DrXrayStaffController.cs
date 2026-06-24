using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Doctor.Models.DrProgressModels;
using CloudClinic.Doctor.Models.CheckPatModels.PatXrayModels;
using CloudClinic.Doctor.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using CloudClinic.Shared;
using CloudClinic.Shared.AuthConstants;

namespace CloudClinic.Doctor.Controllers.DrStaffControllers
{
    [Route(Routes.DrXrayStaffControllerRoute)]
    [ApiController]
    [Authorize(Roles = Roles.DrRole + "," + Roles.DrXrayStaffRole)]
    public class DrXrayStaffController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private object response;

        private CloudClinicDb db { get; set; }
        private IDrDetailsProvider drDetailsProvider { get; set; }
        private IDrClinicalDataService clinicalDataService { get; set; }
        private IDrAppointmentService drAppointmentService { get; set; }

        public DrXrayStaffController(
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

        // this is used for both create and update
        // because the xrays are already enterd by the dr (referring xrays), 
        // staff can only add result to them..
        // ------------------------------------------------------
        // these submitted results will be saved at client side
        // so he can view it and change it (on the same day only...)
        [HttpPost(Routes.SubmitXrayResultRoute, Name = Routes.SubmitXrayResultRoute)]
        public IActionResult SubmitXrayResult([FromForm] XraySubmitModel xrayModel)
        {
            string drId = drDetailsProvider.GetDrId(User);

            try
            {
                PatReport patReport = db.PatReports.FirstOrDefault(r => r.PatReportId == xrayModel.PatReportId
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

                PatXray xray = db.PatXrays.FirstOrDefault(x => x.XrayId == xrayModel.XrayId && x.PatReportId == xrayModel.PatReportId);

                if (xray is null)
                {
                    response = HelperMethods.CreateResponse(
                    "404",
                    new ErrorModel { Type = ErrorTypes.NotFound, Message = "This Xray was not found in referred xrays." },
                    null);

                    return Ok(response);
                }

                // date is already checked for report
                //if(xray.XrayReferDateTime.Date < DateTime.Now.Date)
                //{
                //    response = HelperMethods.CreateResponse(
                //        "404",
                //        new ErrorModel { Type = ErrorTypes.NotFound, Message = "Report update session is expired." },
                //        null);

                //    return Ok(response);
                //}

                if(xray.AttachmentName != null)
                {
                    HelperMethods.DeleteAttachment(xray.AttachmentName, _environment);
                }

                if (xrayModel.AttachmentFile != null)
                {
                    string attachmentName = HelperMethods.AddAttachment(xrayModel.AttachmentFile, _environment);

                    if (attachmentName.StartsWith("error"))
                    {
                        response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = attachmentName },
                        null);

                        return Ok(response);
                    }

                    xray.AttachmentName = attachmentName;
                }

                // before update first note old price
                int oldXrayFee = xray.XrayFee;

                xray.XrayResult = xrayModel.XrayResult;
                xray.XrayFee = xrayModel.XrayFee;
                xray.XraySubmitDateTime = DateTime.Now;

                var p = (from v in db.Visits
                         where v.PatReportId == xray.PatReportId
                         join pay in db.Payments on v.PaymentId equals pay.PaymentId
                         select pay).FirstOrDefault();

                if (p != null)
                {
                    p.XrayFee = (p.XrayFee == 0) ? xrayModel.XrayFee : (p.XrayFee - oldXrayFee) + xrayModel.XrayFee;
                }

                db.SaveChanges();

                response = HelperMethods.CreateResponse(
                        "200", null, new { 
                            submitDate = xray.XraySubmitDateTime.ToString(),
                            attachmentUri = HelperMethods.GenerateAttachmentUri(HttpContext, xray.AttachmentName)
                        });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                           "400",
                           new ErrorModel { Type = ErrorTypes.NotFound, Message = "Failed try again." },
                           null);
                return Ok(response);
            }
        }

        // return only pending referred xrays for the xrayStaff....
        // for the specified reportId, reportId will be retrieved from
        // ViewPendingReport method in the DrCheckController...
        [Authorize(Roles = Roles.DrRole + "," + Roles.DrXrayStaffRole)]
        [HttpGet(Routes.GetReferredXraysRoute, Name = Routes.GetReferredXraysRoute)]
        public IActionResult GetReferredXrays([FromQuery] int d, [FromQuery] int m, [FromQuery] int y, [FromQuery] string drPracTimeName)
        {
            string drId = drDetailsProvider.GetDrId(User);
            DateTime date = new DateTime(y, m, d);

            var q = (from rs in db.PatReports
                     where rs.DrId == new Guid(drId)
                     && rs.ReportDate.Date == date.Date
                     && rs.DrPracTimeName == drPracTimeName
                     join x in db.PatXrays on rs.PatReportId equals x.PatReportId
                     join patu in db.AppUsers on rs.PatId equals patu.Id
                     select new ReferredXrayViewModel
                     {
                         PatName = patu.FullName,
                         PatProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, patu.ProfilePicName),
                         GuardianName = patu.GuardianName,
                         PatPhoneNumber = patu.PhoneNumber,
                         XrayId = x.XrayId,
                         XrayName = x.XrayName,
                         XrayType = x.XrayType,
                         XrayResult = x.XrayResult,
                         XrayFee = x.XrayFee,
                         XrayReferredDateTime = x.XrayReferDateTime.ToShortDateString()+" "+x.XrayReferDateTime.ToShortTimeString(),
                         XraySubmitDateTime = x.XraySubmitDateTime.ToShortDateString()+" "+x.XraySubmitDateTime.ToShortTimeString(),
                         AttachmentUri = HelperMethods.GenerateAttachmentUri(HttpContext, x.AttachmentName),
                         PatReportId = x.PatReportId
                     });

            if (!q.Any())
            {
                response = HelperMethods.CreateResponse(
                   "404",
                   new ErrorModel { Type = ErrorTypes.NotFound, Message = "No reffered xray found" },
                   null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { patXrays = q.ToList() });
            return Ok(response);
        }

        // used by xray-staff when dr submit result and 
        // notify xrayStaff through signalR
        // then staff will use this to get updated xray
        [Authorize(Roles = Roles.DrRole + "," + Roles.DrXrayStaffRole)]
        [HttpGet(Routes.GetReferredXrayRoute, Name = Routes.GetReferredXrayRoute)]
        public IActionResult GetReferredXray([FromQuery] Guid xrayId)
        {
            string drId = drDetailsProvider.GetDrId(User);

            var q = (from rs in db.PatReports
                     where rs.DrId == new Guid(drId)
                     && rs.IsVisible == true
                     join x in db.PatXrays on rs.PatReportId equals x.PatReportId
                     where x.XrayId == xrayId
                     join patu in db.AppUsers on rs.PatId equals patu.Id
                     select new ReferredXrayViewModel
                     {
                         PatName = patu.FullName,
                         PatProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, patu.ProfilePicName),
                         GuardianName = patu.GuardianName,
                         PatPhoneNumber = patu.PhoneNumber,
                         XrayId = x.XrayId,
                         XrayName = x.XrayName,
                         XrayFee = x.XrayFee,
                         XrayType = x.XrayType,
                         XrayResult = x.XrayResult,
                         XrayReferredDateTime = x.XrayReferDateTime.ToShortDateString() + " " + x.XrayReferDateTime.ToShortTimeString(),
                         XraySubmitDateTime = x.XraySubmitDateTime.ToShortDateString() + " " + x.XraySubmitDateTime.ToShortTimeString(),
                         AttachmentUri = HelperMethods.GenerateAttachmentUri(HttpContext, x.AttachmentName),
                         PatReportId = x.PatReportId
                     });

            if (!q.Any())
            {
                response = HelperMethods.CreateResponse(
                   "404",
                   new ErrorModel { Type = ErrorTypes.NotFound, Message = "No reffered xray found" },
                   null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { patXray = q.First() });
            return Ok(response);
        }

        // used by dr when he wants to update localStorage
        [HttpGet(Routes.ViewPatXraysRoute, Name = Routes.ViewPatXraysRoute)]
        [Authorize(Roles = Roles.DrRole)]
        public IActionResult ViewPatXrays([FromQuery] Guid reportId)
        {
            var t = db.Database.BeginTransaction();

            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                var q = (from xrays in db.PatXrays
                         where xrays.PatReportId == reportId
                         select new XrayViewModel
                         {
                             XrayId = xrays.XrayId,
                             XrayReferredDateTime = xrays.XrayReferDateTime.ToShortDateString()+" "+xrays.XrayReferDateTime.ToShortTimeString(),
                             XraySubmitDateTime = xrays.XraySubmitDateTime.ToShortDateString()+" "+xrays.XraySubmitDateTime.ToShortTimeString(),
                             XrayName = xrays.XrayName,
                             XrayType = xrays.XrayType,
                             XrayResult = xrays.XrayResult,
                             XrayFee = xrays.XrayFee,
                             PatReportId = xrays.PatReportId,
                             AttachmentUri = HelperMethods.GenerateAttachmentUri(HttpContext, xrays.AttachmentName)
                         });
                t.Commit();

                if (!q.Any())
                {
                    response = HelperMethods.CreateResponse(
                    "404", new ErrorModel { Type = "", Message = "xrays not found" }, null);

                    return Ok(response);
                }

                response = HelperMethods.CreateResponse("200", null, new { patXrays = q.ToList() });
                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                    "400", new ErrorModel { Type = "", Message = "Some error" }, null);
                return Ok(response);
            }
        }

        // used by dr when he wants to update localStorage
        [HttpGet(Routes.ViewPatXrayRoute, Name = Routes.ViewPatXrayRoute)]
        [Authorize(Roles = Roles.DrRole)]
        public IActionResult ViewPatXray([FromQuery] Guid xrayId)
        {
            var t = db.Database.BeginTransaction();

            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                var q = (from xrays in db.PatXrays
                         where xrays.XrayId == xrayId
                         select new XrayViewModel
                         {
                             XrayId = xrays.XrayId,
                             XrayReferredDateTime = xrays.XrayReferDateTime.ToShortDateString()+" "+xrays.XrayReferDateTime.ToShortTimeString(),
                             XraySubmitDateTime = xrays.XraySubmitDateTime.ToShortDateString()+" "+xrays.XraySubmitDateTime.ToShortTimeString(),
                             XrayName = xrays.XrayName,
                             XrayType = xrays.XrayType,
                             XrayResult = xrays.XrayResult,
                             XrayFee = xrays.XrayFee,
                             PatReportId = xrays.PatReportId,
                             AttachmentUri = HelperMethods.GenerateAttachmentUri(HttpContext, xrays.AttachmentName)
                         });
                t.Commit();

                if (!q.Any())
                {
                    response = HelperMethods.CreateResponse(
                    "404", new ErrorModel { Type = "", Message = "xrays not found" }, null);

                    return Ok(response);
                }

                response = HelperMethods.CreateResponse("200", null, new { patXray = q.First() });
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