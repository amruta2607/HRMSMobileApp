using System.ComponentModel.DataAnnotations.Schema;

namespace MobileWebApi.Models
{
	[Table("Users")]
	public class User
	{
		public int UserId { get; set; }
		public string Username { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string PinNumber { get; set; }
        public string MobileNumber { get; set; }
        public string? PasswordSalt { get; internal set; }
		
        /// <summary>
        /// OrganisationId - maps to TenantId column in database
        /// </summary>
        public int OrganisationId { get; set; }
        
        public string DisplayName { get; internal set; }
        public string Email { get; internal set; }
        public bool IsHrUser { get; internal set; }
        public bool IsTenantAdmin { get; internal set; }
        public int WorkRoleId { get; internal set; }
		public string WorkRoleName { get; set; } = string.Empty;

        public bool IsActive { get; internal set; }
	}
}

