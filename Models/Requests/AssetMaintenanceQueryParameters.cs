using MobileWebApi.Constants;

namespace MobileWebApi.Models.Requests
{
    /// <summary>
    /// Pagination, search and sorting parameters for listing asset maintenance records.
    /// </summary>
    public class AssetMaintenanceQueryParameters
    {
        private int _page = 1;
        private int _pageSize = AssetMaintenanceConstants.DefaultPageSize;

        /// <summary>
        /// 1-based page number. Values below 1 are treated as 1.
        /// </summary>
        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        /// <summary>
        /// Page size. Clamped between 1 and <see cref="AssetMaintenanceConstants.MaxPageSize"/>.
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (value < 1)
                {
                    _pageSize = AssetMaintenanceConstants.DefaultPageSize;
                }
                else
                {
                    _pageSize = value > AssetMaintenanceConstants.MaxPageSize
                        ? AssetMaintenanceConstants.MaxPageSize
                        : value;
                }
            }
        }

        /// <summary>
        /// Free-text search applied to AssetNumber, AssetName, ResponsiblePerson and AssetDescription.
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Field to sort by. Supported: Date, Cost, AssetNumber, AssetName, ResponsiblePerson, Id.
        /// Defaults to Date.
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction: "asc" or "desc". Defaults to desc.
        /// </summary>
        public string? SortDirection { get; set; }
    }
}
