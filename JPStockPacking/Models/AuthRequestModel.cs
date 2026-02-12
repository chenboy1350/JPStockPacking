namespace JPStockPacking.Models
{
    public class AuthRequestModel
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? Audience { get; set; }
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
