using Azure.Core;
using Microsoft.EntityFrameworkCore;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Repositories;

namespace MobileWebApi.Services
{
    public class PaySlipService : IPaySlipService
    {
        private readonly IPaySlipRepository _paySlipRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<PaySlipService> _logger;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IUserService _currentUserService;

		private readonly DapperContext _context;

		public PaySlipService(
            IPaySlipRepository paySlipRepository,
            IEmployeeRepository employeeRepository,
            ILogger<PaySlipService> logger,
            IHttpContextAccessor httpContextAccessor,
            IUserService currentUserService,DapperContext context)
        {
            _paySlipRepository = paySlipRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _currentUserService = currentUserService;
			_context = context;
        }

        /// <summary>
        /// Get list of pay slips for a user (filtered by tenant/organization)
        /// </summary>
        public async Task<PaySlipResponse> GetPaySlipsAsync(PaySlipListRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.PaySlip.FetchingPaySlips, request.user);

                // Validate user ID
                if (request.user <= 0)
                {
                    return new PaySlipResponse
                    {
                        Success = false,
                        Message = PaySlipMessages.UserIdRequired,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Get employee ID and TenantId by user ID
                var (employeeId, tenantId) = await _paySlipRepository.GetEmployeeIdAndTenantByUserIdAsync(request.user);
                
                if (!employeeId.HasValue || !tenantId.HasValue)
                {
                    return new PaySlipResponse
                    {
                        Success = false,
                        Message = PaySlipMessages.EmployeeNotFoundForUser,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Get pay slips from vwPayrollDetailPrint (filtered by tenant)
                var paySlips = await _paySlipRepository.GetPaySlipsAsync(employeeId.Value, tenantId.Value, request.year, request.month);
                var paySlipList = paySlips.ToList();

                // Map to summary view for list display
                var summaryList = paySlipList.Select(ps => new PaySlipSummary
                {
                    Id = ps.Id,
                    PayrollId = ps.PayrollId,
                    PayrollMonth = ps.PayrollMonth,
                    PayrollYear = ps.PayrollYear,
                    PayrollMonthName = ps.PayrollMonthName,
                    FinancialYearStart = ps.FinancialYearStart,
                    Gross = ps.Gross,
                    TotalIncome = ps.TotalIncome,
                    TotalDeduction = ps.TotalDeduction,
                    TakeHomePay = ps.TakeHomePay,
                    Currency = ps.Currency
                }).OrderByDescending(ps => ps.PayrollYear)
                  .ThenByDescending(ps => ps.PayrollMonth)
                  .ToList();

                return new PaySlipResponse
                {
                    Success = true,
                    Message = PaySlipMessages.PaySlipsFetchedSuccessfully,
                    Data = summaryList,
                    TotalRecords = summaryList.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.PaySlip.ErrorFetchingPaySlips);
                return new PaySlipResponse
                {
                    Success = false,
                    Message = string.Format(PaySlipMessages.ErrorFetchingPaySlips, ex.Message),
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Get detailed pay slip by ID (filtered by tenant/organization)
        /// </summary>
        public async Task<PaySlipResponse> GetPaySlipByIdAsync(int userId, int paySlipId)
        {
            try
            {
                _logger.LogInformation(LogMessages.PaySlip.FetchingPaySlipById, paySlipId);

                // Validate user ID
                if (userId <= 0)
                {
                    return new PaySlipResponse
                    {
                        Success = false,
                        Message = PaySlipMessages.UserIdRequired,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Validate pay slip ID
                if (paySlipId <= 0)
                {
                    return new PaySlipResponse
                    {
                        Success = false,
                        Message = PaySlipMessages.PaySlipIdRequired,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Get employee ID and TenantId by user ID
                var (employeeId, tenantId) = await _paySlipRepository.GetEmployeeIdAndTenantByUserIdAsync(userId);
                
                if (!employeeId.HasValue || !tenantId.HasValue)
                {
                    return new PaySlipResponse
                    {
                        Success = false,
                        Message = PaySlipMessages.EmployeeNotFoundForUser,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Get pay slip from vwPayrollDetailPrint (filtered by tenant)
                var paySlip = await _paySlipRepository.GetPaySlipByIdAsync(paySlipId, tenantId.Value);

                if (paySlip == null)
                {
                    return new PaySlipResponse
                    {
                        Success = false,
                        Message = PaySlipMessages.PaySlipNotFound,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Verify the pay slip belongs to the user's employee
                if (paySlip.EmployeeId != employeeId.Value)
                {
                    return new PaySlipResponse
                    {
                        Success = false,
                        Message = PaySlipMessages.UnauthorizedAccess,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Map to detailed view
                var detail = new PaySlipDetail
                {
                    Id = paySlip.Id,
                    PayrollId = paySlip.PayrollId,
                    EmployeeId = paySlip.EmployeeId,
                    
                    // Employee Info
                    EmployeeName = paySlip.EmployeeName,
                    EmployeeNumber = paySlip.EmployeeNumber,
                    Email = paySlip.Email,
                    DateOfBirth = paySlip.DateOfBirth,
                    DateOfJoining = paySlip.DateOfJoining,
                    GenderName = paySlip.GenderName,
                    DesignationName = paySlip.DesignationName,
                    BranchName = paySlip.BranchName,
                    
                    // Tax & Statutory Info
                    TaxNumber = paySlip.TaxNumber,
                    ESINo = paySlip.ESINo,
                    PFNo = paySlip.PFNo,
                    UANNo = paySlip.UANNo,
                    
                    // Payroll Period
                    PayrollMonth = paySlip.PayrollMonth,
                    PayrollYear = paySlip.PayrollYear,
                    PayrollMonthName = paySlip.PayrollMonthName,
                    FinancialYearStart =(int) paySlip.FinancialYearStart,
                    
                    // Salary Details
                    BasicSalary = paySlip.BasicSalary,
                    SalarySlab = paySlip.SalarySlab,
                    SalaryEarned = paySlip.SalaryEarned,
                    Gross = paySlip.Gross,
                    TotalIncome = paySlip.TotalIncome,
                    TotalDeduction = paySlip.TotalDeduction,
                    TakeHomePay = paySlip.TakeHomePay,
                    
                    // Working Days & Attendance
                    DaysPayable = paySlip.DaysPayable,
                    PresentDays = paySlip.PresentDays,
                    LossPayDays = paySlip.LossPayDays,
                    OverTimeDays = paySlip.OverTimeDays,
                    
                    // Wages Info
                    IsPerDayWagesEmployee = paySlip.IsPerDayWagesEmployee,
                    PerDayWages = paySlip.PerDayWages,
                    PerDayOverTimeWages = paySlip.PerDayOverTimeWages,
                    OvertimeSalary = paySlip.OvertimeSalary,
                    
                    // Bank Details (masked for security)
                    BankName = paySlip.BankName,
                    BankAccountNumber = MaskBankAccount(paySlip.BankAccountNumber),
                    IFSCCode = paySlip.IFSCCode,
                    BankBranchName = paySlip.BankBranchName,
                    
                    // Organization Info
                    TenantId = paySlip.TenantId,
                    TenantName = paySlip.TenantName,
                    Currency = paySlip.Currency,
                    Logo = paySlip.Logo
                };
				// Fetch actual earnings & deductions from DB
				var incomes = await _paySlipRepository.GetPaySlipIncomesAsync(paySlip.Id);
				var deductions = await _paySlipRepository.GetPaySlipDeductionsAsync(paySlip.Id);

				detail.Earnings = incomes.ToList();
				detail.Deductions = deductions.ToList();
				return new PaySlipResponse
                {
                    Success = true,
                    Message = PaySlipMessages.PaySlipFetchedSuccessfully,
                    Data = detail,
                    TotalRecords = 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.PaySlip.ErrorFetchingPaySlipById);
                return new PaySlipResponse
                {
                    Success = false,
                    Message = string.Format(PaySlipMessages.ErrorFetchingPaySlip, ex.Message),
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

		/// <summary>
		/// Download pay slip - returns payslip data as JSON for client-side PDF generation
		/// Note: vwPayrollDetailPrint doesn't store file paths, so we return data for client rendering
		/// </summary>
	//	public async Task<PaySlipDownloadResponse> DownloadPaySlipAsync(
	//   int payrollMonth,
	//   int payrollYear,
	//   CancellationToken cancellationToken)
	//	{
	//		try
	//		{
	//			// ==============================
	//			// 1️⃣ Validate Tenant
	//			// ==============================
	//			//var tenantId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirst("TenantId")?.Value);
	//			var userId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirst("UserId")?.Value);

	//			//if (tenantId == null)
	//			//{
	//			//	return new PaySlipDownloadResponse
	//			//	{
	//			//		Success = false,
	//			//		Message = "Invalid tenant."
	//			//	};
	//			//}

	//			// ==============================
	//			// 2️⃣ Get Logged-in Employee
	//			// ==============================
	//			var (employeeId,tenantId) =
	//await _paySlipRepository.GetEmployeeIdAndTenantByUserIdAsync(userId);
	//			if (employeeId == null)
	//			{
	//				return new PaySlipDownloadResponse
	//				{
	//					Success = false,
	//					Message = "Employee not found."
	//				};
	//			}

	//			// ==============================
	//			// 3️⃣ Fetch Payslip from DB
	//			// ==============================
	//			// ==============================
	//			// 3️⃣ Fetch Payslip from Repository
	//			// ==============================
	//			var paySlip = await _paySlipRepository
	//				.GetPaySlipByEmployeeMonthYearAsync(
	//					employeeId.Value,
	//					tenantId.Value,
	//					payrollMonth,
	//					payrollYear);

	//			if (paySlip == null)
	//			{
	//				return new PaySlipDownloadResponse
	//				{
	//					Success = false,
	//					Message = "Payslip not found."
	//				};
	//			}

	//			// ==============================
	//			// 4️⃣ Map Earnings
	//			// ==============================
	//			var earnings = await _paySlipRepository
	//.GetPaySlipIncomesAsync(paySlip.Id);

	//			var deductions = await _paySlipRepository
	//				.GetPaySlipDeductionsAsync(paySlip.Id);

	//			// ==============================
	//			// 5️⃣ Map Deductions
	//			// ==============================
				
	//			// ==============================
	//			// 6️⃣ Map to PaySlipDetail Model
	//			// ==============================
	//			var detail = new PaySlipDetail
	//			{
	//				TenantName = paySlip.TenantName,
	//				PayrollMonthName = paySlip.PayrollMonthName,
	//				PayrollYear = paySlip.PayrollYear,
	//				FinancialYearStart = paySlip.FinancialYearStart,

	//				EmployeeName = paySlip.EmployeeName,
	//				EmployeeNumber = paySlip.EmployeeNumber,
	//				DesignationName = paySlip.DesignationName,
	//				BranchName = paySlip.BranchName,
	//				DateOfJoining = paySlip.DateOfJoining,

	//				BankName = paySlip.BankName,
	//				BankAccountNumber = paySlip.BankAccountNumber,
	//				IFSCCode = paySlip.IFSCCode,

	//				DaysPayable = paySlip.DaysPayable,
	//				PresentDays = paySlip.PresentDays,
	//				LossPayDays = paySlip.LossPayDays,

	//				Earnings = earnings.ToList(),
	//				Deductions = deductions.ToList(),

	//				TotalIncome = paySlip.TotalIncome,
	//				TotalDeduction = paySlip.TotalDeduction,
	//				TakeHomePay = paySlip.TakeHomePay,

	//				Currency = paySlip.Currency ?? "₹"
	//			};

	//			// ==============================
	//			// 7️⃣ Generate PDF
	//			// ==============================
	//			var pdfBytes = SalarySlipPdfGenerator.Generate(detail);

	//			var fileName =
	//				$"PaySlip_{detail.EmployeeName}_{detail.PayrollMonthName}_{detail.PayrollYear}.pdf";

	//			// ==============================
	//			// 8️⃣ Return File Response
	//			// ==============================
	//			return new PaySlipDownloadResponse
	//			{
	//				Success = true,
	//				Message = "Payslip downloaded successfully.",
	//				FileContent = pdfBytes,
	//				FileName = fileName,
	//				ContentType = "application/pdf"
	//			};
	//		}
	//		catch (Exception ex)
	//		{
	//			return new PaySlipDownloadResponse
	//			{
	//				Success = false,
	//				Message = $"Error generating payslip: {ex.Message}"
	//			};
	//		}
	//	}

		/// <summary>
		/// Mask bank account number for security (show only last 4 digits)
		/// </summary>
		private static string? MaskBankAccount(string? accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length <= 4)
                return accountNumber;

            var masked = new string('X', accountNumber.Length - 4) + accountNumber[^4..];
            return masked;
        }

        /// <summary>
        /// Get content type based on format
        /// </summary>
        private static string GetContentType(string format)
        {
            return format.ToLower() switch
            {
                "pdf" => "application/pdf",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/pdf"
            };
        }
		public async Task<PaySlipResponse> GetProvidentFundSummaryAsync(int userId)
		{
			try
			{
				if (userId <= 0)
				{
					return new PaySlipResponse
					{
						Success = false,
						Message = "User Id required"
					};
				}

				var (employeeId, tenantId) =
					await _paySlipRepository.GetEmployeeIdAndTenantByUserIdAsync(userId);

				if (!employeeId.HasValue || !tenantId.HasValue)
				{
					return new PaySlipResponse
					{
						Success = false,
						Message = "Employee not found"
					};
				}

				var (myShare, employerShare) =
					await _paySlipRepository
						.GetEmployeeProvidentFundSummaryAsync(employeeId.Value, tenantId.Value);

				var data = new ProvidentFundSummary
				{
					MyShare = myShare,
					EmployerShare = employerShare,
					TotalProvidentFund = myShare + employerShare
				};

				return new PaySlipResponse
				{
					Success = true,
					Message = "Provident Fund fetched successfully",
					Data = data,
					TotalRecords = 1
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error fetching Provident Fund");

				return new PaySlipResponse
				{
					Success = false,
					Message = ex.Message
				};
			}
		}
		public async Task<MonthlyPaymentSummaryResponse>
	GetMonthlyPaymentSummaryAsync(MonthlyPaymentSummaryRequest request)
		{
			try
			{
				if (request.UserId <= 0)
				{
					return new MonthlyPaymentSummaryResponse
					{
						Success = false,
						Message = "UserId is required"
					};
				}

				var (employeeId, tenantId) =
					await _paySlipRepository
						.GetEmployeeIdAndTenantByUserIdAsync(request.UserId);

				if (!employeeId.HasValue || !tenantId.HasValue)
				{
					return new MonthlyPaymentSummaryResponse
					{
						Success = false,
						Message = "Employee not found"
					};
				}

				var summary =
					await _paySlipRepository
						.GetMonthlyPaymentSummaryAsync(
							employeeId.Value,
							tenantId.Value,
							request.Month,
							request.Year);

				if (summary == null)
				{
					return new MonthlyPaymentSummaryResponse
					{
						Success = false,
						Message = "No payroll data found"
					};
				}

				return new MonthlyPaymentSummaryResponse
				{
					Success = true,
					Message = "Monthly summary fetched successfully",
					Data = summary
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error fetching monthly payment summary");

				return new MonthlyPaymentSummaryResponse
				{
					Success = false,
					Message = ex.Message
				};
			}
		}
		public async Task<MonthlyPaymentSummaryResponse>
GetLastMonthPaymentSummaryAsync(int userId)
		{
			try
			{
				if (userId <= 0)
				{
					return new MonthlyPaymentSummaryResponse
					{
						Success = false,
						Message = "UserId is required"
					};
				}

				var (employeeId, tenantId) =
					await _paySlipRepository
						.GetEmployeeIdAndTenantByUserIdAsync(userId);

				if (!employeeId.HasValue || !tenantId.HasValue)
				{
					return new MonthlyPaymentSummaryResponse
					{
						Success = false,
						Message = "Employee not found"
					};
				}

				// ✅ LAST CALENDAR MONTH
				var lastMonthDate = DateTime.Today.AddMonths(-1);
				int month = lastMonthDate.Month;
				int year = lastMonthDate.Year;

				var summary =
					await _paySlipRepository
						.GetMonthlyPaymentSummaryAsync(
							employeeId.Value,
							tenantId.Value,
							month,
							year);

				if (summary == null)
				{
					return new MonthlyPaymentSummaryResponse
					{
						Success = false,
						Message = "No payroll data found for last month"
					};
				}

				return new MonthlyPaymentSummaryResponse
				{
					Success = true,
					Message = "Last month payroll fetched successfully",
					PayrollMonth = month,
					PayrollYear = year,
					Data = summary
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error fetching last month payroll");

				return new MonthlyPaymentSummaryResponse
				{
					Success = false,
					Message = ex.Message
				};
			}
		}


		public async Task<PaySlipDownloadResponse> DownloadPaySlipByMonthYearAsync(
			PaySlipDownloadByMonthYearRequest request)
		{
			try
			{
				if (request.UserId <= 0)
				{
					return new PaySlipDownloadResponse
					{
						Success = false,
						Message = "UserId is required"
					};
				}

				// 1️⃣ Get employee + tenant
				var (employeeId, tenantId) =
					await _paySlipRepository
						.GetEmployeeIdAndTenantByUserIdAsync(request.UserId);

				if (!employeeId.HasValue || !tenantId.HasValue)
				{
					return new PaySlipDownloadResponse
					{
						Success = false,
						Message = "Employee not found"
					};
				}

				// 2️⃣ Get payslip
				var paySlip = await _paySlipRepository
	.GetPaySlipWithWeekOffAsync(
		employeeId.Value,
		tenantId.Value,
		request.Month,
		request.Year);
				if (paySlip == null)
				{
					return new PaySlipDownloadResponse
					{
						Success = false,
						Message = "PaySlip not found"
					};
				}

				// 3️⃣ Get earnings & deductions
				var incomes = await _paySlipRepository
					.GetPaySlipIncomesAsync(paySlip.Id);

				var deductions = await _paySlipRepository
					.GetPaySlipDeductionsAsync(paySlip.Id);

				// 4️⃣ Build detailed model
				var detail = new PaySlipDetail
				{
					TotalWeekOffDays=paySlip.TotalWeekOffDays,
					Id = paySlip.Id,
					PayrollId = paySlip.PayrollId,
					EmployeeId = paySlip.EmployeeId,

					EmployeeName = paySlip.EmployeeName,
					EmployeeNumber = paySlip.EmployeeNumber,
					Email = paySlip.Email,
					DateOfBirth = paySlip.DateOfBirth,
					DateOfJoining = paySlip.DateOfJoining,
					GenderName = paySlip.GenderName,
					DesignationName = paySlip.DesignationName,
					BranchName = paySlip.BranchName,

					TaxNumber = paySlip.TaxNumber,
					ESINo = paySlip.ESINo,
					PFNo = paySlip.PFNo,
					UANNo = paySlip.UANNo,

					PayrollMonth = paySlip.PayrollMonth,
					PayrollYear = paySlip.PayrollYear,
					PayrollMonthName = paySlip.PayrollMonthName,
					FinancialYearStart = paySlip.FinancialYearStart,

					BasicSalary = (decimal)paySlip.BasicSalary,
					SalarySlab = paySlip.SalarySlab,
					SalaryEarned = paySlip.SalaryEarned,
					Gross = paySlip.Gross,
					TotalIncome = (decimal)paySlip.TotalIncome,
					TotalDeduction = (decimal)paySlip.TotalDeduction,
					TakeHomePay = (decimal)paySlip.TakeHomePay,

					DaysPayable = paySlip.DaysPayable,
					PresentDays = paySlip.PresentDays,
					LossPayDays = paySlip.LossPayDays,
					OverTimeDays = paySlip.OverTimeDays,

					IsPerDayWagesEmployee = paySlip.IsPerDayWagesEmployee,
					PerDayWages = paySlip.PerDayWages,
					PerDayOverTimeWages = paySlip.PerDayOverTimeWages,
					OvertimeSalary = paySlip.OvertimeSalary,

					BankName = paySlip.BankName,
					BankAccountNumber = MaskBankAccount(paySlip.BankAccountNumber),
					IFSCCode = paySlip.IFSCCode,
					BankBranchName = paySlip.BankBranchName,

					TenantId = paySlip.TenantId,
					TenantName = paySlip.TenantName,
					Currency = paySlip.Currency ?? "₹",
					Logo = paySlip.Logo,

					Earnings = incomes.ToList(),
					Deductions = deductions.ToList()
				};

				// 5️⃣ Generate PDF
				var pdfBytes = SalarySlipPdfGenerator.Generate(detail);

				if (pdfBytes == null || pdfBytes.Length == 0)
				{
					return new PaySlipDownloadResponse
					{
						Success = false,
						Message = "PDF generation failed"
					};
				}

				return new PaySlipDownloadResponse
				{
					Success = true,
					Message = "PaySlip downloaded successfully",
					FileContent = pdfBytes,
					FileName = $"PaySlip_{detail.EmployeeName}_{detail.PayrollMonthName}_{detail.PayrollYear}.pdf",
					ContentType = "application/pdf"
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error downloading payslip by month/year");

				return new PaySlipDownloadResponse
				{
					Success = false,
					Message = ex.Message
				};
			}
		}

		public async Task<PaySlipYearsResponse> GetPaySlipYearsAsync(int userId)
		{
			try
			{
				var employee = await _employeeRepository.GetEmployeebyUserIdAsync(userId);
				if (employee == null)
				{
					return new PaySlipYearsResponse
					{
						Success = false,
						Message = "Employee not found"
					};
				}

				int currentYear = DateTime.Now.Year;
				var years = new[] { currentYear, currentYear - 1, currentYear - 2 }.ToList();

				return new PaySlipYearsResponse
				{
					Success = true,
					Message = "Years fetched successfully",
					Years = years
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error fetching payslip years");
				return new PaySlipYearsResponse
				{
					Success = false,
					Message = ex.Message
				};
			}
		}

		public async Task<PaySlipMonthsResponse> GetPaySlipMonthsByYearAsync(int userId, int year)
		{
			try
			{
				var (employeeId, tenantId) = await _paySlipRepository.GetEmployeeIdAndTenantByUserIdAsync(userId);

				if (!employeeId.HasValue || !tenantId.HasValue)
				{
					return new PaySlipMonthsResponse
					{
						Success = false,
						Message = "Employee not found"
					};
				}

				var months = (await _paySlipRepository.GetPaySlipMonthsByYearAsync(
					employeeId.Value, tenantId.Value, year)).ToList();

				return new PaySlipMonthsResponse
				{
					Success = true,
					Message = "Months fetched successfully",
					Year = year,
					Months = months
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error fetching payslip months for year {Year}", year);
				return new PaySlipMonthsResponse
				{
					Success = false,
					Message = ex.Message
				};
			}
		}

        private string MaskBankAccount(object bankAccountNumber)
        {
            throw new NotImplementedException();
        }

        public async Task<PaySlipWithWeekOff?> GetPaySlipAsync(int employeeId, int tenantId, int month, int year)
		{
			var payslip = await _paySlipRepository.GetPaySlipWithWeekOffAsync(employeeId, tenantId, month, year);
			if (payslip == null)
				return null;

			//// Populate Earnings & Deductions
			//payslip.Earnings = (await _paySlipRepository.GetPaySlipIncomesAsync(payslip.)).ToList();
			//payslip.Deductions = (await _paySlipRepository.GetPaySlipDeductionsAsync(payslip.Id)).ToList();

			//payslip.Gross = payslip.Earnings.Sum(x => x.Amount);
			//payslip.TotalDeduction = payslip.Deductions.Sum(x => x.Amount);
			//payslip.TakeHomePay = payslip.Gross - payslip.TotalDeduction;

			return payslip;
		}


	}
}
