namespace MobileWebApi.Models
{
    public class DashboardBirthdayDto
    {
        public int EmployeeId { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime BirthdayDate { get; set; }
        public string? Picture { get; set; }
        public int? DepartmentId { get; set; }
        public int? BranchId { get; set; }
    }
}
