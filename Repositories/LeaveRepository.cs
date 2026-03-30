using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
    public class LeaveRepository : ILeaveRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<LeaveRepository> _logger;
        private readonly QueryProvider _queryProvider;

        public LeaveRepository(DapperContext context, ILogger<LeaveRepository> logger, QueryProvider queryProvider)
        {
            _context = context;
            _logger = logger;
            _queryProvider = queryProvider;
        }

        /// <summary>
        /// Create a new leave request
        /// </summary>
        public async Task<int> CreateLeaveRequestAsync(LeaveRequest leaveRequest)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("CreateLeaveRequest");

            return await conn.ExecuteScalarAsync<int>(query, new
            {
                leaveRequest.Number,
                leaveRequest.EmployeeId,
                leaveRequest.LeaveTypeId,
                leaveRequest.FromDate,
                leaveRequest.ToDate,
                leaveRequest.Duration,
                leaveRequest.Description,
                leaveRequest.CurrentAction,
                leaveRequest.LeaveRequestStatus,
                leaveRequest.DelegatedEmployeeId,
                leaveRequest.OrganisationId,
                leaveRequest.InsertUserId
            });
        }

        /// <summary>
        /// Get leave request by ID
        /// </summary>
        public async Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetLeaveRequestById");

            return await conn.QueryFirstOrDefaultAsync<LeaveRequest>(query, new { Id = id });
        }

		/// <summary>
		/// Get leave requests with filters
		/// </summary>
		public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsAsync(
	int? organisationId,
	int? employeeId,
	int? leaveTypeId
)
		{
			using var conn = _context.CreateConnection();
			string query = _queryProvider.Get("GetLeaveRequests");

			return await conn.QueryAsync<LeaveRequest>(query, new
			{
				OrganisationId = organisationId,
				EmployeeId = employeeId,
				LeaveTypeId = leaveTypeId
			});
		}


		/// <summary>
		/// Get leave requests by employee ID
		/// </summary>
		public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(int employeeId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetLeaveRequestsByEmployeeId");

            return await conn.QueryAsync<LeaveRequest>(query, new { EmployeeId = employeeId });
        }

        /// <summary>
        /// Update leave request status
        /// </summary>
        public async Task<bool> UpdateLeaveRequestStatusAsync(int id, int statusId, string statusText, int updateUserId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("UpdateLeaveRequestStatus");

            var rowsAffected = await conn.ExecuteAsync(query, new
            {
                Id = id,
                StatusId = statusId,
                StatusText = statusText,
                UpdateUserId = updateUserId
            });

            return rowsAffected > 0;
        }

        /// <summary>
        /// Get leave balance by employee ID
        /// </summary>
        public async Task<IEnumerable<LeaveBalance>> GetLeaveBalanceByEmployeeIdAsync(int employeeId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetLeaveBalanceByEmployeeId");

            return await conn.QueryAsync<LeaveBalance>(query, new { EmployeeId = employeeId });
        }

        /// <summary>
        /// Get specific leave balance for employee and leave type
        /// </summary>
        public async Task<LeaveBalance?> GetLeaveBalanceAsync(int employeeId, int leaveTypeId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetLeaveBalance");

            return await conn.QueryFirstOrDefaultAsync<LeaveBalance>(query, new
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId
            });
        }

        /// <summary>
        /// Update leave balance
        /// </summary>
        public async Task<bool> UpdateLeaveBalanceAsync(int employeeId, int leaveTypeId, decimal newBalance, int updateUserId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("UpdateLeaveBalance");

            var rowsAffected = await conn.ExecuteAsync(query, new
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId,
                LeaveBalance = newBalance,
                UpdateUserId = updateUserId
            });

            return rowsAffected > 0;
        }

        /// <summary>
        /// Create leave transaction
        /// </summary>
        public async Task<int> CreateLeaveTransactionAsync(LeaveTransaction transaction)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("CreateLeaveTransaction");

            return await conn.ExecuteScalarAsync<int>(query, new
            {
                transaction.LeaveTransactionType,
                transaction.EmployeeId,
                transaction.LeaveTypeId,
                transaction.Description,
                transaction.LeaveBalance,
                transaction.EffectiveDate,
                transaction.InsertUserId,
                transaction.OrganisationId
            });
        }

        /// <summary>
        /// Get leave transactions by employee ID
        /// </summary>
        public async Task<IEnumerable<LeaveTransaction>> GetLeaveTransactionsByEmployeeIdAsync(int employeeId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetLeaveTransactionsByEmployeeId");

            return await conn.QueryAsync<LeaveTransaction>(query, new { EmployeeId = employeeId });
        }

        /// <summary>
        /// Get leave type ID by name
        /// </summary>
        public async Task<int?> GetLeaveTypeIdByNameAsync(string leaveTypeName)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetLeaveTypeIdByName");

            return await conn.QueryFirstOrDefaultAsync<int?>(query, new { LeaveTypeName = leaveTypeName });
        }

        /// <summary>
        /// Get employee ID by user ID
        /// </summary>
        public async Task<int?> GetEmployeeIdByUserIdAsync(int userId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetEmployeeIdByUserId");

            return await conn.QueryFirstOrDefaultAsync<int?>(query, new { UserId = userId });
        }

        /// <summary>
        /// Generate next leave request number for tenant
        /// </summary>
        public async Task<string?> GenerateLeaveRequestNumberAsync(int organisationId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetLastLeaveRequestNumber");

            var today = DateTime.Now.ToString("yyyyMMdd");
            var prefix = $"LVR/{today}";

            var lastNumber = await conn.QueryFirstOrDefaultAsync<string>(query, new { TenantId = organisationId, Today = today });

            // Required format: LVR/YYYYMMDD#### (sequence resets daily)
            if (string.IsNullOrWhiteSpace(lastNumber) || !lastNumber.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return $"{prefix}0001";
            }

            var seqPart = lastNumber.Substring(prefix.Length);
            if (seqPart.Length == 4 && int.TryParse(seqPart, out var seq))
                return $"{prefix}{(seq + 1):D4}";

            return $"{prefix}0001";
        }
		/// <summary>
		/// Get configured week offs (DayOffId) for a tenant/organization
		/// </summary>
		public async Task<List<int>> GetTenantDayOffsAsync(int organisationId)
		{
			using var conn = _context.CreateConnection();
			string query = _queryProvider.Get("GetTenantDayOffsByTenantId");

			var result = await conn.QueryAsync<int>(query, new { TenantId = organisationId });
			return result.ToList();
		}
		/// <summary>
		/// Get holidays for a tenant between given dates
		/// </summary>
		public async Task<List<Holiday>> GetHolidaysAsync(int organisationId, DateTime fromDate, DateTime toDate)
		{
			using var conn = _context.CreateConnection();
			string query = _queryProvider.Get("GetHolidaysByTenantIdAndDateRange");

			var result = await conn.QueryAsync<Holiday>(query, new
			{
				TenantId = organisationId,
				FromDate = fromDate,
				ToDate = toDate
			});

			return result.ToList();
		}

		public async Task<int> GetTotalLeaveAllocationForEmployeeAsync(int employeeId)
		{
			using var conn = _context.CreateConnection();
			string query = _queryProvider.Get("GetTotalLeaveAllocationForEmployee");

			var total = await conn.ExecuteScalarAsync<int?>(query, new { EmployeeId = employeeId });
			return total ?? 0;
		}
	}
}

