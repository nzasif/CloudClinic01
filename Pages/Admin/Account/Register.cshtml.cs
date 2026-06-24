using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CloudClinic.Admin.Pages.Account
{
    public class RegisterModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult Login()
        {
            return Page();
        }
    }
}
