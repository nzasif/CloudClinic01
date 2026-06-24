using CloudClinic.Data;
using CloudClinic.Patients.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Services
{
    public interface ISearchDrService
    {
        Task<DrsSearchResultsModel> SearchDrByName(CloudClinicDb db, string drName, string cityName, int top, int offset, HttpContext httContext);
        Task<DrsSearchResultsModel> SearchDrBySpecialty(CloudClinicDb db, string drSpecialty, string cityName, int top, int offset, HttpContext httContext);
        Task<PatDrDetailViewModel> GetDrDetails(CloudClinicDb db, Guid drId, HttpContext httpContext);
        Task<List<TopDrs>> GetTopDrs(CloudClinicDb db, HttpContext httpContext, int top);
     }
}
