using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models
{
    public class TrackAppointmentModel
    {
        public Guid AppointId { get; set; }
        public string PatName { get; set; }
        public string Status { get; set; }
    }
}
