using CloudClinic.Data;
using CloudClinic.Patients.Models;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Patients.Services
{
    public class SearchDrService: ISearchDrService
    {
        public async Task<DrsSearchResultsModel> SearchDrByName(CloudClinicDb db, string drName, string cityName, int top, int offset, HttpContext httContext)
        {
            // drName is in the AppUser table
            var searchDrProfilesQuery = (from drs in db.DrDetails
                                         join dru in db.AppUsers on drs.UserId equals dru.Id
                                         where dru.City == cityName && dru.FullName.Contains(drName)
                                         && dru.IsRemoved == false
                                         && drs.IsVerified == true
                                         && drs.IsVisible == true
                                         select new PatDrProfileViewModel
                                         {
                                             DrId = drs.DrId.ToString(),
                                             DrName = dru.FullName,
                                             DrSpecialty = drs.DrSpecialty,
                                             DrGender = dru.Gender,
                                             DrAddress = dru.Street + ", " + dru.City + ", " + dru.Province,
                                             DrPhoneNumber = dru.PhoneNumber,
                                             DrProfilePicUri = HelperMethods.GenerateProfilePicUri(
                                                 httContext, dru.ProfilePicName)
                                         });
            if (!searchDrProfilesQuery.Any())
            {
                return null;
            }

            var searchResultsModel = new DrsSearchResultsModel
            {
                TotalResultsFound = searchDrProfilesQuery.Count(),
                SearchedDrs = (searchDrProfilesQuery.Count() > offset + 10) ? searchDrProfilesQuery.Skip(offset).Take(top).ToList() : searchDrProfilesQuery.ToList()
            };

            return searchResultsModel;
        }

        public async Task<DrsSearchResultsModel> SearchDrBySpecialty(CloudClinicDb db, string drSpecialty, string cityName, int top, int offset, HttpContext httContext)
        {
            // drName is in the AppUser table
            // here by Address we mean City
            var searchDrProfilesQuery = (from drs in db.DrDetails
                                         join dru in db.AppUsers on drs.UserId equals dru.Id
                                         where dru.City == cityName && drs.DrSpecialty.Contains(drSpecialty)
                                         && dru.IsRemoved == false
                                         && drs.IsVerified == true
                                         && drs.IsVisible == true // when IsRemoved = true, he will be invisible
                                         select new PatDrProfileViewModel
                                         {
                                             DrId = drs.DrId.ToString(),
                                             DrName = dru.FullName,
                                             DrSpecialty = drs.DrSpecialty,
                                             DrGender = dru.Gender,
                                             DrAddress = dru.Street + ", " + dru.City + ", " + dru.Province,
                                             DrPhoneNumber = dru.PhoneNumber,
                                             DrProfilePicUri = HelperMethods.GenerateProfilePicUri(httContext, dru.ProfilePicName)
                                         });

            if (!searchDrProfilesQuery.Any())
            {
                return null;
            }

            var searchResultsModel = new DrsSearchResultsModel
            {
                TotalResultsFound = searchDrProfilesQuery.Count(),
                SearchedDrs = (searchDrProfilesQuery.Count() > offset + 10) ? searchDrProfilesQuery.Skip(offset).Take(top).ToList() : searchDrProfilesQuery.ToList()
            };

            return searchResultsModel;
        }

        // Search and get the details of the specified doctor
        // e.g. when user click on 'view details' button in a search list of drs
        // then the individual drDetails will come from here
        public async Task<PatDrDetailViewModel> GetDrDetails(CloudClinicDb db, Guid drId, HttpContext httpContext)
        {
            // drName is in the AppUser table
            PatDrDetailViewModel drDetails = (from drs in db.DrDetails where drs.DrId == drId
                                              && drs.IsVerified == true && drs.IsVisible == true
                                              join dru in db.AppUsers on drs.UserId equals dru.Id
                                              where dru.IsRemoved == false
                                              select new PatDrDetailViewModel
                                              {
                                                 DrId = drs.DrId.ToString(),
                                                 DrName = dru.FullName,
                                                 DrProfilePicUri = HelperMethods.GenerateProfilePicUri(httpContext, dru.ProfilePicName),
                                                 DrAge =   DateTime.Now.Date.Year - dru.DOB.Date.Year,
                                                 DrSpecialty = drs.DrSpecialty,
                                                 DrGender = dru.Gender,
                                                 DrEmail = dru.Email,
                                                 DrExperience = drs.DrExperience,
                                                 DrQualification = drs.DrQualification,
                                                 DrFee = drs.DrFee,
                                                 DrAddress = dru.Street + ", " + dru.City + ", " + dru.Province,
                                                 DrPhoneNumber = dru.PhoneNumber,
                                                 DrDescription = drs.DrDescription
                                             }).FirstOrDefault();

            if (drDetails is null)
            {
                return null;
            }

            var drHollidaysQuery = (from hollidays in db.DrHolidays
                                    where hollidays.DrId == drId
                                    select hollidays.DrHolidayName);

            var patDrCommentsQuery = (from c in db.Reviews
                                      where c.DrId == drId
                                      join patU in db.AppUsers on c.PatId equals patU.Id
                                      select new PatDrReviewViewModel
                                      {
                                          PatName = patU.FullName,
                                          PatAddress = patU.City + " " + patU.Province,
                                          ReviewText = c.ReviewText,
                                          Rating = c.Rating,
                                          ReviewDate = c.ReviewDate.ToShortDateString()
                                      });
            var patDrPracTimesQuery = (from t in db.DrPracTimes
                                       where t.DrId == drId
                                       select new PatDrPracTimeViewModel
                                       {
                                           DrPracTimeName = t.DrPracTimeName,
                                           DrPracStartTime = t.DrPracStartTime.ToShortTimeString(),
                                           DrPracEndTime = t.DrPracEndTime.ToShortTimeString(),
                                           DrPracTimeId = t.DrPracTimeId.ToString()
                                       });

            // now add extra lists to the drDetails
            if (drHollidaysQuery.Any())
            {
                drDetails.DrHolidays = drHollidaysQuery.ToList();
            }

            if (patDrCommentsQuery.Any())
            {
                drDetails.DrReviews = patDrCommentsQuery.ToList();
            }

            if (patDrPracTimesQuery.Any())
            {
                drDetails.DrPracTimes = patDrPracTimesQuery.ToList();
            }

            return drDetails;
        }

        // top ten based on total visits and rating in the last month
        public async Task<List<TopDrs>> GetTopDrs(CloudClinicDb db, HttpContext httpContext, int top)
        {
            var q = (from drs in db.DrDetails where drs.IsVerified == true
                     join drU in db.AppUsers on drs.UserId equals drU.Id
                     join rev in db.Reviews on drs.DrId equals rev.DrId
                     where rev.ReviewDate.Month == DateTime.Now.Month - 1
                     orderby rev.Rating descending
                     select new TopDrs
                     {
                         DrId = drs.DrId,
                         DrName = drU.FullName,
                         DrAddress = $"{drU.City}, {drU.Province}",
                         DrSpecailty = drs.DrSpecialty,
                         DrPhoneNumber = drU.PhoneNumber,
                         ProfilePicUri = HelperMethods.GenerateProfilePicUri(httpContext, drU.ProfilePicName),
                         SatisfactionRate = CalculateSatisfactionRate(
                             (from r in db.Reviews
                              where r.DrId == drs.DrId && r.ReviewDate.Month == DateTime.Now.Month - 1
                              select r.Rating).ToList()
                                  )

                     });

            if (q.Any())
            {
                return q.Take(top).ToList();
            }

            return null;
        }

        private static int CalculateSatisfactionRate(List<int> ratings = null)
        {
            if (ratings is null)
            {
                return 0;
            }

            var above3 = ratings.Where(r => r >= 3).Count();

            return (above3 / ratings.Count) * 100;
        }
    }
}
