namespace MobileWebApi.Models
{
    public class MonthlyPaymentSummaryResponse
    {
		
			public bool Success { get; set; }
			public string? Message { get; set; }
			public MonthlyPaymentSummary? Data { get; set; }
		}
	
}
