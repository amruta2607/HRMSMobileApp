namespace MobileWebApi.Constants
{
    /// <summary>
    /// Centralized exception codes for all modules.
    /// Each public constant should be unique and map to a specific method or layer.
    /// </summary>
    public static class ExceptionCodes
    {
        // General
        public const string UnknownError = "ERR000";

        // Repository layer (generic)
        public static class Repository
        {
            public const string DatabaseError = "REP001";
            public const string NotFound = "REP002";
            public const string AttendanceTodayPunchLogsDatabaseError = "REP003";
            public const string AttendanceGetPunchByEmployeeAndDateDatabaseError = "REP004";
            public const string AttendanceGetPunchByEmployeeAndDateWithTenantDatabaseError = "REP005";
            public const string AttendanceInsertPunchInDatabaseError = "REP006";
            public const string AttendanceUpdatePunchOutDatabaseError = "REP007";
            public const string AttendanceGetDailyAttendanceReportDatabaseError = "REP008";
            public const string AttendanceGetMonthlyAttendanceReportDatabaseError = "REP009";
            public const string AttendanceGetEmployeeAttendanceReportDatabaseError = "REP010";
            public const string AttendanceGetRealTimeAttendanceStatusDatabaseError = "REP011";
            public const string AttendanceGetCurrentlyPunchedInDatabaseError = "REP012";
            public const string AttendanceGetAttendanceByCalendarDatabaseError = "REP013";
            public const string AttendanceGetEmployeeByIdDatabaseError = "REP014";
            public const string AttendanceGetAttendanceReportsByOrganisationDatabaseError = "REP015";
            public const string AttendanceGetPunchByIdDatabaseError = "REP016";
            public const string AttendanceDeletePunchDatabaseError = "REP017";

            public const string AlertGetAlertByIdDatabaseError = "REP018";
            public const string AlertGetAlertsByUserIdDatabaseError = "REP019";
            public const string AlertGetAlertsByOrganisationIdDatabaseError = "REP020";
            public const string AlertGetAlertsDatabaseError = "REP021";
            public const string AlertGetUnreadCountDatabaseError = "REP022";
            public const string AlertCreateAlertDatabaseError = "REP023";
            public const string AlertUpdateAlertDatabaseError = "REP024";
            public const string AlertMarkAsReadDatabaseError = "REP025";
            public const string AlertMarkAllAsReadDatabaseError = "REP026";
            public const string AlertDeleteAlertDatabaseError = "REP027";
            public const string AlertDeactivateAlertDatabaseError = "REP028";
            public const string AlertApproveAlertDatabaseError = "REP029";
            public const string AlertRejectAlertDatabaseError = "REP030";

            public const string ApprovalInsertEventDatabaseError = "REP031";
            public const string ApprovalGetEventByIdDatabaseError = "REP032";
            public const string ApprovalUpdateEventStatusDatabaseError = "REP033";
            public const string ApprovalGetEventTypeIdDatabaseError = "REP034";
            public const string ApprovalGetEventTypeByIdDatabaseError = "REP035";
            public const string ApprovalIsEventTypeActiveDatabaseError = "REP036";
            public const string ApprovalGetFirstLevelNameDatabaseError = "REP037";
            public const string ApprovalGetApprovalStageByLevelNameDatabaseError = "REP038";
            public const string ApprovalIsApprovalStageActiveDatabaseError = "REP039";
            public const string ApprovalGetApproversForStageDatabaseError = "REP040";
            public const string ApprovalGetUserIdByEmployeeIdDatabaseError = "REP041";
            public const string ApprovalGetEmployeeNamesByUserIdsDatabaseError = "REP042";
            public const string ApprovalInsertApprovalDatabaseError = "REP043";
            public const string ApprovalUpdateApprovalStatusDatabaseError = "REP044";
            public const string ApprovalGetApprovalsByEventIdDatabaseError = "REP045";
            public const string ApprovalInsertScreenNotificationDatabaseError = "REP046";
            public const string ApprovalInsertEmailNotificationDatabaseError = "REP047";
            public const string ApprovalGetEmailTemplateDatabaseError = "REP048";
            public const string ApprovalGetNotificationTemplateDatabaseError = "REP049";
            public const string ApprovalGetTenantNameDatabaseError = "REP050";
            public const string ApprovalGetEmployeeByUserIdDatabaseError = "REP051";

            public const string DisputeGetDisputeCategoriesDatabaseError = "REP052";
            public const string DisputeGetEmployeeByIdDatabaseError = "REP053";
            public const string DisputeGetExistingDisputeDatabaseError = "REP054";
            public const string DisputeInsertDisputeDatabaseError = "REP055";

            public const string HolidayCreateHolidayDatabaseError = "REP056";
            public const string HolidayGetHolidayByIdDatabaseError = "REP057";
            public const string HolidayGetAllHolidaysDatabaseError = "REP058";
            public const string HolidayGetHolidaysWithFiltersDatabaseError = "REP059";
            public const string HolidayUpdateHolidayDatabaseError = "REP060";
            public const string HolidayDeleteHolidayDatabaseError = "REP061";
            public const string HolidayBulkCreateHolidaysDatabaseError = "REP062";

            public const string LeaveCreateLeaveRequestDatabaseError = "REP063";
            public const string LeaveGetLeaveRequestByIdDatabaseError = "REP064";
            public const string LeaveGetLeaveRequestsDatabaseError = "REP065";
            public const string LeaveGetLeaveRequestsByEmployeeIdDatabaseError = "REP066";
            public const string LeaveUpdateLeaveRequestStatusDatabaseError = "REP067";
            public const string LeaveGetLeaveBalanceByEmployeeIdDatabaseError = "REP068";
            public const string LeaveGetLeaveBalanceDatabaseError = "REP069";
            public const string LeaveUpdateLeaveBalanceDatabaseError = "REP070";
            public const string LeaveCreateLeaveTransactionDatabaseError = "REP071";
            public const string LeaveGetLeaveTransactionsByEmployeeIdDatabaseError = "REP072";
            public const string LeaveGetLeaveTypeIdByNameDatabaseError = "REP073";
            public const string LeaveGetEmployeeIdByUserIdDatabaseError = "REP074";
            public const string LeaveGenerateLeaveRequestNumberDatabaseError = "REP075";
            public const string LeaveGetTenantDayOffsDatabaseError = "REP076";
            public const string LeaveGetHolidaysDatabaseError = "REP077";
            public const string LeaveHasOverlappingLeaveDatabaseError = "REP078";
            public const string LeaveGetLastLeaveRequestNumberDatabaseError = "REP079";
            public const string LeaveGetLeaveHistoryDatabaseError = "REP080";

            public const string PaySlipGetPaySlipsDatabaseError = "REP081";
            public const string PaySlipGetPaySlipByIdDatabaseError = "REP082";
            public const string PaySlipGetPaySlipByEmployeeMonthYearDatabaseError = "REP083";
            public const string PaySlipGetEmployeeIdAndTenantByUserIdDatabaseError = "REP084";
            public const string PaySlipGetEmployeeProvidentFundSummaryDatabaseError = "REP085";
            public const string PaySlipGetLatestPayrollPeriodDatabaseError = "REP086";
            public const string PaySlipGetMonthlyPaymentSummaryDatabaseError = "REP087";
            public const string PaySlipGetPaySlipIncomesDatabaseError = "REP088";
            public const string PaySlipGetPaySlipDeductionsDatabaseError = "REP089";
            public const string PaySlipGetPaySlipWithWeekOffDatabaseError = "REP090";
            public const string PaySlipGetPaySlipMonthsByYearDatabaseError = "REP091";

            public const string UserGetUserByUsernameOrMobileDatabaseError = "REP092";
            public const string UserGetUserByUsernameForWebLoginDatabaseError = "REP093";
            public const string UserGetUserByEmailDatabaseError = "REP094";
            public const string UserGetUserByMobileDatabaseError = "REP095";
            public const string UserGetAllUsersByOrganisationDatabaseError = "REP096";
            public const string UserGetUserByIdDatabaseError = "REP097";
            public const string UserCreateUserDatabaseError = "REP098";
            public const string UserUpdateUserDatabaseError = "REP099";
            public const string UserDeleteUserDatabaseError = "REP100";
            public const string UserDeactivateUserDatabaseError = "REP101";
            public const string UserUpdatePasswordDatabaseError = "REP102";
            public const string TenantWeekOffGetConfigurationDatabaseError = "REP103";
            public const string TenantWeekOffGetWeekOffDaysDatabaseError = "REP104";
        }

        // Service layer (generic)
        public static class Service
        {
            public const string ProcessingError = "SER001";
            public const string ValidationError = "SER002";
        }

        // Controller layer (generic)
        public static class Controller
        {
            public const string BadRequest = "CON001";
            public const string UnhandledError = "CON002";
        }

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
            public const string RefreshToken = "AUTH-010";
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
            public const string GetTodayPunchLogs = "SHIFT-019";
        }

        // Pay / Payslip module (PaySlipService)
        public static class Pay
        {
            public const string GetPaySlips = "PAY-001";
            public const string GetPaySlipById = "PAY-002";
            public const string DownloadPaySlip = "PAY-003";
        }

        // PaySlip controller (PaySlipController)
        public static class PaySlip
        {
            public const string GetPaySlipYears = "PAYC-001";
            public const string GetPaySlipMonths = "PAYC-002";
            public const string GetPaySlipsGet = "PAYC-003";
            public const string GetPaySlipById = "PAYC-004";
            public const string DownloadPaySlipByMonthYear = "PAYC-005";
            public const string GetProvidentFund = "PAYC-006";
            public const string GetMonthlyPaymentSummary = "PAYC-007";
            public const string GetLastMonthPayroll = "PAYC-008";
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

        // User controller (UserController)
        public static class UserController
        {
            public const string GetUserById = "USRC-001";
            public const string DeleteUser = "USRC-002";
            public const string InactiveUser = "USRC-003";
        }

        // Leave module (LeaveService)
        public static class Leave
        {
            public const string CreateLeaveRequest = "LEAVE-001";
            public const string GetLeaveRequests = "LEAVE-002";
            public const string GetLeaveHistory = "LEAVE-003";
            public const string ApproveLeaveRequest = "LEAVE-004";
            public const string RejectLeaveRequest = "LEAVE-005";
            public const string WithdrawLeaveRequest = "LEAVE-006";
            public const string GetLeaveBalance = "LEAVE-007";
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

        // Mobile events (MobileEventController)
        public static class MobileEvent
        {
            public const string GetEvents = "MOBE-001";
        }

        // Geo-fencing (GeoFencingController)
        public static class GeoFencing
        {
            public const string GetTenantGeofence = "GEOF-001";
        }

        // Location tracking (LocationTrackingController)
        public static class LocationTracking
        {
            public const string RecordLocation = "LOCT-001";
            public const string RecordLocationBatch = "LOCT-002";
            public const string AddIssue = "LOCT-004";
        }

        // Location tracking configuration (MobileLocationTrackingConfigurationController)
        public static class LocationTrackingConfiguration
        {
            public const string GetConfiguration = "LOCT-003";
        }

        // Personal details (PersonalDetailsController)
        public static class PersonalDetails
        {
            public const string GetPersonalDetailsById = "PERS-001";
            public const string GetPersonalDetailsByUser = "PERS-002";
            public const string GetPersonalDetailsByBranch = "PERS-003";
            public const string AddPersonalDetails = "PERS-004";
            public const string UpdatePersonalDetailsPhoneAndPicture = "PERS-005";
            public const string DeletePersonalDetails = "PERS-006";
        }

        // SMS service (SmsService)
        public static class Sms
        {
            public const string SendOtp = "SMS-001";
            public const string SendViaWeb = "SMS-002";
            public const string SendViaTwilio = "SMS-003";
            public const string SendViaMsg91 = "SMS-004";
        }

        // OTP service (OtpService)
        public static class Otp
        {
            public const string GenerateOtp = "OTP-001";
            public const string GenerateMobileOtp = "OTP-002";
            public const string ValidateOtp = "OTP-003";
            public const string ValidateMobileOtp = "OTP-004";
            public const string RemoveOtp = "OTP-005";
            public const string RemoveMobileOtp = "OTP-006";
            public const string GetResendCooldownSeconds = "OTP-007";
        }

        // Approval workflow (ApprovalWorkflowService)
        public static class ApprovalWorkflow
        {
            public const string InitiateLeaveWorkflow = "WF-001";
            public const string InsertInitialStage = "WF-002";
        }

        // Asset dashboard (AssetDashboardRepository)
        public static class AssetDashboard
        {
            public const string GetDashboard = "ASSET-001";
        }

        // Asset list (AssetRepository)
        public static class Asset
        {
            public const string GetList = "ASSET-002";
            public const string Create = "ASSET-003";
        }

        // Asset hand over list (AssetHandOverRepository)
        public static class AssetHandOver
        {
            public const string GetList = "ASSET-004";
        }
    }
}

