using CloudClinic.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CloudClinic.Account.Services
{
    public interface ITokenManager
    {
        string CreateToken(/*Dictionary<string, string> claimsDictionary*/ List<Claim> claimsList);
        /*/*Dictionary<string, string>*/
        Task<List<Claim>> CreateDefaultClaimsList(AppUser user);

    }
}
