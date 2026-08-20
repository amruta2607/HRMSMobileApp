namespace MobileWebApi.Models
{
    /// <summary>
    /// Today's complete location tracking path for an employee.
    /// </summary>
    public class TodayLocationTrackingResponse
    {
        public int EmployeeId { get; set; }

        /// <summary>
        /// Server today's date in yyyy-MM-dd format.
        /// </summary>
        public string Date { get; set; } = string.Empty;

        public List<TodayLocationTrackingPointDto> Points { get; set; } = new();
    }

    /// <summary>
    /// A single location point on today's tracking path.
    /// </summary>
    public class TodayLocationTrackingPointDto
    {
        public int Id { get; set; }
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
