namespace MobileWebApi.Constants
{
    /// <summary>
    /// String constants used throughout the application
    /// Contains all hardcoded string values for easy maintenance
    /// </summary>
    public static class StringConstants
    {
        #region Default Values

        public const string DefaultEmployeeName = "Employee";
        public const string DefaultCompanyName = "Company";
        public const string EmptyString = "";

        #endregion

        #region String Comparisons

        public const string LeaveKeyword = "leave";
        public const string PayrollSubmissionKeyword = "payrollsubmission";

        #endregion

        #region JSON Keys

        public const string JsonKeyReason = "reason";
        public const string JsonKeyPaymentDate = "payment_date";
        public const string JsonKeyLeaveRequestId = "leave_request_id";
        public const string JsonKeyLeaveRequestIdAlt = "leaveRequestId";
        public const string JsonKeyPayrollId = "payroll_id";
        public const string JsonKeyPayrollIdAlt = "payrollId";
        public const string JsonKeyReimbursementId = "reimbursement_id";
        public const string JsonKeyReimbursementIdAlt = "reimbursementId";
        public const string JsonKeyResignationId = "resignation_id";
        public const string JsonKeyResignationIdAlt = "resignationId";
        public const string JsonKeyOvertimeId = "overtime_id";
        public const string JsonKeyOvertimeIdAlt = "overtimeId";
        public const string JsonKeyEmployeeDisputeId = "employee_dispute_id";
        public const string JsonKeyEmployeeDisputeIdAlt = "employeeDisputeId";
        public const string JsonKeyDisputeId = "dispute_id";
        public const string JsonKeyDisputeIdAlt = "disputeId";
        public const string JsonKeyDisputeDate = "dispute_date";
        public const string JsonKeyEmployeeId = "employee_id";
        public const string JsonKeyDisputeCategoryId = "dispute_category_id";
        public const string JsonKeyStartDate = "start_date";
        public const string JsonKeyEndDate = "end_date";
        public const string JsonKeyRequestedUserId = "requested_user_id";
        public const string JsonKeyPunchId = "punch_id";
        public const string JsonKeyRequestedPunchInTime = "requested_punch_in_time";
        public const string JsonKeyRequestedPunchOutTime = "requested_punch_out_time";
        public const string JsonKeyManagerUserId = "manager_user_id";
        public const string JsonKeyApprovedUserId = "approved_user_id";
        public const string JsonKeyApprovedBy = "approvedBy";
        public const string JsonKeyApprovalTimestamp = "approvalTimestamp";
        public const string JsonKeyState = "state";
        public const string JsonKeyId = "id";
        public const string JsonKeyRequestId = "request_id";

        #endregion

        #region Date Formats

        public const string DateFormat = "dd-MMM-yyyy";
        public const string EventDataDateFormat = "yyyy-MM-dd";
        /// <summary>Web EventData punch datetime format (e.g. 2026-06-11 18:46:00).</summary>
        public const string EventDataDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string EventDataApprovalTimestampFormat = "o";

        #endregion

        #region Action Types

        public const string ActionTypeSubmission = "Submission";
        public const string ActionTypeManagerApproval = "ManagerApproval";
        public const string ActionTypeManagerRejection = "ManagerRejection";

        #endregion

        #region Template Types

        public const string TemplateTypeScreenNotification = "Screen Notification";

        #endregion

        #region String Separators

        public const string DateSeparator = " to ";
        public const string SpaceSeparator = " ";

        #endregion

        #region Token Keys

        public const string TokenUsername = "{Username}";
        public const string TokenEmployeeName = "[Employee_Name]";
        public const string TokenEmployeeNameBrace = "{EmployeeName}";
        public const string TokenEmployeeNameBracketNoUnderscore = "[EmployeeName]";
        public const string TokenApproverName = "{ApproverName}";
        public const string TokenApproverNameAlt = "[Approver_Name]";
        public const string TokenCompanyName = "[Company_Name]";
        public const string TokenStartDate = "[Start_Date]";
        public const string TokenStartDateAlt = "{Start_Date}";
        public const string TokenEndDate = "[End_Date]";
        public const string TokenEndDateAlt = "{End_Date}";
        public const string TokenLeaveDates = "{LeaveDates}";
        public const string TokenLeaveDatesAlt = "[LeaveDates]";
        public const string TokenOvertimeDates = "{OvertimeDates}";
        public const string TokenOvertimeDatesAlt = "[OvertimeDates]";
        public const string TokenReimbursementDates = "{ReimbursementDates}";
        public const string TokenReimbursementDatesAlt = "[ReimbursementDates]";
        public const string TokenResignationDates = "{ResignationDates}";
        public const string TokenResignationDatesAlt = "[ResignationDates]";
        public const string TokenRegularizationDetails = "{RegularizationDetails}";
        public const string TokenRegularizationDetailsAlt = "[RegularizationDetails]";
        public const string TokenDisputeDate = "{DisputeDate}";
        public const string TokenDisputeDateAlt = "[Dispute_Date]";
        public const string TokenPayrollMonthYear = "{PayrollMonthYear}";
        public const string TokenPayrollMonthYearAlt = "[Payroll_Month_Year]";
        public const string TokenPayrollMonth = "[Payroll_Month]";
        public const string TokenPayrollYear = "[Payroll_Year]";
        public const string TokenPaymentDate = "[Payment_Date]";
        public const string TokenReason = "[Reason]";
        public const string TokenReasonAlt = "{Reason}";

        #endregion

        #region Format Strings

        public const string LeaveDatesFormat = " {0} to {1}";
        public const string OvertimeDatesFormat = " on {0} for the specified duration {1} minutes";
        public const string ReimbursementDatesFormat = " on {0} with total amount {1}";
        public const string ResignationDatesFormat = " on {0} with Resignation Number {1}";
        public const string PayrollMonthYearFormat = "{0} {1}";

        #endregion

        #region Messages

        public const string CancellationApproved = "Cancellation approved";
        public const string FailedToApproveLeaveCancellationRequest = "Failed to approve leave cancellation request.";
        public const string FailedToApproveLeaveRequest = "Failed to approve leave request.";

        #endregion

        #region Log Messages

        public const string LogHandlingEventUpdatingStatus = "Handling {EventName} event - updating event and approval status";
        public const string LogRejectingCancelLeaveRequest = "Rejecting CancelLeave request - original leave remains approved, balance unchanged";
        public const string LogHandlingEventRejectionUpdatingStatus = "Handling {EventName} rejection - updating event and approval status";

        #endregion

        #region Event Name Patterns

        public const string EventNameLeaveRequest = "LeaveRequest";
        public const string EventNameCancelLeave = "CancelLeave";
        public const string EventNameOvertimeRequest = "OvertimeRequest";
        public const string EventNameReimbursementRequest = "ReimbursementRequest";
        public const string EventNameResignationRequest = "ResignationRequest";
        public const string EventNamePayrollSubmission = "PayrollSubmission";
        public const string EventNameRegularizationRequest = "RegularizationRequest";

        #endregion

        #region Email Templates

        // Email template titles
        public const string EmailTitlePasswordResetOtp = "Password Reset OTP";
        public const string EmailTitlePasswordResetSuccessful = "Password Reset Successful";

        // Email template headers
        public const string EmailHeaderPasswordReset = "Password Reset";
        public const string EmailHeaderPasswordResetSuccessful = "✓ Password Reset Successful";

        // Email template content
        public const string EmailGreeting = "Hello";
        public const string EmailOtpRequestMessage = "We received a request to reset your password. Please use the OTP below to complete the password reset process:";
        public const string EmailOtpLabel = "Your One-Time Password (OTP)";
        public const string EmailOtpValidityMessage = "⏰ This OTP is valid for 10 minutes only.";
        public const string EmailOtpIgnoreMessage = "If you didn't request a password reset, please ignore this email or contact support if you have concerns.";
        public const string EmailPasswordResetSuccessMessage = "Your password has been successfully reset. You can now log in with your new password.";
        public const string EmailSecurityTip = "Security Tip:";
        public const string EmailSecurityTipMessage = "If you did not make this change, please contact our support team immediately.";
        public const string EmailSecurityRecommendations = "For your security, we recommend:";
        public const string EmailSecurityTip1 = "Use a strong, unique password";
        public const string EmailSecurityTip2 = "Never share your password with anyone";
        public const string EmailSecurityTip3 = "Enable two-factor authentication if available";
        public const string EmailAutomatedMessage = "This is an automated message. Please do not reply to this email.";
        public const string EmailCopyrightTemplate = "© {0} Mobile App. All rights reserved.";

        // App name
        public const string AppName = "Mobile App";

        // Email masking
        public const string MaskedEmailPlaceholder = "****";

        #endregion
    }
}

