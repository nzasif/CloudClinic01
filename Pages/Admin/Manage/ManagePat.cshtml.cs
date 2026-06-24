using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Pages.Admin.Models;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CloudClinic.Admin.Pages.Manage
{
    [Authorize(Roles = "ADMIN", AuthenticationSchemes = "Identity.Application")]
    public class ManagePat : PageModel
    {
        private CloudClinicDb db { get; set; }

        private readonly UserManager<AppUser> userManager;
        public PatView Pat { get; set; }

        [BindProperty]
        public string PatId { get; set; }
        [BindProperty]
        public string Cause { get; set; }


        public string TestMessage { get; set; }
        public ManagePat(
            CloudClinicDb Db,
            UserManager<AppUser> manager)
        {
            db = Db;
            userManager = manager;
        }

        public void OnGetAsync(string patId)
        {
            SetPatDetail(patId);           
        }

        public void OnPostRemovePat()
        {
            var user = db.AppUsers.FirstOrDefault(u => u.Id == PatId);
            
            if (user is null)
            {
                ViewData["error"] = "Dr is not found";
                return;
            }

            user.IsRemoved = true;
            var removedUser = db.RemovedUsers.Add(new RemovedUser
            {
                UserId = user.Id,
                RemovalCause = Cause,
                RemovalDate = DateTime.Now
            });

            db.SaveChanges();

            SetPatDetail(PatId);
        }

        public void OnGetUnRemovePat(string patId)
        {
            var u = db.AppUsers.FirstOrDefault(u => u.Id == patId);

            if (u is null)
            {
                ViewData["error"] = "Dr is not found";
                return;
            }

            u.IsRemoved = false;
            var removedUser = db.RemovedUsers.FirstOrDefault(u => u.UserId == patId);

            if (removedUser != null)
            {
                db.RemovedUsers.Remove(removedUser);
            }

            db.SaveChanges();

            SetPatDetail(patId);
        }

        private void SetPatDetail(string patId)
        {
            var q = (from u in db.AppUsers where u.Id == patId
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

            Pat = q.FirstOrDefault();

            if (Pat.IsRemoved)
            {
                var removedUser = db.RemovedUsers.FirstOrDefault(u => u.UserId == patId);

                if (removedUser != null)
                {
                    Pat.RemovalCause = removedUser.RemovalCause;
                }

            }
        }
    }
}
