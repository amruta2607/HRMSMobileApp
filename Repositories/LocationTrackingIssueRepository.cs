using Dapper;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
    public class LocationTrackingIssueRepository : ILocationTrackingIssueRepository
    {
        private readonly DapperContext _context;
        private readonly QueryProvider _queryProvider;
        private readonly ILogger<LocationTrackingIssueRepository> _logger;

        public LocationTrackingIssueRepository(
            DapperContext context,
            QueryProvider queryProvider,
            ILogger<LocationTrackingIssueRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _queryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> InsertAsync(LocationTrackingIssue issue)
        {
            try
            {
                var query = _queryProvider.Get("InsertLocationTrackingIssue");

                using var connection = _context.CreateConnection();
                return await connection.ExecuteScalarAsync<int>(query, new
                {
                    UserId = issue.UserId,
                    TenantId = issue.TenantId,
                    IssueType = issue.IssueType,
                    IssueDescription = issue.IssueDescription,
                    Timestamp = issue.Timestamp,
                    LastKnownLatitude = issue.LastKnownLatitude,
                    LastKnownLongitude = issue.LastKnownLongitude,
                    DeviceId = issue.DeviceId,
                    InsertUserId = issue.InsertUserId,
                    InsertDate = issue.InsertDate,
                    UpdateUserId = issue.UpdateUserId,
                    UpdateDate = issue.UpdateDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    LogMessages.LocationTrackingIssue.FailedToInsert,
                    issue.UserId,
                    issue.TenantId);
                throw;
            }
        }
    }
}
