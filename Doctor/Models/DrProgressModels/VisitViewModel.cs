using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.DrProgressModels
{
    public class VisitViewModel
    {
        public string VisitDate { get; set; }
        public List<VisitView> VisitsList { get; set; }
    }

    public class VisitView
    {
        public Guid VisitId { get; set; }
        public string PatId { get; set; }
        public string PatName { get; set; }
        public string PatProfilePicUri { get; set; }
        public Guid PatReportId { get; set; }
        public Guid AppointId { get; set; }
        public string DrPracTimeName { get; set; }
        public Guid PaymentId { get; set; }

        // for grouping it is needed
        public DateTime VisitDate { get; set; }
    }
}
