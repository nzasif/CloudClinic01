using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using CloudClinic.Account.Models;
using CloudClinic.Account.Services;
using CloudClinic.Account.Validators;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Doctor.Services;
using CloudClinic.Shared;
using CloudClinic.Shared.AuthConstants;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CloudClinic.Account.Controllers
{
    [Route(Routes.AccountRegisterControllerRoute)]
    [ApiController]
    [AllowAnonymous]
    public class RegisterController : ControllerBase
    {
        private object response;

        private CloudClinicDb db { get; set; }
        private UserManager<AppUser> userManager { get; set; }
        private SignInManager<AppUser> signInManager { get; set; }
        private IDrStaffDetailsProvider drStaffDetailsProvider { get; set; }
        private ITokenManager tokenManager { get; set; }
        private IRegisterService registerService { get; set; }
        private IConfiguration configuration { get; set; }
        private byte[] secretKey { get; set; }

        public RegisterController(
            CloudClinicDb Db, 
            UserManager<AppUser> UserManager,
            SignInManager<AppUser> SignInManager,
            IRegisterService RegisterService,
            IDrStaffDetailsProvider DrStaffDetailsProvider,
            IConfiguration Configuration,
            ITokenManager TokenManager
            )
        {
            db = Db;
            userManager = UserManager;
            signInManager = SignInManager;
            registerService = RegisterService;
            drStaffDetailsProvider = DrStaffDetailsProvider;
            tokenManager = TokenManager;
            configuration = Configuration;

            secretKey = Encoding.UTF8.GetBytes(configuration["ApplicationSettings:JWT_Secret"].ToString());
        }

        [HttpGet]
        public string Get()
        {
            return "Register";
        }

        [HttpPost]
        [Route(Routes.SigupRoute)]
        public async Task<IActionResult> CreateUserAsync([FromBody] UserCreateModel createUserModel)
        {
            // Todo: Put everthing in a transaction

            var user = new AppUser
            {
                FullName = createUserModel.FullName,
                UserName = createUserModel.UserName,
                Email = createUserModel.Email,
                Gender = createUserModel.Gender,
                PhoneNumber = createUserModel.PhoneNumber,
                DOB = createUserModel.DOB,
                Province = createUserModel.Province,
                City = createUserModel.City,
                Street = createUserModel.Street,
                RegDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };

            if ((DateTime.Now.Year - user.DOB.Year) < 18 && createUserModel.Role == Roles.DrRole)
            {
                response = HelperMethods.CreateResponse(
                    "400",
                    new ErrorModel { Type = "400", Message = "As a Dr. you must be 18+ years old." },
                    null);

                return Ok(response);
            }
            var result = await userManager.CreateAsync(user, createUserModel.Password);
            if (result.Succeeded)
            {
                // now add user to role
                string role = createUserModel.Role;
                try
                {
                    var r = await userManager.AddToRoleAsync(user, createUserModel.Role);

                    // if adding to role failed then rolback user creation
                    if (!r.Succeeded)
                    {
                        await userManager.DeleteAsync(user);

                        return Ok(
                            HelperMethods.CreateResponse(
                                "400", new ErrorModel { Type = ErrorTypes.RoleAssigningFailed, Message = "some thing bad happened" }, null
                                )
                            );
                    }

                    /// Now add dr init details
                    if (role == Roles.DrRole)
                    {
                        await registerService.AddInitDrDetial(db, user.Id);

                    }
                }
                catch (Exception e)
                {
                    // if it throw exception then also roleback
                    await userManager.DeleteAsync(user);

                    return Ok(
                        HelperMethods.CreateResponse(
                            "400", new ErrorModel { Type = ErrorTypes.RoleAssigningFailed, Message = "some thing bad happen" }, null
                            )
                        );
                }
                #region commented
                    //at this point email is not verified
                    // once it verified then user can login and get token
                    //////////////////////////////////////////////////////////////
                    //List<Claim> claimsList = tokenManager.CreateDefaultClaimsList(user, role);

                    //if (role == Roles.DrRole || role == Roles.DrRole.ToUpper())
                    //{
                    //    var dr = await registerService.AddInitDrDetial(db, user.Id);
                    //    claimsList = AddDrSpecificClaims(claimsList, dr);
                    //}

                    //var token = tokenManager.CreateToken(claimsList, secretKey);
                #endregion

                // generate email confirmation code
                string token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                string code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                //----------------------------- for seding email --------------- uncomment this
                //string callbackUrl = HelperMethods.CreateUrl(Routes.ConfirmEmailRoute);

                //string msg = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";
                // -------------------- for sending email----------------

                if (role == Roles.NormalUserRole)
                {
                    var r = await userManager.ConfirmEmailAsync(user, code);

                    response = HelperMethods.CreateResponse("200", null, new { msg = "Account successfully created" + r.Succeeded});
                    return Ok(response);
                }

                // in production the confirmation link will not directly sent
                // it will be emailed to the user (in role of DR)
                response = HelperMethods.CreateResponse(
                        "200",
                        new ErrorModel { Type = ErrorTypes.UnVerifiedEmail, Message = "Your email is not yet verified" },
                        new { userId = user.Id, code });

                return Ok(response);
            }
            else
            {
                string error = "";

                foreach (var er in result.Errors)
                {
                    error += er.Description + "<br>";
                }

                var response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel { Type = ErrorTypes.ServerError, Message = error },
                        null);

                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route(Routes.SigninRoute)]
        public async Task<IActionResult> SignIn([FromBody] LoginModel login)
        {
            string input = login.UserName;

            AppUser user = null;

            object response;

            if (input.Contains('@'))
            {
                if (ValidatorService.IsValidateEmail(input))
                {
                    user = await userManager.FindByEmailAsync(input);
                }
                else
                {
                    response = HelperMethods.CreateResponse(
                            "400",
                            new ErrorModel { Type = ErrorTypes.ValidationError, Message = "not a valid email" },
                            null);

                    return Ok(response);
                }
            }
            else
            {
                user = await userManager.FindByNameAsync(input);
            }

            if (user is null)
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = "this User is not found" },
                        null);

                return Ok(response);
            }

            if (!(await userManager.CheckPasswordAsync(user, login.Password)))
            {
                response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel { Type = ErrorTypes.NotValidCredentials, Message = "Credentials are not valid" },
                        null);

                return Ok(response);
            }

            //// now signin user
            //await signInManager.SignInAsync(user, true);

            // check if is removed
            if (user.IsRemoved == true)
            {
                // find the removal reason and datetime
                RemovedUser removedUser = db.RemovedUsers.FirstOrDefault(ru => ru.UserId == user.Id);

                response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel { Type = ErrorTypes.RemovedUser, Message = "user is removed due " + removedUser.RemovalCause },
                        null);

                return BadRequest(response);
            }

            // if it is not user\
            if (!(await userManager.IsInRoleAsync(user, Roles.NormalUserRole)))
            {
                // now check if email is confirmed
                if (!(await userManager.IsEmailConfirmedAsync(user)))
                {
                    // user id is sent so he can call the ResendConfirmationEmail()
                    response = HelperMethods.CreateResponse(
                            "400",
                            new ErrorModel { Type = ErrorTypes.UnVerifiedEmail, Message = "Email is not verified, plz verify email" },
                            new { userId = user.Id });
                    return BadRequest(response);
                }
            }

            // common claims for all
            List<Claim> claimsList = await tokenManager.CreateDefaultClaimsList(user);

            if (await userManager.IsInRoleAsync(user, Roles.DrRole))
            {
                var dr = db.DrDetails.FirstOrDefault(dr => dr.UserId == user.Id);

                if (!dr.IsVerified)
                {
                    response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel 
                        { 
                            Type = "not verified",
                            Message = "Your are not yet verified, we will inform you once you are verified"
                        },
                        null);

                    return Ok(response);
                }

                claimsList = AddDrSpecificClaims(claimsList, dr);
            }

            // check the staff master role
            if (await userManager.IsInRoleAsync(user, Roles.StaffRole))
            {
                var staff = drStaffDetailsProvider.GetStaffDetail(db, user.Id);
                var dr = db.DrDetails.FirstOrDefault(dr => dr.DrId == staff.DrId);

                if (!dr.IsVerified)
                {
                    response = HelperMethods.CreateResponse(
                        "400",
                        new ErrorModel
                        {
                            Type = "not verified",
                            Message = "Your are not yet verified, we will inform you once you are verified"
                        },
                        null);

                    return Ok(response);
                }

                // this is one of the staff role
                claimsList = AddStaffSpecificClaims(claimsList, staff, dr, user.Id);
            }

            var token = tokenManager.CreateToken(claimsList);

            response = HelperMethods.CreateResponse(
                    "200", null, new { token });

            return Ok(response);

        }

        // Logout: Just remove or delete the jwt (token) from the local storage.
        // server does not save/remember any thing (other than jwt) about the client, it all 
        // happening due to the jwt
        [Authorize]
        [HttpGet]
        [Route("signout")]
        public async Task<IActionResult> Signout()
        {
            await signInManager.SignOutAsync();

            return Ok("Signout successed");
        }

        [HttpGet]
        [Route(Routes.ConfirmEmailRoute)]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string code)
        {
            if (userId == null || code == null)
            {
                return Redirect(HelperMethods.CreateUrl("home"));
            }

            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                // component with this url
                return Redirect(HelperMethods.CreateUrl("notfound"));
            }

            // todo: check if user is not removed

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await userManager.ConfirmEmailAsync(user, code);

            if (!result.Succeeded)
            {
                var er = "";
                foreach (var err in result.Errors)
                {
                    er += err;
                }
                // there will be a component with this name
                return Redirect(HelperMethods.CreateUrl("emailvarificationfailed"));
            }

            // component
            return Redirect(HelperMethods.CreateUrl("emailconfirmed"));
        }
         
        [HttpGet]
        [Route(Routes.ResendConfirmEmailRoute)]
        public async Task<IActionResult> ResendConfirmationEmail([FromQuery] string userId)
        {
            var u = await userManager.FindByIdAsync(userId);
            object response;

            if (u is null)
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = ErrorMessages.UserNotFound },
                        null);

                return NotFound(response);

            }

            // TODO: also check if user is not removed

            var code = await userManager.GenerateEmailConfirmationTokenAsync(u);

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            // TODO: create emailconfirmation link and send in email....

            // in production this will not done linke this..
            response = HelperMethods.CreateResponse("200", null, new { userId = u.Id, code });

            return Ok(response);
        }

        [HttpPost]
        [Route(Routes.ForgotPasswordRoute)]
        public async Task<IActionResult> ForgotPassword([FromBody] string userName)
        {
            object response;
            var user = await userManager.FindByNameAsync(userName);

            if (user is null)
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = ErrorMessages.UserNotFound },
                        null);

                return Ok(response);
            }

            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            //----------------------------- for seding email --------------- uncomment this
            //string callbackUrl = HelperMethods.CreateUrl(Routes.PasswordResetRoute);

            //string msg = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";
            // -------------------- for sending email----------------

            response = HelperMethods.CreateResponse(
                    "200", null, new { msg = "We have sent a password reset link to your email address" });

            return Ok(response);
        }

        private List<Claim> AddDrSpecificClaims(List<Claim> claimsList, DrDetail dr)
        {
            claimsList.Add(new Claim(DrIdClaim.Name, dr.DrId.ToString()));

            claimsList.Add(new Claim(IsDrVerifiedClaim.Name, dr.IsVerified.ToString()));

            return claimsList;
        }

        private List<Claim> AddStaffSpecificClaims(List<Claim> claimsList, StaffDetail staff, DrDetail dr, string userId)
        {
            claimsList.Add(new Claim(StaffIdClaim.Name, staff.StaffId.ToString()));

            // for the policy "OnlyVerifiedDrs", we need to add this claim to dr Staff too.
            // so as; you must be a staff and your dr must be verified
            //claimsList.Add(new Claim(IsDrVerifiedClaim.Name, dr.IsVerified.ToString()));
            //
            //// the above is removed because token is only given to verified drs

            // always use DrId claim for staff, as drDetailProvider.getDrId() will look for this type;
            claimsList.Add(new Claim(DrIdClaim.Name, staff.DrId.ToString()));
            claimsList.Add(new Claim(StaffDrUserIdClaim.Name, dr.UserId));
            return claimsList;
        }
    }
}