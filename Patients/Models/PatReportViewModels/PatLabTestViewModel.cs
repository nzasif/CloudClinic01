using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models.PatReportViewModels
{
    public class PatLabTestViewModel
    {
        public string LabTestName { get; set; }
        public string LabTestResult { get; set; }
        public string AttachmentUri { get; set; }
    }
}
