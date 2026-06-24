using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using CloudClinic.Doctor.Models.CheckPatModels;
using CloudClinic.Patients.Models.PatReportViewModels;
using CloudClinic.Patients.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CloudClinic.Shared.Services;
using CloudClinic.Shared;
using Microsoft.AspNetCore.Authorization;
using CloudClinic.Shared.AuthConstants;
using CloudClinic.Patients.Models;

namespace CloudClinic.Patients.Controllers
{
    [Authorize(Roles = Roles.NormalUserRole)]
    [Route(Routes.PatReportControllerRoute)]
    [ApiController]
    public class PatReportController : ControllerBase
    {
        private object response;
        private CloudClinicDb db { get; set; }
        private IPatReportManageService patReportManageService { get; set; }
        private IUserProfileProvider userInfoProvider { get; set; }
        public PatReportController(
            CloudClinicDb Db,
            IPatReportManageService reportManageService,
            IUserProfileProvider UserInfoProvider)
        {
            db = Db;
            patReportManageService = reportManageService;
            userInfoProvider = UserInfoProvider;
        }

        [HttpGet(Routes.PatViewReportRoute, Name = Routes.PatViewReportRoute)]
        public IActionResult PatViewReport([FromQuery] Guid reportId)
        {
            string userId = userInfoProvider.GetUserId(User);

            var patReport = patReportManageService.PatViewReport(db, userId, reportId, HttpContext);

            if (patReport is null)
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "not found", Message = "empty result" }, null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { patReport });

            return Ok(response);
        }

        [HttpGet(Routes.PatToggleReportVisibilityRoute, Name = Routes.PatToggleReportVisibilityRoute)]
        public IActionResult PatToggleReportVisibility([FromQuery] Guid reportId)
        {
            string userId = userInfoProvider.GetUserId(User);
            var r = patReportManageService.TogglePatReportVisibility(db, userId, reportId);
            if (r != null)
            {
                response = HelperMethods.CreateResponse("200", null, new { msg = r });
                return Ok(response);
            }

            response = HelperMethods.CreateResponse(
                "400", new ErrorModel { Type = "unknown", Message = $"cannot toggle" }, null);
            return Ok(response);
        }

        [HttpGet(Routes.GetAllVisitedDrsRoute, Name = Routes.GetAllVisitedDrsRoute)]
        public async Task<IActionResult> GetAllVisitedDrs()
        {
            string patId = userInfoProvider.GetUserId(User);
            var q = (from rpts in db.PatReports
                     where rpts.PatId == patId
                     join v in db.Visits on patId equals v.PatId
                     join p in db.Payments on v.PaymentId equals p.PaymentId
                     join drs in db.DrDetails on rpts.DrId equals drs.DrId
                     join dru in db.AppUsers on drs.UserId equals dru.Id
                     select new VisitedDrViewModel
                     {
                         DrId = drs.DrId,
                         DrName = dru.FullName,
                         DrProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, dru.ProfilePicName),
                         DrSpecialty = drs.DrSpecialty,
                         DrAddress = dru.Street + ", " + dru.City + ", " + dru.Province,
                         VisitDate = rpts.ReportDate.ToShortDateString(),
                         ReportId = rpts.PatReportId,
                         PaymentId = p.PaymentId
                     });

            if (!q.Any())
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "404", Message = "notfound" }, null);
                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { allVisitedDrs = q.ToList() });
            return Ok(response);
        }

        [HttpGet(Routes.PatViewPaymentRoute, Name = Routes.PatViewPaymentRoute)]
        public async Task<IActionResult> PatViewPayment([FromQuery] Guid paymentId)
        {
            try
            {
                var p = db.Payments.FirstOrDefault(p => p.PaymentId == paymentId);

                if (p is null)
                {
                    response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = "404", Message = "Not found payments" },
                        null);

                    return Ok(response);
                }

                PatPaymentViewModel payment = new PatPaymentViewModel
                {
                    DrFee = p.DrFee,
                    DrugsPrice = p.DrugsPrice,
                    LabFee = p.LabFee,
                    XrayFee = p.XrayFee,
                    TotalAmount = (p.XrayFee + p.DrFee + p.DrugsPrice + p.LabFee),
                    PaymentDate = p.PaymentDate.ToLongDateString(),
                    PaymentId = p.PaymentId,
                    Paid = p.Paid
                };

                response = HelperMethods.CreateResponse(
                    "200", null, new { payment });
                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                "400",
                new ErrorModel { Type = "paymentError", Message = "payment could not be claculated" },
                null);

                return Ok(response);
            }
        }
    }
}