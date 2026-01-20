using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Linq;

namespace MobileWebApi.Services
{
    public class AlertService : IAlertService
    {
        private readonly IAlertRepository _repo;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILeaveService? _leaveService;
        private readonly ILeaveRepository? _leaveRepository;
        private readonly IApprovalRepository? _approvalRepository;
        private readonly ILogger<AlertService> _logger;

        public AlertService(
            IAlertRepository repo, 
            IEmployeeRepository employeeRepository, 
            ILogger<AlertService> logger,
            ILeaveService? leaveService = null,
            ILeaveRepository? leaveRepository = null,
            IApprovalRepository? approvalRepository = null)
        {
            _repo = repo;
            _employeeRepository = employeeRepository;
            _logger = logger;
            _leaveService = leaveService;
            _leaveRepository = leaveRepository;
            _approvalRepository = approvalRepository;
        }

        public async Task<AlertResponse> GetAlertByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.RetrievingAlertById, id);
                
                var alert = await _repo.GetAlertByIdAsync(id);
                if (alert == null)
                {
                    _logger.LogWarning(AlertMessages.AlertNotFound);
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.AlertNotFound,
                        Data = null
                    };
                }

                return new AlertResponse
                {
                    Success = true,
                    Message = AlertMessages.AlertRetrievedSuccessfully,
                    Data = alert
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorRetrievingAlert, id);
                return new AlertResponse
                {
                    Success = false,
                    Message = string.Format(AlertMessages.ErrorRetrievingAlert, ex.Message),
                    Data = null
                };
            }
        }

        public async Task<AlertListResponse> GetAlertsByUserIdAsync(int userId, bool? isRead = null, bool? isActive = null)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.RetrievingAlertsForUser, userId);
                
                var alerts = await _repo.GetAlertsByUserIdAsync(userId, isRead, isActive);
                var alertList = alerts.ToList();
                var unreadCount = await _repo.GetUnreadCountAsync(userId);

                return new AlertListResponse
                {
                    Success = true,
                    Message = AlertMessages.AlertsRetrievedSuccessfully,
                    Data = alertList,
                    TotalRecords = alertList.Count,
                    UnreadCount = unreadCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorRetrievingAlertsForUser, userId);
                return new AlertListResponse
                {
                    Success = false,
                    Message = string.Format(AlertMessages.ErrorRetrievingAlerts, ex.Message),
                    Data = null,
                    TotalRecords = 0,
                    UnreadCount = 0
                };
            }
        }

        public async Task<AlertListResponse> GetAlertsByOrganisationIdAsync(int organisationId, bool? isRead = null, bool? isActive = null)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.RetrievingAlertsForTenant, organisationId);
                
                var alerts = await _repo.GetAlertsByOrganisationIdAsync(organisationId, isRead, isActive);
                var alertList = alerts.ToList();

                return new AlertListResponse
                {
                    Success = true,
                    Message = AlertMessages.AlertsRetrievedSuccessfully,
                    Data = alertList,
                    TotalRecords = alertList.Count,
                    UnreadCount = alertList.Count(a => !a.IsRead)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorRetrievingAlertsForTenant, organisationId);
                return new AlertListResponse
                {
                    Success = false,
                    Message = string.Format(AlertMessages.ErrorRetrievingAlerts, ex.Message),
                    Data = null,
                    TotalRecords = 0,
                    UnreadCount = 0
                };
            }
        }

        public async Task<AlertListResponse> GetAlertsAsync(GetAlertsRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.RetrievingAlertsForUser, request.UserId);
                
                // Organization ID is now passed directly as int
                var alerts = await _repo.GetAlertsAsync(request);
                var alertList = alerts.ToList();

                return new AlertListResponse
                {
                    Success = true,
                    Message = AlertMessages.AlertsRetrievedSuccessfully,
                    Data = alertList,
                    TotalRecords = alertList.Count,
                    UnreadCount = alertList.Count(a => !a.IsRead)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorRetrievingAlerts);
                return new AlertListResponse
                {
                    Success = false,
                    Message = string.Format(AlertMessages.ErrorRetrievingAlerts, ex.Message),
                    Data = null,
                    TotalRecords = 0,
                    UnreadCount = 0
                };
            }
        }

        public async Task<AlertResponse> CreateAlertAsync(CreateAlertRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.CreatingAlert);
                
                // Organization ID is now passed directly as int
                var alertId = await _repo.CreateAlertAsync(request);
                if (alertId <= 0)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.FailedToCreateAlert,
                        Data = null
                    };
                }

                var alert = await _repo.GetAlertByIdAsync(alertId);
                return new AlertResponse
                {
                    Success = true,
                    Message = AlertMessages.AlertCreatedSuccessfully,
                    Data = alert
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorCreatingAlert);
                return new AlertResponse
                {
                    Success = false,
                    Message = string.Format(AlertMessages.ErrorCreatingAlert, ex.Message),
                    Data = null
                };
            }
        }

        public async Task<AlertResponse> UpdateAlertAsync(UpdateAlertRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.UpdatingAlert, request.Id);
                
                var success = await _repo.UpdateAlertAsync(request);
                if (!success)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.FailedToUpdateAlert,
                        Data = null
                    };
                }

                var alert = await _repo.GetAlertByIdAsync(request.Id);
                return new AlertResponse
                {
                    Success = true,
                    Message = AlertMessages.AlertUpdatedSuccessfully,
                    Data = alert
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorUpdatingAlert, request.Id);
                return new AlertResponse
                {
                    Success = false,
                    Message = string.Format(AlertMessages.ErrorUpdatingAlert, ex.Message),
                    Data = null
                };
            }
        }

        public async Task<AlertResponse> MarkAsReadAsync(int id, int? updateUserId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.MarkingAlertAsRead, id);
                
                var success = await _repo.MarkAsReadAsync(id, updateUserId);
                if (!success)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.FailedToMarkAlertAsRead,
                        Data = null
                    };
                }

                var alert = await _repo.GetAlertByIdAsync(id);
                return new AlertResponse
                {
                    Success = true,
                    Message = AlertMessages.AlertMarkedAsRead,
                    Data = alert
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorMarkingAlertAsRead, id);
                return new AlertResponse
                {
                    Success = false,
                    Message = string.Format(AlertMessages.ErrorMarkingAlertAsRead, ex.Message),
                    Data = null
                };
            }
        }

        public async Task<AlertResponse> MarkAllAsReadAsync(int userId, int? updateUserId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.MarkingAllAlertsAsRead, userId);
                
                var success = await _repo.MarkAllAsReadAsync(userId, updateUserId);
                return new AlertResponse
                {
                    Success = true,
                    Message = success ? AlertMessages.AllAlertsMarkedAsRead : AlertMessages.NoUnreadAlertsFound,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorMarkingAllAlertsAsRead, userId);
                return new AlertResponse
                {
                    Success = false,
                    Message = string.Format(AlertMessages.ErrorMarkingAllAlertsAsRead, ex.Message),
                    Data = null
                };
            }
        }

        public async Task<AlertResponse> DeleteAlertAsync(int id)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.DeletingAlert, id);
                
                var success = await _repo.DeleteAlertAsync(id);
                if (!success)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.FailedToDeleteAlert,
                        Data = null
                    };
                }

                return new AlertResponse
                {
                    Success = true,
                    Message = AlertMessages.AlertDeletedSuccessfully,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorDeletingAlert, id);
                return new AlertResponse
                {
                    Success = false,
                    Message = string.Format(AlertMessages.ErrorDeletingAlert, ex.Message),
                    Data = null
                };
            }
        }

        public async Task<AlertResponse> DeactivateAlertAsync(int id, int? updateUserId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.DeactivatingAlert, id);
                
                var success = await _repo.DeactivateAlertAsync(id, updateUserId);
                if (!success)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.FailedToDeactivateAlert,
                        Data = null
                    };
                }

                var alert = await _repo.GetAlertByIdAsync(id);
                return new AlertResponse
                {
                    Success = true,
                    Message = AlertMessages.AlertDeactivatedSuccessfully,
                    Data = alert
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorDeactivatingAlert, id);
                return new AlertResponse
                {
                    Success = false,
                    Message = string.Format(AlertMessages.ErrorDeactivatingAlert, ex.Message),
                    Data = null
                };
            }
        }

        public async Task<AlertResponse> ApproveAlertAsync(int id, int? updateUserId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.ApprovingAlert, id);
                
                // Check if alert exists
                var existingAlert = await _repo.GetAlertByIdAsync(id);
                if (existingAlert == null)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.AlertNotFound,
                        Data = null
                    };
                }

                var success = await _repo.ApproveAlertAsync(id, updateUserId);
                if (!success)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.FailedToApproveAlert,
                        Data = null
                    };
                }

                var alert = await _repo.GetAlertByIdAsync(id);
                return new AlertResponse
                {
                    Success = true,
                    Message = AlertMessages.AlertApprovedSuccessfully,
                    Data = alert
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorApprovingAlert, id);
                return new AlertResponse
                {
                    Success = false,
                    Message = string.Format(AlertMessages.ErrorApprovingAlert, ex.Message),
                    Data = null
                };
            }
        }

        public async Task<AlertResponse> RejectAlertAsync(int id, int? updateUserId, string? reason)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.RejectingAlert, id);
                
                // Check if alert exists
                var existingAlert = await _repo.GetAlertByIdAsync(id);
                if (existingAlert == null)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.AlertNotFound,
                        Data = null
                    };
                }

                var success = await _repo.RejectAlertAsync(id, updateUserId, reason);
                if (!success)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.FailedToRejectAlert,
                        Data = null
                    };
                }

                var alert = await _repo.GetAlertByIdAsync(id);
                return new AlertResponse
                {
                    Success = true,
                    Message = AlertMessages.AlertRejectedSuccessfully,
                    Data = alert
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorRejectingAlert, id);
                return new AlertResponse
                {
                    Success = false,
                    Message = string.Format(AlertMessages.ErrorRejectingAlert, ex.Message),
                    Data = null
                };
            }
        }

        /// <summary>
        /// Send approval notification to the requester when their request is approved
        /// </summary>
        public async Task<AlertResponse> SendApprovalNotificationAsync(SendApprovalNotificationRequest request, int organizationId, int approverUserId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.SendingApprovalNotification, request.RequesterUserId, request.EventName);

                // Generate default title and message if not provided
                var title = request.Title ?? string.Format(AlertWorkflowMessages.EventApprovedTitle, request.EventName);
                var message = request.Message ?? string.Format(AlertWorkflowMessages.EventApprovedMessage, request.EventName);

                var createRequest = new CreateAlertRequest
                {
                    organization = organizationId,
                    UserId = request.RequesterUserId,
                    EventId = request.EventId,
                    Title = title,
                    Message = message,
                    Status = NotificationStatusConstants.Unread, // Status can only be "Unread" or "Read" - set as "Unread" for new notification
                    InsertUserId = approverUserId
                };

                var alertId = await _repo.CreateAlertAsync(createRequest);
                if (alertId <= 0)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.FailedToCreateAlert,
                        Data = null
                    };
                }

                var alert = await _repo.GetAlertByIdAsync(alertId);
                return new AlertResponse
                {
                    Success = true,
                    Message = AlertMessages.ApprovalNotificationSentSuccessfully,
                    Data = alert
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorSendingApprovalNotification, request.RequesterUserId);
                return new AlertResponse
                {
                    Success = false,
                    Message = string.Format(AlertMessages.ErrorSendingApprovalNotification, ex.Message),
                    Data = null
                };
            }
        }

        /// <summary>
        /// Send rejection notification to the requester when their request is rejected
        /// </summary>
        public async Task<AlertResponse> SendRejectionNotificationAsync(SendRejectionNotificationRequest request, int organizationId, int rejecterUserId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.SendingRejectionNotification, request.RequesterUserId, request.EventName);

                // Generate default title and message if not provided
                var title = request.Title ?? string.Format(AlertWorkflowMessages.EventRejectedTitle, request.EventName);
                var message = request.Message ?? (!string.IsNullOrWhiteSpace(request.Reason) 
                    ? string.Format(AlertWorkflowMessages.EventRejectedMessage, request.EventName, request.Reason)
                    : string.Format(AlertWorkflowMessages.EventRejectedMessageNoReason, request.EventName));

                var createRequest = new CreateAlertRequest
                {
                    organization = organizationId,
                    UserId = request.RequesterUserId,
                    EventId = request.EventId,
                    Title = title,
                    Message = message,
                    Status = NotificationStatusConstants.Unread, // Status can only be "Unread" or "Read" - set as "Unread" for new notification
                    InsertUserId = rejecterUserId
                };

                var alertId = await _repo.CreateAlertAsync(createRequest);
                if (alertId <= 0)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.FailedToCreateAlert,
                        Data = null
                    };
                }

                var alert = await _repo.GetAlertByIdAsync(alertId);
                return new AlertResponse
                {
                    Success = true,
                    Message = AlertMessages.RejectionNotificationSentSuccessfully,
                    Data = alert
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorSendingRejectionNotification, request.RequesterUserId);
                return new AlertResponse
                {
                    Success = false,
                    Message = string.Format(AlertMessages.ErrorSendingRejectionNotification, ex.Message),
                    Data = null
                };
            }
        }

        /// <summary>
        /// Extract request ID from EventData JSON based on event type
        /// </summary>
        private int? ExtractRequestIdFromEventData(string? eventData, string eventName)
        {
            if (string.IsNullOrEmpty(eventData))
                return null;

            try
            {
                var eventDataDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(eventData);
                if (eventDataDict == null)
                    return null;

                var eventNameLower = eventName.ToLower();
                string? idKey = null;

                // Determine the ID key based on event type
                if (eventNameLower.Contains(EventNameConstants.LeaveRequestLower) || 
                    eventNameLower.Contains(StringConstants.LeaveKeyword) || 
                    eventNameLower.Contains(EventNameConstants.CancelLeave.ToLower()))
                {
                    idKey = StringConstants.JsonKeyLeaveRequestId;
                }
                else if (eventNameLower.Contains(EventNameConstants.PayrollSubmission.ToLower()))
                {
                    idKey = StringConstants.JsonKeyPayrollId;
                }
                else if (eventNameLower.Contains(EventNameConstants.ReimbursementRequest.ToLower()))
                {
                    idKey = StringConstants.JsonKeyReimbursementId;
                }
                else if (eventNameLower.Contains(EventNameConstants.ResignationRequest.ToLower()))
                {
                    idKey = StringConstants.JsonKeyResignationId;
                }
                else if (eventNameLower.Contains(EventNameConstants.OvertimeRequest.ToLower()))
                {
                    idKey = StringConstants.JsonKeyOvertimeId;
                }

                JsonElement idElement = default;
                bool found = false;

                // Try the specific key first
                if (idKey != null && eventDataDict.TryGetValue(idKey, out idElement))
                {
                    found = true;
                }
                else
                {
                    // Try common alternative keys if specific key not found
                    var alternativeKeys = new[] { StringConstants.JsonKeyId, StringConstants.JsonKeyRequestId, $"{eventNameLower.Replace(StringConstants.SpaceSeparator, "_")}_id" };
                    foreach (var key in alternativeKeys)
                    {
                        if (eventDataDict.TryGetValue(key, out idElement))
                        {
                            idKey = key;
                            found = true;
                            break;
                        }
                    }
                }

                if (found)
                {
                    if (idElement.ValueKind == JsonValueKind.Number)
                    {
                        return idElement.GetInt32();
                    }
                    else if (idElement.ValueKind == JsonValueKind.String)
                    {
                        if (int.TryParse(idElement.GetString(), out int parsedId))
                        {
                            return parsedId;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorExtractingRequestIdFromEventData, eventName);
                return null;
            }
        }

        /// <summary>
        /// Build token values from EventData for template replacement
        /// Extracts details from database tables similar to web application
        /// </summary>
        private async Task<Dictionary<string, string>> BuildTokenValuesForNotificationAsync(
            Event eventRecord, 
            string eventName, 
            int requesterUserId, 
            int organisationId,
            string? reason = null)
        {
            var tokenValues = new Dictionary<string, string>();

            // Get requester employee info
            var requesterEmployee = await _approvalRepository?.GetEmployeeByUserIdAsync(requesterUserId);
            if (requesterEmployee != null)
            {
                tokenValues[StringConstants.TokenUsername] = requesterEmployee.Name ?? StringConstants.DefaultEmployeeName;
                tokenValues[StringConstants.TokenEmployeeName] = requesterEmployee.Name ?? StringConstants.DefaultEmployeeName;
            }

            // Get tenant name
            if (_approvalRepository != null)
            {
                var tenantName = await _approvalRepository.GetTenantNameAsync(organisationId);
                tokenValues[StringConstants.TokenCompanyName] = tenantName ?? StringConstants.DefaultCompanyName;
            }

            // Extract event details from database tables (similar to web app ExtractEventDetails)
            if (eventRecord != null && _approvalRepository != null)
            {
                try
                {
                    var eventDetails = await _approvalRepository.GetEventDetailsAsync(eventRecord.Id, organisationId);
                    
                    // Add formatted dates/details from database
                    if (!string.IsNullOrEmpty(eventDetails.LeaveDates))
                    {
                        tokenValues[StringConstants.TokenLeaveDates] = eventDetails.LeaveDates;
                        tokenValues[StringConstants.TokenLeaveDatesAlt] = eventDetails.LeaveDates;
                        
                        // Extract start and end dates from LeaveDates string for individual tokens
                        // Format: " dd-MMM-yyyy to dd-MMM-yyyy"
                        var dates = eventDetails.LeaveDates.Trim().Split(new[] { StringConstants.DateSeparator }, StringSplitOptions.RemoveEmptyEntries);
                        if (dates.Length == 2)
                        {
                            tokenValues[StringConstants.TokenStartDate] = dates[0].Trim();
                            tokenValues[StringConstants.TokenStartDateAlt] = dates[0].Trim();
                            tokenValues[StringConstants.TokenEndDate] = dates[1].Trim();
                            tokenValues[StringConstants.TokenEndDateAlt] = dates[1].Trim();
                        }
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

                    if (!string.IsNullOrEmpty(eventDetails.PayrollMonthYear))
                    {
                        tokenValues[StringConstants.TokenPayrollMonthYear] = eventDetails.PayrollMonthYear;
                        tokenValues[StringConstants.TokenPayrollMonthYearAlt] = eventDetails.PayrollMonthYear;
                        
                        // Split month and year if needed
                        var parts = eventDetails.PayrollMonthYear.Split(new[] { StringConstants.SpaceSeparator }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            tokenValues[StringConstants.TokenPayrollMonth] = parts[0];
                            tokenValues[StringConstants.TokenPayrollYear] = parts[parts.Length - 1];
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, LogMessages.Alert.ErrorExtractingEventDetails, eventRecord.Id);
                }
            }

            // Extract other fields from EventData JSON if needed
            if (eventRecord != null && !string.IsNullOrEmpty(eventRecord.EventData))
            {
                try
                {
                    var eventData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(eventRecord.EventData);
                    if (eventData != null)
                    {
                        // Extract reason from EventData if not already set
                        if (eventData.TryGetValue(StringConstants.JsonKeyReason, out var reasonElement) && string.IsNullOrEmpty(reason))
                        {
                            var reasonFromData = reasonElement.GetString() ?? StringConstants.EmptyString;
                            tokenValues[StringConstants.TokenReason] = reasonFromData;
                            tokenValues[StringConstants.TokenReasonAlt] = reasonFromData;
                        }

                        // Extract payment_date for payroll if available
                        var eventNameLower = eventName.ToLower();
                        if (eventNameLower.Contains(StringConstants.PayrollSubmissionKeyword))
                        {
                            if (eventData.TryGetValue(StringConstants.JsonKeyPaymentDate, out var paymentDate))
                            {
                                tokenValues[StringConstants.TokenPaymentDate] = paymentDate.GetString() ?? StringConstants.EmptyString;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, LogMessages.Alert.ErrorParsingEventDataForTokenReplacement, eventRecord.Id);
                }
            }

            // Add rejection reason if provided (overrides any reason from EventData)
            if (!string.IsNullOrEmpty(reason))
            {
                tokenValues[StringConstants.TokenReasonAlt] = reason;
                tokenValues[StringConstants.TokenReason] = reason;
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

        /// <summary>
        /// Get notification template from database and replace parameters
        /// </summary>
        private async Task<(string Title, string Message)> GetNotificationTemplateAsync(
            string eventName,
            bool isApproval,
            Event eventRecord,
            int requesterUserId,
            int organisationId,
            string? reason = null)
        {
            if (_approvalRepository == null)
            {
                // Fallback to hardcoded messages if repository not available
                var title = isApproval 
                    ? string.Format(AlertWorkflowMessages.EventApprovedTitle, eventName)
                    : string.Format(AlertWorkflowMessages.EventRejectedTitle, eventName);
                
                var message = isApproval 
                    ? string.Format(AlertWorkflowMessages.EventApprovedMessage, eventName)
                    : string.Format(AlertWorkflowMessages.EventRejectedMessageNoReason, eventName);
                
                return (title, message);
            }

            try
            {
                // Determine ActionType based on approval/rejection
                // For now, using ManagerApproval/ManagerRejection - can be enhanced to detect HR level
                var actionType = isApproval ? StringConstants.ActionTypeManagerApproval : StringConstants.ActionTypeManagerRejection;

                // Get template from database
                var template = await _approvalRepository.GetNotificationTemplateAsync(
                    eventName,
                    StringConstants.TemplateTypeScreenNotification,
                    actionType,
                    organisationId);

                if (template != null && template.IsActive)
                {
                    // Build token values (including reason if provided)
                    var tokenValues = await BuildTokenValuesForNotificationAsync(
                        eventRecord, 
                        eventName, 
                        requesterUserId, 
                        organisationId,
                        reason);

                    // Replace tokens in template
                    var title = ReplaceTokens(template.Title ?? eventName, tokenValues);
                    var message = ReplaceTokens(template.Body ?? "", tokenValues);

                    return (title, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, LogMessages.Alert.ErrorGettingNotificationTemplate, eventName);
            }

            // Fallback to hardcoded messages if template not found
            var fallbackTitle = isApproval 
                ? string.Format(AlertWorkflowMessages.EventApprovedTitle, eventName)
                : string.Format(AlertWorkflowMessages.EventRejectedTitle, eventName);
            
            var fallbackMessage = isApproval 
                ? string.Format(AlertWorkflowMessages.EventApprovedMessage, eventName)
                : string.Format(AlertWorkflowMessages.EventRejectedMessageNoReason, eventName);
            
            return (fallbackTitle, fallbackMessage);
        }

        /// <summary>
        /// Update Event and Approval status and send notification (common for all event types)
        /// </summary>
        private async Task<bool> UpdateEventAndApprovalStatusAsync(
            int eventId, 
            string eventName, 
            int approverUserId, 
            int organisationId, 
            int requesterUserId,
            bool isApproval,
            Event? eventRecord = null)
        {
            if (_approvalRepository == null)
                return false;

            try
            {
                // Get event record if not provided
                if (eventRecord == null)
                {
                    eventRecord = await _approvalRepository.GetEventByIdAsync(eventId, organisationId);
                }

                // Update Event table
                var state = isApproval ? EventStateConstants.Approved : EventStateConstants.Rejected;
                var status = isApproval ? EventStateConstants.ApprovedByManager : EventStateConstants.RejectedByManager;
                
                var eventUpdated = await _approvalRepository.UpdateEventStatusAsync(
                    eventId, 
                    state, 
                    status, 
                    approverUserId, 
                    organisationId);
                
                if (!eventUpdated)
                {
                    _logger.LogWarning(LogMessages.Alert.FailedToUpdateEventStatus, eventId);
                }

                // Get Approvals for this event and update the approval status
                var approvals = await _approvalRepository.GetApprovalsByEventIdAsync(eventId, organisationId);
                var approvalStatus = isApproval 
                    ? EventConstants.ApprovalStatusApproved 
                    : EventConstants.ApprovalStatusRejected;
                
                var comment = isApproval 
                    ? string.Format(AlertWorkflowMessages.ApprovedByUser, approverUserId)
                    : string.Format(AlertWorkflowMessages.RejectedByUser, approverUserId);

                foreach (var approval in approvals.Where(a => a.ApprovalStatus == EventConstants.ApprovalStatusPending))
                {
                    await _approvalRepository.UpdateApprovalStatusAsync(
                        approval.Id, 
                        approvalStatus, 
                        comment, 
                        approverUserId);
                }

                // Get notification template from database and replace parameters
                var (title, message) = await GetNotificationTemplateAsync(
                    eventName,
                    isApproval,
                    eventRecord,
                    requesterUserId,
                    organisationId);

                // Insert ScreenNotification for the requester
                var notificationId = await _approvalRepository.InsertScreenNotificationAsync(
                    requesterUserId,
                    eventId,
                    title,
                    message,
                    organisationId,
                    approverUserId
                );
                
                _logger.LogInformation(
                    isApproval ? AlertWorkflowMessages.NotificationCreatedForApproval : AlertWorkflowMessages.NotificationCreatedForRejection,
                    notificationId, requesterUserId, eventId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorUpdatingEventAndApprovalStatus, eventId);
                return false;
            }
        }

        /// <summary>
        /// Unified method to approve a request from an alert (for mobile app)
        /// This will: 1) Approve the underlying request, 2) Update alert status, 3) Send notification to requester
        /// Supports: LeaveRequest, CancelLeave, PayrollSubmission, ResignationRequest, OvertimeRequest, ReimbursementRequest
        /// </summary>
        public async Task<AlertResponse> ApproveRequestFromAlertAsync(ApproveRequestFromAlertRequest request, int approverUserId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.ApprovingRequestFromAlert, request.AlertId);

                // Get the alert to extract EventId and EventName
                var alert = await _repo.GetAlertByIdAsync(request.AlertId);
                if (alert == null)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.AlertNotFound,
                        Data = null
                    };
                }

                // EventId in the alert points to the Event table
                var eventId = request.EventId ?? alert.EventId;
                var eventName = request.EventName ?? alert.Title ?? EventNameConstants.Request;
                
                if (!eventId.HasValue)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertWorkflowMessages.EventIdNotFound,
                        Data = null
                    };
                }

                // Get Event from Event table to extract EventData
                Event? eventRecord = null;

                if (_approvalRepository == null)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertWorkflowMessages.ApprovalRepositoryNotAvailable,
                        Data = null
                    };
                }

                eventRecord = await _approvalRepository.GetEventByIdAsync(eventId.Value, alert.OrganisationId);
                if (eventRecord == null)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = string.Format(AlertWorkflowMessages.EventNotFound, eventId.Value),
                        Data = null
                    };
                }

                // Get EventType from EventTypes table to get the proper EventName
                EventType? eventType = null;
                if (eventRecord.EventTypeId > 0)
                {
                    eventType = await _approvalRepository.GetEventTypeByIdAsync(eventRecord.EventTypeId, alert.OrganisationId);
                    if (eventType != null && !string.IsNullOrEmpty(eventType.EventName))
                    {
                        eventName = eventType.EventName;
                    }
                }

                // Extract request ID from EventData based on event type
                var requestId = ExtractRequestIdFromEventData(eventRecord.EventData, eventName);
                
                // Route to appropriate service based on event type
                var eventNameLower = eventName.ToLower();
                bool requestApproved = false;

                // Handle LeaveRequest and CancelLeave with LeaveService
                if (eventNameLower.Contains(EventNameConstants.LeaveRequestLower) || 
                    eventNameLower.Contains(StringConstants.LeaveKeyword) || 
                    eventNameLower.Contains(EventNameConstants.CancelLeave.ToLower()))
                {
                    if (!requestId.HasValue)
                    {
                        return new AlertResponse
                        {
                            Success = false,
                            Message = AlertWorkflowMessages.LeaveRequestIdNotFound,
                            Data = null
                        };
                    }

                    if (_leaveService == null)
                    {
                        return new AlertResponse
                        {
                            Success = false,
                            Message = LeaveMessages.LeaveServiceNotAvailable,
                            Data = null
                        };
                    }

                    // Optionally: Verify the leave request exists before processing
                    if (_leaveRepository != null)
                    {
                        var leaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(requestId.Value);
                        if (leaveRequest == null)
                        {
                            return new AlertResponse
                            {
                                Success = false,
                                Message = string.Format(LeaveMessages.LeaveRequestNotFoundWithId, requestId.Value),
                                Data = null
                            };
                        }
                    }

                    // Handle CancelLeave differently - it should restore the leave balance
                    if (eventNameLower.Contains(EventNameConstants.CancelLeave.ToLower()))
                    {
                        // CancelLeave approval means approving the cancellation request
                        // This will restore the leave balance that was deducted when the leave was originally approved
                        var cancelResult = await _leaveService.CancelLeaveRequestAsync(requestId.Value, approverUserId, StringConstants.CancellationApproved);
                        requestApproved = cancelResult.Success;
                        
                        if (!requestApproved)
                        {
                            return new AlertResponse
                            {
                                Success = false,
                                Message = cancelResult.Message ?? StringConstants.FailedToApproveLeaveCancellationRequest,
                                Data = null
                            };
                        }
                    }
                    else
                    {
                        // Approve the leave request (normal leave approval)
                        var leaveResult = await _leaveService.ApproveLeaveRequestAsync(requestId.Value, approverUserId);
                        requestApproved = leaveResult.Success;
                        
                        if (!requestApproved)
                        {
                            return new AlertResponse
                            {
                                Success = false,
                                Message = leaveResult.Message ?? StringConstants.FailedToApproveLeaveRequest,
                                Data = null
                            };
                        }
                    }
                }
                else
                {
                    // For other event types (PayrollSubmission, ResignationRequest, OvertimeRequest, ReimbursementRequest)
                    // For PayrollSubmission specifically, also update payroll approval status in Payroll table
                    if (eventNameLower.Contains(StringConstants.PayrollSubmissionKeyword) && requestId.HasValue && _approvalRepository != null)
                    {
                        await _approvalRepository.UpdatePayrollApprovalStatusAsync(requestId.Value, true, alert.OrganisationId);
                    }

                    // Update event status only (business logic for other types may be handled by web application)
                    _logger.LogInformation(StringConstants.LogHandlingEventUpdatingStatus, eventName);
                    requestApproved = true; // We'll update the event status regardless
                }

                // Update Event and Approval status, and send notification (common for all event types)
                await UpdateEventAndApprovalStatusAsync(
                    eventId.Value,
                    eventName,
                    approverUserId,
                    alert.OrganisationId,
                    (int)alert.InsertUserId, // Requester's UserId
                    isApproval: true,
                    eventRecord);

                // Update alert status to "Read" (for the approver's alert)
                var updateSuccess = await _repo.ApproveAlertAsync(request.AlertId, approverUserId);
                if (!updateSuccess)
                {
                    _logger.LogWarning(AlertWorkflowMessages.FailedToUpdateAlertStatus);
                }

                // Return updated alert
                var updatedAlert = await _repo.GetAlertByIdAsync(request.AlertId);
                return new AlertResponse
                {
                    Success = true,
                    Message = AlertMessages.RequestApprovedSuccessfully,
                    Data = updatedAlert
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorApprovingRequestFromAlert, request.AlertId);
                return new AlertResponse
                {
                    Success = false,
                    Message = string.Format(AlertWorkflowMessages.ErrorApprovingRequest, ex.Message),
                    Data = null
                };
            }
        }

        /// <summary>
        /// Update Event and Approval status and send notification for rejection (common for all event types)
        /// </summary>
        private async Task<bool> UpdateEventAndApprovalStatusForRejectionAsync(
            int eventId, 
            string eventName, 
            int rejecterUserId, 
            int organisationId, 
            int requesterUserId,
            string reason,
            Event? eventRecord = null)
        {
            if (_approvalRepository == null)
                return false;

            try
            {
                // Get event record if not provided
                if (eventRecord == null)
                {
                    eventRecord = await _approvalRepository.GetEventByIdAsync(eventId, organisationId);
                }

                // Update Event table
                var eventUpdated = await _approvalRepository.UpdateEventStatusAsync(
                    eventId, 
                    EventStateConstants.Rejected, 
                    EventStateConstants.RejectedByManager, 
                    rejecterUserId, 
                    organisationId);
                
                if (!eventUpdated)
                {
                    _logger.LogWarning(LogMessages.Alert.FailedToUpdateEventStatus, eventId);
                }

                // Get Approvals for this event and update the approval status
                var approvals = await _approvalRepository.GetApprovalsByEventIdAsync(eventId, organisationId);
                var comment = string.Format(AlertWorkflowMessages.RejectedWithReason, reason);

                foreach (var approval in approvals.Where(a => a.ApprovalStatus == EventConstants.ApprovalStatusPending))
                {
                    await _approvalRepository.UpdateApprovalStatusAsync(
                        approval.Id, 
                        EventConstants.ApprovalStatusRejected, 
                        comment, 
                        rejecterUserId);
                }

                // Get notification template from database and replace parameters (including reason)
                var (title, message) = await GetNotificationTemplateAsync(
                    eventName,
                    false, // isApproval = false for rejection
                    eventRecord,
                    requesterUserId,
                    organisationId,
                    reason);

                // Insert ScreenNotification for the requester
                var notificationId = await _approvalRepository.InsertScreenNotificationAsync(
                    requesterUserId,
                    eventId,
                    title,
                    message,
                    organisationId,
                    rejecterUserId
                );
                
                _logger.LogInformation(AlertWorkflowMessages.NotificationCreatedForRejection,
                    notificationId, requesterUserId, eventId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorUpdatingEventAndApprovalStatusForRejection, eventId);
                return false;
            }
        }

        /// <summary>
        /// Unified method to reject a request from an alert (for mobile app)
        /// This will: 1) Reject the underlying request, 2) Update alert status, 3) Send notification to requester
        /// Supports: LeaveRequest, CancelLeave, PayrollSubmission, ResignationRequest, OvertimeRequest, ReimbursementRequest
        /// </summary>
        public async Task<AlertResponse> RejectRequestFromAlertAsync(RejectRequestFromAlertRequest request, int rejecterUserId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Alert.RejectingRequestFromAlert, request.AlertId);

                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.RejectionReasonRequired,
                        Data = null
                    };
                }

                // Get the alert to extract EventId and EventName
                var alert = await _repo.GetAlertByIdAsync(request.AlertId);
                if (alert == null)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertMessages.AlertNotFound,
                        Data = null
                    };
                }

                // EventId in the alert points to the Event table
                var eventId = request.EventId ?? alert.EventId;
                var eventName = request.EventName ?? alert.Title ?? EventNameConstants.Request;
                
                if (!eventId.HasValue)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertWorkflowMessages.EventIdNotFoundForRejection,
                        Data = null
                    };
                }

                // Get Event from Event table to extract EventData
                Event? eventRecord = null;

                if (_approvalRepository == null)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = AlertWorkflowMessages.ApprovalRepositoryNotAvailable,
                        Data = null
                    };
                }

                eventRecord = await _approvalRepository.GetEventByIdAsync(eventId.Value, alert.OrganisationId);
                if (eventRecord == null)
                {
                    return new AlertResponse
                    {
                        Success = false,
                        Message = string.Format(AlertWorkflowMessages.EventNotFound, eventId.Value),
                        Data = null
                    };
                }

                // Get EventType from EventTypes table to get the proper EventName
                EventType? eventType = null;
                if (eventRecord.EventTypeId > 0)
                {
                    eventType = await _approvalRepository.GetEventTypeByIdAsync(eventRecord.EventTypeId, alert.OrganisationId);
                    if (eventType != null && !string.IsNullOrEmpty(eventType.EventName))
                    {
                        eventName = eventType.EventName;
                    }
                }

                // Extract request ID from EventData based on event type
                var requestId = ExtractRequestIdFromEventData(eventRecord.EventData, eventName);
                
                // Route to appropriate service based on event type
                var eventNameLower = eventName.ToLower();
                bool requestRejected = false;

                // Handle LeaveRequest and CancelLeave with LeaveService
                if (eventNameLower.Contains(EventNameConstants.LeaveRequestLower) || 
                    eventNameLower.Contains(StringConstants.LeaveKeyword) || 
                    eventNameLower.Contains(EventNameConstants.CancelLeave.ToLower()))
                {
                    if (!requestId.HasValue)
                    {
                        return new AlertResponse
                        {
                            Success = false,
                            Message = AlertWorkflowMessages.LeaveRequestIdNotFound,
                            Data = null
                        };
                    }

                    if (_leaveService == null)
                    {
                        return new AlertResponse
                        {
                            Success = false,
                            Message = LeaveMessages.LeaveServiceNotAvailable,
                            Data = null
                        };
                    }

                    // Optionally: Verify the leave request exists before processing
                    if (_leaveRepository != null)
                    {
                        var leaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(requestId.Value);
                        if (leaveRequest == null)
                        {
                            return new AlertResponse
                            {
                                Success = false,
                                Message = string.Format(LeaveMessages.LeaveRequestNotFoundWithId, requestId.Value),
                                Data = null
                            };
                        }
                    }

                    // For CancelLeave rejection, we just reject the cancellation request
                    // The original leave remains in its current state (approved) and balance remains unchanged
                    // We don't call LeaveService because we don't want to change the original leave request status
                    if (eventNameLower.Contains(EventNameConstants.CancelLeave.ToLower()))
                    {
                        // Just mark the cancellation request as rejected
                        // The original leave request status and balance remain unchanged
                        _logger.LogInformation(StringConstants.LogRejectingCancelLeaveRequest);
                        requestRejected = true; // We'll update the event/approval status below
                    }
                    else
                    {
                        // Reject the leave request (normal leave rejection)
                        var leaveResult = await _leaveService.RejectLeaveRequestAsync(requestId.Value, rejecterUserId, request.Reason);
                        requestRejected = leaveResult.Success;
                        
                        if (!requestRejected)
                        {
                            return new AlertResponse
                            {
                                Success = false,
                                Message = leaveResult.Message ?? "Failed to reject leave request.",
                                Data = null
                            };
                        }
                    }
                }
                else
                {
                    // For other event types (PayrollSubmission, ResignationRequest, OvertimeRequest, ReimbursementRequest)
                    // Update event status only (business logic may be handled by web application)
                    _logger.LogInformation(StringConstants.LogHandlingEventRejectionUpdatingStatus, eventName);
                    requestRejected = true; // We'll update the event status regardless
                }

                // Update Event and Approval status, and send notification (common for all event types)
                await UpdateEventAndApprovalStatusForRejectionAsync(
                    eventId.Value,
                    eventName,
                    rejecterUserId,
                    alert.OrganisationId,
                    (int)alert.InsertUserId, // Requester's UserId
                    request.Reason,
                    eventRecord);

                // Update alert status to "Read" (for the rejecter's alert)
                var updateSuccess = await _repo.RejectAlertAsync(request.AlertId, rejecterUserId, request.Reason);
                if (!updateSuccess)
                {
                    _logger.LogWarning(AlertWorkflowMessages.FailedToUpdateAlertStatusAfterRejection);
                }

                // Return updated alert
                var updatedAlert = await _repo.GetAlertByIdAsync(request.AlertId);
                return new AlertResponse
                {
                    Success = true,
                    Message = AlertMessages.RequestRejectedSuccessfully,
                    Data = updatedAlert
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Alert.ErrorRejectingRequestFromAlert, request.AlertId);
                return new AlertResponse
                {
                    Success = false,
                    Message = string.Format(AlertWorkflowMessages.ErrorRejectingRequest, ex.Message),
                    Data = null
                };
            }
        }
    }
}
