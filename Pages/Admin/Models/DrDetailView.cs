using CloudClinic.Account.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Admin.Models
{
    public class DrDetailView
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
        public string RegisterdDate { get; set; }

        // if unverified
        public string UnVerificationCause { get; set; }
        public List<DrReview> Reviews { get; set; }
    }

    public class DrReview
    {
        public string PatName { get; set; }
        public string PatAddress { get; set; }
        public string PatPhoneNumber { get; set; }
        public string PatProfilePic { get; set; }

        public string ReviewText { get; set; }
        public int Rating { get; set; }
        public string date { get; set; }
    }
}
