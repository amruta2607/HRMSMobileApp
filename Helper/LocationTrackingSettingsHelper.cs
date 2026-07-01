namespace MobileWebApi.Helper
{
    /// <summary>
    /// Computes login response location-tracking flags from tenant and employee configuration.
    /// </summary>
    public static class LocationTrackingSettingsHelper
    {
        public static (
            bool AttendanceEnabled,
            bool EnableLocationTracking,
            bool EnableEmployeeLevelLocationTracking,
            bool EmployeeLocationTrackingEnabled) Resolve(
            bool attendanceEnabled,
            bool tenantLocationTrackingEnabled,
            bool enableEmployeeLevelLocationTracking,
            bool? employeeLocationTrackingEnabled)
        {
            if (!attendanceEnabled)
            {
                return (false, false, false, false);
            }

            if (!tenantLocationTrackingEnabled)
            {
                return (true, false, false, false);
            }

            if (!enableEmployeeLevelLocationTracking)
            {
                return (true, true, false, true);
            }

            return (true, true, true, employeeLocationTrackingEnabled ?? false);
        }
    }
}
