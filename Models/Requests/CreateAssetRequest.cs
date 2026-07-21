using System.ComponentModel.DataAnnotations;

namespace MobileWebApi.Models.Requests
{
    /// <summary>
    /// Request payload for creating a new asset from the mobile application.
    /// Only database NOT NULL fields (PurchaseDate, PurchasePrice) are validated.
    /// </summary>
    public class CreateAssetRequest
    {
        /// <summary>
        /// Asset display name (nullable).
        /// </summary>
        public string? AssetName { get; set; }

        /// <summary>
        /// Asset description (nullable).
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Asset status identifier (nullable).
        /// </summary>
        public int? AssetStatusId { get; set; }

        /// <summary>
        /// Asset category identifier (nullable).
        /// </summary>
        public int? AssetCategoryId { get; set; }

        /// <summary>
        /// Department identifier (nullable).
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// Branch identifier (nullable).
        /// </summary>
        public int? BranchId { get; set; }

        /// <summary>
        /// Business unit identifier (nullable).
        /// </summary>
        public int? BusinessUnitId { get; set; }

        /// <summary>
        /// Asset type identifier (nullable).
        /// </summary>
        public int? AssetTypeId { get; set; }

        /// <summary>
        /// Asset owner (nullable).
        /// </summary>
        public string? Owner { get; set; }

        /// <summary>
        /// Asset location (nullable).
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
        /// Asset tag number (nullable). Duplicate check runs only when non-empty; blank stores NULL.
        /// </summary>
        public string? AssetTagNumber { get; set; }

        /// <summary>
        /// Yearly depreciation percentage (nullable).
        /// </summary>
        public decimal? DepreciationPercentage { get; set; }

        /// <summary>
        /// Actual value (nullable). When omitted, defaults from purchase price.
        /// </summary>
        public decimal? ActualValue { get; set; }

        /// <summary>
        /// Warranty expiry date (nullable).
        /// </summary>
        public DateTime? WarrantyExpiryDate { get; set; }

        /// <summary>
        /// Next maintenance due date (nullable).
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
