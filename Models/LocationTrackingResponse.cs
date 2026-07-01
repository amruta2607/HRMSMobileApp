namespace MobileWebApi.Models
{
    /// <summary>
    /// Response model for location tracking API.
    /// </summary>
    public class LocationTrackingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
