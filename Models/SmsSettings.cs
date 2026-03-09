namespace MobileWebApi.Models
{
	public class SmsSettings
	{
		public string Provider { get; set; } = "Stub";
		public bool EnableDevelopmentMode { get; set; } = true;

		public TwilioSettings? Twilio { get; set; }
		public Msg91Settings? Msg91 { get; set; }

		public WebSmsSettings? WebSms { get; set; }   // WebSMS Provider
	}

	public class WebSmsSettings
	{
		public string BaseUrl { get; set; } = string.Empty;
		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string SenderId { get; set; } = string.Empty;
		public string? Channel { get; set; } 

		public string Dcs { get; set; } = "0";        // Added
		public string FlashSms { get; set; } = "0";   // Added

		public string Route { get; set; } = string.Empty;
		public string Peid { get; set; } = string.Empty;
		public string DltTemplateId { get; set; } = string.Empty;

		public string MessageTemplate { get; set; } = string.Empty;
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
		public string TemplateId { get; set; } = string.Empty;

	}
}
