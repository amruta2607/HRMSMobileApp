namespace MobileWebApi.Models
{
    /// <summary>
    /// Response for POST /api/auth/refresh-token.
    /// </summary>
    public class AccessTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
