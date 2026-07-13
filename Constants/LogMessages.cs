namespace MobileWebApi.Constants
{
    /// <summary>
    /// Log messages for consistent logging across the application
    /// </summary>
    public static class LogMessages
    {
        // User related logs
        public static class User
        {
            public const string RetrievingUserById = "Retrieving user with ID: {UserId}";
            public const string RetrievingUserByLogin = "Retrieving user with login: {Login}";
            public const string RetrievingUsersForTenant = "Retrieving users for tenant: {TenantId}";
            public const string CreatingUser = "Creating new user: {Username}";
            public const string AddingUser = "Adding new user: {Username}";
            public const string UpdatingUser = "Updating user with ID: {UserId}";
            public const string DeletingUser = "Deleting user with ID: {UserId}";
            public const string DeletingUserHard = "Hard deleting user with ID: {UserId}";
            public const string DeactivatingUser = "Deactivating user with ID: {UserId}";
            public const string UserNotFound = "User not found with ID: {UserId}";
            public const string ErrorRetrievingUser = "Error retrieving user with ID {UserId}";
            public const string ErrorRetrievingUserByLogin = "Error retrieving user with login {Login}";
            public const string ErrorRetrievingUsers = "Error retrieving users for tenant {TenantId}";
            public const string ErrorCreatingUser = "Error creating user";
            public const string ErrorUpdatingUser = "Error updating user with ID {UserId}";
            public const string ErrorDeletingUser = "Error deleting user with ID {UserId}";
            public const string ErrorDeactivatingUser = "Error deactivating user with ID {UserId}";
            public const string ErrorFetchingUsersByOrganisationId = "Error fetching users by OrganisationId.";

			public const string ErrorFetchingTenantConfigurationByOrganisationId = "Error fetching TenantConfiguration by OrganisationId.";
			public const string RetrievingCompanyLogoForOrganisation = "Retrieving company logo for organisation: {OrganisationId}";


		}

		// Employee related logs
		public static class Employee
        {
            public const string RetrievingEmployeeById = "Retrieving employee with ID: {Id}";
            public const string RetrievingEmployeeByUserId = "Retrieving employee for user: {UserId}";
            public const string RetrievingEmployeesByBranch = "Retrieving employees for branch: {BranchId}";
            public const string AddingEmployee = "Adding new employee";
            public const string UpdatingEmployee = "Updating employee with ID: {EmployeeId}";
            public const string DeletingEmployee = "Deleting employee with ID: {Id}";
            public const string DeactivatingEmployee = "Deactivating employee with ID: {Id}";
            public const string EmployeeNotFound = "Employee not found with ID: {Id}";
            public const string ErrorRetrievingEmployee = "Error retrieving employee with ID {Id}";
            public const string ErrorRetrievingEmployeeByUser = "Error retrieving employee for user {UserId}";
            public const string ErrorRetrievingEmployeesByBranch = "Error retrieving employees for branch {BranchId}";
            public const string ErrorAddingEmployee = "Error adding employee";
            public const string ErrorUpdatingEmployee = "Error updating employee with ID {EmployeeId}";
            public const string ErrorDeletingEmployee = "Error deleting employee with ID {Id}";
            public const string ErrorDeactivatingEmployee = "Error deactivating employee with ID {Id}";
        }

        // Attendance related logs
        public static class Attendance
        {
            public const string ProcessingPunchIn = "Processing punch-in for employee: {EmployeeId}";
            public const string ProcessingPunchOut = "Processing punch-out for employee: {EmployeeId}";
            public const string FetchingAttendanceReport = "Fetching attendance report";
            public const string PunchInSuccessful = "Punch-in successful for employee: {EmployeeId}";
            public const string PunchOutSuccessful = "Punch-out successful for employee: {EmployeeId}";
            public const string ErrorProcessingPunchIn = "Error processing punch-in for employee {EmployeeId}";
            public const string ErrorProcessingPunchOut = "Error processing punch-out for employee {EmployeeId}";
            public const string ErrorFetchingAttendanceReport = "Error fetching attendance report";
            
            // Daily/Monthly report logs
            public const string FetchingDailyReport = "Fetching daily attendance report for date: {CalendarDate}";
            public const string FetchingMonthlyReport = "Fetching monthly attendance report from {DateFrom} to {DateTo}";
            
            // Employee attendance logs
            public const string FetchingEmployeeAttendance = "Fetching attendance for employee {EmployeeId} from {DateFrom} to {DateTo}";
            public const string ErrorFetchingEmployeeAttendance = "Error fetching employee attendance for employee {EmployeeId}";
            
            // Real-time attendance logs
            public const string FetchingRealTimeStatus = "Fetching real-time attendance status for {PunchDate}";
            public const string FetchingCurrentlyPunchedIn = "Fetching currently punched in employees for {PunchDate}";
            public const string ErrorFetchingRealTimeStatus = "Error fetching real-time attendance status";
            public const string ErrorFetchingCurrentlyPunchedIn = "Error fetching currently punched in employees";
            
            // Calendar attendance logs
            public const string FetchingCalendarAttendance = "Fetching calendar attendance for employee {EmployeeId}, month {Month}, year {Year}";
            public const string FetchingAttendanceByPersonalDetails = "Fetching attendance by personal details for employee {EmployeeId}";
            public const string ErrorFetchingCalendarAttendance = "Error fetching calendar attendance for employee {EmployeeId}";
            
            // Get attendance logs
            public const string FetchingAttendance = "Fetching attendance for date {Date}";
            
            // Attendance summary logs
            public const string FetchingAttendanceSummary = "Fetching attendance summary for employee {EmployeeId} from {FromDate} to {ToDate}";
            public const string ErrorFetchingAttendanceSummary = "Error fetching attendance summary for employee {EmployeeId}";
            
            // Organisation attendance logs
            public const string FetchingAttendanceReportsForOrganisation = "Fetching attendance reports for organisation {OrganisationId}";
            public const string ErrorFetchingAttendanceReportsForOrganisation = "Error fetching attendance reports for organisation {OrganisationId}";
            
            // Attendance Overview logs
            public const string FetchingAttendanceOverview = "Fetching attendance overview for employee {EmployeeId}, organisation {OrganisationId}, from {FromDate} to {ToDate}";
            public const string AttendanceOverviewEffectiveRange = "Effective date range for attendance overview: FromDate={FromDate}, ToDate={ToDate}";
            public const string AttendanceOverviewWorkingHoursAndDays = "Attendance overview - WorkingHours: {WorkingHours}, WorkingDays: {WorkingDays}";
            public const string AttendanceOverviewDayOffIds = "Attendance overview - Day off IDs for OrganisationId {OrganisationId}: {DayOffIds}";
            public const string AttendanceOverviewDateRangeCalculation = "Date range: {FromDate} to {ToDate}, Total days: {TotalDays}, Working days: {WorkingDays}, Day offs excluded: {DayOffs}";
            public const string AttendanceOverviewDateDetails = "Date: {Date} ({DayName}), DayOfWeek: {DayOfWeek}, DayOffId: {DayOffId}, IsDayOff: {IsDayOff}, WorkingDays: {WorkingDays}";
            public const string DeletingAttendanceRecord = "Deleting attendance record with ID: {Id}";
            public const string ErrorDeletingAttendanceRecord = "Error deleting attendance record with ID {Id}";
            public const string GettingAttendanceStatus = "Getting attendance status for employee {EmployeeId} on date {Date}";
            public const string ErrorGettingAttendanceStatus = "Error getting attendance status for employee {EmployeeId} on date {Date}";
            public const string GettingAttendanceOverview = "Getting attendance overview for employee {EmployeeId}, tenant {TenantId}, from {FromDate} to {ToDate}";
            public const string ErrorFetchingAttendanceOverview = "Error fetching attendance overview for employee {EmployeeId}";
            public const string InvalidDateForOperation = "Invalid date for {OperationType}: Requested date {RequestDate} does not match today's date {TodayDate}";
            public const string FetchingTodayPunchLogs = "Fetching today's punch logs for user {UserId}";
            public const string FetchingTenantWeekOffDays = "Fetching tenant week-off days for tenant {TenantId}";
            public const string ErrorFetchingTenantWeekOffDays = "Error fetching tenant week-off days for tenant {TenantId}";
        }

        // Leave related logs
        public static class Leave
        {
            public const string CreatingLeaveRequest = "Creating leave request for user: {UserId}";
            public const string FetchingLeaveRequests = "Fetching leave requests";
            public const string FetchingLeaveRequestById = "Fetching leave request with ID: {Id}";
            public const string FetchingLeaveRequestsByFilter = "Fetching leave requests for user {UserId}, organization {OrganizationId}, status {Status}";
            public const string FetchingLeaveHistory = "Fetching leave history for user: {UserId}";
            public const string ApprovingLeaveRequest = "Approving leave request with ID: {Id}";
            public const string RejectingLeaveRequest = "Rejecting leave request with ID: {Id}";
            public const string CancellingLeaveRequest = "Cancelling leave request with ID: {Id}";
            public const string FetchingLeaveBalance = "Fetching leave balance for user: {UserId}";
            public const string ErrorCreatingLeaveRequest = "Error creating leave request";
            public const string ErrorFetchingLeaveRequests = "Error fetching leave requests";
            public const string ErrorFetchingLeaveRequestById = "Error fetching leave request by ID";
            public const string ErrorApprovingLeaveRequest = "Error approving leave request";
            public const string ErrorRejectingLeaveRequest = "Error rejecting leave request";
            public const string ErrorCancellingLeaveRequest = "Error cancelling leave request";
            public const string ErrorFetchingLeaveBalance = "Error fetching leave balance";
        }

        // Alert related logs
        public static class Alert
        {
            public const string RetrievingAlertById = "Retrieving alert with ID: {Id}";
            public const string RetrievingAlertsForUser = "Retrieving alerts for user: {UserId}";
            public const string RetrievingAlertsForTenant = "Retrieving alerts for tenant: {TenantId}";
            public const string CreatingAlert = "Creating new alert";
            public const string UpdatingAlert = "Updating alert with ID: {Id}";
            public const string MarkingAlertAsRead = "Marking alert as read with ID: {Id}";
            public const string MarkingAllAlertsAsRead = "Marking all alerts as read for user: {UserId}";
            public const string DeactivatingAlert = "Deactivating alert with ID: {Id}";
            public const string DeletingAlert = "Deleting alert with ID: {Id}";
            public const string ApprovingAlert = "Approving alert with ID: {Id}";
            public const string RejectingAlert = "Rejecting alert with ID: {Id}";
            public const string ErrorRetrievingAlert = "Error retrieving alert with ID {Id}";
            public const string ErrorRetrievingAlertsForUser = "Error retrieving alerts for user {UserId}";
            public const string ErrorRetrievingAlertsForTenant = "Error retrieving alerts for tenant {TenantId}";
            public const string ErrorRetrievingAlerts = "Error retrieving alerts";
            public const string ErrorCreatingAlert = "Error creating alert";
            public const string ErrorUpdatingAlert = "Error updating alert with ID {Id}";
            public const string ErrorMarkingAlertAsRead = "Error marking alert as read with ID {Id}";
            public const string ErrorMarkingAllAlertsAsRead = "Error marking all alerts as read for user {UserId}";
            public const string ErrorDeactivatingAlert = "Error deactivating alert with ID {Id}";
            public const string ErrorDeletingAlert = "Error deleting alert with ID {Id}";
            public const string ErrorApprovingAlert = "Error approving alert with ID {Id}";
            public const string ErrorRejectingAlert = "Error rejecting alert with ID {Id}";
            
            // Notification logs
            public const string SendingApprovalNotification = "Sending approval notification to user {RequesterUserId} for event {EventName}";
            public const string ErrorSendingApprovalNotification = "Error sending approval notification to user {RequesterUserId}";
            public const string SendingRejectionNotification = "Sending rejection notification to user {RequesterUserId} for event {EventName}";
            public const string ErrorSendingRejectionNotification = "Error sending rejection notification to user {RequesterUserId}";
            
            // Request approval/rejection from alert logs
            public const string ApprovingRequestFromAlert = "Approving request from alert {AlertId}";
            public const string ErrorApprovingRequestFromAlert = "Error approving request from alert {AlertId}";
            public const string RejectingRequestFromAlert = "Rejecting request from alert {AlertId}";
            public const string ErrorRejectingRequestFromAlert = "Error rejecting request from alert {AlertId}";
            public const string ErrorParsingEventDataJson = "Error parsing EventData JSON for event {EventId}";
            public const string FailedToUpdateEventStatus = "Failed to update Event status for event {EventId}";
            public const string ErrorExtractingRequestIdFromEventData = "Error extracting request ID from event data for event: {EventName}";
            public const string ErrorExtractingEventDetails = "Error extracting event details from database for event {EventId}";
            public const string ErrorParsingEventDataForTokenReplacement = "Error parsing EventData JSON for token replacement in event {EventId}";
            public const string ErrorGettingNotificationTemplate = "Error getting notification template for {EventName}, using fallback";
            public const string ErrorUpdatingEventAndApprovalStatus = "Error updating event and approval status for event {EventId}";
            public const string ErrorUpdatingEventAndApprovalStatusForRejection = "Error updating event and approval status for rejection of event {EventId}";
        }
        
        // Controller related logs
        public static class Controller
        {
            public const string InvalidAlertId = "Invalid alert ID";
        }

        // Authentication related logs
        public static class Auth
        {
            public const string LoginAttempt = "Login attempt for user: {Username}";
            public const string LoginSuccessful = "Login successful for user: {Username}";
            public const string LoginFailed = "Login failed for user: {Username}";
            public const string TokenGenerated = "Token generated for user: {Username}";
            public const string ErrorGeneratingToken = "Error generating token for user {Username}";
            public const string LogoutAttempt = "Logout attempt for user: {Username}";
            public const string LogoutSuccessful = "Logout successful for user: {Username}";
            public const string LogoutUserIdClaimNotFound = "Logout attempted but UserId claim not found in token";
            public const string AccessTokenBlacklisted = "Access token blacklisted for user {Username} (UserId: {UserId})";
            public const string RefreshTokenInvalidated = "Refresh token invalidated for user {Username} (UserId: {UserId})";
            public const string AllRefreshTokensRemoved = "All refresh tokens removed for user {Username} (UserId: {UserId})";
            public const string RefreshTokenSecurityViolation = "User {Username} (UserId: {UserId}) attempted to invalidate refresh token belonging to another user";
            public const string ErrorDuringLogout = "Error during logout for user {Username}";
            public const string ForgotPasswordRequest = "Forgot password request for: {Username}";
            public const string OtpGenerated = "OTP generated for user: {Username}";
            public const string OtpVerificationAttempt = "OTP verification attempt for: {Username}";
            public const string OtpVerificationFailed = "OTP verification failed for: {Username}";
            public const string PasswordResetSuccessful = "Password reset successful for user: {Username}";
            public const string PasswordResetFailed = "Password reset failed for user: {Username}";
            public const string ChangePasswordAttempt = "Change password attempt for user: {Username}";
            public const string ChangePasswordSuccessful = "Password changed successful for user: {Username}";
            public const string ChangePasswordFailed = "Password change failed for user: {Username}";
            public const string CurrentPasswordIncorrect = "Current password incorrect for user: {Username}";
            
            // Refresh token logs
            public const string RefreshTokenAttempt = "Refresh token attempt";
            public const string RefreshTokenSuccessful = "Refresh token successful for user: {Username}";
            public const string RefreshTokenInvalid = "Invalid refresh token provided";
            public const string RefreshTokenExpired = "Refresh token has expired";
            public const string RefreshTokenRevoked = "Refresh token has been revoked";
        }

        // Pay Slip related logs
        public static class PaySlip
        {
            public const string FetchingPaySlips = "Fetching pay slips for user: {UserId}";
            public const string FetchingPaySlipById = "Fetching pay slip with ID: {Id}";
            public const string DownloadingPaySlip = "Downloading pay slip with ID: {Id}";
            public const string ErrorFetchingPaySlips = "Error fetching pay slips";
            public const string ErrorFetchingPaySlipById = "Error fetching pay slip by ID";
            public const string ErrorDownloadingPaySlip = "Error downloading pay slip";
            public const string ErrorFetchingProvidentFund = "Error fetching Provident Fund";
            public const string ErrorFetchingMonthlyPaymentSummary = "Error fetching monthly payment summary";
            public const string ErrorFetchingLastMonthPayroll = "Error fetching last month payroll";
            public const string ErrorDownloadingPaySlipByMonthYear = "Error downloading payslip by month/year";
            public const string ErrorFetchingPaySlipYears = "Error fetching payslip years";
            public const string ErrorFetchingPaySlipMonthsForYear = "Error fetching payslip months for year {Year}";
        }

        // General logs
        public static class General
        {
            public const string ApplicationStarted = "Application started";
            public const string ApplicationStopped = "Application stopped";
            public const string RequestReceived = "Request received: {Method} {Path}";
            public const string ResponseSent = "Response sent: {StatusCode}";
            public const string UnhandledException = "Unhandled exception occurred";
        }

        // OTP related logs
        public static class Otp
        {
            public const string OtpGenerated = "OTP generated for identifier: {Identifier}";
            public const string OtpValidated = "OTP validated for identifier: {Identifier}";
            public const string OtpValidationFailed = "OTP validation failed for identifier: {Identifier}";
            public const string OtpRemoved = "OTP removed for identifier: {Identifier}";
            
            // Mobile OTP logs
            public const string MobileOtpGenerated = "Mobile OTP generated for {MobileNumber}. Resend available in {Seconds} seconds";
            public const string MobileOtpValidatedSuccessfully = "Mobile OTP validated successfully for {MobileNumber}";
            public const string MobileOtpNotFoundOrExpired = "Mobile OTP not found or expired for {MobileNumber}";
            public const string MobileOtpExpired = "Mobile OTP expired for {MobileNumber}";
            public const string InvalidMobileOtpAttempt = "Invalid mobile OTP attempt {Attempt} for {MobileNumber}";
            public const string RateLimitExceeded = "Rate limit exceeded for mobile {MobileNumber}. Reset in {Seconds} seconds";
            public const string ResendCooldownActive = "Resend cooldown active for mobile {MobileNumber}. Available in {Seconds} seconds";
            
            // SMS OTP logs
            public const string SendingOtp = "Sending OTP to {MobileNumber} using provider: {Provider}";
            public const string SmsOtpSentSuccessfully = "SMS OTP sent successfully to {MobileNumber}";
            public const string FailedToSendSmsOtp = "Failed to send SMS OTP to {MobileNumber}";
            public const string StubModeSmsOtp = "STUB MODE: SMS OTP for {MobileNumber}: {Otp}";
            
            // Mobile login OTP logs
            public const string LoginMobileRequestNull = "LoginMobile: Request is null";
            public const string LoginMobileMobileNumberMissing = "LoginMobile: Mobile number is missing";
            public const string LoginMobileMobileHasOtp = "LoginMobile: Mobile={MobileNumber}, HasOtp={HasOtp}";
            public const string LoginMobileVerifyingOtp = "LoginMobile: Verifying OTP for {MobileNumber}";
            public const string LoginMobileSendingOtp = "LoginMobile: Sending OTP for {MobileNumber}";
            public const string LoginMobileUnexpectedError = "LoginMobile: Unexpected error";
            public const string ResendOtpRequest = "Resend OTP request for mobile: {MobileNumber}";
            public const string OtpResentSuccessfully = "OTP resent successfully to {MobileNumber}";
            public const string OtpSendRequest = "OTP send request for mobile: {MobileNumber}";
            public const string OtpSentSuccessfully = "OTP sent successfully to {MobileNumber}";
            public const string OtpVerificationAttempt = "OTP verification attempt for mobile: {MobileNumber}";
            public const string InvalidOtpForMobile = "Invalid OTP for mobile: {MobileNumber}";
            public const string OtpVerifiedSuccessfully = "OTP verified successfully for mobile: {MobileNumber}, UserId: {UserId}, EmployeeId: {EmployeeId}";
        }

        // Location related logs
        public static class Location
        {
            public const string FetchingLocations = "Fetching locations for user_id: {UserId}, organization_id: {OrganizationId}, branchId: {BranchId}";
            public const string ErrorFetchingLocations = "Error fetching locations";
            public const string ExecutingGetLocationsAsync = "Executing GetLocationsAsync with params: UserId={UserId}, OrgId={OrgId}, BranchId={BranchId}";
            public const string FetchedLocationsCount = "Fetched {Count} locations";
            public const string ErrorFetchingLocationsForUserId = "Error fetching locations for UserId={UserId}";
        }

        // Location tracking related logs
        public static class LocationTracking
        {
            public const string RecordingLocation = "Recording location for employee {EmployeeId}, tenant {TenantId}";
            public const string FailedToRecordLocation = "Failed to record location for employee {EmployeeId}";
            public const string ProcessingLocationBatch = "Processing location batch for employee {EmployeeId}, tenant {TenantId}, record count {RecordCount}";
            public const string FailedToRecordLocationBatch = "Failed to record location batch for employee {EmployeeId}";
        }

        public static class LocationTrackingIssue
        {
            public const string ApiRequestReceived = "Location tracking issue API request received from user {UserId}";
            public const string AuthenticatedUser = "Authenticated user {UserId} ({Username}) submitting location tracking issue";
            public const string ValidationFailed = "Location tracking issue validation failed for user {UserId}: {ValidationMessage}";
            public const string LoggingIssue = "Logging location tracking issue for employee {EmployeeId}, tenant {TenantId}, issue type {IssueType}, user {UserId}";
            public const string IssueLoggedSuccessfully = "Location tracking issue {IssueId} logged for employee {EmployeeId}, tenant {TenantId}, user {UserId}";
            public const string FailedToInsert = "Failed to insert location tracking issue for employee {EmployeeId}, tenant {TenantId}";
            public const string EmployeeNotFound = "Employee not found for user {UserId} while logging location tracking issue by user {CurrentUserId}";
            public const string TenantNotFound = "Tenant {TenantId} not found while logging location tracking issue for user {UserId}";
            public const string EmployeeTenantMismatch = "Employee user {UserId} does not belong to tenant {TenantId} while logging issue by user {CurrentUserId}";
            public const string NoAdminRecipients = "No active HR or tenant admin users found for tenant {TenantId} to notify about location tracking issue";
            public const string AdminNotificationsSent = "Sent location tracking issue notifications to {RecipientCount} admin users for issue {IssueId}, tenant {TenantId}";
            public const string AdminNotificationFailed = "Failed to send admin notifications for location tracking issue {IssueId}, tenant {TenantId}";
        }

        public static class LocationTrackingConfiguration
        {
            public const string FetchingConfiguration = "Fetching location tracking configuration for employee {EmployeeId}, tenant {TenantId}";
        }

        // Transaction related logs
        public static class Transaction
        {
            public const string CreatingTransaction = "Creating transaction for employee {EmployeeId}";
            public const string TransactionCreated = "Transaction created successfully";
            public const string TransactionTableNotExist = "Could not create leave transaction record. Table may not exist.";
        }

        // Tenant access logs
        public static class TenantAccess
        {
            public const string TenantAccessViolation = "Tenant access violation";
            public const string UserAccessViolation = "User {UserId} attempted to access data for user {RequestedUserId} without permission";
            public const string UnauthorizedAccessToPersonalDetails = "User {CurrentUserId} attempted to access personal details for employee {EmployeeId} (UserId: {UserId})";
            public const string UnauthorizedUpdatePersonalDetails = "User {CurrentUserId} attempted to update personal details for employee {EmployeeId}";
            public const string UnauthorizedDeletePersonalDetails = "User {CurrentUserId} attempted to delete personal details for employee {EmployeeId}";
            public const string UnauthorizedDeactivateEmployee = "User {CurrentUserId} attempted to deactivate employee {EmployeeId}";
            public const string UserAttemptedAccessAttendance = "User {CurrentUserId} attempted to access attendance for employee {EmployeeId} (SystemUserId: {SystemUserId})";
            public const string UserAttemptedPunchIn = "User {CurrentUserId} attempted to punch in for employee {EmployeeId} (SystemUserId: {SystemUserId})";
            public const string UserAttemptedPunchOut = "User {CurrentUserId} attempted to punch out for employee {EmployeeId} (SystemUserId: {SystemUserId})";
        }

        // Email related logs
        public static class Email
        {
            public const string SendingEmail = "Sending email to: {Email}";
            public const string EmailSentSuccessfully = "Email sent successfully to: {Email}";
            public const string SendingOtpEmail = "Sending OTP email to: {Email}";
            public const string OtpEmailSentSuccessfully = "OTP email sent successfully to: {Email}";
            public const string SendingPasswordResetConfirmation = "Sending password reset confirmation to: {Email}";
            public const string PasswordResetConfirmationSent = "Password reset confirmation sent to: {Email}";
            public const string FailedToSendEmail = "Failed to send email to: {Email}";
            public const string FailedToSendOtpEmail = "Failed to send OTP email to user: {Username}";
            public const string ErrorSendingEmail = "Error sending email to {Email}";
        }

        // Approval Workflow related logs
        public static class ApprovalWorkflow
        {
            // Event logs
            public const string InitiatingApprovalWorkflow = "Initiating approval workflow for LeaveRequest ID: {LeaveRequestId}";
            public const string EventTypeNotFound = "Event type '{EventName}' not found for tenant {TenantId}";
            public const string EventTypeNotActive = "Event type '{EventName}' is not active for tenant {TenantId}";
            public const string EventInsertedSuccessfully = "Event inserted successfully. EventId: {EventId}";
            public const string FailedToInsertEvent = "Failed to insert event for leave request";
            public const string ErrorInitiatingWorkflow = "Error initiating leave request approval workflow";

            // Approval Stage logs
            public const string NoApprovalLevelsConfigured = "No approval levels configured for event type {EventTypeId}";
            public const string ApprovalStageNotFound = "Approval stage not found for level '{LevelName}'";
            public const string ApprovalStageNotActive = "Approval stage {StageId} is not active";
            public const string NoApproversFound = "No approvers found for stage {StageId}";
            public const string InitialApprovalStageInserted = "Initial approval stage inserted. EventId: {EventId}, StageId: {StageId}, Approvers: {ApproverCount}";
            public const string ErrorInsertingApprovalStage = "Error inserting initial approval stage for event {EventId}";

            // Notification logs
            public const string ScreenNotificationCreated = "Screen notification created for approver {ApproverId}";
            public const string EmailNotificationQueued = "Email notification queued for {Email}";
            public const string FailedToSendEmailNotification = "Failed to send email to {Email}";
            public const string ErrorParsingEventData = "Error parsing event data for token replacement";

            // Approval action logs
            public const string ApprovalWorkflowInitiated = "Approval workflow initiated for leave request {LeaveRequestId}. EventId: {EventId}";
            public const string FailedToInitiateWorkflow = "Failed to initiate approval workflow for leave request {LeaveRequestId}: {Message}";
            public const string WorkflowNotConfigured = "Error initiating approval workflow for leave request {LeaveRequestId}. Workflow may not be configured.";
            public const string ApprovalWorkflowNotConfigured = "Approval workflow not configured";
            public const string ErrorExtractingEventDetails = "Error extracting event details for event {EventId}";
            public const string ErrorUpdatingPayrollApprovalStatus = "Error updating payroll approval status for PayrollId {PayrollId} and TenantId {TenantId}";
        }

        // Holiday related logs
        public static class Holiday
        {
            public const string CreatingHoliday = "Creating holiday: {HolidayName}";
            public const string FetchingHolidays = "Fetching holidays for tenant: {TenantId}";
            public const string UpdatingHoliday = "Updating holiday with ID: {Id}";
            public const string DeletingHoliday = "Deleting holiday with ID: {Id}";
            public const string CreatingBulkHolidays = "Creating bulk holidays. Count: {Count}";
            public const string ErrorCreatingHoliday = "Error creating holiday";
            public const string ErrorFetchingHolidays = "Error fetching holidays";
            public const string ErrorUpdatingHoliday = "Error updating holiday";
            public const string ErrorDeletingHoliday = "Error deleting holiday";
            public const string ErrorCreatingBulkHolidays = "Error creating bulk holidays";
        }

        // Mobile dashboard (events/announcements/holidays) logs
        public static class MobileDashboard
        {
            public const string ErrorFetchingLatestEvents = "Error fetching latest events for mobile dashboard.";
            public const string ErrorFetchingLatestAnnouncements = "Error fetching latest announcements for mobile dashboard.";
            public const string ErrorFetchingLatestHolidays = "Error fetching latest holidays for mobile dashboard.";
            public const string ErrorFetchingLatestTrainings = "Error fetching latest trainings for mobile dashboard.";
        }

        // Asset dashboard logs
        public static class AssetDashboard
        {
            public const string ErrorFetchingDashboard = "Error fetching asset dashboard for organisation {OrganisationId}.";
        }

        // Asset list logs
        public static class Asset
        {
            public const string ErrorFetchingAssets = "Error fetching asset list for organisation {OrganisationId}.";
            public const string ErrorCreatingAsset = "Error creating asset for organisation {OrganisationId}.";
            public const string ErrorUpdatingAsset = "Error updating asset {AssetId} for organisation {OrganisationId}.";
            public const string AssetUpdated = "Asset {AssetId} updated by user {UserId} for organisation {OrganisationId} at {Timestamp}.";
            public const string FetchingLookups = "Fetching asset lookups for user {UserId} in organisation {OrganisationId}.";
            public const string LookupsFetched = "Fetched asset lookups for organisation {OrganisationId}: statuses={StatusCount}, categories={CategoryCount}, departments={DepartmentCount}, branches={BranchCount}, businessUnits={BusinessUnitCount}, assetTypes={AssetTypeCount}.";
            public const string ErrorFetchingLookups = "Error fetching asset lookups for organisation {OrganisationId}.";
        }

        // Asset hand over logs
        public static class AssetHandOver
        {
            public const string ErrorFetchingList = "Error fetching asset hand over list for organisation {OrganisationId}.";
            public const string ErrorHandingOver = "Error handing over asset {AssetId} for organisation {OrganisationId}.";
            public const string AssetHandedOver = "Asset {AssetId} handed over to employee {EmployeeId} by user {UserId} for organisation {OrganisationId} at {Timestamp}.";
            public const string FetchingLookups = "Fetching asset handover lookups for user {UserId} in organisation {OrganisationId}.";
            public const string LookupsFetched = "Fetched asset handover lookups for organisation {OrganisationId}: assets={AssetCount}, handOverBy={HandOverByCount}, handOverTo={HandOverToCount}.";
            public const string ErrorFetchingLookups = "Error fetching asset handover lookups for organisation {OrganisationId}.";
        }

        // Scanner logs
        public static class Scanner
        {
            public const string ErrorFetchingAsset = "Error fetching asset by scanner code for organisation {OrganisationId}.";
        }

        // Template logs
        public static class Template
        {
            public const string FetchingTemplates = "Fetching active templates for organisation {OrganisationId}.";
            public const string ErrorFetchingTemplates = "Error fetching active templates for organisation {OrganisationId}.";
        }

        // Dispute related logs
        public static class Dispute
        {
            public const string FetchingDisputeCategories = "Fetching dispute categories";
            public const string SubmittingDispute = "Submitting dispute for employee {EmployeeId}";
            public const string DisputeSubmittedSuccessfully = "Dispute submitted successfully with ID {DisputeId}";
            public const string ErrorFetchingDisputeCategories = "Error fetching dispute categories";
            public const string ErrorSubmittingDispute = "Error submitting dispute for employee {EmployeeId}";
            public const string UserAttemptedSubmitDispute = "User {UserId} attempted to submit dispute for employee {EmployeeId}";
        }

        // Image/File Upload related logs
        public static class ImageUpload
        {
            // Employee image upload logs
            public const string UploadingEmployeeImage = "Uploading employee image for employee ID: {EmployeeId}";
            public const string EmployeeImageUploadedSuccessfully = "Employee image uploaded successfully for employee ID: {EmployeeId}, File: {FileName}";
            public const string ErrorUploadingEmployeeImage = "Error uploading employee image for employee ID: {EmployeeId}";
            public const string EmployeeImageNotFound = "Employee image not found for employee ID: {EmployeeId}";
            public const string FetchingEmployeeImage = "Fetching employee image for employee ID: {EmployeeId}";
            public const string ErrorFetchingEmployeeImage = "Error fetching employee image for employee ID: {EmployeeId}";
            
            // File path operations
            public const string MovingFileToSharedPath = "Moving file from {SourcePath} to shared path {DestinationPath}";
            public const string FileMovedToSharedPath = "File moved successfully to shared path: {DestinationPath}";
            public const string FileCopiedToSharedPath = "File copied successfully to shared path: {DestinationPath}";
            public const string ErrorMovingFileToSharedPath = "Error moving file to shared path: {DestinationPath}";
            public const string ErrorCopyingFileToSharedPath = "Error copying file to shared path: {DestinationPath}";
            public const string SourceFileNotFound = "Source file not found at path: {SourcePath}";
            public const string DestinationDirectoryCreated = "Destination directory created: {DirectoryPath}";
            public const string ErrorCreatingDestinationDirectory = "Error creating destination directory: {DirectoryPath}";
            
            // Shared upload path configuration
            public const string SharedUploadPathNotConfigured = "Shared upload path (EmployeeImagePath) not configured in appsettings.json";
            public const string SharedUploadPathConfigured = "Shared upload path configured: {Path}";
            public const string SharedUploadPathDoesNotExist = "Shared upload path does not exist: {Path}";
            public const string InitializingSharedUploadPath = "Initializing shared upload path: {Path}";
            
            // File serving logs
            public const string ServingFileFromSharedPath = "Serving file from shared path: {FilePath}";
            public const string FileNotFoundInSharedPath = "File not found in shared path: {FilePath}";
            public const string ErrorServingFileFromSharedPath = "Error serving file from shared path: {FilePath}";
            public const string FileRequestReceived = "File request received: {RequestPath}";
            
            // File validation logs
            public const string ValidatingImageFile = "Validating image file: {FileName}";
            public const string ImageFileValidationFailed = "Image file validation failed: {FileName}, Reason: {Reason}";
            public const string ImageFileValidationSuccessful = "Image file validation successful: {FileName}";
            public const string InvalidImageFormat = "Invalid image format: {FileName}, Allowed formats: {AllowedFormats}";
            public const string ImageFileSizeExceeded = "Image file size exceeded: {FileName}, Size: {Size} bytes, Max: {MaxSize} bytes";
            
            // File deletion logs
            public const string DeletingEmployeeImage = "Deleting employee image for employee ID: {EmployeeId}, File: {FileName}";
            public const string EmployeeImageDeletedSuccessfully = "Employee image deleted successfully for employee ID: {EmployeeId}";
            public const string ErrorDeletingEmployeeImage = "Error deleting employee image for employee ID: {EmployeeId}";
            public const string OldImageFileDeleted = "Old image file deleted: {FilePath}";
            public const string ErrorDeletingOldImageFile = "Error deleting old image file: {FilePath}";
            
            // Image upload service specific logs
            public const string CreatedBaseUploadDirectory = "Created base upload directory: {UploadPath}";
            public const string UsingUploadBasePath = "Using upload base path: {UploadPath}";
            public const string CreatedDirectory = "Created directory: {Directory}";
            public const string ImageSavedSuccessfully = "Image saved successfully: {FilePath}";
            public const string ErrorSavingEmployeeImage = "Error saving employee image: {FileName}";
            public const string UploadPathNotConfiguredUsingFallback = "UploadSettings:RootPath is not configured. Using wwwroot as fallback. This should be configured to point to a shared folder outside both projects (e.g., C:\\SharedUploads\\Indotalent).";
            public const string UsingSharedUploadRootPath = "Using shared upload root path from configuration: {Path}";
        }

        // Azure Blob related logs (Punch image upload + cleanup)
        public static class AzureBlob
        {
            // Configuration / initialization
            public const string NotConfiguredUploadDisabled = "AzureBlob is not configured. Punch image upload will be disabled.";
            public const string NotConfiguredCleanupDisabled = "AzureBlob is not configured. Blob cleanup job will be disabled.";
            public const string InvalidConnectionStringCleanupDisabled = "Invalid AzureBlob:ConnectionString format. Blob cleanup job will be disabled.";
            public const string InvalidConnectionStringUploadDisabled = "Invalid AzureBlob:ConnectionString format. Punch image upload will be disabled.";
            public const string InitFailedCleanupDisabled = "Failed to initialize AzureBlob client. Blob cleanup job will be disabled.";
            public const string InitFailedUploadDisabled = "Failed to initialize AzureBlob client. Punch image upload will be disabled.";

            // Upload
            public const string UploadingPunchImage = "Uploading punch image to blob '{BlobName}' for employee {EmpId}.";
            public const string PunchImageUploadedSuccessfully = "Punch image uploaded successfully for employee {EmpId}. Url: {BlobUrl}";
            public const string ErrorUploadingPunchImage = "Error uploading punch image for employee {EmpId}.";

            // Cleanup job
            public const string CleanupServiceDisabled = "BlobCleanupService is disabled due to missing/invalid AzureBlob configuration.";
            public const string CleanupServiceStarted = "BlobCleanupService started. RetentionDays={RetentionDays}";
            public const string CleanupRunFailed = "Blob cleanup run failed.";
            public const string ContainerDoesNotExistSkippingCleanup = "Blob container does not exist. Skipping cleanup.";
            public const string CleanupCompleted = "Blob cleanup completed. Checked={CheckedCount}, Deleted={DeletedCount}, CutoffUtc={CutoffUtc}";
        }

        // Employee resolution logs (shared across services)
        public static class EmployeeResolution
        {
            public const string NoEmployeeFoundForUserId = "No employee found for UserId: {UserId}";
            public const string ErrorResolvingEmployeeIdFromUserId = "Error resolving EmployeeId from UserId: {UserId}";
        }

        // SMS Service related logs
        public static class Sms
        {
            public const string TwilioSettingsNotConfigured = "Twilio settings not configured";
            public const string SendingSmsViaTwilio = "Sending SMS via Twilio to {MobileNumber} from {FromNumber}";
            public const string TwilioSmsSentSuccessfully = "Twilio SMS sent successfully to {MobileNumber}. Response: {Response}";
            public const string TwilioApiReturnedError = "Twilio API returned error. Status: {Status}, Response: {Response}";
            public const string ExceptionWhileSendingSmsViaTwilio = "Exception while sending SMS via Twilio to {MobileNumber}";
            public const string Msg91SettingsNotConfigured = "MSG91 settings not configured";
            public const string SendingSmsViaMsg91 = "Sending SMS via MSG91 to {MobileNumber}";
            public const string Msg91SmsSentSuccessfully = "MSG91 SMS sent successfully to {MobileNumber}. Response: {Response}";
            public const string Msg91ApiReturnedSuccessButFailure = "MSG91 API returned success status but response indicates failure. Response: {Response}";
            public const string Msg91ApiReturnedError = "MSG91 API returned error. Status: {Status}, Response: {Response}";
            public const string ExceptionWhileSendingSmsViaMsg91 = "Exception while sending SMS via MSG91 to {MobileNumber}";
            public const string SmsResponseBody = "SMS Response: {Body}";
        }

        // Tenant Context related logs
        public static class TenantContext
        {
            public const string AttemptedToGetOrganisationIdNotAuthenticated = "Attempted to get OrganisationId but user is not authenticated or claim is missing. User: {Username}";
            public const string TenantAccessValidationFailedNotAuthenticated = "Tenant access validation failed: User is not authenticated. Requested OrgId: {RequestedOrgId}";
            public const string TenantAccessViolationDetected = "Tenant access violation detected! User {Username} (OrgId: {UserOrgId}) attempted to access OrgId: {RequestedOrgId}";
            public const string TenantAccessValidated = "Tenant access validated for User {Username}, OrgId: {OrgId}";
            public const string AttemptedToGetUserIdNotAuthenticated = "Attempted to get UserId but user is not authenticated or claim is missing. User: {Username}";
            public const string UserAccessValidationFailedNotAuthenticated = "User access validation failed: User is not authenticated. Requested UserId: {RequestedUserId}";
            public const string UserHasElevatedAccess = "User {Username} has elevated access (HR/Admin), allowing access to UserId: {RequestedUserId}";
            public const string UserAccessViolationDetected = "User access violation detected! User {Username} (UserId: {CurrentUserId}) attempted to access UserId: {RequestedUserId}";
            public const string UserAccessValidated = "User access validated for User {Username}, accessing own data UserId: {UserId}";
        }

        // Data/Database related logs
        public static class Database
        {
            public const string ConnectionStringMissing = "Connection string 'ConnectionString' is missing or empty in configuration!";
            public const string DapperContextInitialized = "DapperContext initialized with connection string.";
            public const string CreatingNewSqlConnection = "Creating new SqlConnection.";
        }

        // Authentication/Authorization related logs (additional)
        public static class AuthAdditional
        {
            public const string InvalidMobileNumberFormat = "LoginMobile: Invalid mobile number format: {MobileNumber}";
            public const string MobileNumberNotRegistered = "Mobile number not registered: {MobileNumber}";
            public const string EmployeeInactiveForMobile = "Employee is inactive for mobile: {MobileNumber}, EmployeeId: {EmployeeId}";
        }

        // Holiday Service related logs (additional)
        public static class HolidayAdditional
        {
            public const string UserNotFoundForUserId = "User not found for user_id: {UserId}";
            public const string OrganizationIdRequired = "Organization ID is required for getting holidays. Provide either organization_id or user_id parameter.";
            public const string InvalidHolidayIdLog = "Invalid holiday ID";
        }

        // Middleware related logs
        public static class Middleware
        {
            public const string TenantAccessViolationDetected = "Tenant access violation detected";
        }

        // Personal Details Controller related logs
        public static class PersonalDetails
        {
            // Employee lookup by user ID
            public const string EmployeeNotFoundForUserId = "Employee not found for user ID: {UserId}";
            public const string EmployeeDoesNotHaveEmployeeNumber = "Employee {EmployeeId} does not have an employee number";
            public const string FoundEmployeeNumberForUserId = "Found employee number {EmployeeNumber} for user ID {UserId}";
            public const string EmployeeNotFoundWithEmployeeNumber = "Employee not found with employee number: {EmployeeNumber}";
            public const string UsingEmployeeIdFromEmployeeNumber = "Using employee ID {EmployeeId} (empId) from employee number {EmployeeNumber}";
            
            // Request validation
            public const string InvalidRequestOrUserId = "Invalid request or UserId";
            public const string InvalidUserId = "Invalid UserId";
            public const string BothPhoneAndPictureEmpty = "UpdatePersonalDetailsPhoneAndPicture: Both Phone and Picture are empty";
            
            // Image upload
            public const string ImageValidationFailed = "Image validation failed: {Error}";
            public const string PictureSavedSuccessfully = "Picture saved successfully for employee {EmployeeId} (UserId: {UserId}): {Path}";
            public const string ErrorSavingEmployeePicture = "Error saving employee picture for UserId {UserId}";
        }
    }
}

