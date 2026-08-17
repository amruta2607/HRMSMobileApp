using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
        private static readonly string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        private static readonly string[] AllowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png" };

        private readonly BlobServiceClient? _blobServiceClient;
        private readonly BlobContainerClient? _containerClient;
        private readonly bool _isAzureConfigured;
        private readonly ILogger<BlobService> _logger;

        public BlobService(IConfiguration configuration, ILogger<BlobService> logger)
        {
            _logger = logger;

            var connectionString = configuration["AzureBlob:ConnectionString"];
            var containerName = configuration["AzureBlob:ContainerName"];

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
                // The returned URL is the blob URL; to access private blobs, you can later add SAS token support.
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
                _logger.LogInformation(LogMessages.AzureBlob.PunchImageUploadedSuccessfully, empId, blobUrl);
                return blobUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.AzureBlob.ErrorUploadingPunchImage, empId);
                throw;
            }
        }
    }
}

