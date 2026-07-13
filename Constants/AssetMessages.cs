namespace MobileWebApi.Constants
{
    /// <summary>
    /// Asset module messages.
    /// </summary>
    public static class AssetMessages
    {
        public const string CreatedSuccessfully = "Asset created successfully.";
        public const string UpdatedSuccessfully = "Asset updated successfully.";
        public const string HandedOverSuccessfully = "Asset handed over successfully.";
        public const string HandoverUpdatedSuccessfully = "Asset handover updated successfully.";
        public const string HandoverNotFound = "Asset handover record not found.";
        public const string NotFound = "Asset not found.";
        public const string EmployeeNotFound = "Employee not found.";
        public const string AssetInactiveOrDisposed = "Asset is disposed or retired and cannot be handed over.";
        public const string SameHandoverEmployee = "Asset is already assigned to this employee.";
        public const string HandoverByEmployeeRequired = "A valid employee record is required to perform handover.";
        public const string InvalidWarrantyDate = "Warranty expiry date is invalid.";
        public const string InvalidMaintenanceDate = "Maintenance due date is invalid.";
        public const string TenantConfigurationNotFound = "Tenant configuration was not found.";
        public const string InvalidAssetCategory = "The selected asset category is invalid.";
        public const string InvalidBranch = "The selected branch is invalid.";
        public const string InvalidDepartment = "The selected department is invalid.";
        public const string InvalidBusinessUnit = "The selected business unit is invalid.";
        public const string InvalidAssetType = "The selected asset type is invalid.";
        public const string InvalidAssetStatus = "The selected asset status is invalid.";
        public const string DuplicateAssetTagNumber = "An asset with this tag number already exists.";
        public const string EmployeeRequiredForMaintenance = "An employee record is required to add maintenance items.";
        public const string RequestBodyCannotBeNull = "Request body cannot be null.";
        public const string LookupsFetchedSuccessfully = "Asset lookups fetched successfully.";
        public const string HandoverLookupsFetchedSuccessfully = "Lookup data retrieved successfully.";
        public const string AssetNotAvailableForHandover = "The selected asset is not available for handover.";
        public const string InvalidHandOverByEmployee = "The selected hand over by employee is invalid.";
    }
}
