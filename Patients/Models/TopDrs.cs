using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models
{
    public class TopDrs
    {
        public Guid DrId { get; set; }
        public string DrName { get; set; }
        public string DrAddress { get; set; }
        public string DrPhoneNumber { get; set; }
        public string DrSpecailty { get; set; }
        public string ProfilePicUri { get; set; }
        public int SatisfactionRate { get; set; }
    }
}
