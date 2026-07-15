namespace MobileWebApi.Models
{
    /// <summary>
    /// Consolidated location tracking configuration for the mobile application.
    /// </summary>
    public class LocationTrackingConfigurationResponse
    {
        public bool AttendanceEnabled { get; set; }
        public bool EnableLocationTracking { get; set; }
        public bool EnableEmployeeLevelLocationTracking { get; set; }
        public bool EmployeeLocationTrackingEnabled { get; set; }

        public int GPSPollingInterval { get; set; }
        public int MinimumDisplacement { get; set; }
        public int GPSAccuracyThreshold { get; set; }
        public int DuplicateLocationRadius { get; set; }
        public int AutoPunchOutTimeout { get; set; }
        public int OfflineStorageLimit { get; set; }
        public int AutoDataCleanupDays { get; set; }
        public int RetryInterval { get; set; }
        public int ServerSyncBatchSize { get; set; }

        public int GeofenceRadius { get; set; }
        public bool EnableFromAnywhere { get; set; }
        public bool BlockPunchOnHoliday { get; set; }
        public bool EnableLocationGapValidation { get; set; }
        public bool EnableBatteryOptimizationCheck { get; set; }
        public int BatteryOptimizationMode { get; set; }

        public bool AutoPunchOutOnGPSTurnOff { get; set; }
        public bool AutoPunchOutOnLocationServicesOff { get; set; }
        public bool AutoPunchOutOnAppKilled { get; set; }
        public bool AutoPunchOutOnPowerSavingMode { get; set; }
        public bool AutoPunchOutOnAirplaneMode { get; set; }
        public int LocationTimeoutDuration { get; set; }

        public bool DuplicateSessionCheck { get; set; }
        public bool AlwaysAllowPermissionCheck { get; set; }
        public bool PermissionRevokedAutoPunchOut { get; set; }
    }
}
