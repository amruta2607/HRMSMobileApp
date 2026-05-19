
namespace MobileWebApi.Repositories
{
	public class PaySlipWithWeekOff
	{
		public int EmployeeId { get; set; }
		public int TenantId { get; set; }
		public string EmployeeName { get; set; }
		public double BasicSalary { get; set; }
		public double TotalIncome { get; set; }
		public double TotalDeduction { get; set; }
		public double TakeHomePay { get; set; }
		public short PayrollMonth { get; set; }
		public short PayrollYear { get; set; }

		// Add total week off days here
		public int TotalWeekOffDays { get; set; }
        public int Id { get; internal set; }
        public int PayrollId { get; internal set; }
        public string EmployeeNumber { get; internal set; }
        public string Email { get; internal set; }
        public string GenderName { get; internal set; }
        public string DesignationName { get; internal set; }
        public string BranchName { get; internal set; }
        public DateTime? DateOfBirth { get; internal set; }
        public DateTime? DateOfJoining { get; internal set; }
        public string TaxNumber { get; internal set; }
        public string ESINo { get; internal set; }
        public string PFNo { get; internal set; }
        public string UANNo { get; internal set; }
        public string PayrollMonthName { get; internal set; }
        public int FinancialYearStart { get; internal set; }
        public decimal SalarySlab { get; internal set; }
        public decimal SalaryEarned { get; internal set; }
        public decimal Gross { get; internal set; }
        public decimal DaysPayable { get; internal set; }
        public decimal PresentDays { get; internal set; }
        public decimal LossPayDays { get; internal set; }
        public decimal OverTimeDays { get; internal set; }
        public bool IsPerDayWagesEmployee { get; internal set; }
        public decimal PerDayWages { get; internal set; }
        public decimal OvertimeSalary { get; internal set; }
        public decimal PerDayOverTimeWages { get; internal set; }
        public string BankName { get; internal set; }
        public string? BankAccountNumber { get; internal set; }
        public string IFSCCode { get; internal set; }
        public string BankBranchName { get; internal set; }
        public string TenantName { get; internal set; }
        public string? Currency { get; internal set; }
        public string Logo { get; internal set; }
    }
}