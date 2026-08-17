using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;
using MobileWebApi.Constants;

namespace MobileWebApi.Repositories
{
    public class AlertRepository : IAlertRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<AlertRepository> _logger;
        private readonly QueryProvider _queryProvider;

        public AlertRepository(DapperContext context, ILogger<AlertRepository> logger, QueryProvider queryProvider)
        {
            _context = context;
            _logger = logger;
            _queryProvider = queryProvider;
        }

        public async Task<Alert?> GetAlertByIdAsync(int id)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetAlertById");
                return await conn.QueryFirstOrDefaultAsync<Alert>(query, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetAlertByIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AlertGetAlertByIdDatabaseError}: Failed to fetch alert by id",
                    ex);
            }
        }

        public async Task<IEnumerable<Alert>> GetAlertsByUserIdAsync(int userId, bool? isRead = null, bool? isActive = null)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetAlertsByUserId");
                return await conn.QueryAsync<Alert>(query, new { UserId = userId, IsRead = isRead, IsActive = isActive });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetAlertsByUserIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AlertGetAlertsByUserIdDatabaseError}: Failed to fetch alerts by user id",
                    ex);
            }
        }

        public async Task<IEnumerable<Alert>> GetAlertsByOrganisationIdAsync(int organisationId, bool? isRead = null, bool? isActive = null)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetAlertsByOrganisationId");
                return await conn.QueryAsync<Alert>(query, new { OrganisationId = organisationId, IsRead = isRead, IsActive = isActive });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetAlertsByOrganisationIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AlertGetAlertsByOrganisationIdDatabaseError}: Failed to fetch alerts by organisation id",
                    ex);
            }
        }

        public async Task<IEnumerable<Alert>> GetAlertsAsync(GetAlertsRequest request)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetAlerts");
                return await conn.QueryAsync<Alert>(query, new
                {
                    OrganisationId = request.organization,
                    UserId = request.UserId,
                    IsRead = request.IsRead,
                    IsActive = request.IsActive,
                    Status = request.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetAlertsAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AlertGetAlertsDatabaseError}: Failed to fetch alerts",
                    ex);
            }
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetUnreadAlertCount");
                return await conn.ExecuteScalarAsync<int>(query, new { UserId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetUnreadCountAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AlertGetUnreadCountDatabaseError}: Failed to fetch unread alert count",
                    ex);
            }
        }

        public async Task<int> CreateAlertAsync(CreateAlertRequest request)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("CreateAlert");
                return await conn.ExecuteScalarAsync<int>(query, new
                {
                    OrganisationId = request.organization,
                    UserId = request.UserId,
                    EventId = request.EventId,
                    Title = request.Title,
                    Message = request.Message,
                    Status = request.Status,
                    InsertUserId = request.InsertUserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(CreateAlertAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AlertCreateAlertDatabaseError}: Failed to create alert",
                    ex);
            }
        }

        public async Task<bool> UpdateAlertAsync(UpdateAlertRequest request)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("UpdateAlert");
                var rowsAffected = await conn.ExecuteAsync(query, new
                {
                    Id = request.Id,
                    Title = request.Title,
                    Message = request.Message,
                    IsRead = request.IsRead,
                    IsActive = request.IsActive,
                    Status = request.Status,
                    UpdateUserId = request.UpdateUserId
                });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(UpdateAlertAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AlertUpdateAlertDatabaseError}: Failed to update alert",
                    ex);
            }
        }

        public async Task<bool> MarkAsReadAsync(int id, int? updateUserId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("MarkAlertAsRead");
                var rowsAffected = await conn.ExecuteAsync(query, new { Id = id, UpdateUserId = updateUserId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(MarkAsReadAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AlertMarkAsReadDatabaseError}: Failed to mark alert as read",
                    ex);
            }
        }

        public async Task<bool> MarkAllAsReadAsync(int userId, int? updateUserId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("MarkAllAlertsAsRead");
                var rowsAffected = await conn.ExecuteAsync(query, new { UserId = userId, UpdateUserId = updateUserId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(MarkAllAsReadAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AlertMarkAllAsReadDatabaseError}: Failed to mark all alerts as read",
                    ex);
            }
        }

        public async Task<bool> DeleteAlertAsync(int id)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("DeleteAlert");
                var rowsAffected = await conn.ExecuteAsync(query, new { Id = id });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(DeleteAlertAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AlertDeleteAlertDatabaseError}: Failed to delete alert",
                    ex);
            }
        }

        public async Task<bool> DeactivateAlertAsync(int id, int? updateUserId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("DeactivateAlert");
                var rowsAffected = await conn.ExecuteAsync(query, new { Id = id, UpdateUserId = updateUserId });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(DeactivateAlertAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AlertDeactivateAlertDatabaseError}: Failed to deactivate alert",
                    ex);
            }
        }

        public async Task<bool> ApproveAlertAsync(int id, int? updateUserId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("UpdateAlert");
                // Status can only be "Unread" or "Read" based on CHECK constraint
                // Mark as "Read" when approved
                var rowsAffected = await conn.ExecuteAsync(query, new
                {
                    Id = id,
                    Title = (string?)null,
                    Message = (string?)null,
                    IsRead = true, // Mark as read
                    IsActive = (bool?)null,
                    Status = NotificationStatusConstants.Read, // Use "Read" instead of "Approved" to satisfy CHECK constraint
                    UpdateUserId = updateUserId
                });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(ApproveAlertAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AlertApproveAlertDatabaseError}: Failed to approve alert",
                    ex);
            }
        }

        public async Task<bool> RejectAlertAsync(int id, int? updateUserId, string? reason)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("UpdateAlert");

                // If reason is provided, append it to the message
                string? updatedMessage = null;
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    var existingAlert = await GetAlertByIdAsync(id);
                    if (existingAlert != null)
                    {
                        updatedMessage = string.IsNullOrWhiteSpace(existingAlert.Message)
                            ? $"Rejected: {reason}"
                            : $"{existingAlert.Message}\nRejected: {reason}";
                    }
                }

                // Status can only be "Unread" or "Read" based on CHECK constraint
                // Mark as "Read" when rejected
                var rowsAffected = await conn.ExecuteAsync(query, new
                {
                    Id = id,
                    Title = (string?)null,
                    Message = updatedMessage,
                    IsRead = true, // Mark as read
                    IsActive = (bool?)null,
                    Status = NotificationStatusConstants.Read, // Use "Read" instead of "Rejected" to satisfy CHECK constraint
                    UpdateUserId = updateUserId
                });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(RejectAlertAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.AlertRejectAlertDatabaseError}: Failed to reject alert",
                    ex);
            }
        }
    }
}

