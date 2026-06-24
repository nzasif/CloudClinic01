using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Data.DbSets
{
    public class StaffDetail
    {
        [Key]
        public Guid StaffId { get; set; }
        public int StaffSalary { get; set; }

        [Required]
        public Guid DrId { get; set; }
        public DrDetail? DrDetails { get; set; }
        public required string UserId { get; set; }
        public AppUser? AppUser { get; set; }
    }
}
