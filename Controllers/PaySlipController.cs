using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;

namespace MobileWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaySlipController : TenantBaseController
    {
        private readonly IPaySlipService _paySlipService;

        public PaySlipController(
            IPaySlipService paySlipService, 
            ITenantContext tenantContext,
            ILogger<PaySlipController> logger)
            : base(tenantContext, logger)
        {
            _paySlipService = paySlipService;
        }

        /// <summary>
        /// Get list of pay slips for a user
        /// POST: api/payslip/list
        /// Note: Regular users can only see their own payslips. HR/TenantAdmin can see all.
        /// </summary>
        /// <param name="request">Filter parameters from mobile app</param>
        /// <returns>List of pay slips</returns>
        //[HttpPost("list")]
        //public async Task<IActionResult> GetPaySlips([FromBody] PaySlipListRequest request)
        //{
        //    // Validate user access - regular users can only see their own payslips
        //    try
        //    {
        //        request.user = GetValidatedUserId(request.user);
        //    }
        //    catch (Services.TenantAccessException)
        //    {
        //        return UserAccessDenied();
        //    }

        //    Logger.LogInformation(LogMessages.PaySlip.FetchingPaySlips, request.user);
        //    var result = await _paySlipService.GetPaySlipsAsync(request);

        //    if (result.Success)
        //    {
        //        return Ok(result);
        //    }

        //    return BadRequest(result);
        //}

        /// <summary>
        /// Get list of pay slips using GET method with query parameters
        /// GET: api/payslip
        /// Note: Regular users can only see their own payslips. HR/TenantAdmin can see all.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPaySlipsGet(
            [FromQuery] int user,
            [FromQuery] int? organization = null,
            [FromQuery] int? year = null,
            [FromQuery] int? month = null)
        {
            // Validate tenant access - use user's org if not specified
            var validatedOrgId = GetValidatedOrganisationId(organization);
            
            // Validate user access - regular users can only see their own payslips
            int validatedUserId;
            try
            {
                validatedUserId = GetValidatedUserId(user);
            }
            catch (Services.TenantAccessException)
            {
                return UserAccessDenied();
            }
            
            Logger.LogInformation(LogMessages.PaySlip.FetchingPaySlips, validatedUserId);

            var request = new PaySlipListRequest
            {
                user = validatedUserId,
                organization = validatedOrgId,
                year = year,
                month = month
            };

            var result = await _paySlipService.GetPaySlipsAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get detailed pay slip by ID
        /// GET: api/payslip/{id}
        /// Note: Regular users can only see their own payslips. HR/TenantAdmin can see all.
        /// </summary>
        /// <param name="id">Pay slip ID</param>
        /// <param name="user">User ID</param>
        /// <returns>Pay slip details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaySlipById(int id, [FromQuery] int user)
        {
            // Validate user access - regular users can only see their own payslips
            int validatedUserId;
            try
            {
                validatedUserId = GetValidatedUserId(user);
            }
            catch (Services.TenantAccessException)
            {
                return UserAccessDenied();
            }

            Logger.LogInformation(LogMessages.PaySlip.FetchingPaySlipById, id);
            var result = await _paySlipService.GetPaySlipByIdAsync(validatedUserId, id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

		/// <summary>
		/// Get pay slip by ID using POST
		/// POST: api/payslip/get
		/// Note: Regular users can only see their own payslips. HR/TenantAdmin can see all.
		/// </summary>
		/// <param name="request">Pay slip request details</param>
		/// <returns>Pay slip details</returns>
		//[HttpPost("get")]
		//public async Task<IActionResult> GetPaySlip([FromBody] PaySlipGetRequest request)
		//{
		//    // Validate user access - regular users can only see their own payslips
		//    try
		//    {
		//        request.user = GetValidatedUserId(request.user);
		//    }
		//    catch (Services.TenantAccessException)
		//    {
		//        return UserAccessDenied();
		//    }

		//    Logger.LogInformation(LogMessages.PaySlip.FetchingPaySlipById, request.payslip_id);
		//    var result = await _paySlipService.GetPaySlipByIdAsync(request.user, request.payslip_id);

		//    if (result.Success)
		//    {
		//        return Ok(result);
		//    }

		//    return NotFound(result);
		//}

		/// <summary>
		/// Download pay slip data for PDF generation
		/// POST: api/payslip/download
		/// Note: Regular users can only download their own payslips. HR/TenantAdmin can download all.
		/// </summary>
		/// <param name="request">Download request</param>
		/// <returns>Pay slip data</returns>
		//[HttpPost("download")]
		//public async Task<IActionResult> DownloadPaySlip([FromBody] PaySlipDownloadRequest request)
		//{
		//    // Validate user access - regular users can only download their own payslips
		//    try
		//    {
		//        request.user = GetValidatedUserId(request.user);
		//    }
		//    catch (Services.TenantAccessException)
		//    {
		//        return UserAccessDenied();
		//    }

		//    Logger.LogInformation(LogMessages.PaySlip.DownloadingPaySlip, request.payslip_id);
		//    var result = await _paySlipService.DownloadPaySlipAsync(request);

		//    if (result.Success)
		//    {
		//        // If file content exists, return as file
		//        if (result.FileContent != null)
		//        {
		//            return File(result.FileContent, result.ContentType ?? "application/pdf", result.FileName);
		//        }

		//        // Otherwise return payslip data for client-side PDF generation
		//        return Ok(new PaySlipResponse
		//        {
		//            Success = true,
		//            Message = result.Message,
		//            Data = result.PaySlipData,
		//            TotalRecords = 1
		//        });
		//    }

		//    return NotFound(new PaySlipResponse
		//    {
		//        Success = false,
		//        Message = result.Message,
		//        Data = null,
		//        TotalRecords = 0
		//    });
		//}

		/// <summary>
		/// Download pay slip data using GET method
		/// GET: api/payslip/{id}/download
		/// Note: Regular users can only download their own payslips. HR/TenantAdmin can download all.
		/// </summary>
		/// <param name="id">Pay slip ID</param>
		/// <param name="user">User ID</param>
		/// <param name="format">Download format (pdf/excel)</param>
		/// <returns>Pay slip data</returns>
		/// <summary>
		/// Download pay slip using Month & Year
		/// GET: api/payslip/download?user=5&month=4&year=2025
		/// </summary>
		[HttpGet("download")]
		public async Task<IActionResult> DownloadPaySlipByMonthYear(
			[FromQuery] int user,
			[FromQuery] int month,
			[FromQuery] int year)
		{
			int validatedUserId;

			try
			{
				validatedUserId = GetValidatedUserId(user);
			}
			catch
			{
				return UserAccessDenied();
			}

			var request = new PaySlipDownloadByMonthYearRequest
			{
				UserId = validatedUserId,
				Month = month,
				Year = year
			};

			var result = await _paySlipService
				.DownloadPaySlipByMonthYearAsync(request);

			if (result.Success)
				return Ok(result);

			return NotFound(result);
		}
		/// <summary>
		/// Get Employee Provident Fund Summary
		/// GET: api/payslip/provident-fund?user=5
		/// </summary>
		[HttpGet("provident-fund")]
		public async Task<IActionResult> GetProvidentFund([FromQuery] int user)
		{
			int validatedUserId;

			try
			{
				validatedUserId = GetValidatedUserId(user);
			}
			catch (Services.TenantAccessException)
			{
				return UserAccessDenied();
			}

			var result = await _paySlipService
				.GetProvidentFundSummaryAsync(validatedUserId);

			if (result.Success)
				return Ok(result);

			return BadRequest(result);
		}
		[HttpGet("monthly-summary")]
		public async Task<IActionResult> GetMonthlyPaymentSummary(
	[FromQuery] int user,
	[FromQuery] int month,
	[FromQuery] int year)
		{
			int validatedUserId;

			try
			{
				validatedUserId = GetValidatedUserId(user);
			}
			catch
			{
				return UserAccessDenied();
			}

			var request = new MonthlyPaymentSummaryRequest
			{
				UserId = validatedUserId,
				Month = month,
				Year = year
			};

			var result = await _paySlipService
				.GetMonthlyPaymentSummaryAsync(request);

			if (result.Success)
				return Ok(result);

			return BadRequest(result);
		}
	}

}

