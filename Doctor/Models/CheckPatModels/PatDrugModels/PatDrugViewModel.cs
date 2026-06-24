using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.CheckPatModels.PatDrugModels
{
    public class PatDrugViewModel
    {
        public Guid PatDrugId { get; set; }
        public string PatDrugName { get; set; }
        public string PatDrugTime { get; set; }
        public string PatDrugDosage { get; set; }
        public string PatDrugInstruction { get; set; }
        public int DrugGivenQuantity { get; set; }
        public Guid PatReportId { get; set; }
    }
}
