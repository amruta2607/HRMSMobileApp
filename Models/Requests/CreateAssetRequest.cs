using System.ComponentModel.DataAnnotations;

namespace MobileWebApi.Models.Requests
{
    /// <summary>
    /// Request payload for creating a new asset from the mobile application.
    /// </summary>
    public class CreateAssetRequest
    {
        /// <summary>
        /// Asset display name.
        /// </summary>
        [Required(ErrorMessage = "Asset name is required.")]
        public string AssetName { get; set; } = string.Empty;

        /// <summary>
        /// Asset description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Asset status identifier.
        /// </summary>
        public int? AssetStatusId { get; set; }

        /// <summary>
        /// Asset category identifier.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Asset category is required.")]
        public int AssetCategoryId { get; set; }

        /// <summary>
        /// Department identifier.
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// Branch identifier.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Branch is required.")]
        public int BranchId { get; set; }

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
        /// Purchase date.
        /// </summary>
        [Required(ErrorMessage = "Purchase date is required.")]
        public DateTime PurchaseDate { get; set; }

        /// <summary>
        /// Purchase price.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Purchase price must be greater than or equal to 0.")]
        public decimal PurchasePrice { get; set; }

        /// <summary>
        /// Purchase order number.
        /// </summary>
        [Required(ErrorMessage = "Purchase order number is required.")]
        public string PurchaseOrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// Purchase order bill reference.
        /// </summary>
        public string? PurchaseOrderBill { get; set; }

        /// <summary>
        /// Support center details.
        /// </summary>
        public string? SupportCenter { get; set; }

        /// <summary>
        /// Manufacturer name.
        /// </summary>
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Model name.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Serial number.
        /// </summary>
        public string? SerialNumber { get; set; }

        /// <summary>
        /// Production year.
        /// </summary>
        public int? ProductionYear { get; set; }

        /// <summary>
        /// Asset tag number.
        /// </summary>
        [Required(ErrorMessage = "Asset tag number is required.")]
        public string AssetTagNumber { get; set; } = string.Empty;

        /// <summary>
        /// Yearly depreciation percentage.
        /// </summary>
        [Range(0, 100, ErrorMessage = "Depreciation percentage must be between 0 and 100.")]
        public decimal DepreciationPercentage { get; set; }

        /// <summary>
        /// Warranty expiry date.
        /// </summary>
        public DateTime? WarrantyExpiryDate { get; set; }

        /// <summary>
        /// Next maintenance due date.
        /// </summary>
        public DateTime? MaintenanceDueDate { get; set; }

        /// <summary>
        /// Asset image paths.
        /// </summary>
        public string? Images { get; set; }

        /// <summary>
        /// Optional maintenance records to create with the asset.
        /// </summary>
        public List<CreateAssetMaintenanceRequest> MaintenanceList { get; set; } = new();
    }
}
