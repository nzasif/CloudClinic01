using CloudClinic.Data.DbSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.CheckPatModels.PatLabTestModels
{
    // used by Dr itself
    public class LabTestViewModel
    {
        public Guid LabTestId { get; set; }
        public Guid PatReportId { get; set; }
        public string LabTestName { get; set; }
        public string LabTestResult { get; set; }
        public int LabTestFee { get; set; }
        public string LabTestSubmitDateTime { get; set; }
        public string LabTestReferDateTime { get; set; }
        public string AttachmentUri { get; set; }
    }
}
