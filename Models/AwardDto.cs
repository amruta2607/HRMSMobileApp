namespace MobileWebApi.Models
{
    public class AwardDto
    {
        public int AwardId { get; set; }
        public string? AwardName { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public string? Reward { get; set; }
        public string? Achievement { get; set; }
        public int? AwardeeId { get; set; }
        public string? AwardeeName { get; set; }
        public string? Picture { get; set; }
        public int? BranchId { get; set; }
        public int? DepartmentId { get; set; }
    }
}
