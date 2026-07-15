namespace MobileWebApi.Helper
{
    /// <summary>
    /// Application-level fixed rules returned as read-only values to the mobile client.
    /// </summary>
    public static class LocationTrackingFixedRules
    {
        public const bool DuplicateSessionCheck = true;
        public const bool AlwaysAllowPermissionCheck = true;
        public const bool PermissionRevokedAutoPunchOut = true;
    }
}
