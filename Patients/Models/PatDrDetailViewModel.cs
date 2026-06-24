using CloudClinic.Account.Models;
using CloudClinic.Doctor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models
{
    public class PatDrDetailViewModel
    {
        public string DrId { get; set; }
        public string DrName { get; set; }
        public string DrProfilePicUri { get; set; }
        public int DrAge { get; set; }
        public string DrGender { get; set; }
        public string DrEmail { get; set; }
        public string DrPhoneNumber { get; set; }
        public string DrSpecialty { get; set; }
        public string DrQualification { get; set; }
        public string DrExperience { get; set; }
        public int DrFee { get; set; }
        public string DrAddress { get; set; }
        public string DrDescription { get; set; }
        public List<string> DrHolidays { get; set; }
        public List<PatDrReviewViewModel> DrReviews { get; set; }
        public List<PatDrPracTimeViewModel> DrPracTimes { get; set; }
    }

    public class PatDrReviewViewModel
    {
        public string PatName { get; set; }
        public string PatAddress { get; set; }
        public string ReviewText { get; set; }
        public int Rating { get; set; }
        public string ReviewDate { get; set; }
    }

    public class PatDrPracTimeViewModel
    {
        public string DrPracTimeId { get; set; }
        public string DrPracTimeName { get; set; }
        public string DrPracStartTime { get; set; }
        public string DrPracEndTime { get; set; }
    }
}
