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
            string? punchInImage);
        Task UpdatePunchOut(
            int punchId,
            DateTime punchOut,
            double? duration,
            string outSource,
            string? coordinateOut,
            string? linkOut,
            string? punchOutImage,
            int userId = 0,
            string? punchOutReason = null);

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
            string? punchOutImage,
            string? punchOutReason = null);

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
        /// Gets punch by id for the given tenant.
        /// </summary>
        Task<Punch?> GetPunchByIdAsync(int id, int tenantId);

        /// <summary>
        /// Deletes the Punch record.
        /// </summary>
        Task<bool> DeletePunchAsync(int id, int tenantId);

        // Today punch logs (DeviceLog + Punch table)
        Task<IEnumerable<TodayPunchLogItem>> GetTodayPunchLogsAsync(string biometricNumber, DateTime date);
        Task<IEnumerable<TodayPunchLogItem>> GetTodayPunchLogsFromPunchAsync(int employeeId, int tenantId, DateTime date);
    }
}
