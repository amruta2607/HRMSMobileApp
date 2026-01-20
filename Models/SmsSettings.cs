namespace MobileWebApi.Models
{
    public class SmsSettings
    {
        public string Provider { get; set; } = "Stub";
        public bool EnableDevelopmentMode { get; set; } = true;
        public TwilioSettings? Twilio { get; set; }
        public Msg91Settings? Msg91 { get; set; }
    }

    public class TwilioSettings
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromNumber { get; set; } = string.Empty;
    }

    public class Msg91Settings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
    }
}
