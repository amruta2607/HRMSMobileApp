using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;

namespace MobileWebApi.Services
{
    public class PaySlipService : IPaySlipService
    {
        private readonly IPaySlipRepository _paySlipRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<PaySlipService> _logger;

        public PaySlipService(
            IPaySlipRepository paySlipRepository,
            IEmployeeRepository employeeRepository,
            ILogger<PaySlipService> logger)
        {
            _paySlipRepository = paySlipRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
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
                    FinancialYearStart = paySlip.FinancialYearStart,
                    
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
        public async Task<PaySlipDownloadResponse> DownloadPaySlipAsync(PaySlipDownloadRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.PaySlip.DownloadingPaySlip, request.payslip_id);

                // Validate user ID
                if (request.user <= 0)
                {
                    return new PaySlipDownloadResponse
                    {
                        Success = false,
                        Message = PaySlipMessages.UserIdRequired,
                        FileContent = null,
                        FileName = null,
                        ContentType = null
                    };
                }

                // Validate pay slip ID
                if (request.payslip_id <= 0)
                {
                    return new PaySlipDownloadResponse
                    {
                        Success = false,
                        Message = PaySlipMessages.PaySlipIdRequired,
                        FileContent = null,
                        FileName = null,
                        ContentType = null
                    };
                }

                // Get employee ID and TenantId by user ID
                var (employeeId, tenantId) = await _paySlipRepository.GetEmployeeIdAndTenantByUserIdAsync(request.user);
                
                if (!employeeId.HasValue || !tenantId.HasValue)
                {
                    return new PaySlipDownloadResponse
                    {
                        Success = false,
                        Message = PaySlipMessages.EmployeeNotFoundForUser,
                        FileContent = null,
                        FileName = null,
                        ContentType = null
                    };
                }

                // Get pay slip to verify ownership (filtered by tenant)
                var paySlip = await _paySlipRepository.GetPaySlipByIdAsync(request.payslip_id, tenantId.Value);

                if (paySlip == null)
                {
                    return new PaySlipDownloadResponse
                    {
                        Success = false,
                        Message = PaySlipMessages.PaySlipNotFound,
                        FileContent = null,
                        FileName = null,
                        ContentType = null
                    };
                }

                // Verify the pay slip belongs to the user's employee
                if (paySlip.EmployeeId != employeeId.Value)
                {
                    return new PaySlipDownloadResponse
                    {
                        Success = false,
                        Message = PaySlipMessages.UnauthorizedAccess,
                        FileContent = null,
                        FileName = null,
                        ContentType = null
                    };
                }

				// Since vwPayrollDetailPrint is a view without stored files,
				// return payslip data as JSON for client-side PDF generation/printing
				// Build detailed payslip (same structure as GetPaySlipByIdAsync)
				var detail = new PaySlipDetail
				{
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

					BasicSalary = paySlip.BasicSalary,
					SalarySlab = paySlip.SalarySlab,
					SalaryEarned = paySlip.SalaryEarned,
					Gross = paySlip.Gross,
					TotalIncome = paySlip.TotalIncome,
					TotalDeduction = paySlip.TotalDeduction,
					TakeHomePay = paySlip.TakeHomePay,

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
					Currency = paySlip.Currency,
					Logo = paySlip.Logo
				};

				// 🔥 ADD THIS (IMPORTANT)
				var incomes = await _paySlipRepository.GetPaySlipIncomesAsync(paySlip.Id);
				var deductions = await _paySlipRepository.GetPaySlipDeductionsAsync(paySlip.Id);

				detail.Earnings = incomes.ToList();
				detail.Deductions = deductions.ToList();

				var fileName = $"PaySlip_{paySlip.EmployeeName}_{paySlip.PayrollMonthName}_{paySlip.PayrollYear}.json";

				return new PaySlipDownloadResponse
				{
					Success = true,
					Message = PaySlipMessages.PaySlipDownloadedSuccessfully,
					FileContent = null,
					FileName = fileName,
					ContentType = "application/json",
					PaySlipData = detail   // ✅ RETURN DETAIL, NOT paySlip
				};
			}
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.PaySlip.ErrorDownloadingPaySlip);
                return new PaySlipDownloadResponse
                {
                    Success = false,
                    Message = string.Format(PaySlipMessages.ErrorDownloadingPaySlip, ex.Message),
                    FileContent = null,
                    FileName = null,
                    ContentType = null
                };
            }
        }

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
		public async Task<PaySlipDownloadResponse>DownloadPaySlipByMonthYearAsync(PaySlipDownloadByMonthYearRequest request)
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

				// 🔥 IMPORTANT CHANGE
				var paySlip =
					await _paySlipRepository
						.GetPaySlipByEmployeeMonthYearAsync(
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

				// Build detailed payslip
				var detail = new PaySlipDetail
				{
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

					BasicSalary = paySlip.BasicSalary,
					SalarySlab = paySlip.SalarySlab,
					SalaryEarned = paySlip.SalaryEarned,
					Gross = paySlip.Gross,
					TotalIncome = paySlip.TotalIncome,
					TotalDeduction = paySlip.TotalDeduction,
					TakeHomePay = paySlip.TakeHomePay,

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
					Currency = paySlip.Currency,
					Logo = paySlip.Logo
				};

				// Fetch earnings & deductions
				var incomes = await _paySlipRepository
					.GetPaySlipIncomesAsync(paySlip.Id);

				var deductions = await _paySlipRepository
					.GetPaySlipDeductionsAsync(paySlip.Id);

				detail.Earnings = incomes.ToList();
				detail.Deductions = deductions.ToList();

				return new PaySlipDownloadResponse
				{
					Success = true,
					Message = "PaySlip fetched successfully",
					PaySlipData = detail
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
	}
}
