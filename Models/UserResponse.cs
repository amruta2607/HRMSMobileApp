namespace MobileWebApi.Models
{
   public class UserResponse
        {
            public int UserId { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string mobile_number { get; set; } = string.Empty;

        public string WorkRoleName { get; set; } = string.Empty;
            
            /// <summary>
            /// OrganisationId - maps to TenantId column in database
            /// </summary>
            public int OrganisationId { get; set; }
            
            public string Token { get; set; } = string.Empty;
        public bool IsActive { get; internal set; }

    }
}


