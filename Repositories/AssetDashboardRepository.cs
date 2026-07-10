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
    /// Provides tenant-scoped asset dashboard data using Dapper.
    /// Business logic mirrors the web Asset Dashboard repository.
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

                var kpiRow = await QueryKpiMetricsAsync(connection, tenantId, startOfMonth);

                return new AssetDashboardResponse
                {
                    TotalAssets = BuildKpi(kpiRow.TotalCount, kpiRow.TotalPrevCount),
                    AssetsInUse = BuildKpi(kpiRow.InUseCount, kpiRow.InUsePrevCount),
                    UnderMaintenance = BuildKpi(kpiRow.MaintenanceCount, kpiRow.MaintenancePrevCount),
                    OutOfService = BuildKpi(kpiRow.OutOfServiceCount, kpiRow.OutOfServicePrevCount),
                    CategoryBreakdown = await BuildCategoryBreakdownAsync(connection, tenantId),
                    BranchBreakdown = await BuildBranchBreakdownAsync(connection, tenantId)
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
            DateTime startOfMonth)
        {
            var outOfService = _queries.Get("AssetDashboard_StatusOutOfService");
            var sql = _queries.Get("AssetDashboard_GetKpiMetrics")
                .Replace("{IN_USE}", _queries.Get("AssetDashboard_StatusInUse"))
                .Replace("{OUT_OF_SERVICE}", outOfService)
                .Replace("{MAINTENANCE}", _queries.Get("AssetDashboard_StatusMaintenance")
                    .Replace("{OUT_OF_SERVICE}", outOfService));

            var result = await connection.QueryFirstOrDefaultAsync<KpiMetricsRow>(sql, new
            {
                TenantId = tenantId,
                StartOfMonth = startOfMonth
            });

            return result ?? new KpiMetricsRow();
        }

        private async Task<List<AssetCategoryBreakdownDto>> BuildCategoryBreakdownAsync(
            System.Data.IDbConnection connection,
            int tenantId)
        {
            var sql = _queries.Get("AssetDashboard_GetCategoryBreakdown");
            var rows = (await connection.QueryAsync<CategoryRow>(sql, new { TenantId = tenantId })).ToList();
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
            int tenantId)
        {
            var sql = _queries.Get("AssetDashboard_GetBranchBreakdown");
            var rows = (await connection.QueryAsync<BranchRow>(sql, new { TenantId = tenantId })).ToList();
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
