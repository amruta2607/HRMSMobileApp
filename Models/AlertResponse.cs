namespace MobileWebApi.Models
{
    public class AlertResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Alert? Data { get; set; }
    }

    public class AlertListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Alert>? Data { get; set; }
        public int TotalRecords { get; set; }
        public int UnreadCount { get; set; }
    }
}

