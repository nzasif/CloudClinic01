using System.ComponentModel.DataAnnotations;

namespace CloudClinic.Data.DbSets
{
    public class DrDetail
    {
        [Key]
        public Guid DrId { get; set; }
        public string? DrQualification { get; set; }
        public string? DrSpecialty { get; set; }
        public string? DrExperience { get; set; }
        public int DrAvgCheckTime { get; set; }
        public int DrFee { get; set; }
        public bool IsVerified { get; set; }
        public bool IsVisible { get; set; }
        public string? DrDescription { get; set; }
        public required string UserId { get; set; }
        public AppUser? AppUser { get; set; }

        // as create date is same as AppUser regDate; here only updateDate
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
    }
}
