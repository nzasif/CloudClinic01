using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Doctor.Models;
using CloudClinic.Doctor.Models.DrProgressModels;
using CloudClinic.Doctor.Services;
using CloudClinic.Shared;
using CloudClinic.Shared.AuthConstants;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloudClinic.Doctor.Controllers
{
    [Route(Routes.DrProgressControllerRoute)]
    [ApiController]
    [Authorize(Roles = Roles.DrRole)]
    public class DrProgressController : ControllerBase
    {
        private object response;

        private CloudClinicDb db { get; set; }
        private IDrDetailsProvider drDetailsProvider { get; set; }
        private IDrClinicalDataService clinicalDataService { get; set; }

        public DrProgressController(
            CloudClinicDb Db,
            IDrDetailsProvider DrDetailsProvider,
            IDrClinicalDataService ClinicalDataService)
        {
            db = Db;
            drDetailsProvider = DrDetailsProvider;
            clinicalDataService = ClinicalDataService;
        }

        [HttpGet(Routes.ViewMonthlyProgressRoute, Name = Routes.ViewMonthlyProgressRoute)]
        public IActionResult ViewMonthlyProgress([FromQuery] int year, [FromQuery] int month)
        {
            if (year > DateTime.Now.Year || year < 2020)
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "400", Message = $"Year ({year}) is not valid" }, null);
                return Ok(response);
            }

            string drId = drDetailsProvider.GetDrId(User);

            MonthlyProgressViewModel monthlyProgress = new MonthlyProgressViewModel();
            monthlyProgress.Month = month;

            #region appointment list setup
            var apQ = db.Appointments.Where(appoints => appoints.DrId == new Guid(drId)
                                                         && appoints.AppointDate.Year == year
                                                         && appoints.AppointDate.Month == month);

            if (apQ.Any())
            {
                var appointsInMonth = apQ.ToList().GroupBy(ap => ap.AppointDate.Day);
                var appointsList = new List<DailyTotalAppointments>();

                foreach (var apGroup in appointsInMonth)
                {
                    DailyTotalAppointments totalAppointments = new DailyTotalAppointments
                    {
                        DayOfMonth = apGroup.Key,
                        Sum = apGroup.Count()
                    };

                    appointsList.Add(totalAppointments);
                }

                monthlyProgress.TotalAppointments = appointsList;
            }
            #endregion

            #region Visits list setup
            var vQ = db.Visits.Where(v => v.DrId == new Guid(drId)
                                                         && v.VisitDate.Year == year
                                                         && v.VisitDate.Month == month);

            if (vQ.Any())
            {
                var visitsInMonth = vQ.ToList().GroupBy(v => v.VisitDate.Day);

                var visitsList = new List<DailyTotalVisits>();

                foreach (var vGroup in visitsInMonth)
                {
                    var v = new DailyTotalVisits
                    {
                        DayOfMonth = vGroup.Key,
                        Sum = vGroup.Count()
                    };

                    visitsList.Add(v);
                }

                monthlyProgress.TotalVisits = visitsList;
            }
            #endregion

            #region extracting from payment
            var pQ = (from v in db.Visits
                      where v.DrId == new Guid(drId)
                      && v.VisitDate.Month == month && v.VisitDate.Year == year
                      join p in db.Payments on v.PaymentId equals p.PaymentId
                      select new Payment
                      {
                          DrFee = p.DrFee,
                          DrugsPrice = p.DrugsPrice,
                          DrugsExpenditure = p.DrugsExpenditure,
                          LabFee = p.LabFee,
                          XrayFee = p.XrayFee,
                          PaymentDate = p.PaymentDate // date is required when groupBy p.PaymentDate.Day
                      });
            if (pQ.Any())
            {
                var paymentsInMonth = pQ.ToList().GroupBy(p => p.PaymentDate.Day);

                var drugProfitList = new List<DailyTotalDrugsProfit>();
                var drFeeList = new List<DailyTotalDrFee>();
                var labFeeList = new List<DailyTotalLabFee>();
                var xrayFeeList = new List<DailyTotalXrayFee>();

                foreach (var paymentGroup in paymentsInMonth)
                {
                    DailyTotalDrugsProfit totalDrugsProfit = new DailyTotalDrugsProfit
                    {
                        DayOfMonth = paymentGroup.Key,
                        Sum = paymentGroup.Sum(drugs => drugs.DrugsPrice) - paymentGroup.Sum(drugs => drugs.DrugsExpenditure)
                    };

                    DailyTotalDrFee totalDrFee = new DailyTotalDrFee
                    {
                        DayOfMonth = paymentGroup.Key,
                        Sum = paymentGroup.Sum(p => p.DrFee)
                    };

                    DailyTotalLabFee totalLabFee = new DailyTotalLabFee
                    {
                        DayOfMonth = paymentGroup.Key,
                        Sum = paymentGroup.Sum(p => p.LabFee)
                    };

                    DailyTotalXrayFee totalXrayFee = new DailyTotalXrayFee
                    {
                        DayOfMonth = paymentGroup.Key,
                        Sum = paymentGroup.Sum(p => p.LabFee)
                    };

                    drFeeList.Add(totalDrFee);
                    labFeeList.Add(totalLabFee);
                    xrayFeeList.Add(totalXrayFee);
                    drugProfitList.Add(totalDrugsProfit);
                }

                monthlyProgress.TotalDrugsProfit = drugProfitList;
                monthlyProgress.TotalDrFee = drFeeList;
                monthlyProgress.TotalLabFee = labFeeList;
                monthlyProgress.TotalXrayFee = xrayFeeList;
            }
            #endregion


            response = HelperMethods.CreateResponse("200", null, new { monthlyProgress });
            return Ok(response);
        }

        [HttpGet(Routes.ViewYearlyProgressRoute, Name = Routes.ViewYearlyProgressRoute)]
        public IActionResult ViewYearlyProgress([FromQuery] int year)
        {
            if (year > DateTime.Now.Year || year < 2020)
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "400", Message = $"Year ({year}) is not valid" }, null);
                return Ok(response);
            }

            string drId = drDetailsProvider.GetDrId(User);

            YearlyProgressViewModel yearlyProgress = new YearlyProgressViewModel();
            yearlyProgress.Year = year;

            #region appointment list setup
            var apQ = db.Appointments.Where(appoints => appoints.DrId == new Guid(drId)
                                                         && appoints.AppointDate.Year == year);

            if (apQ.Any())
            {
                var appointsInYear = apQ.ToList().GroupBy(ap => ap.AppointDate.Month);
                var appointsList = new List<MonthlyTotalAppointments>();

                foreach (var apGroup in appointsInYear)
                {
                    MonthlyTotalAppointments totalAppointments = new MonthlyTotalAppointments
                    {
                        MonthOfYear = apGroup.Key,
                        Sum = apGroup.Count()
                    };

                    appointsList.Add(totalAppointments);
                }
                  
                yearlyProgress.TotalAppointments = appointsList;
            }
            #endregion

            #region Visits list setup
            var vQ = db.Visits.Where(v => v.DrId == new Guid(drId)
                                                         && v.VisitDate.Year == year);

            if (vQ.Any())
            {
                var visitsInYear = vQ.ToList().GroupBy(v => v.VisitDate.Month);

                var visitsList = new List<MonthlyTotalVisits>();

                foreach (var vGroup in visitsInYear)
                {
                    var v = new MonthlyTotalVisits
                    {
                        MonthOfYear = vGroup.Key,
                        Sum = vGroup.Count()
                    };

                    visitsList.Add(v);
                }

                yearlyProgress.TotalVisits = visitsList;
            }
            #endregion

            #region extracting from payment
            var pQ = (from v in db.Visits
                      where v.DrId == new Guid(drId) && v.VisitDate.Year == year
                      join p in db.Payments on v.PaymentId equals p.PaymentId
                      select new Payment
                      {
                          DrFee = p.DrFee,
                          DrugsPrice = p.DrugsPrice,
                          DrugsExpenditure = p.DrugsExpenditure,
                          LabFee = p.LabFee,
                          XrayFee = p.XrayFee,
                          PaymentDate = p.PaymentDate
                      });

            if (pQ.Any())
            {
                var paymentsInYear = pQ.ToList().GroupBy(p => p.PaymentDate.Month);

                var drugProfitList = new List<MonthlyTotalDrugsProfit>();
                var drFeeList = new List<MonthlyTotalDrFee>();
                var labFeeList = new List<MonthlyTotalLabFee>();
                var xrayFeeList = new List<MonthlyTotalXrayFee>();

                foreach (var paymentGroup in paymentsInYear)
                {
                    MonthlyTotalDrugsProfit totalDrugsProfit = new MonthlyTotalDrugsProfit
                    {
                        MonthOfYear = paymentGroup.Key,
                        Sum = paymentGroup.Sum(drugs => drugs.DrugsPrice) - paymentGroup.Sum(drugs => drugs.DrugsExpenditure)
                    };

                    MonthlyTotalDrFee totalDrFee = new MonthlyTotalDrFee
                    {
                        MonthOfYear = paymentGroup.Key,
                        Sum = paymentGroup.Sum(p => p.DrFee)
                    };

                    MonthlyTotalLabFee totalLabFee = new MonthlyTotalLabFee
                    {
                        MonthOfYear = paymentGroup.Key,
                        Sum = paymentGroup.Sum(p => p.LabFee)
                    };

                    MonthlyTotalXrayFee totalXrayFee = new MonthlyTotalXrayFee
                    {
                        MonthOfYear = paymentGroup.Key,
                        Sum = paymentGroup.Sum(p => p.LabFee)
                    };

                    drFeeList.Add(totalDrFee);
                    labFeeList.Add(totalLabFee);
                    xrayFeeList.Add(totalXrayFee);
                    drugProfitList.Add(totalDrugsProfit);
                }

                yearlyProgress.TotalDrugsProfit = drugProfitList;
                yearlyProgress.TotalDrFee = drFeeList;
                yearlyProgress.TotalLabFee = labFeeList;
                yearlyProgress.TotalXrayFee = xrayFeeList;
            }
            #endregion

            response = HelperMethods.CreateResponse("200", null, new { yearlyProgress });
            return Ok(response);
        }

        [HttpGet(Routes.ViewVisitsRoute, Name = Routes.ViewVisitsRoute)]
        public IActionResult ViewVisits([FromQuery] int year, int month)
        {
            if (year < 2020 || year > 3000 || month > 12 || month < 1)
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "404", Message = "not valid year | month" }, null);
                return Ok(response);
            }

            string drId = drDetailsProvider.GetDrId(User);

            var q = (from v in db.Visits
                     where v.DrId == new Guid(drId)
                     && v.VisitDate.Year == year
                     && v.VisitDate.Month == month
                     join pat in db.AppUsers on v.PatId equals pat.Id
                     select new VisitView
                     {
                         VisitId = v.VisitId,
                         AppointId = v.AppointId,
                         PatReportId = v.PatReportId,
                         PaymentId = v.PaymentId,
                         DrPracTimeName = v.DrPracTimeName,
                         PatId = pat.Id,
                         PatName = pat.FullName,
                         PatProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, pat.ProfilePicName),
                         VisitDate = v.VisitDate
                     });


            if(!q.Any())
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "404", Message = "no visits found" }, null);
                return Ok(response);
            }

            var vList = q.ToList().GroupBy(v => v.VisitDate.Date);

            var visitGroups = new List<VisitViewModel>();

            foreach (var vGroup in vList)
            {
                var visitGroup = new VisitViewModel();
                visitGroup.VisitDate = vGroup.Key.ToString();

                visitGroup.VisitsList = new List<VisitView>();

                foreach(var v in vGroup)
                {
                    visitGroup.VisitsList.Add(v);
                }

                visitGroups.Add(visitGroup);
            }

            response = HelperMethods.CreateResponse("200", null, new { visits = visitGroups });
            return Ok(response);
        }
    }
}