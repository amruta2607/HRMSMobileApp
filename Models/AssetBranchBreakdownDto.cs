namespace MobileWebApi.Models
{
    /// <summary>
    /// Represents asset distribution for a single branch.
    /// </summary>
    public class AssetBranchBreakdownDto
    {
        /// <summary>
        /// Branch identifier. Zero when the branch is unassigned.
        /// </summary>
        public int BranchId { get; set; }

        /// <summary>
        /// Branch display name.
        /// </summary>
        public string BranchName { get; set; } = string.Empty;

        /// <summary>
        /// Number of assets in the branch.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Share of the displayed branch breakdown.
        /// </summary>
        public double Percentage { get; set; }
    }
}
