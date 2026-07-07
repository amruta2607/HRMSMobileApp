namespace MobileWebApi.Constants
{
    /// <summary>
    /// Authentication and Login related messages
    /// </summary>
    public static class AuthMessages
    {
        public const string InvalidCredentials = "Invalid username or password.";
        public const string TokenGenerated = "Token generated successfully.";
        public const string UsernameMissingInToken = "Username missing in token.";
        public const string OrganisationIdMissingInToken = "OrganisationId missing in token.";
        public const string LogoutSuccessful = "Logout successful.";
        public const string InvalidAuthenticationToken = "Invalid authentication token.";
        public const string LogoutError = "An error occurred during logout. Please try again.";
        
        // Forgot Password messages
        public const string OtpSentSuccessfully = "OTP has been sent to your registered contact.";
        public const string InvalidOtp = "Invalid or expired OTP.";
        public const string PasswordResetSuccessful = "Password/PIN reset successful.";
        public const string PasswordResetFailed = "Failed to reset password/PIN.";
        public const string EmailOrMobileRequired = "Email or mobile number is required.";
        public const string OtpRequired = "OTP is required.";
        public const string NewPasswordRequired = "New password/PIN is required.";
        public const string UserNotFoundOrInactive = "User not found or inactive.";
        
        // Change Password messages
        public const string CurrentPasswordRequired = "Current password/PIN is required.";
        public const string CurrentPasswordIncorrect = "Current password/PIN is incorrect.";
        public const string PasswordChangeSuccessful = "Password/PIN changed successfully.";
        public const string PasswordChangeFailed = "Failed to change password/PIN.";
        public const string NewPasswordSameAsCurrent = "New password/PIN must be different from current password/PIN.";
        
        // Refresh Token messages
        public const string RefreshTokenRequired = "Refresh token is required.";
        public const string InvalidRefreshToken = "Invalid refresh token.";
        public const string RefreshTokenExpired = "Refresh token has expired. Please login again.";
        public const string TokenRefreshed = "Token refreshed successfully.";
        public const string RefreshTokenRevoked = "Refresh token has been revoked successfully.";
        public const string RefreshTokenRevokedLoginRequired = "Refresh token has been revoked. Please login again.";
        public const string RefreshTokenAlreadyUsed = "Refresh token has already been used. Please login again.";
        public const string InvalidAccessToken = "Invalid access token.";
        public const string AccessTokenRequired = "Access token is required.";
    }

    /// <summary>
    /// User related messages
    /// </summary>
    public static class UserMessages
    {
        // Success messages
        public const string UserRetrievedSuccessfully = "User retrieved successfully.";
        public const string UsersRetrievedSuccessfully = "Users retrieved successfully.";
        public const string UserCreatedSuccessfully = "User created successfully.";
        public const string UserUpdatedSuccessfully = "User updated successfully.";
        public const string UserDeletedSuccessfully = "User deleted successfully.";
        public const string UserDeactivatedSuccessfully = "User deactivated successfully.";

        // Error messages
        public const string UserNotFound = "User not found.";
        public const string InvalidUserId = "Invalid user id.";
        public const string UserIdRequired = "User ID is required.";
        public const string UsernameRequired = "Username is required.";
        public const string PasswordRequired = "Password is required.";
        public const string InvalidUserIdForUpdate = "Invalid User Id.";
        public const string UsernameAlreadyExists = "Username already exists.";
        public const string MobileNumberAlreadyExists = "Mobile number already exists.";
        public const string FailedToCreateUser = "Failed to create user.";
        public const string FailedToUpdateUser = "Failed to update user. No changes applied.";
        public const string FailedToDeleteUser = "Failed to delete user.";
        public const string FailedToDeactivateUser = "Failed to deactivate user.";
        public const string UserAlreadyInactive = "User is already inactive.";

        // Error templates (use with string.Format or interpolation)
        public const string ErrorRetrievingUser = "Error retrieving user: {0}";
        public const string ErrorCreatingUser = "Error creating user: {0}";
        public const string ErrorUpdatingUser = "Error updating user: {0}";
        public const string ErrorDeletingUser = "Error deleting user: {0}";
        public const string ErrorDeactivatingUser = "Error deactivating user: {0}";
    }

    /// <summary>
    /// Employee / Personal Details related messages
    /// </summary>
    public static class EmployeeMessages
    {
        // Success messages
        public const string EmployeeRetrievedSuccessfully = "Employee retrieved successfully.";
        public const string EmployeesRetrievedSuccessfully = "Employees retrieved successfully.";
        public const string EmployeeAddedSuccessfully = "Employee added successfully.";
        public const string EmployeeUpdatedSuccessfully = "Employee updated successfully.";
        public const string EmployeeDeletedSuccessfully = "Employee deleted successfully.";
        public const string EmployeeDeactivatedSuccessfully = "Employee deactivated successfully.";

        // Error messages
        public const string EmployeeNotFound = "Employee not found.";
        public const string EmployeeProfileNotFound = "Employee profile not found.";
        public const string InvalidEmployeeId = "Invalid employee ID.";
        public const string InvalidUserId = "Invalid user ID.";
        public const string InvalidBranchId = "Invalid branch ID.";
        public const string RequestBodyNull = "Request body is null.";
        public const string RequestCannotBeNull = "Request cannot be null.";
        public const string NameAndEmailRequired = "Name and email are required.";
        public const string InvalidRequestOrEmployeeId = "Invalid request or employee ID.";
        public const string FailedToUpdateEmployee = "Failed to update employee.";
        public const string FailedToDeleteEmployee = "Failed to delete employee.";
        public const string FailedToDeactivateEmployee = "Failed to deactivate employee.";

        // Error templates
        public const string ErrorRetrievingEmployee = "Error retrieving employee: {0}";
        public const string ErrorAddingEmployee = "Error adding employee: {0}";
        public const string ErrorUpdatingEmployee = "Error updating employee: {0}";
        public const string ErrorDeletingEmployee = "Error deleting employee: {0}";
        public const string ErrorDeactivatingEmployee = "Error deactivating employee: {0}";
        public const string EmployeeNotFoundForUserId = "No employee found for the specified UserId.";
        public const string EmployeeNotFoundForGivenUser = "Employee not found for the given user.";
        public const string PhoneOrPictureRequiredForUpdate = "At least one field (Phone or Picture) must be provided for update.";
    }

    /// <summary>
    /// Attendance related messages
    /// </summary>
    public static class AttendanceMessages
    {
        // Success messages
        public const string PunchInSuccessful = "Punch In Successful";
        public const string PunchOutSuccessful = "Punch Out Successful";
        public const string ManualPunchOutReason = "Manual punch out";
        public const string AttendanceReportFetchedSuccessfully = "Attendance report fetched successfully.";
        public const string RealTimeAttendanceFetchedSuccessfully = "Real-time attendance status fetched successfully.";
        public const string CurrentlyPunchedInFetchedSuccessfully = "Currently punched in employees fetched successfully.";
        public const string EmployeeAttendanceFetchedSuccessfully = "Employee attendance fetched successfully.";
        public const string TodayPunchLogsFetchedSuccessfully = "Today's punch logs fetched successfully.";

        // Error messages
        public const string PunchInAlreadyDone = "Punch In already done for today.";
        public const string PunchInFailed = "Punch In Failed";
        public const string CannotPunchOutWithoutPunchIn = "Cannot Punch Out — Punch In not done.";
        public const string PunchOutAlreadyDone = "Punch Out already done.";
        public const string CalendarDateRequiredForDaily = "CalendarDate is required for daily attendance report.";
        public const string DateRangeRequiredForMonthly = "DateFrom and DateTo are required for monthly attendance report.";
        public const string EmployeeIdRequired = "Employee ID is required.";
        public const string DateRangeRequired = "Date range (dateFrom and dateTo) is required.";

        // Calendar attendance messages
        public const string CalendarAttendanceFetchedSuccessfully = "Calendar attendance fetched successfully.";
        public const string InvalidMonth = "Invalid month. Month must be between 1 and 12.";
        public const string InvalidYear = "Invalid year. Year must be between 2000 and 2100.";
        public const string EmployeeNotFound = "Employee not found.";

        // Attendance summary messages
        public const string AttendanceSummaryFetchedSuccessfully = "Attendance summary fetched successfully.";
        public const string InvalidDateRange = "Invalid date range. From date must be before or equal to To date.";

        // Delete attendance messages
        public const string AttendanceDeletedSuccessfully = "Attendance record deleted successfully.";
        public const string AttendanceNotFound = "Attendance record not found.";
        public const string FailedToDeleteAttendance = "Failed to delete attendance record.";

        // Attendance status messages
        public const string AttendanceStatusRetrievedSuccessfully = "Attendance status retrieved successfully.";
        public const string DateRequired = "Date parameter is required.";

        // Attendance overview messages
        public const string AttendanceOverviewFetchedSuccessfully = "Attendance overview fetched successfully.";
        public const string OrganisationIdRequired = "Organisation ID is required.";
        public const string WorkingHoursNotFound = "WorkingHours not found for the specified OrganisationId.";
        public const string DateRangeExceedsMaximum = "Date range cannot exceed 7 days. Please select a date range within 7 days.";

        // Error templates
        public const string ErrorFetchingAttendanceReport = "Error fetching attendance report: {0}";
        public const string ErrorFetchingRealTimeAttendance = "Error fetching real-time attendance: {0}";
        public const string ErrorFetchingEmployeeAttendance = "Error fetching employee attendance: {0}";
        public const string ErrorFetchingCalendarAttendance = "Error fetching calendar attendance: {0}";
        public const string ErrorFetchingAttendanceSummary = "Error fetching attendance summary: {0}";
        public const string ErrorFetchingAttendanceOverview = "Error fetching attendance overview: {0}";
    }

    /// <summary>
    /// Leave related messages
    /// </summary>
    public static class LeaveMessages
    {
        // Success messages
        public const string LeaveRequestSubmittedSuccessfully = "Leave request submitted successfully.";
        public const string LeaveRequestsFetchedSuccessfully = "Leave requests fetched successfully.";
        public const string LeaveRequestFetchedSuccessfully = "Leave request fetched successfully.";
        public const string LeaveRequestApprovedSuccessfully = "Leave request approved successfully.";
        public const string LeaveRequestRejectedSuccessfully = "Leave request rejected successfully.";
        public const string LeaveRequestCancelledSuccessfully = "Leave request cancelled successfully.";
        public const string LeaveBalanceFetchedSuccessfully = "Leave balance fetched successfully.";
        public const string LeaveHistoryFetchedSuccessfully = "Leave history fetched successfully.";

        // Error messages
        public const string UserIdRequired = "User ID is required.";
        public const string LeaveTypeRequired = "Leave type is required.";
        public const string OrganizationNotFound = "Organization not found.";
        public const string EmployeeNotFoundForUser = "Employee not found for the given user.";
        public const string LeaveTypeNotFound = "Leave type not found.";
        public const string InsufficientLeaveBalance = "Insufficient leave balance. Available: {0}, Requested: {1}";
        public const string LeaveRequestNotFound = "Leave request not found.";
        public const string LeaveRequestAlreadyApproved = "Leave request is already approved.";
        public const string FailedToCreateLeaveRequest = "Failed to create leave request.";
        public const string FailedToApproveLeaveRequest = "Failed to approve leave request.";
        public const string FailedToRejectLeaveRequest = "Failed to reject leave request.";
        public const string FailedToCancelLeaveRequest = "Failed to cancel leave request.";
        public const string LeaveServiceNotAvailable = "Leave service not available.";
        public const string InvalidRequest = "Invalid leave request data.";

        public const string LeaveAlreadyAppliedForSelectedDate = "Leave already applied for selected date.";

		// Error templates
		public const string LeaveRequestNotFoundWithId = "Leave request with ID {0} not found.";
        public const string ErrorCreatingLeaveRequest = "Error creating leave request: {0}";
        public const string ErrorFetchingLeaveRequests = "Error fetching leave requests: {0}";
        public const string ErrorFetchingLeaveRequest = "Error fetching leave request: {0}";
        public const string ErrorApprovingLeaveRequest = "Error approving leave request: {0}";
        public const string ErrorRejectingLeaveRequest = "Error rejecting leave request: {0}";
        public const string ErrorCancellingLeaveRequest = "Error cancelling leave request: {0}";
        public const string ErrorFetchingLeaveBalance = "Error fetching leave balance: {0}";
        public const string LeaveRequestIdRequired = "Leave request ID is required and must be greater than 0";
    }

    /// <summary>
    /// Alert/Notification related messages
    /// </summary>
    public static class AlertMessages
    {
        // Success messages
        public const string AlertRetrievedSuccessfully = "Alert retrieved successfully.";
        public const string AlertsRetrievedSuccessfully = "Alerts retrieved successfully.";
        public const string AlertCreatedSuccessfully = "Alert created successfully.";
        public const string AlertUpdatedSuccessfully = "Alert updated successfully.";
        public const string AlertMarkedAsRead = "Alert marked as read.";
        public const string AllAlertsMarkedAsRead = "All alerts marked as read.";
        public const string NoUnreadAlertsFound = "No unread alerts found.";
        public const string AlertDeactivatedSuccessfully = "Alert deactivated successfully.";
        public const string AlertDeletedSuccessfully = "Alert deleted successfully.";
        public const string AlertApprovedSuccessfully = "Alert approved successfully.";
        public const string AlertRejectedSuccessfully = "Alert rejected successfully.";

        // Error messages
        public const string AlertNotFound = "Alert not found.";
        public const string AlertIdRequired = "Alert ID is required and must be greater than 0.";
        public const string RejectionReasonRequired = "Rejection reason is required.";
        public const string FailedToCreateAlert = "Failed to create alert.";
        public const string FailedToUpdateAlert = "Failed to update alert. Alert may not exist.";
        public const string FailedToMarkAlertAsRead = "Failed to mark alert as read. Alert may not exist.";
        public const string FailedToDeactivateAlert = "Failed to deactivate alert. Alert may not exist.";
        public const string FailedToDeleteAlert = "Failed to delete alert. Alert may not exist.";
        public const string FailedToApproveAlert = "Failed to approve alert. Alert may not exist.";
        public const string FailedToRejectAlert = "Failed to reject alert. Alert may not exist.";

        // Error templates
        public const string ErrorRetrievingAlert = "Error retrieving alert: {0}";
        public const string ErrorRetrievingAlerts = "Error retrieving alerts: {0}";
        public const string ErrorCreatingAlert = "Error creating alert: {0}";
        public const string ErrorUpdatingAlert = "Error updating alert: {0}";
        public const string ErrorMarkingAlertAsRead = "Error marking alert as read: {0}";
        public const string ErrorMarkingAllAlertsAsRead = "Error marking all alerts as read: {0}";
        public const string ErrorDeactivatingAlert = "Error deactivating alert: {0}";
        public const string ErrorDeletingAlert = "Error deleting alert: {0}";
        public const string ErrorApprovingAlert = "Error approving alert: {0}";
        public const string ErrorRejectingAlert = "Error rejecting alert: {0}";
        public const string ErrorSendingApprovalNotification = "Error sending approval notification: {0}";
        public const string ErrorSendingRejectionNotification = "Error sending rejection notification: {0}";
        public const string ApprovalNotificationSentSuccessfully = "Approval notification sent successfully.";
        public const string RejectionNotificationSentSuccessfully = "Rejection notification sent successfully.";
        public const string RequestApprovedSuccessfully = "Request approved successfully and notification sent to requester.";
        public const string RequestRejectedSuccessfully = "Request rejected successfully and notification sent to requester.";
    }

    /// <summary>
    /// Pay Slip related messages
    /// </summary>
    public static class PaySlipMessages
    {
        // Success messages
        public const string PaySlipsFetchedSuccessfully = "Pay slips fetched successfully.";
        public const string PaySlipFetchedSuccessfully = "Pay slip fetched successfully.";
        public const string PaySlipDownloadedSuccessfully = "Pay slip downloaded successfully.";
        public const string ProvidentFundFetchedSuccessfully = "Provident Fund fetched successfully.";
        public const string MonthlySummaryFetchedSuccessfully = "Monthly summary fetched successfully.";
        public const string LastMonthPayrollFetchedSuccessfully = "Last month payroll fetched successfully.";
        public const string YearsFetchedSuccessfully = "Years fetched successfully.";
        public const string MonthsFetchedSuccessfully = "Months fetched successfully.";

        // Error messages
        public const string UserIdRequired = "User ID is required.";
        public const string PaySlipIdRequired = "Pay slip ID is required.";
        public const string EmployeeNotFoundForUser = "Employee not found for the given user.";
        public const string EmployeeNotFound = "Employee not found.";
        public const string PaySlipNotFound = "Pay slip not found.";
        public const string PaySlipFileNotFound = "Pay slip file not found.";
        public const string UnauthorizedAccess = "You are not authorized to access this pay slip.";
        public const string NoPayrollDataFound = "No payroll data found.";
        public const string NoPayrollDataFoundForLastMonth = "No payroll data found for last month.";
        public const string PdfGenerationFailed = "PDF generation failed.";

        // Error templates
        public const string ErrorFetchingPaySlips = "Error fetching pay slips: {0}";
        public const string ErrorFetchingPaySlip = "Error fetching pay slip: {0}";
        public const string ErrorDownloadingPaySlip = "Error downloading pay slip: {0}";
    }

    /// <summary>
    /// Location related messages
    /// </summary>
    public static class LocationMessages
    {
        public const string NoLocationsFound = "No locations found.";
        public const string LocationsRetrievedSuccessfully = "Locations retrieved successfully.";
        
        // Error templates
        public const string ErrorRetrievingLocations = "Error retrieving locations: {0}";
    }

    /// <summary>
    /// Organisation related messages
    /// </summary>
    public static class OrganisationMessages
    {
        public const string OrganisationIdRequired = "Organisation ID is required.";
        public const string OrganisationNotFound = "Organisation not found.";
        public const string InvalidOrganisationId = "Invalid organisation ID.";
    }

    /// <summary>
    /// Tenant access related messages
    /// </summary>
    public static class TenantAccessMessages
    {
        public const string TenantAccessDenied = "Access denied: You can only access data from your own organisation.";
        public const string UserAccessDenied = "Access denied: You can only access your own data. HR or Admin users can access all users' data.";
        public const string UserAccessDeniedSimple = "Access denied: You can only access your own data.";
        public const string UserNotAuthenticated = "User is not authenticated.";
    }

    /// <summary>
    /// OTP related messages
    /// </summary>
    public static class OtpMessages
    {
        public const string OtpGenerated = "OTP generated successfully.";
        public const string OtpValidated = "OTP validated successfully.";
        public const string OtpRemoved = "OTP removed successfully.";
        
        // Mobile OTP messages
        public const string RequestBodyRequired = "Request body is required";
        public const string MobileNumberRequired = "Mobile number is required";
        public const string InvalidMobileNumberFormat = "Invalid mobile number format. Must be 10 digits.";
        public const string MobileNumberNotRegistered = "Mobile number not registered";
        public const string FailedToGenerateOtp = "Failed to generate OTP. Please try again.";
        public const string FailedToSendOtp = "Failed to send OTP. Please try again.";
        public const string OtpSentSuccessfully = "OTP sent successfully";
        public const string OtpSentSuccessfullyToMobile = "OTP sent successfully to your mobile number";
        public const string InvalidOtpFormat = "Invalid OTP format. Must be 6 digits.";
        public const string InvalidOrExpiredOtp = "Invalid or expired OTP";
        public const string LoginSuccessful = "Login successful";
        public const string MaximumOtpLimitReached = "Maximum OTP limit reached. Please try again after some time.";
        public const string PleaseWaitBeforeRequestingOtp = "Please wait before requesting another OTP.";
        public const string ErrorProcessingRequest = "An error occurred while processing your request";
        public const string EmployeeAccountInactive = "Employee account is inactive";
    }

    /// <summary>
    /// Email related messages
    /// </summary>
    public static class EmailMessages
    {
        // Success messages
        public const string EmailSentSuccessfully = "Email sent successfully.";
        public const string OtpEmailSentSuccessfully = "OTP has been sent to your registered email address.";
        public const string PasswordResetEmailSent = "Password reset confirmation email sent.";

        // Error messages
        public const string EmailNotFoundForUser = "No email address found for this user. Please contact support.";
        public const string FailedToSendEmail = "Failed to send email. Please try again later.";
        public const string FailedToSendOtpEmail = "Failed to send OTP email. Please try again later.";
        public const string InvalidEmailAddress = "Invalid email address.";
        public const string SmtpConfigurationError = "Email service configuration error. Please contact support.";

        // Email subjects
        public const string PasswordResetOtpSubject = "Password Reset OTP - Mobile App";
        public const string PasswordResetSuccessfulSubject = "Password Reset Successful - Mobile App";

        // Error templates
        public const string ErrorSendingEmail = "Error sending email: {0}";
    }

    /// <summary>
    /// Tenant / organisation configuration messages
    /// </summary>
    public static class TenantMessages
    {
        public const string TenantConfigurationNotFound = "Tenant configuration was not found for your organization.";
        public const string CompanyLogoRetrievedSuccessfully = "Company logo retrieved successfully.";
    }

    /// <summary>
    /// General messages used across the application
    /// </summary>
    public static class GeneralMessages
    {
        public const string OperationSuccessful = "Operation completed successfully.";
        public const string OperationFailed = "Operation failed.";
        public const string UnexpectedError = "An unexpected error occurred.";
        public const string InvalidRequest = "Invalid request.";
        public const string RequestBodyCannotBeNull = "Request body cannot be null.";
        public const string RequestCannotBeNull = "Request cannot be null.";
        public const string SomethingWentWrongContactAdmin = "Something went wrong. Please contact the administration team.";
        public const string SomethingWentWrongWithCode = "Something went wrong. Please contact the administration team. (Error Code: {0})";
    }

    /// <summary>
    /// Transaction related messages
    /// </summary>
    public static class TransactionMessages
    {
        public const string TransactionCreated = "Transaction created successfully.";
        public const string TransactionFailed = "Failed to create transaction.";
        public const string TransactionTableNotExist = "Could not create transaction record. Table may not exist.";
    }

    /// <summary>
    /// Approval Workflow related messages
    /// </summary>
    public static class ApprovalWorkflowMessages
    {
        // Success messages
        public const string WorkflowInitiatedSuccessfully = "Leave request approval workflow initiated successfully.";
        public const string ApprovalSubmittedSuccessfully = "Approval submitted successfully.";
        public const string ApprovalApprovedSuccessfully = "Request approved successfully.";
        public const string ApprovalRejectedSuccessfully = "Request rejected successfully.";
        public const string NotificationSentSuccessfully = "Notification sent successfully.";

        // Error messages
        public const string EventTypeNotConfigured = "Event type not configured.";
        public const string EventTypeNotActive = "Event type is not active.";
        public const string FailedToCreateEvent = "Failed to create event.";
        public const string NoApprovalLevelsConfigured = "No approval levels configured for this event type.";
        public const string ApprovalStageNotFound = "Approval stage not found.";
        public const string ApprovalStageNotActive = "Approval stage is not active.";
        public const string NoApproversConfigured = "No approvers configured for this approval stage.";
        public const string FailedToCreateApproval = "Failed to create approval record.";
        public const string FailedToSendNotification = "Failed to send notification.";
        public const string ApprovalNotFound = "Approval record not found.";
        public const string InvalidApprovalAction = "Invalid approval action.";
        public const string UnauthorizedApprover = "You are not authorized to approve this request.";
        public const string ApprovalAlreadyProcessed = "This approval has already been processed.";

        // Notification messages
        public const string LeaveRequestPendingApproval = "New Leave Request Pending Approval";
        public const string LeaveRequestRequiresApproval = "A new leave request requires your approval.";

        // Error templates
        public const string ErrorInitiatingWorkflow = "Error initiating approval workflow: {0}";
        public const string ErrorProcessingApproval = "Error processing approval: {0}";
        public const string ErrorSendingNotification = "Error sending notification: {0}";
    }

    /// <summary>
    /// Holiday related messages
    /// </summary>
    public static class HolidayMessages
    {
        // Success messages
        public const string HolidayCreatedSuccessfully = "Holiday created successfully.";
        public const string HolidaysFetchedSuccessfully = "Holidays retrieved successfully.";
        public const string HolidayUpdatedSuccessfully = "Holiday updated successfully.";
        public const string HolidayDeletedSuccessfully = "Holiday deleted successfully.";
        public const string BulkHolidaysCreatedSuccessfully = "Successfully created {0} holiday(s).";

        // Error messages
        public const string HolidayNotFound = "Holiday not found.";
        public const string InvalidHolidayId = "Invalid holiday ID.";
        public const string HolidayNameRequired = "Holiday name is required.";
        public const string HolidayDateRequired = "Holiday date is required.";
        public const string HolidaysListRequired = "Holidays list is required.";
        public const string FailedToCreateHoliday = "Failed to create holiday.";
        public const string FailedToUpdateHoliday = "Failed to update holiday.";
        public const string FailedToDeleteHoliday = "Failed to delete holiday.";

        // Error templates
        public const string ErrorCreatingHoliday = "Error creating holiday: {0}";
        public const string ErrorFetchingHolidays = "Error fetching holidays: {0}";
        public const string ErrorUpdatingHoliday = "Error updating holiday: {0}";
        public const string ErrorDeletingHoliday = "Error deleting holiday: {0}";
        public const string ErrorCreatingBulkHolidays = "Error creating bulk holidays: {0}";
        public const string OrganizationIdRequiredWithUserId = "Organization ID is required. Please provide either organization_id or user_id parameter.";
    }

    /// <summary>
    /// Dispute related messages
    /// </summary>
    public static class DisputeMessages
    {
        public const string DisputeCategoriesFetchedSuccessfully = "Dispute categories fetched successfully.";
        public const string EmployeeNotFoundForGivenUser = "Employee not found for the given user.";
        public const string InvalidDisputeDate = "Invalid dispute date. Please provide a valid date (e.g., 2026-03-03).";
        public const string DisputeDateCannotBeFuture = "Dispute date cannot be a future date.";
        public const string DescriptionRequired = "Description is required.";
        public const string EmployeeNotFound = "Employee not found.";
        public const string OnlyOneDisputePerDay = "Only one dispute can be submitted per day. A dispute for this date already exists.";
        public const string DisputeSubmittedSuccessfully = "Dispute submitted successfully.";
        public const string FailedToSubmitDispute = "Failed to submit dispute.";
    }

    /// <summary>
    /// Event and Approval workflow state constants
    /// </summary>
    public static class EventStateConstants
    {
        // Event States
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        
        // Event Statuses
        public const string Active = "Active";
        public const string ApprovedByManager = "Approved by Manager";
        public const string RejectedByManager = "Rejected by Manager";
    }

    /// <summary>
    /// Notification status constants
    /// </summary>
    public static class NotificationStatusConstants
    {
        public const string Unread = "Unread";
        public const string Read = "Read";
    }

    /// <summary>
    /// Event name constants
    /// </summary>
    public static class EventNameConstants
    {
        public const string LeaveRequest = "LeaveRequest";
        public const string LeaveRequestLower = "leaverequest";
        public const string PayrollSubmission = "PayrollSubmission";
        public const string ReimbursementRequest = "ReimbursementRequest";
        public const string ResignationRequest = "ResignationRequest";
        public const string OvertimeRequest = "OvertimeRequest";
        public const string CancelLeave = "CancelLeave";
        public const string Request = "Request";
    }

    /// <summary>
    /// Alert/Approval workflow error messages
    /// </summary>
    public static class AlertWorkflowMessages
    {
        public const string EventIdNotFound = "Event ID not found in alert. Cannot approve request.";
        public const string EventIdNotFoundForRejection = "Event ID not found in alert. Cannot reject request.";
        public const string EventNotFound = "Event with ID {0} not found.";
        public const string LeaveRequestIdNotFound = "Leave request ID not found in event data.";
        public const string ApprovalRepositoryNotAvailable = "Approval repository not available. Cannot retrieve event data.";
        public const string EventTypeNotSupported = "Event type {0} not supported for approval in API. Only updating alert status.";
        public const string EventTypeNotSupportedForRejection = "Event type {0} not supported for rejection in API. Only updating alert status.";
        public const string FailedToUpdateEventStatus = "Failed to update Event status for event {0}";
        public const string FailedToUpdateAlertStatus = "Failed to update alert status after approving request";
        public const string FailedToUpdateAlertStatusAfterRejection = "Failed to update alert status after rejecting request";
        public const string ErrorApprovingRequest = "Error approving request: {0}";
        public const string ErrorRejectingRequest = "Error rejecting request: {0}";
        public const string ErrorParsingEventData = "Error parsing event data: {0}";
        public const string NotificationCreatedForApproval = "Created notification {0} for requester {1} about approved event {2}";
        public const string NotificationCreatedForRejection = "Created notification {0} for requester {1} about rejected event {2}";
        
        // Approval/Rejection comment templates
        public const string ApprovedByUser = "Approved by User {0}";
        public const string RejectedByUser = "Rejected by User {0}";
        public const string RejectedWithReason = "Rejected: {0}";
        
        // Notification title templates
        public const string EventApprovedTitle = "{0} Approved";
        public const string EventRejectedTitle = "{0} Rejected";
        
        // Notification message templates
        public const string EventApprovedMessage = "Your {0} has been approved.";
        public const string EventRejectedMessage = "Your {0} has been rejected. Reason: {1}";
        public const string EventRejectedMessageNoReason = "Your {0} has been rejected.";
    }

    /// <summary>
    /// Location tracking related messages
    /// </summary>
    public static class LocationTrackingMessages
    {
        public const string LocationRecordedSuccessfully = "Location recorded successfully.";
        public const string UserIdRequired = "User ID is required.";
        public const string LatitudeRequired = "Latitude is required.";
        public const string LongitudeRequired = "Longitude is required.";
        public const string InvalidLatitude = "Latitude must be between -90 and 90.";
        public const string InvalidLongitude = "Longitude must be between -180 and 180.";
        public const string TrackingDateTimeRequired = "Tracking date and time is required.";
        public const string EmployeeNotFound = "Employee not found.";
        public const string TenantNotFound = "Tenant not found.";
        public const string EmployeeDoesNotBelongToTenant = "Employee does not belong to the specified tenant.";
        public const string EmployeeNotPunchedIn = "Employee is not currently punched in.";
        public const string LocationTrackingDisabled = "Location tracking is disabled.";
        public const string FailedToRecordLocation = "Failed to record location.";
        public const string LocationsRequired = "At least one location record is required.";
        public const string BatchProcessedSuccessfully = "Location batch processed successfully.";
        public const string BatchPartiallyProcessed = "Location batch processed with some invalid records skipped.";
        public const string BatchAllRecordsInvalid = "All location records in the batch are invalid.";
        public const string FailedToRecordLocationBatch = "Failed to record location batch.";
    }

    /// <summary>
    /// Location tracking issue API messages
    /// </summary>
    public static class LocationTrackingIssueMessages
    {
        public const string IssueLoggedSuccessfully = "Location tracking issue logged successfully.";
        public const string InvalidIssueType = "Invalid Issue Type.";
        public const string IssueTypeRequired = "Issue type is required.";
        public const string IssueDescriptionRequired = "Issue description is required.";
        public const string TimestampRequired = "Timestamp is required.";
        public const string EmployeeNotFound = "Employee not found.";
        public const string TenantNotFound = "Tenant not found.";
        public const string EmployeeDoesNotBelongToTenant = "Employee does not belong to the specified tenant.";
        public const string FailedToLogIssue = "Failed to log location tracking issue.";
        public const string ViolationNotificationTitle = "Location Tracking Violation";
        public const string ViolationNotificationMessage = "Employee {0} triggered a {1} violation.";
        public const string UnknownEmployeeName = "Unknown Employee";
    }

    /// <summary>
    /// Location tracking configuration API messages
    /// </summary>
    public static class LocationTrackingConfigurationMessages
    {
        public const string ConfigurationFetchedSuccessfully = "Location tracking configuration fetched successfully.";
        public const string EmployeeNotFound = "Employee not found.";
        public const string TenantNotFound = "Tenant not found.";
        public const string TenantConfigurationNotFound = "Tenant configuration not found.";
        public const string LocationTrackingConfigurationNotFound = "Location tracking configuration not found.";
        public const string EmployeeDoesNotBelongToTenant = "Employee does not belong to the specified tenant.";
    }

    /// <summary>
    /// Attendance status text used in responses
    /// </summary>
    public static class AttendanceStatusMessages
    {
        public const string Present = "Present";
        public const string Absent = "Absent";
        public const string Weekend = "Weekend";
        public const string Future = "Future";
        public const string NotMarked = "Not Marked";
    }
}

