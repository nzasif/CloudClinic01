using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.CheckPatModels.PatXrayModels
{
    public class XraySubmitModel
    {
        [Required]
        public Guid XrayId { get; set; }
        [Required]
        public string XrayResult { get; set; }
        public int XrayFee { get; set; }
        public IFormFile AttachmentFile { get; set; }
        [Required]
        public Guid PatReportId { get; set; }
    }
}
