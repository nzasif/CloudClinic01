using CloudClinic.Account.Models;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Doctor.Models;
using CloudClinic.Patients.Models;
using CloudClinic.Shared.AuthConstants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Services
{
    public class DrDetailsProvider: IDrDetailsProvider
    {
        // return the number of booked appointments (at specified date and time) of a Dr.
        // e.g. booked appointments for drId at 2010-04-14, time: at morning
        public async Task<int> GetBookedAppointCountAsync(CloudClinicDb db, Guid drId, DateTime date, string drPracTimeName)
        {
            int count = await db.Appointments.CountAsync(appoint => 
                appoint.DrId == drId &&
                appoint.AppointDate == date.Date &&
                appoint.DrPracTimeName == drPracTimeName);

            return count;
        }

        public async Task<PatDrProfileViewModel> GetDrProfileAsync(CloudClinicDb db, string drId)
        {
            var drProfile = await (from dr in db.DrDetails where (dr.DrId == new Guid(drId))
                                            join dru in db.AppUsers on dr.UserId equals dru.Id
                                            select new PatDrProfileViewModel
                                            {
                                                DrName = dru.FullName,
                                                DrAddress = dru.Street + ", " + dru.City + ", " + dru.Province,
                                                DrPhoneNumber = dru.PhoneNumber,
                                                DrSpecialty = dr.DrSpecialty
                                            }).FirstOrDefaultAsync();
            return drProfile;
        }

        public DrDetail GetDrDetail(CloudClinicDb db, string drId)
        {
            return db.DrDetails.FirstOrDefault(dr => dr.DrId == new Guid(drId));
        }

        public DrHoliday GetDrHoliday(CloudClinicDb db, string drId, Guid hollidayId)
        {
            return db.DrHolidays.FirstOrDefault(h => h.DrHolidayId == hollidayId && h.DrId == new Guid(drId));
        }

        public List<DrHolidayViewModel> GetDrHolidays(CloudClinicDb db, string drId)
        {
            var q = (from h in db.DrHolidays
                     where h.DrId == new Guid(drId)
                     select new DrHolidayViewModel
                     {
                         DrHolidayId = h.DrHolidayId.ToString(),
                         DrHolidayName = h.DrHolidayName,
                         EntryDate = h.EntryDate.ToShortDateString()
                     });
            if (!q.Any())
            {
                return null;
            }

            return q.ToList();
        }

        public DrPracTime GetDrPracTime(CloudClinicDb db, string drId, Guid drPracTimeId)
        {
            return db.DrPracTimes.FirstOrDefault(drPt =>
                   drPt.DrId == new Guid(drId) && drPt.DrPracTimeId == drPracTimeId);
        }

        public List<DrPracTimeViewModel> GetDrPracTimes(CloudClinicDb db, string drId)
        {
            var q = (from pt in db.DrPracTimes
                     where pt.DrId == new Guid(drId)
                     select new DrPracTimeViewModel
                     {
                         DrPracTimeId = pt.DrPracTimeId.ToString(),
                         DrPracTimeName = pt.DrPracTimeName,
                         DrPracStartTime = pt.DrPracStartTime.ToShortTimeString(),
                         DrPracEndTime = pt.DrPracEndTime.ToShortTimeString(),
                         DrMaxAppointments = pt.DrMaxAppointments
                     });

            if(!q.Any())
            {
                return null;
            }

            return q.ToList();
        }

        public int GetDrFee(CloudClinicDb db, string drId)
        {
            return db.DrDetails.Find(new Guid(drId)).DrFee;
        }

        public string IsDrVerified(ClaimsPrincipal user)
        {
            return user.FindFirst(IsDrVerifiedClaim.Name).Value;
        }

        public string GetDrUserId(ClaimsPrincipal user)
        {
            return user.FindFirst(UserIdClaim.Name).Value;
        }

        public string GetDrId(ClaimsPrincipal user)
        {
            return user.FindFirst(DrIdClaim.Name).Value;
        }
    }
}
