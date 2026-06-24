using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.AccountantModels
{
    public class PaymentViewModel
    {
        public string PatName { get; set; }
        public string GuardianName { get; set; }
        public string Address { get; set; }
        public string PhoneNubmer { get; set; }
        public string ProfilePicUri { get; set; }

        public Guid PaymentId { get; set; }
        public int DrugsPrice { get; set; }
        public int LabFee { get; set; }
        public int XrayFee { get; set; }
        public int DrugsExpenditure { get; set; }
        public int LabExpenditure { get; set; }
        public int DrFee { get; set; }
        public int TotalAmount { get; set; }
        public string PaymentDate { get; set; }
        public string DrPracTimeName { get; set; }
        public bool Paid { get; set; }
    }
}
