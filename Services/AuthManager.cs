using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace facebook_demo.Services
{
    public class AuthManager
    {
        public bool IsSignedIn(ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException("principal");

            if ((principal != null ? principal.Identities : null) != null)
                return principal.Identities.Any(i => i.AuthenticationType == "Facebook");

            return false;
        }

        public string GetUserName(ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException("principal");

            return principal.FindFirstValue(ClaimTypes.Name);
        }

        public async Task SignOut(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public async Task SignIn(HttpContext httpContext, string name, string pageToken)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, name, ClaimValueTypes.String),
                new Claim("PageToken", pageToken, ClaimValueTypes.String),
            };

            var userIdentity = new ClaimsIdentity(claims, "Facebook");

            var userPrincipal = new ClaimsPrincipal(userIdentity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                userPrincipal);
        }

        public string GetPageToken(ClaimsPrincipal principal)
        {
            return principal.FindFirstValue("PageToken");
        }
    }
}
