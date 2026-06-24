using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Models.PatReportViewModels
{
    public class PatReportViewModel
    {
        public Guid ReportId { get; set; }
        public string PatName { get; set; }
        public string PatGender { get; set; }
        public string PatDOB { get; set; }
        public string DrFullName { get; set; }
        public string DrProfilePicUri { get; set; }
        public string DrQualification { get; set; }
        public string DrSpecialty { get; set; }
        public string DrAddress { get; set; }
        public string Symptoms { get; set; }
        public string Diagnosis { get; set; }
        public string Remarks { get; set; }
        public string ReportDate { get; set; }
        public bool IsVisible { get; set; }

        public List<PatDrugViewModel> PatDrugs { get; set; }
        public List<PatLabTestViewModel> PatLabTests { get; set; }
        public List<PatXrayViewModel> PatXrays { get; set; }
    }
}
