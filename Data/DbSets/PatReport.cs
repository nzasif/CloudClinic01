using System.ComponentModel.DataAnnotations;

namespace CloudClinic.Data.DbSets
{
    public class PatReport
    {
        [Key]
        public Guid PatReportId { get; set; }
        public string? Symptoms { get; set; }
        public string? Diagnosis { get; set; }
        public string? Remarks { get; set; }
        public DateTime ReportDate { get; set; }
        public required string PatId { get; set; }
        public AppUser? AppUser { get; set; }
        public Guid DrId { get; set; }
        public DrDetail? DrDetail { get; set; }
        public Guid AppointId { get; set; }
        public Appointment? Appointment { get; set; }
        public required string DrPracTimeName { get; set; }
        public bool IsPending { get; set; }

        // for user
        public bool IsDeleted { get; set; }
        public bool IsVisible { get; set; }
    }
}
