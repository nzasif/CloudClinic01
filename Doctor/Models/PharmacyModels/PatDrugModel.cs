using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.PharmacyModels
{
    public class Prescription
    {
        public Guid ReportId { get; set; }
        public string ReportDate { get; set; }
        public string PracTimeName { get; set; }
        public string PatName { get; set; }
        public string ProfilePicUri { get; set; }
        public string GuardianName { get; set; }
        public string Address { get; set; }
        public string PhoneNubmer { get; set; }
        public int DrugsPrice { get; set; }
        public List<PatDrugModel> PatDrugs { get; set; }
    }

    public class PatDrugModel
    {
        public Guid PatDrugId { get; set; }
        public string PatDrugName { get; set; }
        public string PatDrugTime { get; set; }
        public string PatDrugDosage { get; set; }
        public string PatDrugInstruction { get; set; }
        public int DrugGivenQuantity { get; set; }
    }
}
