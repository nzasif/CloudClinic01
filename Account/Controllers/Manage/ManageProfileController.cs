using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CloudClinic.Account.Models.Manage;
using CloudClinic.Data.DbSets;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using CloudClinic.Shared.Services;
using CloudClinic.Shared;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using CloudClinic.Shared.AuthConstants;
using Microsoft.EntityFrameworkCore;

namespace CloudClinic.Account.Controllers.Manage
{
    [Route(Routes.AccountManageControllerRoute)]
    [ApiController]
    [Authorize]
    public class ManageProfileController : ControllerBase
    {
        #region properties ctor
        private CloudClinicDb db { get; set; }
        private UserManager<AppUser> userManager { get; set; }
        private SignInManager<AppUser> signInManager { get; set; }
        private readonly IWebHostEnvironment _environment;
        private readonly IUserProfileProvider _userInfoProvider;

        public ManageProfileController(
            CloudClinicDb Db,
            UserManager<AppUser> UserManager,
            SignInManager<AppUser> SignInManager,
            IWebHostEnvironment environment,
            IUserProfileProvider userInfoProvider)
        {
            _environment = environment;
            db = Db;
            userManager = UserManager;
            signInManager = SignInManager;
            _userInfoProvider = userInfoProvider;
        }
        #endregion

        [HttpPost]
        [Route(Routes.ChangeEmailRoute)]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailModel model)
        {
            var user = await db.AppUsers.FirstOrDefaultAsync(u => u.Id == _userInfoProvider.GetUserId(User));
            object response;

            if (user == null)
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = ErrorMessages.UserNotFound },
                        null);

                return Ok(response);
            }

            var code = await userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
            var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            //----------------------------- for seding email --------------- uncomment this
            //string callbackUrl = HelperMethods.CreateUrl(Routes.ConfirmEmailRoute);

            //string msg = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";
            // -------------------- for sending email----------------

            if (await userManager.IsInRoleAsync(user, Roles.NormalUserRole))
            {
                await userManager.ChangeEmailAsync(user, model.NewEmail, code);

                response = HelperMethods.CreateResponse("200", null, new { msg = "updated" });
                return Ok(response);
            }

            response = HelperMethods.CreateResponse(
                    "400",
                    new ErrorModel { Type = ErrorTypes.UnVerifiedEmail, Message = "please verify your new email, after this session you will not be able to signin." },
                    new { userId = user.Id, code });

            return Ok(response);
        }

        [HttpPost]
        [Route(Routes.ChangePasswordRoute)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var user = await db.AppUsers.FirstOrDefaultAsync(u => u.Id == _userInfoProvider.GetUserId(User));

            object response;

            if (user is null)
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = ErrorMessages.UserNotFound },
                        null);

                return Ok(response);
            }

            var r = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!r.Succeeded)
            {
                string er = "";

                foreach (var e in r.Errors)
                {
                    er += e;
                }

                response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel { Type = ErrorTypes.ServerError, Message = er },
                        null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse(
                    "200", null, new { updateDate = user.UpdateDate });

            return Ok(response);
        }

        [HttpPost]
        [Route(Routes.ChangeUserNameRoute)]
        public async Task<IActionResult> ChangeUserName([FromBody] ChangeUserNameModel model)
        {
            var user = await db.AppUsers.FirstOrDefaultAsync(u => u.Id == _userInfoProvider.GetUserId(User));

            object response;

            if (user is null)
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = ErrorMessages.UserNotFound },
                        null);

                return Ok(response);
            }

            var r = await userManager.SetUserNameAsync(user, model.NewUserName);

            if (!r.Succeeded)
            {
                string er = "";

                foreach (var e in r.Errors)
                {
                    er += e;
                }

                response = HelperMethods.CreateResponse(
                    "400",
                    new ErrorModel { Type = ErrorTypes.ServerError, Message = er },
                    null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse(
                    "200", null, new { updateDate = user.UpdateDate });

            return Ok(response);
        }

        /////////////////////// role can not be updated ////////////
        [HttpPost]
        [Route(Routes.ProfileUpdateRoute)]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateModel model)
        {
            var user = await db.AppUsers.FirstOrDefaultAsync(u => u.Id == _userInfoProvider.GetUserId(User));
            object response;

            if (user == null)
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = ErrorMessages.UserNotFound },
                        null);

                return NotFound(response);
            }

            user.DOB = model.DOB.Date;
            user.FullName = model.FullName;
            user.DOB = model.DOB.Date;
            user.City = model.City;
            user.Street = model.Street;
            user.Province = model.Province;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.UpdateDate = DateTime.Now;

            int i = await db.SaveChangesAsync();

            if (i == -1)
            {
                response = HelperMethods.CreateResponse(
                    "400",
                    new ErrorModel { Type = ErrorTypes.ServerError, Message = "failed, try again" },
                    null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse(
                    "200", null, new { updateDate = user.UpdateDate.ToShortDateString() });

            return Ok(response);
        }

        [HttpGet]
        [Route(Routes.ProfileViewRoute)]
        public async Task<IActionResult> ViewProfile()
        {
            var user = await db.AppUsers.FirstOrDefaultAsync(u => u.Id == _userInfoProvider.GetUserId(User));
            object response;

            if (user == null)
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = ErrorMessages.UserNotFound },
                        null);

                return NotFound(response);
            }

            ProfileViewModel profile = new ProfileViewModel
            {
                FullName = user.FullName,
                City = user.City,
                Province = user.Province,
                Street = user.Street,
                DOB = user.DOB.ToShortDateString(),
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                Email = user.Email,
                UserName = user.UserName,
                profilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, user.ProfilePicName),
                RegDate = user.RegDate.ToShortDateString(),
                UpdateDate = user.UpdateDate.ToShortDateString()
            };

            response = HelperMethods.CreateResponse(
                    "200", null, new { userProfile = profile });
            return Ok(response);
        }

        [HttpPost]
        [Route(Routes.SetProfilePicRoute)]
        public async Task<IActionResult> SetProfilePic([FromForm] IFormFile profilePic)
        {
            var user = await db.AppUsers.FirstOrDefaultAsync(u => u.Id == _userInfoProvider.GetUserId(User));
            object response;

            if (user == null)
            {
                 response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = ErrorMessages.UserNotFound + " why.." },
                        null);

                return Ok(response);
            }
            
            // first check if user already has profile pic 
            if (user.ProfilePicName != null)
            {
                // now delete the old one and add new one
                HelperMethods.DeleteProfilePic(user.ProfilePicName, _environment);
            }     


            string name = HelperMethods.AddProfilePic(profilePic, _environment);

            if (name.Contains("error"))
            {
                response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel { Type = "file error", Message = name },
                        null);

                return Ok(response);
            }

            user.ProfilePicName = name;
            user.UpdateDate = DateTime.Now;

            await db.SaveChangesAsync();

            response = HelperMethods.CreateResponse(
                    "200",
                    null,
                    new {
                            profilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, user.ProfilePicName),
                            updateDate = user.UpdateDate.ToShortDateString()
                        });

            return Ok(response);

        }

        [HttpPost]
        [Route(Routes.RemoveAccountRoute)]
        public async Task<IActionResult> RemoveAccount([FromBody] RemoveAccountModel model)
        {
            var user = await db.AppUsers.FirstOrDefaultAsync(u => u.Id == _userInfoProvider.GetUserId(User));
            object response;

            if (user is null)
            {
                response = HelperMethods.CreateResponse(
                       "404",
                       new ErrorModel { Type = ErrorTypes.NotFound, Message = ErrorMessages.UserNotFound },
                       null);

                return Ok(response);
            }

            if (!await userManager.CheckPasswordAsync(user, model.Password))
            {
                response = HelperMethods.CreateResponse(
                   "400",
                   new ErrorModel { Type = "invalid-password", Message = "Provide a valid password" },
                   null);

                return Ok(response);
            }

            user.IsRemoved = true;

            db.RemovedUsers.Add(new RemovedUser
            {
                UserId = user.Id,
                RemovalCause = "You have removed your account",
                RemovalDate = DateTime.Now
            });

            await db.SaveChangesAsync();

            response = HelperMethods.CreateResponse("200", null, new {msg = "your account is removed" });

            return Ok(response);
        }
    }
}
