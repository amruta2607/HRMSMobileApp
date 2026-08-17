using System.Text.Json.Serialization;

namespace MobileWebApi.Models
{
    /// <summary>
    /// Login request for username/email + password authentication (mobile and email login API).
    /// Prefer <see cref="usernameOrEmail"/>. Legacy <see cref="email"/> is still accepted.
    /// </summary>
    public class EmailLoginRequest
    {
        /// <summary>
        /// Username or email address (case-insensitive).
        /// </summary>
        public string? usernameOrEmail { get; set; }

        /// <summary>
        /// Legacy login identifier. If <see cref="usernameOrEmail"/> is empty, this value is used.
        /// </summary>
        public string? email { get; set; }

        public string password { get; set; } = string.Empty;

        /// <summary>
        /// Resolves the effective login identifier from usernameOrEmail, falling back to email.
        /// </summary>
        [JsonIgnore]
        public string ResolvedUsernameOrEmail => GetUsernameOrEmail();

        /// <summary>
        /// Resolves the effective login identifier from usernameOrEmail, falling back to email.
        /// </summary>
        public string GetUsernameOrEmail()
        {
            if (!string.IsNullOrWhiteSpace(usernameOrEmail))
                return usernameOrEmail.Trim();

            if (!string.IsNullOrWhiteSpace(email))
                return email.Trim();

            return string.Empty;
        }
    }

    /// <summary>
    /// Login request model for Mobile Number and OTP login
    /// If OTP is empty/null, sends OTP. If OTP is provided, verifies and logs in.
    /// </summary>
    public class MobileLoginRequest
    {
        public string mobileNumber { get; set; } = string.Empty;
        public string? otp { get; set; }
    }
}
