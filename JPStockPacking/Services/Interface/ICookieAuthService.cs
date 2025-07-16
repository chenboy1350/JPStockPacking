namespace JPStockPacking.Services.Interface
{
    public interface ICookieAuthService
    {
        Task SignInAsync(HttpContext context, int id, string username, string role, bool rememberMe);
        Task SignOutAsync(HttpContext context);
    }
}
