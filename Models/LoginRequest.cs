namespace MobileWebApi.Models
{
    /// <summary>
    /// Login request model for Email and Password login
    /// </summary>
    public class EmailLoginRequest
    {
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Login request model for Mobile Number and OTP login
    /// If OTP is empty/null, sends OTP. If OTP is provided, verifies and logs in.
    /// </summary>
    public class MobileLoginRequest
    {
        public string mobileNumber { get; set; } = string.Empty;
        public string? otp { get; set; }
    }
}
