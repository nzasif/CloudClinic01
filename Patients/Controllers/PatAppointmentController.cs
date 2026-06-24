using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Patients.Models;
using CloudClinic.Patients.Services;
using CloudClinic.Shared;
using CloudClinic.Shared.AuthConstants;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudClinic.Patients.Controllers
{
    [ApiController]
    [Authorize(Roles = Roles.NormalUserRole)]
    [Route(Routes.PatAppointmentControllerRoute)]
    public class PatAppointmentController : ControllerBase
    {
        private object response;
        private CloudClinicDb db { get; set; }
        private IPatAppointmentService appointmentService { get; set; }
        private IUserProfileProvider userInfoProvider { get; set; }
        public PatAppointmentController(
            CloudClinicDb Db,
            IPatAppointmentService AppointmentService,
            IUserProfileProvider UserInfoProvider)
        {
            db = Db;
            appointmentService = AppointmentService;
            userInfoProvider = UserInfoProvider;
        }

        [HttpPost(Routes.CreateAppointmentRoute, Name = Routes.CreateAppointmentRoute)]
        public async Task<IActionResult> CreateAppointment(AppointmentCreateModel model)
        {
            var t = db.Database.BeginTransaction();

            try
            {
                string patId = userInfoProvider.GetUserId(User);

                // check duplicate appointment
                Appointment appointment = db.Appointments.FirstOrDefault(ap => ap.PatId == patId && ap.DrId == model.DrId
                                        && ap.AppointDate.Date == model.AppointDate
                                        && ap.DrPracTimeName == model.DrPracTimeName);

                if (appointment != null)
                {
                    response = HelperMethods.CreateResponse(
                      "400",
                      new ErrorModel { Type = ErrorTypes.ServerError, Message = "duplicate found" },
                      null);

                    return Ok(response);
                }

                // Now check for appointment availability
                string error = await appointmentService.IsAppointAvailableAsync(db, model.DrId, model.AppointDate.Date, model.DrPracTimeName);

                if (error != null)
                {
                    response = HelperMethods.CreateResponse(
                      "404",
                      new ErrorModel { Type = ErrorTypes.ServerError, Message = error },
                      null);

                    return Ok(response);
                }

                // Everything is checked, now create appointment
                string appointId = await appointmentService.CreateAppointmentAsync(db, model, patId);

                t.Commit();

                if (appointId is null)
                {
                   response = HelperMethods.CreateResponse(
                   "400",
                   new ErrorModel { Type = ErrorTypes.ServerError, Message = "failed, try again.." },
                   null);

                   return Ok(response);
                }

                response = HelperMethods.CreateResponse("200", null, new { appointId = new Guid(appointId) });
                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                      "400",
                      new ErrorModel { Type = ErrorTypes.ServerError, Message = "failed try again" + e.Message },
                      null);

                return Ok(response);
            }
        }

        [HttpGet(Routes.PatViewAllAppointmentsRoute, Name = Routes.PatViewAllAppointmentsRoute)]
        public async Task<IActionResult> PatViewAllAppointmentsAsync()
        {
            try
            {
                string patId = userInfoProvider.GetUserId(User);

                var appointments = await appointmentService.ViewAllAppointmentsAsync(db, patId, HttpContext);

                if (appointments is null)
                {
                    response = HelperMethods.CreateResponse(
                        "404", new ErrorModel { Type = ErrorTypes.NotFound, Message = "no appoints found" }, null);
                    return Ok(response);
                }


                response = HelperMethods.CreateResponse("200", null, new { patAppoints = appointments });
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // this method will auto, or by button will called and look for current appointment status
        // and bring only neccessay info...
        [HttpGet(Routes.TrackAppointmentRoute, Name = Routes.TrackAppointmentRoute)]
        public async Task<IActionResult> TrackAppointment([FromQuery] Guid appointId)
        {
            var ap = db.Appointments.FirstOrDefault(app => app.AppointId == appointId);

            if (ap is null)
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "404", Message = "Coud not find" }, null);
                return Ok(response);
            }

            var q = (from aps in db.Appointments
                     where aps.RemovedFromView == false && // if user remove appointment it will not show to Drs, so donot need to care
                     aps.AppointDate.Date == ap.AppointDate.Date
                     && aps.DrPracTimeName == ap.DrPracTimeName &&
                     aps.AppointStatus != AppointmentStatus.rejected
                     join patU in db.AppUsers on aps.PatId equals patU.Id
                     orderby aps.AppointCreateDate ascending
                     select new TrackAppointmentModel
                     {
                         AppointId = aps.AppointId,
                         PatName = patU.FullName,
                         Status = aps.AppointStatus.ToString()
                     });

            // as self appoint will always there.
            //if (!q.Any())
            //{
            //    response = HelperMethods.CreateResponse("200", new ErrorModel { Type = "200", Message = "No other appointments found, its only you." }, null);
            //    return Ok(response);
            //}

            response = HelperMethods.CreateResponse("200", null, new { trackAppointmentModel = await q.ToListAsync() });
            return Ok(response);
        }

        [HttpGet(Routes.CancelAppointmentRoute, Name = Routes.CancelAppointmentRoute)]
        public IActionResult CancelAppointment([FromQuery] Guid appointId)
        {
            string patId = userInfoProvider.GetUserId(User);

            var ap = db.Appointments.FirstOrDefault(app => app.AppointId == appointId && app.PatId == patId);

            if (ap is null)
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "404", Message = "Not found" }, null);
                return Ok(response);
            }

            if ((ap.AppointStatus == AppointmentStatus.current ||
                ap.AppointStatus == AppointmentStatus.waiting) &&
                ap.AppointDate == DateTime.Now.Date)
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "400", Message = "this appointment is currently in progress try later" }, null);
                return Ok(response);
            }

            // check if it has report
            var r = db.PatReports.FirstOrDefault(rp => rp.AppointId == ap.AppointId);
            if (r is null)
            {
                db.Appointments.Remove(ap);
                db.SaveChanges();

                response = HelperMethods.CreateResponse("200", null, new { canceled = true });
                return Ok(response);
            }

            // as report table referencing it we can not completely remove it
            ap.RemovedFromView = true;
            db.SaveChanges();

            response = HelperMethods.CreateResponse("200", null, new { canceled = true });
            return Ok(response);
        }
    }
}