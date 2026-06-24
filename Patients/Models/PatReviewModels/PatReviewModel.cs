using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models.PatReviewModels
{
    public class PatReviewModel
    {
        // kept as string because, new review model(with empty reviewId) will give json conversion error
        public string ReviewId { get; set; }
        [Required]
        public Guid DrId { get; set; }
        public string ReviewText { get; set; }
        [Range(0, 5)]
        public int Rating { get; set; }
        // when using inside ViewModel
        public string ReviewDate { get; set; }
    }
}
