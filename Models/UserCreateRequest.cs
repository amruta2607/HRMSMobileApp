namespace MobileWebApi.Models
{
    public class UserCreateRequest
    {
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string PinNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string WorkRoleName { get; set; } = string.Empty;
        
        /// <summary>
        /// Organization ID (TenantId - foreign key to Tenant table)
        /// </summary>
        public int organization { get; set; }
        
        /// <summary>
        /// Branch ID (foreign key to Branch table)
        /// </summary>
        public int branch { get; set; }
        
        public bool IsHrUser { get; set; } = false;
        public bool IsTenantAdmin { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}

