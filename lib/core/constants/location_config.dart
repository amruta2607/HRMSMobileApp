/**
 * Location Configuration Constants
 *
 * All values are now fetched dynamically from the server API:
 *   GET /api/mobile/locationtrackingconfiguration
 *
 * Unit notes (as documented in backend API):
 *   - gpsPollingInterval      → backend sends MINUTES → converted ×60 → seconds here
 *   - retryInterval           → backend sends MINUTES → converted ×60 → seconds here
 *   - minimumDisplacement     → meters   (used as-is)
 *   - gpsAccuracyThreshold    → meters   (used as-is)
 *   - duplicateLocationRadius → meters   (used as-is)
 *   - autoPunchOutTimeout     → minutes  (used as minutes in gap detector)
 *   - offlineStorageLimit     → count    (used as-is)
 *   - autoDataCleanupDays     → days     (used as-is)
 *   - serverSyncBatchSize     → count    (used as-is)
 *   - locationTimeoutDuration → seconds  (used as-is for HTTP timeouts)
 *
 * Fallback defaults (used when no cached config is available):
 *   gpsPollingInterval=20 min, minimumDisplacement=10m, gpsAccuracyThreshold=50m,
 *   duplicateLocationRadius=5m, autoPunchOutTimeout=80 min, offlineStorageLimit=500,
 *   autoDataCleanupDays=7, retryInterval=5 min, serverSyncBatchSize=50,
 *   locationTimeoutDuration=80s
 */

import 'dart:math' as math;

import '../Background_location _tracking/services/location_config_service.dart';

class LocationConfig {
  // =================== LOCATION TRACKING INTERVALS ===================

  /// Location update interval in seconds.
  /// Backend: gpsPollingInterval (minutes) → converted ×60 → seconds.
  static int get LOCATION_UPDATE_INTERVAL_SECONDS =>
      LocationConfigService.gpsPollingIntervalSeconds;

  /// Location update interval in milliseconds (for compatibility).
  static int get LOCATION_UPDATE_INTERVAL_MS =>
      LOCATION_UPDATE_INTERVAL_SECONDS * 1000;

  /// Fast location update interval in seconds.
  static int get FAST_LOCATION_UPDATE_INTERVAL_SECONDS =>
      LocationConfigService.gpsPollingIntervalSeconds;

  /// Fast location update interval in milliseconds (for compatibility).
  static int get FAST_LOCATION_UPDATE_INTERVAL_MS =>
      FAST_LOCATION_UPDATE_INTERVAL_SECONDS * 1000;

  /// Background location update interval in seconds.
  static int get BACKGROUND_LOCATION_UPDATE_INTERVAL_SECONDS =>
      LocationConfigService.gpsPollingIntervalSeconds;

  /// Background location update interval in milliseconds (for compatibility).
  static int get BACKGROUND_LOCATION_UPDATE_INTERVAL_MS =>
      BACKGROUND_LOCATION_UPDATE_INTERVAL_SECONDS * 1000;

  // =================== API CALL INTERVALS ===================

  /// API call interval in seconds.
  static int get API_CALL_INTERVAL_SECONDS =>
      LocationConfigService.gpsPollingIntervalSeconds;

  /// API call interval in milliseconds (for compatibility).
  static int get API_CALL_INTERVAL_MS => API_CALL_INTERVAL_SECONDS * 1000;

  /// Immediate API call timeout in seconds.
  /// Backend: locationTimeoutDuration (seconds, no conversion needed).
  static int get IMMEDIATE_API_TIMEOUT_SECONDS =>
      LocationConfigService.locationTimeoutDurationSeconds;

  /// Batch API call timeout in seconds.
  static int get BATCH_API_TIMEOUT_SECONDS =>
      LocationConfigService.locationTimeoutDurationSeconds;

  // =================== LOCATION ACCURACY SETTINGS ===================

  /// Distance filter in meters.
  /// Backend: minimumDisplacement (meters, no conversion).
  static double get DISTANCE_FILTER_METERS =>
      LocationConfigService.minimumDisplacementMeters;

  /// Location accuracy threshold in meters.
  /// Backend: gpsAccuracyThreshold (meters, no conversion).
  static double get LOCATION_ACCURACY_THRESHOLD_METERS =>
      LocationConfigService.gpsAccuracyThresholdMeters;

  /// Minimum location update interval in seconds (fixed floor).
  static const int MIN_LOCATION_UPDATE_INTERVAL_SECONDS = 5;

  /// Maximum location update interval in seconds.
  static int get MAX_LOCATION_UPDATE_INTERVAL_SECONDS =>
      LocationConfigService.gpsPollingIntervalSeconds;

  /// Maximum allowed gap between location updates in minutes.
  /// Backend: autoPunchOutTimeout (minutes, no conversion — used as minutes).
  static int get MAX_LOCATION_GAP_MINUTES =>
      LocationConfigService.autoPunchOutTimeoutMinutes;

  /// Maximum consecutive duplicate locations allowed before forcing acceptance.
  static const int MAX_CONSECUTIVE_DUPLICATES = 5;

  /// Duplicate location detection threshold in meters.
  /// Backend: duplicateLocationRadius (meters, no conversion).
  static double get DUPLICATE_LOCATION_THRESHOLD_METERS =>
      LocationConfigService.duplicateLocationRadiusMeters;

  /// Minimum locations required in first hour to prevent bypass.
  static const int MIN_LOCATIONS_FIRST_HOUR = 3;

  // =================== BATCH AND SYNC SETTINGS ===================

  /// Maximum batch size for location uploads.
  /// Backend: serverSyncBatchSize (count, no conversion).
  static int get MAX_BATCH_SIZE => LocationConfigService.serverSyncBatchSize;

  /// Minimum batch size for bulk upload (fixed).
  static const int MIN_BATCH_SIZE = 5;

  /// Location staleness threshold in seconds.
  static int get LOCATION_STALENESS_THRESHOLD_SECONDS =>
      LocationConfigService.gpsPollingIntervalSeconds;

  /// Location staleness threshold in milliseconds (for compatibility).
  static int get LOCATION_STALENESS_THRESHOLD_MS =>
      LOCATION_STALENESS_THRESHOLD_SECONDS * 1000;

  /// Maximum locations to keep in memory (fixed).
  static const int MAX_LOCATIONS_IN_MEMORY = 100;

  /// Maximum locations to keep in database.
  /// Backend: offlineStorageLimit (count, no conversion).
  static int get MAX_LOCATIONS_IN_DATABASE =>
      LocationConfigService.offlineStorageLimit;

  // =================== CLEANUP SETTINGS ===================

  /// Location cleanup interval in hours (fixed).
  static const int LOCATION_CLEANUP_INTERVAL_HOURS = 24;

  /// Location retention period in days.
  /// Backend: autoDataCleanupDays (days, no conversion).
  static int get LOCATION_RETENTION_DAYS =>
      LocationConfigService.autoDataCleanupDays;

  /// Auto cleanup on punch in.
  static const bool AUTO_CLEANUP_ON_PUNCH_IN = true;

  /// Auto cleanup on punch out.
  static const bool AUTO_CLEANUP_ON_PUNCH_OUT = true;

  // =================== CACHE SETTINGS ===================

  /// Location cache validity in hours (fixed).
  static const int LOCATION_CACHE_VALIDITY_HOURS = 24;

  /// Location cache validity in milliseconds (for compatibility).
  static const int LOCATION_CACHE_VALIDITY_MS =
      LOCATION_CACHE_VALIDITY_HOURS * 60 * 60 * 1000;

  /// Maximum cached locations (fixed).
  static const int MAX_CACHED_LOCATIONS = 50;

  // =================== RETRY SETTINGS ===================

  /// Maximum retry attempts for failed requests (fixed).
  static const int MAX_RETRY_ATTEMPTS = 5;

  /// Retry delay in seconds.
  /// Backend: retryInterval (minutes) → converted ×60 → seconds.
  static int get RETRY_DELAY_SECONDS =>
      LocationConfigService.retryIntervalSeconds;

  /// Exponential backoff multiplier (fixed).
  static const double RETRY_BACKOFF_MULTIPLIER = 2.0;

  // =================== NETWORK SETTINGS ===================

  /// Network check interval in seconds (fixed).
  static const int NETWORK_CHECK_INTERVAL_SECONDS = 30;

  /// Network timeout in seconds (fixed).
  static const int NETWORK_TIMEOUT_SECONDS = 15;

  /// Immediate upload on network restore (fixed).
  static const bool IMMEDIATE_UPLOAD_ON_NETWORK_RESTORE = true;

  // =================== PERFORMANCE SETTINGS ===================

  /// Heartbeat interval in seconds.
  static int get HEARTBEAT_INTERVAL_SECONDS =>
      LocationConfigService.gpsPollingIntervalSeconds;

  /// Prevent suspend (fixed).
  static const bool PREVENT_SUSPEND = true;

  /// Enable headless mode (fixed).
  static const bool ENABLE_HEADLESS_MODE = true;

  // =================== POLICY CONTROLS (from API dashboard) ===================

  static bool get ENABLE_BATTERY_OPTIMIZATION_CHECK =>
      LocationConfigService.enableBatteryOptimizationCheck;

  /// 0 = Warning Only, 1 = Strict (block), 2 = Lenient (skip)
  static int get BATTERY_OPTIMIZATION_MODE =>
      LocationConfigService.batteryOptimizationMode;

  static bool get ENABLE_FROM_ANYWHERE =>
      LocationConfigService.enableFromAnywhere;

  static bool get BLOCK_PUNCH_ON_HOLIDAY =>
      LocationConfigService.blockPunchOnHoliday;

  static bool get ENABLE_LOCATION_GAP_VALIDATION =>
      LocationConfigService.enableLocationGapValidation;

  /// Master switch: org + employee location tracking both enabled.
  static bool get IS_LOCATION_TRACKING_ENABLED =>
      LocationConfigService.enableLocationTracking &&
      LocationConfigService.employeeLocationTrackingEnabled;

  static bool get DUPLICATE_SESSION_CHECK =>
      LocationConfigService.duplicateSessionCheck;

  static double get GEOFENCE_RADIUS_METERS =>
      LocationConfigService.geofenceRadiusMeters;

  static bool get AUTO_PUNCH_OUT_ON_GPS_OFF =>
      LocationConfigService.autoPunchOutOnGPSTurnOff;

  static bool get AUTO_PUNCH_OUT_ON_LOCATION_OFF =>
      LocationConfigService.autoPunchOutOnLocationServicesOff;

  static bool get AUTO_PUNCH_OUT_ON_APP_KILLED =>
      LocationConfigService.autoPunchOutOnAppKilled;

  static bool get AUTO_PUNCH_OUT_ON_POWER_SAVING =>
      LocationConfigService.autoPunchOutOnPowerSavingMode;

  static bool get AUTO_PUNCH_OUT_ON_AIRPLANE_MODE =>
      LocationConfigService.autoPunchOutOnAirplaneMode;

  static bool get PERMISSION_REVOKED_AUTO_PUNCH_OUT =>
      LocationConfigService.permissionRevokedAutoPunchOut;

  // =================== HELPER METHODS ===================

  /// Get location update interval in duration format.
  static Duration get locationUpdateInterval =>
      Duration(seconds: LOCATION_UPDATE_INTERVAL_SECONDS);

  /// Get API call interval in duration format.
  static Duration get apiCallInterval =>
      Duration(seconds: API_CALL_INTERVAL_SECONDS);

  /// Get background location update interval in duration format.
  static Duration get backgroundLocationUpdateInterval =>
      Duration(seconds: BACKGROUND_LOCATION_UPDATE_INTERVAL_SECONDS);

  /// Get location staleness threshold in duration format.
  static Duration get locationStalenessThreshold =>
      Duration(seconds: LOCATION_STALENESS_THRESHOLD_SECONDS);

  /// Get location retention period in duration format.
  static Duration get locationRetentionPeriod =>
      Duration(days: LOCATION_RETENTION_DAYS);

  /// Get location cache validity in duration format.
  static Duration get locationCacheValidity =>
      Duration(hours: LOCATION_CACHE_VALIDITY_HOURS);

  /// Get network check interval in duration format.
  static Duration get networkCheckInterval =>
      Duration(seconds: NETWORK_CHECK_INTERVAL_SECONDS);

  /// Get heartbeat interval in duration format.
  static Duration get heartbeatInterval =>
      Duration(seconds: HEARTBEAT_INTERVAL_SECONDS);

  /// Get retry delay in duration format.
  static Duration get retryDelay => Duration(seconds: RETRY_DELAY_SECONDS);

  /// Calculate exponential backoff delay.
  static Duration calculateBackoffDelay(int attemptNumber) {
    return Duration(
        seconds: (RETRY_DELAY_SECONDS *
                (math.pow(RETRY_BACKOFF_MULTIPLIER, attemptNumber - 1)))
            .round());
  }
}
