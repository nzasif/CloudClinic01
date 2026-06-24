using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Data.DbSets
{
    public class PatDrug
    {
        [Key]
        public Guid PatDrugId { get; set; }
        [Required]
        public string? PatDrugName { get; set; } // name also include type, weight
        public string? PatDrugTime { get; set; }
        public string? PatDrugDosage { get; set; }
        public string? PatDrugInstruction { get; set; }
        public int DrugGivenQuantity { get; set; }
        public Guid PatReportId { get; set; }
        public PatReport? PatReport { get; set; }
    }
}
