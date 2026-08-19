namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Paged asset maintenance list response for the current tenant.
    /// </summary>
    public class AssetMaintenanceListResponse
    {
        /// <summary>
        /// Asset maintenance records for the requested page.
        /// </summary>
        public List<AssetMaintenanceDto> Items { get; set; } = new();

        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Page size used for the query.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of records matching the query (ignoring pagination).
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Total number of pages available for the current page size.
        /// </summary>
        public int TotalPages { get; set; }
    }
}
