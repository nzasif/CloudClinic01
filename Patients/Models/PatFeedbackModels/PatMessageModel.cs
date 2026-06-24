using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models.PatFeedbackModels
{
    public class PatMessageModel
    {
        [Required]
        public Guid DrId { get; set; }
        [Required]
        public string MessageText { get; set; }
    }
}
