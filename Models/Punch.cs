namespace MobileWebApi.Models
{
    public class Punch
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime PunchDate { get; set; }
        public DateTime? PunchIn { get; set; }
        public DateTime? PunchOut { get; set; }
        public double? Duration { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUserId { get; set; }
        
        /// <summary>
        /// OrganisationId - maps to TenantId column in database
        /// </summary>
        public int OrganisationId { get; set; }

        public string? InSource { get; set; }
        public string? OutSource { get; set; }
        public string? CoordinateIn { get; set; }
        public string? CoordinateOut { get; set; }
        public string? LinkIn { get; set; }
        public string? LinkOut { get; set; }
        public bool? Manual { get; set; }
        public string? PunchOutReason { get; set; }
    }
}
