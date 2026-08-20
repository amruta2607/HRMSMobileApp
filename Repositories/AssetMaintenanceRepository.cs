using Dapper;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Models.Requests;
using MobileWebApi.Models.Responses;
using MobileWebApi.Repositories.Interfaces;
using MobileWebApi.Resources;
using MobileWebApi.Services;

namespace MobileWebApi.Repositories
{
    /// <summary>
    /// Provides tenant-scoped asset maintenance data access using Dapper.
    /// </summary>
    public class AssetMaintenanceRepository : IAssetMaintenanceRepository
    {
        private readonly DapperContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly QueryProvider _queries;
        private readonly ILogger<AssetMaintenanceRepository> _logger;

        /// <summary>
        /// Whitelist of client sort keys mapped to their physical column names.
        /// Prevents SQL injection through the dynamic ORDER BY clause.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> SortableColumns =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["date"] = "Date",
                ["cost"] = "Cost",
                ["assetnumber"] = "AssetNumber",
                ["assetname"] = "AssetName",
                ["responsibleperson"] = "ResponsiblePerson",
                ["id"] = "Id"
            };

        public AssetMaintenanceRepository(
            DapperContext context,
            ITenantContext tenantContext,
            QueryProvider queries,
            ILogger<AssetMaintenanceRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<CreateAssetMaintenanceResponse> CreateAsync(AssetMaintenanceRequest request, List<FileAttachment>? attachments)
        {
            var tenantId = _tenantContext.GetRequiredOrganisationId();
            var userId = _tenantContext.GetRequiredUserId();

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var assetExists = await connection.ExecuteScalarAsync<int?>(
                    _queries.Get("AssetMaintenance_ExistsAsset"),
                    new { AssetId = request.AssetId, TenantId = tenantId },
                    transaction);

                if (assetExists != 1)
                    throw new AssetValidationException(AssetMessages.InvalidAsset);

                var maintenanceId = await connection.ExecuteScalarAsync<int>(
                    _queries.Get("AssetMaintenance_Insert"),
                    new
                    {
                        AssetId = request.AssetId,
                        Cost = request.Cost.HasValue ? (double?)request.Cost.Value : null,
                        Attachment = AttachmentJsonHelper.Serialize(attachments),
                        Date = request.Date,
                        ResponsiblePerson = OptionalValueHelper.NullIfEmpty(request.ResponsiblePerson),
                        AssetNumber = OptionalValueHelper.NullIfEmpty(request.AssetNumber),
                        AssetName = OptionalValueHelper.NullIfEmpty(request.AssetName),
                        AssetDescription = OptionalValueHelper.NullIfEmpty(request.AssetDescription),
                        InsertUserId = userId,
                        TenantId = tenantId
                    },
                    transaction);

                transaction.Commit();

                _logger.LogInformation(
                    LogMessages.AssetMaintenance.Created,
                    maintenanceId,
                    request.AssetId,
                    userId,
                    tenantId,
                    DateTime.UtcNow);

                return new CreateAssetMaintenanceResponse
                {
                    Success = true,
                    Id = maintenanceId,
                    Message = AssetMessages.MaintenanceCreatedSuccessfully
                };
            }
            catch (AssetValidationException)
            {
                transaction.Rollback();
                throw;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogException(
                    ExceptionCodes.AssetMaintenance.Create,
                    nameof(CreateAsync),
                    ex,
                    userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<UpdateAssetMaintenanceResponse> UpdateAsync(int id, UpdateAssetMaintenanceRequest request, List<FileAttachment>? attachments)
        {
            var tenantId = _tenantContext.GetRequiredOrganisationId();
            var userId = _tenantContext.GetRequiredUserId();

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Ensure the maintenance record exists for the current tenant.
                var existing = await connection.QueryFirstOrDefaultAsync<AssetMaintenanceDeleteRow>(
                    _queries.Get("AssetMaintenance_GetForDelete"),
                    new { Id = id, TenantId = tenantId },
                    transaction);

                if (existing == null)
                    throw new AssetNotFoundException(AssetMessages.MaintenanceNotFound);

                // Ensure the target asset exists for the current tenant.
                var assetExists = await connection.ExecuteScalarAsync<int?>(
                    _queries.Get("AssetMaintenance_ExistsAsset"),
                    new { AssetId = request.AssetId, TenantId = tenantId },
                    transaction);

                if (assetExists != 1)
                    throw new AssetValidationException(AssetMessages.InvalidAsset);

                var rowsAffected = await connection.ExecuteAsync(
                    _queries.Get("AssetMaintenance_Update"),
                    new
                    {
                        Id = id,
                        AssetId = request.AssetId,
                        Cost = request.Cost.HasValue ? (double?)request.Cost.Value : null,
                        Attachment = AttachmentJsonHelper.Serialize(attachments),
                        Date = request.Date,
                        ResponsiblePerson = OptionalValueHelper.NullIfEmpty(request.ResponsiblePerson),
                        AssetNumber = OptionalValueHelper.NullIfEmpty(request.AssetNumber),
                        AssetName = OptionalValueHelper.NullIfEmpty(request.AssetName),
                        AssetDescription = OptionalValueHelper.NullIfEmpty(request.AssetDescription),
                        UpdateUserId = userId,
                        TenantId = tenantId
                    },
                    transaction);

                if (rowsAffected == 0)
                    throw new AssetNotFoundException(AssetMessages.MaintenanceNotFound);

                transaction.Commit();

                _logger.LogInformation(
                    LogMessages.AssetMaintenance.Updated,
                    id,
                    request.AssetId,
                    userId,
                    tenantId,
                    DateTime.UtcNow);

                return new UpdateAssetMaintenanceResponse
                {
                    Success = true,
                    Id = id,
                    Message = AssetMessages.MaintenanceUpdatedSuccessfully
                };
            }
            catch (AssetValidationException)
            {
                transaction.Rollback();
                throw;
            }
            catch (AssetNotFoundException)
            {
                transaction.Rollback();
                throw;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogException(
                    ExceptionCodes.AssetMaintenance.Update,
                    nameof(UpdateAsync),
                    ex,
                    userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AssetMaintenanceHistoryResponse> GetByAssetIdAsync(int assetId)
        {
            try
            {
                var tenantId = _tenantContext.GetRequiredOrganisationId();

                using var connection = _context.CreateConnection();
                var items = (await connection.QueryAsync<AssetMaintenanceRow>(
                    _queries.Get("AssetMaintenance_GetByAssetId"),
                    new { AssetId = assetId, TenantId = tenantId }))
                    .Select(MapToDto)
                    .ToList();

                _logger.LogInformation(
                    LogMessages.AssetMaintenance.FetchedByAsset,
                    items.Count,
                    assetId,
                    tenantId);

                return new AssetMaintenanceHistoryResponse
                {
                    Success = true,
                    Message = AssetMessages.MaintenanceRetrievedSuccessfully,
                    Data = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(
                    ExceptionCodes.AssetMaintenance.GetByAsset,
                    nameof(GetByAssetIdAsync),
                    ex,
                    _tenantContext.UserId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AssetMaintenanceListResponse> GetAllAsync(AssetMaintenanceQueryParameters query)
        {
            try
            {
                var tenantId = _tenantContext.GetRequiredOrganisationId();

                var searchTerm = OptionalValueHelper.NullIfEmpty(query.Search);
                var searchPattern = searchTerm == null ? null : $"%{searchTerm}%";
                var orderByClause = BuildOrderByClause(query.SortBy, query.SortDirection);
                var offset = (query.Page - 1) * query.PageSize;

                using var connection = _context.CreateConnection();

                var totalRecords = await connection.ExecuteScalarAsync<int>(
                    _queries.Get("AssetMaintenance_GetCount"),
                    new { TenantId = tenantId, Search = searchPattern });

                var pagedSql = _queries.Get("AssetMaintenance_GetPaged")
                    .Replace("{ORDER_BY}", orderByClause);

                var items = (await connection.QueryAsync<AssetMaintenanceRow>(
                    pagedSql,
                    new
                    {
                        TenantId = tenantId,
                        Search = searchPattern,
                        Offset = offset,
                        PageSize = query.PageSize
                    }))
                    .Select(MapToDto)
                    .ToList();

                var totalPages = query.PageSize > 0
                    ? (int)Math.Ceiling(totalRecords / (double)query.PageSize)
                    : 0;

                return new AssetMaintenanceListResponse
                {
                    Items = items,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(
                    ExceptionCodes.AssetMaintenance.GetList,
                    nameof(GetAllAsync),
                    ex,
                    _tenantContext.UserId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AssetMaintenanceLookupResponse> GetAssetMaintenanceLookupsAsync()
        {
            try
            {
                var tenantId = _tenantContext.GetRequiredOrganisationId();
                var userId = _tenantContext.UserId;

                _logger.LogInformation(
                    LogMessages.AssetMaintenance.FetchingLookups,
                    userId,
                    tenantId);

                using var connection = _context.CreateConnection();

                var assets = (await connection.QueryAsync<AssetMaintenanceLookupAssetDto>(
                    _queries.Get("AssetMaintenance_LookupAssets"),
                    new { TenantId = tenantId })).ToList();

                var responsiblePersons = (await connection.QueryAsync<AssetMaintenanceLookupEmployeeDto>(
                    _queries.Get("AssetMaintenance_LookupResponsiblePersons"),
                    new { TenantId = tenantId })).ToList();

                _logger.LogInformation(
                    LogMessages.AssetMaintenance.LookupsFetched,
                    tenantId,
                    assets.Count,
                    responsiblePersons.Count);

                return new AssetMaintenanceLookupResponse
                {
                    Success = true,
                    Message = AssetMessages.MaintenanceLookupsFetchedSuccessfully,
                    Data = new AssetMaintenanceLookupData
                    {
                        Assets = assets,
                        ResponsiblePersons = responsiblePersons
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(
                    ExceptionCodes.AssetMaintenance.GetLookups,
                    nameof(GetAssetMaintenanceLookupsAsync),
                    ex,
                    _tenantContext.UserId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AssetTimelineListResponse> GetAssetMaintenanceTimelineAsync(int assetId)
        {
            try
            {
                var tenantId = _tenantContext.GetRequiredOrganisationId();

                using var connection = _context.CreateConnection();
                var items = (await connection.QueryAsync<AssetTimelineResponse>(
                    _queries.Get("AssetMaintenance_GetTimeline"),
                    new { AssetId = assetId, TenantId = tenantId })).ToList();

                _logger.LogInformation(
                    LogMessages.AssetMaintenance.TimelineFetched,
                    items.Count,
                    assetId,
                    tenantId);

                return new AssetTimelineListResponse
                {
                    Success = true,
                    Message = AssetMessages.TimelineFetchedSuccessfully,
                    Data = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(
                    ExceptionCodes.AssetMaintenance.GetTimeline,
                    nameof(GetAssetMaintenanceTimelineAsync),
                    ex,
                    _tenantContext.UserId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AssetOperationResponse> DeleteAsync(int id, string? ipAddress)
        {
            var tenantId = _tenantContext.GetRequiredOrganisationId();
            var userId = _tenantContext.GetRequiredUserId();

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var record = await connection.QueryFirstOrDefaultAsync<AssetMaintenanceDeleteRow>(
                    _queries.Get("AssetMaintenance_GetForDelete"),
                    new { Id = id, TenantId = tenantId },
                    transaction);

                if (record == null)
                    throw new AssetNotFoundException(AssetMessages.MaintenanceNotFound);

                var rowsAffected = await connection.ExecuteAsync(
                    _queries.Get("AssetMaintenance_DeleteById"),
                    new { Id = id, TenantId = tenantId },
                    transaction);

                if (rowsAffected == 0)
                    throw new AssetNotFoundException(AssetMessages.MaintenanceNotFound);

                transaction.Commit();

                _logger.LogInformation(
                    LogMessages.AssetMaintenance.Deleted,
                    record.Id,
                    record.AssetId,
                    userId,
                    tenantId,
                    ipAddress ?? "N/A",
                    DateTime.UtcNow);

                return new AssetOperationResponse
                {
                    Success = true,
                    Message = AssetMessages.MaintenanceDeletedSuccessfully
                };
            }
            catch (AssetNotFoundException)
            {
                transaction.Rollback();
                throw;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogException(
                    ExceptionCodes.AssetMaintenance.Delete,
                    nameof(DeleteAsync),
                    ex,
                    userId);
                throw;
            }
        }

        private static string BuildOrderByClause(string? sortBy, string? sortDirection)
        {
            var column = !string.IsNullOrWhiteSpace(sortBy) && SortableColumns.TryGetValue(sortBy.Trim(), out var mapped)
                ? mapped
                : "Date";

            var direction = string.Equals(sortDirection?.Trim(), "asc", StringComparison.OrdinalIgnoreCase)
                ? "ASC"
                : "DESC";

            if (string.Equals(column, "Id", StringComparison.OrdinalIgnoreCase))
                return $"[Id] {direction}";

            return $"[{column}] {direction}, [Id] DESC";
        }

		private static AssetMaintenanceDto MapToDto(AssetMaintenanceRow row) => new()
		{
			Id = row.Id,
			AssetId = row.AssetId,
			Cost = row.Cost,
			Attachment = AttachmentJsonHelper.Deserialize(row.Attachment),
			Date = row.Date,
			ResponsiblePerson = row.ResponsiblePerson,
			AssetNumber = row.AssetNumber,
			AssetName = row.AssetName,
			AssetDescription = row.AssetDescription
		};

        private sealed class AssetMaintenanceDeleteRow
        {
            public int Id { get; set; }
            public int AssetId { get; set; }
        }

		/// <summary>
		/// Raw database projection where Attachment is the stored JSON string,
		/// mapped to <see cref="AssetMaintenanceDto"/> via <see cref="MapToDto"/>.
		/// </summary>
		private sealed class AssetMaintenanceRow
		{
			public int Id { get; set; }
			public int AssetId { get; set; }
			public decimal? Cost { get; set; }
			public string? Attachment { get; set; }
			public DateTime? Date { get; set; }
			public string? ResponsiblePerson { get; set; }
			public string? AssetNumber { get; set; }
			public string? AssetName { get; set; }
			public string? AssetDescription { get; set; }
		}
	}
}