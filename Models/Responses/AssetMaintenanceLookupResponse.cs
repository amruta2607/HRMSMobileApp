namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Asset option for the Asset Maintenance screen.
    /// </summary>
    public class AssetMaintenanceLookupAssetDto
    {
        public int Id { get; set; }
        public string AssetNumber { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string AssetCode { get; set; } = string.Empty;
        public string AssetTagNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Employee option for the Asset Maintenance responsible person picker.
    /// </summary>
    public class AssetMaintenanceLookupEmployeeDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lookup collections required by the Asset Maintenance module.
    /// </summary>
    public class AssetMaintenanceLookupData
    {
        public List<AssetMaintenanceLookupAssetDto> Assets { get; set; } = new();
        public List<AssetMaintenanceLookupEmployeeDto> ResponsiblePersons { get; set; } = new();
    }

    /// <summary>
    /// Response wrapper for asset maintenance lookup data.
    /// </summary>
    public class AssetMaintenanceLookupResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AssetMaintenanceLookupData Data { get; set; } = new();
    }
}
