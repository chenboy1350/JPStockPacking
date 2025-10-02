using JPStockPacking.Services.Interface;
using System.Net;
using System.Net.Http.Headers;

namespace JPStockPacking.Services.Helper
{
    public class TokenHandler(IHttpContextAccessor contextAccessor, IAuthService authService) : DelegatingHandler
    {
        private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
        private readonly IAuthService _authService = authService;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _contextAccessor.HttpContext;

            // ดึง AccessToken จาก Cookie
            var token = context?.Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // ยิง request ครั้งแรก
            var response = await base.SendAsync(request, cancellationToken);

            // ถ้าเจอ 401 → ลอง Refresh Token แล้ว retry
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshResult = await _authService.RefreshTokenAsync();
                if (refreshResult.Success && !string.IsNullOrEmpty(refreshResult.AccessToken))
                {
                    // เขียน AccessToken ใหม่กลับเข้า Cookie
                    context?.Response.Cookies.Append("AccessToken", refreshResult.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(1),
                        IsEssential = true
                    });

                    // ยิง request ซ้ำด้วย token ใหม่
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshResult.AccessToken);
                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    // refresh ไม่ได้ → redirect ไป login
                    context?.Response.Redirect("\\login");
                }
            }

            return response;
        }
    }
}
