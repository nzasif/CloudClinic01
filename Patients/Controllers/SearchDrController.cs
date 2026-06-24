using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using CloudClinic.Patients.Models;
using CloudClinic.Patients.Services;
using CloudClinic.Shared;
using CloudClinic.Shared.AuthConstants;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudClinic.Patients.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route(Routes.SearchDrControllerRoute, Name = Routes.SearchDrControllerRoute)]
    public class SearchDrController : ControllerBase
    {
        private object response;

        private CloudClinicDb db { get; set; }
        private ISearchDrService searchDrService { get; set; }
        private IUserProfileProvider userProfileProvider { get; set; }
        public SearchDrController(CloudClinicDb Db, ISearchDrService SearchDrService, IUserProfileProvider userInfoProvider)
        {
            db = Db;
            searchDrService = SearchDrService;
            userProfileProvider = userInfoProvider;
        }

        [HttpGet(Routes.SearchDrByNameRoute, Name = Routes.SearchDrByNameRoute)]
        public async Task<IActionResult> SearchDrByName([FromQuery] string drName, [FromQuery] string cityName, [FromQuery] int offset)
        {
            if (drName == null)
            {
                response = HelperMethods.CreateResponse(
                    "400",
                    new ErrorModel { Type = ErrorTypes.ServerError, Message = "provide a name" },
                    null);

                return Ok(response);
            }

            var drProfiles = await searchDrService.SearchDrByName(db, drName, cityName, 10, offset, HttpContext);

            if (drProfiles == null)
            {
                response = HelperMethods.CreateResponse(
                   "404",
                   new ErrorModel { Type = ErrorTypes.ServerError, Message = "not found" },
                   null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse(
                   "200", null, new { drsSearchResults = drProfiles });

            return Ok(response);
        }

        [HttpGet(Routes.SearchDrBySpecialtyRoute, Name = Routes.SearchDrBySpecialtyRoute)]
        public async Task<IActionResult> SearchDrBySpecialty([FromQuery] string drSpecialty, [FromQuery] string cityName, [FromQuery] int offset)
        {
            if (drSpecialty == null)
            {
                response = HelperMethods.CreateResponse(
                       "400",
                       new ErrorModel { Type = ErrorTypes.ServerError, Message = "provide a specialty term" },
                       null);

                return Ok(response);
            }

            var drProfiles = await searchDrService.SearchDrBySpecialty(db, drSpecialty, cityName, 10, offset, HttpContext);

            if (drProfiles is null)
            {
                response = HelperMethods.CreateResponse(
                      "404",
                      new ErrorModel { Type = ErrorTypes.ServerError, Message = "not found" },
                      null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse(
                   "200", null, new { drsSearchResults = drProfiles });

            return Ok(response);
        }

        [HttpGet(Routes.GetDrDetailsRoute, Name = Routes.GetDrDetailsRoute)]
        public async Task<IActionResult> GetDrDetails([FromQuery] Guid drId)
        {
            try
            {
                PatDrDetailViewModel drDetail = await searchDrService.GetDrDetails(db, drId, HttpContext);
                
                if (drDetail is null)
                {
                    response = HelperMethods.CreateResponse(
                      "404",
                      new ErrorModel { Type = ErrorTypes.ServerError, Message = "not found dr with id = " + drId.ToString() },
                      null);

                    return Ok(response);
                }

                response = HelperMethods.CreateResponse(
                   "200", null, new { drDetail });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
                         "400",
                         new ErrorModel { Type = "bad", Message = "not found" },
                         null);

                return Ok(response);
            }
        }

        [HttpGet(Routes.GetTopTenDrsRoute, Name = Routes.GetTopTenDrsRoute)]
        public async Task<IActionResult> GetTopDrs([FromQuery] int top)
        {
            var topTenDrs = await searchDrService.GetTopDrs(db, HttpContext, top);

            if (topTenDrs is null)
            {
                response = HelperMethods.CreateResponse(
                  "404",
                  new ErrorModel { Type = ErrorTypes.ServerError, Message = "not found drs" },
                  null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse("200", null, new { topTenDrs });

            return Ok(response);
        }
    }
}