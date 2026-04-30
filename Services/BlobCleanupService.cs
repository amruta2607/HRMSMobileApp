using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MobileWebApi.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MobileWebApi.Services
{
    /// <summary>
    /// Daily background cleanup for punch images.
    /// Deletes blobs older than 90 days based on blob CreatedOn timestamp.
    /// </summary>
    public class BlobCleanupService : BackgroundService
    {
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromDays(1);
        private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(90);

        private readonly BlobContainerClient? _containerClient;
        private readonly bool _isAzureConfigured;
        private readonly ILogger<BlobCleanupService> _logger;

        public BlobCleanupService(IConfiguration configuration, ILogger<BlobCleanupService> logger)
        {
            _logger = logger;

            var connectionString = configuration["AzureBlob:ConnectionString"];
            var containerName = configuration["AzureBlob:ContainerName"];

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(containerName))
            {
                _isAzureConfigured = false;
                _containerClient = null;
                _logger.LogWarning(LogMessages.AzureBlob.NotConfiguredCleanupDisabled);
                return;
            }

            try
            {
                var serviceClient = new BlobServiceClient(connectionString);
                _containerClient = serviceClient.GetBlobContainerClient(containerName);
                _isAzureConfigured = true;
            }
            catch (FormatException ex)
            {
                _isAzureConfigured = false;
                _containerClient = null;
                _logger.LogError(ex, LogMessages.AzureBlob.InvalidConnectionStringCleanupDisabled);
            }
            catch (Exception ex)
            {
                _isAzureConfigured = false;
                _containerClient = null;
                _logger.LogError(ex, LogMessages.AzureBlob.InitFailedCleanupDisabled);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_isAzureConfigured || _containerClient == null)
            {
                _logger.LogWarning(LogMessages.AzureBlob.CleanupServiceDisabled);
                return;
            }

            _logger.LogInformation(LogMessages.AzureBlob.CleanupServiceStarted, (int)RetentionPeriod.TotalDays);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, LogMessages.AzureBlob.CleanupRunFailed);
                }

                try
                {
                    await Task.Delay(CleanupInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Ignore on shutdown
                }
            }
        }

        private async Task CleanupOnceAsync(CancellationToken cancellationToken)
        {
            if (!_isAzureConfigured || _containerClient == null)
                return;

            if (!await _containerClient.ExistsAsync(cancellationToken))
            {
                _logger.LogInformation(LogMessages.AzureBlob.ContainerDoesNotExistSkippingCleanup);
                return;
            }

            var cutoffUtc = DateTime.UtcNow.Subtract(RetentionPeriod);

            var checkedCount = 0;
            var deletedCount = 0;

            await foreach (var blobItem in _containerClient.GetBlobsAsync(cancellationToken: cancellationToken))
            {
                checkedCount++;

                // CreatedOn is nullable depending on SDK/endpoint.
                var createdOn = blobItem.Properties.CreatedOn;
                if (!createdOn.HasValue)
                    continue;

                if (createdOn.Value.UtcDateTime < cutoffUtc)
                {
                    var blobClient = _containerClient.GetBlobClient(blobItem.Name);
                    var deleted = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

                    if (deleted)
                        deletedCount++;
                }
            }

            _logger.LogInformation(LogMessages.AzureBlob.CleanupCompleted, checkedCount, deletedCount, cutoffUtc);
        }
    }
}

