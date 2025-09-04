using JPStockPacking.Services.Interface;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace JPStockPacking.Services.Implement
{
    public class CookieAuthService : ICookieAuthService
    {
        public async Task SignInAsync(HttpContext context, int id, string username, string role, string department, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, id.ToString()),
                new (ClaimTypes.Name, username),
                new (ClaimTypes.Role, role),
                new ("Department", department)
            };

            var identity = new ClaimsIdentity(claims, "AppCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(1)
            };

            await context.SignInAsync("AppCookieAuth", principal, authProps);
        }

        public async Task SignOutAsync(HttpContext context)
        {
            await context.SignOutAsync("AppCookieAuth");
        }
    }
}
