namespace MobileWebApi.Models.Requests
{
    /// <summary>
    /// Request payload for updating an existing asset from the mobile application.
    /// </summary>
    public class UpdateAssetRequest
    {
        /// <summary>
        /// Asset display name (nullable).
        /// </summary>
        public string? AssetName { get; set; }

        /// <summary>
        /// Asset category identifier (nullable).
        /// </summary>
        public int? AssetCategoryId { get; set; }

        /// <summary>
        /// Asset type identifier (nullable).
        /// </summary>
        public int? AssetTypeId { get; set; }

        /// <summary>
        /// Asset status identifier (nullable).
        /// </summary>
        public int? AssetStatusId { get; set; }

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
        /// Asset location (nullable).
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Asset owner (nullable).
        /// </summary>
        public string? Owner { get; set; }

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
        /// Purchase date (NOT NULL).
        /// </summary>
     
        public DateTime PurchaseDate { get; set; }

        /// <summary>
        /// Purchase price (NOT NULL).
        /// </summary>
      
        public decimal PurchasePrice { get; set; }

        /// <summary>
        /// Actual value (nullable).
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
        /// Asset description (nullable).
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Asset tag number (nullable). Duplicate check runs only when non-empty.
        /// </summary>
        public string? AssetTagNumber { get; set; }

        /// <summary>
        /// Asset image paths (nullable).
        /// </summary>
        public string? Images { get; set; }
    }
}
