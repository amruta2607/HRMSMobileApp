namespace MobileWebApi.Models.Requests
{
    /// <summary>
    /// Request payload for updating an existing asset handover record.
    /// </summary>
    public class UpdateAssetHandoverRequest
    {
        public DateTime HandoverDate { get; set; }

        public int HandoverToEmployeeId { get; set; }

        public string? Location { get; set; }

        public string? Remarks { get; set; }
    }
}
