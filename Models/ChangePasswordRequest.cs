namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for change password/PIN
    /// </summary>
    public class ChangePasswordRequest
    {
        /// <summary>
        /// Current password or PIN
        /// </summary>
        public string current_password { get; set; } = string.Empty;

        /// <summary>
        /// New password or PIN
        /// </summary>
        public string new_password { get; set; } = string.Empty;
    }
}

