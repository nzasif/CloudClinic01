using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Patients.Models.PatReportViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Services
{
    public interface IPatReportManageService
    {
        PatReportViewModel PatViewReport(CloudClinicDb db, string userId, Guid reportId, HttpContext httpContext);
        List<PatReportShortViewModel> PatGetAllReports(CloudClinicDb db, string userId, HttpContext httpContext);
        string PatDeleteReport(CloudClinicDb db, string userId, Guid reportId);
        string PatRecoverReport(CloudClinicDb db, string userId, string reportId);
        string TogglePatReportVisibility(CloudClinicDb db, string userId, Guid reportId);
    }
}
