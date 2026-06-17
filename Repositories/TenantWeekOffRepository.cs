using Microsoft.EntityFrameworkCore;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;

namespace MobileWebApi.Repositories
{
    public class TenantWeekOffRepository : ITenantWeekOffRepository
    {
        private static readonly List<DayOfWeek> DefaultWeeklyOffDays = new()
        {
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        };

        private readonly HrmsDbContext _context;
        private readonly ILogger<TenantWeekOffRepository> _logger;

        public TenantWeekOffRepository(HrmsDbContext context, ILogger<TenantWeekOffRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int?> GetTenantConfigurationIdByTenantIdAsync(int tenantId)
        {
            try
            {
                return await _context.TenantConfigurations
                    .AsNoTracking()
                    .Where(tc => tc.TenantId == tenantId)
                    .Select(tc => (int?)tc.TenantConfigurationId)
                    .FirstOrDefaultAsync();
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
                return await (
                    from tcd in _context.TenantConfiguredDayOffDays
                    join tc in _context.TenantConfigurations
                        on tcd.TenantConfigurationId equals tc.TenantConfigurationId
                    join d in _context.Days
                        on tcd.DayOffId equals d.Id
                    where tc.TenantId == tenantId
                    orderby d.Id
                    select new TenantWeekOffDayDto
                    {
                        Id = d.Id,
                        Day = d.DayName
                    }
                ).AsNoTracking().ToListAsync();
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
                var tenantConfig = await _context.TenantConfigurations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId);

                if (tenantConfig == null)
                {
                    return new List<DayOfWeek>(DefaultWeeklyOffDays);
                }

                var configuredDays = await (
                    from tcd in _context.TenantConfiguredDayOffDays
                    join d in _context.Days
                        on tcd.DayOffId equals d.Id
                    where tcd.TenantConfigurationId == tenantConfig.TenantConfigurationId
                    select d.DayName
                ).AsNoTracking().ToListAsync();

                if (!configuredDays.Any())
                {
                    return new List<DayOfWeek>(DefaultWeeklyOffDays);
                }

                var weeklyOffDays = configuredDays
                    .Where(x => Enum.TryParse<DayOfWeek>(x, true, out _))
                    .Select(x => Enum.Parse<DayOfWeek>(x, true))
                    .Distinct()
                    .ToList();

                if (!weeklyOffDays.Any())
                {
                    return new List<DayOfWeek>(DefaultWeeklyOffDays);
                }

                return weeklyOffDays;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Attendance.ErrorFetchingTenantWeekOffDays, tenantId);
                throw new Exception(
                    $"{ExceptionCodes.Repository.TenantWeekOffGetWeekOffDaysDatabaseError}: Failed to fetch tenant weekly off days",
                    ex);
            }
        }
    }
}
