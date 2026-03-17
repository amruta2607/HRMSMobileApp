using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IAttendanceService
    {
        Task<string> PunchInAsync(PunchInRequest req);
        Task<string> PunchOutAsync(PunchOutRequest req);
        
        // Attendance Report Methods
        Task<AttendanceReportResponse> GetAttendanceReportAsync(AttendanceReportRequest request);
        
        // Employee-specific Attendance
        Task<AttendanceReportResponse> GetEmployeeAttendanceAsync(int userId, DateTime dateFrom, DateTime dateTo);
        
        // Real-time Attendance Status
        Task<RealTimeAttendanceResponse> GetRealTimeAttendanceStatusAsync(DateTime? punchDate = null, int? organisationId = null, int? branchId = null, int? departmentId = null);
        Task<RealTimeAttendanceResponse> GetCurrentlyPunchedInAsync(DateTime? punchDate = null, int? organisationId = null, int? branchId = null, int? departmentId = null);
        
        // Calendar-based Attendance
        Task<CalendarAttendanceResponse> GetAttendanceByCalendarAsync(int userId, int month, int year);
        
        // Attendance Summary
        Task<AttendanceSummaryResponse> GetAttendanceSummaryAsync(int organizationId, int userId, DateTime fromDate, DateTime toDate);
        
        // Organization-based Attendance Reports
        Task<AttendanceReportResponse> GetAttendanceReportsByOrganisationAsync(int organisationId);
        
        // Delete Attendance
        Task<AttendanceDeleteResponse> DeleteAttendanceAsync(int id, int tenantId);
        
        // Get Attendance Status
        Task<AttendanceStatusResponse> GetAttendanceStatusAsync(int userId, DateTime date, int tenantId);

        // Today's punch logs for logged-in user
        Task<TodayPunchLogsResponse> GetTodayPunchLogsAsync(int userId, int tenantId);
    }
}