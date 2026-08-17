namespace MobileWebApi.Models
{
    /// <summary>
    /// Response model for leave request operations
    /// </summary>
    public class LeaveRequestResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
        public int TotalRecords { get; set; }
    }

    /// <summary>
    /// Response model for a single leave request with details
    /// </summary>
    public class LeaveRequestDetailResponse
    {
        public int Id { get; set; }
        public string? Number { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public int LeaveTypeId { get; set; }
        public string? LeaveTypeName { get; set; }
        public decimal? LeaveBalance { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal Duration { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? CurrentAction { get; set; }
        public DateTime? InsertDate { get; set; }
    }

    /// <summary>
    /// Response model for leave balance
    /// </summary>
    public class LeaveBalanceResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<LeaveBalanceDetail>? Data { get; set; }
    }

    /// <summary>
    /// Leave balance detail for a specific leave type
    /// </summary>
    public class LeaveBalanceDetail
    {
        public int LeaveTypeId { get; set; }
        public string? LeaveTypeName { get; set; }
        public decimal TotalBalance { get; set; }
       
        public decimal RemainingBalance { get; set; }
    }

    public class LeaveHistoryResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int LeavesAvailed { get; set; }
        public int Year { get; set; }
        public List<LeaveHistoryItem>? Data { get; set; }
    }

    public class LeaveHistoryItem
    {
        public DateTime LeaveDate { get; set; }
        public string? LeaveType { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
    }
}

