using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Shared.AuthConstants
{
    // Must Add All of these Roles to table 'Roles' in Db, manually.
    public static class Roles
    {
        // this is the master role for all type of staff
        // it helps to allow all staff to retriew their profile info
        public const string StaffRole = "STAFF";

        public const string DrLabStaffRole = "DRLABSTAFF";
        public const string DrXrayStaffRole = "DRXRAYSTAFF";
        public const string DrAssistantRole = "DRASSISTANT";
        public const string DrPharmacyStaffRole = "DRPHARMACYSTAFF";
        public const string DrAccountantRole = "DRACCOUNTANT";
        
        public const string DrRole = "DR";
        public const string NormalUserRole = "NORMALUSER";
        public const string AdminRole = "ADMIN";

    }
}
