using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CloudClinic.Account.Models;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Doctor.Models;
using CloudClinic.Doctor.Services;
using CloudClinic.Shared;
using CloudClinic.Shared.AuthConstants;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CloudClinic.Doctor.Controllers
{
    [ApiController]
    [Route(Routes.DrDetailsControllerRoute)]
    [Authorize(Roles = Roles.DrRole)]
    public class DrDetailsController : ControllerBase
    {
        private object response;
        private CloudClinicDb db { get; set; }
        private IDrDetailsProvider drDetailsProvider { get; set; }

        public DrDetailsController( 
            CloudClinicDb Db, 
            IDrDetailsProvider DrDetailsProvider)
        {
            db = Db;
            drDetailsProvider = DrDetailsProvider;
        }

        ///////////////////////////////////////////////////
        //
        [HttpPost(Routes.AddOrUpdateDrDetailRoute, Name = Routes.AddOrUpdateDrDetailRoute)]
        public async Task<IActionResult> AddOrUpdateDrDetail([FromBody] DrDetailUpdateModel model)
        {
            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                DrDetail dr = drDetailsProvider.GetDrDetail(db, drId);

                if (dr is null)
                {
                    response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.DrNotVerified, Message = "The specified dr can not be found" },
                        null);

                    return NotFound(response);
                }

                dr.DrQualification = model.DrQualification;
                dr.DrSpecialty = model.DrSpecialty;
                dr.DrExperience = model.DrExperience;
                dr.DrAvgCheckTime = model.DrAvgCheckTime;
                dr.DrFee = model.DrFee;
                dr.IsVisible = model.IsVisible;
                dr.DrDescription = model.DrDescription;
                dr.UpdateDate = DateTime.Now;
                
                await db.SaveChangesAsync();
                response = HelperMethods.CreateResponse(
                            "200", null, new { updateDate = dr.UpdateDate.ToShortDateString() });

                return Ok(response);
            }
            catch (Exception)
            {
                response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel { Type = ErrorTypes.ServerError, Message = "failed try again" },
                        null);

                return BadRequest(response);
            }
        }

        ///////////////////////////////////////////////////
        [HttpGet(Routes.ViewDrDetailRoute, Name = Routes.ViewDrDetailRoute)]
        public async Task<IActionResult> ViewDrDetail()
        {
            try
            {
                string drId = drDetailsProvider.GetDrId(User);

                DrDetail dr = drDetailsProvider.GetDrDetail(db, drId);

                if (dr is null)
                {
                    response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.DrNotVerified, Message = "The specified dr can not be found" },
                        null);

                    return NotFound(response);
                }

                var drView = new DrDetailViewModel
                {
                    DrDescription = dr.DrDescription,
                    DrAvgCheckTime = dr.DrAvgCheckTime,
                    DrExperience = dr.DrExperience,
                    DrFee = dr.DrFee,
                    DrQualification = dr.DrQualification,
                    DrSpecialty = dr.DrSpecialty,
                    IsVerified = dr.IsVerified,
                    IsVisible = dr.IsVisible,
                    UpdateDate = dr.UpdateDate.ToShortDateString()
                };

                response = HelperMethods.CreateResponse(
                            "200", null, new { drDetail = drView });

                return Ok(response);
            }
            catch (Exception)
            {
                response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel { Type = ErrorTypes.ServerError, Message = "failed try again" },
                        null);

                return BadRequest(response);
            }
        }
    }
}