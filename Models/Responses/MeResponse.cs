namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Profile and work-role information for the currently authenticated user.
    /// </summary>
    public class MeResponse
    {
        /// <summary>
        /// Authenticated user identifier.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Tenant / organisation identifier from JWT.
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// Linked employee identifier, if mapped.
        /// </summary>
        public int? EmployeeId { get; set; }

        /// <summary>
        /// Login username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Display name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Mobile number.
        /// </summary>
        public string Mobile { get; set; } = string.Empty;

        /// <summary>
        /// Assigned work roles including the default User role.
        /// </summary>
        public List<WorkRoleDto> WorkRoles { get; set; } = new();

        /// <summary>
        /// Highest effective work role (SuperAdmin &gt; Admin &gt; User).
        /// </summary>
        public string PrimaryWorkRole { get; set; } = "User";
    }
}
