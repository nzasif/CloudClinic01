using System;
using System.Collections.Generic;
using System.IO;
using CloudClinic.Account.Services;
using CloudClinic.Data;
using CloudClinic.Data.DbSets;
using CloudClinic.Doctor.Models;
using CloudClinic.Doctor.Services;
using CloudClinic.Shared;
using CloudClinic.Shared.AuthConstants;
using CloudClinic.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CloudClinic.Doctor.Controllers
{
    [ApiController]
    [Route(Routes.DrStaffManageControllerRoute)]
    [Authorize(Roles = Roles.DrRole)]
    public class DrStaffManageController : ControllerBase
    {
        private readonly IWebHostEnvironment environment;

        private CloudClinicDb db { get; set; }
        private UserManager<AppUser> userManager { get; set; }
        private IRegisterService registerService { get; set; }
        private IDrStaffDetailsProvider drStaffDetailsProvider { get; set; }
        private IDrDetailsProvider drDetailsProvider { get; set; }

        public DrStaffManageController(
            UserManager<AppUser> UserManager,
            CloudClinicDb Db,
            IRegisterService RegisterService,
            IDrStaffDetailsProvider DrStaffDetailsProvider,
            IDrDetailsProvider DrDetailsProvider,
            IWebHostEnvironment env
            )
        {
            db = Db;
            registerService = RegisterService;
            userManager = UserManager;
            drDetailsProvider = DrDetailsProvider;
            drStaffDetailsProvider = DrStaffDetailsProvider;
            environment = env;
        }

        [HttpPost(Routes.AddDrStaffRoute, Name = Routes.AddDrStaffRoute)]
        public async Task<IActionResult> AddDrStaff([FromBody] StaffCreateModel model)
        {
            string drId = drDetailsProvider.GetDrId(User);
            object response;

            var t = db.Database.BeginTransaction();
            try
            {
                //first add to AppUsers
                AppUser user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    EmailConfirmed = true,
                    PhoneNumber = model.PhoneNumber,
                    FullName = model.FullName,
                    Gender = model.Gender,
                    DOB = model.DOB,
                    Province = model.Province,
                    City = model.City,
                    Street = model.Street,
                    IsRemoved = false,
                    RegDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    string er = "";
                    foreach (var error in result.Errors)
                    {
                        er += error + ", ";
                    }

                    response = HelperMethods.CreateResponse(
                            "400",
                            new ErrorModel { Type = ErrorTypes.ServerError, Message = er },
                            null);

                    return Ok(response);
                }

                // add to Staff role
                // first add the Staff master role to model.Roles array
                model.Roles.Add(Roles.StaffRole);
                var r = await userManager.AddToRolesAsync(user, model.Roles);

                // Now add To "StaffDetails"
                StaffDetail staffDetail = new StaffDetail
                {
                    StaffSalary = model.StaffSalary,
                    DrId = new Guid(drId),
                    UserId = user.Id
                };
                
                await db.StaffDetails.AddAsync(staffDetail);
                await db.SaveChangesAsync();
                await t.CommitAsync();

                response = HelperMethods.CreateResponse(
                "400", null,
                new 
                {
                    staffId = staffDetail.StaffId,
                    userId = user.Id,
                    regDate = user.RegDate.ToShortDateString()
                });

                return Ok(response);
            }
            catch (Exception e)
            {
                response = HelperMethods.CreateResponse(
               "400",
               new ErrorModel { Type = ErrorTypes.ServerError, Message = e.Message },
               null);

                return Ok(response);
            }
        }

        [HttpPost(Routes.UpdateStaffDataRoute, Name = Routes.UpdateStaffDataRoute)]
        public async Task<IActionResult> UpdateStaffData([FromBody] StaffUpdateModel model)
        {
            object response;

            var transaction = await db.Database.BeginTransactionAsync();

            string drId = drDetailsProvider.GetDrId(User);

            var staff = drStaffDetailsProvider.GetStaffProfile(db, model.StaffUserId);

            if (staff is null)
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = "staff profile not found"},
                        null);

                return Ok(response);
            }

            if (staff.UserName != model.UserName)
            {
                var r0 = await userManager.SetUserNameAsync(staff, model.UserName);

                if (!r0.Succeeded)
                {
                    response = HelperMethods.CreateResponse(
                            "400",
                            new ErrorModel { Type = "user manger error", Message = "user name is not valid or already taken" },
                            null);

                    return Ok(response);
                }
            }

            if (staff.Email != model.Email)
            {
                staff.Email = model.Email;
                staff.EmailConfirmed = true;
            }

            string code = await userManager.GeneratePasswordResetTokenAsync(staff);
            var r = await userManager.ResetPasswordAsync(staff, code, model.Password);
                
            if (!r.Succeeded)
            {
                response = HelperMethods.CreateResponse(
                "400",
                new ErrorModel { Type = "user manager error", Message = "reseting password failed" },
                null);

                return Ok(response);
            }

            // first remove all roles then add new roles
            var oldRoles = await userManager.GetRolesAsync(staff);
            if (oldRoles != model.Roles)
            {
                await userManager.RemoveFromRolesAsync(staff, oldRoles);
                await userManager.AddToRolesAsync(staff, model.Roles);
            }
            /////
           
            if (staff.ProfilePicName != null)
            {
                HelperMethods.DeleteProfilePic(staff.ProfilePicName, environment);
                staff.ProfilePicName = null;
            }

            if (model.ProfilePic != null)
            {
                string name = HelperMethods.AddProfilePic(model.ProfilePic, environment);
                
                if (name.Contains("error"))
                {
                    response = HelperMethods.CreateResponse(
                            "400",
                            new ErrorModel { Type = "file error", Message = "profile image could not be uploaded" },
                            null);

                    return Ok(response);
                }

                staff.ProfilePicName = name;
            }

            staff.FullName = model.FullName;
            staff.PhoneNumber = model.PhoneNumber;
            staff.Gender = model.Gender;
            staff.DOB = model.DOB;
            staff.City = model.City;
            staff.Province = model.Province;
            staff.Street = model.Street;
            staff.UpdateDate = DateTime.Now;

            await db.SaveChangesAsync();

            var staffDetail = db.StaffDetails.FirstOrDefault(st => st.UserId == staff.Id && st.DrId == new Guid(drId));
            if (staffDetail == null)
            {
                response = HelperMethods.CreateResponse(
                "404",
                new ErrorModel { Type = ErrorTypes.NotFound, Message = "staff Detail not found" },
                null);

                return Ok(response);
            }

            staffDetail.StaffSalary = model.StaffSalary;
            await db.SaveChangesAsync();

            await transaction.CommitAsync();

            response = HelperMethods.CreateResponse(
            "200", null,
            new { updateDate = staff.UpdateDate });

            return Ok(response);
        }

        [HttpGet(Routes.ViewAllStaffDetailsRoute, Name = Routes.ViewAllStaffDetailsRoute)]
        public async Task<IActionResult> ViewStaffDetail()
        {
            object response;

            string drId = drDetailsProvider.GetDrId(User);

            //var staffDetail = db.StaffDetails.FirstOrDefault(st => st.DrId == new Guid(drId));

            //var staffProfile = drStaffDetailsProvider.GetStaffProfile(db, staffDetail.UserId);

            var staffs = await (from staff in db.StaffDetails
                          where staff.DrId == new Guid(drId)
                          join staffU in db.AppUsers on staff.UserId equals staffU.Id
                          select new StaffViewModel
                          {
                              StaffId = staff.StaffId.ToString(),
                              StaffUserId = staff.UserId,
                              StaffSalary = staff.StaffSalary,
                              FullName = staffU.FullName,
                              UserName = staffU.UserName,
                              Email = staffU.Email,
                              DOB = staffU.DOB.ToShortDateString(),
                              PhoneNumber = staffU.PhoneNumber,
                              Gender = staffU.Gender,
                              Street = staffU.Street,
                              City = staffU.City,
                              Province = staffU.Province,
                              ProfilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, staffU.ProfilePicName),
                              RegDate = staffU.RegDate.ToShortDateString(),
                              UpdateDate = staffU.UpdateDate.ToShortDateString(),

                              Roles = (from userRole in db.UserRoles
                                       where userRole.UserId == staffU.Id
                                       join role in db.Roles on userRole.RoleId equals role.Id
                                       select role.Name).ToList()
                          }).ToListAsync();

            if (staffs.Count < 1)
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message =  "No staff found"},
                        null);

                return Ok(response);
            }

            response = HelperMethods.CreateResponse(
            "404", null, new { allStaff = staffs });

            return Ok(response);
        }

        [HttpGet(Routes.RemoveStaffRoute, Name = Routes.RemoveStaffRoute)]
        public async Task<IActionResult> RemoveStaff([FromQuery] Guid staffId)
        {
            object response;

            string drId = drDetailsProvider.GetDrId(User);

            var staffDetail = db.StaffDetails.FirstOrDefault(st => st.StaffId == staffId && st.DrId == new Guid(drId));

            var staffProfile = db.AppUsers.FirstOrDefault(u => u.Id == staffDetail.UserId);

            if (staffProfile is null || staffDetail is null)
            {
                response = HelperMethods.CreateResponse(
                        "404",
                        new ErrorModel { Type = ErrorTypes.NotFound, Message = "staff profile or detail not found" },
                        null);

                return Ok(response);
            }

            var roles = await userManager.GetRolesAsync(staffProfile);

            if (roles.Count > 0)
            {
                await userManager.RemoveFromRolesAsync(staffProfile, roles);
            }

            db.AppUsers.Remove(staffProfile);
            db.StaffDetails.Remove(staffDetail);

            await db.SaveChangesAsync();

            response = HelperMethods.CreateResponse(
            "200", null, new { removed = true });

            return Ok(response);
        }

        [HttpPost(Routes.SetStaffProfilePicRoute, Name = Routes.SetStaffProfilePicRoute)]
        public async Task<IActionResult> SetProfilePic([FromForm] string staffUserId, [FromForm] IFormFile profilePic)
        {
            var user = await db.AppUsers.FirstOrDefaultAsync(u => u.Id == staffUserId);
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
                HelperMethods.DeleteProfilePic(user.ProfilePicName, environment);
            }


            string name = HelperMethods.AddProfilePic(profilePic, environment);

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
                    new
                    {
                        profilePicUri = HelperMethods.GenerateProfilePicUri(HttpContext, user.ProfilePicName),
                        updateDate = user.UpdateDate.ToShortDateString()
                    });

            return Ok(response);

        }
    }
}