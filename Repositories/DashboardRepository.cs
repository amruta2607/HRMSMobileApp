using Dapper;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DapperContext _context;
        private readonly QueryProvider _queries;
        private readonly ILogger<DashboardRepository> _logger;

        public DashboardRepository(
            DapperContext context,
            QueryProvider queries,
            ILogger<DashboardRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<DashboardBirthdayDto>> GetUpcomingBirthdaysAsync(int tenantId, DateTime today, DateTime endDate)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var sql = _queries.Get("Dashboard_GetUpcomingBirthdays");
                var result = await connection.QueryAsync<DashboardBirthdayDto>(
                    sql,
                    new { TenantId = tenantId, Today = today.Date, EndDate = endDate.Date });

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetUpcomingBirthdaysAsync));
                throw new Exception($"{ExceptionCodes.Dashboard.GetBirthdays}: Failed to fetch upcoming birthdays", ex);
            }
        }

        public async Task<IReadOnlyList<DashboardWorkAnniversaryDto>> GetUpcomingWorkAnniversariesAsync(int tenantId, DateTime today, DateTime endDate)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var sql = _queries.Get("Dashboard_GetUpcomingWorkAnniversaries");
                var result = await connection.QueryAsync<DashboardWorkAnniversaryDto>(
                    sql,
                    new { TenantId = tenantId, Today = today.Date, EndDate = endDate.Date });

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetUpcomingWorkAnniversariesAsync));
                throw new Exception($"{ExceptionCodes.Dashboard.GetWorkAnniversaries}: Failed to fetch upcoming work anniversaries", ex);
            }
        }

        public async Task<IReadOnlyList<DashboardAwardDto>> GetUpcomingAwardsAsync(int tenantId, DateTime today, DateTime endDate)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var sql = _queries.Get("Dashboard_GetUpcomingAwards");
                var result = await connection.QueryAsync<DashboardAwardDto>(
                    sql,
                    new { TenantId = tenantId, Today = today.Date, EndDate = endDate.Date });

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetUpcomingAwardsAsync));
                throw new Exception($"{ExceptionCodes.Dashboard.GetAwards}: Failed to fetch upcoming awards", ex);
            }
        }
    }
}
