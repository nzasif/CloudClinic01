using CloudClinic.Shared.AuthConstants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CloudClinic.Shared.Services
{
    public class UserProfileProvider : IUserProfileProvider
    {
        public string GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirst(UserIdClaim.Name).Value;
        }
    }
}
