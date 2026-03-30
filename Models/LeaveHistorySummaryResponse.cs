namespace MobileWebApi.Models
{
	public class LeaveHistorySummaryResponse
	{
		public bool Success { get; set; }
		public string? Message { get; set; }

		public int EmployeeId { get; set; }
		public int AvailableLeaves { get; set; }
		public int Year { get; set; }

		public List<LeaveHistorySummaryItem>? LeaveHistory { get; set; }
	}

	public class LeaveHistorySummaryItem
	{
		public int LeaveRequestId { get; set; }
		public string LeaveDates { get; set; } = string.Empty; // "dd-MM-yyyy" or "dd-MM-yyyy - dd-MM-yyyy"
		public string? LeaveType { get; set; }
		public int UsedDays { get; set; }
		public string? Status { get; set; }
	}
}

