namespace MobileWebApi.Interfaces
{
    /// <summary>
    /// SMS service interface for sending OTP via SMS
    /// </summary>
    public interface ISmsService
    {
        /// <summary>
        /// Sends OTP to the specified mobile number
        /// </summary>
        /// <param name="mobileNumber">Mobile number (10 digits)</param>
        /// <param name="otp">OTP to send</param>
        /// <returns>True if sent successfully, false otherwise</returns>
        Task<bool> SendOtpAsync(string mobileNumber, string otp);
    }
}
