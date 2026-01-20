namespace MobileWebApi.Models
{
    /// <summary>
    /// Response model for delete attendance API
    /// </summary>
    public class AttendanceDeleteResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
    }
}

