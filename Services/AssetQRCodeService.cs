using Dapper;
using Microsoft.Extensions.Configuration;
using MobileWebApi.Interfaces;
using MobileWebApi.Resources;
using QRCoder;
using System.Data;
using System.Globalization;

namespace MobileWebApi.Services
{
    /// <summary>
    /// Generates per-tenant asset codes and QR images for the Asset module.
    /// </summary>
    public class AssetQRCodeService : IAssetQRCodeService
    {
        public const string AssetCodePrefix = "AST-";

        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly QueryProvider _queries;

        public AssetQRCodeService(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            QueryProvider queries)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        /// <inheritdoc />
        public string GenerateAssetCode(IDbConnection connection, int tenantId, IDbTransaction? transaction = null)
        {
            var max = connection.QueryFirstOrDefault<string>(
                _queries.Get("Asset_GetMaxAssetCode"),
                new { TenantId = tenantId, Pattern = AssetCodePrefix + "%" },
                transaction);

            long next = 1;
            if (!string.IsNullOrEmpty(max) &&
                max.Length > AssetCodePrefix.Length &&
                long.TryParse(max[AssetCodePrefix.Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var current))
            {
                next = current + 1;
            }

            return AssetCodePrefix + next.ToString("D6", CultureInfo.InvariantCulture);
        }

        /// <inheritdoc />
        public AssetQRResult EnsureQRCode(
            IDbConnection connection,
            IDbTransaction transaction,
            int assetId,
            int tenantId)
        {
            var asset = connection.QueryFirstOrDefault<AssetQrRow>(
                _queries.Get("Asset_GetForQrCode"),
                new { AssetId = assetId, TenantId = tenantId },
                transaction);

            if (asset == null)
                throw new AssetValidationException("Asset was not found for QR code generation.");

            var assetCode = string.IsNullOrWhiteSpace(asset.AssetCode)
                ? GenerateAssetCode(connection, tenantId, transaction)
                : asset.AssetCode;

            var qrUrl = BuildQRUrl(assetId);
            var pngBytes = GenerateQRCode(qrUrl);
            var relativePath = SaveQRCode(pngBytes, tenantId, assetCode);
            var generatedAt = DateTime.Now;

            connection.Execute(
                _queries.Get("Asset_UpdateQrCode"),
                new
                {
                    AssetId = assetId,
                    TenantId = tenantId,
                    AssetCode = assetCode,
                    QRCodePath = relativePath,
                    QRCodeText = qrUrl,
                    QRCodeGeneratedDate = generatedAt
                },
                transaction);

            return new AssetQRResult
            {
                AssetId = assetId,
                AssetCode = assetCode,
                QRCodePath = relativePath,
                QRCodeText = qrUrl,
                QRCodeGenerated = true,
                QRCodeGeneratedDate = generatedAt
            };
        }

        private string BuildQRUrl(int assetId)
        {
            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');
            // Match existing web behavior (public QR landing page).
            return $"{baseUrl}/Asset/ViewByQR/{assetId}";
        }

        private static byte[] GenerateQRCode(string text)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(text ?? string.Empty, QRCodeGenerator.ECCLevel.Q);
            var pngQr = new PngByteQRCode(data);
            return pngQr.GetGraphic(20);
        }

        private string SaveQRCode(byte[] pngBytes, int tenantId, string assetCode)
        {
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
                webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");

            var absoluteDir = Path.Combine(webRoot, "Upload", "AssetQR",
                tenantId.ToString(CultureInfo.InvariantCulture));
            Directory.CreateDirectory(absoluteDir);

            var fileName = assetCode + ".png";
            File.WriteAllBytes(Path.Combine(absoluteDir, fileName), pngBytes);

            return $"Upload/AssetQR/{tenantId}/{fileName}";
        }

        private sealed class AssetQrRow
        {
            public int Id { get; set; }
            public string? AssetCode { get; set; }
            public string? QRCodePath { get; set; }
            public string? QRCodeText { get; set; }
            public bool? QRCodeGenerated { get; set; }
            public DateTime? QRCodeGeneratedDate { get; set; }
        }
    }
}
