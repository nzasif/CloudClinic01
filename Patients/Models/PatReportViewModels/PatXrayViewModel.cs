using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models.PatReportViewModels
{
    public class PatXrayViewModel
    {
        public string XrayName { get; set; }
        public string XrayType { get; set; }
        public string XrayResult { get; set; }
        public string AttachmentUri { get; set; }
    }
}
