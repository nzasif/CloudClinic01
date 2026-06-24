using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using CloudClinic.Pages.Admin.Models;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CloudClinic.Pages.Admin.Manage
{
    [Authorize(Roles = "ADMIN", AuthenticationSchemes = "Identity.Application")]
    public class ViewPatsModel : PageModel
    {
        private CloudClinicDb db;
        private UserManager<AppUser> userManager;

        [BindProperty(SupportsGet = true)]
        public int Year { get; set; }
        [BindProperty(SupportsGet = true)]
        public int Month { get; set; }
        public List<PatView> Pats { get; set; }

        public ViewPatsModel(
            CloudClinicDb Db,
            UserManager<AppUser> manager)
        {
            db = Db;
            userManager = manager;
        }

        public void OnGet()
        {
            SetDrList();
        }

        private void SetDrList()
        {
            if (Year == 0 || Month == 0)
            {
                Year = DateTime.Now.Year;
                Month = DateTime.Now.Month;
            }

            var q = (from userRoles in db.UserRoles
                     join roles in db.Roles on userRoles.RoleId equals roles.Id
                     where roles.Name == "NORMALUSER"
                     join u in db.AppUsers on userRoles.UserId equals u.Id
                     where u.RegDate.Year == Year && u.RegDate.Month == Month
                     select new PatView
                     {
                         PatId = u.Id,
                         FullName = u.FullName,
                         GuardianName = u.GuardianName,
                         Address = u.City + ", " + u.Province,
                         Gender = u.Gender,
                         PhoneNumber = u.PhoneNumber,
                         Email = u.Email,
                         RegDate = u.RegDate.ToShortDateString(),
                         ProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, u.ProfilePicName),
                         IsRemoved = u.IsRemoved,
                         DOB = u.DOB.ToShortDateString()
                     });

            if (!q.Any())
            {
                return;
            }

            Pats = q.ToList();
        }
    }
}
