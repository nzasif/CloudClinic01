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
using CloudClinic.Doctor.Models;
using CloudClinic.Shared;
using CloudClinic.Shared.AuthConstants;
using Microsoft.AspNetCore.Authorization;

namespace CloudClinic.Doctor.Controllers.DrStaffControllers
{
    [Route(Routes.DrAssistantControllerRoute)]
    [ApiController]
    [Authorize(Roles = Roles.DrRole + "," + Roles.DrAssistantRole)]
    public class DrAssistantController : ControllerBase
    {
        #region props and ctor
        private readonly IWebHostEnvironment _environment;
        private object response;

        private CloudClinicDb db { get; set; }
        private IDrDetailsProvider drDetailsProvider { get; set; }
        private IDrClinicalDataService clinicalDataService { get; set; }
        private IDrAppointmentService drAppointmentService { get; set; }
        private readonly DateTime appointDefaultDate = new DateTime(2000, 1, 1);

        public DrAssistantController(
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

        [HttpGet(Routes.GetTodayAppointmentsRoute, Name = Routes.GetTodayAppointmentsRoute)]
        public async Task<IActionResult> GetTodayAppointments([FromQuery] string drPracTimeName)
        {
            try
            {
                string drId = drDetailsProvider.GetDrId(User);
                var appointments = await drAppointmentService.GetTodayAppointments(db, HttpContext, drId, drPracTimeName);

                if (appointments is null)
                {
                    response = HelperMethods.CreateResponse("404", new ErrorModel { Type = ErrorTypes.NotFound, Message = "not found" }, null);

                    return Ok(response);
                }

                response = HelperMethods.CreateResponse("200", null, new { todayAppoints = appointments });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                "400", new ErrorModel { Type = ErrorTypes.ServerError, Message = e.Message }, null);

                return Ok(response);
            }
        }

        [HttpGet(Routes.DrSearchAppointmentsRoute, Name = Routes.DrSearchAppointmentsRoute)]
        public async Task<IActionResult> DrSearchAppointments(
            [FromQuery] int d, [FromQuery] int m, [FromQuery] int y, string drPracTimeName)
        {
            try
            {
                DateTime date = new DateTime(y, m, d);

                string drId = drDetailsProvider.GetDrId(User);
                var appointments = await drAppointmentService.DrSearchAppointments(
                    db, HttpContext, drId, date, drPracTimeName);

                if (appointments is null)
                {
                    response = HelperMethods.CreateResponse(
                    "404", new ErrorModel { Type = ErrorTypes.NotFound, Message = $"not found" }, null);

                    return Ok(response);
                }

                response = HelperMethods.CreateResponse(
                    "200", null, new { appointments });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                "400", new ErrorModel { Type = ErrorTypes.ServerError, Message = e.ToString() }, null);

                return Ok(response);
            }
        }

        [HttpGet(Routes.GetAppointmentDetailRoute, Name = Routes.GetAppointmentDetailRoute)]
        public async Task<IActionResult> DrGetAppointmentDetail([FromQuery] string appointId)
        {
            string drId = drDetailsProvider.GetDrId(User);
            var appointDetail = await drAppointmentService.GetDrAppointmentDetail(db, drId, appointId, HttpContext);

            if (appointDetail is null)
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = ErrorTypes.NotFound, Message = "not found" }, null);
                return Ok(response);
            }

            response = HelperMethods.CreateResponse(
            "200", null, new { appointDetail });
            return Ok(response);
        }

        [HttpGet(Routes.RejectAppointmentRoute, Name = Routes.RejectAppointmentRoute)]
        public IActionResult RejectAppointment([FromQuery] Guid appointId)
        {
            string drId = drDetailsProvider.GetDrId(User);

            Appointment appointment = db.Appointments.FirstOrDefault(ap => ap.AppointId == appointId);

            if (appointment.AppointDate.Date < DateTime.Now.Date)
            {
                response = HelperMethods.CreateResponse(
                "400",
                new ErrorModel
                {
                    Type = ErrorTypes.ServerError,
                    Message = "This appointment is already expired no, need to reject."
                },
                null);

                return Ok(response);
            }

            if (appointment.AppointDate.Date == DateTime.Now.Date)
            {
                response = HelperMethods.CreateResponse(
                "400",
                new ErrorModel 
                { 
                    Type = ErrorTypes.ServerError,
                    Message = "You can't reject an appointment at appointment day..." 
                },
                null);

                return Ok(response);
            }

            TimeSpan appointDateCreateDateDif = appointment.AppointDate - appointment.AppointCreateDate;

            if (appointDateCreateDateDif.TotalDays <= 3)
            {
                double hrs = (DateTime.Now - appointment.AppointCreateDate).TotalHours;

                if (hrs < 6)
                {
                    response = HelperMethods.CreateResponse(
                    "400",
                    new ErrorModel
                    { 
                        Type = ErrorTypes.ServerError,
                        Message = "You cant reject an appointment after 6 hrs of its creation when it taken before <=3 days."
                    },
                    null);

                    return Ok(response);
                }

                appointment.AppointStatus = AppointmentStatus.rejected;
                db.SaveChanges();

                response = HelperMethods.CreateResponse(
                         "200", null, new { rejected = true });

                return Ok(response);
            }

            if ((appointment.AppointCreateDate.Date - DateTime.Now.Date).TotalDays <= 3)
            {
                response = HelperMethods.CreateResponse(
                "400",
                new ErrorModel
                {
                    Type = ErrorTypes.ServerError,
                    Message = "You cant reject an appointment after 3 days of its creation."
                },
                null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { rejected = true });
            return Ok(response);
        }

        [HttpGet(Routes.UnRejectAppointmentRoute, Name = Routes.UnRejectAppointmentRoute)]
        public async Task<IActionResult> UnRejectAppointment([FromQuery] Guid appointId)
        {
            string drId = drDetailsProvider.GetDrId(User);

            var appointment = db.Appointments.FirstOrDefault(ap => ap.AppointId == appointId
            && ap.AppointDate.Date >= DateTime.Now.Date);

            if (appointment is null)
            {
                response = HelperMethods.CreateResponse(
                    "404", new ErrorModel {Type="notfound", Message = "appointment not found it may be expired"}, null);

                return Ok(response);
            }

            appointment.AppointStatus = AppointmentStatus.pending;
            db.SaveChanges();

            response = HelperMethods.CreateResponse("200", null, new { rejected = true });
            return Ok(response);
        }

        [HttpGet(Routes.GetAppointmentsCountRoute, Name = Routes.GetAppointmentsCountRoute)]
        public async Task<IActionResult> GetAppointmentsCount()
        {
            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                var drTotalAppoints = await drAppointmentService.GetTotalAppointments(db, drId);

                if (drTotalAppoints is null)
                {
                    response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = "404", Message = "Not found" },
                        null);

                    return Ok(response);
                }

                var groups = drTotalAppoints.GroupBy(ap => ap.DrPracTimeName);

                var newList = new List<DrTotalAppointsViewModel>();

                foreach (var g in groups)
                {
                    newList.Add(new DrTotalAppointsViewModel
                    {
                        DrPracTimeName = g.Key,
                        TotalAppoints = g.Count()
                    });
                }

                response = HelperMethods.CreateResponse("200", null, new { todayTotalAppoints = newList });
                return Ok(response);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //+++++++++++++-------------+++++++ Managing clinic details ++++++++++++++------------------+++++++++++++
        [HttpPost(Routes.AddDrPracTimeRoute, Name = Routes.AddDrPracTimeRoute)]
        public async Task<IActionResult> AddDrPracTime([FromBody] DrPracTimeCreateModel model)
        {
            // unique index; DrId + PracTimeName
            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                if (db.DrPracTimes.Any(
                                    pt => pt.DrPracTimeName == model.DrPracTimeName
                                    && pt.DrId == new Guid(drId)))
                {
                    response = HelperMethods.CreateResponse(
                            "404",
                            new ErrorModel { Type = ErrorTypes.NotFound, Message = "Duplicate entry is not allowed" },
                            null);

                    return Ok(response);
                }

                DrPracTime drPracTime = new DrPracTime
                {
                    DrPracTimeName = model.DrPracTimeName,
                    DrPracStartTime = model.DrPracStartTime,
                    DrPracEndTime = model.DrPracEndTime,
                    DrMaxAppointments = model.DrMaxAppointments,
                    DrId = new Guid(drId)
                };

                await db.AddAsync(drPracTime);
                await db.SaveChangesAsync();

                response = HelperMethods.CreateResponse(
                        "200", null,
                        new { drPracTimeId = drPracTime.DrPracTimeId, added = true });

                return Ok(response);

            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                            "400",
                            new ErrorModel { Type = ErrorTypes.NotFound, Message = "Some thing goes wrond try again" },
                            null);

                return BadRequest(response);
            }
        }

        [HttpGet(Routes.GetDrPracTimesRoute, Name = Routes.GetDrPracTimesRoute)]
        public async Task<IActionResult> GetDrPracTimes()
        {
            string drId = drDetailsProvider.GetDrId(User);

            var ts = drDetailsProvider.GetDrPracTimes(db, drId);

            if (ts is null)
            {
                response = HelperMethods.CreateResponse(
                    "404",
                    new ErrorModel { Type = ErrorTypes.NotFound, Message = "Not found" },
                    null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { drPracTimes = ts });

            return Ok(response);
        }

        [HttpGet(Routes.DeleteDrPracTimeRoute, Name = Routes.DeleteDrPracTimeRoute)]
        public async Task<IActionResult> DeleteDrPracTime([FromQuery] string drPracTimeName)
        {
            object response;

            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                DrPracTime drPracTime = db.DrPracTimes.FirstOrDefault(t => t.DrId == new Guid(drId)
                    && t.DrPracTimeName == drPracTimeName);

                if (drPracTime is null)
                {
                    response = HelperMethods.CreateResponse(
                            "404",
                            new ErrorModel { Type = ErrorTypes.NotFound, Message = "drPracTime was not found" },
                            null);

                    return Ok(response);
                }

                db.DrPracTimes.Remove(drPracTime);
                await db.SaveChangesAsync();

                response = HelperMethods.CreateResponse(
                        "200", null, new { deleted = true });

                return Ok(response);

            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                            "400",
                            new ErrorModel { Type = ErrorTypes.ServerError, Message = "can not delete, try again" },
                            null);

                return BadRequest(response);
            }
        }

        [HttpPost(Routes.AddDrHolidayRoute, Name = Routes.AddDrHolidayRoute)]
        public async Task<IActionResult> AddDrHoliday([FromBody] DrHolidayCreateModel model)
        {
            object response;

            // hollidayName + drId is unique index
            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                if (db.DrHolidays.Any(
                    h => h.DrHolidayName == model.DrHolidayName
                    && h.DrId == new Guid(drId)))
                {
                    response = HelperMethods.CreateResponse(
                            "400",
                            new ErrorModel { Type = ErrorTypes.NotFound, Message = "Duplicate entry is not allowed" + model.DrHolidayName },
                            null);

                    return Ok(response);
                }

                DrHoliday drHoliday = new DrHoliday
                {
                    DrHolidayName = model.DrHolidayName,
                    EntryDate = DateTime.Now,
                    DrId = new Guid(drId)
                };

                await db.DrHolidays.AddAsync(drHoliday);

                await db.SaveChangesAsync();

                response = HelperMethods.CreateResponse(
                        "200", null, new { drHolidayId = drHoliday.DrHolidayId, added = true });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel { Type = ErrorTypes.ServerError, Message = "Failed to add holiday, make sure holiday name is duplicated" },
                        null);

                return BadRequest(response);
            }
        }

        [HttpGet(Routes.DeleteDrHolidayRoute, Name = Routes.DeleteDrHolidayRoute)]
        public async Task<IActionResult> RemoveDrHoliday([FromQuery] Guid drHolidayId)
        {
            object response;

            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                DrHoliday drHoliday = drDetailsProvider.GetDrHoliday(db, drId, drHolidayId);

                if (drHoliday is null)
                {
                    response = HelperMethods.CreateResponse(
                            "404",
                            new ErrorModel { Type = ErrorTypes.NotFound, Message = "holiday was not found" },
                            null);

                    return NotFound(response);

                }

                db.DrHolidays.Remove(drHoliday);

                await db.SaveChangesAsync();


                response = HelperMethods.CreateResponse(
                        "200", null, new { deleted = true });

                return Ok(response);

            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel { Type = ErrorTypes.ServerError, Message = "Failed to remove holiday, try again" },
                        null);

                return BadRequest(response);
            }
        }

        [HttpGet(Routes.GetDrHolidaysRoute, Name = Routes.GetDrHolidaysRoute)]
        public async Task<IActionResult> GetDrHolidays()
        {
            object response;

            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                List<DrHolidayViewModel> drHolidays = drDetailsProvider.GetDrHolidays(db, drId);

                if (drHolidays != null)
                {
                    response = HelperMethods.CreateResponse(
                            "200", null, new { drHolidays = drHolidays });
                    return Ok(response);
                }

                response = HelperMethods.CreateResponse(
                        "404", new ErrorModel { Type = "notfound", Message = "no dr holidays found" }, null);
                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel { Type = ErrorTypes.ServerError, Message = "Failed to get holidays list, try again" },
                        null);

                return BadRequest(response);
            }
        }
    }
}