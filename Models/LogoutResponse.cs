namespace MobileWebApi.Models
{
    /// <summary>
    /// Response model for logout operation
    /// </summary>
    public class LogoutResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}

