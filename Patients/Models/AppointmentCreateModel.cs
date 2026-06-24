using CloudClinic.Shared.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models
{
    public class AppointmentCreateModel
    {
        [Required]
        public Guid DrId { get; set; }
        // normal datetime formate(24hr): 2020-05-27T18:29:00.3921242+05:00
        [Required]
        [DataType(DataType.Date)]
        [ValidateAppointDate]
        public DateTime AppointDate { get; set; }
        //[ValidateDrPracTime]
        //public string DrPracTime { get; set; }
        [Required]
        public string DrPracTimeName { get; set; }
        public DateTime DrPracStartTime { get; set; }
        public DateTime DrPractEndTime { get; set; }
    }
}
