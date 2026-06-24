using CloudClinic.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models
{
    public class DrAppointmentShortViewModel
    {
        public string AppointId { get; set; }
        public string DrPracTimeName { get; set; }
        public string PatName { get; set; }
        public string PatPhoneNumber { get; set; }
        public string PatId { get; set; }
        public int AppointSeqNo { get; set; }
        public string AppointStatus { get; set; }
        public string AppointCreateDate { get; set; }
        public string AppointDate { get; set; }
    }
}
