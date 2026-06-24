using CloudClinic.Data;
using CloudClinic.Shared.AuthConstants;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CloudClinic.Account.Services
{
    public class TokenManager: ITokenManager
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;
        public TokenManager(UserManager<AppUser> manager, IConfiguration configuration)
        {
            _userManager = manager;
            _config = configuration;
        }

        public /*Dictionary<string, string>*/ async Task<List<Claim>> CreateDefaultClaimsList(AppUser user)
        {
            ClaimsIdentityOptions identityOptions = new ClaimsIdentityOptions();

            List<Claim> claimsList = new List<Claim>();
            var roles = await _userManager.GetRolesAsync(user);

            claimsList.Add(new Claim(UserIdClaim.Name, user.Id));
            // these are removed because when user change profile a new token needs to be granted
            //
            //claimsList.Add(new Claim(UserNameClaim.Name, user.UserName));
            //claimsList.Add(new Claim(UserEmailClaim.Name, user.Email));
            //claimsList.Add(new Claim(UserPhoneNumberClaim.Name, user.PhoneNumber));
            //claimsList.Add(new Claim(UserFullNameClaim.Name, user.FullName));

            // Is Removed is checked before creating token in signin method...
            // so if user is removed then token will not be granted..
            //claimsList.Add(new Claim(IsRemovedClaim.Name, user.IsRemoved.ToString()));

            foreach (var role in roles)
            {
                claimsList.Add(new Claim(identityOptions.RoleClaimType, role));
            }

            // this one is added to make the role claim as array in token(Json formate)
            // even if there is only one role associated with user
            // this is done for consitency of role claim use in client side code
            claimsList.Add(new Claim(identityOptions.RoleClaimType, "r"));

            return claimsList;

            //// add AppUser properties to claimsDictionary
            //return new Dictionary<string, string>
            //    {
            //        {UserIdClaim.Name, user.Id },
            //        {UserNameClaim.Name, user.UserName },
            //        {UserEmailClaim.Name, user.Email },
            //        {UserPhoneNumberClaim.Name, user.PhoneNumber },
            //        {UserFullNameClaim.Name, user.FullName },
            //        {IsRemovedClaim.Name, user.IsRemoved.ToString() },
            //        {identityOptions.RoleClaimType, role }
            //    };
        }

        public string CreateToken(/*Dictionary<string, string> claimsDictionary*/ List<Claim> claimsList)
        {
            //List<Claim> claimsList = new List<Claim>();

            //foreach (var claim in claimsDictionary)
            //{
            //    claimsList.Add(new Claim(claim.Key, claim.Value));
            //}
            // Fetch the key here so it always matches Program.cs
            var secret = _config["ApplicationSettings:JWT_Secret"];
            var key = Encoding.UTF8.GetBytes(secret!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claimsList),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            var token = tokenHandler.WriteToken(securityToken);

            return token;               
        }

    }
}
