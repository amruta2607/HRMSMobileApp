namespace MobileWebApi.Constants
{
    /// <summary>
    /// Field length limits for the Asset Maintenance module.
    /// These should be kept in sync with the corresponding [dbo].[AssetMaintenance] column definitions.
    /// </summary>
    public static class AssetMaintenanceConstants
    {
        public const int AssetNumberMaxLength = 50;
        public const int AssetNameMaxLength = 250;
        public const int ResponsiblePersonMaxLength = 250;
        public const int AssetDescriptionMaxLength = 1000;

        /// <summary>
        /// Default page size used when the client does not supply one.
        /// </summary>
        public const int DefaultPageSize = 20;

        /// <summary>
        /// Maximum page size a client is allowed to request.
        /// </summary>
        public const int MaxPageSize = 200;
    }
}
