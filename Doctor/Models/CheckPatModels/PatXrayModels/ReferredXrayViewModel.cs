using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.CheckPatModels.PatXrayModels
{
    public class ReferredXrayViewModel
    {
        public string PatName { get; set; }
        public string PatProfilePicUri { get; set; }
        public string GuardianName { get; set; }
        public string PatPhoneNumber { get; set; }
        public Guid XrayId { get; set; }
        public string XrayName { get; set; }
        public string XrayType { get; set; }
        public string XrayResult { get; set; }
        public int XrayFee { get; set; }
        public string AttachmentUri { get; set; }
        public string XrayReferredDateTime { get; set; }
        public string XraySubmitDateTime { get; set; }
        public Guid PatReportId { get; set; }
    }
}
