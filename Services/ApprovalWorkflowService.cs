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

                // Insert initial approval stage
                await InsertInitialApprovalStageAsync(eventId, eventTypeId, userId, tenantId);

                return (true, ApprovalWorkflowMessages.WorkflowInitiatedSuccessfully, eventId);
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.ApprovalWorkflow.InitiateLeaveWorkflow, nameof(InitiateLeaveRequestApprovalAsync), ex, userId);
                return (false, ApprovalWorkflowMessages.ErrorInitiatingWorkflow, 0);
            }
        }

        /// <summary>
        /// Insert the initial approval stage for an event
        /// </summary>
        public async Task InsertInitialApprovalStageAsync(int eventId, int eventTypeId, int userId, int tenantId)
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

                // Get approvers for this stage
                var approvers = await _approvalRepository.GetApproversForStageAsync(
                    stage.Id, 
                    stage.WorkRoleId, 
                    stage.ExplicitUserIds, 
                    tenantId);

                var approverList = approvers.ToList();
                if (!approverList.Any())
                {
                    _logger.LogWarning(LogMessages.ApprovalWorkflow.NoApproversFound, stage.Id);
                    return;
                }

                // Get notification template from database for screen notifications
                var screenNotificationTemplate = await _approvalRepository.GetNotificationTemplateAsync(
                    EventConstants.LeaveEvent, 
                    "Screen Notification", 
                    "Submission", 
                    tenantId);

                // Build token values for template replacement
                var eventRecord = await _approvalRepository.GetEventByIdAsync(eventId, tenantId);
                var requestingEmployee = await _approvalRepository.GetEmployeeByUserIdAsync(userId);

                // Insert approval records and send notifications for each approver
                foreach (var approver in approverList)
                {
                    // Insert approval record
                    await _approvalRepository.InsertApprovalAsync(eventId, stage.Id, approver.UserId, userId, tenantId);

                    // Build token values for this notification
                    var tokenValues = await BuildTokenValuesAsync(eventId, approver, tenantId);
                    
                    // Add additional tokens for screen notification template
                    tokenValues["{Username}"] = requestingEmployee?.Name ?? "Employee";
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
                    string notificationTitle = screenNotificationTemplate?.Title ?? ApprovalWorkflowMessages.LeaveRequestPendingApproval;
                    string notificationMessage = screenNotificationTemplate?.Body ?? ApprovalWorkflowMessages.LeaveRequestRequiresApproval;

                    // Replace tokens in template
                    notificationTitle = ReplaceTokens(notificationTitle, tokenValues);
                    notificationMessage = ReplaceTokens(notificationMessage, tokenValues);
                    
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
                            // Get email template
                            var template = await _approvalRepository.GetEmailTemplateAsync(EventConstants.LeaveEvent, "Submission", tenantId);
                            
                            string emailSubject = template?.Subject ?? notificationTitle;
                            string emailBody = template?.Body ?? notificationMessage;

                            // Replace tokens in template
                            emailSubject = ReplaceTokens(emailSubject, tokenValues);
                            emailBody = ReplaceTokens(emailBody, tokenValues);

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
        /// Build token values for email template
        /// </summary>
        private async Task<Dictionary<string, string>> BuildTokenValuesAsync(int eventId, ApproverInfo approver, int tenantId)
        {
            var tokenValues = new Dictionary<string, string>
            {
                ["[Approver_Name]"] = approver.Name ?? "Approver"
            };

            // Get tenant name
            var tenantName = await _approvalRepository.GetTenantNameAsync(tenantId);
            tokenValues["[Company_Name]"] = tenantName ?? "Company";

            // Get event data and extract relevant fields
            var eventRecord = await _approvalRepository.GetEventByIdAsync(eventId, tenantId);
            if (eventRecord != null && !string.IsNullOrEmpty(eventRecord.EventData))
            {
                try
                {
                    var eventData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(eventRecord.EventData);
                    if (eventData != null)
                    {
                        if (eventData.TryGetValue("start_date", out var startDate))
                            tokenValues["[Start_Date]"] = startDate.GetString() ?? "";
                        
                        if (eventData.TryGetValue("end_date", out var endDate))
                            tokenValues["[End_Date]"] = endDate.GetString() ?? "";
                        
                        if (eventData.TryGetValue("reason", out var reason))
                            tokenValues["[Reason]"] = reason.GetString() ?? "";

                        if (eventData.TryGetValue("requested_user_id", out var requestedUserId))
                        {
                            var employee = await _approvalRepository.GetEmployeeByUserIdAsync(requestedUserId.GetInt32());
                            tokenValues["[Employee_Name]"] = employee?.Name ?? "Employee";
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

        /// <summary>
        /// Replace tokens in template with actual values
        /// Supports both {Token} and [Token] formats
        /// </summary>
        private string ReplaceTokens(string template, Dictionary<string, string> tokenValues)
        {
            if (string.IsNullOrEmpty(template)) return template;

            foreach (var token in tokenValues)
            {
                template = template.Replace(token.Key, token.Value);
            }

            return template;
        }

        #endregion
    }
}
