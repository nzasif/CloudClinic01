using CloudClinic.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CloudClinic.Pages.Admin.Services
{
    public class CustomClaimsFactory: UserClaimsPrincipalFactory<AppUser>
    {
        public CustomClaimsFactory(
        UserManager<AppUser> userManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, optionsAccessor)
        {

        }

        protected async override Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var opt = new ClaimsIdentityOptions();

            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(opt.RoleClaimType, "ADMIN"));

            return identity;
        }
    }
}
