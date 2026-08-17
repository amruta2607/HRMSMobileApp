using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace MobileWebApi.Services
{
    /// <summary>
    /// Email service implementation using SMTP
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Send an email asynchronously
        /// </summary>
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    UseDefaultCredentials = _emailSettings.UseDefaultCredentials
                };

                if (!_emailSettings.UseDefaultCredentials)
                {
                    smtpClient.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                }

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                
                _logger.LogInformation(LogMessages.Email.EmailSentSuccessfully, MaskEmail(toEmail));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Email.ErrorSendingEmail, MaskEmail(toEmail));
                return false;
            }
        }

        /// <summary>
        /// Send OTP email for forgot password
        /// </summary>
        public async Task<bool> SendForgotPasswordOtpAsync(string toEmail, string userName, string otp)
        {
            var subject = EmailMessages.PasswordResetOtpSubject;
            var body = GetForgotPasswordEmailTemplate(userName, otp);

            return await SendEmailAsync(toEmail, subject, body);
        }

        /// <summary>
        /// Send password reset confirmation email
        /// </summary>
        public async Task<bool> SendPasswordResetConfirmationAsync(string toEmail, string userName)
        {
            var subject = EmailMessages.PasswordResetSuccessfulSubject;
            var body = GetPasswordResetConfirmationTemplate(userName);

            return await SendEmailAsync(toEmail, subject, body);
        }

        /// <summary>
        /// HTML template for forgot password OTP email
        /// </summary>
        private static string GetForgotPasswordEmailTemplate(string userName, string otp)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{StringConstants.EmailTitlePasswordResetOtp}</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>{StringConstants.EmailHeaderPasswordReset}</h1>
    </div>
    
    <div style='background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;'>
        <p style='font-size: 16px;'>{StringConstants.EmailGreeting} <strong>{userName}</strong>,</p>
        
        <p style='font-size: 16px;'>{StringConstants.EmailOtpRequestMessage}</p>
        
        <div style='background: #f8f9fa; border: 2px dashed #667eea; border-radius: 10px; padding: 20px; text-align: center; margin: 30px 0;'>
            <p style='font-size: 14px; color: #666; margin: 0 0 10px 0;'>{StringConstants.EmailOtpLabel}</p>
            <p style='font-size: 36px; font-weight: bold; color: #667eea; letter-spacing: 8px; margin: 0;'>{otp}</p>
        </div>
        
        <p style='font-size: 14px; color: #666;'>
            <strong>{StringConstants.EmailOtpValidityMessage}</strong>
        </p>
        
        <p style='font-size: 14px; color: #666;'>
            {StringConstants.EmailOtpIgnoreMessage}
        </p>
        
        <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 30px 0;'>
        
        <p style='font-size: 12px; color: #999; text-align: center;'>
            {StringConstants.EmailAutomatedMessage}<br>
            {string.Format(StringConstants.EmailCopyrightTemplate, DateTime.Now.Year)}
        </p>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// HTML template for password reset confirmation email
        /// </summary>
        private static string GetPasswordResetConfirmationTemplate(string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{StringConstants.EmailTitlePasswordResetSuccessful}</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>{StringConstants.EmailHeaderPasswordResetSuccessful}</h1>
    </div>
    
    <div style='background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;'>
        <p style='font-size: 16px;'>{StringConstants.EmailGreeting} <strong>{userName}</strong>,</p>
        
        <p style='font-size: 16px;'>{StringConstants.EmailPasswordResetSuccessMessage}</p>
        
        <div style='background: #e8f5e9; border-left: 4px solid #4caf50; padding: 15px; margin: 20px 0;'>
            <p style='margin: 0; font-size: 14px;'>
                <strong>{StringConstants.EmailSecurityTip}</strong> {StringConstants.EmailSecurityTipMessage}
            </p>
        </div>
        
        <p style='font-size: 14px; color: #666;'>
            {StringConstants.EmailSecurityRecommendations}
        </p>
        <ul style='font-size: 14px; color: #666;'>
            <li>{StringConstants.EmailSecurityTip1}</li>
            <li>{StringConstants.EmailSecurityTip2}</li>
            <li>{StringConstants.EmailSecurityTip3}</li>
        </ul>
        
        <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 30px 0;'>
        
        <p style='font-size: 12px; color: #999; text-align: center;'>
            {StringConstants.EmailAutomatedMessage}<br>
            {string.Format(StringConstants.EmailCopyrightTemplate, DateTime.Now.Year)}
        </p>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Mask email for logging (privacy)
        /// </summary>
        private static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@'))
                return StringConstants.MaskedEmailPlaceholder;

            var parts = email.Split('@');
            if (parts[0].Length <= 2)
                return "**@" + parts[1];

            return parts[0][..2] + new string('*', parts[0].Length - 2) + "@" + parts[1];
        }
    }
}

