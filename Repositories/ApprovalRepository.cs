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

        public async Task<Event?> GetEventByIdAsync(int eventId, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetEventById");

            return await conn.QueryFirstOrDefaultAsync<Event>(query, new { Id = eventId, TenantId = tenantId });
        }

        public async Task<bool> UpdateEventStatusAsync(int eventId, string state, string status, int updateUserId, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("UpdateEventStatus");

            var rowsAffected = await conn.ExecuteAsync(query, new
            {
                EventId = eventId,
                State = state,
                Status = status,
                UpdateUserId = updateUserId,
                TenantId = tenantId
            });

            return rowsAffected > 0;
        }

        public async Task<int> GetEventTypeIdAsync(string eventName, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetEventTypeIdByName");

            return await conn.QueryFirstOrDefaultAsync<int>(query, new { EventName = eventName, TenantId = tenantId });
        }

        public async Task<EventType?> GetEventTypeByIdAsync(int eventTypeId, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetEventTypeById");

            return await conn.QueryFirstOrDefaultAsync<EventType>(query, new { Id = eventTypeId, TenantId = tenantId });
        }

        public async Task<bool> IsEventTypeActiveAsync(int eventTypeId, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("IsEventTypeActive");

            return await conn.QueryFirstOrDefaultAsync<bool>(query, new { Id = eventTypeId, TenantId = tenantId });
        }

        #endregion

        #region Approval Stage Operations

        public async Task<string?> GetFirstLevelNameAsync(int eventTypeId, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetFirstApprovalLevelName");

            return await conn.QueryFirstOrDefaultAsync<string>(query, new { EventTypeId = eventTypeId, TenantId = tenantId });
        }

        public async Task<ApprovalStage?> GetApprovalStageByLevelNameAsync(int eventTypeId, string levelName, int tenantId)
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

        public async Task<bool> IsApprovalStageActiveAsync(int stageId, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("IsApprovalStageActive");

            return await conn.QueryFirstOrDefaultAsync<bool>(query, new { Id = stageId, TenantId = tenantId });
        }

        #endregion

        #region Approver Operations

        public async Task<IEnumerable<ApproverInfo>> GetApproversForStageAsync(
        int stageId, int? workRoleId, string explicitUserIds, int tenantId)
        {
            using var conn = _context.CreateConnection();

            var approvers = new List<ApproverInfo>();

            // 1️⃣ Get approvers by Work Role
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

            // 2️⃣ Get explicit approvers from CSV list
            if (!string.IsNullOrWhiteSpace(explicitUserIds))
            {
                // Convert "229,230,231" → List<int> {229,230,231}
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

            // 3️⃣ Remove duplicates by UserId
            return approvers.DistinctBy(x => x.UserId);
        }

        public async Task<int> GetUserIdByEmployeeIdAsync(int employeeId, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetUserIdByEmployeeId");

            return await conn.QueryFirstOrDefaultAsync<int>(query, new { EmployeeId = employeeId, TenantId = tenantId });
        }

        public async Task<IEnumerable<string>> GetEmployeeNamesByUserIdsAsync(IEnumerable<int> userIds, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetEmployeeNamesByUserIds");

            return await conn.QueryAsync<string>(query, new { UserIds = userIds.ToList(), TenantId = tenantId });
        }

        #endregion

        #region Approval Operations

        public async Task<int> InsertApprovalAsync(int eventId, int stageId, int approverId, int insertUserId, int tenantId)
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

        public async Task<bool> UpdateApprovalStatusAsync(int approvalId, string status, string? comments, int updateUserId)
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

        public async Task<IEnumerable<Approval>> GetApprovalsByEventIdAsync(int eventId, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetApprovalsByEventId");

            return await conn.QueryAsync<Approval>(query, new { EventId = eventId, TenantId = tenantId });
        }

        #endregion

        #region Screen Notification Operations

        public async Task<int> InsertScreenNotificationAsync(int userId, int? eventId, string title, string message, int tenantId, int insertUserId)
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

        #endregion

        #region Email Operations

        public async Task<int> InsertEmailNotificationAsync(string toEmail, string subject, string body, int tenantId, int insertUserId)
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

        public async Task<EmailTemplate?> GetEmailTemplateAsync(string eventName, string actionType, int tenantId)
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

        public async Task<NotificationTemplate?> GetNotificationTemplateAsync(string templateName, string templateType, string actionType, int tenantId)
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

        #endregion

        #region Tenant Operations

        public async Task<string?> GetTenantNameAsync(int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetTenantName");

            return await conn.QueryFirstOrDefaultAsync<string>(query, new { TenantId = tenantId });
        }

        #endregion

        #region Employee Operations

        public async Task<Employee?> GetEmployeeByUserIdAsync(int userId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetEmployeeByUserId");

            return await conn.QueryFirstOrDefaultAsync<Employee>(query, new { SystemUserId = userId });
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

