using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Doctor.Models;
using CloudClinic.Doctor.Services;
using CloudClinic.Shared;
using CloudClinic.Shared.AuthConstants;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudClinic.Doctor.Controllers
{
    [Route(Routes.DrRevewsControllerRoute)]
    [ApiController]
    [Authorize(Roles = Roles.DrRole)]
    public class DrReviewController : ControllerBase
    {
        private object response;

        private CloudClinicDb db { get; set; }
        private IDrDetailsProvider drDetailsProvider { get; set; }

        public DrReviewController(
            CloudClinicDb Db,
            IDrDetailsProvider DrDetailsProvider)
        {
            db = Db;
            drDetailsProvider = DrDetailsProvider;
        }

        [HttpGet(Routes.DrViewReviewsRoute, Name = Routes.DrViewReviewsRoute)]
        public async Task<IActionResult> DrViewReviews([FromQuery] int year, [FromQuery] int month)
        {
            if (year < 2020 || month > 12 || month < 1)
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "404", Message = "not valid year or month" }, null);
                return Ok(response);
            }

            string drId = drDetailsProvider.GetDrId(User);

            var q = (from r in db.Reviews
                     where r.DrId == new Guid(drId)
                     && r.ReviewDate.Date.Year == year
                     && r.ReviewDate.Month == month
                     && r.IsVisible == true
                     join p in db.AppUsers on r.PatId equals p.Id
                     select new DrReviewViewModel
                     {
                         ReviewId = r.ReviewId,
                         ReviewText = r.ReviewText,
                         Rating = r.Rating,
                         IsReported = r.IsReported,
                         ReviewDate = r.ReviewDate.ToShortDateString() + " " + r.ReviewDate.ToShortTimeString(),
                         PatName = p.FullName,
                         PatAddress = p.Street + ", " + p.City + ", " + p.Province,
                         PatProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, p.ProfilePicName)
                         //IsVisible = c.IsVisible // when invisible by admin, now review will be shown
                     });
            if (!(await q.AnyAsync()))
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "404", Message = $"not found {year}, {month}" }, null);
                return Ok(response);
            }

            response = HelperMethods.CreateResponse("404", null, new { reviews = await q.ToListAsync() });
            return Ok(response);

        }

        [HttpGet(Routes.ToggleReportReviewRoute, Name = Routes.ToggleReportReviewRoute)]
        public async Task<IActionResult> ToggleReportReview([FromQuery] string reviewId)
        {
            string drId = drDetailsProvider.GetDrId(User);

            Review review = db.Reviews.FirstOrDefault(c => c.ReviewId == new Guid(reviewId) && c.DrId == new Guid(drId));

            if (review is null)
            {
                response = HelperMethods.CreateResponse("404", new ErrorModel { Type = "404", Message = "not found" }, null);
                return Ok(response);
            }

            review.IsReported = !(review.IsReported);
            await db.SaveChangesAsync();

            response = HelperMethods.CreateResponse("404", null, new { isReported = review.IsReported });
            return Ok(response);
        }
    }
}