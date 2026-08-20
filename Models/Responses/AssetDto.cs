namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Represents a single asset record for the mobile asset list.
    /// </summary>
    public class AssetDto
    {
        /// <summary>
        /// Asset identifier.
        /// </summary>
        public int HistoryId { get; set; }

        /// <summary>
        /// Asset number.
        /// </summary>
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Asset code.
        /// </summary>
        public string AssetCode { get; set; } = string.Empty;

        /// <summary>
        /// QR code file path.
        /// </summary>
        public string QRCodePath { get; set; } = string.Empty;

        /// <summary>
        /// URL encoded inside the QR image.
        /// </summary>
        public string QRCodeText { get; set; } = string.Empty;

        /// <summary>
        /// Asset display name.
        /// </summary>
        public string AssetName { get; set; } = string.Empty;

        /// <summary>
        /// Current asset status name.
        /// </summary>
        public string AssetStatus { get; set; } = string.Empty;

        /// <summary>
        /// Asset owner.
        /// </summary>
        public string Owner { get; set; } = string.Empty;

        /// <summary>
        /// Asset category name.
        /// </summary>
        public string AssetCategory { get; set; } = string.Empty;

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
        /// Asset purchase date.
        /// </summary>
        public DateTime? PurchaseDate { get; set; }

        /// <summary>
        /// Asset purchase price.
        /// </summary>
        public decimal? PurchasePrice { get; set; }

        /// <summary>
        /// Current actual value of the asset.
        /// </summary>
        public decimal? ActualValue { get; set; }

        /// <summary>
        /// Asset manufacturer.
        /// </summary>
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        /// Asset model.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Year of production.
        /// </summary>
        public int? ProductionYear { get; set; }

        /// <summary>
        /// Date when the QR code was generated.
        /// </summary>
        public DateTime? QRCodeGeneratedDate { get; set; }
    }
}
