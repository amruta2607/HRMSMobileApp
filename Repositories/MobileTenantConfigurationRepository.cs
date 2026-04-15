using Dapper;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
    public class MobileTenantConfigurationRepository : IMobileTenantConfigurationRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<MobileTenantConfigurationRepository> _logger;
        private readonly QueryProvider _queryProvider;

        public MobileTenantConfigurationRepository(
            DapperContext context,
            ILogger<MobileTenantConfigurationRepository> logger,
            QueryProvider queryProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
        }

        public async Task<MobileTenantConfiguration?> GetByTenantIdAsync(int organizationId)
        {
            try
            {
                var query = _queryProvider.Get("GetMobileTenantConfigurationByTenantId");

                using var connection = _context.CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<MobileTenantConfiguration>(
                    query,
                    new { TenantId = organizationId }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.User.ErrorFetchingTenantConfigurationByOrganisationId);
                throw;
            }
        }
    }
}

