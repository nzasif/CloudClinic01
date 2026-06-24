using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Doctor.Models;
using CloudClinic.Doctor.Services;
using CloudClinic.Patients.Models;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Services
{
    public class PatAppointmentService: IPatAppointmentService
    {
        private IDrDetailsProvider drDetailsProvider { get; set; }
        public PatAppointmentService(IDrDetailsProvider DrDetailsProvider)
        {
            drDetailsProvider = DrDetailsProvider;
        }

        // this method gets called after IsAppointAvailableAsync(...);
        public async Task<string> CreateAppointmentAsync(CloudClinicDb db, AppointmentCreateModel model, string userId)
        {
            Appointment appointment = new Appointment
            {
                PatId = userId,
                DrId = model.DrId,
                AppointDate = model.AppointDate,
                DrPracTimeName = model.DrPracTimeName,
                DrPracStartTime = model.DrPracStartTime,
                DrPracEndTime = model.DrPractEndTime,
                AppointStatus = AppointmentStatus.pending,
                AppointCreateDate = DateTime.Now
            };

            await db.Appointments.AddAsync(appointment);

            int x = await db.SaveChangesAsync().ConfigureAwait(true);

            if (x > -1)
            {
                return appointment.AppointId.ToString();
            }
            else
            {
                return null;
            }
        }

        // this method gets called before CreateAppointment(...);
        public async Task<string> IsAppointAvailableAsync(CloudClinicDb db, Guid drId, DateTime appointDate, string drPracTimeName)
        {
            DrDetail dr = db.DrDetails.FirstOrDefault(dr => dr.DrId == drId
                                                     && dr.IsVerified == true
                                                     && dr.IsVisible == true);
            // first check for dr
            if (dr is null)
            {
                return "error: This dr is not exist";
            }

            DrPracTime drPracTime = db.DrPracTimes.FirstOrDefault(t => t.DrPracTimeName == drPracTimeName && t.DrId == drId);
            
            if (drPracTime is null)
            {
                return "error: Dr. does not has this practime";
            }

            // check if it is not a holiday
            string day = appointDate.DayOfWeek.ToString();

            DrHoliday drHolliday = db.DrHolidays.FirstOrDefault(drH => drH.DrId == drId && drH.DrHolidayName == day);

            if (drHolliday != null)
            {
                return "error: It is Dr. Holliday";
            }

            // check if dr does practice at the specified drPracTime, and max appointn not reached
            int bookedAppointments = await drDetailsProvider.GetBookedAppointCountAsync(db, drId, appointDate, drPracTimeName);

            if (bookedAppointments >= drPracTime.DrMaxAppointments)
            {
                return "error: Dr. Max appoints reached";
            }

            // if it is today, then confirm that the time specified is not over
            if (appointDate.Date == DateTime.Now.Date)
            {
                TimeSpan drT = (drPracTime.DrPracEndTime.TimeOfDay == TimeSpan.FromHours(0)) ? TimeSpan.FromHours(23.59) : drPracTime.DrPracEndTime.TimeOfDay;

                if (drT < DateTime.Now.TimeOfDay)
                {
                    return $"error: This time is over, drPracTime End time: { drPracTime.DrPracEndTime.TimeOfDay + ", time of now: " + DateTime.Now.TimeOfDay}";
                }
            }

            // if reach this so far then everything is ok
            return null;
        }

        public async Task<List<PatAppointmentDetailViewModel>> ViewAllAppointmentsAsync(CloudClinicDb db, string patId, HttpContext httpContext)
        {
            var q = (from ap in db.Appointments
                     where ap.PatId == patId && ap.RemovedFromView == false
                     join dr in db.DrDetails on ap.DrId equals dr.DrId
                     join dru in db.AppUsers on dr.UserId equals
                     dru.Id
                     select new PatAppointmentDetailViewModel
                     {
                         AppointId = ap.AppointId,
                         DrProfilePicUri = HelperMethods.GenerateProfilePicUri(httpContext, dru.ProfilePicName),
                         DrName = dru.FullName,
                         DrSpecialty = dr.DrSpecialty,
                         DrAddress = dru.Street + ", " + dru.City + ", " + dru.Province,
                         DrId = dr.DrId,
                         DrAvgCheckTime = dr.DrAvgCheckTime,
                         AppointDate = ap.AppointDate.Date,
                         AppointCreateDate = ap.AppointCreateDate.ToShortDateString() +" "+ ap.AppointCreateDate.ToLongTimeString(),
                         DrPracStartTime = ap.DrPracStartTime.ToShortTimeString(),
                         DrPracEndTime = ap.DrPracEndTime.ToShortTimeString(),
                         DrPracTimeName = ap.DrPracTimeName,
                         DrPhoneNumber = dru.PhoneNumber,
                         AppointStatus = ap.AppointStatus.ToString()
                     });

            // 
            if (!q.Any())
            {
                return null;
            }

            var appoints = q.ToList();

            DateTime dateTime = DateTime.Now.Date;

            foreach (var ap in appoints)
            {
                if (ap.AppointStatus != "done" && ap.AppointDate < dateTime)
                {
                    ap.AppointStatus = "expired";
                }

                if (ap.AppointStatus != "done" && ap.AppointDate.Date >= dateTime)
                {
                    TimeSpan time = ap.AppointDate.Date.Subtract(dateTime);
                    ap.AppointRemainingTime = (time.TotalDays).ToString();
                }
            }

            return appoints;
        }
    }
}
