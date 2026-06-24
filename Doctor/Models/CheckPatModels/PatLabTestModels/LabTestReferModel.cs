using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.CheckPatModels.PatLabTestModels
{
    public class LabTestReferModel
    {
        public string LabTestName { get; set; }
        public Guid PatReportId { get; set; }
    }
}
