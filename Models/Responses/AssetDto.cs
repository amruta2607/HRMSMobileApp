namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Complete asset record for the mobile asset list / update pre-populate screen.
    /// </summary>
    public class AssetDto
    {
        /// <summary>
        /// Asset identifier.
        /// </summary>
        public int Id { get; set; }

        // Status & lookups
        public int? AssetStatusId { get; set; }
        public string? AssetStatus { get; set; }
        public int? AssetCategoryId { get; set; }
        public string? AssetCategory { get; set; }
        public int? AssetTypeId { get; set; }
        public string? AssetType { get; set; }
        public int? DepartmentId { get; set; }
        public string? Department { get; set; }
        public int? BranchId { get; set; }
        public string? Branch { get; set; }
        public int? BusinessUnitId { get; set; }
        public string? BusinessUnit { get; set; }

        // Ownership
        public string? Owner { get; set; }
        public string? Location { get; set; }

        // Purchase
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? ActualValue { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public string? PurchaseOrderBill { get; set; }

        // Manufacturer
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public int? ProductionYear { get; set; }

        // Support
        public string? SupportCenter { get; set; }

        // Asset details
        public string? AssetTagNumber { get; set; }

        // Warranty & maintenance
        public DateTime? WarrantyExpiryDate { get; set; }
        public DateTime? MaintenanceDueDate { get; set; }
        public decimal? DepreciationPercentage { get; set; }

        // Media
        public string? Images { get; set; }

        // Audit
        public DateTime? InsertDate { get; set; }
        public int? InsertUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUserId { get; set; }
    }
}
