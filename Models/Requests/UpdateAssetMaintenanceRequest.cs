using Microsoft.AspNetCore.Http;

namespace MobileWebApi.Models.Requests
{
    /// <summary>
    /// Request payload for updating an existing asset maintenance record (multipart/form-data).
    /// Id, TenantId and insert audit fields (InsertUserId/InsertDate) are never updated;
    /// UpdateUserId/UpdateDate are populated automatically from the authenticated user context.
    /// </summary>
    public class UpdateAssetMaintenanceRequest
    {
        /// <summary>
        /// Identifier of the asset the maintenance record belongs to. Must exist for the current tenant.
        /// </summary>
        public int AssetId { get; set; }

        /// <summary>
        /// Maintenance cost. Must be greater than or equal to 0 when supplied.
        /// </summary>
        public decimal? Cost { get; set; }

        /// <summary>
        /// Optional files to upload for this maintenance record. Uploaded via the existing storage
        /// mechanism; the resulting references are serialized into the Attachment column as JSON.
        /// When provided, replaces the existing attachments; when null/empty, the existing attachments are kept unchanged.
        /// </summary>
        public List<IFormFile>? Attachments { get; set; }

        /// <summary>
        /// Date the maintenance took place. Required.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Person responsible for the maintenance.
        /// </summary>
        public string? ResponsiblePerson { get; set; }

        /// <summary>
        /// Asset number captured on the maintenance record.
        /// </summary>
        public string? AssetNumber { get; set; }

        /// <summary>
        /// Asset name captured on the maintenance record.
        /// </summary>
        public string? AssetName { get; set; }

        /// <summary>
        /// Asset description captured on the maintenance record.
        /// </summary>
        public string? AssetDescription { get; set; }
    }
}
