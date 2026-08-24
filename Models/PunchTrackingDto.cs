namespace MobileWebApi.Models
{
    /// <summary>
    /// Data transfer object for a punch tracking record.
    /// </summary>
    public class PunchTrackingDto
    {
        public int Id { get; set; }
        public int PunchId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime PunchDate { get; set; }
        public string Direction { get; set; } = string.Empty;
        public DateTime? PunchIn { get; set; }
        public DateTime? PunchOut { get; set; }
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
