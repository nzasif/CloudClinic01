using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Pages.Admin.Services
{
    public class CustomCokieManager : ICookieManager
    {
        public void AppendResponseCookie(HttpContext context, string key, string value, CookieOptions options)
        {

        }

        public void DeleteCookie(HttpContext context, string key, CookieOptions options)
        {
            throw new NotImplementedException();
        }

        public string GetRequestCookie(HttpContext context, string key)
        {
            return "";
        }
    }
}
