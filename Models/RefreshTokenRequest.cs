namespace MobileWebApi.Models
{
    /// <summary>
    /// Request body for POST /api/auth/refresh-token.
    /// </summary>
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
