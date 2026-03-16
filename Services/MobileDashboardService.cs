using System.Data;
using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;
using MobileWebApi.Constants;

namespace MobileWebApi.Services
{
    public class MobileDashboardService : IMobileDashboardService
    {
        private readonly DapperContext _context;
        private readonly QueryProvider _queries;
        private readonly ILogger<MobileDashboardService> _logger;

        public MobileDashboardService(
            DapperContext context,
            QueryProvider queries,
            ILogger<MobileDashboardService> logger)
        {
            _context = context;
            _queries = queries;
            _logger = logger;
        }

        public async Task<IReadOnlyList<TrainingDto>> GetLatestTrainingsAsync(int tenantId)
        {
            try
            {
                using IDbConnection conn = _context.CreateConnection();
                var sql = _queries.Get("MobileDashboard_GetLatestTrainings");
                var result = await conn.QueryAsync<TrainingDto>(sql, new { TenantId = tenantId });

                var today = DateTime.Today;
                return result
                    .Where(t => t.EndDate.Date >= today)
                    .OrderBy(t => t.StartDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.MobileDashboard.GetLatestTrainings, nameof(GetLatestTrainingsAsync), ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<AnnouncementDto>> GetLatestAnnouncementsAsync(int tenantId)
        {
            try
            {
                using IDbConnection conn = _context.CreateConnection();
                var sql = _queries.Get("MobileDashboard_GetLatestAnnouncements");
                var result = await conn.QueryAsync<AnnouncementDto>(sql, new { TenantId = tenantId });

                var today = DateTime.Today;
                return result
                    .Where(a => a.Date.Date >= today)
                    .OrderBy(a => a.Date)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.MobileDashboard.GetLatestAnnouncements, nameof(GetLatestAnnouncementsAsync), ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<EventDto>> GetLatestEventsAsync(int tenantId)
        {
            try
            {
                using IDbConnection conn = _context.CreateConnection();
                var sql = _queries.Get("MobileDashboard_GetLatestEvents");
                var result = await conn.QueryAsync<EventDto>(sql, new { TenantId = tenantId });

                var today = DateTime.Today;
                return result
                    .Where(e => e.EndDate.Date >= today)
                    .OrderBy(e => e.StartDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.MobileDashboard.GetLatestEvents, nameof(GetLatestEventsAsync), ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<HolidayDto>> GetLatestHolidaysAsync(int tenantId)
        {
            try
            {
                using IDbConnection conn = _context.CreateConnection();
                var sql = _queries.Get("MobileDashboard_GetLatestHolidays");
                var result = await conn.QueryAsync<HolidayDto>(sql, new { TenantId = tenantId });

                var today = DateTime.Today;
                return result
                    .Where(h => h.Date.Date >= today)
                    .OrderBy(h => h.Date)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.MobileDashboard.GetLatestHolidays, nameof(GetLatestHolidaysAsync), ex);
                throw;
            }
        }
    }
}

