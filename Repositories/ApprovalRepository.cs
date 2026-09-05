using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;
using MobileWebApi.Constants;
using System;

namespace MobileWebApi.Repositories
{
    public class ApprovalRepository : IApprovalRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<ApprovalRepository> _logger;
        private readonly QueryProvider _queryProvider;

        public ApprovalRepository(DapperContext context, ILogger<ApprovalRepository> logger, QueryProvider queryProvider)
        {
            _context = context;
            _logger = logger;
            _queryProvider = queryProvider;
        }

        #region Event Operations

        public async Task<int> InsertEventAsync(int userId, int eventTypeId, string eventData, string state, string status, int tenantId, int insertUserId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("InsertEvent");

                return await conn.ExecuteScalarAsync<int>(query, new
                {
                    UserId = userId,
                    EventTypeId = eventTypeId,
                    EventData = eventData,
                    State = state,
                    Status = status,
                    TenantId = tenantId,
                    InsertUserId = insertUserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(InsertEventAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalInsertEventDatabaseError}: Failed to insert event",
                    ex);
            }
        }

        public async Task<Event?> GetEventByIdAsync(int eventId, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetEventById");

                return await conn.QueryFirstOrDefaultAsync<Event>(query, new { Id = eventId, TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEventByIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalGetEventByIdDatabaseError}: Failed to fetch event by id",
                    ex);
            }
        }

        public async Task<bool> UpdateEventStatusAsync(int eventId, string state, string status, int updateUserId, int tenantId, string? eventData = null)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("UpdateEventStatus");

                var rowsAffected = await conn.ExecuteAsync(query, new
                {
                    EventId = eventId,
                    State = state,
                    Status = status,
                    UpdateUserId = updateUserId,
                    TenantId = tenantId,
                    EventData = eventData
                });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(UpdateEventStatusAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalUpdateEventStatusDatabaseError}: Failed to update event status",
                    ex);
            }
        }

        public async Task<int> GetEventTypeIdAsync(string eventName, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetEventTypeIdByName");

                return await conn.QueryFirstOrDefaultAsync<int>(query, new { EventName = eventName, TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEventTypeIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalGetEventTypeIdDatabaseError}: Failed to fetch event type id",
                    ex);
            }
        }

        public async Task<EventType?> GetEventTypeByIdAsync(int eventTypeId, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetEventTypeById");

                return await conn.QueryFirstOrDefaultAsync<EventType>(query, new { Id = eventTypeId, TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEventTypeByIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalGetEventTypeByIdDatabaseError}: Failed to fetch event type by id",
                    ex);
            }
        }

        public async Task<bool> IsEventTypeActiveAsync(int eventTypeId, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("IsEventTypeActive");

                return await conn.QueryFirstOrDefaultAsync<bool>(query, new { Id = eventTypeId, TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(IsEventTypeActiveAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalIsEventTypeActiveDatabaseError}: Failed to check event type active status",
                    ex);
            }
        }

        #endregion

        #region Approval Stage Operations

        public async Task<string?> GetFirstLevelNameAsync(int eventTypeId, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetFirstApprovalLevelName");

                return await conn.QueryFirstOrDefaultAsync<string>(query, new { EventTypeId = eventTypeId, TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetFirstLevelNameAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalGetFirstLevelNameDatabaseError}: Failed to fetch first approval level name",
                    ex);
            }
        }

        public async Task<ApprovalStage?> GetApprovalStageByLevelNameAsync(int eventTypeId, string levelName, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetApprovalStageByLevelName");

                return await conn.QueryFirstOrDefaultAsync<ApprovalStage>(query, new
                {
                    EventTypeId = eventTypeId,
                    LevelName = levelName,
                    TenantId = tenantId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetApprovalStageByLevelNameAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalGetApprovalStageByLevelNameDatabaseError}: Failed to fetch approval stage by level name",
                    ex);
            }
        }

        public async Task<bool> IsApprovalStageActiveAsync(int stageId, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("IsApprovalStageActive");

                return await conn.QueryFirstOrDefaultAsync<bool>(query, new { Id = stageId, TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(IsApprovalStageActiveAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalIsApprovalStageActiveDatabaseError}: Failed to check approval stage active status",
                    ex);
            }
        }

        #endregion

        #region Approver Operations

        public async Task<IEnumerable<ApproverInfo>> GetApproversForStageAsync(
            int stageId, int? workRoleId, string explicitUserIds, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();

                var approvers = new List<ApproverInfo>();

                // Get approvers by Work Role
                if (workRoleId.HasValue && workRoleId > 0)
                {
                    string roleQuery = _queryProvider.Get("GetApproversByWorkRole");

                    var roleApprovers = await conn.QueryAsync<ApproverInfo>(roleQuery, new
                    {
                        WorkRoleId = workRoleId,
                        TenantId = tenantId
                    });

                    approvers.AddRange(roleApprovers);
                }

                // Get explicit approvers from CSV list
                if (!string.IsNullOrWhiteSpace(explicitUserIds))
                {
                    var userIdList = explicitUserIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => int.Parse(id.Trim()))
                        .ToList();

                    string explicitQuery = _queryProvider.Get("GetApproversByUserIds");

                    var explicitApprovers = await conn.QueryAsync<ApproverInfo>(explicitQuery, new
                    {
                        UserIds = userIdList,
                        TenantId = tenantId
                    });

                    approvers.AddRange(explicitApprovers);
                }

                return approvers.DistinctBy(x => x.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetApproversForStageAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalGetApproversForStageDatabaseError}: Failed to fetch approvers for stage",
                    ex);
            }
        }

        public async Task<ApproverInfo?> GetSupervisorByEmployeeIdAsync(int employeeId, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetSupervisorByEmployeeId");

                return await conn.QueryFirstOrDefaultAsync<ApproverInfo>(
                    query,
                    new { EmployeeId = employeeId, TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetSupervisorByEmployeeIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalGetUserIdByEmployeeIdDatabaseError}: Failed to fetch supervisor by employee id",
                    ex);
            }
        }

        public async Task<int> GetUserIdByEmployeeIdAsync(int employeeId, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetUserIdByEmployeeId");

                return await conn.QueryFirstOrDefaultAsync<int>(query, new { EmployeeId = employeeId, TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetUserIdByEmployeeIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalGetUserIdByEmployeeIdDatabaseError}: Failed to fetch user id by employee id",
                    ex);
            }
        }

        public async Task<IEnumerable<string>> GetEmployeeNamesByUserIdsAsync(IEnumerable<int> userIds, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetEmployeeNamesByUserIds");

                return await conn.QueryAsync<string>(query, new { UserIds = userIds.ToList(), TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeeNamesByUserIdsAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalGetEmployeeNamesByUserIdsDatabaseError}: Failed to fetch employee names by user ids",
                    ex);
            }
        }

        #endregion

        #region Approval Operations

        public async Task<int> InsertApprovalAsync(int eventId, int stageId, int approverId, int insertUserId, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("InsertApproval");

                return await conn.ExecuteScalarAsync<int>(query, new
                {
                    EventId = eventId,
                    StageId = stageId,
                    ApproverId = approverId,
                    Action = EventConstants.ApprovalStatusPending,
                    TenantId = tenantId,
                    InsertUserId = insertUserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(InsertApprovalAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalInsertApprovalDatabaseError}: Failed to insert approval",
                    ex);
            }
        }

        public async Task<bool> UpdateApprovalStatusAsync(int approvalId, string status, string? comments, int updateUserId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("UpdateApprovalStatus");

                var rowsAffected = await conn.ExecuteAsync(query, new
                {
                    Id = approvalId,
                    Action = status,
                    Comments = comments,
                    ActionDate = DateTime.Now,
                    UpdateUserId = updateUserId
                });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(UpdateApprovalStatusAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalUpdateApprovalStatusDatabaseError}: Failed to update approval status",
                    ex);
            }
        }

        public async Task<IEnumerable<Approval>> GetApprovalsByEventIdAsync(int eventId, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetApprovalsByEventId");

                return await conn.QueryAsync<Approval>(query, new { EventId = eventId, TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetApprovalsByEventIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalGetApprovalsByEventIdDatabaseError}: Failed to fetch approvals by event id",
                    ex);
            }
        }

        #endregion

        #region Screen Notification Operations

        public async Task<int> InsertScreenNotificationAsync(int userId, int? eventId, string title, string message, int tenantId, int insertUserId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("InsertScreenNotification");

                return await conn.ExecuteScalarAsync<int>(query, new
                {
                    UserId = userId,
                    EventId = eventId,
                    Title = title,
                    Message = message,
                    IsRead = false,
                    IsActive = true,
                    TenantId = tenantId,
                    InsertUserId = insertUserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(InsertScreenNotificationAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalInsertScreenNotificationDatabaseError}: Failed to insert screen notification",
                    ex);
            }
        }

        public async Task<int> MarkScreenNotificationsReadByLeaveRequestIdAsync(int leaveRequestId, int tenantId, int updateUserId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("MarkScreenNotificationsReadByLeaveRequestId");

                return await conn.ExecuteAsync(query, new
                {
                    LeaveRequestId = leaveRequestId,
                    TenantId = tenantId,
                    UpdateUserId = updateUserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(MarkScreenNotificationsReadByLeaveRequestIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalInsertScreenNotificationDatabaseError}: Failed to mark screen notifications as read for withdrawn leave request",
                    ex);
            }
        }

        #endregion

        #region Email Operations

        public async Task<int> InsertEmailNotificationAsync(string toEmail, string subject, string body, int tenantId, int insertUserId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("InsertEmailNotification");

                return await conn.ExecuteScalarAsync<int>(query, new
                {
                    ToEmail = toEmail,
                    Subject = subject,
                    Body = body,
                    TenantId = tenantId,
                    InsertUserId = insertUserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(InsertEmailNotificationAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalInsertEmailNotificationDatabaseError}: Failed to insert email notification",
                    ex);
            }
        }

        public async Task<EmailTemplate?> GetEmailTemplateAsync(string eventName, string actionType, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetEmailTemplate");

                return await conn.QueryFirstOrDefaultAsync<EmailTemplate>(query, new
                {
                    EventName = eventName,
                    ActionType = actionType,
                    TenantId = tenantId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmailTemplateAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalGetEmailTemplateDatabaseError}: Failed to fetch email template",
                    ex);
            }
        }

        public async Task<NotificationTemplate?> GetNotificationTemplateAsync(string templateName, string templateType, string actionType, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetNotificationTemplate");

                return await conn.QueryFirstOrDefaultAsync<NotificationTemplate>(query, new
                {
                    TemplateName = templateName,
                    TemplateType = templateType,
                    ActionType = actionType,
                    TenantId = tenantId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetNotificationTemplateAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalGetNotificationTemplateDatabaseError}: Failed to fetch notification template",
                    ex);
            }
        }

        #endregion

        #region Tenant Operations

        public async Task<string?> GetTenantNameAsync(int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetTenantName");

                return await conn.QueryFirstOrDefaultAsync<string>(query, new { TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetTenantNameAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalGetTenantNameDatabaseError}: Failed to fetch tenant name",
                    ex);
            }
        }

        #endregion

        #region Employee Operations

        public async Task<Employee?> GetEmployeeByUserIdAsync(int userId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetEmployeeByUserId");

                return await conn.QueryFirstOrDefaultAsync<Employee>(query, new { SystemUserId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeeByUserIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.ApprovalGetEmployeeByUserIdDatabaseError}: Failed to fetch employee by user id",
                    ex);
            }
        }

        #endregion

        #region Event Details Extraction

        public async Task<EventDetails> GetEventDetailsAsync(int eventId, int tenantId)
        {
            var eventDetails = new EventDetails();

            try
            {
                using var conn = _context.CreateConnection();
                
                // Get EventData and EventName from Events table
                string eventQuery = _queryProvider.Get("GetEventDataAndName");

                var eventData = await conn.QueryFirstOrDefaultAsync<(string EventData, string EventName)>(
                    eventQuery, new { EventId = eventId, TenantId = tenantId });

                if (eventData.EventData == null)
                    return eventDetails;

                // Parse EventData JSON
                using var doc = System.Text.Json.JsonDocument.Parse(eventData.EventData);
                var root = doc.RootElement;
                var eventName = eventData.EventName;

                // Extract LeaveDates - only for leave events
                if (eventName.Contains(StringConstants.EventNameLeaveRequest, StringComparison.OrdinalIgnoreCase) || 
                    eventName.Contains(StringConstants.EventNameCancelLeave, StringComparison.OrdinalIgnoreCase) ||
                    eventName.Contains(StringConstants.LeaveKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    if (TryGetId(root, StringConstants.JsonKeyLeaveRequestId, StringConstants.JsonKeyLeaveRequestIdAlt, out int leaveRequestId))
                    {
                        string leaveQuery = _queryProvider.Get("GetLeaveRequestDates");

                        var leave = await conn.QueryFirstOrDefaultAsync<(DateTime FromDate, DateTime ToDate)>(
                            leaveQuery, new { LeaveRequestId = leaveRequestId, TenantId = tenantId });

                        if (leave != default)
                        {
                            eventDetails.LeaveDates = string.Format(StringConstants.LeaveDatesFormat, leave.FromDate.ToString(StringConstants.DateFormat), leave.ToDate.ToString(StringConstants.DateFormat));
                        }
                    }
                }

                // Extract OvertimeDates
                if (eventName.Contains(StringConstants.EventNameOvertimeRequest, StringComparison.OrdinalIgnoreCase))
                {
                    if (TryGetId(root, StringConstants.JsonKeyOvertimeId, StringConstants.JsonKeyOvertimeIdAlt, out int overtimeId))
                    {
                        string overtimeQuery = _queryProvider.Get("GetOvertimeDetails");

                        var overtime = await conn.QueryFirstOrDefaultAsync<(DateTime OvertimeDate, int Duration)>(
                            overtimeQuery, new { OvertimeId = overtimeId, TenantId = tenantId });

                        if (overtime != default)
                        {
                            eventDetails.OvertimeDates = string.Format(StringConstants.OvertimeDatesFormat, overtime.OvertimeDate.ToString(StringConstants.DateFormat), overtime.Duration);
                        }
                    }
                }

                // Extract ReimbursementDates
                if (eventName.Contains(StringConstants.EventNameReimbursementRequest, StringComparison.OrdinalIgnoreCase))
                {
                    if (TryGetId(root, StringConstants.JsonKeyReimbursementId, StringConstants.JsonKeyReimbursementIdAlt, out int reimbursementId))
                    {
                        string reimbursementQuery = _queryProvider.Get("GetReimbursementDetails");

                        var reimbursement = await conn.QueryFirstOrDefaultAsync<(DateTime TransactionDate, double TotalAmount)>(
                            reimbursementQuery, new { ReimbursementId = reimbursementId, TenantId = tenantId });

                        if (reimbursement != default)
                        {
                            eventDetails.ReimbursementDates = string.Format(StringConstants.ReimbursementDatesFormat, reimbursement.TransactionDate.ToString(StringConstants.DateFormat), reimbursement.TotalAmount);
                        }
                    }
                }

                // Extract ResignationDates
                if (eventName.Contains(StringConstants.EventNameResignationRequest, StringComparison.OrdinalIgnoreCase))
                {
                    if (TryGetId(root, StringConstants.JsonKeyResignationId, StringConstants.JsonKeyResignationIdAlt, out int resignationId))
                    {
                        string resignationQuery = _queryProvider.Get("GetResignationDetails");

                        var resignation = await conn.QueryFirstOrDefaultAsync<(DateTime ResignationDate, string Number)>(
                            resignationQuery, new { ResignationId = resignationId, TenantId = tenantId });

                        if (resignation != default)
                        {
                            eventDetails.ResignationDates = string.Format(StringConstants.ResignationDatesFormat, resignation.ResignationDate.ToString(StringConstants.DateFormat), resignation.Number);
                        }
                    }
                }

                // Extract PayrollMonthYear
                if (eventName.Contains(StringConstants.EventNamePayrollSubmission, StringComparison.OrdinalIgnoreCase))
                {
                    if (TryGetId(root, StringConstants.JsonKeyPayrollId, StringConstants.JsonKeyPayrollIdAlt, out int payrollId))
                    {
                        string payrollQuery = _queryProvider.Get("GetPayrollMonthYear");

                        var payroll = await conn.QueryFirstOrDefaultAsync<(int Month, int Year)>(
                            payrollQuery, new { PayrollId = payrollId, TenantId = tenantId });

                        if (payroll != default)
                        {
                            var monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(payroll.Month);
                            eventDetails.PayrollMonthYear = string.Format(StringConstants.PayrollMonthYearFormat, monthName, payroll.Year);
                        }
                    }
                }

                // Extract RegularizationDetails (aligned with Web EventRepository.ExtractEventDetails).
                // Prefer EventData punch/date fields when present so approve/reject does not require
                // an extra EmployeeDispute lookup; fall back to the dispute table when needed.
                if (eventName.Contains(StringConstants.EventNameRegularizationRequest, StringComparison.OrdinalIgnoreCase))
                {
                    var fromEventData = Helper.NotificationTokenHelper.BuildRegularizationDetailsFromEventData(root);
                    if (!string.IsNullOrEmpty(fromEventData))
                    {
                        eventDetails.RegularizationDetails = fromEventData;
                    }

                    if (root.TryGetProperty(StringConstants.JsonKeyDisputeDate, out var ddEl)
                        || root.TryGetProperty("disputeDate", out ddEl))
                    {
                        var ddStr = ddEl.ValueKind == System.Text.Json.JsonValueKind.String ? ddEl.GetString() : ddEl.ToString();
                        if (DateTime.TryParse(ddStr, System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var parsedDd))
                        {
                            eventDetails.DisputeDate = parsedDd.ToString(StringConstants.DateFormat);
                        }
                    }

                    if (string.IsNullOrEmpty(eventDetails.RegularizationDetails)
                        && (TryGetId(root, StringConstants.JsonKeyDisputeId, StringConstants.JsonKeyDisputeIdAlt, out int employeeDisputeId)
                            || TryGetId(root, StringConstants.JsonKeyEmployeeDisputeId, StringConstants.JsonKeyEmployeeDisputeIdAlt, out employeeDisputeId)))
                    {
                        string disputeQuery = _queryProvider.Get("GetEmployeeDisputeNotificationDetails");

                        var dispute = await conn.QueryFirstOrDefaultAsync<(
                            DateTime DisputeDate,
                            DateTime? RequestedPunchInTime,
                            DateTime? RequestedPunchOutTime,
                            string? Description,
                            int DisputeCategoryId,
                            string? CategoryName)>(
                            disputeQuery, new { EmployeeDisputeId = employeeDisputeId, TenantId = tenantId });

                        if (dispute.DisputeDate != default)
                        {
                            eventDetails.RegularizationDetails = Helper.NotificationTokenHelper.BuildRegularizationDetails(
                                dispute.DisputeDate, dispute.RequestedPunchInTime, dispute.RequestedPunchOutTime);
                            eventDetails.DisputeDate = dispute.DisputeDate.ToString(StringConstants.DateFormat);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, LogMessages.ApprovalWorkflow.ErrorExtractingEventDetails, eventId);
            }

            return eventDetails;
        }

        private bool TryGetId(System.Text.Json.JsonElement root, string primaryKey, string secondaryKey, out int id)
        {
            id = 0;
            
            if (root.TryGetProperty(primaryKey, out var primaryElement))
            {
                if (primaryElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    id = primaryElement.GetInt32();
                    return id > 0;
                }
                else if (primaryElement.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    if (int.TryParse(primaryElement.GetString(), out int parsedId))
                    {
                        id = parsedId;
                        return id > 0;
                    }
                }
            }

            if (root.TryGetProperty(secondaryKey, out var secondaryElement))
            {
                if (secondaryElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    id = secondaryElement.GetInt32();
                    return id > 0;
                }
                else if (secondaryElement.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    if (int.TryParse(secondaryElement.GetString(), out int parsedId))
                    {
                        id = parsedId;
                        return id > 0;
                    }
                }
            }

            return false;
        }
        
        #endregion

        #region Payroll operations

        public async Task UpdatePayrollApprovalStatusAsync(int payrollId, bool isApproved, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("UpdatePayrollApprovalStatus");

                await conn.ExecuteAsync(query, new
                {
                    IsApproved = isApproved,
                    PayrollId = payrollId,
                    TenantId = tenantId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.ApprovalWorkflow.ErrorUpdatingPayrollApprovalStatus, payrollId, tenantId);
                throw;
            }
        }

        #endregion
    }
}

