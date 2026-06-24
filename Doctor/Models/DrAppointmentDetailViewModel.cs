using CloudClinic.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models
{
    public class DrAppointmentDetailtViewModel
    {
        public Guid AppointId { get; set; }
        public string PatId { get; set; }
        public string PatName { get; set; }
        public string PatAddress { get; set; }
        public string  PatDOB { get; set; }
        public string PatGender { get; set; }
        public string PatPhoneNumber { get; set; }
        public string ProfilePicUri { get; set; }
        public string DrPracTimeName { get; set; }
        public string DrPracStartTime { get; set; }
        public string DrPracEndTime { get; set; }
        public int AppointSeqNo { get; set; }
        public string AppointStatus { get; set; }
        public string AppointDate { get; set; }
        public string AppointCreateDate { get; set; }
        public bool IsRejected { get; set; }
    }
}
