using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using CloudClinic.Doctor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using CloudClinic.Shared;
using CloudClinic.Shared.AuthConstants;
using CloudClinic.Doctor.Models.AccountantModels;

namespace CloudClinic.Doctor.Controllers.DrStaffControllers
{
    [Route(Routes.DrAccountantControllerRoute)]
    [ApiController]
    [Authorize(Roles = Roles.DrRole + "," + Roles.DrAccountantRole)]
    public class DrAccountantController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private object response;

        private CloudClinicDb db { get; set; }
        private IDrDetailsProvider drDetailsProvider { get; set; }

        public DrAccountantController(
            CloudClinicDb Db,
            IDrDetailsProvider DrDetailsProvider,
            IWebHostEnvironment environment)
        {
            db = Db;
            drDetailsProvider = DrDetailsProvider;
            _environment = environment;
        }

        // when init report, its visit and payment Ids are sent back to client
        // on the client it will be saved (in a report object), to send to this method for calculating 
        // and viewing payment, and also create payment/add payment to the database
        // for confirmation a confirm method, will get payment id and mark it as "Paid = true"
        [HttpGet(Routes.ViewAllPaymentsRoute, Name = Routes.ViewAllPaymentsRoute)]
        public async Task<IActionResult> ViewAllPayments([FromQuery] int d, [FromQuery] int m, [FromQuery] int y, [FromQuery] string drPracTimeName)
        {
            var t = await db.Database.BeginTransactionAsync();
            DateTime date = new DateTime(y, m, d);
            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                var q = (from v in db.Visits
                         where v.DrId == new Guid(drId) &&
                         v.VisitDate.Date == date.Date &&
                         v.DrPracTimeName == drPracTimeName
                         join rp in db.PatReports on v.PatReportId equals rp.PatReportId
                         where rp.IsPending == false
                         join patU in db.AppUsers on v.PatId equals patU.Id
                         join p in db.Payments on v.PaymentId equals p.PaymentId
                         select new PaymentViewModel
                         {
                             PatName = patU.FullName,
                             GuardianName = patU.GuardianName,
                             PhoneNubmer = patU.PhoneNumber,
                             Address = $"{patU.Street}, {patU.City}, {patU.Province}",
                             ProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, patU.ProfilePicName),

                             PaymentId = p.PaymentId,
                             PaymentDate = p.PaymentDate.ToString(),
                             DrPracTimeName = v.DrPracTimeName,
                             DrugsExpenditure = p.DrugsExpenditure,
                             DrFee = p.DrFee,
                             DrugsPrice = p.DrugsPrice,
                             LabFee = p.LabFee,
                             LabExpenditure = 0,
                             XrayFee = p.XrayFee,
                             TotalAmount = p.DrFee + p.DrugsPrice + p.LabFee + p.XrayFee,
                             Paid = p.Paid
                         }).ToList();

                #region commented
                //int drFee = drDetailsProvider.GetDrFee(db, drId);

                //int totalLabFee = db.PatLabTests.Where(d => d.PatReportId == reportId).Sum(d => d.LabTestFee);

                //int totalXrayFee = db.PatXrays.Where(x => x.PatReportId == reportId).Sum(x => x.XrayFee);

                //int totalDrugPrice = 0;
                //int totalDrugExpenditure = 0;

                //totalDrugPrice = (from pd in db.PatDrugs
                //                  where pd.PatReportId == reportId
                //                  join d in db.Drugs on pd.DrugId equals d.DrugId
                //                  select d.DrugSalePrice).Sum();

                //totalDrugExpenditure = (from pd in db.PatDrugs
                //                  where pd.PatReportId == reportId
                //                  join d in db.Drugs on pd.DrugId equals d.DrugId
                //                  select d.DrugPurchasePrice).Sum();

                //Payment payment = db.Payments.FirstOrDefault(p => p.PaymentId == new Guid(paymentId));

                //payment.LabFee = totalLabFee;
                //payment.DrugsPrice = totalDrugPrice;
                //payment.DrugsExpenditure = totalDrugExpenditure;
                //payment.DrFee = drFee;
                //payment.XrayFee = totalXrayFee;
                //payment.PaymentDate = DateTime.Now;

                //payment.Paid = false;
                //await db.SaveChangesAsync();
                // for viewing to client
                //PaymentViewModel paymentView = new PaymentViewModel
                //{
                //    DrFee = drFee,
                //    DrugsExpenditure = totalDrugExpenditure,
                //    DrugsPrice = totalDrugPrice,
                //    LabExpenditure = 0, // currently we dont have a system for that...
                //    LabFee = totalLabFee,
                //    XrayFee = totalXrayFee,
                //    TotalAmount = totalDrugPrice + totalLabFee + totalXrayFee + drFee,
                //    PaymentDate = DateTime.Now.ToString(),
                //    PaymentId = paymentId
                //};
                #endregion commented

                await t.CommitAsync();

                if (!q.Any())
                {
                    response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = "404", Message = "Not found payments" },
                        null);

                    return Ok(response);

                }

                response = HelperMethods.CreateResponse(
                    "200", null, new { payments = q.ToList() });
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

        [HttpGet(Routes.GetPaymentRoute, Name = Routes.GetPaymentRoute)]
        public async Task<IActionResult> GetPayment([FromQuery] Guid paymentId)
        {
            var t = await db.Database.BeginTransactionAsync();

            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                var q = (from v in db.Visits
                         where v.DrId == new Guid(drId) 
                         && v.PaymentId == paymentId
                         join patU in db.AppUsers on v.PatId equals patU.Id
                         join p in db.Payments on v.PaymentId equals p.PaymentId
                         select new PaymentViewModel
                         {
                             PatName = patU.FullName,
                             GuardianName = patU.GuardianName,
                             PhoneNubmer = patU.PhoneNumber,
                             Address = $"{patU.Street}, {patU.City}, {patU.Province}",
                             ProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, patU.ProfilePicName),

                             PaymentId = p.PaymentId,
                             PaymentDate = p.PaymentDate.ToString(),
                             DrPracTimeName = v.DrPracTimeName,
                             DrugsExpenditure = p.DrugsExpenditure,
                             DrFee = p.DrFee,
                             DrugsPrice = p.DrugsPrice,
                             LabFee = p.LabFee,
                             LabExpenditure = 0,
                             XrayFee = p.XrayFee,
                             TotalAmount = p.DrFee + p.DrugsPrice + p.LabFee + p.XrayFee,
                             Paid = p.Paid
                         });

                await t.CommitAsync();

                if (!q.Any())
                {
                    response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = "404", Message = "Not found payments" },
                        null);

                    return Ok(response);
                }

                response = HelperMethods.CreateResponse(
                    "200", null, new { payment = q.First() });
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

        [HttpGet(Routes.ToggleConfirmPaymentRoute, Name = Routes.ToggleConfirmPaymentRoute)]
        public async Task<IActionResult> ToggleConfirmPayment([FromQuery] Guid paymentId)
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

                p.Paid = !(p.Paid);

                await db.SaveChangesAsync();

                response = HelperMethods.CreateResponse(
                    "200", null, new { paid = p.Paid });
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

        [HttpGet(Routes.DrViewPaymentRoute, Name = Routes.DrViewPaymentRoute)]
        public async Task<IActionResult> DrViewPayment([FromQuery] Guid paymentId)
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

                DrtPaymentViewModel payment = new DrtPaymentViewModel
                {
                    DrFee = p.DrFee,
                    DrugsPrice = p.DrugsPrice,
                    DrugsExpenditure = p.DrugsExpenditure,
                    LabFee = p.LabFee,
                    LabExpenditure = 0,
                    XrayFee = p.XrayFee,
                    TotalAmount = (p.XrayFee + p.DrFee + p.DrugsPrice + p.LabFee),
                    PaymentDate = p.PaymentDate.ToShortTimeString(),
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
        // 
        ///  lab, xray fee is adding during submitting result
        ///  // and subtracted when removed
        ///  drugFee added and removed when adding and removing pat drug
        ///  drFee is adding during report init
    }
}