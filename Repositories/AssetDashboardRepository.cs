using System.Diagnostics;
using Dapper;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Repositories.Interfaces;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
    /// <summary>
    /// Provides asset dashboard data using Dapper.
    /// Role dashboard uses the same response shape with work-role visibility filters.
    /// </summary>
    public class AssetDashboardRepository : IAssetDashboardRepository
    {
        private readonly DapperContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly QueryProvider _queries;
        private readonly ILogger<AssetDashboardRepository> _logger;

        public AssetDashboardRepository(
            DapperContext context,
            ITenantContext tenantContext,
            QueryProvider queries,
            ILogger<AssetDashboardRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<AssetDashboardResponse> GetDashboardAsync()
        {
            try
            {
                var tenantId = _tenantContext.GetRequiredOrganisationId();
                var referenceDate = DateTime.Today;
                var startOfMonth = new DateTime(referenceDate.Year, referenceDate.Month, 1);

                using var connection = _context.CreateConnection();

                var kpiRow = await QueryKpiMetricsAsync(
                    connection,
                    tenantId,
                    employeeId: null,
                    startOfMonth,
                    scopeFilter: "a.TenantId = @TenantId",
                    useScopedQuery: false);

                return new AssetDashboardResponse
                {
                    TotalAssets = BuildKpi(kpiRow.TotalCount, kpiRow.TotalPrevCount),
                    AssetsInUse = BuildKpi(kpiRow.InUseCount, kpiRow.InUsePrevCount),
                    UnderMaintenance = BuildKpi(kpiRow.MaintenanceCount, kpiRow.MaintenancePrevCount),
                    OutOfService = BuildKpi(kpiRow.OutOfServiceCount, kpiRow.OutOfServicePrevCount),
                    CategoryBreakdown = await BuildCategoryBreakdownAsync(
                        connection, tenantId, employeeId: null, "a.TenantId = @TenantId", useScopedQuery: false),
                    BranchBreakdown = await BuildBranchBreakdownAsync(
                        connection, tenantId, employeeId: null, "a.TenantId = @TenantId", useScopedQuery: false)
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(
                    ExceptionCodes.AssetDashboard.GetDashboard,
                    nameof(GetDashboardAsync),
                    ex,
                    _tenantContext.UserId);

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AssetDashboardResponse> GetRoleDashboardAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var userId = _tenantContext.GetRequiredUserId();
            var tenantId = _tenantContext.GetRequiredOrganisationId();

            try
            {
                using var connection = _context.CreateConnection();

                var assignedRoleNames = (await connection.QueryAsync<string>(
                    _queries.Get("GetActiveWorkRolesByUserId"),
                    new { UserId = userId })).ToList();

                var workRoleNames = WorkRoleHelper.BuildLoginWorkRoles(assignedRoleNames);
                var primaryWorkRole = WorkRoleHelper.ResolvePrimaryWorkRole(workRoleNames);
                var scope = WorkRoleHelper.ResolveDashboardAccessScope(primaryWorkRole);

                var scopeFilter = scope == DashboardAccessScope.AllTenants
                    ? "1 = 1"
                    : "a.TenantId = @TenantId";

                var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                var kpiRow = await QueryKpiMetricsAsync(
                    connection,
                    tenantId,
                    employeeId: null,
                    startOfMonth,
                    scopeFilter,
                    useScopedQuery: true);

                var response = new AssetDashboardResponse
                {
                    TotalAssets = BuildKpi(kpiRow.TotalCount, kpiRow.TotalPrevCount),
                    AssetsInUse = BuildKpi(kpiRow.InUseCount, kpiRow.InUsePrevCount),
                    UnderMaintenance = BuildKpi(kpiRow.MaintenanceCount, kpiRow.MaintenancePrevCount),
                    OutOfService = BuildKpi(kpiRow.OutOfServiceCount, kpiRow.OutOfServicePrevCount),
                    CategoryBreakdown = await BuildCategoryBreakdownAsync(
                        connection, tenantId, employeeId: null, scopeFilter, useScopedQuery: true),
                    BranchBreakdown = await BuildBranchBreakdownAsync(
                        connection, tenantId, employeeId: null, scopeFilter, useScopedQuery: true)
                };

                stopwatch.Stop();
                _logger.LogInformation(
                    LogMessages.Dashboard.StatsLoaded,
                    userId,
                    tenantId,
                    primaryWorkRole,
                    stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogException(
                    ExceptionCodes.Dashboard.GetDashboard,
                    nameof(GetRoleDashboardAsync),
                    ex,
                    userId);
                throw;
            }
        }

        private static AssetKpiDto BuildKpi(int current, int previous)
        {
            var trend = previous <= 0
                ? (current > 0 ? 100.0 : 0.0)
                : Math.Round((current - previous) * 100.0 / previous, 1);

            return new AssetKpiDto
            {
                Count = current,
                TrendPercent = Math.Abs(trend),
                IsUp = trend >= 0
            };
        }

        private async Task<KpiMetricsRow> QueryKpiMetricsAsync(
            System.Data.IDbConnection connection,
            int tenantId,
            int? employeeId,
            DateTime startOfMonth,
            string scopeFilter,
            bool useScopedQuery)
        {
            var outOfService = _queries.Get("AssetDashboard_StatusOutOfService");
            var sqlKey = useScopedQuery
                ? "AssetDashboard_GetKpiMetricsScoped"
                : "AssetDashboard_GetKpiMetrics";

            var sql = _queries.Get(sqlKey)
                .Replace("{SCOPE_FILTER}", scopeFilter)
                .Replace("{IN_USE}", _queries.Get("AssetDashboard_StatusInUse"))
                .Replace("{OUT_OF_SERVICE}", outOfService)
                .Replace("{MAINTENANCE}", _queries.Get("AssetDashboard_StatusMaintenance")
                    .Replace("{OUT_OF_SERVICE}", outOfService));

            var result = await connection.QueryFirstOrDefaultAsync<KpiMetricsRow>(sql, new
            {
                TenantId = tenantId,
                EmployeeId = employeeId,
                StartOfMonth = startOfMonth
            });

            return result ?? new KpiMetricsRow();
        }

        private async Task<List<AssetCategoryBreakdownDto>> BuildCategoryBreakdownAsync(
            System.Data.IDbConnection connection,
            int tenantId,
            int? employeeId,
            string scopeFilter,
            bool useScopedQuery)
        {
            var sqlKey = useScopedQuery
                ? "AssetDashboard_GetCategoryBreakdownScoped"
                : "AssetDashboard_GetCategoryBreakdown";

            var sql = _queries.Get(sqlKey).Replace("{SCOPE_FILTER}", scopeFilter);
            var rows = (await connection.QueryAsync<CategoryRow>(sql, new
            {
                TenantId = tenantId,
                EmployeeId = employeeId
            })).ToList();

            var total = rows.Sum(r => r.Count);
            var safeTotal = Math.Max(total, 1);

            return rows.Select(row => new AssetCategoryBreakdownDto
            {
                CategoryId = row.CategoryId,
                CategoryName = row.CategoryName,
                Count = row.Count,
                Percentage = Math.Round(row.Count * 100.0 / safeTotal, 1)
            }).ToList();
        }

        private async Task<List<AssetBranchBreakdownDto>> BuildBranchBreakdownAsync(
            System.Data.IDbConnection connection,
            int tenantId,
            int? employeeId,
            string scopeFilter,
            bool useScopedQuery)
        {
            var sqlKey = useScopedQuery
                ? "AssetDashboard_GetBranchBreakdownScoped"
                : "AssetDashboard_GetBranchBreakdown";

            var sql = _queries.Get(sqlKey).Replace("{SCOPE_FILTER}", scopeFilter);
            var rows = (await connection.QueryAsync<BranchRow>(sql, new
            {
                TenantId = tenantId,
                EmployeeId = employeeId
            })).ToList();

            var total = rows.Sum(r => r.Count);
            var safeTotal = Math.Max(total, 1);

            return rows.Select(row => new AssetBranchBreakdownDto
            {
                BranchId = row.BranchId,
                BranchName = row.BranchName,
                Count = row.Count,
                Percentage = Math.Round(row.Count * 100.0 / safeTotal, 1)
            }).ToList();
        }

        private sealed class KpiMetricsRow
        {
            public int TotalCount { get; set; }
            public int TotalPrevCount { get; set; }
            public int InUseCount { get; set; }
            public int InUsePrevCount { get; set; }
            public int MaintenanceCount { get; set; }
            public int MaintenancePrevCount { get; set; }
            public int OutOfServiceCount { get; set; }
            public int OutOfServicePrevCount { get; set; }
        }

        private sealed class CategoryRow
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        private sealed class BranchRow
        {
            public int BranchId { get; set; }
            public string BranchName { get; set; } = string.Empty;
            public int Count { get; set; }
        }
    }
}
