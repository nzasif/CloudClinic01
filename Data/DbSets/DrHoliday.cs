using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Data.DbSets
{
    public class DrHoliday
    {
        [Key]
        public Guid DrHolidayId { get; set; }
        public string? DrHolidayName { get; set; }
        public DateTime EntryDate { get; set; }
        public Guid DrId { get; set; }
        public DrDetail? DrDetail { get; set; }
    }
}
