using System.Text.Json.Serialization;

namespace MobileWebApi.Models
{
    /// <summary>
    /// Login request model for Altroz Web Application.
    /// Prefer <see cref="UsernameOrEmail"/>. Legacy <see cref="Username"/> is still accepted.
    /// </summary>
    public class WebLoginRequest
    {
        /// <summary>
        /// Username or email address (case-insensitive).
        /// </summary>
        public string? UsernameOrEmail { get; set; }

        /// <summary>
        /// Legacy username field. If <see cref="UsernameOrEmail"/> is empty, this value is used.
        /// </summary>
        public string? Username { get; set; }

        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Resolves the effective login identifier from UsernameOrEmail, falling back to Username.
        /// </summary>
        [JsonIgnore]
        public string ResolvedUsernameOrEmail => GetUsernameOrEmail();

        /// <summary>
        /// Resolves the effective login identifier from UsernameOrEmail, falling back to Username.
        /// </summary>
        public string GetUsernameOrEmail()
        {
            if (!string.IsNullOrWhiteSpace(UsernameOrEmail))
                return UsernameOrEmail.Trim();

            if (!string.IsNullOrWhiteSpace(Username))
                return Username.Trim();

            return string.Empty;
        }
    }
}
