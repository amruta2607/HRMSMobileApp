namespace MobileWebApi.Models
{
    /// <summary>
    /// Represents the Holiday table in Altroz HRMS database
    /// </summary>
    public class Holiday
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
        public bool IsActive { get; set; } = true;
    }
}

