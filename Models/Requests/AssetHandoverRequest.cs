using System.ComponentModel.DataAnnotations;

namespace MobileWebApi.Models.Requests
{
    /// <summary>
    /// Request payload for transferring an asset to another employee.
    /// Only database NOT NULL fields are validated; Description is nullable.
    /// </summary>
    public class AssetHandoverRequest
    {
        /// <summary>
        /// Asset to hand over.
        /// </summary>
        [Range(1, 999999, ErrorMessage = "Asset id is required.")]
        public int AssetId { get; set; }

        /// <summary>
        /// Employee who is handing over the asset (from employee lookup).
        /// Stored in AssetHandOver.HandOverById.
        /// </summary>
        [Range(1, 999999, ErrorMessage = "Handover by employee id is required.")]
        public int HandoverByEmployeeId { get; set; }

        /// <summary>
        /// Employee receiving the asset.
        /// </summary>
        [Range(1, 999999, ErrorMessage = "Handover to employee id is required.")]
        public int HandoverToEmployeeId { get; set; }

        /// <summary>
        /// Handover date (NOT NULL).
        /// </summary>
        [Required(ErrorMessage = "Handover date is required.")]
        public DateTime HandoverDate { get; set; }

        public string? Location { get; set; }

        /// <summary>
        /// Maps to AssetHandOver.Description (nullable).
        /// </summary>
        public string? Remarks { get; set; }
    }
}
