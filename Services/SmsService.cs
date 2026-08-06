using MobileWebApi.Interfaces;
using MobileWebApi.Constants;
using MobileWebApi.Models;
using MobileWebApi.Helper;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace MobileWebApi.Services
{
    /// <summary>
    /// SMS service implementation.
    /// Providers: Stub, Web (SMS gateway), Twilio, MSG91.
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
                if (string.IsNullOrWhiteSpace(mobileNumber) || string.IsNullOrWhiteSpace(otp))
                {
                    _logger.LogError(LogMessages.Sms.InvalidMobileOrOtp);
                    return false;
                }

                var provider = _smsSettings.Provider ?? string.Empty;
                _logger.LogInformation(LogMessages.Otp.SendingOtp,
                    MaskMobileNumber(mobileNumber), provider);

                bool result = provider.Trim().ToLowerInvariant() switch
                {
                    "web" => await SendViaWebSms(mobileNumber, otp),
                    "twilio" => await SendViaTwilio(mobileNumber, otp),
                    "msg91" => await SendViaMsg91(mobileNumber, otp),
                    "stub" => await SendViaStub(mobileNumber, otp),
                    _ => await SendViaStub(mobileNumber, otp)
                };

                if (result)
                    _logger.LogInformation(LogMessages.Otp.SmsOtpSentSuccessfully, MaskMobileNumber(mobileNumber));
                else
                    _logger.LogWarning(LogMessages.Otp.FailedToSendSmsOtp, MaskMobileNumber(mobileNumber));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Sms.SendOtp, nameof(SendOtpAsync), ex);
                return false;
            }
        }

        private async Task<bool> SendViaWebSms(string mobileNumber, string otp)
        {
            var s = _smsSettings.WebSms;
            if (!TryValidateWebSmsSettings(s, out var missingFields))
            {
                _logger.LogError(LogMessages.Sms.WebSmsSettingsNotConfigured, string.Join(", ", missingFields));
                return false;
            }

            try
            {
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

                var messageText = BuildMessageFromTemplate(s!.MessageTemplate, otp);
                var encodedText = Uri.EscapeDataString(messageText);
                var destinationNumber = FormatDestinationNumber(mobileNumber, s.CountryCode);

                var url =
                    $"{s.BaseUrl.TrimEnd('?', '&')}" +
                    $"?user={Uri.EscapeDataString(s.Username)}" +
                    $"&password={Uri.EscapeDataString(s.Password)}" +
                    $"&senderid={Uri.EscapeDataString(s.SenderId)}" +
                    $"&channel={Uri.EscapeDataString(s.Channel ?? string.Empty)}" +
                    $"&DCS={Uri.EscapeDataString(s.Dcs)}" +
                    $"&flashsms={Uri.EscapeDataString(s.FlashSms)}" +
                    $"&number={Uri.EscapeDataString(destinationNumber)}" +
                    $"&text={encodedText}" +
                    $"&route={Uri.EscapeDataString(s.Route)}" +
                    $"&peid={Uri.EscapeDataString(s.Peid)}" +
                    $"&DLTTemplateId={Uri.EscapeDataString(s.DltTemplateId)}";

                _logger.LogInformation(
                    LogMessages.Sms.WebSmsRequestPayload,
                    MaskUrlPassword(url),
                    s.Username,
                    s.SenderId,
                    s.Channel,
                    s.Route,
                    s.Peid,
                    s.DltTemplateId,
                    MaskMobileNumber(destinationNumber),
                    MaskOtpInText(messageText, otp));

                _logger.LogInformation(LogMessages.Sms.WebSmsCallingApi, MaskUrlPassword(url));

                var response = await httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                _logger.LogInformation(
                    LogMessages.Sms.WebSmsApiResponse,
                    (int)response.StatusCode,
                    body);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        LogMessages.Sms.WebSmsApiHttpError,
                        (int)response.StatusCode,
                        body);
                    return false;
                }

                if (!TryParseWebSmsSuccess(body, out var errorCode, out var errorMessage, out var jobId))
                {
                    _logger.LogError(
                        LogMessages.Sms.WebSmsApiBusinessError,
                        errorCode ?? "N/A",
                        errorMessage ?? "Unknown error or unparseable response",
                        body);
                    return false;
                }

                _logger.LogInformation(
                    LogMessages.Sms.WebSmsSentSuccessfully,
                    MaskMobileNumber(destinationNumber),
                    jobId ?? string.Empty);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Sms.SendViaWeb, nameof(SendViaWebSms), ex);
                return false;
            }
        }

        private static bool TryValidateWebSmsSettings(WebSmsSettings? settings, out List<string> missingFields)
        {
            missingFields = new List<string>();

            if (settings == null)
            {
                missingFields.Add("SmsSettings:WebSms");
                return false;
            }

            if (string.IsNullOrWhiteSpace(settings.BaseUrl)) missingFields.Add(nameof(settings.BaseUrl));
            if (string.IsNullOrWhiteSpace(settings.Username)) missingFields.Add(nameof(settings.Username));
            if (string.IsNullOrWhiteSpace(settings.Password)) missingFields.Add(nameof(settings.Password));
            if (string.IsNullOrWhiteSpace(settings.SenderId)) missingFields.Add(nameof(settings.SenderId));
            if (string.IsNullOrWhiteSpace(settings.Route)) missingFields.Add(nameof(settings.Route));
            if (string.IsNullOrWhiteSpace(settings.Peid)) missingFields.Add(nameof(settings.Peid));
            if (string.IsNullOrWhiteSpace(settings.DltTemplateId)) missingFields.Add(nameof(settings.DltTemplateId));
            if (string.IsNullOrWhiteSpace(settings.MessageTemplate)) missingFields.Add(nameof(settings.MessageTemplate));

            return missingFields.Count == 0;
        }

        private static string BuildMessageFromTemplate(string template, string otp)
        {
            return template
                .Replace("{#var#}", otp, StringComparison.Ordinal)
                .Replace("{OTP}", otp, StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatDestinationNumber(string mobileNumber, string? countryCode)
        {
            var digits = new string(mobileNumber.Where(char.IsDigit).ToArray());
            var cc = string.IsNullOrWhiteSpace(countryCode) ? "91" : countryCode.Trim();

            if (digits.StartsWith(cc, StringComparison.Ordinal) && digits.Length > 10)
                return digits;

            if (digits.Length == 10)
                return cc + digits;

            return digits;
        }

        private static bool TryParseWebSmsSuccess(
            string body,
            out string? errorCode,
            out string? errorMessage,
            out string? jobId)
        {
            errorCode = null;
            errorMessage = null;
            jobId = null;

            if (string.IsNullOrWhiteSpace(body))
                return false;

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (root.TryGetProperty("ErrorCode", out var codeProp))
                    errorCode = codeProp.GetString();
                if (root.TryGetProperty("ErrorMessage", out var msgProp))
                    errorMessage = msgProp.GetString();
                if (root.TryGetProperty("JobId", out var jobProp))
                    jobId = jobProp.GetString();

                // Gateway success: ErrorCode "000"
                return string.Equals(errorCode, "000", StringComparison.Ordinal);
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static string MaskUrlPassword(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            const string marker = "password=";
            var start = url.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (start < 0)
                return url;

            start += marker.Length;
            var end = url.IndexOf('&', start);
            if (end < 0)
                end = url.Length;

            return string.Concat(url.AsSpan(0, start), "***", url.AsSpan(end));
        }

        private static string MaskOtpInText(string text, string otp)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(otp))
                return text;

            return text.Replace(otp, "******", StringComparison.Ordinal);
        }

        private async Task<bool> SendViaStub(string mobileNumber, string otp)
        {
            _logger.LogInformation(LogMessages.Otp.StubModeSmsOtp,
                MaskMobileNumber(mobileNumber), otp);

            if (_smsSettings.EnableDevelopmentMode || _environment.IsDevelopment())
            {
                Console.WriteLine($"=== OTP FOR {mobileNumber} ===: {otp}");
            }

            await Task.Delay(100);
            return true;
        }

        private async Task<bool> SendViaTwilio(string mobileNumber, string otp)
        {
            if (_smsSettings.Twilio == null ||
                string.IsNullOrEmpty(_smsSettings.Twilio.AccountSid) ||
                string.IsNullOrEmpty(_smsSettings.Twilio.AuthToken) ||
                string.IsNullOrWhiteSpace(_smsSettings.Twilio.FromNumber))
            {
                _logger.LogError(LogMessages.Sms.TwilioSettingsNotConfigured);
                return false;
            }

            try
            {
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

                var accountSid = _smsSettings.Twilio.AccountSid;
                var authToken = _smsSettings.Twilio.AuthToken;
                var fromNumber = _smsSettings.Twilio.FromNumber;
                var toNumber = $"+91{mobileNumber}";

                if (!fromNumber.StartsWith("+"))
                    fromNumber = "+" + fromNumber;

                var url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{accountSid}:{authToken}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

                var messageBody = $"Your OTP for login is {otp}. Valid for 5 minutes. Do not share this OTP with anyone.";
                var formData = new List<KeyValuePair<string, string>>
                {
                    new("From", fromNumber),
                    new("To", toNumber),
                    new("Body", messageBody)
                };

                _logger.LogInformation(LogMessages.Sms.SendingSmsViaTwilio,
                    MaskMobileNumber(mobileNumber), fromNumber);

                var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(formData));
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(LogMessages.Sms.TwilioSmsSentSuccessfully,
                        MaskMobileNumber(mobileNumber), responseContent);
                    return true;
                }

                _logger.LogError(LogMessages.Sms.TwilioApiReturnedError,
                    response.StatusCode, responseContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Sms.SendViaTwilio, nameof(SendViaTwilio), ex);
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
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

                var url = "https://control.msg91.com/api/v5/otp";
                httpClient.DefaultRequestHeaders.Add("authkey", _smsSettings.Msg91.ApiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var payload = new
                {
                    mobile = $"91{mobileNumber}",
                    otp,
                    otp_expiry = 5,
                    template_id = _smsSettings.Msg91.TemplateId
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                _logger.LogInformation(LogMessages.Sms.SendingSmsViaMsg91, MaskMobileNumber(mobileNumber));

                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode &&
                    responseContent.Contains("\"type\":\"success\"", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation(LogMessages.Sms.Msg91SmsSentSuccessfully,
                        MaskMobileNumber(mobileNumber), responseContent);
                    return true;
                }

                _logger.LogError(LogMessages.Sms.Msg91ApiReturnedError,
                    response.StatusCode, responseContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Sms.SendViaMsg91, nameof(SendViaMsg91), ex);
                return false;
            }
        }

        private static string MaskMobileNumber(string? mobile)
        {
            if (string.IsNullOrEmpty(mobile) || mobile.Length < 4)
                return "****";

            return mobile.Substring(0, 2) + "******" + mobile.Substring(mobile.Length - 2);
        }
    }
}
