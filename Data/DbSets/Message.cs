using System.ComponentModel.DataAnnotations;

namespace CloudClinic.Data.DbSets
{
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; }
        public Guid DrId { get; set; }
        public DrDetail? DrDetail { get; set; }
        public string? PatId { get; set; }
        public AppUser? AppUser { get; set; }
        public string? MessageText { get; set; }
        public bool IsRead { get; set; }
        public bool IsVisible { get; set; }
        public DateTime MessageDate { get; set; }
    }
}
