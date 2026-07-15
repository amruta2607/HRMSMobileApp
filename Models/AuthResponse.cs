namespace MobileWebApi.Models
{
    /// <summary>
    /// Token pair returned by login and refresh-token endpoints.
    /// </summary>
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }
}
