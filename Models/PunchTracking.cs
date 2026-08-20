namespace MobileWebApi.Models
{
    /// <summary>
    /// Entity representing a single punch in/out event in the PunchTracking table.
    /// </summary>
    public class PunchTracking
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int EmployeeId { get; set; }
        public int PunchId { get; set; }
        public DateTime PunchDate { get; set; }
        public string Direction { get; set; } = string.Empty;
        public DateTime? PunchIn { get; set; }
        public DateTime? PunchOut { get; set; }
        public double? Duration { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUserId { get; set; }
        public string? InSource { get; set; }
        public string? OutSource { get; set; }
        public string? CoordinateIn { get; set; }
        public string? CoordinateOut { get; set; }
        public string? LinkIn { get; set; }
        public string? LinkOut { get; set; }
		public string? PunchInImage { get; set; }
		public string? PunchOutImage { get; set; }
		public bool? Manual { get; set; }
        public string? PunchOutReason { get; set; }
    }
}
