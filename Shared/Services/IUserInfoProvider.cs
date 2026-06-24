using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CloudClinic.Shared.Services
{
    public interface IUserProfileProvider
    {
        string GetUserId(ClaimsPrincipal user);
    }
}
