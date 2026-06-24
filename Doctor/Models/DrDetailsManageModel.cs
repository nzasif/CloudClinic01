using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models
{
    // as drDetail is created when dr create user account
    // so only update of details is required,
    // (not create the entity but update exiting one)
    public class DrDetailUpdateModel
    {
        public string DrQualification { get; set; }
        public string DrSpecialty { get; set; }
        public string DrExperience { get; set; }
        [Range(5, 30)]
        public int DrAvgCheckTime { get; set; }
        [Range(0, 5000)]
        public int DrFee { get; set; }
        public bool IsVisible { get; set; }
        public string DrDescription { get; set; }
    }

    public class DrDetailViewModel
    {
        public string DrQualification { get; set; }
        public string DrSpecialty { get; set; }
        public string DrExperience { get; set; }
        public int DrAvgCheckTime { get; set; }
        public int DrFee { get; set; }
        public bool IsVisible { get; set; }
        public bool IsVerified { get; set; }
        public string DrDescription { get; set; }
        public string UpdateDate { get; set; }
    }
    //////////////////////////////////////////////////////////
   
    public class DrPracTimeCreateModel
    {
        [Required]
        public string DrPracTimeName { get; set; }

        [DataType(DataType.Time)]
        public DateTime DrPracStartTime { get; set; }

        [DataType(DataType.Time)]
        public DateTime DrPracEndTime { get; set; }

        [Range(1, 300)]
        public int DrMaxAppointments { get; set; }
    }

    public class DrPracTimeUpdateModel
    {
        [Required]
        public Guid DrPracTimeId { get; set; }
        [Required]
        public string DrPracTimeName { get; set; }

        [DataType(DataType.Time)]
        public DateTime DrPracStartTime { get; set; }

        [DataType(DataType.Time)]
        public DateTime DrPracEndTime { get; set; }

        [Range(1, 300)]
        public int DrMaxAppointments { get; set; }
    }

    public class DrPracTimeViewModel
    {
        public string DrPracTimeId { get; set; }
        public string DrPracTimeName { get; set; }
        public string DrPracStartTime { get; set; }
        public string DrPracEndTime { get; set; }
        public int DrMaxAppointments { get; set; }
    }
    public class DrHolidayCreateModel
    {
        [Required]
        public string DrHolidayName { get; set; }
    }
    public class DrHolidayUpdateModel
    {
        [Required]
        public Guid DrHolidayId { get; set; }
        [Required]
        public string DrHolidayName { get; set; }
    }

    public class DrHolidayViewModel
    {
        public string DrHolidayId { get; set; }
        public string DrHolidayName { get; set; }
        public string EntryDate { get; set; }
    }
}
