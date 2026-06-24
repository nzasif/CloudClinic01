using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models.PatReviewModels
{
    public class PatReviewViewModel
    {
        public Guid DrId { get; set; }
        public string DrName { get; set; }
        public string DrProfilePicUrl { get; set; }
        public PatReviewModel PatReview { get; set; }
    }
}
