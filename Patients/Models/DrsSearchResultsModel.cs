using CloudClinic.Account.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models
{
    public class DrsSearchResultsModel
    {
        public int TotalResultsFound { get; set; }
        public List<PatDrProfileViewModel> SearchedDrs { get; set; }
    }

    public class PatDrProfileViewModel
    {
        public string DrId { get; set; }
        public string DrName { get; set; }
        public string DrAddress { get; set; }
        public string DrGender { get; set; }
        public string DrPhoneNumber { get; set; }
        public string DrSpecialty { get; set; }
        public string DrProfilePicUri { get; set; }
    }
}
