using JPStockPacking.Services.Interface;

namespace JPStockPacking.Services.Middleware
{
    public class AutoRefreshTokenMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        private readonly RequestDelegate _next = next;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var authTime = context.User.FindFirst("AuthTime")?.Value;
                if (authTime != null && DateTime.TryParse(authTime, out var authDateTime))
                {
                    if (authDateTime.AddMinutes(10) < DateTime.UtcNow)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                        var refreshResult = await authService.RefreshTokenAsync();
                        if (!refreshResult.Success)
                        {
                            await authService.LogoutAsync();
                            context.Response.Redirect("/login");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
