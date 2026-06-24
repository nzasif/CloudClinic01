using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.CheckPatModels.PatDrugModels
{
    public class PatDrugUpdateModel
    {
        [Required]
        public Guid PatDrugId { get; set; }
        public string PatDrugName { get; set; }
        public string PatDrugTime { get; set; }
        public string PatDrugDosage { get; set; }
        public string PatDrugInstruction { get; set; }
        public int DrugGivenQuantity { get; set; }
        [Required]
        public Guid PatReportId { get; set; }
    }
}
