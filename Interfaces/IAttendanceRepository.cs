using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<Punch?> GetPunchByEmployeeAndDate(int employeeId, DateTime punchDate);
        Task<Punch?> GetPunchByEmployeeAndDateWithTenant(int employeeId, DateTime punchDate, int tenantId);
        Task<int> InsertPunchIn(int employeeId, DateTime punchIn, DateTime punchDate);
        Task UpdatePunchOut(int employeeId, DateTime punchOut, DateTime punchDate, double? duration);
        
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
        Task<Punch?> GetPunchByIdAsync(int id, int tenantId);
        Task<bool> DeletePunchAsync(int id, int tenantId);
    }
}