using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using MobileWebApi.Constants;
using SixLabors.ImageSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;

namespace MobileWebApi.Services
{
    public class BlobService
    {
        private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB
        private const int DefaultSasExpiryMinutes = 30;
        private const int MinSasExpiryMinutes = 1;
        private const int MaxSasExpiryMinutes = 1440;
        private static readonly string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        private static readonly string[] AllowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png" };

        private readonly BlobServiceClient? _blobServiceClient;
        private readonly BlobContainerClient? _containerClient;
        private readonly string? _containerName;
        private readonly int _sasExpiryMinutes;
        private readonly bool _isAzureConfigured;
        private readonly ILogger<BlobService> _logger;

        public BlobService(IConfiguration configuration, ILogger<BlobService> logger)
        {
            _logger = logger;

            var connectionString = configuration["AzureBlob:ConnectionString"];
            var containerName = configuration["AzureBlob:ContainerName"];
            _sasExpiryMinutes = ResolveSasExpiryMinutes(configuration["AzureBlob:SasExpiryMinutes"]);

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(containerName))
            {
                _isAzureConfigured = false;
                _logger.LogWarning(LogMessages.AzureBlob.NotConfiguredUploadDisabled);
                return;
            }

            try
            {
                _blobServiceClient = new BlobServiceClient(connectionString);
                _containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                _containerName = containerName;
                _isAzureConfigured = true;
            }
            catch (FormatException ex)
            {
                // Azure SDK throws FormatException if connection string isn't in expected form.
                _isAzureConfigured = false;
                _logger.LogError(ex, LogMessages.AzureBlob.InvalidConnectionStringUploadDisabled);
            }
            catch (Exception ex)
            {
                _isAzureConfigured = false;
                _logger.LogError(ex, LogMessages.AzureBlob.InitFailedUploadDisabled);
            }
        }

        /// <summary>
        /// Uploads the punch image to Azure Blob Storage and returns a blob URL to store in DB.
        /// File naming: {empId}_{yyyyMMdd_HHmmss}.jpg
        /// Folder structure: {year}/{month}/
        /// </summary>
        public async Task<string> UploadAsync(IFormFile file, int empId)
        {
            if (!_isAzureConfigured || _containerClient == null)
                throw new InvalidOperationException("Azure Blob is not configured correctly. Please set AzureBlob:ConnectionString and AzureBlob:ContainerName.");

            if (file == null || file.Length == 0)
                throw new ArgumentException("Image file is required.");

            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException("Image size must be less than or equal to 2 MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new ArgumentException("Only JPG/JPEG and PNG images are allowed.");

            if (!string.IsNullOrWhiteSpace(file.ContentType))
            {
                var contentType = file.ContentType.ToLowerInvariant();
                if (!AllowedContentTypes.Contains(contentType))
                    throw new ArgumentException("Invalid image content type. Only JPG/JPEG and PNG are allowed.");
            }

            // Use UTC timestamps to avoid server timezone issues.
            var nowUtc = DateTime.UtcNow;
            var year = nowUtc.ToString("yyyy");
            var month = nowUtc.ToString("MM");

            var fileName = $"{empId}_{nowUtc:yyyyMMdd_HHmmss}.jpg";
            var blobName = $"{year}/{month}/{fileName}";

            _logger.LogInformation(LogMessages.AzureBlob.UploadingPunchImage, blobName, empId);

            try
            {
                // Create container if it doesn't exist. Keep private (PublicAccessType.None).
                // DB stores the plain blob URL; callers generate a read SAS when returning images to clients.
                await _containerClient.CreateIfNotExistsAsync(publicAccessType: PublicAccessType.None);

                var blobClient = _containerClient.GetBlobClient(blobName);

                // Convert/compress all uploads to JPEG to keep a consistent naming scheme (.jpg).
                await using var inputStream = file.OpenReadStream();
                using var image = Image.Load(inputStream);

                await using var outputStream = new MemoryStream();
                image.Save(outputStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 85 });
                outputStream.Position = 0;

                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = "image/jpeg" }
                };

                await blobClient.UploadAsync(outputStream, uploadOptions);

                var blobUrl = blobClient.Uri.ToString();
                _logger.LogInformation(LogMessages.AzureBlob.PunchImageUploadedSuccessfully, empId, blobName);
                return blobUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.AzureBlob.ErrorUploadingPunchImage, empId);
                throw;
            }
        }

        /// <summary>
        /// Generates a temporary read-only SAS URL for a stored punch image blob URL (or blob path).
        /// Returns the original value unchanged when null/empty, Azure is not configured, or conversion fails.
        /// Does not log the SAS signature.
        /// </summary>
        public string? GenerateReadSasUrl(string? blobUrlOrPath)
        {
            if (string.IsNullOrWhiteSpace(blobUrlOrPath))
                return blobUrlOrPath;

            if (!_isAzureConfigured || _containerClient == null || string.IsNullOrWhiteSpace(_containerName))
                return blobUrlOrPath;

            try
            {
                if (!TryResolveBlobName(blobUrlOrPath, out var blobName))
                {
                    _logger.LogWarning(LogMessages.AzureBlob.SasInvalidBlobReference);
                    return blobUrlOrPath;
                }

                var blobClient = _containerClient.GetBlobClient(blobName);
                if (!blobClient.CanGenerateSasUri)
                {
                    _logger.LogWarning(LogMessages.AzureBlob.SasCannotGenerate, blobName);
                    return blobUrlOrPath;
                }

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerName,
                    BlobName = blobName,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1),
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(_sasExpiryMinutes)
                };
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                return blobClient.GenerateSasUri(sasBuilder).ToString();
            }
            catch (Exception ex)
            {
                // Do not break attendance APIs when a single image URL cannot be signed.
                _logger.LogWarning(ex, LogMessages.AzureBlob.SasGenerationFailed);
                return blobUrlOrPath;
            }
        }

        private static int ResolveSasExpiryMinutes(string? configuredValue)
        {
            if (!int.TryParse(configuredValue, out var minutes))
                return DefaultSasExpiryMinutes;

            if (minutes < MinSasExpiryMinutes)
                return MinSasExpiryMinutes;

            if (minutes > MaxSasExpiryMinutes)
                return MaxSasExpiryMinutes;

            return minutes;
        }

        private bool TryResolveBlobName(string blobUrlOrPath, out string blobName)
        {
            blobName = string.Empty;

            // Absolute blob URL: https://account.blob.core.windows.net/container/path/to/blob.jpg
            if (Uri.TryCreate(blobUrlOrPath, UriKind.Absolute, out var absoluteUri))
            {
                // Strip any existing query (e.g. expired SAS) before resolving the blob path.
                var path = absoluteUri.AbsolutePath.Trim('/');
                if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(_containerName))
                    return false;

                var containerPrefix = _containerName + "/";
                if (path.StartsWith(containerPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    blobName = path[containerPrefix.Length..];
                    return !string.IsNullOrWhiteSpace(blobName);
                }

                // Path is only the blob name (no container segment) — uncommon but support it.
                if (!path.Contains('/'))
                {
                    blobName = path;
                    return true;
                }

                // If the first segment is some other container name, take the remainder as blob name
                // so legacy URLs against the same account still work when container matches config.
                var firstSlash = path.IndexOf('/');
                if (firstSlash > 0 && firstSlash < path.Length - 1)
                {
                    var urlContainer = path[..firstSlash];
                    if (urlContainer.Equals(_containerName, StringComparison.OrdinalIgnoreCase))
                    {
                        blobName = path[(firstSlash + 1)..];
                        return !string.IsNullOrWhiteSpace(blobName);
                    }
                }

                return false;
            }

            // Relative blob path already stored without host, e.g. "2026/07/420_....jpg"
            blobName = blobUrlOrPath.Trim().TrimStart('/');
            if (blobName.StartsWith(_containerName + "/", StringComparison.OrdinalIgnoreCase))
                blobName = blobName[(_containerName!.Length + 1)..];

            return !string.IsNullOrWhiteSpace(blobName);
        }
    }
}
