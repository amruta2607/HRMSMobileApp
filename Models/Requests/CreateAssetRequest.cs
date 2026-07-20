using System.ComponentModel.DataAnnotations;

namespace MobileWebApi.Models.Requests
{
    /// <summary>
    /// Request payload for creating a new asset from the mobile application.
    /// Only database NOT NULL fields are validated; nullable columns accept null/empty.
    /// </summary>
    public class CreateAssetRequest
    {
        /// <summary>
        /// Asset display name.
        /// </summary>
        [Required(ErrorMessage = "Asset name is required.")]
        public string AssetName { get; set; } = string.Empty;

        /// <summary>
        /// Asset description (nullable).
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Asset status identifier.
        /// </summary>
        [Range(1, 999999, ErrorMessage = "Asset status is required.")]
        public int AssetStatusId { get; set; }

        /// <summary>
        /// Asset category identifier.
        /// </summary>
        [Range(1, 999999, ErrorMessage = "Asset category is required.")]
        public int AssetCategoryId { get; set; }

        /// <summary>
        /// Department identifier.
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// Branch identifier.
        /// </summary>
        public int? BranchId { get; set; }

        /// <summary>
        /// Business unit identifier.
        /// </summary>
        public int? BusinessUnitId { get; set; }

        /// <summary>
        /// Asset type identifier.
        /// </summary>
        public int? AssetTypeId { get; set; }

        /// <summary>
        /// Asset owner.
        /// </summary>
        public string? Owner { get; set; }

        /// <summary>
        /// Asset location.
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Purchase date (NOT NULL).
        /// </summary>
        [Required(ErrorMessage = "Purchase date is required.")]
        public DateTime PurchaseDate { get; set; }

        /// <summary>
        /// Purchase price (NOT NULL).
        /// </summary>
        [Required(ErrorMessage = "Purchase price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Purchase price must be greater than or equal to 0.")]
        public decimal PurchasePrice { get; set; }

        /// <summary>
        /// Purchase order number (nullable).
        /// </summary>
        public string? PurchaseOrderNumber { get; set; }

        /// <summary>
        /// Purchase order bill reference (nullable).
        /// </summary>
        public string? PurchaseOrderBill { get; set; }

        /// <summary>
        /// Support center details (nullable).
        /// </summary>
        public string? SupportCenter { get; set; }

        /// <summary>
        /// Manufacturer name (nullable).
        /// </summary>
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Model name (nullable).
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Serial number (nullable).
        /// </summary>
        public string? SerialNumber { get; set; }

        /// <summary>
        /// Production year (nullable).
        /// </summary>
        public int? ProductionYear { get; set; }

        /// <summary>
        /// Asset tag number (nullable).
        /// </summary>
        public string? AssetTagNumber { get; set; }

        /// <summary>
        /// Yearly depreciation percentage.
        /// </summary>
        public decimal? DepreciationPercentage { get; set; }

        /// <summary>
        /// Warranty expiry date.
        /// </summary>
        public DateTime? WarrantyExpiryDate { get; set; }

        /// <summary>
        /// Next maintenance due date.
        /// </summary>
        public DateTime? MaintenanceDueDate { get; set; }

        /// <summary>
        /// Asset image paths (nullable).
        /// </summary>
        public string? Images { get; set; }

        /// <summary>
        /// Optional maintenance records to create with the asset.
        /// </summary>
        public List<CreateAssetMaintenanceRequest> MaintenanceList { get; set; } = new();
    }
}
