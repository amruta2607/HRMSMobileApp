namespace MobileWebApi.Models
{
	public class Alert
	{
		public int Id { get; set; }

		/// <summary>
		/// OrganisationId - maps to TenantId column in database
		/// </summary>
		public int OrganisationId { get; set; }

		public int UserId { get; set; }
		public int? EventId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
		public bool IsRead { get; set; }
		public bool IsActive { get; set; }
		public string? Status { get; set; }

		public int? InsertUserId { get; set; }
		public DateTime? UpdateDate { get; set; }
		public int? UpdateUserId { get; set; }
	}
}
