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
    public class ViewReportedReviewsModel : PageModel
    {
        private CloudClinicDb db;
        private UserManager<AppUser> userManager;

        [BindProperty(SupportsGet = true)]
        public int Year { get; set; }
        [BindProperty(SupportsGet = true)]
        public int Month { get; set; }
        public List<ReportedReview> ReportedReviews { get; set; }

        public ViewReportedReviewsModel(
            CloudClinicDb Db,
            UserManager<AppUser> manager)
        {
            db = Db;
            userManager = manager;
        }

        public void OnGet()
        {
            SetReportedReviewsList();
        }

        public void OnGetToggleVisibility(Guid reviewId)
        {
            var r = db.Reviews.FirstOrDefault(rev => rev.ReviewId == reviewId);

            if (r != null)
            {
                r.IsVisible = !r.IsVisible;
                // set y and M for bring the same review List
                Year = r.ReviewDate.Year;
                Month = r.ReviewDate.Month;

                db.SaveChanges();
            }

            SetReportedReviewsList();
        }

        private void SetReportedReviewsList()
        {
            if (Year == 0 || Month == 0)
            {
                Year = DateTime.Now.Year;
                Month = DateTime.Now.Month;
            }

            var q = (from r in db.Reviews where r.ReviewDate.Month == Month && r.ReviewDate.Year == Year
                     join u in db.AppUsers on r.PatId equals u.Id
                     select new ReportedReview
                     {
                         PatId = u.Id,
                         FullName = u.FullName,
                         Address = u.City + ", " + u.Province,
                         PhoneNumber = u.PhoneNumber,
                         ProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, u.ProfilePicName),
                         
                         ReviewId = r.ReviewId,
                         ReviewText = r.ReviewText,
                         ReviewDate = r.ReviewDate.ToShortDateString(),
                         IsVisible = r.IsVisible
                     });

            if (!q.Any())
            {
                return;
            }

            ReportedReviews = q.ToList();
        }
    }
}
