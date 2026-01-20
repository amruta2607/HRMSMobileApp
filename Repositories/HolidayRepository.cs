using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
    public class HolidayRepository : IHolidayRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<HolidayRepository> _logger;
        private readonly QueryProvider _queryProvider;

        public HolidayRepository(DapperContext context, ILogger<HolidayRepository> logger, QueryProvider queryProvider)
        {
            _context = context;
            _logger = logger;
            _queryProvider = queryProvider;
        }

        /// <summary>
        /// Create a new holiday
        /// </summary>
        public async Task<int> CreateHolidayAsync(Holiday holiday)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("CreateHoliday");

            return await conn.ExecuteScalarAsync<int>(query, new
            {
                holiday.Name,
                holiday.Date,
                holiday.Description,
                holiday.TenantId,
                holiday.InsertUserId
            });
        }

        /// <summary>
        /// Get holiday by ID
        /// </summary>
        public async Task<Holiday?> GetHolidayByIdAsync(int id, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetHolidayById");

            return await conn.QueryFirstOrDefaultAsync<Holiday>(query, new { Id = id, TenantId = tenantId });
        }

        /// <summary>
        /// Get all holidays for a tenant
        /// </summary>
        public async Task<IEnumerable<Holiday>> GetAllHolidaysAsync(int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetAllHolidays");

            return await conn.QueryAsync<Holiday>(query, new { TenantId = tenantId });
        }

        /// <summary>
        /// Get holidays with filters (year)
        /// </summary>
        public async Task<IEnumerable<Holiday>> GetHolidaysWithFiltersAsync(int tenantId, int? year)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetHolidaysWithFilters");

            return await conn.QueryAsync<Holiday>(query, new 
            { 
                TenantId = tenantId,
                Year = year
            });
        }

        /// <summary>
        /// Update a holiday
        /// </summary>
        public async Task<bool> UpdateHolidayAsync(Holiday holiday)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("UpdateHoliday");

            var rowsAffected = await conn.ExecuteAsync(query, new
            {
                holiday.Id,
                holiday.Name,
                holiday.Date,
                holiday.Description,
                holiday.UpdateUserId,
                holiday.TenantId
            });

            return rowsAffected > 0;
        }

        /// <summary>
        /// Delete a holiday
        /// </summary>
        public async Task<bool> DeleteHolidayAsync(int id, int tenantId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("DeleteHoliday");

            var rowsAffected = await conn.ExecuteAsync(query, new
            {
                Id = id,
                TenantId = tenantId
            });

            return rowsAffected > 0;
        }

        /// <summary>
        /// Create multiple holidays in bulk
        /// </summary>
        public async Task<int> BulkCreateHolidaysAsync(IEnumerable<Holiday> holidays)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("CreateHoliday");
            int totalInserted = 0;

            // Use a transaction for bulk insert
            using var transaction = conn.BeginTransaction();
            try
            {
                foreach (var holiday in holidays)
                {
                    await conn.ExecuteScalarAsync<int>(query, new
                    {
                        holiday.Name,
                        holiday.Date,
                        holiday.Description,
                        holiday.TenantId,
                        holiday.InsertUserId
                    }, transaction);
                    totalInserted++;
                }

                transaction.Commit();
                return totalInserted;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}

