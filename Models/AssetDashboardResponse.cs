namespace MobileWebApi.Models
{
    /// <summary>
    /// Tenant-specific asset dashboard response for the mobile application.
    /// </summary>
    public class AssetDashboardResponse
    {
        /// <summary>
        /// Total assets KPI.
        /// </summary>
        public AssetKpiDto TotalAssets { get; set; } = new();

        /// <summary>
        /// Assets in use KPI.
        /// </summary>
        public AssetKpiDto AssetsInUse { get; set; } = new();

        /// <summary>
        /// Assets under maintenance KPI.
        /// </summary>
        public AssetKpiDto UnderMaintenance { get; set; } = new();

        /// <summary>
        /// Out of service assets KPI.
        /// </summary>
        public AssetKpiDto OutOfService { get; set; } = new();

        /// <summary>
        /// Top asset categories for the tenant.
        /// </summary>
        public List<AssetCategoryBreakdownDto> CategoryBreakdown { get; set; } = new();

        /// <summary>
        /// Top asset branches for the tenant.
        /// </summary>
        public List<AssetBranchBreakdownDto> BranchBreakdown { get; set; } = new();
    }
}
