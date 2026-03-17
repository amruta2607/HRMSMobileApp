using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;
using MobileWebApi.Constants;

namespace MobileWebApi.Repositories
{
    public class PaySlipRepository : IPaySlipRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<PaySlipRepository> _logger;
        private readonly QueryProvider _queryProvider;

        public PaySlipRepository(DapperContext context, ILogger<PaySlipRepository> logger, QueryProvider queryProvider)
        {
            _context = context;
            _logger = logger;
            _queryProvider = queryProvider;
        }

        /// <summary>
        /// Get list of pay slips for an employee with optional filters (filtered by tenant)
        /// </summary>
        public async Task<IEnumerable<PaySlip>> GetPaySlipsAsync(int employeeId, int tenantId, int? year = null, int? month = null)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetPaySlipsByEmployeeId");

                return await conn.QueryAsync<PaySlip>(query, new
                {
                    EmployeeId = employeeId,
                    TenantId = tenantId,
                    Year = year,
                    Month = month
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetPaySlipsAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.PaySlipGetPaySlipsDatabaseError}: Failed to fetch pay slips",
                    ex);
            }
        }

        /// <summary>
        /// Get a specific pay slip by ID (filtered by tenant)
        /// </summary>
        public async Task<PaySlip?> GetPaySlipByIdAsync(int id, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetPaySlipById");

                return await conn.QueryFirstOrDefaultAsync<PaySlip>(query, new { Id = id, TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetPaySlipByIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.PaySlipGetPaySlipByIdDatabaseError}: Failed to fetch pay slip by id",
                    ex);
            }
        }

        /// <summary>
        /// Get pay slip by employee, month and year (filtered by tenant)
        /// </summary>
        public async Task<PaySlip?> GetPaySlipByEmployeeMonthYearAsync(int employeeId, int tenantId, int month, int year)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetPaySlipByEmployeeMonthYear");

                return await conn.QueryFirstOrDefaultAsync<PaySlip>(query, new
                {
                    EmployeeId = employeeId,
                    TenantId = tenantId,
                    Month = month,
                    Year = year
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetPaySlipByEmployeeMonthYearAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.PaySlipGetPaySlipByEmployeeMonthYearDatabaseError}: Failed to fetch pay slip by employee, month and year",
                    ex);
            }
        }

        /// <summary>
        /// Get employee ID and TenantId by user ID
        /// </summary>
        public async Task<(int? EmployeeId, int? TenantId)> GetEmployeeIdAndTenantByUserIdAsync(int userId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetEmployeeIdAndTenantByUserId");

                var result = await conn.QueryFirstOrDefaultAsync<dynamic>(query, new { UserId = userId });

                if (result == null)
                    return (null, null);

                return ((int?)result.Id, (int?)result.TenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeeIdAndTenantByUserIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.PaySlipGetEmployeeIdAndTenantByUserIdDatabaseError}: Failed to fetch employee and tenant by user id",
                    ex);
            }
        }
        public async Task<(decimal MyShare, decimal EmployerShare)> GetEmployeeProvidentFundSummaryAsync(int employeeId, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetEmployeeProvidentFundSummary");

                var result = await conn.QueryFirstOrDefaultAsync<dynamic>(query, new
                {
                    EmployeeId = employeeId,
                    TenantId = tenantId
                });

                if (result == null)
                    return (0, 0);

                return ((decimal?)result.MyShare ?? 0,
                        (decimal?)result.EmployerShare ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeeProvidentFundSummaryAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.PaySlipGetEmployeeProvidentFundSummaryDatabaseError}: Failed to fetch employee provident fund summary",
                    ex);
            }
        }

        public async Task<(int Month, int Year)?> GetLatestPayrollPeriodAsync(
            int employeeId,
            int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();

                var sql = _queryProvider.Get("GetLatestPayrollPeriod");

                return await conn.QueryFirstOrDefaultAsync<(int Month, int Year)?>(sql, new
                {
                    EmployeeId = employeeId,
                    TenantId = tenantId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetLatestPayrollPeriodAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.PaySlipGetLatestPayrollPeriodDatabaseError}: Failed to fetch latest payroll period",
                    ex);
            }
        }

        public async Task<MonthlyPaymentSummary?> GetMonthlyPaymentSummaryAsync(
            int employeeId,
            int tenantId,
            int month,
            int year)
        {
            try
            {
                using var conn = _context.CreateConnection();

                var param = new
                {
                    EmployeeId = employeeId,
                    TenantId = tenantId,
                    Month = month,
                    Year = year
                };

                var summaryQuery = _queryProvider.Get("GetMonthlyPayrollSummary");
                var incomeQuery = _queryProvider.Get("GetMonthlyPayrollIncomes");
                var deductionQuery = _queryProvider.Get("GetMonthlyPayrollDeductions");

                var summary = await conn.QueryFirstOrDefaultAsync<MonthlyPaymentSummary>(summaryQuery, param);

                if (summary == null)
                    return null;

                var incomes = await conn.QueryAsync<IncomeItem>(incomeQuery, param);
                var deductions = await conn.QueryAsync<DeductionItem>(deductionQuery, param);

                summary.Incomes = incomes.ToList();
                summary.Deductions = deductions.ToList();

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetMonthlyPaymentSummaryAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.PaySlipGetMonthlyPaymentSummaryDatabaseError}: Failed to fetch monthly payment summary",
                    ex);
            }
        }

        public async Task<IEnumerable<PaySlipLineItem>> GetPaySlipIncomesAsync(int paySlipId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetPaySlipIncomes");

                return await conn.QueryAsync<PaySlipLineItem>(
                    query,
                    new { PaySlipId = paySlipId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetPaySlipIncomesAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.PaySlipGetPaySlipIncomesDatabaseError}: Failed to fetch pay slip incomes",
                    ex);
            }
        }

        public async Task<IEnumerable<PaySlipLineItem>> GetPaySlipDeductionsAsync(int paySlipId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetPaySlipDeductions");

                return await conn.QueryAsync<PaySlipLineItem>(
                    query,
                    new { PaySlipId = paySlipId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetPaySlipDeductionsAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.PaySlipGetPaySlipDeductionsDatabaseError}: Failed to fetch pay slip deductions",
                    ex);
            }
        }

        public async Task<PaySlipWithWeekOff> GetPaySlipWithWeekOffAsync(int employeeId, int tenantId, int month, int year)
        {
            try
            {
                using var conn = _context.CreateConnection();

                // 1. Fetch the basic payroll info
                string payrollQuery = @"
        SELECT *
        FROM vwPayrollDetailPrint
        WHERE EmployeeId = @EmployeeId 
          AND TenantId = @TenantId
          AND PayrollMonth = @Month
          AND PayrollYear = @Year
    ";

                var payroll = await conn.QueryFirstOrDefaultAsync<PaySlipWithWeekOff>(
                    payrollQuery,
                    new { EmployeeId = employeeId, TenantId = tenantId, Month = month, Year = year }
                );

                if (payroll == null)
                    return null;

                // 2. Get tenant day offs
                string dayOffQuery = @"
        SELECT 
   
    tcd.DayOffId
FROM TenantConfiguration tc
INNER JOIN TenantConfiguredDayOffDays tcd
    ON tc.TenantConfigurationId = tcd.TenantConfigurationId
WHERE tc.TenantId = @TenantId

    ";

                var dayOffs = (await conn.QueryAsync<int>(dayOffQuery, new { TenantId = tenantId })).ToList();

                // 3. Calculate total week off days
                payroll.TotalWeekOffDays = GetTotalWeekOffDays(dayOffs, month, year);

                return payroll;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetPaySlipWithWeekOffAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.PaySlipGetPaySlipWithWeekOffDatabaseError}: Failed to fetch pay slip with week off",
                    ex);
            }
        }

        private int GetTotalWeekOffDays(List<int> dayOffIds, int month, int year)
        {
            var normalizedDayOffs = dayOffIds
                .Select(NormalizeDayOfWeek)
                .ToHashSet();

            int totalDays = DateTime.DaysInMonth(year, month);
            int count = 0;

            for (int day = 1; day <= totalDays; day++)
            {
                int dayOfWeek = (int)new DateTime(year, month, day).DayOfWeek;
                if (normalizedDayOffs.Contains(dayOfWeek))
                    count++;
            }

            return count;
        }

        private int NormalizeDayOfWeek(int dayOffId)
        {
            // DB: 1–7 (Mon–Sun)
            // .NET: 0–6 (Sun–Sat)
            if (dayOffId == 7)
                return 0; // Sunday
            return dayOffId; // Monday–Saturday
        }

        public async Task<IEnumerable<PaySlipMonthItem>> GetPaySlipMonthsByYearAsync(int employeeId, int tenantId, int year)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetPaySlipMonthsByYear");

                var months = await conn.QueryAsync<int>(query, new
                {
                    EmployeeId = employeeId,
                    TenantId = tenantId,
                    Year = year
                });

                return months.Select(m => new PaySlipMonthItem
                {
                    Month = m,
                    MonthName = $"{new DateTime(year, m, 1):MMMM} {year}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetPaySlipMonthsByYearAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.PaySlipGetPaySlipMonthsByYearDatabaseError}: Failed to fetch pay slip months by year",
                    ex);
            }
        }
	}
}
