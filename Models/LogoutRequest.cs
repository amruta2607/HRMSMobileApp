namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for logout operation
    /// </summary>
    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}

