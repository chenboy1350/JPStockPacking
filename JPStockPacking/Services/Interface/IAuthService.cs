using JPStockPacking.Models;
using static JPStockPacking.Services.Implement.AuthService;

namespace JPStockPacking.Services.Interface
{
    public interface IAuthService
    {
        Task<LoginResult> LoginUserAsync(string username, string password, bool rememberMe);
        Task<RefreshTokenResult> RefreshTokenAsync();
        Task<bool> LogoutAsync();
    }
}
