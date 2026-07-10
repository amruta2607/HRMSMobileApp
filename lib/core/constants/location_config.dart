/**
 * Location Configuration Constants
 * 
 * This file contains all configurable values for location tracking.
 * Change these values to adjust location tracking behavior globally.
 */

import 'dart:math' as math;

class LocationConfig {
  // =================== LOCATION TRACKING INTERVALS ===================

  /// Location update interval in seconds
  /// How often to request location updates from GPS
  static const int LOCATION_UPDATE_INTERVAL_SECONDS = 1200; // 20 minutes

  /// Location update interval in milliseconds (for compatibility)
  static const int LOCATION_UPDATE_INTERVAL_MS =
      LOCATION_UPDATE_INTERVAL_SECONDS * 1000;

  /// Fast location update interval in seconds
  /// Used for more frequent updates when needed
  static const int FAST_LOCATION_UPDATE_INTERVAL_SECONDS = 1200; // 20 minutes

  /// Fast location update interval in milliseconds (for compatibility)
  static const int FAST_LOCATION_UPDATE_INTERVAL_MS =
      FAST_LOCATION_UPDATE_INTERVAL_SECONDS * 1000;

  /// Background location update interval in seconds
  /// Used when app is in background
  static const int BACKGROUND_LOCATION_UPDATE_INTERVAL_SECONDS =
      1200; // 20 minutes

  /// Background location update interval in milliseconds (for compatibility)
  static const int BACKGROUND_LOCATION_UPDATE_INTERVAL_MS =
      BACKGROUND_LOCATION_UPDATE_INTERVAL_SECONDS * 1000;

  // =================== API CALL INTERVALS ===================

  /// API call interval in seconds
  /// How often to send location data to server
  static const int API_CALL_INTERVAL_SECONDS = 1200; // 20 minutes

  /// API call interval in milliseconds (for compatibility)
  static const int API_CALL_INTERVAL_MS = API_CALL_INTERVAL_SECONDS * 1000;

  /// Immediate API call timeout in seconds
  /// Maximum time to wait for immediate API call
  static const int IMMEDIATE_API_TIMEOUT_SECONDS = 15;

  /// Batch API call timeout in seconds
  /// Maximum time to wait for batch API call
  static const int BATCH_API_TIMEOUT_SECONDS = 30;

  // =================== LOCATION ACCURACY SETTINGS ===================

  /// Distance filter in meters
  /// Minimum distance change required to trigger location update
  static const double DISTANCE_FILTER_METERS = 10.0;

  /// Location accuracy threshold in meters
  /// Required location accuracy for valid readings
  static const double LOCATION_ACCURACY_THRESHOLD_METERS = 50.0;

  /// Minimum location update interval in seconds
  /// Prevents too frequent location updates
  static const int MIN_LOCATION_UPDATE_INTERVAL_SECONDS = 5;

  /// Maximum location update interval in seconds
  /// Prevents too long gaps between location updates
  static const int MAX_LOCATION_UPDATE_INTERVAL_SECONDS = 1200; // 20 minutes

  /// Maximum allowed gap between location updates in minutes
  /// If gap exceeds this, trigger automatic punch out
  static const int MAX_LOCATION_GAP_MINUTES = 80;

  /// Maximum consecutive duplicate locations allowed before forcing acceptance
  static const int MAX_CONSECUTIVE_DUPLICATES = 5;

  /// Duplicate location detection threshold in meters
  static const double DUPLICATE_LOCATION_THRESHOLD_METERS = 5.0;

  /// Minimum locations required in first hour to prevent bypass
  static const int MIN_LOCATIONS_FIRST_HOUR = 3; // With 20-min interval

  // =================== BATCH AND SYNC SETTINGS ===================

  /// Maximum batch size for location uploads
  /// Number of locations to send in single batch
  static const int MAX_BATCH_SIZE = 10;

  /// Minimum batch size for bulk upload
  /// Minimum number of locations to trigger bulk upload
  static const int MIN_BATCH_SIZE = 5;

  /// Location staleness threshold in seconds
  /// Consider location stale after this duration
  static const int LOCATION_STALENESS_THRESHOLD_SECONDS = 1200; // 20 minutes

  /// Location staleness threshold in milliseconds (for compatibility)
  static const int LOCATION_STALENESS_THRESHOLD_MS =
      LOCATION_STALENESS_THRESHOLD_SECONDS * 1000;

  /// Maximum locations to keep in memory
  /// Prevents memory issues with large location history
  static const int MAX_LOCATIONS_IN_MEMORY = 100;

  /// Maximum locations to keep in database
  /// Prevents database size issues
  static const int MAX_LOCATIONS_IN_DATABASE = 1000;

  // =================== CLEANUP SETTINGS ===================

  /// Location cleanup interval in hours
  /// How often to clean up old synced locations
  static const int LOCATION_CLEANUP_INTERVAL_HOURS = 24; // Daily

  /// Location retention period in days
  /// How long to keep synced locations before deletion
  static const int LOCATION_RETENTION_DAYS = 7; // 1 week

  /// Auto cleanup on punch in
  /// Whether to automatically clean up old locations on punch in
  static const bool AUTO_CLEANUP_ON_PUNCH_IN = true;

  /// Auto cleanup on punch out
  /// Whether to automatically clean up old locations on punch out
  static const bool AUTO_CLEANUP_ON_PUNCH_OUT = true;

  // =================== CACHE SETTINGS ===================

  /// Location cache validity in hours
  /// How long cached location data is considered valid
  static const int LOCATION_CACHE_VALIDITY_HOURS = 24;

  /// Location cache validity in milliseconds (for compatibility)
  static const int LOCATION_CACHE_VALIDITY_MS =
      LOCATION_CACHE_VALIDITY_HOURS * 60 * 60 * 1000;

  /// Maximum cached locations
  /// Number of locations to cache for offline use
  static const int MAX_CACHED_LOCATIONS = 50;

  // =================== RETRY SETTINGS ===================

  /// Maximum retry attempts for failed requests
  /// Number of times to retry failed API calls
  static const int MAX_RETRY_ATTEMPTS = 5;

  /// Retry delay in seconds
  /// Initial delay before retry attempt
  static const int RETRY_DELAY_SECONDS = 30;

  /// Exponential backoff multiplier
  /// Multiplier for retry delay (exponential backoff)
  static const double RETRY_BACKOFF_MULTIPLIER = 2.0;

  // =================== NETWORK SETTINGS ===================

  /// Network check interval in seconds
  /// How often to check network connectivity
  static const int NETWORK_CHECK_INTERVAL_SECONDS = 30;

  /// Network timeout in seconds
  /// Maximum time to wait for network response
  static const int NETWORK_TIMEOUT_SECONDS = 15;

  /// Immediate upload on network restore
  /// Whether to immediately upload when network is restored
  static const bool IMMEDIATE_UPLOAD_ON_NETWORK_RESTORE = true;

  // =================== PERFORMANCE SETTINGS ===================

  /// Heartbeat interval in seconds
  /// Interval for background heartbeat when app is killed
  static const int HEARTBEAT_INTERVAL_SECONDS = 1200; // 20 minutes

  /// Prevent suspend
  /// Whether to prevent app suspension for continuous tracking
  static const bool PREVENT_SUSPEND = true;

  /// Enable headless mode
  /// Whether to enable headless mode for background operation
  static const bool ENABLE_HEADLESS_MODE = true;

  // =================== HELPER METHODS ===================

  /// Get location update interval in duration format
  static Duration get locationUpdateInterval =>
      Duration(seconds: LOCATION_UPDATE_INTERVAL_SECONDS);

  /// Get API call interval in duration format
  static Duration get apiCallInterval =>
      Duration(seconds: API_CALL_INTERVAL_SECONDS);

  /// Get background location update interval in duration format
  static Duration get backgroundLocationUpdateInterval =>
      Duration(seconds: BACKGROUND_LOCATION_UPDATE_INTERVAL_SECONDS);

  /// Get location staleness threshold in duration format
  static Duration get locationStalenessThreshold =>
      Duration(seconds: LOCATION_STALENESS_THRESHOLD_SECONDS);

  /// Get location retention period in duration format
  static Duration get locationRetentionPeriod =>
      Duration(days: LOCATION_RETENTION_DAYS);

  /// Get location cache validity in duration format
  static Duration get locationCacheValidity =>
      Duration(hours: LOCATION_CACHE_VALIDITY_HOURS);

  /// Get network check interval in duration format
  static Duration get networkCheckInterval =>
      Duration(seconds: NETWORK_CHECK_INTERVAL_SECONDS);

  /// Get heartbeat interval in duration format
  static Duration get heartbeatInterval =>
      Duration(seconds: HEARTBEAT_INTERVAL_SECONDS);

  /// Get retry delay in duration format
  static Duration get retryDelay => Duration(seconds: RETRY_DELAY_SECONDS);

  /// Calculate exponential backoff delay
  static Duration calculateBackoffDelay(int attemptNumber) {
    return Duration(
        seconds: (RETRY_DELAY_SECONDS *
                (math.pow(RETRY_BACKOFF_MULTIPLIER, attemptNumber - 1)))
            .round());
  }
}
