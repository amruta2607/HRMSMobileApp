using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<Punch?> GetPunchByEmployeeAndDate(int employeeId, DateTime punchDate);
        Task<Punch?> GetPunchByEmployeeAndDateWithTenant(int employeeId, DateTime punchDate, int tenantId);
        Task<Punch?> GetOpenPunchByEmployeeId(int employeeId);
        Task<int> InsertPunchIn(
            int employeeId,
            DateTime punchIn,
            DateTime punchDate,
            string inSource,
            string? coordinateIn,
            string? linkIn,
            string? imageUrl);
        Task UpdatePunchOut(
            int punchId,
            DateTime punchOut,
            double? duration,
            string outSource,
            string? coordinateOut,
            string? linkOut,
            string? imageUrl,
            bool manual,
            string? punchOutReason,
            int userId = 0);

        /// <summary>
        /// Gets today's punch record for an employee with tenant filter.
        /// </summary>
        Task<Punch?> GetTodayPunchAsync(int employeeId, int tenantId, DateTime punchDate);

        /// <summary>
        /// Gets the last punch tracking record for the current day.
        /// </summary>
        Task<PunchTracking?> GetLastPunchTrackingAsync(int employeeId, int tenantId, DateTime punchDate);

        /// <summary>
        /// Sums duration (in minutes) from completed OUT punch-tracking sessions for a punch record.
        /// </summary>
        Task<double> GetCompletedPunchTrackingDurationSumAsync(int punchId);

        /// <summary>
        /// Gets the most recent IN record not yet paired with an OUT for the punch.
        /// </summary>
        Task<PunchTracking?> GetLastUnmatchedPunchInAsync(int punchId);

        /// <summary>
        /// Inserts a punch tracking record.
        /// </summary>
        Task InsertPunchTrackingAsync(PunchTracking tracking);

        /// <summary>
        /// Updates punch-out on the Punch table (latest out time).
        /// </summary>
        Task UpdatePunchOutAsync(
            int punchId,
            DateTime punchOut,
            double? duration,
            int userId,
            string outSource,
            string? coordinateOut,
            string? linkOut,
            string? imageUrl,
            bool manual,
            string? punchOutReason);

        /// <summary>
        /// Inserts a punch-in and tracking record in a single transaction.
        /// </summary>
        Task<int> InsertPunchInWithTrackingAsync(
            int employeeId,
            int tenantId,
            DateTime punchIn,
            DateTime punchDate,
            string inSource,
            string? coordinateIn,
            string? linkIn,
            string? imageUrl,
            int userId,
            PunchTracking tracking);

        /// <summary>
        /// Inserts tracking and updates punch-out in a single transaction.
        /// </summary>
        /// <param name="totalPunchDuration">Total worked duration for the Punch table (sum of all sessions).</param>
        /// <param name="tracking">OUT tracking record; Duration must already hold the current session duration.</param>
        Task UpdatePunchOutWithTrackingAsync(
            int punchId,
            DateTime punchOut,
            double? totalPunchDuration,
            int userId,
            string outSource,
            string? coordinateOut,
            string? linkOut,
            string? imageUrl,
            bool manual,
            string? punchOutReason,
            PunchTracking tracking);
        Task<List<DateTime>> GetHolidayDatesAsync(int tenantId, DateTime fromDate, DateTime toDate);
        Task<List<(DateTime FromDate, DateTime ToDate)>> GetApprovedLeaveDateRangesAsync(int employeeId, DateTime fromDate, DateTime toDate);
        
        // Attendance Report Methods
        Task<IEnumerable<AttendanceReport>> GetAttendanceReportAsync(AttendanceReportRequest request);
        Task<IEnumerable<AttendanceReport>> GetDailyAttendanceReportAsync(int? branchId, DateTime calendarDate, int? organisationId, int? employeeId = null, int? departmentId = null);
        Task<IEnumerable<AttendanceReport>> GetMonthlyAttendanceReportAsync(int? branchId, DateTime dateFrom, DateTime dateTo, int? organisationId, int? employeeId = null, int? departmentId = null);
        
        // Employee-specific Attendance
        Task<IEnumerable<AttendanceReport>> GetEmployeeAttendanceReportAsync(int employeeId, DateTime dateFrom, DateTime dateTo);
        
        // Real-time Attendance Status
        Task<IEnumerable<RealTimeAttendanceStatus>> GetRealTimeAttendanceStatusAsync(DateTime punchDate, int? organisationId = null, int? branchId = null, int? departmentId = null);
        Task<IEnumerable<RealTimeAttendanceStatus>> GetCurrentlyPunchedInAsync(DateTime punchDate, int? organisationId = null, int? branchId = null, int? departmentId = null);
        
        // Calendar-based Attendance
        Task<IEnumerable<AttendanceReport>> GetAttendanceByCalendarAsync(int employeeId, int month, int year);
        Task<Employee?> GetEmployeeByIdAsync(int employeeId);
        
        // Organization-based Attendance Reports
        Task<IEnumerable<AttendanceReport>> GetAttendanceReportsByOrganisationAsync(int organisationId, DateTime dateFrom, DateTime dateTo);
        
        // Delete Attendance
        /// <summary>
        /// Deletes punch and related PunchTracking rows for the given punch id.
        /// </summary>
        Task<Punch?> GetPunchByIdAsync(int id, int tenantId);

        /// <summary>
        /// Deletes PunchTracking then Punch in a single transaction.
        /// </summary>
        Task<bool> DeletePunchAsync(int id, int tenantId);

        // Today punch logs (DeviceLog + Punch table)
        Task<IEnumerable<TodayPunchLogItem>> GetTodayPunchLogsAsync(string biometricNumber, DateTime date);
        Task<IEnumerable<TodayPunchLogItem>> GetTodayPunchLogsFromPunchAsync(int employeeId, int tenantId, DateTime date);
    }
}