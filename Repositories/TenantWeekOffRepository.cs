using Dapper;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
    public class TenantWeekOffRepository : ITenantWeekOffRepository
    {
        private static readonly List<DayOfWeek> DefaultWeeklyOffDays = new()
        {
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        };

        private readonly DapperContext _dapperContext;
        private readonly QueryProvider _queryProvider;
        private readonly ILogger<TenantWeekOffRepository> _logger;

        public TenantWeekOffRepository(
            DapperContext dapperContext,
            QueryProvider queryProvider,
            ILogger<TenantWeekOffRepository> logger)
        {
            _dapperContext = dapperContext;
            _queryProvider = queryProvider;
            _logger = logger;
        }

        public async Task<int?> GetTenantConfigurationIdByTenantIdAsync(int tenantId)
        {
            try
            {
                using var connection = _dapperContext.CreateConnection();
                var query = _queryProvider.Get("GetTenantConfigurationIdByTenantId");

                return await connection.QueryFirstOrDefaultAsync<int?>(query, new { TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Attendance.ErrorFetchingTenantWeekOffDays, tenantId);
                throw new Exception(
                    $"{ExceptionCodes.Repository.TenantWeekOffGetConfigurationDatabaseError}: Failed to fetch tenant configuration",
                    ex);
            }
        }

        public async Task<List<TenantWeekOffDayDto>> GetWeekOffDaysByTenantIdAsync(int tenantId)
        {
            try
            {
                using var connection = _dapperContext.CreateConnection();
                var query = _queryProvider.Get("GetTenantWeekOffDaysWithNamesByTenantId");

                var result = await connection.QueryAsync<TenantWeekOffDayDto>(query, new { TenantId = tenantId });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Attendance.ErrorFetchingTenantWeekOffDays, tenantId);
                throw new Exception(
                    $"{ExceptionCodes.Repository.TenantWeekOffGetWeekOffDaysDatabaseError}: Failed to fetch tenant week-off days",
                    ex);
            }
        }

        public async Task<List<DayOfWeek>> GetTenantWeeklyOffDaysAsync(int tenantId)
        {
            try
            {
                using var connection = _dapperContext.CreateConnection();

                var tenantConfigurationId = await connection.QueryFirstOrDefaultAsync<int?>(
                    _queryProvider.Get("GetTenantConfigurationIdByTenantId"),
                    new { TenantId = tenantId });

                if (!tenantConfigurationId.HasValue)
                {
                    return new List<DayOfWeek>(DefaultWeeklyOffDays);
                }

                var dayOffIds = (await connection.QueryAsync<int>(
                    _queryProvider.Get("GetTenantDayOffsByTenantId"),
                    new { TenantId = tenantId }))
                    .Distinct()
                    .ToList();

                if (!dayOffIds.Any())
                {
                    return new List<DayOfWeek>(DefaultWeeklyOffDays);
                }

                // Prefer day names from Days table when available (Sunday, Monday, etc.)
                var configuredDayNames = (await connection.QueryAsync<string>(
                    _queryProvider.Get("GetTenantWeekOffDayNamesByTenantId"),
                    new { TenantId = tenantId }))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (configuredDayNames.Any())
                {
                    var weeklyOffDaysFromNames = configuredDayNames
                        .Where(x => Enum.TryParse<DayOfWeek>(x, true, out _))
                        .Select(x => Enum.Parse<DayOfWeek>(x, true))
                        .Distinct()
                        .ToList();

                    if (weeklyOffDaysFromNames.Any())
                    {
                        return weeklyOffDaysFromNames;
                    }
                }

                // DayOffId matches Days.Id and DayOfWeek enum (0=Sunday .. 6=Saturday)
                var weeklyOffDaysFromIds = dayOffIds
                    .Select(id => (DayOfWeek)PayrollHelper.NormalizeDayOfWeek(id))
                    .Distinct()
                    .ToList();

                return weeklyOffDaysFromIds.Any()
                    ? weeklyOffDaysFromIds
                    : new List<DayOfWeek>(DefaultWeeklyOffDays);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, LogMessages.Attendance.ErrorFetchingTenantWeekOffDays, tenantId);
                return new List<DayOfWeek>(DefaultWeeklyOffDays);
            }
        }
    }
}
