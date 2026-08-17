using System.ComponentModel.DataAnnotations;

namespace MobileWebApi.Models.Requests
{
    /// <summary>
    /// Request payload for updating an existing asset handover record.
    /// </summary>
    public class UpdateAssetHandoverRequest
    {
        [Required(ErrorMessage = "Handover date is required.")]
        public DateTime HandoverDate { get; set; }

        [Range(1, 999999, ErrorMessage = "Handover to employee id is required.")]
        public int HandoverToEmployeeId { get; set; }

        public string? Location { get; set; }

        public string? Remarks { get; set; }
    }
}
