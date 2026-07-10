/**
 * Punch Out Reasons Constants
 * 
 * This file contains all possible punch out reasons for the attendance system.
 * Each constant has a clear description of when it's used.
 * 
 * Usage: Import this file and use the constants instead of hardcoded strings.
 * Benefits: Easy to modify, maintain, and understand all punch out scenarios.
 */

class PunchOutReasons {
  // =================== FOREGROUND APP SCENARIOS ===================

  /// User manually disabled GPS while app is in foreground AND has internet connection
  /// When: App is active, user turns off GPS, internet available
  /// API Call: Will be attempted immediately
  static const String GPS_DISABLED_FOREGROUND_WITH_INTERNET =
      'GPS_DISABLED_FOREGROUND_WITH_INTERNET';

  /// User manually disabled GPS while app is in foreground BUT no internet connection
  /// When: App is active, user turns off GPS, no internet available
  /// API Call: Will be cached and retried when internet is restored
  static const String GPS_DISABLED_FOREGROUND_WITHOUT_INTERNET =
      'GPS_DISABLED_FOREGROUND_WITHOUT_INTERNET';

  /// User disabled entire location services while app is in foreground
  /// When: App is active, user turns off location services completely
  /// API Call: Will use cached location data
  static const String LOCATION_SERVICES_DISABLED_FOREGROUND =
      'LOCATION_SERVICES_DISABLED_FOREGROUND';

  // =================== BACKGROUND APP SCENARIOS ===================

  /// GPS was disabled while app is in background/minimized AND has internet
  /// When: App is backgrounded, user turns off GPS, internet available
  /// API Call: Handled by headless mode
  static const String GPS_DISABLED_BACKGROUND_WITH_INTERNET =
      'GPS_DISABLED_BACKGROUND_WITH_INTERNET';

  /// GPS was disabled while app is in background/minimized BUT no internet
  /// When: App is backgrounded, user turns off GPS, no internet available
  /// API Call: Cached and retried when internet is restored
  static const String GPS_DISABLED_BACKGROUND_WITHOUT_INTERNET =
      'GPS_DISABLED_BACKGROUND_WITHOUT_INTERNET';

  /// Location services disabled while app is in background/minimized
  /// When: App is backgrounded, user turns off location services completely
  /// API Call: Uses cached location data
  static const String LOCATION_SERVICES_DISABLED_BACKGROUND =
      'LOCATION_SERVICES_DISABLED_BACKGROUND';

  // =================== APP KILLED SCENARIOS ===================

  /// GPS was disabled while app is completely killed/terminated
  /// When: App is not running, user turns off GPS
  /// API Call: Handled by headless task with cached data
  static const String GPS_DISABLED_APP_KILLED = 'GPS_DISABLED_APP_KILLED';

  /// Location services disabled while app is completely killed/terminated
  /// When: App is not running, user turns off location services
  /// API Call: Handled by headless task with cached data
  static const String LOCATION_SERVICES_DISABLED_APP_KILLED =
      'LOCATION_SERVICES_DISABLED_APP_KILLED';

  // =================== SYSTEM SCENARIOS ===================

  /// Device automatically disabled GPS due to power saving mode
  /// When: System automatically turns off GPS to save battery
  /// API Call: Uses last known location
  static const String GPS_DISABLED_POWER_SAVING = 'GPS_DISABLED_POWER_SAVING';

  /// Location permissions were revoked by user
  /// When: User revokes location permissions from system settings
  /// API Call: Uses cached location data
  static const String LOCATION_PERMISSION_REVOKED =
      'LOCATION_PERMISSION_REVOKED';

  /// Device airplane mode enabled
  /// When: User enables airplane mode
  /// API Call: Cached until airplane mode is disabled
  static const String AIRPLANE_MODE_ENABLED = 'AIRPLANE_MODE_ENABLED';

  // =================== NETWORK SCENARIOS ===================

  /// No network connectivity available for punch out
  /// When: Internet connection is lost during punch out attempt
  /// API Call: Cached and retried when network is restored
  static const String NO_NETWORK_CONNECTIVITY = 'NO_NETWORK_CONNECTIVITY';

  /// Server is unreachable for punch out
  /// When: Server is down or unreachable
  /// API Call: Retried with exponential backoff
  static const String SERVER_UNREACHABLE = 'SERVER_UNREACHABLE';

  // =================== FALLBACK SCENARIOS ===================

  /// Unknown reason for automatic punch out
  /// When: Automatic punch out triggered but reason cannot be determined
  /// API Call: Uses best available data
  static const String UNKNOWN_REASON = 'UNKNOWN_REASON';

  /// Manual punch out by user
  /// When: User manually punches out from the app
  /// API Call: Normal punch out flow
  static const String MANUAL_PUNCH_OUT = 'MANUAL_PUNCH_OUT';

  /// Forced punch out due to app crash or unexpected termination
  /// When: App crashes or terminates unexpectedly
  /// API Call: Handled by recovery mechanism
  static const String FORCED_PUNCH_OUT = 'FORCED_PUNCH_OUT';

  /// User logout from the application
  /// When: User logs out from the app
  /// API Call: Normal punch out flow with immediate API call
  static const String USER_LOGOUT = 'USER_LOGOUT';

  /// User manually turned off GPS/Location services
  static const String GPS_DISABLED_BY_USER = 'GPS disabled by user';

  // =================== HELPER METHODS ===================

  /// Get human-readable description of punch out reason
  static String getReasonDescription(String reason) {
    switch (reason) {
      case GPS_DISABLED_FOREGROUND_WITH_INTERNET:
        return 'GPS disabled while app was active (with internet)';
      case GPS_DISABLED_FOREGROUND_WITHOUT_INTERNET:
        return 'GPS disabled while app was active (no internet)';
      case LOCATION_SERVICES_DISABLED_FOREGROUND:
        return 'Location services disabled while app was active';
      case GPS_DISABLED_BACKGROUND_WITH_INTERNET:
        return 'GPS disabled while app was in background (with internet)';
      case GPS_DISABLED_BACKGROUND_WITHOUT_INTERNET:
        return 'GPS disabled while app was in background (no internet)';
      case LOCATION_SERVICES_DISABLED_BACKGROUND:
        return 'Location services disabled while app was in background';
      case GPS_DISABLED_APP_KILLED:
        return 'GPS disabled while app was not running';
      case LOCATION_SERVICES_DISABLED_APP_KILLED:
        return 'Location services disabled while app was not running';
      case GPS_DISABLED_POWER_SAVING:
        return 'GPS disabled by power saving mode';
      case LOCATION_PERMISSION_REVOKED:
        return 'Location permission revoked by user';
      case AIRPLANE_MODE_ENABLED:
        return 'Airplane mode enabled';
      case NO_NETWORK_CONNECTIVITY:
        return 'No network connectivity available';
      case SERVER_UNREACHABLE:
        return 'Server unreachable';
      case MANUAL_PUNCH_OUT:
        return 'Manual punch out by user';
      case FORCED_PUNCH_OUT:
        return 'Forced punch out due to app termination';
      case USER_LOGOUT:
        return 'User logged out from the app';
      case GPS_DISABLED_BY_USER:
        return 'GPS disabled by user';
      case UNKNOWN_REASON:
      default:
        return 'Unknown reason';
    }
  }

  /// Check if reason requires immediate API call
  static bool requiresImmediateApiCall(String reason) {
    return [
      GPS_DISABLED_FOREGROUND_WITH_INTERNET,
      GPS_DISABLED_BACKGROUND_WITH_INTERNET,
      LOCATION_SERVICES_DISABLED_FOREGROUND,
      LOCATION_SERVICES_DISABLED_BACKGROUND,
      MANUAL_PUNCH_OUT,
      USER_LOGOUT,
    ].contains(reason);
  }

  /// Check if reason should be cached for later retry
  static bool shouldCacheForRetry(String reason) {
    return [
      GPS_DISABLED_FOREGROUND_WITHOUT_INTERNET,
      GPS_DISABLED_BACKGROUND_WITHOUT_INTERNET,
      NO_NETWORK_CONNECTIVITY,
      SERVER_UNREACHABLE,
      GPS_DISABLED_APP_KILLED,
      LOCATION_SERVICES_DISABLED_APP_KILLED,
    ].contains(reason);
  }

  /// Check if reason is related to GPS being disabled
  static bool isGpsRelated(String reason) {
    return [
      GPS_DISABLED_FOREGROUND_WITH_INTERNET,
      GPS_DISABLED_FOREGROUND_WITHOUT_INTERNET,
      GPS_DISABLED_BACKGROUND_WITH_INTERNET,
      GPS_DISABLED_BACKGROUND_WITHOUT_INTERNET,
      GPS_DISABLED_APP_KILLED,
      GPS_DISABLED_POWER_SAVING,
      GPS_DISABLED_BY_USER,
    ].contains(reason);
  }

  /// Check if reason is related to location services being disabled
  static bool isLocationServicesRelated(String reason) {
    return [
      LOCATION_SERVICES_DISABLED_FOREGROUND,
      LOCATION_SERVICES_DISABLED_BACKGROUND,
      LOCATION_SERVICES_DISABLED_APP_KILLED,
      LOCATION_PERMISSION_REVOKED,
    ].contains(reason);
  }

  /// Check if reason is network related
  static bool isNetworkRelated(String reason) {
    return [
      NO_NETWORK_CONNECTIVITY,
      SERVER_UNREACHABLE,
    ].contains(reason);
  }

  /// Get all GPS-related reasons
  static List<String> getGpsReasons() {
    return [
      GPS_DISABLED_FOREGROUND_WITH_INTERNET,
      GPS_DISABLED_FOREGROUND_WITHOUT_INTERNET,
      GPS_DISABLED_BACKGROUND_WITH_INTERNET,
      GPS_DISABLED_BACKGROUND_WITHOUT_INTERNET,
      GPS_DISABLED_APP_KILLED,
      GPS_DISABLED_POWER_SAVING,
      GPS_DISABLED_BY_USER,
    ];
  }

  /// Get all location services related reasons
  static List<String> getLocationServicesReasons() {
    return [
      LOCATION_SERVICES_DISABLED_FOREGROUND,
      LOCATION_SERVICES_DISABLED_BACKGROUND,
      LOCATION_SERVICES_DISABLED_APP_KILLED,
      LOCATION_PERMISSION_REVOKED,
    ];
  }

  /// Get all network related reasons
  static List<String> getNetworkReasons() {
    return [
      NO_NETWORK_CONNECTIVITY,
      SERVER_UNREACHABLE,
    ];
  }

  /// Get all available punch out reasons
  static List<String> getAllReasons() {
    return [
      MANUAL_PUNCH_OUT,
      GPS_DISABLED_FOREGROUND_WITH_INTERNET,
      GPS_DISABLED_FOREGROUND_WITHOUT_INTERNET,
      LOCATION_SERVICES_DISABLED_FOREGROUND,
      GPS_DISABLED_BACKGROUND_WITH_INTERNET,
      GPS_DISABLED_BACKGROUND_WITHOUT_INTERNET,
      LOCATION_SERVICES_DISABLED_BACKGROUND,
      GPS_DISABLED_APP_KILLED,
      LOCATION_SERVICES_DISABLED_APP_KILLED,
      GPS_DISABLED_POWER_SAVING,
      LOCATION_PERMISSION_REVOKED,
      AIRPLANE_MODE_ENABLED,
      NO_NETWORK_CONNECTIVITY,
      SERVER_UNREACHABLE,
      FORCED_PUNCH_OUT,
      USER_LOGOUT,
      GPS_DISABLED_BY_USER,
      UNKNOWN_REASON,
    ];
  }
}
