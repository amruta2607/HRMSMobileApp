namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for forgot password/PIN - Step 1: Request OTP
    /// </summary>
    public class ForgotPasswordRequest
    {
        /// <summary>
        /// Email or Mobile Number
        /// </summary>
        public string email { get; set; } = string.Empty;
    }
}



