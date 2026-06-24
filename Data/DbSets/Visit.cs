using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Data.DbSets
{
    public class Visit
    {
        [Key]
        public Guid VisitId { get; set; }
        public DateTime VisitDate { get; set; }
        public Guid DrId { get; set; }
        [NotMapped]
        public DrDetail? DrDetail { get; set; }
        public required string PatId { get; set; }
        [NotMapped]
        public AppUser? AppUser { get; set; }
        public Guid PatReportId { get; set; }
        [NotMapped]
        public PatReport? PatReport { get; set; }
        public Guid AppointId { get; set; }
        [NotMapped]
        public Appointment? Appointment { get; set; }
        public string? DrPracTimeName { get; set; }
        public Guid PaymentId { get; set; }
        [NotMapped]
        public Payment? Payment { get; set; }
    }
}
