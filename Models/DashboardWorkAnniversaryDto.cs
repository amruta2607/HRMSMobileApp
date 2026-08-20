namespace MobileWebApi.Models
{
    public class DashboardWorkAnniversaryDto
    {
        public int EmployeeId { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime DateOfJoining { get; set; }
        public DateTime AnniversaryDate { get; set; }
        public int ServiceYears { get; set; }
        public string? Picture { get; set; }
        public int? DepartmentId { get; set; }
        public int? BranchId { get; set; }
    }
}
