using Dapper;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models.Requests;
using MobileWebApi.Models.Responses;
using MobileWebApi.Repositories.Interfaces;
using MobileWebApi.Resources;
using MobileWebApi.Services;
using System.Data;
using System.Data.SqlClient;

namespace MobileWebApi.Repositories
{
    /// <summary>
    /// Provides tenant-scoped asset data access using Dapper.
    /// </summary>
    public class AssetRepository : IAssetRepository
    {
        private readonly DapperContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly IAssetQRCodeService _qrCodeService;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly QueryProvider _queries;
        private readonly ILogger<AssetRepository> _logger;

        public AssetRepository(
            DapperContext context,
            ITenantContext tenantContext,
            IAssetQRCodeService qrCodeService,
            IEmployeeRepository employeeRepository,
            QueryProvider queries,
            ILogger<AssetRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<AssetListResponse> GetAssetsAsync()
        {
            try
            {
                var tenantId = _tenantContext.GetRequiredOrganisationId();
                var sql = _queries.Get("Asset_GetList");

                using var connection = _context.CreateConnection();
                var assets = (await connection.QueryAsync<AssetDto>(sql, new { TenantId = tenantId })).ToList();

                return new AssetListResponse
                {
                    Assets = assets
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(
                    ExceptionCodes.Asset.GetList,
                    nameof(GetAssetsAsync),
                    ex,
                    _tenantContext.UserId);

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AssetLookupsResponse> GetLookupsAsync()
        {
            try
            {
                var tenantId = _tenantContext.GetRequiredOrganisationId();
                var userId = _tenantContext.UserId;

                _logger.LogInformation(
                    LogMessages.Asset.FetchingLookups,
                    userId,
                    tenantId);

                using var connection = _context.CreateConnection();

                var statuses = (await connection.QueryAsync<AssetLookupItemDto>(
                    _queries.Get("Asset_LookupAssetStatuses"),
                    new { TenantId = tenantId })).ToList();
                var categories = (await connection.QueryAsync<AssetLookupItemDto>(
                    _queries.Get("Asset_LookupAssetCategories"),
                    new { TenantId = tenantId })).ToList();
                var departments = (await connection.QueryAsync<AssetLookupItemDto>(
                    _queries.Get("Asset_LookupDepartments"),
                    new { TenantId = tenantId })).ToList();
                var branches = (await connection.QueryAsync<AssetLookupItemDto>(
                    _queries.Get("Asset_LookupBranches"),
                    new { TenantId = tenantId })).ToList();
                var businessUnits = (await connection.QueryAsync<AssetLookupItemDto>(
                    _queries.Get("Asset_LookupBusinessUnits"),
                    new { TenantId = tenantId })).ToList();
                var assetTypes = (await connection.QueryAsync<AssetLookupItemDto>(
                    _queries.Get("Asset_LookupAssetTypes"),
                    new { TenantId = tenantId })).ToList();

                var data = new AssetLookupsData
                {
                    AssetStatuses = statuses,
                    AssetCategories = categories,
                    Departments = departments,
                    Branches = branches,
                    BusinessUnits = businessUnits,
                    AssetTypes = assetTypes
                };

                _logger.LogInformation(
                    LogMessages.Asset.LookupsFetched,
                    tenantId,
                    data.AssetStatuses.Count,
                    data.AssetCategories.Count,
                    data.Departments.Count,
                    data.Branches.Count,
                    data.BusinessUnits.Count,
                    data.AssetTypes.Count);

                return new AssetLookupsResponse
                {
                    Success = true,
                    Message = AssetMessages.LookupsFetchedSuccessfully,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(
                    ExceptionCodes.Asset.GetLookups,
                    nameof(GetLookupsAsync),
                    ex,
                    _tenantContext.UserId);

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<CreateAssetResponse> CreateAssetAsync(CreateAssetRequest request)
        {
            var tenantId = _tenantContext.GetRequiredOrganisationId();
            var userId = _tenantContext.GetRequiredUserId();

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var description = OptionalValueHelper.NullIfEmpty(request.Description);
                var owner = OptionalValueHelper.NullIfEmpty(request.Owner);
                var location = OptionalValueHelper.NullIfEmpty(request.Location);
                var purchaseOrderNumber = OptionalValueHelper.NullIfEmpty(request.PurchaseOrderNumber);
                var purchaseOrderBill = OptionalValueHelper.NullIfEmpty(request.PurchaseOrderBill);
                var supportCenter = OptionalValueHelper.NullIfEmpty(request.SupportCenter);
                var manufacturer = OptionalValueHelper.NullIfEmpty(request.Manufacturer);
                var model = OptionalValueHelper.NullIfEmpty(request.Model);
                var serialNumber = OptionalValueHelper.NullIfEmpty(request.SerialNumber);
                var images = OptionalValueHelper.NullIfEmpty(request.Images);
                var assetTagNumber = OptionalValueHelper.NullIfEmpty(request.AssetTagNumber);
                var assetName = OptionalValueHelper.NullIfEmpty(request.AssetName);

                var assetStatusId = OptionalValueHelper.NullIfNonPositive(request.AssetStatusId);
                var assetCategoryId = OptionalValueHelper.NullIfNonPositive(request.AssetCategoryId);
                var departmentId = OptionalValueHelper.NullIfNonPositive(request.DepartmentId);
                var branchId = OptionalValueHelper.NullIfNonPositive(request.BranchId);
                var businessUnitId = OptionalValueHelper.NullIfNonPositive(request.BusinessUnitId);
                var assetTypeId = OptionalValueHelper.NullIfNonPositive(request.AssetTypeId);
                var productionYear = OptionalValueHelper.NullIfNonPositive(request.ProductionYear);

                var purchaseDate = OptionalValueHelper.NullIfDefault(request.PurchaseDate)
                    ?? throw new AssetValidationException(AssetMessages.PurchaseDateRequired);

                if (request.PurchasePrice < 0)
                    throw new AssetValidationException(AssetMessages.PurchasePriceRequired);

                var purchasePriceDb = (double)request.PurchasePrice;
                double? actualValue = request.ActualValue.HasValue && request.ActualValue.Value > 0
                    ? (double?)request.ActualValue.Value
                    : purchasePriceDb;

                var warrantyExpiryDate = OptionalValueHelper.NullIfDefault(request.WarrantyExpiryDate);
                var maintenanceDueDate = OptionalValueHelper.NullIfDefault(request.MaintenanceDueDate);

                double? depreciationPercentage = OptionalValueHelper.NullIfNonPositive(
                    request.DepreciationPercentage.HasValue
                        ? (double?)request.DepreciationPercentage.Value
                        : null);

                if (!depreciationPercentage.HasValue && assetCategoryId.HasValue)
                {
                    var categoryPercentage = AssetDepreciationHelper.GetCategoryYearlyPercentage(
                        connection, assetCategoryId.Value, tenantId, _queries, transaction);
                    depreciationPercentage = OptionalValueHelper.NullIfNonPositive(categoryPercentage);
                }

                await ValidateOptionalLookupsAsync(
                    connection,
                    transaction,
                    assetStatusId,
                    assetCategoryId,
                    departmentId,
                    branchId,
                    businessUnitId,
                    assetTypeId,
                    tenantId);

                // Duplicate tag check only for real tag values; blank/null skips and stores NULL.
                if (!string.IsNullOrEmpty(assetTagNumber))
                {
                    await EnsureAssetTagNumberIsUniqueAsync(
                        connection,
                        transaction,
                        assetTagNumber,
                        tenantId,
                        excludeAssetId: null);
                }

                var number = AssetNumberHelper.GenerateNextNumber(connection, tenantId, _queries, transaction);
                var assetCode = _qrCodeService.GenerateAssetCode(connection, tenantId, transaction);
                int? responsibleEmployeeId = null;

                if (request.MaintenanceList.Count > 0)
                {
                    var employee = await _employeeRepository.GetEmployeebyUserIdAsync(userId);
                    if (employee == null)
                        throw new AssetValidationException(AssetMessages.EmployeeRequiredForMaintenance);

                    responsibleEmployeeId = employee.Id;
                }

                var insertAssetSql = _queries.Get("Asset_Insert");
                var assetId = await connection.ExecuteScalarAsync<int>(insertAssetSql, new
                {
                    Number = number,
                    Description = description,
                    PurchaseDate = purchaseDate,
                    PurchasePrice = purchasePriceDb,
                    PurchaseOrderNumber = purchaseOrderNumber,
                    PurchaseOrderBill = purchaseOrderBill,
                    SupportCenter = supportCenter,
                    Manufacturer = manufacturer,
                    Model = model,
                    SerialNumber = serialNumber,
                    ProductionYear = productionYear,
                    Images = images,
                    AssetTagNumber = assetTagNumber,
                    InsertUserId = userId,
                    TenantId = tenantId,
                    AssetStatusId = assetStatusId,
                    CategoryId = (int?)null,
                    DepartmentId = departmentId,
                    AssetName = assetName,
                    AssetCategoryId = assetCategoryId,
                    ActualValue = actualValue,
                    WarrantyExpiryDate = warrantyExpiryDate,
                    MaintenanceDueDate = maintenanceDueDate,
                    DepreciationPercentage = depreciationPercentage,
                    BranchId = branchId,
                    AssetCode = assetCode,
                    BusinessUnitId = businessUnitId,
                    Location = location,
                    Owner = owner,
                    AssetTypeId = assetTypeId
                }, transaction);

                var insertMaintenanceSql = _queries.Get("Asset_InsertMaintenance");
                foreach (var maintenance in request.MaintenanceList)
                {
                    var maintenanceCost = OptionalValueHelper.NullIfNonPositive(maintenance.Cost);
                    await connection.ExecuteAsync(insertMaintenanceSql, new
                    {
                        AssetId = assetId,
                        Cost = maintenanceCost.HasValue ? (double?)maintenanceCost.Value : null,
                        Attachment = OptionalValueHelper.NullIfEmpty(maintenance.Remarks),
                        Date = OptionalValueHelper.NullIfDefault(maintenance.MaintenanceDate),
                        ResponsiblePerson = responsibleEmployeeId,
                        InsertUserId = userId,
                        TenantId = tenantId
                    }, transaction);
                }

                var qrResult = _qrCodeService.EnsureQRCode(connection, transaction, assetId, tenantId);
                transaction.Commit();

                return new CreateAssetResponse
                {
                    AssetId = assetId,
                    Number = number,
                    AssetCode = qrResult.AssetCode,
                    Message = AssetMessages.CreatedSuccessfully
                };
            }
            catch (AssetValidationException)
            {
                transaction.Rollback();
                throw;
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                transaction.Rollback();
                // Only surface tag-duplicate when a non-empty tag was provided; other unique keys rethrow.
                var attemptedTag = OptionalValueHelper.NullIfEmpty(request.AssetTagNumber);
                if (!string.IsNullOrEmpty(attemptedTag))
                    throw new AssetValidationException(AssetMessages.DuplicateAssetTagNumber);

                throw;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogException(
                    ExceptionCodes.Asset.Create,
                    nameof(CreateAssetAsync),
                    ex,
                    _tenantContext.UserId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AssetOperationResponse> UpdateAssetAsync(int assetId, UpdateAssetRequest request)
        {
            var tenantId = _tenantContext.GetRequiredOrganisationId();
            var userId = _tenantContext.GetRequiredUserId();

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var asset = await connection.QueryFirstOrDefaultAsync<AssetSummaryRow>(
                    _queries.Get("Asset_GetById"),
                    new { AssetId = assetId, TenantId = tenantId },
                    transaction);

                if (asset == null)
                    throw new AssetNotFoundException(AssetMessages.NotFound);

                //var purchaseDate = OptionalValueHelper.NullIfDefault(request.PurchaseDate)
                //    ?? throw new AssetValidationException(AssetMessages.PurchaseDateRequired);

                if (request.PurchasePrice < 0)
                    throw new AssetValidationException(AssetMessages.PurchasePriceRequired);

                var assetName = OptionalValueHelper.NullIfEmpty(request.AssetName);
                var description = OptionalValueHelper.NullIfEmpty(request.Description);
                var owner = OptionalValueHelper.NullIfEmpty(request.Owner);
                var location = OptionalValueHelper.NullIfEmpty(request.Location);
                var manufacturer = OptionalValueHelper.NullIfEmpty(request.Manufacturer);
                var model = OptionalValueHelper.NullIfEmpty(request.Model);
                var serialNumber = OptionalValueHelper.NullIfEmpty(request.SerialNumber);
                var images = OptionalValueHelper.NullIfEmpty(request.Images);
                var assetTagNumber = OptionalValueHelper.NullIfEmpty(request.AssetTagNumber);

                var assetCategoryId = OptionalValueHelper.NullIfNonPositive(request.AssetCategoryId);
                var assetTypeId = OptionalValueHelper.NullIfNonPositive(request.AssetTypeId);
                var assetStatusId = OptionalValueHelper.NullIfNonPositive(request.AssetStatusId);
                var departmentId = OptionalValueHelper.NullIfNonPositive(request.DepartmentId);
                var branchId = OptionalValueHelper.NullIfNonPositive(request.BranchId);
                var businessUnitId = OptionalValueHelper.NullIfNonPositive(request.BusinessUnitId);
                var productionYear = OptionalValueHelper.NullIfNonPositive(request.ProductionYear);
                var purchaseDate = OptionalValueHelper.NullIfDefault(request.PurchaseDate);

				var warrantyExpiryDate = OptionalValueHelper.NullIfDefault(request.WarrantyExpiryDate);
                var maintenanceDueDate = OptionalValueHelper.NullIfDefault(request.MaintenanceDueDate);

                await ValidateOptionalLookupsAsync(
                    connection,
                    transaction,
                    assetStatusId,
                    assetCategoryId,
                    departmentId,
                    branchId,
                    businessUnitId,
                    assetTypeId,
                    tenantId);

                // Duplicate tag check only for real tag values; blank/null skips and stores NULL.
                if (!string.IsNullOrEmpty(assetTagNumber))
                {
                    await EnsureAssetTagNumberIsUniqueAsync(
                        connection,
                        transaction,
                        assetTagNumber,
                        tenantId,
                        excludeAssetId: assetId);
                }

                double? actualValue = OptionalValueHelper.NullIfNonPositive(
                    request.ActualValue.HasValue ? (double?)request.ActualValue.Value : null)
                    ?? (double)request.PurchasePrice;

                var rowsAffected = await connection.ExecuteAsync(
                    _queries.Get("Asset_Update"),
                    new
                    {
                        AssetId = assetId,
                        TenantId = tenantId,
                        AssetName = assetName,
                        AssetCategoryId = assetCategoryId,
                        AssetTypeId = assetTypeId,
                        AssetStatusId = assetStatusId,
                        DepartmentId = departmentId,
                        BranchId = branchId,
                        BusinessUnitId = businessUnitId,
                        Location = location,
                        Owner = owner,
                        Manufacturer = manufacturer,
                        Model = model,
                        SerialNumber = serialNumber,
                        ProductionYear = productionYear,
                        PurchaseDate = purchaseDate,
                        PurchasePrice = (double)request.PurchasePrice,
                        ActualValue = actualValue,
                        WarrantyExpiryDate = warrantyExpiryDate,
                        MaintenanceDueDate = maintenanceDueDate,
                        Description = description,
                        Images = images,
                        AssetTagNumber = assetTagNumber,
                        UpdateUserId = userId
                    },
                    transaction);

                if (rowsAffected == 0)
                    throw new AssetNotFoundException(AssetMessages.NotFound);

                transaction.Commit();

                _logger.LogInformation(
                    LogMessages.Asset.AssetUpdated,
                    assetId,
                    userId,
                    tenantId,
                    DateTime.UtcNow);

                return new AssetOperationResponse
                {
                    Success = true,
                    Message = AssetMessages.UpdatedSuccessfully
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
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                transaction.Rollback();
                var attemptedTag = OptionalValueHelper.NullIfEmpty(request.AssetTagNumber);
                if (!string.IsNullOrEmpty(attemptedTag))
                    throw new AssetValidationException(AssetMessages.DuplicateAssetTagNumber);

                throw;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogException(
                    ExceptionCodes.Asset.Update,
                    nameof(UpdateAssetAsync),
                    ex,
                    userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AssetOperationResponse> DeleteAssetAsync(int assetId, string? ipAddress)
        {
            var tenantId = _tenantContext.GetRequiredOrganisationId();
            var userId = _tenantContext.GetRequiredUserId();

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var asset = await connection.QueryFirstOrDefaultAsync<AssetDeleteSummaryRow>(
                    _queries.Get("Asset_GetSummaryForDelete"),
                    new { AssetId = assetId, TenantId = tenantId },
                    transaction);

                if (asset == null)
                    throw new AssetNotFoundException(AssetMessages.NotFound);

                // Known dependent tables (delete when present even without FK metadata).
                await DeleteOptionalDependentTablesAsync(
                    connection,
                    transaction,
                    assetId,
                    new[]
                    {
                        "AssetMaintenance",
                        "AssetHandOver",
                        "AssetHistory",
                        "AssetMovement",
                        "AssetDocuments",
                        "AssetImages",
                        "AssetAudit",
                        "AssetDepreciation",
                        "AssetAllocation"
                    });

                // Delete any remaining tables that reference Asset via FK.
                var children = (await connection.QueryAsync<ForeignKeyChildRow>(
                    _queries.Get("Asset_GetForeignKeyChildren"),
                    transaction: transaction)).ToList();

                foreach (var child in children)
                {
                    await DeleteFromChildTableAsync(connection, transaction, child.TableName, child.ColumnName, assetId);
                }

                var rowsAffected = await connection.ExecuteAsync(
                    _queries.Get("Asset_DeleteById"),
                    new { AssetId = assetId, TenantId = tenantId },
                    transaction);

                if (rowsAffected == 0)
                    throw new AssetNotFoundException(AssetMessages.NotFound);

                transaction.Commit();

                _logger.LogInformation(
                    LogMessages.Asset.AssetDeleted,
                    asset.Id,
                    asset.Number,
                    asset.AssetName,
                    userId,
                    tenantId,
                    ipAddress ?? "N/A",
                    DateTime.UtcNow);

                return new AssetOperationResponse
                {
                    Success = true,
                    Message = AssetMessages.DeletedSuccessfully
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
                    ExceptionCodes.Asset.Delete,
                    nameof(DeleteAssetAsync),
                    ex,
                    userId);
                throw;
            }
        }

        private static async Task DeleteOptionalDependentTablesAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            int assetId,
            IEnumerable<string> tableNames)
        {
            foreach (var tableName in tableNames)
            {
                if (!IsSafeSqlIdentifier(tableName))
                    continue;

                var exists = await connection.ExecuteScalarAsync<int>(
                    $"SELECT CASE WHEN OBJECT_ID(N'dbo.[{tableName}]', N'U') IS NULL THEN 0 ELSE 1 END",
                    transaction: transaction);

                if (exists != 1)
                    continue;

                await connection.ExecuteAsync(
                    $"DELETE FROM [dbo].[{tableName}] WHERE [AssetId] = @AssetId",
                    new { AssetId = assetId },
                    transaction);
            }
        }

        private static async Task DeleteFromChildTableAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            string tableName,
            string columnName,
            int assetId)
        {
            if (!IsSafeSqlIdentifier(tableName) || !IsSafeSqlIdentifier(columnName))
                return;

            if (tableName.Equals("Asset", StringComparison.OrdinalIgnoreCase))
                return;

            await connection.ExecuteAsync(
                $"DELETE FROM [dbo].[{tableName}] WHERE [{columnName}] = @AssetId",
                new { AssetId = assetId },
                transaction);
        }

        private static bool IsSafeSqlIdentifier(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(value, @"^[A-Za-z0-9_ ]+$");
        }

        private sealed class AssetDeleteSummaryRow
        {
            public int Id { get; set; }
            public string Number { get; set; } = string.Empty;
            public string AssetName { get; set; } = string.Empty;
        }

        private sealed class ForeignKeyChildRow
        {
            public string TableName { get; set; } = string.Empty;
            public string ColumnName { get; set; } = string.Empty;
        }

        /// <summary>
        /// Ensures AssetTagNumber is unique within the tenant. Call only when the tag is non-empty.
        /// </summary>
        private async Task EnsureAssetTagNumberIsUniqueAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            string assetTagNumber,
            int tenantId,
            int? excludeAssetId)
        {
            var sqlKey = excludeAssetId.HasValue
                ? "Asset_ExistsAssetTagNumberExceptId"
                : "Asset_ExistsAssetTagNumber";

            var count = excludeAssetId.HasValue
                ? await connection.ExecuteScalarAsync<int>(
                    _queries.Get(sqlKey),
                    new { TenantId = tenantId, AssetTagNumber = assetTagNumber, AssetId = excludeAssetId.Value },
                    transaction)
                : await connection.ExecuteScalarAsync<int>(
                    _queries.Get(sqlKey),
                    new { TenantId = tenantId, AssetTagNumber = assetTagNumber },
                    transaction);

            if (count > 0)
                throw new AssetValidationException(AssetMessages.DuplicateAssetTagNumber);
        }

        private async Task ValidateOptionalLookupsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            int? assetStatusId,
            int? assetCategoryId,
            int? departmentId,
            int? branchId,
            int? businessUnitId,
            int? assetTypeId,
            int tenantId)
        {
            if (assetStatusId.HasValue &&
                !await ExistsForTenantAsync(
                    connection, transaction, _queries.Get("Asset_ExistsAssetStatus"),
                    assetStatusId.Value, tenantId))
            {
                throw new AssetValidationException(AssetMessages.InvalidAssetStatus);
            }

            if (assetCategoryId.HasValue &&
                !await ExistsForTenantAsync(
                    connection, transaction, _queries.Get("Asset_ExistsAssetCategory"),
                    assetCategoryId.Value, tenantId))
            {
                throw new AssetValidationException(AssetMessages.InvalidAssetCategory);
            }

            if (departmentId.HasValue &&
                !await ExistsForTenantAsync(
                    connection, transaction, _queries.Get("Asset_ExistsDepartment"),
                    departmentId.Value, tenantId))
            {
                throw new AssetValidationException(AssetMessages.InvalidDepartment);
            }

            if (branchId.HasValue &&
                !await ExistsForTenantAsync(
                    connection, transaction, _queries.Get("Asset_ExistsBranch"),
                    branchId.Value, tenantId))
            {
                throw new AssetValidationException(AssetMessages.InvalidBranch);
            }

            if (businessUnitId.HasValue &&
                !await ExistsForTenantAsync(
                    connection, transaction, _queries.Get("Asset_ExistsBusinessUnit"),
                    businessUnitId.Value, tenantId))
            {
                throw new AssetValidationException(AssetMessages.InvalidBusinessUnit);
            }

            if (assetTypeId.HasValue &&
                !await ExistsForTenantAsync(
                    connection, transaction, _queries.Get("Asset_ExistsAssetType"),
                    assetTypeId.Value, tenantId))
            {
                throw new AssetValidationException(AssetMessages.InvalidAssetType);
            }
        }

        private sealed class AssetSummaryRow
        {
            public int Id { get; set; }
            public int? AssetStatusId { get; set; }
            public string AssetStatusName { get; set; } = string.Empty;
            public string Owner { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public int? DepartmentId { get; set; }
            public int? BusinessUnitId { get; set; }
            public int? BranchId { get; set; }
        }

        private static async Task<bool> ExistsForTenantAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            string sql,
            int id,
            int tenantId)
        {
            return await connection.ExecuteScalarAsync<int>(sql, new { Id = id, TenantId = tenantId }, transaction) == 1;
        }
    }
}
