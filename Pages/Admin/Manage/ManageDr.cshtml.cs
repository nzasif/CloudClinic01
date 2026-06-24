using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Admin.Models;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CloudClinic.Admin.Pages.Manage
{
    [Authorize(Roles = "ADMIN", AuthenticationSchemes = "Identity.Application")]
    public class ManageDr : PageModel
    {
        private CloudClinicDb db { get; set; }

        private readonly UserManager<AppUser> userManager;
        public DrDetailView Dr { get; set; }

        [BindProperty]
        public Guid DrId { get; set; }
        [BindProperty]
        public string Cause { get; set; }


        public string TestMessage { get; set; }
        public ManageDr(
            CloudClinicDb Db,
            UserManager<AppUser> manager)
        {
            db = Db;
            userManager = manager;
        }

        public void OnGetAsync(Guid drId)
        {
            SetDrDetail(drId);           
        }

        public void OnPostUnVerifyDr()
        {
            var dr = db.DrDetails.FirstOrDefault(dr => dr.DrId == DrId);
            
            if (dr is null)
            {
                ViewData["error"] = "Dr is not found";
                return;
            }

            dr.IsVerified = false;
            var unVerifiedDrs = db.UnVerifiedDrs.Add(new UnVerifiedDr
            {
                DrId = dr.DrId,
                UnVerificationCause = Cause,
                UnVerificationDate = DateTime.Now
            });

            db.SaveChanges();

            SetDrDetail(DrId);
        }

        public void OnGetVerifyDr(Guid drId)
        {
            var dr = db.DrDetails.FirstOrDefault(dr => dr.DrId == drId);

            if (dr is null)
            {
                ViewData["error"] = "Dr is not found";
                return;
            }

            dr.IsVerified = true;
            var unVerifiedDr = db.UnVerifiedDrs.FirstOrDefault(d => d.DrId == drId);

            if (unVerifiedDr != null)
            {
                db.UnVerifiedDrs.Remove(unVerifiedDr);
            }

            db.SaveChanges();

            SetDrDetail(drId);
        }

        private void SetDrDetail(Guid drId)
        {
            var q = (from drs in db.DrDetails where drs.DrId == drId
                     join dru in db.AppUsers on drs.UserId equals dru.Id
                     select new DrDetailView
                     {
                         DrId = drs.DrId,
                         DrName = dru.FullName,
                         DrEmail = dru.Email,
                         DrAddress = dru.City + ", " + dru.Province,
                         DrGender = dru.Gender,
                         DrPhoneNumber = dru.PhoneNumber,
                         DrSpecialty = drs.DrSpecialty,
                         DrUserId = dru.Id,
                         IsVerified = drs.IsVerified,
                         RegisterdDate = dru.RegDate.ToShortDateString(),
                         DrProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, dru.ProfilePicName),

                         Reviews = (from r in db.Reviews where r.DrId == DrId
                                    join patU in db.AppUsers on r.PatId equals patU.Id
                                    select new DrReview
                                    {
                                        ReviewText = r.ReviewText,
                                        date = r.ReviewDate.ToShortDateString(),
                                        Rating = r.Rating,
                                        PatName = patU.FullName,
                                        PatAddress = $"{patU.City}, {patU.Province}",
                                        PatPhoneNumber = patU.PhoneNumber,
                                        PatProfilePic = HelperMethods.GenerateProfilePicUri(HttpContext, patU.ProfilePicName)
                                    }).ToList()
                     });

            Dr = q.FirstOrDefault();

            if (!Dr.IsVerified)
            {
                var unVerifiedDr = db.UnVerifiedDrs.FirstOrDefault(d => d.DrId == drId);

                if (unVerifiedDr != null)
                {
                    Dr.UnVerificationCause = unVerifiedDr.UnVerificationCause;
                }
            }
        }
    }
}
