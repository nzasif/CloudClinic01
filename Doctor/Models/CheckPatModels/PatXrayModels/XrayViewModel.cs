using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.CheckPatModels.PatXrayModels
{
    public class XrayViewModel
    {
        public Guid XrayId { get; set; }
        public Guid PatReportId { get; set; }
        public string XrayName { get; set; }
        public string XrayType { get; set; }
        public string XrayResult { get; set; }
        public int XrayFee { get; set; }
        public string AttachmentUri { get; set; }
        public string XrayReferredDateTime { get; set; }
        public string XraySubmitDateTime { get; set; }
    }
}
