using CloudClinic.Doctor.Models.CheckPatModels.PatDrugModels;
using CloudClinic.Doctor.Models.CheckPatModels.PatLabTestModels;
using CloudClinic.Doctor.Models.CheckPatModels.PatXrayModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.CheckPatModels.ReportModels
{
    public class ReportViewModel
    {
        public Guid PatReportId { get; set; }
        public string Symptoms { get; set; }
        public string Diagnosis { get; set; }
        public string Remarks { get; set; }
        public Guid DrId { get; set; }
        public string DrName { get; set; }
        public string DrProfilePicUri { get; set; }
        public string DrSpecialty { get; set; }
        public string DrAddress { get; set; }
        public string PatId { get; set; }
        public string PatName { get; set; }
        public string PatAddress { get; set; }
        public string PatDOB { get; set; }
        public string PatGender { get; set; }
        public string ReportDate { get; set; }
        public Guid AppointId { get; set; }

        public List<PatDrugViewModel> PatDrugs { get; set; }
        public List<LabTestViewModel> PatLabTests { get; set; }
        public List<XrayViewModel> PatXrayView{ get; set; }
    }
}
