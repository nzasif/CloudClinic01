using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Pages.Admin.Models
{
    public class PatView
    {
        public string PatId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string GuardianName { get; set; }
        public string Gender { get; set; }
        public string DOB { get; set; }
        public string Address { get; set; }
        public string ProfilePicUri { get; set; }
        public bool IsRemoved { get; set; }
        public string RegDate { get; set; }

        public string RemovalCause { get; set; }
    }
}
