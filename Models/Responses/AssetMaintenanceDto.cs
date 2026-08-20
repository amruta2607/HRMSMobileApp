namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Represents a single asset maintenance record for the current tenant.
    /// </summary>
    public class AssetMaintenanceDto
    {
        /// <summary>
        /// Maintenance record identifier.
        /// </summary>
        public int HistoryId { get; set; }

        /// <summary>
        /// Related asset identifier.
        /// </summary>
        public int AssetId { get; set; }

        /// <summary>
        /// Maintenance cost.
        /// </summary>
        public decimal? Cost { get; set; }

        /// <summary>
        /// Uploaded attachment references, deserialized from the stored Attachment JSON.
        /// Null when the record has no attachments.
        /// </summary>
        public List<FileAttachment>? Attachment { get; set; }

        /// <summary>
        /// Date the maintenance took place.
        /// </summary>
        public DateTime? Date { get; set; }

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
