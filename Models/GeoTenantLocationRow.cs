namespace MobileWebApi.Models
{
	public class GeoTenantLocationRow
	{
		public int Id { get; set; }

		public int BranchId { get; set; }

		// 6 decimal precision for GPS
		public decimal Latitude { get; set; }

		public decimal Longitude { get; set; }

		// Radius in meters
		public int Radius { get; set; }

		public int OrganisationId { get; set; }

		public string? LocationAddress { get; set; }
		public string? BranchName { get; set; }


		public bool IsActive { get; set; }

		public DateTime? InsertDate { get; set; }

		public int? InsertUserId { get; set; }

		public DateTime? UpdateDate { get; set; }

		public int? UpdateUserId { get; set; }
	}
}
