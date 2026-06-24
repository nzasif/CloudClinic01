using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Data.DbSets
{
    public class PatXray
    {
        [Key]
        public Guid XrayId { get; set; }
        public string? XrayName { get; set; }
        public string? XrayType { get; set; }
        public string? XrayResult { get; set; }
        public int XrayFee { get; set; }
        public string? AttachmentName { get; set; }
        public DateTime XrayReferDateTime { get; set; }
        public DateTime XraySubmitDateTime { get; set; }
        public Guid PatReportId { get; set; }
        public PatReport? PatReport { get; set; }

    }
}
