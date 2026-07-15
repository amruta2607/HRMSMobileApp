namespace MobileWebApi.Models.Requests
{
    /// <summary>
    /// Maintenance line item submitted when creating an asset.
    /// </summary>
    public class CreateAssetMaintenanceRequest
    {
        /// <summary>
        /// Maintenance date.
        /// </summary>
        public DateTime? MaintenanceDate { get; set; }

        /// <summary>
        /// Maintenance remarks.
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// Maintenance cost.
        /// </summary>
        public decimal? Cost { get; set; }

        /// <summary>
        /// Optional vendor identifier. Not persisted on AssetMaintenance in the current schema.
        /// </summary>
        public int? VendorId { get; set; }
    }
}
