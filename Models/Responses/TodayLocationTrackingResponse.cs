namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Today's location tracking path for an employee (start → end chronological order).
    /// </summary>
    public class TodayLocationTrackingResponse
    {
        public int EmployeeId { get; set; }

        /// <summary>
        /// Server date used for the query (yyyy-MM-dd).
        /// </summary>
        public string Date { get; set; } = string.Empty;

        public List<LocationTrackingPointDto> Points { get; set; } = new();
    }

    /// <summary>
    /// A single GPS point on the employee's path.
    /// </summary>
    public class LocationTrackingPointDto
    {
        public int Id { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        /// <summary>Date portion (yyyy-MM-dd).</summary>
        public string Date { get; set; } = string.Empty;

        /// <summary>Time portion (HH:mm:ss).</summary>
        public string Time { get; set; } = string.Empty;

        public string? LocationFrom { get; set; }
    }
}
