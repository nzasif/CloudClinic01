using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Pages.Admin.Models
{
    public class ReportedReview
    {
        public string PatId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string ProfilePicUri { get; set; }

        public Guid ReviewId { get; set; }
        public string ReviewText { get; set; }
        public string ReviewDate { get; set; }
        public bool IsVisible { get; set; }
    }
}
