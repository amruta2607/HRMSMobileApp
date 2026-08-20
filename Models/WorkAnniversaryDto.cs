namespace MobileWebApi.Models
{
    public class WorkAnniversaryDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime DateOfJoining { get; set; }
        public DateTime AnniversaryDate { get; set; }
        public int YearsCompleted { get; set; }
        public string? Picture { get; set; }
        public int DesignationId { get; set; }
        public int DepartmentId { get; set; }
    }
}
