using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Data.DbSets
{
    public class Drug
    {
        [Key]
        public Guid DrugId { get; set; }
        public string? DrugName { get; set; }
        public DateTime DrugExpiryDate { get; set; }
        public int DrugPurchasePrice { get; set; }
        public int DrugSalePrice { get; set; }
        public string? DrugFormula { get; set; }
        public string? DrugType { get; set; }
        public string? DrugCompany{ get; set; }
        public int DrugAvailQuantity { get; set; }
        public string? DrugWeight { get; set; }
        public bool IsDeleted {get; set;}
        public Guid DrId { get; set; }
        public DrDetail? DrDetail { get; set; }
    }
}
