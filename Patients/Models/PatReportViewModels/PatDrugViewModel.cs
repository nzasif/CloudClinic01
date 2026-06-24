using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models.PatReportViewModels
{
    public class PatDrugViewModel
    {
        public string DrugName { get; set; }
        public string PatDrugTime { get; set; }
        public string PatDrugDosage { get; set; }
        public string PatDrugInstruction { get; set; }
        public int DrugGivenQuantity { get; set; }
        public string DrugFormula { get; set; }
        public DateTime DrugExpiryDate { get; set; }
    }
}
