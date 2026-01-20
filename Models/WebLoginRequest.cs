namespace MobileWebApi.Models
{
    /// <summary>
    /// Login request model for Altroz Web Application
    /// </summary>
    public class WebLoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

