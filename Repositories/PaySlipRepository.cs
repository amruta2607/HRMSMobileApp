using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;

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

        /// <summary>
        /// Get a specific pay slip by ID (filtered by tenant)
        /// </summary>
        public async Task<PaySlip?> GetPaySlipByIdAsync(int id, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetPaySlipById");

            return await conn.QueryFirstOrDefaultAsync<PaySlip>(query, new { Id = id, TenantId = tenantId });
        }

        /// <summary>
        /// Get pay slip by employee, month and year (filtered by tenant)
        /// </summary>
        public async Task<PaySlip?> GetPaySlipByEmployeeMonthYearAsync(int employeeId, int tenantId, int month, int year)
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

        /// <summary>
        /// Get employee ID and TenantId by user ID
        /// </summary>
        public async Task<(int? EmployeeId, int? TenantId)> GetEmployeeIdAndTenantByUserIdAsync(int userId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetEmployeeIdAndTenantByUserId");

            var result = await conn.QueryFirstOrDefaultAsync<dynamic>(query, new { UserId = userId });
            
            if (result == null)
                return (null, null);
                
            return ((int?)result.Id, (int?)result.TenantId);
        }
		public async Task<(decimal MyShare, decimal EmployerShare)>GetEmployeeProvidentFundSummaryAsync(int employeeId, int tenantId)
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
		public async Task<MonthlyPaymentSummary?> GetMonthlyPaymentSummaryAsync(
	int employeeId,
	int tenantId,
	int month,
	int year)
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
		public async Task<IEnumerable<PaySlipLineItem>> GetPaySlipIncomesAsync(int paySlipId)
		{
			using var conn = _context.CreateConnection();
			string query = _queryProvider.Get("GetPaySlipIncomes");

			return await conn.QueryAsync<PaySlipLineItem>(
				query,
				new { PaySlipId = paySlipId });
		}

		public async Task<IEnumerable<PaySlipLineItem>> GetPaySlipDeductionsAsync(int paySlipId)
		{
			using var conn = _context.CreateConnection();
			string query = _queryProvider.Get("GetPaySlipDeductions");

			return await conn.QueryAsync<PaySlipLineItem>(
				query,
				new { PaySlipId = paySlipId });
		}
	}
}
