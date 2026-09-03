namespace MobileWebApi.Models
{
    /// <summary>
    /// Response model for dispute category list
    /// </summary>
    public class DisputeCategoryResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<DisputeCategoryDto>? Data { get; set; }
    }

    /// <summary>
    /// DTO for dispute category
    /// </summary>
    public class DisputeCategoryDto
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Response model for submitting a dispute
    /// </summary>
    public class DisputeSubmitResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public EmployeeDisputeDto? Data { get; set; }
    }

    /// <summary>
    /// DTO for employee dispute
    /// </summary>
    public class EmployeeDisputeDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int DisputeCategoryId { get; set; }
        public DateTime DisputeDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? PunchId { get; set; }
        public DateTime? RequestedPunchInTime { get; set; }
        public DateTime? RequestedPunchOutTime { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

