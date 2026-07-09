using Dapper;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models.Responses;
using MobileWebApi.Repositories.Interfaces;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
    /// <summary>
    /// Provides tenant-scoped asset hand over list data using Dapper.
    /// </summary>
    public class AssetHandOverRepository : IAssetHandOverRepository
    {
        private readonly DapperContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly QueryProvider _queries;
        private readonly ILogger<AssetHandOverRepository> _logger;

        public AssetHandOverRepository(
            DapperContext context,
            ITenantContext tenantContext,
            QueryProvider queries,
            ILogger<AssetHandOverRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<AssetHandOverListResponse> GetListAsync()
        {
            try
            {
                var tenantId = _tenantContext.GetRequiredOrganisationId();
                var sql = _queries.Get("AssetHandOverList");

                using var connection = _context.CreateConnection();
                var items = (await connection.QueryAsync<AssetHandOverDto>(sql, new { TenantId = tenantId })).ToList();

                return new AssetHandOverListResponse
                {
                    Items = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(
                    ExceptionCodes.AssetHandOver.GetList,
                    nameof(GetListAsync),
                    ex,
                    _tenantContext.UserId);

                throw;
            }
        }
    }
}
