namespace MobileWebApi.Interfaces
{
    /// <summary>
    /// Interface for email sending operations
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Send an email asynchronously
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body (HTML supported)</param>
        /// <param name="isHtml">Whether the body is HTML formatted</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Send OTP email for forgot password
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="userName">User's name for personalization</param>
        /// <param name="otp">The OTP code</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        Task<bool> SendForgotPasswordOtpAsync(string toEmail, string userName, string otp);

        /// <summary>
        /// Send password reset confirmation email
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="userName">User's name for personalization</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        Task<bool> SendPasswordResetConfirmationAsync(string toEmail, string userName);
    }
}

