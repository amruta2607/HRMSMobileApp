namespace MobileWebApi.Models
{
    /// <summary>
    /// Represents a single asset dashboard KPI metric with trend information.
    /// </summary>
    public class AssetKpiDto
    {
        /// <summary>
        /// Current count for the KPI.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Absolute month-over-month trend percentage.
        /// </summary>
        public double TrendPercent { get; set; }

        /// <summary>
        /// Indicates whether the trend is positive or flat.
        /// </summary>
        public bool IsUp { get; set; }
    }
}
