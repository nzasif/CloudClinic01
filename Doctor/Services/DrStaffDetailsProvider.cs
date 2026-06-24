using CloudClinic.Data;
using CloudClinic.Data.DbSets;

namespace CloudClinic.Doctor.Services
{
    public class DrStaffDetailsProvider: IDrStaffDetailsProvider
    {
        public StaffDetail GetStaffDetail(CloudClinicDb db, string drId, string staffId)
        {
            return db.StaffDetails.FirstOrDefault(staff => staff.DrId == new Guid(drId) && staff.StaffId == new Guid(staffId));
        }

        // for staff itself
        public AppUser GetStaffProfile(CloudClinicDb db, string staffUserId)
        {
            return db.AppUsers.FirstOrDefault(u => u.Id == staffUserId);
        }
        
        // for staff itself
        public StaffDetail GetStaffDetail(CloudClinicDb db, string staffUserId)
        {
            return db.StaffDetails.FirstOrDefault(a => a.UserId == staffUserId);
        }

        public string GetStaffUserId(CloudClinicDb db, string drId, string staffId)
        {
            return db.StaffDetails.FirstOrDefault(staff => staff.DrId == new Guid(drId)
                   && staff.StaffId == new Guid(staffId)).UserId;
        }

        public string GetStaffId(CloudClinicDb db, string drId, string staffUserId)
        {
            return db.StaffDetails.FirstOrDefault(staff => staff.DrId == new Guid(drId)
                   && staff.UserId == staffUserId).StaffId.ToString();

        }

        // any number of staff members with the specified roles is allowed...

        //public Task<bool> IsStaffExist(CloudClinicDb db, string drId, string role)
        //{
            
        //    return db.StaffDetails.AnyAsync(a => a.DrId == new Guid(drId));
        //}
    }
}
