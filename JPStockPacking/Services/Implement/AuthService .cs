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
        IConfiguration configuration,
        Serilog.ILogger logger
        ) : IAuthService
    {
        private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
        private readonly ICookieAuthService _cookieAuthService = cookieAuthService;
        private readonly IConfiguration _configuration = configuration;
        private readonly Serilog.ILogger _logger = logger;

        public async Task<LoginResult> LoginUserAsync(string username, string password, bool rememberMe)
        {
            try
            {
                var context = _contextAccessor.HttpContext!;
                var authResult = await ValidateUserAsync(username, password);

                _logger.Information("AuthResult: {@AuthResult}", authResult);

                if (authResult == null || string.IsNullOrEmpty(authResult.AccessToken))
                    return new LoginResult { Success = false, Message = "Invalid credentials" };

                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(authResult.AccessToken);
                var exp = token.ValidTo;
                var userId = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var usernameFromToken = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                if (userId == null || usernameFromToken == null)
                    return new LoginResult { Success = false, Message = "Invalid token claims" };


                await _cookieAuthService.SignInAsync(context, int.Parse(userId), usernameFromToken, rememberMe);

                if (!string.IsNullOrEmpty(authResult.RefreshToken))
                {
                    var refreshTokenExpiry = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddMinutes(60);

                    context.Response.Cookies.Append("AccessToken", authResult.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(120),
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
            catch (Exception ex)
            {
                return new LoginResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<RefreshTokenResult> RefreshTokenAsync()
        {
            var context = _contextAccessor.HttpContext!;
            var refreshToken = context.Request.Cookies["RefreshToken"];

            _logger.Information("refreshToken Cookies : {@refreshToken}", refreshToken);

            if (string.IsNullOrEmpty(refreshToken))
                return new RefreshTokenResult { Success = false, Message = "No refresh token found" };

            try
            {
                var refreshResult = await CallRefreshTokenApi(refreshToken);

                _logger.Information("RefreshResult: {@RefreshResult}", refreshResult);

                if (refreshResult == null || string.IsNullOrEmpty(refreshResult.AccessToken))
                    return new RefreshTokenResult { Success = false, Message = "Failed to refresh token" };

                if (!string.IsNullOrEmpty(refreshResult.AccessToken))
                {
                    context.Response.Cookies.Append("AccessToken", refreshResult.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(120),
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

                if (userId == null || usernameFromToken == null)
                    return new RefreshTokenResult { Success = false, Message = "Invalid token claims" };

                await _cookieAuthService.SignInAsync(context, int.Parse(userId), usernameFromToken, false);

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
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
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
            var urlRevokeToken = apiSettings["RevokeToken"];

            using var httpClient = new HttpClient();

            var request = new RefreshTokenRequestModel { RefreshToken = refreshToken };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(urlRevokeToken, content);
            return response.IsSuccessStatusCode;
        }

        private async Task<AuthResponseModel> ValidateUserAsync(string username, string password)
        {
            try
            {
                InputValidator validator = new();
                var apiSettings = _configuration.GetSection("ApiSettings");
                var apiKey = apiSettings["APIKey"];
                var Audience = apiSettings["Audience"];
                var urlAccessToken = apiSettings["AccessToken"];

                if (validator.IsValidInput(username) && validator.IsValidInput(password))
                {
                    using var httpClient = new HttpClient();
                    var requestBody = new AuthRequestModel
                    {
                        ClientId = username,
                        ClientSecret = password,
                        Audience = Audience
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
            catch (Exception)
            {
                throw;
            }
        }

        private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}
