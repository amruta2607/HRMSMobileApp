using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;
using MobileWebApi.Constants;

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
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(CreateHolidayAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.HolidayCreateHolidayDatabaseError}: Failed to create holiday",
                    ex);
            }
        }

        /// <summary>
        /// Get holiday by ID
        /// </summary>
        public async Task<Holiday?> GetHolidayByIdAsync(int id, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetHolidayById");

                return await conn.QueryFirstOrDefaultAsync<Holiday>(query, new { Id = id, TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetHolidayByIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.HolidayGetHolidayByIdDatabaseError}: Failed to fetch holiday by id",
                    ex);
            }
        }

        /// <summary>
        /// Get all holidays for a tenant
        /// </summary>
        public async Task<IEnumerable<Holiday>> GetAllHolidaysAsync(int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetAllHolidays");

                return await conn.QueryAsync<Holiday>(query, new { TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetAllHolidaysAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.HolidayGetAllHolidaysDatabaseError}: Failed to fetch all holidays",
                    ex);
            }
        }

        /// <summary>
        /// Get holidays with filters (year)
        /// </summary>
        public async Task<IEnumerable<Holiday>> GetHolidaysWithFiltersAsync(int tenantId, int? year)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetHolidaysWithFilters");

                return await conn.QueryAsync<Holiday>(query, new
                {
                    TenantId = tenantId,
                    Year = year
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetHolidaysWithFiltersAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.HolidayGetHolidaysWithFiltersDatabaseError}: Failed to fetch holidays with filters",
                    ex);
            }
        }

        /// <summary>
        /// Update a holiday
        /// </summary>
        public async Task<bool> UpdateHolidayAsync(Holiday holiday)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(UpdateHolidayAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.HolidayUpdateHolidayDatabaseError}: Failed to update holiday",
                    ex);
            }
        }

        /// <summary>
        /// Delete a holiday
        /// </summary>
        public async Task<bool> DeleteHolidayAsync(int id, int tenantId)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(DeleteHolidayAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.HolidayDeleteHolidayDatabaseError}: Failed to delete holiday",
                    ex);
            }
        }

        /// <summary>
        /// Create multiple holidays in bulk
        /// </summary>
        public async Task<int> BulkCreateHolidaysAsync(IEnumerable<Holiday> holidays)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(BulkCreateHolidaysAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.HolidayBulkCreateHolidaysDatabaseError}: Failed to bulk create holidays",
                    ex);
            }
        }
    }
}

