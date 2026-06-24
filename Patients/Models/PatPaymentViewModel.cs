using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models
{
    public class PatPaymentViewModel
    {
        public Guid PaymentId { get; set; }
        public int DrugsPrice { get; set; }
        public int LabFee { get; set; }
        public int XrayFee { get; set; }
        public int DrFee { get; set; }
        public int TotalAmount { get; set; }
        public string PaymentDate { get; set; }
        public bool Paid { get; set; }
    }
}
