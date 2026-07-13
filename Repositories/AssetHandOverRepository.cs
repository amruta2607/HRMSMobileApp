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

namespace MobileWebApi.Repositories
{
    /// <summary>
    /// Provides tenant-scoped asset hand over list data using Dapper.
    /// </summary>
    public class AssetHandOverRepository : IAssetHandOverRepository
    {
        private readonly DapperContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly QueryProvider _queries;
        private readonly ILogger<AssetHandOverRepository> _logger;

        public AssetHandOverRepository(
            DapperContext context,
            ITenantContext tenantContext,
            IEmployeeRepository employeeRepository,
            QueryProvider queries,
            ILogger<AssetHandOverRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
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

        /// <inheritdoc />
        public async Task<AssetHandOverLookupsResponse> GetLookupsAsync()
        {
            try
            {
                var tenantId = _tenantContext.GetRequiredOrganisationId();
                var userId = _tenantContext.UserId;

                _logger.LogInformation(
                    LogMessages.AssetHandOver.FetchingLookups,
                    userId,
                    tenantId);

                using var connection = _context.CreateConnection();

                var assets = (await connection.QueryAsync<AssetHandOverLookupAssetDto>(
                    _queries.Get("AssetHandOver_LookupAssets"),
                    new { TenantId = tenantId })).ToList();

                var handOverByEmployees = (await connection.QueryAsync<AssetHandOverLookupEmployeeDto>(
                    _queries.Get("AssetHandOver_LookupHandOverByEmployees"),
                    new { TenantId = tenantId })).ToList();

                var handOverToEmployees = (await connection.QueryAsync<AssetHandOverLookupEmployeeDto>(
                    _queries.Get("AssetHandOver_LookupHandOverToEmployees"),
                    new { TenantId = tenantId })).ToList();

                _logger.LogInformation(
                    LogMessages.AssetHandOver.LookupsFetched,
                    tenantId,
                    assets.Count,
                    handOverByEmployees.Count,
                    handOverToEmployees.Count);

                return new AssetHandOverLookupsResponse
                {
                    Success = true,
                    Message = AssetMessages.HandoverLookupsFetchedSuccessfully,
                    Data = new AssetHandOverLookupsData
                    {
                        Assets = assets,
                        HandOverByEmployees = handOverByEmployees,
                        HandOverToEmployees = handOverToEmployees
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(
                    ExceptionCodes.AssetHandOver.GetLookups,
                    nameof(GetLookupsAsync),
                    ex,
                    _tenantContext.UserId);

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AssetOperationResponse> AssetHandoverAsync(AssetHandoverRequest request)
        {
            var tenantId = _tenantContext.GetRequiredOrganisationId();
            var userId = _tenantContext.GetRequiredUserId();

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var asset = await connection.QueryFirstOrDefaultAsync<AssetHandoverAssetRow>(
                    _queries.Get("Asset_GetById"),
                    new { AssetId = request.AssetId, TenantId = tenantId },
                    transaction);

                if (asset == null)
                    throw new AssetNotFoundException(AssetMessages.NotFound);

                if (AssetHandoverStatusHelper.IsUnavailableForHandover(asset.AssetStatusName))
                    throw new AssetValidationException(AssetMessages.AssetNotAvailableForHandover);

                var handoverToEmployee = await connection.QueryFirstOrDefaultAsync<AssetHandoverEmployeeRow>(
                    _queries.Get("Asset_ValidateEmployee"),
                    new { EmployeeId = request.HandoverToEmployeeId, TenantId = tenantId },
                    transaction);

                if (handoverToEmployee == null)
                    throw new AssetEmployeeNotFoundException(AssetMessages.EmployeeNotFound);

                var handoverByEmployee = await _employeeRepository.GetEmployeebyUserIdAsync(userId);
                if (handoverByEmployee == null || handoverByEmployee.OrganisationId != tenantId)
                    throw new AssetValidationException(AssetMessages.HandoverByEmployeeRequired);

                if (!handoverByEmployee.IsEmployeeActive)
                    throw new AssetValidationException(AssetMessages.InvalidHandOverByEmployee);

                var handoverByExists = await connection.ExecuteScalarAsync<int>(
                    _queries.Get("AssetHandOver_ExistsHandOverByEmployee"),
                    new { EmployeeId = handoverByEmployee.Id, TenantId = tenantId },
                    transaction);

                if (handoverByExists != 1)
                    throw new AssetValidationException(AssetMessages.InvalidHandOverByEmployee);

                if (handoverByEmployee.Id == request.HandoverToEmployeeId)
                    throw new AssetValidationException(AssetMessages.SameHandoverEmployee);

                var lastHandoverToId = await connection.QueryFirstOrDefaultAsync<int?>(
                    _queries.Get("AssetHandOver_GetLastHandoverToId"),
                    new { AssetId = request.AssetId, TenantId = tenantId },
                    transaction);

                if (lastHandoverToId.HasValue && lastHandoverToId.Value == request.HandoverToEmployeeId)
                    throw new AssetValidationException(AssetMessages.SameHandoverEmployee);

                var handoverNumber = AssetHandoverNumberHelper.GenerateNextNumber(
                    connection,
                    tenantId,
                    _queries,
                    transaction);

                var newLocation = string.IsNullOrWhiteSpace(request.Location)
                    ? asset.Location
                    : request.Location.Trim();

                var departmentId = handoverToEmployee.DepartmentId ?? asset.DepartmentId;
                var branchId = handoverToEmployee.BranchId ?? asset.BranchId;
                var businessUnitId = asset.BusinessUnitId;

                await connection.ExecuteScalarAsync<int>(
                    _queries.Get("AssetHandOver_Insert"),
                    new
                    {
                        Number = handoverNumber,
                        Description = request.Remarks,
                        HandOverDate = request.HandoverDate,
                        HandOverById = handoverByEmployee.Id,
                        HandOverToId = request.HandoverToEmployeeId,
                        AssetId = request.AssetId,
                        InsertUserId = userId,
                        TenantId = tenantId,
                        BusinessUnitId = businessUnitId,
                        Location = newLocation
                    },
                    transaction);

                var rowsAffected = await connection.ExecuteAsync(
                    _queries.Get("Asset_UpdateAfterHandover"),
                    new
                    {
                        AssetId = request.AssetId,
                        TenantId = tenantId,
                        Owner = handoverToEmployee.Name,
                        Location = newLocation,
                        DepartmentId = departmentId,
                        BranchId = branchId,
                        BusinessUnitId = businessUnitId,
                        UpdateUserId = userId
                    },
                    transaction);

                if (rowsAffected == 0)
                    throw new AssetNotFoundException(AssetMessages.NotFound);

                transaction.Commit();

                _logger.LogInformation(
                    LogMessages.AssetHandOver.AssetHandedOver,
                    request.AssetId,
                    request.HandoverToEmployeeId,
                    userId,
                    tenantId,
                    DateTime.UtcNow);

                return new AssetOperationResponse
                {
                    Success = true,
                    Message = AssetMessages.HandedOverSuccessfully
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
            catch (AssetEmployeeNotFoundException)
            {
                transaction.Rollback();
                throw;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogException(
                    ExceptionCodes.AssetHandOver.Handover,
                    nameof(AssetHandoverAsync),
                    ex,
                    userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AssetOperationResponse> UpdateAssetHandoverAsync(int handoverId, UpdateAssetHandoverRequest request)
        {
            var tenantId = _tenantContext.GetRequiredOrganisationId();
            var userId = _tenantContext.GetRequiredUserId();

            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var handover = await connection.QueryFirstOrDefaultAsync<AssetHandoverRecordRow>(
                    _queries.Get("AssetHandOver_GetById"),
                    new { Id = handoverId, TenantId = tenantId },
                    transaction);

                if (handover == null)
                    throw new AssetHandoverNotFoundException(AssetMessages.HandoverNotFound);

                var handoverToEmployee = await connection.QueryFirstOrDefaultAsync<AssetHandoverEmployeeRow>(
                    _queries.Get("Asset_ValidateEmployee"),
                    new { EmployeeId = request.HandoverToEmployeeId, TenantId = tenantId },
                    transaction);

                if (handoverToEmployee == null)
                    throw new AssetEmployeeNotFoundException(AssetMessages.EmployeeNotFound);

                var newLocation = string.IsNullOrWhiteSpace(request.Location)
                    ? handover.Location
                    : request.Location.Trim();

                var rowsAffected = await connection.ExecuteAsync(
                    _queries.Get("AssetHandOver_Update"),
                    new
                    {
                        Id = handoverId,
                        TenantId = tenantId,
                        Description = request.Remarks,
                        HandOverDate = request.HandoverDate,
                        HandOverToId = request.HandoverToEmployeeId,
                        Location = newLocation,
                        UpdateUserId = userId
                    },
                    transaction);

                if (rowsAffected == 0)
                    throw new AssetHandoverNotFoundException(AssetMessages.HandoverNotFound);

                var latestHandoverId = await connection.QueryFirstOrDefaultAsync<int?>(
                    _queries.Get("AssetHandOver_GetLatestId"),
                    new { AssetId = handover.AssetId, TenantId = tenantId },
                    transaction);

                if (latestHandoverId == handoverId)
                {
                    var asset = await connection.QueryFirstOrDefaultAsync<AssetHandoverAssetRow>(
                        _queries.Get("Asset_GetById"),
                        new { AssetId = handover.AssetId, TenantId = tenantId },
                        transaction);

                    if (asset != null)
                    {
                        await connection.ExecuteAsync(
                            _queries.Get("Asset_UpdateAfterHandover"),
                            new
                            {
                                AssetId = handover.AssetId,
                                TenantId = tenantId,
                                Owner = handoverToEmployee.Name,
                                Location = newLocation,
                                DepartmentId = handoverToEmployee.DepartmentId ?? asset.DepartmentId,
                                BranchId = handoverToEmployee.BranchId ?? asset.BranchId,
                                BusinessUnitId = handover.BusinessUnitId ?? asset.BusinessUnitId,
                                UpdateUserId = userId
                            },
                            transaction);
                    }
                }

                transaction.Commit();

                return new AssetOperationResponse
                {
                    Success = true,
                    Message = AssetMessages.HandoverUpdatedSuccessfully
                };
            }
            catch (AssetValidationException)
            {
                transaction.Rollback();
                throw;
            }
            catch (AssetHandoverNotFoundException)
            {
                transaction.Rollback();
                throw;
            }
            catch (AssetEmployeeNotFoundException)
            {
                transaction.Rollback();
                throw;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogException(
                    ExceptionCodes.AssetHandOver.Update,
                    nameof(UpdateAssetHandoverAsync),
                    ex,
                    userId);
                throw;
            }
        }

        private sealed class AssetHandoverRecordRow
        {
            public int Id { get; set; }
            public int AssetId { get; set; }
            public int? HandOverToId { get; set; }
            public string Location { get; set; } = string.Empty;
            public int? BusinessUnitId { get; set; }
        }

        private sealed class AssetHandoverAssetRow
        {
            public int Id { get; set; }
            public string AssetStatusName { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public int? DepartmentId { get; set; }
            public int? BusinessUnitId { get; set; }
            public int? BranchId { get; set; }
        }

        private sealed class AssetHandoverEmployeeRow
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int? DepartmentId { get; set; }
            public int? BranchId { get; set; }
        }
    }
}
