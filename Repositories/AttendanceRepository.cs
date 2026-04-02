using Dapper;
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
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetPunchByEmployeeAndDate");

            return await conn.QueryFirstOrDefaultAsync<Punch>(query,
                new { EmployeeId = employeeId, PunchDate = punchDate.Date });
        }

        /// <summary>
        /// Get punch record by employee and date with tenant filter
        /// </summary>
        public async Task<Punch?> GetPunchByEmployeeAndDateWithTenant(int employeeId, DateTime punchDate, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetPunchByEmployeeAndDateWithTenant");

            return await conn.QueryFirstOrDefaultAsync<Punch>(query,
                new { EmployeeId = employeeId, PunchDate = punchDate.Date, TenantId = tenantId });
        }

        public async Task<Punch?> GetOpenPunchByEmployeeId(int employeeId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetOpenPunchByEmployeeId");

            return await conn.QueryFirstOrDefaultAsync<Punch>(query, new { EmployeeId = employeeId });
        }

        public async Task<int> InsertPunchIn(int employeeId, DateTime punchIn, DateTime punchDate, string inSource, string? coordinateIn, string? linkIn)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("InsertPunchIn");

            return await conn.ExecuteScalarAsync<int>(query,
                new 
                { 
                    EmployeeId = employeeId, 
                    PunchDate = punchDate.Date, 
                    PunchIn = punchIn,
                    InSource = inSource,
                    CoordinateIn = coordinateIn,
                    LinkIn = linkIn
                });
        }

        public async Task UpdatePunchOut(int punchId, DateTime punchOut, double? duration, string outSource, string? coordinateOut, string? linkOut)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("UpdatePunchOut");

            await conn.ExecuteAsync(query,
                new 
                { 
                    Id = punchId,
                    PunchOut = punchOut, 
                    Duration = duration,
                    OutSource = outSource,
                    CoordinateOut = coordinateOut,
                    LinkOut = linkOut
                });
        }

        public async Task<List<DateTime>> GetHolidayDatesAsync(int tenantId, DateTime fromDate, DateTime toDate)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetHolidaysByTenantIdAndDateRange");

            var rows = await conn.QueryAsync<Holiday>(query, new { TenantId = tenantId, FromDate = fromDate.Date, ToDate = toDate.Date });
            return rows.Select(h => h.Date.Date).Distinct().ToList();
        }

        private sealed class LeaveDateRangeRow
        {
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
        }

        public async Task<List<(DateTime FromDate, DateTime ToDate)>> GetApprovedLeaveDateRangesAsync(int employeeId, DateTime fromDate, DateTime toDate)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetApprovedLeaveDateRangesByEmployeeAndDateRange");

            var rows = await conn.QueryAsync<LeaveDateRangeRow>(query, new { EmployeeId = employeeId, FromDate = fromDate.Date, ToDate = toDate.Date });
            return rows.Select(r => (r.FromDate.Date, r.ToDate.Date)).ToList();
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

        /// <summary>
        /// Get monthly attendance report for a date range
        /// </summary>
        public async Task<IEnumerable<AttendanceReport>> GetMonthlyAttendanceReportAsync(int? branchId, DateTime dateFrom, DateTime dateTo, int? organisationId, int? employeeId = null, int? departmentId = null)
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

        /// <summary>
        /// Get attendance report for a specific employee
        /// </summary>
        public async Task<IEnumerable<AttendanceReport>> GetEmployeeAttendanceReportAsync(int employeeId, DateTime dateFrom, DateTime dateTo)
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

        /// <summary>
        /// Get real-time attendance status for a specific date
        /// Shows all employees who have punched in (whether punched out or not)
        /// </summary>
        public async Task<IEnumerable<RealTimeAttendanceStatus>> GetRealTimeAttendanceStatusAsync(DateTime punchDate, int? organisationId = null, int? branchId = null, int? departmentId = null)
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

        /// <summary>
        /// Get employees who are currently punched in (no punch out yet)
        /// </summary>
        public async Task<IEnumerable<RealTimeAttendanceStatus>> GetCurrentlyPunchedInAsync(DateTime punchDate, int? organisationId = null, int? branchId = null, int? departmentId = null)
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

        /// <summary>
        /// Get attendance data for a specific employee by month and year (calendar view)
        /// </summary>
        public async Task<IEnumerable<AttendanceReport>> GetAttendanceByCalendarAsync(int employeeId, int month, int year)
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

        /// <summary>
        /// Get employee details by ID
        /// </summary>
        public async Task<Employee?> GetEmployeeByIdAsync(int employeeId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetEmployeeById");

            return await conn.QueryFirstOrDefaultAsync<Employee>(query,
                new { Id = employeeId });
        }

        /// <summary>
        /// Get attendance reports by organisation ID
        /// </summary>
        public async Task<IEnumerable<AttendanceReport>> GetAttendanceReportsByOrganisationAsync(int organisationId, DateTime dateFrom, DateTime dateTo)
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

        /// <summary>
        /// Get punch by ID
        /// </summary>
        public async Task<Punch?> GetPunchByIdAsync(int id, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetPunchById");

            return await conn.QueryFirstOrDefaultAsync<Punch>(query,
                new { Id = id, TenantId = tenantId });
        }

        /// <summary>
        /// Delete a punch record
        /// </summary>
        public async Task<bool> DeletePunchAsync(int id, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("DeletePunch");

            var rowsAffected = await conn.ExecuteAsync(query,
                new { Id = id, TenantId = tenantId });

            return rowsAffected > 0;
        }

        /// <summary>
        /// Get today's punch in/out logs from DeviceLog for a biometric number.
        /// </summary>
        public async Task<IEnumerable<TodayPunchLogItem>> GetTodayPunchLogsAsync(string biometricNumber, DateTime date)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetTodayPunchLogs");

            return await conn.QueryAsync<TodayPunchLogItem>(query, new
            {
                BiometricNumber = biometricNumber,
                LogDate = date.Date
            });
        }

        /// <summary>
        /// Get today's punch in/out logs from Punch table for an employee.
        /// </summary>
        public async Task<IEnumerable<TodayPunchLogItem>> GetTodayPunchLogsFromPunchAsync(int employeeId, int tenantId, DateTime date)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetTodayPunchLogsFromPunch");

            return await conn.QueryAsync<TodayPunchLogItem>(query, new
            {
                EmployeeId = employeeId,
                TenantId = tenantId,
                LogDate = date.Date
            });
        }
    }
}
