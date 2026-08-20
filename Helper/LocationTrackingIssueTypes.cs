namespace MobileWebApi.Helper
{
    /// <summary>
    /// Allowed location tracking issue types reported by the mobile application.
    /// </summary>
    public static class LocationTrackingIssueTypes
    {
        public const string GpsDisabled = "gps_disabled";
        public const string TrackingGap = "tracking_gap";
        public const string PermissionDenied = "permission_denied";
        public const string MockLocation = "mock_location";

        private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            GpsDisabled,
            TrackingGap,
            PermissionDenied,
            MockLocation
        };

        public static bool IsValid(string? issueType) =>
            !string.IsNullOrWhiteSpace(issueType) && AllowedTypes.Contains(issueType.Trim());

        public static string Normalize(string issueType) => issueType.Trim().ToLowerInvariant();

        public static string GetDisplayName(string issueType) =>
            Normalize(issueType) switch
            {
                GpsDisabled => "GPS Disabled",
                TrackingGap => "Tracking Gap",
                PermissionDenied => "Permission Denied",
                MockLocation => "Mock Location",
                _ => issueType
            };
    }
}
