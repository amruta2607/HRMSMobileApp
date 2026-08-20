using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using System.Text.Json;

namespace MobileWebApi.Services
{
    public class ApprovalWorkflowService : IApprovalWorkflowService
    {
        private readonly IApprovalRepository _approvalRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<ApprovalWorkflowService> _logger;

        public ApprovalWorkflowService(
            IApprovalRepository approvalRepository,
            IEmailService emailService,
            ILogger<ApprovalWorkflowService> logger)
        {
            _approvalRepository = approvalRepository;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Initiates the approval workflow for a leave request
        /// </summary>
        public async Task<(bool Success, string Message, int EventId)> InitiateLeaveRequestApprovalAsync(
            LeaveRequest leaveRequest, int userId, int tenantId)
        {
            try
            {
                _logger.LogInformation(LogMessages.ApprovalWorkflow.InitiatingApprovalWorkflow, leaveRequest.Id);

                // Get event type ID for Leave Request
                var eventTypeId = await _approvalRepository.GetEventTypeIdAsync(EventConstants.LeaveEvent, tenantId);
                if (eventTypeId == 0)
                {
                    _logger.LogWarning(LogMessages.ApprovalWorkflow.EventTypeNotFound, EventConstants.LeaveEvent, tenantId);
                    return (false, ApprovalWorkflowMessages.EventTypeNotConfigured, 0);
                }

                // Check if event type is active
                var isActive = await _approvalRepository.IsEventTypeActiveAsync(eventTypeId, tenantId);
                if (!isActive)
                {
                    _logger.LogWarning(LogMessages.ApprovalWorkflow.EventTypeNotActive, EventConstants.LeaveEvent, tenantId);
                    return (false, ApprovalWorkflowMessages.EventTypeNotActive, 0);
                }

                // Create event data for leave request
                var eventData = new
                {
                    leave_request_id = leaveRequest.Id,
                    leave_type = leaveRequest.LeaveTypeId,
                    start_date = leaveRequest.FromDate.ToString("yyyy-MM-dd"),
                    end_date = leaveRequest.ToDate.ToString("yyyy-MM-dd"),
                    reason = leaveRequest.Description,
                    requested_user_id = userId,
                    duration = leaveRequest.Duration
                };

                // Serialize event data
                var eventDataJson = JsonSerializer.Serialize(eventData);

                // Insert event with State="Pending" and Status="Active"
                var eventId = await _approvalRepository.InsertEventAsync(userId, eventTypeId, eventDataJson, "Pending", "Active", tenantId, userId);
                if (eventId == 0)
                {
                    _logger.LogError(LogMessages.ApprovalWorkflow.FailedToInsertEvent);
                    return (false, ApprovalWorkflowMessages.FailedToCreateEvent, 0);
                }

                _logger.LogInformation(LogMessages.ApprovalWorkflow.EventInsertedSuccessfully, eventId);

                // Insert initial approval stage and notify approvers
                await InsertInitialApprovalStageAsync(eventId, eventTypeId, userId, tenantId, EventConstants.LeaveEvent);

                return (true, ApprovalWorkflowMessages.WorkflowInitiatedSuccessfully, eventId);
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.ApprovalWorkflow.InitiateLeaveWorkflow, nameof(InitiateLeaveRequestApprovalAsync), ex, userId);
                return (false, ApprovalWorkflowMessages.ErrorInitiatingWorkflow, 0);
            }
        }

        /// <summary>
        /// Initiates the approval workflow for a regularization (EmployeeDispute) request.
        /// First-level approval is assigned to the reporting manager (managerUserId).
        /// </summary>
        public async Task<(bool Success, string Message, int EventId)> InitiateRegularizationRequestApprovalAsync(
            EmployeeDispute dispute, int userId, int tenantId, int managerUserId)
        {
            try
            {
                _logger.LogInformation(LogMessages.ApprovalWorkflow.InitiatingRegularizationApprovalWorkflow, dispute.Id);

                if (managerUserId <= 0)
                {
                    return (false, DisputeMessages.NoReportingManagerAssigned, 0);
                }

                var eventTypeId = await _approvalRepository.GetEventTypeIdAsync(EventConstants.RegularizationEvent, tenantId);
                if (eventTypeId == 0)
                {
                    _logger.LogWarning(LogMessages.ApprovalWorkflow.EventTypeNotFound, EventConstants.RegularizationEvent, tenantId);
                    return (false, ApprovalWorkflowMessages.EventTypeNotConfigured, 0);
                }

                var isActive = await _approvalRepository.IsEventTypeActiveAsync(eventTypeId, tenantId);
                if (!isActive)
                {
                    _logger.LogWarning(LogMessages.ApprovalWorkflow.EventTypeNotActive, EventConstants.RegularizationEvent, tenantId);
                    return (false, ApprovalWorkflowMessages.EventTypeNotActive, 0);
                }

                // Build EventData identical to Web ApproveHelper + Mobile approval fields.
                // Must include dispute_id (required by Web approval engine) and full punch/date payload
                // so approve/reject can resolve RegularizationDetails from EventData alone.
                var eventDataJson = NotificationTokenHelper.BuildRegularizationEventDataJson(
                    dispute, userId, managerUserId);

                var eventId = await _approvalRepository.InsertEventAsync(userId, eventTypeId, eventDataJson, "Pending", "Active", tenantId, userId);
                if (eventId == 0)
                {
                    _logger.LogError(LogMessages.ApprovalWorkflow.FailedToInsertRegularizationEvent);
                    return (false, ApprovalWorkflowMessages.FailedToCreateEvent, 0);
                }

                _logger.LogInformation(LogMessages.ApprovalWorkflow.EventInsertedSuccessfully, eventId);
                _logger.LogInformation(LogMessages.ApprovalWorkflow.RoutingToReportingManager, eventId, managerUserId);

                // Assign first approval to the reporting manager and notify via existing Alert framework
                await InsertInitialApprovalStageAsync(
                    eventId, eventTypeId, userId, tenantId, EventConstants.RegularizationEvent, managerUserId);

                return (true, ApprovalWorkflowMessages.WorkflowInitiatedSuccessfully, eventId);
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.ApprovalWorkflow.InitiateRegularizationWorkflow, nameof(InitiateRegularizationRequestApprovalAsync), ex, userId);
                return (false, ApprovalWorkflowMessages.ErrorInitiatingWorkflow, 0);
            }
        }

        /// <summary>
        /// Insert the initial approval stage for an event and create Screen/Email notifications via existing Alert framework.
        /// When <paramref name="assignedApproverUserId"/> is set, that user is the sole first-level approver.
        /// </summary>
        public async Task InsertInitialApprovalStageAsync(
            int eventId, int eventTypeId, int userId, int tenantId, string eventName, int? assignedApproverUserId = null)
        {
            try
            {
                // Get first approval level name (e.g., "Level1")
                var levelName = await _approvalRepository.GetFirstLevelNameAsync(eventTypeId, tenantId);
                if (string.IsNullOrEmpty(levelName))
                {
                    _logger.LogWarning(LogMessages.ApprovalWorkflow.NoApprovalLevelsConfigured, eventTypeId);
                    return;
                }

                // Get approval stage details
                var stage = await _approvalRepository.GetApprovalStageByLevelNameAsync(eventTypeId, levelName, tenantId);
                if (stage == null)
                {
                    _logger.LogWarning(LogMessages.ApprovalWorkflow.ApprovalStageNotFound, levelName);
                    return;
                }

                // Check if approval stage is active
                var isStageActive = await _approvalRepository.IsApprovalStageActiveAsync(stage.Id, tenantId);
                if (!isStageActive)
                {
                    _logger.LogWarning(LogMessages.ApprovalWorkflow.ApprovalStageNotActive, stage.Id);
                    return;
                }

                // Resolve approvers: manager override (regularization) or stage WorkRole / ExplicitUserIds
                IEnumerable<ApproverInfo> approvers;
                if (assignedApproverUserId.HasValue && assignedApproverUserId.Value > 0)
                {
                    approvers = await _approvalRepository.GetApproversForStageAsync(
                        stage.Id,
                        workRoleId: null,
                        explicitUserIds: assignedApproverUserId.Value.ToString(),
                        tenantId);
                }
                else
                {
                    approvers = await _approvalRepository.GetApproversForStageAsync(
                        stage.Id,
                        stage.WorkRoleId,
                        stage.ExplicitUserIds,
                        tenantId);
                }

                var approverList = approvers.ToList();
                if (!approverList.Any())
                {
                    _logger.LogWarning(LogMessages.ApprovalWorkflow.NoApproversFound, stage.Id);
                    return;
                }

                // Get notification template from database (TemplateName = eventName, ActionType = Submission)
                var screenNotificationTemplate = await _approvalRepository.GetNotificationTemplateAsync(
                    eventName, 
                    StringConstants.TemplateTypeScreenNotification, 
                    StringConstants.ActionTypeSubmission, 
                    tenantId);

                // Build token values for template replacement
                var eventRecord = await _approvalRepository.GetEventByIdAsync(eventId, tenantId);
                var requestingEmployee = await _approvalRepository.GetEmployeeByUserIdAsync(userId);
                var eventDetails = await _approvalRepository.GetEventDetailsAsync(eventId, tenantId);

                var isLeaveEvent = string.Equals(eventName, EventConstants.LeaveEvent, StringComparison.OrdinalIgnoreCase);
                string fallbackTitle = isLeaveEvent
                    ? ApprovalWorkflowMessages.LeaveRequestPendingApproval
                    : ApprovalWorkflowMessages.RegularizationRequestPendingApproval;
                string fallbackMessage = isLeaveEvent
                    ? ApprovalWorkflowMessages.LeaveRequestRequiresApproval
                    : ApprovalWorkflowMessages.RegularizationRequestRequiresApproval;

                // Insert approval records and send notifications for each approver
                foreach (var approver in approverList)
                {
                    // Insert approval record
                    await _approvalRepository.InsertApprovalAsync(eventId, stage.Id, approver.UserId, userId, tenantId);

                    // Build token values for this notification (Web-aligned placeholder map)
                    var tokenValues = await BuildTokenValuesAsync(eventId, approver, tenantId, eventDetails);

                    NotificationTokenHelper.AddPersonNameTokens(
                        tokenValues,
                        employeeName: requestingEmployee?.Name ?? StringConstants.DefaultEmployeeName,
                        approverName: approver.Name ?? "Approver");

                    // Leave dates from EventData (keep existing behaviour)
                    if (eventRecord != null && !string.IsNullOrEmpty(eventRecord.EventData))
                    {
                        try
                        {
                            var eventData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(eventRecord.EventData);
                            if (eventData != null)
                            {
                                var startDate = eventData.TryGetValue("start_date", out var sd) ? sd.GetString() : "";
                                var endDate = eventData.TryGetValue("end_date", out var ed) ? ed.GetString() : "";
                                tokenValues["{LeaveDates}"] = $" from {startDate} to {endDate}";
                            }
                        }
                        catch { /* Ignore parsing errors */ }
                    }

                    // Get notification title and message from database template or fallback to constants
                    string notificationTitle = screenNotificationTemplate?.Title ?? fallbackTitle;
                    string notificationMessage = screenNotificationTemplate?.Body ?? fallbackMessage;

                    // Replace tokens in template
                    notificationTitle = NotificationTokenHelper.ReplaceTokens(notificationTitle, tokenValues);
                    notificationMessage = NotificationTokenHelper.ReplaceTokens(notificationMessage, tokenValues);
                    
                    await _approvalRepository.InsertScreenNotificationAsync(
                        approver.UserId, 
                        eventId, 
                        notificationTitle, 
                        notificationMessage, 
                        tenantId, 
                        userId);

                    _logger.LogInformation(LogMessages.ApprovalWorkflow.ScreenNotificationCreated, approver.UserId);

                    // Send email notification if email is available
                    if (!string.IsNullOrEmpty(approver.Email))
                    {
                        try
                        {
                            // Get email template (EventName = eventName, ActionType = Submission)
                            var template = await _approvalRepository.GetEmailTemplateAsync(eventName, StringConstants.ActionTypeSubmission, tenantId);
                            
                            string emailSubject = template?.Subject ?? notificationTitle;
                            string emailBody = template?.Body ?? notificationMessage;

                            // Replace tokens in template
                            emailSubject = NotificationTokenHelper.ReplaceTokens(emailSubject, tokenValues);
                            emailBody = NotificationTokenHelper.ReplaceTokens(emailBody, tokenValues);

                            // Insert email notification for background processing
                            await _approvalRepository.InsertEmailNotificationAsync(
                                approver.Email, 
                                emailSubject, 
                                emailBody, 
                                tenantId, 
                                userId);

                            _logger.LogInformation(LogMessages.ApprovalWorkflow.EmailNotificationQueued, approver.Email);
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning(emailEx, LogMessages.ApprovalWorkflow.FailedToSendEmailNotification, approver.Email);
                        }
                    }
                }

                _logger.LogInformation(LogMessages.ApprovalWorkflow.InitialApprovalStageInserted, 
                    eventId, stage.Id, approverList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ApprovalWorkflow.ErrorInsertingApprovalStage, eventId);
                throw;
            }
        }

        #region Helper Methods

        /// <summary>
        /// Build token values for email/screen templates (aligned with Web token map).
        /// </summary>
        private async Task<Dictionary<string, string>> BuildTokenValuesAsync(
            int eventId,
            ApproverInfo approver,
            int tenantId,
            EventDetails? eventDetails = null)
        {
            var tokenValues = new Dictionary<string, string>();

            NotificationTokenHelper.AddPersonNameTokens(
                tokenValues,
                approverName: approver.Name ?? "Approver");

            // Get tenant name
            var tenantName = await _approvalRepository.GetTenantNameAsync(tenantId);
            tokenValues[StringConstants.TokenCompanyName] = tenantName ?? StringConstants.DefaultCompanyName;

            eventDetails ??= await _approvalRepository.GetEventDetailsAsync(eventId, tenantId);

            if (!string.IsNullOrEmpty(eventDetails.LeaveDates))
            {
                tokenValues[StringConstants.TokenLeaveDates] = eventDetails.LeaveDates;
                tokenValues[StringConstants.TokenLeaveDatesAlt] = eventDetails.LeaveDates;
            }

            if (!string.IsNullOrEmpty(eventDetails.OvertimeDates))
            {
                tokenValues[StringConstants.TokenOvertimeDates] = eventDetails.OvertimeDates;
                tokenValues[StringConstants.TokenOvertimeDatesAlt] = eventDetails.OvertimeDates;
            }

            if (!string.IsNullOrEmpty(eventDetails.ReimbursementDates))
            {
                tokenValues[StringConstants.TokenReimbursementDates] = eventDetails.ReimbursementDates;
                tokenValues[StringConstants.TokenReimbursementDatesAlt] = eventDetails.ReimbursementDates;
            }

            if (!string.IsNullOrEmpty(eventDetails.ResignationDates))
            {
                tokenValues[StringConstants.TokenResignationDates] = eventDetails.ResignationDates;
                tokenValues[StringConstants.TokenResignationDatesAlt] = eventDetails.ResignationDates;
            }

            NotificationTokenHelper.AddRegularizationTokens(
                tokenValues,
                eventDetails.RegularizationDetails,
                string.IsNullOrEmpty(eventDetails.DisputeDate) ? null : eventDetails.DisputeDate);

            // Get event data and extract remaining fields
            var eventRecord = await _approvalRepository.GetEventByIdAsync(eventId, tenantId);
            if (eventRecord != null && !string.IsNullOrEmpty(eventRecord.EventData))
            {
                try
                {
                    var eventData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(eventRecord.EventData);
                    if (eventData != null)
                    {
                        if (eventData.TryGetValue("start_date", out var startDate))
                        {
                            tokenValues[StringConstants.TokenStartDate] = startDate.GetString() ?? "";
                            tokenValues[StringConstants.TokenStartDateAlt] = startDate.GetString() ?? "";
                        }

                        if (eventData.TryGetValue("end_date", out var endDate))
                        {
                            tokenValues[StringConstants.TokenEndDate] = endDate.GetString() ?? "";
                            tokenValues[StringConstants.TokenEndDateAlt] = endDate.GetString() ?? "";
                        }

                        if (eventData.TryGetValue("reason", out var reason))
                        {
                            tokenValues[StringConstants.TokenReason] = reason.GetString() ?? "";
                            tokenValues[StringConstants.TokenReasonAlt] = reason.GetString() ?? "";
                        }

                        // Build RegularizationDetails from EventData when not already resolved from DB
                        if (string.IsNullOrEmpty(eventDetails.RegularizationDetails)
                            && (eventData.ContainsKey(StringConstants.JsonKeyEmployeeDisputeId)
                                || eventData.ContainsKey(StringConstants.JsonKeyDisputeId)
                                || eventData.ContainsKey(StringConstants.JsonKeyDisputeDate)))
                        {
                            using var doc = JsonDocument.Parse(eventRecord.EventData);
                            var details = NotificationTokenHelper.BuildRegularizationDetailsFromEventData(doc.RootElement);
                            NotificationTokenHelper.AddRegularizationTokens(tokenValues, details);
                        }

                        if (eventData.TryGetValue("requested_user_id", out var requestedUserId)
                            && requestedUserId.ValueKind == JsonValueKind.Number)
                        {
                            var employee = await _approvalRepository.GetEmployeeByUserIdAsync(requestedUserId.GetInt32());
                            NotificationTokenHelper.AddPersonNameTokens(
                                tokenValues,
                                employeeName: employee?.Name ?? StringConstants.DefaultEmployeeName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, LogMessages.ApprovalWorkflow.ErrorParsingEventData);
                }
            }

            return tokenValues;
        }

        #endregion
    }
}
