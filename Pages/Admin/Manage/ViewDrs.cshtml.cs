using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Admin.Models;
using CloudClinic.Data;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CloudClinic.Pages.Admin.Manage
{
    [Authorize(Roles = "ADMIN", AuthenticationSchemes = "Identity.Application")]
    public class ViewDrsModel : PageModel
    {
        private CloudClinicDb db;
        private UserManager<AppUser> userManager;

        [BindProperty(SupportsGet = true)]
        public int Year { get; set; }
        [BindProperty(SupportsGet = true)]
        public int Month { get; set; }
        public List<DrsList> Drs { get; set; }

        public ViewDrsModel(
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
                     where roles.Name == "DR"
                     join u in db.AppUsers on userRoles.UserId equals u.Id
                     where u.RegDate.Year == Year && u.RegDate.Month == Month
                     join drs in db.DrDetails on u.Id equals drs.UserId
                     select new DrsList
                     {
                         DrId = drs.DrId,
                         DrName = u.FullName,
                         DrAddress = u.City + ", " + u.Province,
                         DrGender = u.Gender,
                         DrPhoneNumber = u.PhoneNumber,
                         DrEmail = u.Email,
                         DrSpecialty = drs.DrSpecialty,
                         DrUserId = u.Id,
                         IsVerified = drs.IsVerified,
                         RegisterdDate = u.RegDate.ToShortDateString(),
                         DrProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, u.ProfilePicName),

                         DrPracTimes = (from times in db.DrPracTimes
                                        where times.DrId == drs.DrId
                                        select times.DrPracTimeName).ToList()
                     });

            if (!q.Any())
            {
                return;
            }

            Drs = q.ToList();
        }
    }
}
