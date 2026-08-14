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

        /// <summary>
        /// Configured access-token lifetime in seconds (from Jwt:AccessTokenExpiryInSeconds).
        /// </summary>
        public int AccessTokenExpiresIn { get; set; }

        /// <summary>
        /// Configured refresh-token lifetime in hours (from Jwt:RefreshTokenExpiryInHours).
        /// </summary>
        public int RefreshTokenExpiresIn { get; set; }

        /// <summary>
        /// Local/server expiry of the generated access token (yyyy-MM-ddTHH:mm:ss).
        /// </summary>
        public string AccessTokenExpiry { get; set; } = string.Empty;

        /// <summary>
        /// Local/server expiry of the generated refresh token (yyyy-MM-ddTHH:mm:ss).
        /// </summary>
        public string RefreshTokenExpiry { get; set; } = string.Empty;
    }
}
