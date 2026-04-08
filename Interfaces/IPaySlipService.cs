using MobileWebApi.Models;
using MobileWebApi.Repositories;

namespace MobileWebApi.Interfaces
{
	public interface IPaySlipService
	{
		Task<PaySlipResponse> GetPaySlipsAsync(PaySlipListRequest request);

		Task<PaySlipResponse> GetPaySlipByIdAsync(int userId, int paySlipId);

		Task<PaySlipResponse> GetProvidentFundSummaryAsync(int validatedUserId);

		Task<MonthlyPaymentSummaryResponse> GetMonthlyPaymentSummaryAsync(
			MonthlyPaymentSummaryRequest request);

		// Same response structure as GetMonthlyPaymentSummaryAsync, but returns only published payroll records.
		Task<MonthlyPaymentSummaryResponse> GetMonthlyPaymentSummaryPublishedAsync(
			MonthlyPaymentSummaryRequest request);

		Task<PaySlipDownloadResponse> DownloadPaySlipByMonthYearAsync(
			PaySlipDownloadByMonthYearRequest request);
		Task<PaySlipWithWeekOff?> GetPaySlipAsync(int employeeId, int tenantId, int month, int year);
		Task<MonthlyPaymentSummaryResponse>GetLastMonthPaymentSummaryAsync(int userId);
		Task<PaySlipYearsResponse> GetPaySlipYearsAsync(int userId);
		Task<PaySlipMonthsResponse> GetPaySlipMonthsByYearAsync(int userId, int year);
	}
}




