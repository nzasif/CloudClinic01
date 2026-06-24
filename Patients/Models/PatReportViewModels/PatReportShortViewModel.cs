using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models.PatReportViewModels
{
    public class PatReportShortViewModel
    {
        public string ReportId { get; set; }
        public DateTime ReportDate { get; set; }
        public string DrName { get; set; }
        public string DrProfilePicUri { get; set; }
        public string DrSpecialty { get; set; }
    }
}
