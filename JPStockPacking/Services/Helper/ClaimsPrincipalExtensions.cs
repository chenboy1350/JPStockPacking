using System.Security.Claims;

namespace JPStockPacking.Services.Helper
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out var id))
            {
                return id;
            }
            return null;
        }

        public static string? GetUsername(this ClaimsPrincipal user)
        {
            return user.Identity?.Name;
        }

        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            return user.HasClaim("Permission", permission);
        }
    }
}
