using MobileWebApi.Interfaces;
using MobileWebApi.Constants;
using MobileWebApi.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace MobileWebApi.Services
{
    /// <summary>
    /// SMS service implementation
    /// Supports multiple providers: Stub (development), Twilio, MSG91
    /// </summary>
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;
        private readonly SmsSettings _smsSettings;
        private readonly IWebHostEnvironment _environment;

        public SmsService(
            ILogger<SmsService> logger, 
            IOptions<SmsSettings> smsSettings,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _smsSettings = smsSettings.Value;
            _environment = environment;
        }

        public async Task<bool> SendOtpAsync(string mobileNumber, string otp)
        {
            try
            {
                _logger.LogInformation(LogMessages.Otp.SendingOtp, 
                    MaskMobileNumber(mobileNumber), _smsSettings.Provider);

                bool result = false;

                switch (_smsSettings.Provider.ToLower())
                {
                    case "twilio":
                        result = await SendViaTwilio(mobileNumber, otp);
                        break;
                    case "msg91":
                        result = await SendViaMsg91(mobileNumber, otp);
                        break;
                    case "stub":
                    default:
                        result = await SendViaStub(mobileNumber, otp);
                        break;
                }

                if (result)
                {
                    _logger.LogInformation(LogMessages.Otp.SmsOtpSentSuccessfully, MaskMobileNumber(mobileNumber));
                }
                else
                {
                    _logger.LogWarning(LogMessages.Otp.FailedToSendSmsOtp, MaskMobileNumber(mobileNumber));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Otp.FailedToSendSmsOtp, 
                    MaskMobileNumber(mobileNumber));
                return false;
            }
        }

        private async Task<bool> SendViaStub(string mobileNumber, string otp)
        {
            // STUB IMPLEMENTATION - Log OTP instead of sending
            _logger.LogInformation(LogMessages.Otp.StubModeSmsOtp, 
                MaskMobileNumber(mobileNumber), otp);
            
            // In development mode, also log to console
            if (_smsSettings.EnableDevelopmentMode || _environment.IsDevelopment())
            {
                Console.WriteLine($"=== OTP FOR {mobileNumber} ===: {otp}");
                Console.WriteLine($"=== OTP FOR {mobileNumber} ===: {otp}");
            }

            // Simulate async SMS sending
            await Task.Delay(100);
            return true;
        }

        private async Task<bool> SendViaTwilio(string mobileNumber, string otp)
        {
            if (_smsSettings.Twilio == null || 
                string.IsNullOrEmpty(_smsSettings.Twilio.AccountSid) ||
                string.IsNullOrEmpty(_smsSettings.Twilio.AuthToken))
            {
                _logger.LogError(LogMessages.Sms.TwilioSettingsNotConfigured);
                return false;
            }

            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                // Twilio REST API endpoint
                var accountSid = _smsSettings.Twilio.AccountSid;
                var authToken = _smsSettings.Twilio.AuthToken;
                var fromNumber = _smsSettings.Twilio.FromNumber;

                // Format mobile number: +91XXXXXXXXXX (country code + 10 digits)
                var toNumber = $"+91{mobileNumber}";
                
                // Ensure fromNumber has + prefix
                if (!fromNumber.StartsWith("+"))
                {
                    fromNumber = "+" + fromNumber;
                }

                // Twilio API endpoint
                var url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";

                // Create Basic Auth header (AccountSid:AuthToken base64 encoded)
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{accountSid}:{authToken}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

                // Message body
                var messageBody = $"Your OTP for login is {otp}. Valid for 5 minutes. Do not share this OTP with anyone.";

                // Create form data
                var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("From", fromNumber),
                    new KeyValuePair<string, string>("To", toNumber),
                    new KeyValuePair<string, string>("Body", messageBody)
                };

                var content = new FormUrlEncodedContent(formData);

                _logger.LogInformation(LogMessages.Sms.SendingSmsViaTwilio, 
                    MaskMobileNumber(mobileNumber), fromNumber);

                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(LogMessages.Sms.TwilioSmsSentSuccessfully, 
                        MaskMobileNumber(mobileNumber), responseContent);
                    return true;
                }
                else
                {
                    _logger.LogError(LogMessages.Sms.TwilioApiReturnedError, 
                        response.StatusCode, responseContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Sms.ExceptionWhileSendingSmsViaTwilio, 
                    MaskMobileNumber(mobileNumber));
                return false;
            }
        }

		private async Task<bool> SendViaMsg91(string mobileNumber, string otp)
		{
			if (_smsSettings.Msg91 == null ||
				string.IsNullOrEmpty(_smsSettings.Msg91.ApiKey) ||
				string.IsNullOrEmpty(_smsSettings.Msg91.TemplateId))
			{
				_logger.LogError(LogMessages.Sms.Msg91SettingsNotConfigured);
				return false;
			}

			try
			{
				using var httpClient = new HttpClient
				{
					Timeout = TimeSpan.FromSeconds(30)
				};

				// MSG91 OTP v5 endpoint
				var url = "https://control.msg91.com/api/v5/otp";

				// Headers
				httpClient.DefaultRequestHeaders.Add("authkey", _smsSettings.Msg91.ApiKey);
				httpClient.DefaultRequestHeaders.Accept.Add(
					new MediaTypeWithQualityHeaderValue("application/json"));

				// Payload
				var payload = new
				{
					mobile = $"91{mobileNumber}",
					otp = otp,
					otp_expiry = 5,
					template_id = _smsSettings.Msg91.TemplateId
				};

				var content = new StringContent(
					JsonSerializer.Serialize(payload),
					Encoding.UTF8,
					"application/json");

				_logger.LogInformation(
					LogMessages.Sms.SendingSmsViaMsg91,
					MaskMobileNumber(mobileNumber));

				var response = await httpClient.PostAsync(url, content);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode &&
					responseContent.Contains("\"type\":\"success\"", StringComparison.OrdinalIgnoreCase))
				{
					_logger.LogInformation(
						LogMessages.Sms.Msg91SmsSentSuccessfully,
						MaskMobileNumber(mobileNumber),
						responseContent);

					return true;
				}

				_logger.LogError(
					LogMessages.Sms.Msg91ApiReturnedError,
					response.StatusCode,
					responseContent);

				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					LogMessages.Sms.ExceptionWhileSendingSmsViaMsg91,
					MaskMobileNumber(mobileNumber));

				return false;
			}
		}

		private static string MaskMobileNumber(string mobileNumber)
        {
            if (string.IsNullOrEmpty(mobileNumber) || mobileNumber.Length <= 4)
                return "****";

            return new string('*', mobileNumber.Length - 4) + mobileNumber[^4..];
        }
    }
}
