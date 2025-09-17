using JPStockPacking.Services.Interface;

namespace JPStockPacking.Services.Middleware
{
    public class AutoRefreshTokenMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        private readonly RequestDelegate _next = next;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task InvokeAsync(HttpContext context)
        {
            // Step 1: ถ้า user login อยู่ ให้เช็คว่า AuthTime หมดอายุหรือยัง
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

            // Step 2: ดัก response จาก pipeline
            var originalBody = context.Response.Body;
            using var newBody = new MemoryStream();
            context.Response.Body = newBody;

            await _next(context);

            // ถ้า API ตอบกลับ 401 → บังคับ logout + redirect ไป login
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                using var scope = _serviceProvider.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                await authService.LogoutAsync();
                context.Response.Redirect("/login");
                return;
            }

            // Copy response body กลับไป
            newBody.Seek(0, SeekOrigin.Begin);
            await newBody.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }
    }
}
