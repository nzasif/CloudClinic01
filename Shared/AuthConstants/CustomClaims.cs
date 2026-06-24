using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Shared.AuthConstants
{
    public static class IsRemovedClaim
    {
        public const string Name = "isRemoved";
        public const string Value = "False";
    }

    public static class IsDrVerifiedClaim
    {
        public const string Name = "isDrVerified";
        public const string Value = "True";
    }  
    
    public static class UserIdClaim
    {
        public const string Name = "userId";
        public const string Value = "";
    }
    
    public static class UserNameClaim
    {
        public const string Name = "userName";
        public const string Value = "";
    }    
    
    public static class UserEmailClaim
    {
        public const string Name = "userEmail";
        public const string Value = "";
    }
    
    public static class UserPhoneNumberClaim
    {
        public const string Name = "userPhoneNumber";
        public const string Value = "";
    }

    public static class UserFullNameClaim
    {
        public const string Name = "userFullName";
        public const string Value = "";
    }

    public static class RoleClaim
    {
        public const string Name = "role";
        public const string Value = "";
    }

    public static class DrIdClaim
    {
        public const string Name = "drId";
        public const string Value = "";
    }
    
    public static class StaffIdClaim
    {
        public const string Name = "staffId";
        public const string Value = "";
    }

    // as DrId itself is always included in staff token
    //public static class StaffDrIdClaim
    //{
    //    public const string Name = "staffDrId";
    //    public const string Value = "";
    //}

    public static class StaffDrUserIdClaim
    {
        public const string Name = "staffDrUserId";
        public const string Value = "";
    }
}
