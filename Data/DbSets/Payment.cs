using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Data.DbSets
{
    public class Payment
    {
        [Key]
        public Guid PaymentId { get; set; }
        public int DrugsPrice { get; set; }
        public int DrugsExpenditure { get; set; }
        public int LabFee { get; set; }
        public int XrayFee { get; set; }
        public int DrFee { get; set; }
        public bool Paid { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
