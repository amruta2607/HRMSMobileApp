namespace MobileWebApi.Helper
{
    /// <summary>
    /// Computes location-tracking configuration flags and effective tracking decisions.
    /// </summary>
    public static class LocationTrackingSettingsHelper
    {
        /// <summary>
        /// Returns configuration flags for the Login API response.
        /// When attendance is disabled, all location-tracking flags are returned as false.
        /// Otherwise returns the tenant and employee configuration values as-is.
        /// </summary>
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

            return (
                true,
                tenantLocationTrackingEnabled,
                enableEmployeeLevelLocationTracking,
                employeeLocationTrackingEnabled ?? false);
        }

        /// <summary>
        /// Determines whether location tracking should be active based on tenant and employee configuration.
        /// </summary>
        public static bool ShouldTrackLocation(
            bool attendanceEnabled,
            bool tenantLocationTrackingEnabled,
            bool enableEmployeeLevelLocationTracking,
            bool employeeLocationTrackingEnabled)
        {
            // Rule 1: attendance disabled
            if (!attendanceEnabled)
            {
                return false;
            }

            // Rule 2: tenant has disabled location tracking
            if (!tenantLocationTrackingEnabled)
            {
                return false;
            }

            // Rule 3: employee-level config off — use tenant setting (enabled)
            if (!enableEmployeeLevelLocationTracking)
            {
                return true;
            }

            // Rule 5: employee explicitly enabled — use employee setting
            if (employeeLocationTrackingEnabled)
            {
                return true;
            }

            // Rule 4: employee disabled — fall back to tenant setting (enabled)
            return tenantLocationTrackingEnabled;
        }
    }
}
