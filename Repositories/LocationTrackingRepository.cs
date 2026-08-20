using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Models.Responses;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
    public class LocationTrackingRepository : ILocationTrackingRepository
    {
        private readonly DapperContext _context;
        private readonly QueryProvider _queryProvider;

        public LocationTrackingRepository(DapperContext context, QueryProvider queryProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _queryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
        }

        public async Task<int> InsertAsync(
            int employeeId,
            int tenantId,
            decimal latitude,
            decimal longitude,
            DateTime trackingDateTime,
            string? locationFrom,
            int insertUserId)
        {
            var now = DateTime.Now;
            var query = _queryProvider.Get("InsertLocationTracking");

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query, new
            {
                EmployeeId = employeeId,
                TenantId = tenantId,
                Latitude = latitude,
                Longitude = longitude,
                Date = trackingDateTime.Date,
                Time = trackingDateTime,
                LocationFrom = locationFrom,
                InsertUserId = insertUserId,
                InsertDate = now,
                UpdateUserId = insertUserId,
                UpdateDate = now
            });
        }

        public async Task<int> InsertBatchAsync(
            int employeeId,
            int tenantId,
            IReadOnlyList<LocationTrackingInsertRecord> records,
            int insertUserId)
        {
            if (records.Count == 0)
            {
                return 0;
            }

            var now = DateTime.Now;
            var query = _queryProvider.Get("InsertLocationTrackingBatch");

            var rows = records
                .OrderBy(r => r.TrackingDateTime)
                .Select(r => new
                {
                    EmployeeId = employeeId,
                    TenantId = tenantId,
                    Latitude = r.Latitude,
                    Longitude = r.Longitude,
                    Date = r.TrackingDateTime.Date,
                    Time = r.TrackingDateTime,
                    LocationFrom = r.LocationFrom,
                    InsertUserId = insertUserId,
                    InsertDate = now,
                    UpdateUserId = insertUserId,
                    UpdateDate = now
                })
                .ToList();

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var inserted = await connection.ExecuteAsync(query, rows, transaction);
                transaction.Commit();
                return inserted;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IReadOnlyList<LocationTrackingPointRow>> GetTodayByEmployeeIdAsync(
            int employeeId,
            int tenantId,
            DateTime today)
        {
            var query = _queryProvider.Get("GetTodayLocationTrackingByEmployeeId");

            using var connection = _context.CreateConnection();
            var rows = await connection.QueryAsync<LocationTrackingPointRow>(
                query,
                new
                {
                    EmployeeId = employeeId,
                    TenantId = tenantId,
                    Today = today.Date
                });

            return rows.ToList();
        }
    }
}
