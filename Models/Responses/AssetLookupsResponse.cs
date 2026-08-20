namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Generic id/name pair used by asset lookup endpoints.
    /// </summary>
    public class AssetLookupItemDto
    {
        /// <summary>
        /// Lookup record identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Display name for the lookup record.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lookup collections required by the Create Asset screen.
    /// </summary>
    public class AssetLookupsData
    {
        public List<AssetLookupItemDto> AssetStatuses { get; set; } = new();
        public List<AssetLookupItemDto> AssetCategories { get; set; } = new();
        public List<AssetLookupItemDto> Departments { get; set; } = new();
        public List<AssetLookupItemDto> Branches { get; set; } = new();
        public List<AssetLookupItemDto> BusinessUnits { get; set; } = new();
        public List<AssetLookupItemDto> AssetTypes { get; set; } = new();
    }

    /// <summary>
    /// Response wrapper for asset lookup data.
    /// </summary>
    public class AssetLookupsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AssetLookupsData Data { get; set; } = new();
    }
}
