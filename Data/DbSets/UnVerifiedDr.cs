using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Data.DbSets
{
    public class UnVerifiedDr
    {
        public Guid UnVerifiedDrId { get; set; }
        public string? UnVerificationCause { get; set; }
        public DateTime UnVerificationDate { get; set; }
        public Guid DrId { get; set; }
        public DrDetail? DrDetail { get; set; }
    }
}
