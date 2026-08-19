namespace MobileWebApi.Models.Requests
{
    /// <summary>
    /// Request payload for transferring an asset to another employee.
    /// </summary>
    public class AssetHandoverRequest
    {
        /// <summary>
        /// Asset to hand over.
        /// </summary>
        public int AssetId { get; set; }

        /// <summary>
        /// Employee who is handing over the asset (from employee lookup).
        /// Stored in AssetHandOver.HandOverById.
        /// </summary>
        public int HandoverByEmployeeId { get; set; }

        /// <summary>
        /// Employee receiving the asset.
        /// </summary>
        public int HandoverToEmployeeId { get; set; }

        /// <summary>
        /// Handover date (NOT NULL).
        /// </summary>
        public DateTime HandoverDate { get; set; }

        public string? Location { get; set; }

        /// <summary>
        /// Maps to AssetHandOver.Description (nullable).
        /// </summary>
        public string? Remarks { get; set; }
    }
}
