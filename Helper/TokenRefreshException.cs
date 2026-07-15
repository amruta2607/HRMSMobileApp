namespace MobileWebApi.Helper
{
    /// <summary>
    /// Thrown when refresh token validation fails (expired, revoked, reused, or mismatched).
    /// </summary>
    public class TokenRefreshException : Exception
    {
        public TokenRefreshException(string message) : base(message) { }
    }
}
