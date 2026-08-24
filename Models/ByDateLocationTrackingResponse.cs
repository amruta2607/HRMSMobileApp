namespace MobileWebApi.Models
{
    /// <summary>
    /// Complete location tracking path for an employee on a specified date.
    /// </summary>
    public class ByDateLocationTrackingResponse
    {
        public bool Success { get; set; }

        /// <summary>
        /// Requested date in yyyy-MM-dd format.
        /// </summary>
        public string Date { get; set; } = string.Empty;

        public int UserId { get; set; }

        public List<ByDateLocationTrackingPointDto> Locations { get; set; } = new();
    }

    /// <summary>
    /// A single location tracking record for the requested date.
    /// </summary>
    public class ByDateLocationTrackingPointDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int TenantId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        /// <summary>
        /// Point date in yyyy-MM-dd format.
        /// </summary>
        public string Date { get; set; } = string.Empty;

        /// <summary>
        /// Point time in HH:mm:ss format.
        /// </summary>
        public string Time { get; set; } = string.Empty;

        public string? LocationFrom { get; set; }
    }
}
