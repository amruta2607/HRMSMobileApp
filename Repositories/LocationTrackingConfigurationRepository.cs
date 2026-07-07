using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
    public class LocationTrackingConfigurationRepository : ILocationTrackingConfigurationRepository
    {
        private readonly DapperContext _context;
        private readonly QueryProvider _queryProvider;

        public LocationTrackingConfigurationRepository(DapperContext context, QueryProvider queryProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _queryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
        }

        public async Task<LocationTrackingConfiguration?> GetByIdAsync(int id)
        {
            var query = _queryProvider.Get("GetLocationTrackingConfigurationById");

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<LocationTrackingConfiguration>(
                query,
                new { Id = id });
        }
    }
}
