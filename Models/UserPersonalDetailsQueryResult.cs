namespace MobileWebApi.Models
{
    /// <summary>
    /// Query result for personal details built from Users + WorkRole (non-employee users).
    /// </summary>
    public class UserPersonalDetailsQueryResult
    {
        public int SystemUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Picture { get; set; }
        public string? Designation { get; set; }
        public string? Department { get; set; }
    }
}
