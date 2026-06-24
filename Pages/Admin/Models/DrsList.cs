using CloudClinic.Account.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Admin.Models
{
    public class DrsList
    {
        public Guid DrId { get; set; }
        public string DrUserId { get; set; }
        public string DrName { get; set; }
        public string DrAddress { get; set; }
        public string DrGender { get; set; }
        public string DrPhoneNumber { get; set; }
        public string DrEmail { get; set; }
        public string DrSpecialty { get; set; }
        public string DrProfilePicUri { get; set; }
        public bool IsVerified { get; set; }
        public List<string> DrPracTimes { get; set; }
        public string RegisterdDate { get; set; }
    }
}
