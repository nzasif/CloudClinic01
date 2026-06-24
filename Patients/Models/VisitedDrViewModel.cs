using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models
{
    public class VisitedDrViewModel
    {
        public Guid DrId { get; set; }
        public string DrName { get; set; }
        public string DrProfilePicUri { get; set; }
        public string DrSpecialty { get; set; }
        public string DrAddress { get; set; }
        public string VisitDate { get; set; }
        public Guid ReportId { get; set; }
        public Guid PaymentId { get; set; }
    }
}
