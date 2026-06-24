using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Patients.Models.PatReportViewModels;
using CloudClinic.Shared;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Services
{
    public class PatReportManageService : IPatReportManageService
    {
        public string PatDeleteReport(CloudClinicDb db, string userId, Guid reportId)
        {
            var r = db.PatReports.FirstOrDefault(r => r.PatReportId == reportId && r.PatId == userId);

            if(r != null)
            {
                r.IsDeleted = true;
                // also change the visibility to false
                r.IsVisible = false;
                db.SaveChanges();

                return "ok";
            }

            return null;
        }

        public string PatRecoverReport(CloudClinicDb db, string userId, string reportId)
        {
            var r = db.PatReports.FirstOrDefault(r => r.PatReportId == new Guid(reportId) && r.PatId == userId);

            if (r != null)
            {
                r.IsDeleted = false;
                r.IsVisible = true;
                db.SaveChanges();

                return "ok";
            }

            return null;
        }

        // may visited drs (dr search conroller) used instead of that
        public List<PatReportShortViewModel> PatGetAllReports(CloudClinicDb db, string userId, HttpContext httpContext)
        {
            var q = (from patReport in db.PatReports
                     where patReport.PatId == userId
                     join drs in db.DrDetails on patReport.DrId equals drs.DrId
                     join dru in db.AppUsers on drs.UserId equals dru.Id
                     select new PatReportShortViewModel
                     {
                         ReportId = patReport.PatReportId.ToString(),
                         DrName = dru.FullName,
                         DrProfilePicUri = HelperMethods.GenerateProfilePicUri(httpContext, dru.ProfilePicName),
                         DrSpecialty = drs.DrSpecialty,
                         ReportDate = patReport.ReportDate
                     });

            if (!q.Any())
            {
                return null;
            }

            return q.ToList();
        }

        public string TogglePatReportVisibility(CloudClinicDb db, string userId, Guid reportId)
        {
            var r = db.PatReports.FirstOrDefault(r => r.PatId == userId && r.PatReportId == reportId);

            if (r is null)
            {
                return null;
            }

            r.IsVisible = !r.IsVisible;
            db.SaveChanges();

            return r.IsVisible.ToString();
        }

        public PatReportViewModel PatViewReport(CloudClinicDb db, string userId, Guid reportId, HttpContext httpContext)
        {
            var t = db.Database.BeginTransaction();

            try
            {
                PatReportViewModel report;
                var reportQuery = (from patReport in db.PatReports
                         where patReport.PatId == userId
                         && patReport.PatReportId == reportId
                         join drs in db.DrDetails on patReport.DrId equals drs.DrId
                         join dru in db.AppUsers on drs.UserId equals dru.Id
                         join patU in db.AppUsers on patReport.PatId equals patU.Id
                         select new PatReportViewModel
                         {
                             ReportId = reportId,
                             DrFullName = dru.FullName,
                             DrProfilePicUri = HelperMethods.GenerateProfilePicUri(httpContext, dru.ProfilePicName),
                             DrSpecialty = drs.DrSpecialty,
                             DrQualification = drs.DrQualification,
                             DrAddress = dru.Street + ", " + dru.City + ", " + dru.Province,
                             PatName = patU.FullName,
                             PatDOB = patU.DOB.ToShortDateString(),
                             PatGender = patU.Gender,
                             Symptoms = patReport.Symptoms,
                             Diagnosis = patReport.Diagnosis,
                             Remarks = patReport.Remarks,
                             ReportDate = patReport.ReportDate.ToShortDateString()+" "+patReport.ReportDate.ToShortTimeString(),
                             IsVisible = patReport.IsVisible
                         });

                var patLabTestsQuery = (from test in db.PatLabTests
                                        where test.PatReportId == reportId
                                        select new PatLabTestViewModel
                                        {
                                            LabTestName = test.LabTestName,
                                            LabTestResult = test.LabTestResult,
                                            AttachmentUri = HelperMethods.GenerateAttachmentUri(httpContext, test.AttachmentName)
                                        });

                var patXraysQuery = (from x in db.PatXrays
                                     where x.PatReportId == reportId
                                     select new PatXrayViewModel
                                     {
                                         XrayName = x.XrayName,
                                         XrayResult = x.XrayResult,
                                         AttachmentUri = HelperMethods.GenerateAttachmentUri(httpContext, x.AttachmentName)
                                     });

                var patDrugsQuery = (from pdrugs in db.PatDrugs
                                     where pdrugs.PatReportId == reportId
                                     select new PatDrugViewModel
                                     {
                                         DrugName = pdrugs.PatDrugName,
                                         DrugGivenQuantity = pdrugs.DrugGivenQuantity,
                                         PatDrugDosage = pdrugs.PatDrugDosage,
                                         PatDrugInstruction = pdrugs.PatDrugInstruction,
                                         PatDrugTime = pdrugs.PatDrugTime
                                     });

                t.Commit();

                if (!reportQuery.Any())
                {
                    return null;
                }

                report = reportQuery.First();

                if (patDrugsQuery.Any())
                {
                    report.PatDrugs = patDrugsQuery.ToList();
                }

                if (patLabTestsQuery.Any())
                {
                    report.PatLabTests = patLabTestsQuery.ToList();
                }

                if (patXraysQuery.Any())
                {
                    report.PatXrays = patXraysQuery.ToList();
                }

                return report;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
