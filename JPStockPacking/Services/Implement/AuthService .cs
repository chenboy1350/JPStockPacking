using JPStockPacking.Models;
using JPStockPacking.Services.Helper;
using JPStockPacking.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace JPStockPacking.Services.Implement
{
    public class AuthService(
        IHttpContextAccessor contextAccessor,
        ICookieAuthService cookieAuthService,
        IConfiguration configuration) : IAuthService
    {
        private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
        private readonly ICookieAuthService _cookieAuthService = cookieAuthService;
        private readonly IConfiguration _configuration = configuration;

        public async Task<LoginResult> LoginUserAsync(string username, string password, bool rememberMe)
        {
            var context = _contextAccessor.HttpContext!;
            var authResult = await ValidateUserAsync(username, password);

            if (authResult == null || string.IsNullOrEmpty(authResult.AccessToken))
                return new LoginResult { Success = false, Message = "Invalid credentials" };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(authResult.AccessToken);
            var exp = token.ValidTo;
            var userId = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var usernameFromToken = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var roles = token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            if (userId == null || usernameFromToken == null || roles.Count == 0)
                return new LoginResult { Success = false, Message = "Invalid token claims" };

            var role = roles.Contains("1") ? "Admin" : roles.Contains("2") ? "User" : "Guest";

            await _cookieAuthService.SignInAsync(context, int.Parse(userId), usernameFromToken, role, rememberMe);

            if (!string.IsNullOrEmpty(authResult.RefreshToken))
            {
                var refreshTokenExpiry = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(24);

                context.Response.Cookies.Append("AccessToken", authResult.AccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = rememberMe
                        ? DateTimeOffset.UtcNow.AddDays(7)
                        : DateTimeOffset.UtcNow.AddHours(1),
                    IsEssential = true
                });


                context.Response.Cookies.Append("RefreshToken", authResult.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = refreshTokenExpiry,
                    IsEssential = true
                });
            }

            if (rememberMe)
            {
                context.Response.Cookies.Append("RememberedUsername", username, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    IsEssential = true
                });
                context.Response.Cookies.Append("RememberMeChecked", "true", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    IsEssential = true
                });
            }
            else
            {
                context.Response.Cookies.Delete("RememberedUsername");
                context.Response.Cookies.Delete("RememberMeChecked");
            }

            return new LoginResult { Success = true };
        }

        public async Task<RefreshTokenResult> RefreshTokenAsync()
        {
            var context = _contextAccessor.HttpContext!;
            var refreshToken = context.Request.Cookies["RefreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return new RefreshTokenResult { Success = false, Message = "No refresh token found" };

            try
            {
                var refreshResult = await CallRefreshTokenApi(refreshToken);

                if (refreshResult == null || string.IsNullOrEmpty(refreshResult.AccessToken))
                    return new RefreshTokenResult { Success = false, Message = "Failed to refresh token" };

                if (!string.IsNullOrEmpty(refreshResult.AccessToken))
                {
                    context.Response.Cookies.Append("AccessToken", refreshResult.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(1),
                        IsEssential = true
                    });
                }

                if (!string.IsNullOrEmpty(refreshResult.RefreshToken))
                {
                    context.Response.Cookies.Append("RefreshToken", refreshResult.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddDays(7),
                        IsEssential = true
                    });
                }

                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(refreshResult.AccessToken);
                var userId = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var usernameFromToken = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var role = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (userId == null || usernameFromToken == null || role == null)
                    return new RefreshTokenResult { Success = false, Message = "Invalid token claims" };

                await _cookieAuthService.SignInAsync(context, int.Parse(userId), usernameFromToken, role, false);

                return new RefreshTokenResult
                {
                    Success = true,
                    AccessToken = refreshResult.AccessToken,
                    RefreshToken = refreshResult.RefreshToken
                };
            }
            catch (Exception ex)
            {
                return new RefreshTokenResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<bool> LogoutAsync()
        {
            var context = _contextAccessor.HttpContext!;
            var refreshToken = context.Request.Cookies["RefreshToken"];

            try
            {
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await CallRevokeTokenApi(refreshToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error revoking token: {ex.Message}");
            }
            finally
            {
                context.Response.Cookies.Delete("RefreshToken");
                context.Response.Cookies.Delete("RememberedUsername");
                context.Response.Cookies.Delete("RememberMeChecked");

                await _cookieAuthService.SignOutAsync(context);
            }

            return true;
        }

        private async Task<AuthResponseModel?> CallRefreshTokenApi(string refreshToken)
        {
            var apiSettings = _configuration.GetSection("ApiSettings");
            var apiKey = apiSettings["APIKey"];
            var urlRefreshToken = apiSettings["RefreshToken"];

            using var httpClient = new HttpClient();

            var request = new RefreshTokenRequestModel { RefreshToken = refreshToken };
            var json = JsonSerializer.Serialize(request, CachedJsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(urlRefreshToken, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AuthResponseModel>(responseContent, CachedJsonSerializerOptions);
            }

            return null;
        }

        private async Task<bool> CallRevokeTokenApi(string refreshToken)
        {
            var apiSettings = _configuration.GetSection("ApiSettings");
            var apiKey = apiSettings["APIKey"];
            var urlAccessToken = apiSettings["AccessToken"];

            using var httpClient = new HttpClient();

            var request = new RefreshTokenRequestModel { RefreshToken = refreshToken };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://localhost/JPWEBAPI/api/auth/RevokeToken", content);
            return response.IsSuccessStatusCode;
        }

        private async Task<AuthResponseModel> ValidateUserAsync(string username, string password)
        {
            try
            {
                InputValidator validator = new();
                var apiSettings = _configuration.GetSection("ApiSettings");
                var apiKey = apiSettings["APIKey"];
                var urlAccessToken = apiSettings["AccessToken"];

                if (validator.IsValidInput(username) && validator.IsValidInput(password))
                {
                    using var httpClient = new HttpClient();
                    var requestBody = new AuthRequestModel
                    {
                        ClientId = username,
                        ClientSecret = password,
                        Department = 1
                    };
                    var content = new StringContent(JsonSerializer.Serialize(requestBody, CachedJsonSerializerOptions), Encoding.UTF8, "application/json");
                    httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
                    var response = await httpClient.PostAsync(urlAccessToken, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        var user = JsonSerializer.Deserialize<AuthResponseModel>(responseString, CachedJsonSerializerOptions);
                        return user ?? new AuthResponseModel();
                    }
                    else
                    {
                        return new AuthResponseModel();
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid input provided.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while validating the user.", ex);
            }
        }

        private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public class AuthRequestModel
        {
            public string? ClientId { get; set; }
            public string? ClientSecret { get; set; }
            public int? Department { get; set; } = 0;
        }

        public class AuthResponseModel
        {
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public string TokenType { get; set; } = "Bearer";
            public int ExpiresIn { get; set; }
        }

        public class RefreshTokenResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
        }

        public class RefreshTokenRequestModel
        {
            public string RefreshToken { get; set; } = string.Empty;
        }
    }
}
