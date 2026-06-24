using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Patients.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Services
{
    public interface IPatAppointmentService
    {
        Task<string> CreateAppointmentAsync(CloudClinicDb db, AppointmentCreateModel model, string userId);
        Task<string> IsAppointAvailableAsync(CloudClinicDb db, Guid drId, DateTime appointDate, string drPracTime);
        Task<List<PatAppointmentDetailViewModel>> ViewAllAppointmentsAsync(CloudClinicDb db, string patId, HttpContext httpContext);
    }
}