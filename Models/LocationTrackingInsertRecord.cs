namespace MobileWebApi.Models
{
    public class LocationTrackingInsertRecord
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime TrackingDateTime { get; set; }
        public string? LocationFrom { get; set; }
    }
}
