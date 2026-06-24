using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.CheckPatModels.ReportModels
{
    public class PatMedHistoryModel
    {
        public string DrName { get; set; }
        public string DrSpecialty { get; set; }
        public string PatId { get; set; }
        public string DrAddress { get; set; }
        public Guid ReportId { get; set; }
        public string Diagnosis { get; set; }
        public string ReportDate { get; set; }
    }
}
