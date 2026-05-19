namespace MobileWebApi.Models
{
    /// <summary>
    /// Pay Slip entity representing data from vwPayrollDetailPrint view
    /// </summary>
    public class PaySlip
    {
		public int Id { get; set; }
		public int PayrollId { get; set; }
		public int EmployeeId { get; set; }

		// Employee Info
		public string? EmployeeName { get; set; }
		public string? EmployeeNumber { get; set; }
		public string? Email { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public DateTime? DateOfJoining { get; set; }
		public string? GenderName { get; set; }
		public string? DesignationName { get; set; }
		public string? BranchName { get; set; }

		// Tax & Statutory Info
		public string? TaxNumber { get; set; }
		public string? ESINo { get; set; }
		public string? PFNo { get; set; }
		public string? UANNo { get; set; }

		// Payroll Period
		public int PayrollMonth { get; set; }
		public int PayrollYear { get; set; }
		public string? PayrollMonthName { get; set; }
		public int FinancialYearStart { get; set; }

		// Salary Details
		public decimal BasicSalary { get; set; }
		public decimal SalarySlab { get; set; }
		public decimal SalaryEarned { get; set; }
		public decimal Gross { get; set; }
		public decimal TotalIncome { get; set; }
		public decimal TotalDeduction { get; set; }
		public decimal TakeHomePay { get; set; }

		// Working Days & Attendance
		public decimal DaysPayable { get; set; }
		public decimal PresentDays { get; set; }
		public decimal LossPayDays { get; set; }
		public decimal OverTimeDays { get; set; }
		public decimal TotalWeekOffDays { get; set; }
		// Wages Info
		public bool IsPerDayWagesEmployee { get; set; }
		public decimal PerDayWages { get; set; }
		public decimal PerDayOverTimeWages { get; set; }
		public decimal OvertimeSalary { get; set; }

		// Bank Details
		public string? BankName { get; set; }
		public string? BankAccountNumber { get; set; }
		public string? IFSCCode { get; set; }
		public string? BankBranchName { get; set; }

		// Organization Info
		public int TenantId { get; set; }
		public string? TenantName { get; set; }
		public string? Currency { get; set; }
		public string? Logo { get; set; }

		// Computed Properties for Display
		public List<PaySlipLineItem> Earnings { get; set; } = new();
		public List<PaySlipLineItem> Deductions { get; set; } = new();
		//public PaySlipAttendanceSummary AttendanceSummary => GetAttendanceSummary();
	}
}
