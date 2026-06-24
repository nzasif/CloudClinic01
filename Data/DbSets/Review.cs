using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudClinic.Data.DbSets
{
    public class Review
    {
        [Key]
        public Guid ReviewId { get; set; }
        public Guid DrId { get; set; }
        [NotMapped]
        public DrDetail? DrDetail { get; set; }
        public string? PatId { get; set; }
        [NotMapped]
        public AppUser? AppUser { get; set; }
        public string? ReviewText { get; set; }
        public int Rating { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool IsVisible { get; set; }
        public bool IsReported { get; set; }
    }
}
