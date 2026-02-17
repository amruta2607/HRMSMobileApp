namespace MobileWebApi.Models
{
	public class PaySlipDownloadByMonthYearRequest
	{
		public int UserId { get; set; }
		public int Month { get; set; }
		public int Year { get; set; }
	}
}
