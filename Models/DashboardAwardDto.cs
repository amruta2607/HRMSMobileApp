namespace MobileWebApi.Models
{
    public class DashboardAwardDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public string? Reward { get; set; }
        public string? Achievement { get; set; }
        public int? AwardeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? Picture { get; set; }
        public int? BranchId { get; set; }
        public int? DepartmentId { get; set; }
    }
}
