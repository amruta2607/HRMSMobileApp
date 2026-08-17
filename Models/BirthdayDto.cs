namespace MobileWebApi.Models
{
    public class BirthdayDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime BirthdayDate { get; set; }
        public string? Picture { get; set; }
        public int DesignationId { get; set; }
        public int DepartmentId { get; set; }
    }
}
