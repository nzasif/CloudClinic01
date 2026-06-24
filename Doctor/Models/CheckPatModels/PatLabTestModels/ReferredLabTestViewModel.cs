using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.CheckPatModels.PatLabTestModels
{
    // used by lab. staff
    public class ReferredLabTestViewModel
    {
        public string PatName { get; set; }
        public string GuardianName { get; set; }
        public string PatProfilePicUri { get; set; }
        public string PatPhoneNumber { get; set; }
        public Guid LabTestId { get; set; }
        public string LabTestName { get; set; }
        public string LabTestResult { get; set; }
        public int LabTestFee { get; set; }
        public string AttachmentUri { get; set; }
        public string LabTestSubmitDateTime { get; set; }
        public string LabTestReferredDateTime { get; set; }
        public Guid PatReportId { get; set; }
    }
}
