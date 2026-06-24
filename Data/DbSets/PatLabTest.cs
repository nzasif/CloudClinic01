using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Data.DbSets
{
    public class PatLabTest
    {
        [Key]
        public Guid LabTestId { get; set; }
        public string? LabTestName { get; set; }
        public string? LabTestResult { get; set; }
        public int LabTestFee { get; set; }
        public string? AttachmentName { get; set; }
        public Guid PatReportId { get; set; }
        public PatReport? PatReport { get; set; }
        public DateTime LabTestSubmitDateTime { get; set; }
        public DateTime LabTestReferDateTime { get; set; }
    }
}
