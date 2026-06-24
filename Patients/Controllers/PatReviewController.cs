using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Patients.Models.PatReviewModels;
using CloudClinic.Patients.Services;
using CloudClinic.Shared;
using CloudClinic.Shared.AuthConstants;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudClinic.Patients.Controllers
{
    [Authorize(Roles = Roles.NormalUserRole)]
    [ApiController]
    [Route(Routes.PatReviewControllerRoute)]
    public class PatReviewController : ControllerBase
    {
        private object response;

        private CloudClinicDb db { get; set; }
        private IUserProfileProvider userInfoProvider { get; set; }
        public PatReviewController(
            CloudClinicDb Db,
            IUserProfileProvider UserInfoProvider)
        {
            db = Db;
            userInfoProvider = UserInfoProvider;
        }

        [HttpGet(Routes.PatGetAllReviewsRoute, Name = Routes.PatGetAllReviewsRoute)]
        public async Task<IActionResult> ViewAllReviews()
        {
            string patId = userInfoProvider.GetUserId(User);

            var q = (from reports in db.PatReports
                     where reports.PatId == patId
                     join drs in db.DrDetails on reports.DrId equals drs.DrId
                     join dru in db.AppUsers on drs.UserId equals dru.Id
                     select new PatReviewViewModel
                     {
                         DrName = dru.FullName,
                         DrId = drs.DrId,
                         DrProfilePicUrl = HelperMethods.GenerateProfilePicUri(HttpContext, dru.ProfilePicName),
                         PatReview = (from reviews in db.Reviews where reviews.DrId == drs.DrId && reviews.PatId == patId
                                      select new PatReviewModel
                                      {
                                          DrId = reviews.DrId,
                                          ReviewId = reviews.ReviewId.ToString(),
                                          ReviewText = reviews.ReviewText,
                                          Rating = reviews.Rating,
                                          ReviewDate = reviews.ReviewDate.ToShortDateString()
                                      }).FirstOrDefault()
                     });

            if (!q.Any())
            {
                response = HelperMethods.CreateResponse("200", new ErrorModel { Type = "notfound", Message = "Notfound" }, null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { allReviews = q.ToList() });

            return Ok(response);
        }

        [HttpGet(Routes.RemovePatReviewRoute, Name = Routes.RemovePatReviewRoute)]
        public async Task<IActionResult> RemoveReview([FromQuery] Guid reviewId)
        {
            string patId = userInfoProvider.GetUserId(User);

            Review review = db.Reviews.FirstOrDefault(c => c.ReviewId == reviewId);

            if (review == null)
            {
                response = HelperMethods.CreateResponse("200", new ErrorModel { Type = "notfound", Message = "Notfound" }, null );

                return Ok(response);
            }

            db.Reviews.Remove(review);
            await db.SaveChangesAsync();

            response = HelperMethods.CreateResponse("200", null, new { msg = "removed" });

            return Ok(response);
        }

        [HttpPost(Routes.AddOrUpdatePatReviewRoute, Name = Routes.AddOrUpdatePatReviewRoute)]
        public async Task<IActionResult> AddOrUpdateReview([FromBody] PatReviewModel model)
        {
            string patId = userInfoProvider.GetUserId(User);

            Review review = null;

            if (!string.IsNullOrEmpty(model.ReviewId))
            {
              review = db.Reviews.FirstOrDefault(c => c.ReviewId == new Guid(model.ReviewId) && c.PatId == patId);
            }

            if (review is null)
            {
                review = new Review
                {
                    DrId = model.DrId,
                    PatId = patId,
                    ReviewText = model.ReviewText,
                    Rating = model.Rating,
                    ReviewDate = DateTime.Now,
                    IsReported = false,
                    IsVisible = true
                };

                db.Reviews.Add(review);
                db.SaveChanges();

                response = HelperMethods.CreateResponse("200", null, new { reviewId = review.ReviewId, reviewDate = review.ReviewDate.ToShortDateString() });

                return Ok(response);
            }

            review.ReviewText = model.ReviewText;
            review.Rating = model.Rating;
            review.ReviewDate = DateTime.Now.Date;

            await db.SaveChangesAsync();

            response = HelperMethods.CreateResponse("200", null, new { reviewId = review.ReviewId, reviewDate = review.ReviewDate.ToShortDateString() });

            return Ok(response);
        }
    }
}