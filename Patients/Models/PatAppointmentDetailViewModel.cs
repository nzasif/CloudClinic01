using CloudClinic.Data;
using CloudClinic.Doctor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models
{
    public class PatAppointmentDetailViewModel
    {
        public Guid AppointId { get; set; }
        public Guid DrId { get; set; }
        public string DrProfilePicUri { get; set; }
        public string DrName { get; set; }
        public string DrSpecialty { get; set; }
        public string DrAddress { get; set; }
        public string DrPhoneNumber { get; set; }
        public int DrAvgCheckTime { get; set; }
        public DateTime AppointDate { get; set; }
        public string AppointCreateDate { get; set; }
        public string DrPracTimeName { get; set; }
        public string DrPracStartTime { get; set; }
        public string DrPracEndTime { get; set; }
        public int AppointSeqNo { get; set; }
        public string AppointStatus { get; set; }
        public string AppointRemainingTime { get; set; }
    }
}
