namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Asset option for the hand over screen.
    /// </summary>
    public class AssetHandOverLookupAssetDto
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string AssetTagNumber { get; set; } = string.Empty;
        public string AssetStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Employee option for the hand over screen.
    /// </summary>
    public class AssetHandOverLookupEmployeeDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lookup collections required by the Asset HandOver screen.
    /// </summary>
    public class AssetHandOverLookupsData
    {
        public List<AssetHandOverLookupAssetDto> Assets { get; set; } = new();
        public List<AssetHandOverLookupEmployeeDto> HandOverByEmployees { get; set; } = new();
        public List<AssetHandOverLookupEmployeeDto> HandOverToEmployees { get; set; } = new();
    }

    /// <summary>
    /// Response wrapper for asset handover lookup data.
    /// </summary>
    public class AssetHandOverLookupsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AssetHandOverLookupsData Data { get; set; } = new();
    }
}
