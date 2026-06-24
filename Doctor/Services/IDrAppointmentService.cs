using CloudClinic.Data;
using CloudClinic.Doctor.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Services
{
    public interface IDrAppointmentService
    {
        public string ChangeAppointmentStatus(CloudClinicDb db, string drId, Guid appointId, string status);
        Task<List<DrAppointmentDetailtViewModel>> GetTodayAppointments(CloudClinicDb db, HttpContext httpContext, string drId, string drPracTimeName);
        Task<List<DrAppointmentDetailtViewModel>> DrSearchAppointments(CloudClinicDb db, HttpContext httpContext, string drId, DateTime AppointDate, string drPracTimeName);
        Task<DrAppointmentDetailtViewModel> GetDrAppointmentDetail(CloudClinicDb db, string drId, string appointId, HttpContext httpContext);
        Task<List<DrTotalAppointsViewModel>> GetTotalAppointments(CloudClinicDb db, string drId);
    }
}
