namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for reset password/PIN - Step 2: Verify OTP and set new password
    /// </summary>
    public class ResetPasswordRequest
    {
        /// <summary>
        /// Email or Mobile Number
        /// </summary>
        public string email { get; set; } = string.Empty;

        /// <summary>
        /// OTP received via SMS/Email
        /// </summary>
        public string otp { get; set; } = string.Empty;

        /// <summary>
        /// New password or PIN number
        /// </summary>
        public string new_password { get; set; } = string.Empty;
    }
}



