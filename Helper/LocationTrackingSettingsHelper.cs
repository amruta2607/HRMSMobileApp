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
        /// Hierarchy (same as Login / LocationTrackingConfiguration):
        /// 1. EnableLocationTracking is the master switch.
        /// 2. When master is on and EnableEmployeeLevelLocationTracking is off, all employees are tracked.
        /// 3. When both are on, Employee.EnableLocationTracking decides.
        /// </summary>
        public static bool ShouldTrackLocation(
            bool attendanceEnabled,
            bool tenantLocationTrackingEnabled,
            bool enableEmployeeLevelLocationTracking,
            bool employeeLocationTrackingEnabled)
        {
            // Attendance disabled → no tracking
            if (!attendanceEnabled)
            {
                return false;
            }

            // Master switch off → no tracking (employee-level ignored)
            if (!tenantLocationTrackingEnabled)
            {
                return false;
            }

            // Master on, employee-level off → track all employees (employee setting ignored)
            if (!enableEmployeeLevelLocationTracking)
            {
                return true;
            }

            // Master on, employee-level on → use employee setting
            return employeeLocationTrackingEnabled;
        }
    }
}
