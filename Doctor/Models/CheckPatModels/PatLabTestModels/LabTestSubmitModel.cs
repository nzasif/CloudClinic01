using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.CheckPatModels.PatLabTestModels
{
    public class LabTestSubmitModel
    {
        [Required]
        public Guid LabTestId { get; set; }
        [Required]
        public string LabTestResult { get; set; }
        public int LabTestFee { get; set; }
        public IFormFile AttachmentFile { get; set; }
        [Required]
        public Guid PatReportId { get; set; }
    }
}
