using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.CheckPatModels.ReportModels
{
    public class InitReportViewModel
    {
        public Guid PatReportId { get; set; }
        public Guid AppointId { get; set; }
        public bool IsPending { get; set; }
        public string DrPracTimeName { get; set; }
        public string PatId { get; set; }
        public Guid VisitId { get; set; }
        public Guid PaymentId { get; set; }
    }
}
