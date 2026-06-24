using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models
{
    public class DrReviewViewModel
    {
        public string ReviewText { get; set; }
        public int Rating { get; set; }
        public bool IsReported { get; set; }
        public string ReviewDate { get; set; }
        public string PatName { get; set; }
        public string PatAddress { get; set; }
        public string PatProfilePicUri { get; set; }
        public Guid ReviewId { get; set; }
    }
}
