using Dapper;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;
using System;

namespace MobileWebApi.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<AttendanceRepository> _logger;
        private readonly QueryProvider _queryProvider;

        public AttendanceRepository(DapperContext context, ILogger<AttendanceRepository> logger, QueryProvider queryProvider)
        {
            _context = context;
            _logger = logger;
            _queryProvider = queryProvider;
        }

        public async Task<Punch?> GetPunchByEmployeeAndDate(int employeeId, DateTime punchDate)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetPunchByEmployeeAndDate");

                return await conn.QueryFirstOrDefaultAsync<Punch>(query,
                    new { EmployeeId = employeeId, PunchDate = punchDate.Date });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetPunchByEmployeeAndDate));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceGetPunchByEmployeeAndDateDatabaseError}: Failed to fetch punch by employee and date",
                    ex);
            }
        }

        /// <summary>
        /// Get punch record by employee and date with tenant filter
        /// </summary>
        public async Task<Punch?> GetPunchByEmployeeAndDateWithTenant(int employeeId, DateTime punchDate, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetPunchByEmployeeAndDateWithTenant");

                return await conn.QueryFirstOrDefaultAsync<Punch>(query,
                    new { EmployeeId = employeeId, PunchDate = punchDate.Date, TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetPunchByEmployeeAndDateWithTenant));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceGetPunchByEmployeeAndDateWithTenantDatabaseError}: Failed to fetch punch by employee, date and tenant",
                    ex);
            }
        }

        public async Task<int> InsertPunchIn(int employeeId, DateTime punchIn, DateTime punchDate)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("InsertPunchIn");

                return await conn.ExecuteScalarAsync<int>(query,
                    new
                    {
                        EmployeeId = employeeId,
                        PunchDate = punchDate.Date,
                        PunchIn = punchIn


                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(InsertPunchIn));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceInsertPunchInDatabaseError}: Failed to insert punch in record",
                    ex);
            }
        }

        public async Task UpdatePunchOut(int employeeId, DateTime punchOut, DateTime punchDate, double? duration)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("UpdatePunchOut");

                await conn.ExecuteAsync(query,
                    new
                    {
                        EmployeeId = employeeId,
                        PunchDate = punchDate.Date,
                        PunchOut = punchOut,
                        Duration = duration

                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(UpdatePunchOut));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceUpdatePunchOutDatabaseError}: Failed to update punch out record",
                    ex);
            }
        }

        /// <summary>
        /// Get attendance report based on request parameters (daily or monthly)
        /// </summary>
        public async Task<IEnumerable<AttendanceReport>> GetAttendanceReportAsync(AttendanceReportRequest request)
        {
            if (request.Daily && request.CalendarDate.HasValue)
            {
                return await GetDailyAttendanceReportAsync(request.BranchId, request.CalendarDate.Value, request.organization, request.EmployeeId, request.DepartmentId);
            }
            else if (request.Monthly && request.DateFrom.HasValue && request.DateTo.HasValue)
            {
                return await GetMonthlyAttendanceReportAsync(request.BranchId, request.DateFrom.Value, request.DateTo.Value, request.organization, request.EmployeeId, request.DepartmentId);
            }
            
            // Default: return empty list if parameters are invalid
            return Enumerable.Empty<AttendanceReport>();
        }

        /// <summary>
        /// Get daily attendance report for a specific date
        /// </summary>
        public async Task<IEnumerable<AttendanceReport>> GetDailyAttendanceReportAsync(int? branchId, DateTime calendarDate, int? organisationId, int? employeeId = null, int? departmentId = null)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetDailyAttendanceReport");

                var result = await conn.QueryAsync<AttendanceReport>(query,
                    new
                    {
                        BranchId = branchId,
                        CalendarDate = calendarDate.Date,
                        OrganisationId = organisationId,
                        EmployeeId = employeeId,
                        DepartmentId = departmentId
                    });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetDailyAttendanceReportAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceGetDailyAttendanceReportDatabaseError}: Failed to fetch daily attendance report",
                    ex);
            }
        }

        /// <summary>
        /// Get monthly attendance report for a date range
        /// </summary>
        public async Task<IEnumerable<AttendanceReport>> GetMonthlyAttendanceReportAsync(int? branchId, DateTime dateFrom, DateTime dateTo, int? organisationId, int? employeeId = null, int? departmentId = null)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetMonthlyAttendanceReport");

                var result = await conn.QueryAsync<AttendanceReport>(query,
                    new
                    {
                        BranchId = branchId,
                        DateFrom = dateFrom.Date,
                        DateTo = dateTo.Date,
                        OrganisationId = organisationId,
                        EmployeeId = employeeId,
                        DepartmentId = departmentId
                    });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetMonthlyAttendanceReportAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceGetMonthlyAttendanceReportDatabaseError}: Failed to fetch monthly attendance report",
                    ex);
            }
        }

        /// <summary>
        /// Get attendance report for a specific employee
        /// </summary>
        public async Task<IEnumerable<AttendanceReport>> GetEmployeeAttendanceReportAsync(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetEmployeeAttendanceReport");

                var result = await conn.QueryAsync<AttendanceReport>(query,
                    new
                    {
                        EmployeeId = employeeId,
                        DateFrom = dateFrom.Date,
                        DateTo = dateTo.Date
                    });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeeAttendanceReportAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceGetEmployeeAttendanceReportDatabaseError}: Failed to fetch employee attendance report",
                    ex);
            }
        }

        /// <summary>
        /// Get real-time attendance status for a specific date
        /// Shows all employees who have punched in (whether punched out or not)
        /// </summary>
        public async Task<IEnumerable<RealTimeAttendanceStatus>> GetRealTimeAttendanceStatusAsync(DateTime punchDate, int? organisationId = null, int? branchId = null, int? departmentId = null)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetRealTimeAttendanceStatus");

                var result = await conn.QueryAsync<RealTimeAttendanceStatus>(query,
                    new
                    {
                        PunchDate = punchDate.Date,
                        OrganisationId = organisationId,
                        BranchId = branchId,
                        DepartmentId = departmentId
                    });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetRealTimeAttendanceStatusAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceGetRealTimeAttendanceStatusDatabaseError}: Failed to fetch real-time attendance status",
                    ex);
            }
        }

        /// <summary>
        /// Get employees who are currently punched in (no punch out yet)
        /// </summary>
        public async Task<IEnumerable<RealTimeAttendanceStatus>> GetCurrentlyPunchedInAsync(DateTime punchDate, int? organisationId = null, int? branchId = null, int? departmentId = null)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetCurrentlyPunchedIn");

                var result = await conn.QueryAsync<RealTimeAttendanceStatus>(query,
                    new
                    {
                        PunchDate = punchDate.Date,
                        OrganisationId = organisationId,
                        BranchId = branchId,
                        DepartmentId = departmentId
                    });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetCurrentlyPunchedInAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceGetCurrentlyPunchedInDatabaseError}: Failed to fetch currently punched-in employees",
                    ex);
            }
        }

        /// <summary>
        /// Get attendance data for a specific employee by month and year (calendar view)
        /// </summary>
        public async Task<IEnumerable<AttendanceReport>> GetAttendanceByCalendarAsync(int employeeId, int month, int year)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetAttendanceByCalendar");

                var dateFrom = new DateTime(year, month, 1);
                var dateTo = dateFrom.AddMonths(1).AddDays(-1);

                var result = await conn.QueryAsync<AttendanceReport>(query,
                    new
                    {
                        EmployeeId = employeeId,
                        DateFrom = dateFrom.Date,
                        DateTo = dateTo.Date
                    });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetAttendanceByCalendarAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceGetAttendanceByCalendarDatabaseError}: Failed to fetch attendance by calendar",
                    ex);
            }
        }

        /// <summary>
        /// Get employee details by ID
        /// </summary>
        public async Task<Employee?> GetEmployeeByIdAsync(int employeeId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetEmployeeById");

                return await conn.QueryFirstOrDefaultAsync<Employee>(query,
                    new { Id = employeeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeeByIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceGetEmployeeByIdDatabaseError}: Failed to fetch employee by id",
                    ex);
            }
        }

        /// <summary>
        /// Get attendance reports by organisation ID
        /// </summary>
        public async Task<IEnumerable<AttendanceReport>> GetAttendanceReportsByOrganisationAsync(int organisationId, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetAttendanceReportsByOrganisation");

                var result = await conn.QueryAsync<AttendanceReport>(query,
                    new
                    {
                        OrganisationId = organisationId,
                        DateFrom = dateFrom.Date,
                        DateTo = dateTo.Date
                    });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetAttendanceReportsByOrganisationAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceGetAttendanceReportsByOrganisationDatabaseError}: Failed to fetch attendance reports by organisation",
                    ex);
            }
        }

        /// <summary>
        /// Get punch by ID
        /// </summary>
        public async Task<Punch?> GetPunchByIdAsync(int id, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetPunchById");

                return await conn.QueryFirstOrDefaultAsync<Punch>(query,
                    new { Id = id, TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetPunchByIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceGetPunchByIdDatabaseError}: Failed to fetch punch by id",
                    ex);
            }
        }

        /// <summary>
        /// Delete a punch record
        /// </summary>
        public async Task<bool> DeletePunchAsync(int id, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("DeletePunch");

                var rowsAffected = await conn.ExecuteAsync(query,
                    new { Id = id, TenantId = tenantId });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(DeletePunchAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceDeletePunchDatabaseError}: Failed to delete punch",
                    ex);
            }
        }

        /// <summary>
        /// Get today's punch logs for a specific biometric number from DeviceLog
        /// </summary>
        public async Task<IEnumerable<TodayPunchLogItem>> GetTodayPunchLogsAsync(string biometricNumber, DateTime date)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetTodayPunchLogs");

                var result = await conn.QueryAsync<TodayPunchLogItem>(query, new
                {
                    BiometricNumber = biometricNumber,
                    LogDate = date.Date
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetTodayPunchLogsAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceTodayPunchLogsDatabaseError}: Failed to fetch today's punch logs",
                    ex);
            }
        }

        /// <summary>
        /// Get today's punch logs from Punch table for a specific employee and tenant.
        /// </summary>
        public async Task<IEnumerable<TodayPunchLogItem>> GetTodayPunchLogsFromPunchAsync(int employeeId, int tenantId, DateTime date)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetTodayPunchLogsFromPunch");

                var result = await conn.QueryAsync<TodayPunchLogItem>(query, new
                {
                    EmployeeId = employeeId,
                    TenantId = tenantId,
                    LogDate = date.Date
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetTodayPunchLogsFromPunchAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AttendanceTodayPunchLogsDatabaseError}: Failed to fetch today's punch logs from Punch table",
                    ex);
            }
        }
    }
}
