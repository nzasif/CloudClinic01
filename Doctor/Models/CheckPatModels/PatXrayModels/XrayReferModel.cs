using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.CheckPatModels.PatXrayModels
{
    public class XrayReferModel
    {
        [Required]
        public string XrayName { get; set; }
        [Required]
        public string XrayType { get; set; }
        [Required]
        public Guid PatReportId { get; set; }
    }
}
