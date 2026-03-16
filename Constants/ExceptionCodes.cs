namespace MobileWebApi.Constants
{
    /// <summary>
    /// Centralized exception codes for all modules.
    /// Each public constant should be unique and map to a specific method.
    /// </summary>
    public static class ExceptionCodes
    {
        // Employee module (EmployeeService)
        public static class Employee
        {
            public const string GetEmployeeById = "EMP-001";
            public const string GetLoggedInEmployee = "EMP-002";
            public const string GetEmployeesByBranch = "EMP-003";
            public const string GetEmployeesByBranchExceptUser = "EMP-004";
            public const string AddEmployee = "EMP-005";
            public const string UpdateEmployee = "EMP-006";
            public const string UpdateEmployeePhoneAndPicture = "EMP-007";
            public const string DeleteEmployee = "EMP-008";
            public const string DeactivateEmployee = "EMP-009";
        }

        // Authentication module (AuthController, TokenService)
        public static class Auth
        {
            public const string LoginWithEmail = "AUTH-001";
            public const string LoginMobile = "AUTH-002";
            public const string ResendOtp = "AUTH-003";
            public const string SendOtpForMobile = "AUTH-004";
            public const string VerifyOtpAndLogin = "AUTH-005";
            public const string Logout = "AUTH-006";
            public const string ForgotPassword = "AUTH-007";
            public const string ResetPassword = "AUTH-008";
            public const string ChangePassword = "AUTH-009";
        }

        // Attendance / Shift module (AttendanceService)
        public static class Attendance
        {
            // Use SHIFT-XXX prefix to align with requirement
            public const string PunchIn = "SHIFT-001";
            public const string PunchOut = "SHIFT-002";
            public const string GetAttendanceReport = "SHIFT-010";
            public const string GetEmployeeAttendance = "SHIFT-011";
            public const string GetRealTimeStatus = "SHIFT-012";
            public const string GetCurrentlyPunchedIn = "SHIFT-013";
            public const string GetCalendarAttendance = "SHIFT-014";
            public const string GetAttendanceSummary = "SHIFT-015";
            public const string GetOrganisationReports = "SHIFT-016";
            public const string DeleteAttendance = "SHIFT-017";
            public const string GetStatus = "SHIFT-018";
        }

        // Pay / Payslip module (PaySlipService)
        public static class Pay
        {
            public const string GetPaySlips = "PAY-001";
            public const string GetPaySlipById = "PAY-002";
            public const string DownloadPaySlip = "PAY-003";
        }

        // User module (UserService)
        public static class User
        {
            public const string GetUserById = "USR-001";
            public const string GetUserByLogin = "USR-002";
            public const string GetAllUsers = "USR-003";
            public const string CreateUser = "USR-004";
            public const string UpdateUser = "USR-005";
            public const string DeleteUser = "USR-006";
            public const string DeactivateUser = "USR-007";
        }

        // Leave module (LeaveService)
        public static class Leave
        {
            public const string CreateLeaveRequest = "LEAVE-001";
            public const string GetLeaveRequests = "LEAVE-002";
            public const string GetLeaveHistory = "LEAVE-003";
        }

        // Holiday module (HolidayService)
        public static class Holiday
        {
            public const string AddHoliday = "HOL-001";
            public const string GetHolidaysWithFilters = "HOL-002";
            public const string GetAllHolidays = "HOL-003";
            public const string UpdateHoliday = "HOL-004";
            public const string DeleteHoliday = "HOL-005";
            public const string AddBulkHolidays = "HOL-006";
            public const string UpdateHolidayDate = "HOL-007";
        }

        // Alert / Notification module (AlertService)
        public static class Alert
        {
            public const string GetAlertById = "ALERT-001";
            public const string GetAlertsByUser = "ALERT-002";
            public const string GetAlertsByOrganisation = "ALERT-003";
            public const string GetAlerts = "ALERT-004";
            public const string CreateAlert = "ALERT-005";
            public const string UpdateAlert = "ALERT-006";
            public const string MarkAsRead = "ALERT-007";
            public const string MarkAllAsRead = "ALERT-008";
            public const string DeleteAlert = "ALERT-009";
            public const string DeactivateAlert = "ALERT-010";
            public const string ApproveAlert = "ALERT-011";
            public const string RejectAlert = "ALERT-012";
            public const string SendApprovalNotification = "ALERT-013";
            public const string SendRejectionNotification = "ALERT-014";
            public const string ApproveRequestFromAlert = "ALERT-015";
            public const string RejectRequestFromAlert = "ALERT-016";
            public const string GetUnreadCountByUser = "ALERT-017";
        }

        // Attendance overview (AttendanceOverviewService)
        public static class AttendanceOverview
        {
            public const string GetOverview = "SHIFT-020";
        }

        // Mobile dashboard (MobileDashboardService)
        public static class MobileDashboard
        {
            public const string GetLatestTrainings = "MOBD-001";
            public const string GetLatestAnnouncements = "MOBD-002";
            public const string GetLatestEvents = "MOBD-003";
            public const string GetLatestHolidays = "MOBD-004";
        }

        // SMS service (SmsService)
        public static class Sms
        {
            public const string SendOtp = "SMS-001";
            public const string SendViaWeb = "SMS-002";
            public const string SendViaTwilio = "SMS-003";
            public const string SendViaMsg91 = "SMS-004";
        }

        // Approval workflow (ApprovalWorkflowService)
        public static class ApprovalWorkflow
        {
            public const string InitiateLeaveWorkflow = "WF-001";
            public const string InsertInitialStage = "WF-002";
        }
    }
}

