namespace MobileWebApi.Models
{
	public class MonthlyPaymentSummary
	{
		public decimal BasicSalary { get; set; }
		public decimal Gross { get; set; }
		public decimal TotalIncome { get; set; }
		public decimal TotalDeduction { get; set; }
		public decimal TakeHomePay { get; set; }

		public List<IncomeItem> Incomes { get; set; } = new();
		public List<DeductionItem> Deductions { get; set; } = new();
	}

	public class IncomeItem
	{
		public string Name { get; set; } = string.Empty;
		public decimal Amount { get; set; }
	}

	public class DeductionItem
	{
		public string Name { get; set; } = string.Empty;
		public string DeductionCode { get; set; } = string.Empty;
		public decimal Amount { get; set; }
	}
}
