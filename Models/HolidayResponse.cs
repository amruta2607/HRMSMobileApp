namespace MobileWebApi.Models
{
    /// <summary>
    /// Response model for holiday operations
    /// </summary>
    public class HolidayResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
        public int TotalRecords { get; set; }
    }

    /// <summary>
    /// Response model for a single holiday with details
    /// </summary>
    public class HolidayDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public DateTime? InsertDate { get; set; }
        public int? InsertUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUserId { get; set; }
        public int TenantId { get; set; }
        public bool IsActive { get; set; }
    }
}

