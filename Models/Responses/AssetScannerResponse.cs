namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Complete asset details returned by the scanner API.
    /// </summary>
    public class AssetScannerResponse
    {
        /// <summary>
        /// Asset identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Asset number.
        /// </summary>
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Asset code.
        /// </summary>
        public string AssetCode { get; set; } = string.Empty;

        /// <summary>
        /// Asset display name.
        /// </summary>
        public string AssetName { get; set; } = string.Empty;

        /// <summary>
        /// Asset tag number.
        /// </summary>
        public string AssetTagNumber { get; set; } = string.Empty;

        /// <summary>
        /// Asset status identifier.
        /// </summary>
        public int? AssetStatusId { get; set; }

        /// <summary>
        /// Asset status name.
        /// </summary>
        public string AssetStatus { get; set; } = string.Empty;

        /// <summary>
        /// Asset category identifier.
        /// </summary>
        public int? AssetCategoryId { get; set; }

        /// <summary>
        /// Asset category name.
        /// </summary>
        public string AssetCategory { get; set; } = string.Empty;

        /// <summary>
        /// Asset type identifier.
        /// </summary>
        public int? AssetTypeId { get; set; }

        /// <summary>
        /// Asset type name.
        /// </summary>
        public string AssetType { get; set; } = string.Empty;

        /// <summary>
        /// Asset owner.
        /// </summary>
        public string Owner { get; set; } = string.Empty;

        /// <summary>
        /// Department name.
        /// </summary>
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// Branch name.
        /// </summary>
        public string Branch { get; set; } = string.Empty;

        /// <summary>
        /// Business unit name.
        /// </summary>
        public string BusinessUnit { get; set; } = string.Empty;

        /// <summary>
        /// Asset location.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Purchase date.
        /// </summary>
        public DateTime? PurchaseDate { get; set; }

        /// <summary>
        /// Purchase price.
        /// </summary>
        public decimal? PurchasePrice { get; set; }

        /// <summary>
        /// Current actual value.
        /// </summary>
        public decimal? ActualValue { get; set; }

        /// <summary>
        /// Manufacturer name.
        /// </summary>
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        /// Model name.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Serial number.
        /// </summary>
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// Production year.
        /// </summary>
        public int? ProductionYear { get; set; }

        /// <summary>
        /// Next maintenance due date.
        /// </summary>
        public DateTime? MaintenanceDueDate { get; set; }

        /// <summary>
        /// Warranty expiry date.
        /// </summary>
        public DateTime? WarrantyExpiryDate { get; set; }

        /// <summary>
        /// Asset description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Asset image paths.
        /// </summary>
        public string Images { get; set; } = string.Empty;

        /// <summary>
        /// QR code image path (absolute URL when available).
        /// </summary>
        public string QRCodePath { get; set; } = string.Empty;

        /// <summary>
        /// URL encoded inside the QR image.
        /// </summary>
        public string QRCodeText { get; set; } = string.Empty;
    }
}
