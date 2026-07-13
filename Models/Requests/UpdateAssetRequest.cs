using System.ComponentModel.DataAnnotations;

namespace MobileWebApi.Models.Requests
{
    /// <summary>
    /// Request payload for updating an existing asset from the mobile application.
    /// </summary>
    public class UpdateAssetRequest
    {
        [Required(ErrorMessage = "Asset name is required.")]
        public string AssetName { get; set; } = string.Empty;

        [Range(1, 999999, ErrorMessage = "Asset category is required.")]
        public int AssetCategoryId { get; set; }

        public int? AssetTypeId { get; set; }

        public int? AssetStatusId { get; set; }

        public int? DepartmentId { get; set; }

        [Range(1, 999999, ErrorMessage = "Branch is required.")]
        public int BranchId { get; set; }

        public int? BusinessUnitId { get; set; }

        public string? Location { get; set; }

        public string? Owner { get; set; }

        public string? Manufacturer { get; set; }

        public string? Model { get; set; }

        public string? SerialNumber { get; set; }

        public int? ProductionYear { get; set; }

        [Required(ErrorMessage = "Purchase date is required.")]
        public DateTime PurchaseDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Purchase price must be greater than or equal to 0.")]
        public decimal PurchasePrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Actual value must be greater than or equal to 0.")]
        public decimal? ActualValue { get; set; }

        public DateTime? WarrantyExpiryDate { get; set; }

        public DateTime? MaintenanceDueDate { get; set; }

        public string? Description { get; set; }

        public string? Images { get; set; }
    }
}
