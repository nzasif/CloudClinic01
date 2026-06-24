using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.CheckPatModels.ReportModels
{
    public class PatReportCreateModel
    {
        [Required]
        public Guid PatReportId { get; set; }
        public string Diagnosis { get; set; }
        public string Remarks { get; set; }
        public string Symptoms { get; internal set; }
    }
}
