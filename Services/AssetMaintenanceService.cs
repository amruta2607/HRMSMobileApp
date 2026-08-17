using MobileWebApi.Constants;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Models.Requests;
using MobileWebApi.Models.Responses;
using MobileWebApi.Repositories.Interfaces;

namespace MobileWebApi.Services
{
    /// <summary>
    /// Encapsulates validation and business logic for the asset maintenance module.
    /// </summary>
    public class AssetMaintenanceService : IAssetMaintenanceService
    {
        private readonly IAssetMaintenanceRepository _repository;
        private readonly ITenantContext _tenantContext;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<AssetMaintenanceService> _logger;

        public AssetMaintenanceService(
            IAssetMaintenanceRepository repository,
            ITenantContext tenantContext,
            IImageUploadService imageUploadService,
            ILogger<AssetMaintenanceService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<CreateAssetMaintenanceResponse> CreateAssetMaintenanceAsync(AssetMaintenanceRequest request)
        {
            if (request == null)
                throw new AssetValidationException(AssetMessages.RequestBodyCannotBeNull);

            if (request.AssetId <= 0)
                throw new AssetValidationException(AssetMessages.InvalidAsset);

            if (IsDefaultDate(request.Date))
                throw new AssetValidationException(AssetMessages.MaintenanceDateRequired);

            if (request.Cost.HasValue && request.Cost.Value < 0)
                throw new AssetValidationException(AssetMessages.MaintenanceCostInvalid);

            // Upload files first so no DB changes are made if any upload fails.
            var attachments = await UploadAttachmentsAsync(request.Attachments);

            _logger.LogInformation(
                LogMessages.AssetMaintenance.Creating,
                request.AssetId,
                _tenantContext.UserId,
                _tenantContext.OrganisationId);

            return await _repository.CreateAsync(request, attachments);
        }

        /// <inheritdoc />
        public async Task<UpdateAssetMaintenanceResponse> UpdateAssetMaintenanceAsync(int id, UpdateAssetMaintenanceRequest request)
        {
            if (request == null)
                throw ValidationError(AssetMessages.RequestBodyCannotBeNull);

            if (request.AssetId <= 0)
                throw ValidationError(AssetMessages.InvalidAsset);

            if (IsDefaultDate(request.Date))
                throw ValidationError(AssetMessages.MaintenanceDateRequired);

            if (request.Cost.HasValue && request.Cost.Value < 0)
                throw ValidationError(AssetMessages.MaintenanceCostInvalid);

            // Upload any new files first; a null result keeps the existing attachments unchanged.
            var attachments = await UploadAttachmentsAsync(request.Attachments);

            _logger.LogInformation(
                LogMessages.AssetMaintenance.Updating,
                id,
                request.AssetId,
                _tenantContext.UserId,
                _tenantContext.OrganisationId);

            return await _repository.UpdateAsync(id, request, attachments);
        }

        /// <inheritdoc />
        public Task<AssetMaintenanceHistoryResponse> GetAssetMaintenanceByAssetIdAsync(int assetId)
        {
            _logger.LogInformation(
                LogMessages.AssetMaintenance.FetchingByAsset,
                assetId,
                _tenantContext.OrganisationId);

            return _repository.GetByAssetIdAsync(assetId);
        }

        /// <inheritdoc />
        public Task<AssetMaintenanceListResponse> GetAllAssetMaintenanceAsync(AssetMaintenanceQueryParameters query)
        {
            query ??= new AssetMaintenanceQueryParameters();
            return _repository.GetAllAsync(query);
        }

        /// <inheritdoc />
        public Task<AssetOperationResponse> DeleteAssetMaintenanceAsync(int id, string? ipAddress)
        {
            _logger.LogInformation(
                LogMessages.AssetMaintenance.Deleting,
                id,
                _tenantContext.UserId,
                _tenantContext.OrganisationId);

            return _repository.DeleteAsync(id, ipAddress);
        }

        /// <inheritdoc />
        public Task<AssetMaintenanceLookupResponse> GetAssetMaintenanceLookupsAsync()
        {
            _logger.LogInformation(
                LogMessages.AssetMaintenance.FetchingLookups,
                _tenantContext.UserId,
                _tenantContext.OrganisationId);

            return _repository.GetAssetMaintenanceLookupsAsync();
        }

        /// <inheritdoc />
        public Task<AssetTimelineListResponse> GetAssetMaintenanceTimelineAsync(int assetId)
        {
            _logger.LogInformation(
                LogMessages.AssetMaintenance.FetchingTimeline,
                assetId,
                _tenantContext.UserId,
                _tenantContext.OrganisationId);

            return _repository.GetAssetMaintenanceTimelineAsync(assetId);
        }

        /// <summary>
        /// Validates and uploads the supplied files using the existing upload service and returns
        /// the resulting attachment metadata. Returns <c>null</c> when no files are supplied so the
        /// caller can leave the Attachment column null (create) or unchanged (update).
        /// If any file is invalid or fails to upload, an exception is thrown before any DB change is made.
        /// </summary>
        private async Task<List<FileAttachment>?> UploadAttachmentsAsync(List<IFormFile>? files)
        {
            var incoming = files?.Where(f => f != null && f.Length > 0).ToList();
            if (incoming == null || incoming.Count == 0)
                return null;

            var tenantId = _tenantContext.GetRequiredOrganisationId();
            var attachments = new List<FileAttachment>();

            foreach (var file in incoming)
            {
                var validation = _imageUploadService.ValidateAttachment(file);
                if (!validation.IsValid)
                    throw ValidationError(validation.ErrorMessage);

                try
                {
                    var storedPath = await _imageUploadService.SaveAssetDocumentAsync(file, tenantId);
                    attachments.Add(new FileAttachment
                    {
                        Filename = storedPath,
                        OriginalName = file.FileName
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        LogMessages.AssetMaintenance.AttachmentUploadFailed,
                        file.FileName,
                        tenantId);

                    throw new AssetValidationException(AssetMessages.AttachmentUploadFailed);
                }
            }

            _logger.LogInformation(
                LogMessages.AssetMaintenance.AttachmentsUploaded,
                attachments.Count,
                tenantId,
                _tenantContext.UserId);

            return attachments;
        }

        private static bool IsDefaultDate(DateTime value)
            => value == DateTime.MinValue || value.Year <= 1;

        /// <summary>
        /// Logs a validation failure and returns the corresponding exception to throw.
        /// </summary>
        private AssetValidationException ValidationError(string reason)
        {
            _logger.LogWarning(
                LogMessages.AssetMaintenance.ValidationFailed,
                _tenantContext.UserId,
                _tenantContext.OrganisationId,
                reason);

            return new AssetValidationException(reason);
        }
    }
}
