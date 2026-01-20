namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for sending OTP
    /// </summary>
    public class SendOtpRequest
    {
        public string mobileNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model for sending OTP
    /// </summary>
    public class SendOtpResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public int? resendAfterSeconds { get; set; }
    }

    /// <summary>
    /// Request model for verifying OTP
    /// </summary>
    public class VerifyOtpRequest
    {
        public string mobileNumber { get; set; } = string.Empty;
        public string otp { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model for verifying OTP and login
    /// </summary>
    public class VerifyOtpResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public string? token { get; set; }
        public UserData? user { get; set; }
    }

    /// <summary>
    /// Unified request model for mobile login (matches UI)
    /// If OTP is empty/null, sends OTP. If OTP is provided, verifies and logs in.
    /// </summary>
   

    /// <summary>
    /// Unified response model for mobile login
    /// </summary>
    public class MobileLoginResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public string? token { get; set; }
        public UserData? user { get; set; }
        public int? resendAfterSeconds { get; set; }
        public bool otpSent { get; set; }
    }

    /// <summary>
    /// User data returned after successful OTP verification
    /// </summary>
    public class UserData
    {
        public int employeeId { get; set; }
        public int tenantId { get; set; }
        public string name { get; set; } = string.Empty;
    }
}
