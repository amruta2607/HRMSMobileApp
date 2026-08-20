namespace MobileWebApi.Helper
{
    /// <summary>
    /// Shared asset status rules for handover lookup and validation.
    /// </summary>
    public static class AssetHandoverStatusHelper
    {
        /// <summary>
        /// Returns true when the asset status prevents handover (disposed, lost, retired, scrapped).
        /// </summary>
        public static bool IsUnavailableForHandover(string? statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
                return false;

            var normalized = statusName.Trim().ToLowerInvariant();
            return normalized.Contains("disposed", StringComparison.Ordinal)
                || normalized.Contains("scrapped", StringComparison.Ordinal)
                || normalized.Contains("scrap", StringComparison.Ordinal)
                || normalized.Contains("retired", StringComparison.Ordinal)
                || normalized.Contains("lost", StringComparison.Ordinal);
        }
    }
}
