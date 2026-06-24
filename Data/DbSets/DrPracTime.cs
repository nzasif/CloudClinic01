using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Data.DbSets
{
    public class DrPracTime
    {
        [Key]
        public Guid DrPracTimeId { get; set; }
        public string? DrPracTimeName { get; set; }

        [DataType(DataType.Time)]
        public DateTime DrPracStartTime { get; set; }

        [DataType(DataType.Time)]
        public DateTime DrPracEndTime { get; set; }
        public int DrMaxAppointments { get; set; }    
        public Guid DrId { get; set; }
        public DrDetail? DrDetail { get; set; }
    }
}