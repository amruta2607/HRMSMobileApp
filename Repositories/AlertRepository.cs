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
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetAlertById");
            return await conn.QueryFirstOrDefaultAsync<Alert>(query, new { Id = id });
        }

        public async Task<IEnumerable<Alert>> GetAlertsByUserIdAsync(int userId, bool? isRead = null, bool? isActive = null)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetAlertsByUserId");
            return await conn.QueryAsync<Alert>(query, new { UserId = userId, IsRead = isRead, IsActive = isActive });
        }

        public async Task<IEnumerable<Alert>> GetAlertsByOrganisationIdAsync(int organisationId, bool? isRead = null, bool? isActive = null)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetAlertsByOrganisationId");
            return await conn.QueryAsync<Alert>(query, new { OrganisationId = organisationId, IsRead = isRead, IsActive = isActive });
        }

        public async Task<IEnumerable<Alert>> GetAlertsAsync(GetAlertsRequest request)
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

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetUnreadAlertCount");
            return await conn.ExecuteScalarAsync<int>(query, new { UserId = userId });
        }

        public async Task<int> CreateAlertAsync(CreateAlertRequest request)
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

        public async Task<bool> UpdateAlertAsync(UpdateAlertRequest request)
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

        public async Task<bool> MarkAsReadAsync(int id, int? updateUserId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("MarkAlertAsRead");
            var rowsAffected = await conn.ExecuteAsync(query, new { Id = id, UpdateUserId = updateUserId });
            return rowsAffected > 0;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId, int? updateUserId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("MarkAllAlertsAsRead");
            var rowsAffected = await conn.ExecuteAsync(query, new { UserId = userId, UpdateUserId = updateUserId });
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAlertAsync(int id)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("DeleteAlert");
            var rowsAffected = await conn.ExecuteAsync(query, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> DeactivateAlertAsync(int id, int? updateUserId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("DeactivateAlert");
            var rowsAffected = await conn.ExecuteAsync(query, new { Id = id, UpdateUserId = updateUserId });
            return rowsAffected > 0;
        }

        public async Task<bool> ApproveAlertAsync(int id, int? updateUserId)
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

        public async Task<bool> RejectAlertAsync(int id, int? updateUserId, string? reason)
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
    }
}

