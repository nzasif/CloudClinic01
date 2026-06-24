using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Doctor.Models;
using CloudClinic.Patients.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Services
{
    public interface IDrDetailsProvider
    {
        DrPracTime GetDrPracTime(CloudClinicDb db, string drId, Guid drPracTimeId);
        List<DrPracTimeViewModel> GetDrPracTimes(CloudClinicDb db, string drId);
        Task<int> GetBookedAppointCountAsync(CloudClinicDb db, Guid drId, DateTime date, string drPracTimeName);
        Task<PatDrProfileViewModel> GetDrProfileAsync(CloudClinicDb db, string drId);
        DrDetail GetDrDetail(CloudClinicDb db, string drId);
        DrHoliday GetDrHoliday(CloudClinicDb db, string drId, Guid hollidayId);
        List<DrHolidayViewModel> GetDrHolidays(CloudClinicDb db, string drId);
        int GetDrFee(CloudClinicDb db, string drId);
        string GetDrUserId(ClaimsPrincipal user);
        string GetDrId(ClaimsPrincipal user);
        string IsDrVerified(ClaimsPrincipal user);
    }
}