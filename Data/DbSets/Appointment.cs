using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudClinic.Data.DbSets
{
    public class Appointment
    {
        [Key]
        public Guid AppointId { get; set; }

        // Foreign Key to AppUser (Patient)
        [Required]
        public string PatId { get; set; } = string.Empty;

        [ForeignKey(nameof(PatId))]
        public AppUser? AppUser { get; set; }

        // Foreign Key to DrDetail
        [Required]
        public Guid DrId { get; set; }

        [ForeignKey(nameof(DrId))]
        public DrDetail? DrDetail { get; set; }

        [Required]
        public DateTime AppointDate { get; set; }

        [Required]
        public string DrPracTimeName { get; set; } = string.Empty;

        public DateTime DrPracStartTime { get; set; }
        public DateTime DrPracEndTime { get; set; }

        // Enums work best with DataAnnotations or Fluent API configuration
        public AppointmentStatus AppointStatus { get; set; }

        public DateTime AppointCreateDate { get; set; } = DateTime.UtcNow;

        public bool RemovedFromView { get; set; } = false;
    }
}