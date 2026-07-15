using System.ComponentModel.DataAnnotations;

namespace MobileWebApi.Models.Requests
{
    /// <summary>
    /// Request payload for transferring an asset to another employee.
    /// </summary>
    public class AssetHandoverRequest
    {
        [Range(1, 999999, ErrorMessage = "Asset id is required.")]
        public int AssetId { get; set; }

        [Range(1, 999999, ErrorMessage = "Handover to employee id is required.")]
        public int HandoverToEmployeeId { get; set; }

        [Required(ErrorMessage = "Handover date is required.")]
        public DateTime HandoverDate { get; set; }

        public string? Location { get; set; }

        public string? Remarks { get; set; }
    }
}
