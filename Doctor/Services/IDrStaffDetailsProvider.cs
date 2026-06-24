using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Services
{
    public interface IDrStaffDetailsProvider
    {
        StaffDetail GetStaffDetail(CloudClinicDb db, string drId, string staffId);
        AppUser GetStaffProfile(CloudClinicDb db, string staffUserId);
        //Task<bool> IsStaffExist(CloudClinicDb db, string drId, string role);
        // for staff itself
        StaffDetail GetStaffDetail(CloudClinicDb db, string staffUserId);
        string GetStaffUserId(CloudClinicDb db, string drId, string staffId);
        string GetStaffId(CloudClinicDb db, string drId, string staffUserId);
    }
}
