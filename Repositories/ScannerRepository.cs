using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models.Responses;
using MobileWebApi.Repositories.Interfaces;
using MobileWebApi.Resources;
using MobileWebApi.Services;

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
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ScannerRepository> _logger;

        public ScannerRepository(
            DapperContext context,
            ITenantContext tenantContext,
            QueryProvider queries,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ScannerRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<AssetScannerResponse?> GetAssetAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ScannerValidationException(ScannerMessages.InvalidQrCode);

            try
            {
                var tenantId = _tenantContext.GetRequiredOrganisationId();
                var scannedValue = code.Trim();
                AssetScannerResponse? asset;

                if (AssetQrScannerHelper.TryParseAssetId(scannedValue, out var assetId))
                {
                    if (assetId <= 0)
                        throw new ScannerValidationException(ScannerMessages.InvalidQrCode);

                    asset = await GetAssetByIdAsync(tenantId, assetId);
                }
                else
                {
                    asset = await GetAssetByCodeAsync(tenantId, scannedValue);
                }

                if (asset == null)
                    return null;

                ApplyAbsoluteMediaUrls(asset);
                return asset;
            }
            catch (ScannerValidationException)
            {
                throw;
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

        private async Task<AssetScannerResponse?> GetAssetByIdAsync(int tenantId, int assetId)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<AssetScannerResponse>(
                _queries.Get("GetAssetByScannerId"),
                new { TenantId = tenantId, AssetId = assetId });
        }

        private async Task<AssetScannerResponse?> GetAssetByCodeAsync(int tenantId, string scannedValue)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<AssetScannerResponse>(
                _queries.Get("GetAssetByScanner"),
                new { TenantId = tenantId, Code = scannedValue });
        }

        private void ApplyAbsoluteMediaUrls(AssetScannerResponse asset)
        {
            var baseUrl = ResolvePublicBaseUrl();
            asset.QRCodePath = AssetQrScannerHelper.ToAbsoluteUrl(asset.QRCodePath, baseUrl);
        }

        private string ResolvePublicBaseUrl()
        {
            var configuredBaseUrl = _configuration["ApiSettings:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(configuredBaseUrl))
                return configuredBaseUrl.TrimEnd('/');

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
                return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

            return string.Empty;
        }
    }
}
