using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
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
                InsertUserId = insertUserId,
                InsertDate = now,
                UpdateUserId = insertUserId,
                UpdateDate = now
            });
        }
    }
}
