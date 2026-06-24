using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Doctor.Models;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Services
{
    public class DrAppointmentService : IDrAppointmentService
    {
        public string ChangeAppointmentStatus(CloudClinicDb db, string drId, Guid appointId, string status)
        {
            AppointmentStatus appointmentStatus = AppointmentStatus.pending;

            switch (status)
            {
                case "pending":
                    appointmentStatus = AppointmentStatus.pending;
                    break;
                case "waiting":
                    appointmentStatus = AppointmentStatus.waiting;
                    break;
                // rejection is done by assistant controller directly
                //
                // it is not using this method
                // this is only used during patient checking process
                //
                //case "rejected":
                //    appointmentStatus = AppointmentStatus.rejected;
                //    break;
                case "current":
                    appointmentStatus = AppointmentStatus.current;
                    break;
                case "done":
                    appointmentStatus = AppointmentStatus.done;
                    break;
                default:
                    return null;
            }

            // appointment must of today date
            Appointment appointment = db.Appointments.FirstOrDefault(ap => ap.AppointId == appointId
                                      && ap.DrId == new Guid(drId)
                                      && ap.AppointDate.Date == DateTime.Now.Date);
            if (appointment is null)
            {
                return null;
            }

            // to avoid unneccessay db call
            // specially from GeneratePatReport method
            if (appointment.AppointStatus != appointmentStatus)
            {
                appointment.AppointStatus = appointmentStatus;
                db.SaveChanges();
            }

            return status;
        }

        // by default this will return today appointments, today date will be provided by client
        // and upcomming appointments...
        public async Task<List<DrAppointmentDetailtViewModel>> DrSearchAppointments(CloudClinicDb db, HttpContext httpContext, string drId, DateTime appointDate, string drPracTimeName)
        {
            var q = (from ap in db.Appointments
                     where ap.DrId == new Guid(drId)
                     && ap.RemovedFromView == false
                     && ap.DrPracTimeName == drPracTimeName
                     && ap.AppointDate.Date == appointDate.Date
                     join u in db.AppUsers on ap.PatId equals u.Id
                     orderby ap.AppointCreateDate ascending
                     select new DrAppointmentDetailtViewModel
                     {
                         AppointId = ap.AppointId,
                         AppointDate = ap.AppointDate.ToShortDateString(),
                         AppointCreateDate = ap.AppointCreateDate.ToShortDateString()+ ", " +ap.AppointCreateDate.ToLongTimeString(),
                         AppointStatus = (ap.AppointDate.Date < DateTime.Now.Date) ? "expired" : ap.AppointStatus.ToString(),
                         DrPracTimeName = drPracTimeName,
                         PatId = ap.PatId,
                         PatName = u.FullName,
                         PatDOB = u.DOB.ToShortDateString(),
                         PatPhoneNumber = u.PhoneNumber,
                         PatAddress = u.Street + ", " + u.City + ", " + u.Province,
                         ProfilePicUri = HelperMethods.GenerateProfilePicUri(httpContext, u.ProfilePicName)
                     });
            if (!q.Any())
            {
                return null;
            }

            return q.ToList();
        }

        public async Task<List<DrAppointmentDetailtViewModel>> GetTodayAppointments(
            CloudClinicDb db, HttpContext httpContext, string drId, string drPracTimeName)
        {
            var q = (from ap in db.Appointments
                     where ap.DrId == new Guid(drId)
                     && ap.RemovedFromView == false
                     && ap.AppointDate.Date == DateTime.Now.Date
                     && ap.DrPracTimeName == drPracTimeName
                     && ap.AppointStatus != AppointmentStatus.rejected
                     join u in db.AppUsers on ap.PatId equals u.Id
                     orderby ap.AppointCreateDate ascending
                     select new DrAppointmentDetailtViewModel
                     {
                         AppointId = ap.AppointId,
                         AppointDate = ap.AppointDate.ToShortDateString(),
                         AppointCreateDate = ap.AppointCreateDate.ToShortDateString() + ", " + ap.AppointCreateDate.ToLongTimeString(),
                         AppointStatus = ap.AppointStatus.ToString(),
                         DrPracTimeName = ap.DrPracTimeName,
                         PatId = ap.PatId,
                         PatName = u.FullName,
                         PatDOB = u.DOB.ToShortDateString(),
                         PatGender = u.Gender,
                         PatPhoneNumber = u.PhoneNumber,
                         PatAddress = u.Street + ", " + u.City + ", " + u.Province,
                         ProfilePicUri = HelperMethods.GenerateProfilePicUri(httpContext, u.ProfilePicName)
                     });
            if (!q.Any())
            {
                return null;
            }

            return q.ToList();
        }

        public async Task<DrAppointmentDetailtViewModel> GetDrAppointmentDetail(CloudClinicDb db, string drId, string appointId, HttpContext httpContext)
        {
            var drAppointmentDetailts = (from ap in db.Appointments
                                         where ap.DrId == new Guid(drId)
                                         && ap.AppointId == new Guid(appointId)
                                         && ap.RemovedFromView == false
                                         join drpracTime in db.DrPracTimes on ap.DrPracTimeName equals drpracTime.DrPracTimeName
                                         join u in db.AppUsers on ap.PatId equals u.Id
                                         orderby ap.AppointCreateDate ascending
                                         select new DrAppointmentDetailtViewModel
                                         {
                                             AppointId = ap.AppointId,
                                             AppointDate = ap.AppointDate.ToShortDateString(),
                                             AppointCreateDate = 
                                             ap.AppointCreateDate.ToShortDateString() + " " + ap.AppointCreateDate.ToLongTimeString(),
                                             AppointStatus = ap.AppointStatus.ToString(),
                                             DrPracTimeName = ap.DrPracTimeName,
                                             DrPracEndTime = ap.DrPracEndTime.ToShortTimeString(),
                                             DrPracStartTime = ap.DrPracStartTime.ToShortTimeString(),
                                             PatId = ap.PatId,
                                             PatName = u.FullName,
                                             PatDOB = u.DOB.ToShortDateString(),
                                             PatPhoneNumber = u.PhoneNumber,
                                             PatAddress = u.Street + ", " + u.City + ", " + u.Province,
                                             ProfilePicUri = HelperMethods.GenerateProfilePicUri(httpContext, u.ProfilePicName)
                                         }).FirstOrDefault();

            return drAppointmentDetailts;
        }

        public async Task<List<DrTotalAppointsViewModel>> GetTotalAppointments(CloudClinicDb db, string drId)
        {
            var q = (from ap in db.Appointments
                     where ap.DrId == new Guid(drId) && ap.AppointDate.Date == DateTime.Now.Date && ap.RemovedFromView == false
                     select new DrTotalAppointsViewModel
                     {
                         DrPracTimeName = ap.DrPracTimeName
                     });

            if (!q.Any())
            {
                return null;
            }

            return q.ToList();
        }
    }
}
