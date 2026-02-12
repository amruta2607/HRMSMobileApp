using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IPaySlipService
    {
        /// <summary>
        /// Get list of monthly pay slips for the user
        /// </summary>
        Task<PaySlipResponse> GetPaySlipsAsync(PaySlipListRequest request);
        
        /// <summary>
        /// Get detailed pay slip by ID
        /// </summary>
        Task<PaySlipResponse> GetPaySlipByIdAsync(int userId, int paySlipId);
        
        /// <summary>
        /// Get pay slip for download
        /// </summary>
        Task<PaySlipDownloadResponse> DownloadPaySlipAsync(PaySlipDownloadRequest request);
        Task<PaySlipResponse> GetProvidentFundSummaryAsync(int validatedUserId);
		Task<MonthlyPaymentSummaryResponse> GetMonthlyPaymentSummaryAsync(MonthlyPaymentSummaryRequest request);
	}
}




