namespace MobileWebApi.Models
{
    /// <summary>
    /// Raw LocationTracking row projection used when reading a location tracking path.
    /// </summary>
    public class LocationTrackingPointRow
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int TenantId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public string? LocationFrom { get; set; }
    }
}
