using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;
using MobileWebApi.Constants;

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
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(CreateLeaveRequestAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveCreateLeaveRequestDatabaseError}: Failed to create leave request",
                    ex);
            }
        }

        /// <summary>
        /// Get leave request by ID
        /// </summary>
        public async Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetLeaveRequestById");

                return await conn.QueryFirstOrDefaultAsync<LeaveRequest>(query, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetLeaveRequestByIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveGetLeaveRequestByIdDatabaseError}: Failed to fetch leave request by id",
                    ex);
            }
        }

		/// <summary>
		/// Get leave requests with filters
		/// </summary>
        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsAsync(
            int? organisationId,
            int? employeeId,
            int? leaveTypeId)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetLeaveRequestsAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveGetLeaveRequestsDatabaseError}: Failed to fetch leave requests",
                    ex);
            }
        }


		/// <summary>
		/// Get leave requests by employee ID
		/// </summary>
        public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(int employeeId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetLeaveRequestsByEmployeeId");

                return await conn.QueryAsync<LeaveRequest>(query, new { EmployeeId = employeeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetLeaveRequestsByEmployeeIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveGetLeaveRequestsByEmployeeIdDatabaseError}: Failed to fetch leave requests by employee id",
                    ex);
            }
        }

        /// <summary>
        /// Update leave request status
        /// </summary>
        public async Task<bool> UpdateLeaveRequestStatusAsync(int id, int statusId, string statusText, int updateUserId)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(UpdateLeaveRequestStatusAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveUpdateLeaveRequestStatusDatabaseError}: Failed to update leave request status",
                    ex);
            }
        }

        /// <summary>
        /// Get leave balance by employee ID
        /// </summary>
        public async Task<IEnumerable<LeaveBalance>> GetLeaveBalanceByEmployeeIdAsync(int employeeId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetLeaveBalanceByEmployeeId");

                return await conn.QueryAsync<LeaveBalance>(query, new { EmployeeId = employeeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetLeaveBalanceByEmployeeIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveGetLeaveBalanceByEmployeeIdDatabaseError}: Failed to fetch leave balance by employee id",
                    ex);
            }
        }

        /// <summary>
        /// Get specific leave balance for employee and leave type
        /// </summary>
        public async Task<LeaveBalance?> GetLeaveBalanceAsync(int employeeId, int leaveTypeId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetLeaveBalance");

                return await conn.QueryFirstOrDefaultAsync<LeaveBalance>(query, new
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = leaveTypeId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetLeaveBalanceAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveGetLeaveBalanceDatabaseError}: Failed to fetch leave balance",
                    ex);
            }
        }

        /// <summary>
        /// Update leave balance
        /// </summary>
        public async Task<bool> UpdateLeaveBalanceAsync(int employeeId, int leaveTypeId, decimal newBalance, int updateUserId)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(UpdateLeaveBalanceAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveUpdateLeaveBalanceDatabaseError}: Failed to update leave balance",
                    ex);
            }
        }

        /// <summary>
        /// Create leave transaction
        /// </summary>
        public async Task<int> CreateLeaveTransactionAsync(LeaveTransaction transaction)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(CreateLeaveTransactionAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveCreateLeaveTransactionDatabaseError}: Failed to create leave transaction",
                    ex);
            }
        }

        /// <summary>
        /// Get leave transactions by employee ID
        /// </summary>
        public async Task<IEnumerable<LeaveTransaction>> GetLeaveTransactionsByEmployeeIdAsync(int employeeId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetLeaveTransactionsByEmployeeId");

                return await conn.QueryAsync<LeaveTransaction>(query, new { EmployeeId = employeeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetLeaveTransactionsByEmployeeIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveGetLeaveTransactionsByEmployeeIdDatabaseError}: Failed to fetch leave transactions by employee id",
                    ex);
            }
        }

        /// <summary>
        /// Get leave type ID by name
        /// </summary>
        public async Task<int?> GetLeaveTypeIdByNameAsync(string leaveTypeName)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetLeaveTypeIdByName");

                return await conn.QueryFirstOrDefaultAsync<int?>(query, new { LeaveTypeName = leaveTypeName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetLeaveTypeIdByNameAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveGetLeaveTypeIdByNameDatabaseError}: Failed to fetch leave type id by name",
                    ex);
            }
        }

        /// <summary>
        /// Get employee ID by user ID
        /// </summary>
        public async Task<int?> GetEmployeeIdByUserIdAsync(int userId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetEmployeeIdByUserId");

                return await conn.QueryFirstOrDefaultAsync<int?>(query, new { UserId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeeIdByUserIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveGetEmployeeIdByUserIdDatabaseError}: Failed to fetch employee id by user id",
                    ex);
            }
        }

        /// <summary>
        /// Generate next leave request number for tenant
        /// </summary>
        public async Task<string?> GenerateLeaveRequestNumberAsync(int organisationId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetNextLeaveRequestNumber");

                var lastNumber = await conn.QueryFirstOrDefaultAsync<string>(query, new { OrganisationId = organisationId });

                // Generate next number (format: LR-YYYYMMDD-0001)
                var today = DateTime.Now.ToString("yyyyMMdd");
                var prefix = $"LR-{today}-";

                if (string.IsNullOrEmpty(lastNumber) || !lastNumber.Contains(today))
                {
                    return $"{prefix}0001";
                }

                // Extract and increment the sequence number
                var parts = lastNumber.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int seq))
                {
                    return $"{prefix}{(seq + 1):D4}";
                }

                return $"{prefix}0001";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GenerateLeaveRequestNumberAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveGenerateLeaveRequestNumberDatabaseError}: Failed to generate leave request number",
                    ex);
            }
        }
		/// <summary>
		/// Get configured week offs (DayOffId) for a tenant/organization
		/// </summary>
        public async Task<List<int>> GetTenantDayOffsAsync(int organisationId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetTenantDayOffsByTenantId");

                var result = await conn.QueryAsync<int>(query, new { TenantId = organisationId });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetTenantDayOffsAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveGetTenantDayOffsDatabaseError}: Failed to fetch tenant day offs",
                    ex);
            }
        }
		/// <summary>
		/// Get holidays for a tenant between given dates
		/// </summary>
        public async Task<List<Holiday>> GetHolidaysAsync(int organisationId, DateTime fromDate, DateTime toDate)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetHolidaysAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveGetHolidaysDatabaseError}: Failed to fetch holidays for tenant",
                    ex);
            }
        }

        public async Task<bool> HasOverlappingLeaveAsync(
            int employeeId,
            DateTime fromDate,
            DateTime toDate)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("CheckOverlappingLeave");

                return await conn.ExecuteScalarAsync<bool>(query, new
                {
                    EmployeeId = employeeId,
                    FromDate = fromDate.Date,
                    ToDate = toDate.Date
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(HasOverlappingLeaveAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveHasOverlappingLeaveDatabaseError}: Failed to check overlapping leave",
                    ex);
            }
        }

        public async Task<string?> GetLastLeaveRequestNumberAsync(string today, int organisationId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetLastLeaveRequestNumber");

                return await conn.QueryFirstOrDefaultAsync<string>(query,
                    new
                    {
                        Today = today,
                        TenantId = organisationId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetLastLeaveRequestNumberAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveGetLastLeaveRequestNumberDatabaseError}: Failed to fetch last leave request number",
                    ex);
            }
        }

        public async Task<IEnumerable<LeaveHistoryItem>> GetLeaveHistoryAsync(int employeeId, int year)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetLeaveHistory");

                var rows = await conn.QueryAsync<(DateTime LeaveDate, string LeaveType, string Reason, int LeaveRequestStatus)>(
                    query, new { EmployeeId = employeeId, Year = year });

                return rows.Select(r => new LeaveHistoryItem
                {
                    LeaveDate = r.LeaveDate,
                    LeaveType = r.LeaveType,
                    Reason = r.Reason,
                    Status = MapStatusIdToText(r.LeaveRequestStatus)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetLeaveHistoryAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.LeaveGetLeaveHistoryDatabaseError}: Failed to fetch leave history",
                    ex);
            }
        }

		private static string MapStatusIdToText(int statusId)
		{
			return statusId switch
			{
				1 => "Submit",
				2 => "Approved",
				3 => "Rejected",
				4 => "Withdrawn",
				5 => "Canceled",
				6 => "Pending",
				7 => "Pending For Approval",
				8 => "Cancellation Approved",
				9 => "Cancellation Rejected",
				_ => "Unknown"
			};
		}
	}
}

