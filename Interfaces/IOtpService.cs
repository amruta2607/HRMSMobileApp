namespace MobileWebApi.Interfaces
{
    public interface IOtpService
    {
        /// <summary>
        /// Generates and stores an OTP for the given identifier (username/mobile/email)
        /// </summary>
        string GenerateOtp(string identifier);

        /// <summary>
        /// Generates and stores an OTP for mobile number with hashing, resend cooldown, and rate limiting
        /// Returns the plain OTP for sending via SMS
        /// </summary>
        (string otp, int resendAfterSeconds, bool canSend) GenerateMobileOtp(string mobileNumber);

        /// <summary>
        /// Validates the OTP for the given identifier
        /// </summary>
        bool ValidateOtp(string identifier, string otp);

        /// <summary>
        /// Validates mobile OTP with hashing
        /// </summary>
        bool ValidateMobileOtp(string mobileNumber, string otp);

        /// <summary>
        /// Removes the OTP after successful validation or expiry
        /// </summary>
        void RemoveOtp(string identifier);

        /// <summary>
        /// Removes mobile OTP from cache
        /// </summary>
        void RemoveMobileOtp(string mobileNumber);

        /// <summary>
        /// Gets resend cooldown time remaining in seconds
        /// </summary>
        int GetResendCooldownSeconds(string mobileNumber);
    }
}



