using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CloudClinic.Data
{
    public class AppUser : IdentityUser
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        public string? GuardianName { get; set; }

        [Required, StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [Required, StringLength(200)]
        public string Street { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Province { get; set; } = string.Empty;

        public string? ProfilePicName { get; set; } = string.Empty;

        public bool IsRemoved { get; set; } = false;

        public DateTime RegDate { get; set; } = DateTime.UtcNow;

        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
    }
}