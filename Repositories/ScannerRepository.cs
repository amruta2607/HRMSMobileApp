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
    /// Provides tenant-scoped asset lookup for the mobile scanner using Dapper.
    /// </summary>
    public class ScannerRepository : IScannerRepository
    {
        private readonly DapperContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly QueryProvider _queries;
        private readonly ILogger<ScannerRepository> _logger;

        public ScannerRepository(
            DapperContext context,
            ITenantContext tenantContext,
            QueryProvider queries,
            ILogger<ScannerRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<AssetScannerResponse?> GetAssetAsync(string code)
        {
            try
            {
                var tenantId = _tenantContext.GetRequiredOrganisationId();
                var sql = _queries.Get("GetAssetByScanner");

                using var connection = _context.CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<AssetScannerResponse>(sql, new
                {
                    TenantId = tenantId,
                    Code = code.Trim()
                });
            }
            catch (Exception ex)
            {
                _logger.LogException(
                    ExceptionCodes.Scanner.GetAsset,
                    nameof(GetAssetAsync),
                    ex,
                    _tenantContext.UserId);

                throw;
            }
        }
    }
}
