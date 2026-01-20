namespace MobileWebApi.Models
{
    /// <summary>
    /// Represents the EmployeeDispute table in the database
    /// </summary>
    public class EmployeeDispute
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int DisputeCategoryId { get; set; }
        public DateTime DisputeDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedOn { get; set; }
    }
}

