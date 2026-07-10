namespace MobileWebApi.Models
{
    /// <summary>
    /// Represents asset distribution for a single category.
    /// </summary>
    public class AssetCategoryBreakdownDto
    {
        /// <summary>
        /// Asset category identifier. Zero when the category is unassigned.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Asset category display name.
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Number of assets in the category.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Share of the displayed category breakdown.
        /// </summary>
        public double Percentage { get; set; }
    }
}
