using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudClinic.Admin.Models;
using CloudClinic.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CloudClinic.Pages.Admin.Account
{
    [AllowAnonymous]
    public class AdminLoginModel : PageModel
    {
        private UserManager<AppUser> userManager;
        private SignInManager<AppUser> loginManager;
        private CloudClinicDb db;

        [BindProperty]
        public LoginModel LoginModel { get; set; }

        public AdminLoginModel(
            CloudClinicDb DB,
            UserManager<AppUser> manager,
            SignInManager<AppUser> signInManager)
        {
            userManager = manager;
            loginManager = signInManager;
            db = DB;
        }
        public async Task OnGet()
        {
            //var u = new AppUser
            //{
            //    FullName = "Asif",
            //    DOB = new DateTime(1998, 1, 20),
            //    UserName = "asif2020",
            //    Email = "a@a.a",
            //    EmailConfirmed = true,
            //    GuardianName = "a",
            //    PhoneNumber = "0321702924",
            //    ProfilePicName = "admin.png",
            //    Gender = "male",
            //    Street = "Baka Khel",
            //    City = "Bannu",
            //    Province = "KPK",
            //    RegDate = DateTime.Now
            //};

            //await userManager.CreateAsync(u, "1234");
            //u = await userManager.FindByNameAsync("asif2020");
            //db.Roles.Add(new IdentityRole { Id = "2222222323", Name = "ADMIN", NormalizedName = "ADMIN" });
            //db.SaveChanges();
            //var r = await userManager.AddToRoleAsync(u, "ADMIN");
        }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            var r = await loginManager.PasswordSignInAsync(
                LoginModel.UserName, LoginModel.Password, isPersistent: true, lockoutOnFailure: false);

            if (r.Succeeded)
            {
                return RedirectToPage("../Admin");
            }

            ViewData.Add("error", "Invalid credentials");

            return Page();
        }

        public async Task OnGetLogout()
        {
            await loginManager.SignOutAsync();

            HttpContext.Response.Cookies.Delete("Identity.Application");
        }
    }
}
