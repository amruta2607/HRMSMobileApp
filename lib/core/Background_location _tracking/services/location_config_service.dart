import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

import '../../Utils/Urls/urls.dart';
import '../../Utils/services/token_storage.dart';
import '../../constants/log_levels.dart';

/// LocationConfigService
///
/// Fetches location tracking configuration from the server API:
///   GET /api/mobile/locationtrackingconfiguration
///
/// All API field units (as documented in backend):
///   - gpsPollingInterval      → minutes  (multiply ×60 for seconds)
///   - retryInterval           → minutes  (multiply ×60 for seconds)
///   - minimumDisplacement     → meters
///   - gpsAccuracyThreshold    → meters
///   - duplicateLocationRadius → meters
///   - autoPunchOutTimeout     → minutes
///   - offlineStorageLimit     → count
///   - autoDataCleanupDays     → days
///   - serverSyncBatchSize     → count
///   - geofenceRadius          → meters
///   - locationTimeoutDuration → seconds
///   - batteryOptimizationMode → int (0=Normal, 1=Strict, 2=Lenient)
///
/// The service caches the last-fetched config in SharedPreferences so it
/// is immediately available on the next app launch while a fresh fetch
/// runs in the background.
class LocationConfigService {
  static final LocationConfigService _instance =
      LocationConfigService._internal();
  static LocationConfigService get instance => _instance;

  LocationConfigService._internal();

  static SharedPreferences? _prefs;
  static Map<String, dynamic> _cachedConfig = {};

  // SharedPreferences key for persisting the config JSON
  static const String _cacheKey = 'location_tracking_config_cache';

  // =================== INITIALIZATION ===================

  /// Initialize service: loads from local cache, then fetches fresh from server.
  static Future<void> initialize() async {
    try {
      _prefs = await SharedPreferences.getInstance();
      _loadFromLocalCache();

      // Fetch fresh config in the background (non-blocking)
      fetchConfig();
    } catch (e) {
      LogConfig.logError('Failed to initialize LocationConfigService', e);
    }
  }

  // =================== CACHE HELPERS ===================

  /// Load last-fetched config from SharedPreferences into memory.
  static void _loadFromLocalCache() {
    if (_prefs == null) return;
    try {
      final cachedJson = _prefs!.getString(_cacheKey);
      if (cachedJson != null) {
        _cachedConfig = Map<String, dynamic>.from(json.decode(cachedJson));
        LogConfig.logBackground(
            '[LocationConfigService] Loaded config from local cache.');
      }
    } catch (e) {
      LogConfig.logError(
          '[LocationConfigService] Error loading config from local cache', e);
    }
  }

  /// Persist freshly fetched config to SharedPreferences and update memory cache.
  static Future<void> _saveToLocalCache(Map<String, dynamic> config) async {
    if (_prefs == null) return;
    try {
      _cachedConfig = config;
      await _prefs!.setString(_cacheKey, json.encode(config));
      LogConfig.logSuccess(
          '[LocationConfigService] Saved location config to local cache.');
    } catch (e) {
      LogConfig.logError(
          '[LocationConfigService] Error saving config to local cache', e);
    }
  }

  // =================== API FETCH ===================

  /// Fetch configuration from
  ///   GET {base}/api/mobile/locationtrackingconfiguration
  ///
  /// Returns true on success, false on failure.
  /// On success the in-memory cache and SharedPreferences are updated.
  static Future<bool> fetchConfig() async {
    try {
      final token = await TokenStorage.getToken();

      final String configUrl =
          '${BaseUrls.base}/api/mobile/locationtrackingconfiguration';

      LogConfig.logApi(
          '[LocationConfigService] Fetching config from: $configUrl');

      final headers = <String, String>{
        'accept': '*/*',
        if (token != null && token.isNotEmpty)
          'Authorization': 'Bearer $token',
      };

      final response = await http
          .get(Uri.parse(configUrl), headers: headers)
          .timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) {
        final Map<String, dynamic> data =
            json.decode(response.body) as Map<String, dynamic>;
        await _saveToLocalCache(data);
        LogConfig.logSuccess(
            '[LocationConfigService] Config fetched & cached successfully.');

        // Notify LocationService that config has been refreshed
        try {
          // Import is done via a callback to avoid circular dependencies
          _onConfigFetchedCallback?.call();
        } catch (e) {
          LogConfig.logError(
              '[LocationConfigService] Error calling onConfigFetched callback',
              e);
        }
        return true;
      } else {
        LogConfig.logError(
            '[LocationConfigService] Fetch failed. Status: ${response.statusCode}');
        return false;
      }
    } catch (e) {
      LogConfig.logError(
          '[LocationConfigService] Error fetching config from server', e);
      return false;
    }
  }

  // =================== CALLBACK (avoid circular import) ===================

  /// Optional callback invoked after a successful config fetch.
  /// Set this from LocationService to reload geolocation settings.
  static void Function()? _onConfigFetchedCallback;

  static void setOnConfigFetchedCallback(void Function() callback) {
    _onConfigFetchedCallback = callback;
  }

  // =================== TYPED ACCESSORS ===================

  static int getInt(String key, int defaultValue) {
    if (_cachedConfig.containsKey(key)) {
      final value = _cachedConfig[key];
      if (value is int) return value;
      if (value is double) return value.toInt();
      if (value is String) return int.tryParse(value) ?? defaultValue;
    }
    return defaultValue;
  }

  static double getDouble(String key, double defaultValue) {
    if (_cachedConfig.containsKey(key)) {
      final value = _cachedConfig[key];
      if (value is double) return value;
      if (value is int) return value.toDouble();
      if (value is String) return double.tryParse(value) ?? defaultValue;
    }
    return defaultValue;
  }

  static bool getBool(String key, bool defaultValue) {
    if (_cachedConfig.containsKey(key)) {
      final value = _cachedConfig[key];
      if (value is bool) return value;
      if (value is num) return value != 0;
      if (value is String) {
        final v = value.trim().toLowerCase();
        if (v == 'true' || v == 'yes' || v == '1') return true;
        if (v == 'false' || v == 'no' || v == '0') return false;
      }
    }
    return defaultValue;
  }

  // =================== CONVENIENCE GETTERS ===================
  // These mirror the API field names and apply the correct unit conversions.

  /// GPS polling interval in **seconds**.
  /// Backend sends minutes → multiply ×60.
  static int get gpsPollingIntervalSeconds =>
      getInt('gpsPollingInterval', 20) * 60;

  /// Minimum displacement in **meters** (no conversion needed).
  static double get minimumDisplacementMeters =>
      getDouble('minimumDisplacement', 10.0);

  /// GPS accuracy threshold in **meters** (no conversion needed).
  static double get gpsAccuracyThresholdMeters =>
      getDouble('gpsAccuracyThreshold', 50.0);

  /// Duplicate location radius in **meters** (no conversion needed).
  static double get duplicateLocationRadiusMeters =>
      getDouble('duplicateLocationRadius', 5.0);

  /// Auto punch-out timeout in **minutes** (no conversion; used as minutes).
  static int get autoPunchOutTimeoutMinutes =>
      getInt('autoPunchOutTimeout', 80);

  /// Offline storage limit (count, no conversion).
  static int get offlineStorageLimit => getInt('offlineStorageLimit', 500);

  /// Auto data cleanup in **days** (no conversion needed).
  static int get autoDataCleanupDays => getInt('autoDataCleanupDays', 7);

  /// Retry interval in **seconds**.
  /// Backend sends minutes → multiply ×60.
  static int get retryIntervalSeconds => getInt('retryInterval', 5) * 60;

  /// Server sync batch size (count, no conversion).
  static int get serverSyncBatchSize => getInt('serverSyncBatchSize', 50);

  /// Geofence radius in **meters** (no conversion needed).
  static double get geofenceRadiusMeters =>
      getDouble('geofenceRadius', 100.0);

  /// Location timeout duration in **seconds** (no conversion needed).
  static int get locationTimeoutDurationSeconds =>
      getInt('locationTimeoutDuration', 80);

  // =================== FEATURE FLAGS ===================

  static bool get attendanceEnabled => getBool('attendanceEnabled', true);
  static bool get enableLocationTracking =>
      getBool('enableLocationTracking', true);
  static bool get enableEmployeeLevelLocationTracking =>
      getBool('enableEmployeeLevelLocationTracking', true);
  static bool get employeeLocationTrackingEnabled =>
      getBool('employeeLocationTrackingEnabled', true);
  static bool get enableFromAnywhere => getBool('enableFromAnywhere', false);
  static bool get blockPunchOnHoliday => getBool('blockPunchOnHoliday', false);
  static bool get enableLocationGapValidation =>
      getBool('enableLocationGapValidation', true);
  static bool get enableBatteryOptimizationCheck =>
      getBool('enableBatteryOptimizationCheck', true);
  static int get batteryOptimizationMode =>
      getInt('batteryOptimizationMode', 0);
  static bool get autoPunchOutOnGPSTurnOff =>
      getBool('autoPunchOutOnGPSTurnOff', true);
  static bool get autoPunchOutOnLocationServicesOff =>
      getBool('autoPunchOutOnLocationServicesOff', true);
  static bool get autoPunchOutOnAppKilled =>
      getBool('autoPunchOutOnAppKilled', true);
  static bool get autoPunchOutOnPowerSavingMode =>
      getBool('autoPunchOutOnPowerSavingMode', false);
  static bool get autoPunchOutOnAirplaneMode =>
      getBool('autoPunchOutOnAirplaneMode', true);
  static bool get duplicateSessionCheck =>
      getBool('duplicateSessionCheck', true);
  static bool get alwaysAllowPermissionCheck =>
      getBool('alwaysAllowPermissionCheck', true);
  static bool get permissionRevokedAutoPunchOut =>
      getBool('permissionRevokedAutoPunchOut', true);

  /// Snapshot of last cached API config (for debug / tracker UI).
  static Map<String, dynamic> get cachedConfigSnapshot =>
      Map<String, dynamic>.from(_cachedConfig);

  static bool get hasCachedConfig => _cachedConfig.isNotEmpty;
}
