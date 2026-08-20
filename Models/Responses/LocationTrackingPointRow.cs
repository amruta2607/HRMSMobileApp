namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Raw LocationTracking row used by the repository before formatting for the API response.
    /// Date/Time are already formatted by SQL as yyyy-MM-dd / HH:mm:ss.
    /// </summary>
    public class LocationTrackingPointRow
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string? LocationFrom { get; set; }
    }
}
