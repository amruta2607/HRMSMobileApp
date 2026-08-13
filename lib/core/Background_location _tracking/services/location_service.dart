/**
 * Location Service for Background Location Tracking
 *
 * This service handles comprehensive location tracking for attendance monitoring.
 * It integrates with background geolocation, manages location persistence,
 * and handles automatic punch out scenarios.
 *
 * Key Features:
 * - Background location tracking with foreground service
 * - Automatic punch out on GPS/location service disable
 * - Comprehensive punch out reason tracking
 * - Offline support with local caching
 * - Headless mode operation (when app is killed)
 * - Duplicate location detection
 * - Batch location processing
 * - Failed request retry mechanism
 * - Continues tracking until manual punch out
 */

import 'dart:async';
import 'dart:convert';
import 'dart:io';
import 'dart:math';
import 'package:flutter/material.dart';
import 'package:sqflite/sqflite.dart';
import 'package:flutter_background_geolocation/flutter_background_geolocation.dart'
    as bg;
import 'package:shared_preferences/shared_preferences.dart';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';
import 'package:geolocator/geolocator.dart';
import 'package:connectivity_plus/connectivity_plus.dart';

import '../../Utils/services/token_storage.dart';
import '../../Utils/Urls/urls.dart';
import '../../constants/punch_out_reasons.dart';
import '../../constants/log_levels.dart';
import '../../constants/location_config.dart';
import '../../Utils/services/Attendance service/attendance_service.dart';
import '../models/location_model.dart';
import 'api_service.dart';
import 'database_helper.dart';
import 'location_issue_tracker.dart';
import 'location_config_service.dart';
import 'offline_manager.dart';
import 'device_state_service.dart';


class LocationService {
  // =================== SINGLETON PATTERN ===================
  static LocationService? _instance;
  static LocationService get instance => _instance ??= LocationService._();
  LocationService._();

  static const String _keySessionActive = 'attendance_session_active';
  static const String _keyPendingTerminatePunchOut =
      'pending_terminate_punch_out';
  static const String _keyPendingTerminateReason = 'pending_terminate_reason';
  static const String _keyPendingTerminateAt = 'pending_terminate_at';

  // =================== CORE SERVICES ===================
  final ApiService _apiService = ApiService();
  final DatabaseHelper _dbHelper = DatabaseHelper.instance;
  final LocationIssueTracker _issueTracker = LocationIssueTracker.instance;
  final OfflineManager _offlineManager = OfflineManager.instance;

  // =================== LOCATION TRACKING STATE ===================
  final List<LocationData> _locationHistory = [];
  final StreamController<List<LocationData>> _locationStreamController =
      StreamController<List<LocationData>>.broadcast();

  // =================== TIMERS AND TRACKING ===================
  Timer? _apiTimer;
  int _lastApiCallTimestamp = 0;

  // Location deduplication tracking
  double? _lastSentLatitude;
  double? _lastSentLongitude;
  int _lastSentTimestamp = 0;

  // App state tracking
  bool _isInForeground = true;

  // Tracking state management
  bool _isTrackingEnabled = false;
  bool _hasUserPunchedOut = false;
  bool _isInitialized = false;
  Future<void>? _initializeFuture;

  // =================== CONFIGURATION ===================
  static int get API_CALL_INTERVAL => LocationConfig.API_CALL_INTERVAL_MS;
  static int get LOCATION_STALENESS_THRESHOLD =>
      LocationConfig.LOCATION_STALENESS_THRESHOLD_MS;
  static const int LOCATION_CACHE_VALIDITY =
      LocationConfig.LOCATION_CACHE_VALIDITY_MS;

  /// Called by LocationConfigService when a fresh config is fetched from server.
  /// Re-applies updated interval/accuracy settings to BackgroundGeolocation.
  void onConfigUpdated() {
    LogConfig.logBackground(
        '[LocationService] Config updated — reapplying geolocation settings.');
    try {
      bg.BackgroundGeolocation.setConfig(bg.Config(
        distanceFilter: LocationConfig.DISTANCE_FILTER_METERS,
        locationUpdateInterval: LocationConfig.LOCATION_UPDATE_INTERVAL_MS,
        fastestLocationUpdateInterval:
            LocationConfig.FAST_LOCATION_UPDATE_INTERVAL_MS,
        heartbeatInterval: LocationConfig.HEARTBEAT_INTERVAL_SECONDS,
        preventSuspend: LocationConfig.PREVENT_SUSPEND,
        enableHeadless: LocationConfig.ENABLE_HEADLESS_MODE,
      ));
      if (_isTrackingEnabled && !_hasUserPunchedOut) {
        _setupApiTimer();
      }
    } catch (e) {
      LogConfig.logError(
          '[LocationService] Error reapplying geolocation settings', e);
    }
  }

  // =================== CONNECTIVITY DETECTION ===================

  /**
   * Check if internet connection is available
   */
  Future<bool> _hasInternetConnection() async {
    try {
      final connectivityResult = await Connectivity().checkConnectivity();
      if (connectivityResult == ConnectivityResult.none || (connectivityResult is List && connectivityResult.contains(ConnectivityResult.none))) {
        return false;
      }
      return true;
    } catch (e) {
      LogConfig.logError('Error checking internet connectivity', e);
      return false;
    }
  }

  /**
   * Get location source based on connectivity
   */
  Future<String> _getLocationSource() async {
    final hasInternet = await _hasInternetConnection();
    return hasInternet ? 'online' : 'offline';
  }

  /**
   * Static method to get location source for headless mode
   */
  static Future<String> _getLocationSourceStatic() async {
    try {
      final connectivityResult = await Connectivity().checkConnectivity();
      final hasInternet = connectivityResult != ConnectivityResult.none && !(connectivityResult is List && connectivityResult.contains(ConnectivityResult.none));
      return hasInternet ? 'online' : 'offline';
    } catch (e) {
      LogConfig.logError(
          'Error checking internet connectivity in headless mode', e);
      return 'offline';
    }
  }

  // =================== PUBLIC GETTERS ===================
  Stream<List<LocationData>> get locationStream =>
      _locationStreamController.stream;
  List<LocationData> get locationHistory => _locationHistory;
  bool get isTrackingEnabled => _isTrackingEnabled;
  bool get hasUserPunchedOut => _hasUserPunchedOut;

  set isInForeground(bool value) {
    if (_isInForeground == value) return;
    _isInForeground = value;
    LogConfig.logBackground('App is in ${value ? "foreground" : "background"}');

    // Don't reload all saved locations on every brief pause/resume
    // (Track Location / OEM activity causes rapid toggles).
  }

  // =================== INITIALIZATION ===================

  /**
   * Initialize the location tracking service
   * Sets up background geolocation with optimal settings
   */
  Future<void> initialize() async {
    if (_isInitialized) return;
    if (_initializeFuture != null) return _initializeFuture!;

    _initializeFuture = _doInitialize();
    try {
      await _initializeFuture;
    } finally {
      _initializeFuture = null;
    }
  }

  Future<void> _doInitialize() async {
    try {
      // Initialize supporting services
      await _offlineManager.initialize();
      await _issueTracker.startMonitoring();

      // Check user punch out status
      final prefs = await SharedPreferences.getInstance();
      _hasUserPunchedOut = prefs.getBool('user_punched_out') ?? false;

      // Phone was powered off / app terminated while punched in — finish punch-out.
      await processPendingTerminatePunchOut();

      if (_hasUserPunchedOut) {
        _isInitialized = true;
        return;
      }

      // Configure background geolocation with optimal settings
      await bg.BackgroundGeolocation.ready(bg.Config(
          debug: false,
          logLevel: bg.Config.LOG_LEVEL_OFF,

          desiredAccuracy: bg.Config.DESIRED_ACCURACY_HIGH,
          distanceFilter: LocationConfig.DISTANCE_FILTER_METERS,
          locationUpdateInterval: LocationConfig.LOCATION_UPDATE_INTERVAL_MS,
          fastestLocationUpdateInterval:
              LocationConfig.FAST_LOCATION_UPDATE_INTERVAL_MS,

          stopOnTerminate: false,
          startOnBoot: true,
          enableHeadless: LocationConfig.ENABLE_HEADLESS_MODE,
          heartbeatInterval: LocationConfig.HEARTBEAT_INTERVAL_SECONDS,
          preventSuspend: LocationConfig.PREVENT_SUSPEND,

          isMoving: true,

          disableElasticity: true,
          disableMotionActivityUpdates: true,
          pausesLocationUpdatesAutomatically: false,

          foregroundService: true,
          notification: bg.Notification(
              title: "Location Tracking",
              text: "Tracking your location for attendance",
              priority: bg.Config.NOTIFICATION_PRIORITY_MIN,
              channelName: "Background Location",
              channelId: "background_location",
              sticky: true),

          extras: {"app_name": "location_tracking"}));

      // Register event listeners
      _registerEventListeners();

      // Load saved locations
      await _loadSavedLocations();

      // Check if tracking should be auto-started
      final shouldTrack = prefs.getBool('isTracking') ?? false;
      if (shouldTrack && !_hasUserPunchedOut) {
        await startTracking();
      }
      _isInitialized = true;
    } catch (e) {
      rethrow;
    }
  }

  /**
   * Register all background geolocation event listeners
   */
  void _registerEventListeners() {
    bg.BackgroundGeolocation.onLocation(_onLocation);
    bg.BackgroundGeolocation.onMotionChange(_onMotionChange);
    bg.BackgroundGeolocation.onProviderChange(_onProviderChange);
    bg.BackgroundGeolocation.onHeartbeat(_onHeartbeat);
    bg.BackgroundGeolocation.onActivityChange(_onActivityChange);
    bg.BackgroundGeolocation.onEnabledChange(_onEnabledChange);
    bg.BackgroundGeolocation.onConnectivityChange(_onConnectivityChange);
    bg.BackgroundGeolocation.onNotificationAction(_onNotificationAction);
  }

  // =================== LOCATION TRACKING CONTROL ===================

  /**
   * Start location tracking with background support
   * Only starts if user hasn't punched out
   */
  Future<void> startTracking() async {
    if (_hasUserPunchedOut) {
      return;
    }

    if (!LocationConfig.IS_LOCATION_TRACKING_ENABLED) {
      LogConfig.logWarning(
          'Location tracking disabled by dashboard config — not starting');
      return;
    }

    try {
      final state = await bg.BackgroundGeolocation.state;
      await _cacheUserDataForHeadless();

      if (!state.enabled) {
        await bg.BackgroundGeolocation.setConfig(bg.Config(
            debug: false,
            logLevel: bg.Config.LOG_LEVEL_OFF,
            distanceFilter: LocationConfig.DISTANCE_FILTER_METERS,
            locationUpdateInterval: LocationConfig.LOCATION_UPDATE_INTERVAL_MS,
            fastestLocationUpdateInterval:
                LocationConfig.FAST_LOCATION_UPDATE_INTERVAL_MS,
            heartbeatInterval: LocationConfig.HEARTBEAT_INTERVAL_SECONDS,
            preventSuspend: LocationConfig.PREVENT_SUSPEND,
            stopOnTerminate: false,
            startOnBoot: true,
            enableHeadless: LocationConfig.ENABLE_HEADLESS_MODE));

        await bg.BackgroundGeolocation.start();
        _isTrackingEnabled = true;

        _setupApiTimer();

        final prefs = await SharedPreferences.getInstance();
        await prefs.setBool('isTracking', true);
      } else {
        _isTrackingEnabled = true;
        _setupApiTimer();
      }
    } catch (e) {
      rethrow;
    }
  }

  /**
   * Stop location tracking
   */
  Future<void> stopTracking() async {
    if (!_isTrackingEnabled) {
      return;
    }

    try {
      _isTrackingEnabled = false;
      _hasUserPunchedOut = true;

      final prefs = await SharedPreferences.getInstance();
      await prefs.setBool('user_punched_out', true);
      await prefs.setBool(_keySessionActive, false);
      await prefs.remove(_keyPendingTerminatePunchOut);

      await bg.BackgroundGeolocation.stop();

      _locationHistory.clear();
      _locationStreamController.add(_locationHistory);

      _issueTracker.stopMonitoring();
      _offlineManager.dispose();
    } catch (e) {
      LogConfig.logError('Error stopping location tracking', e);
    }
  }

  /**
   * Reset punch out status (for new day or manual reset)
   */
  Future<void> resetPunchOutStatus() async {
    try {
      _hasUserPunchedOut = false;
      final prefs = await SharedPreferences.getInstance();
      await prefs.setBool('user_punched_out', false);
      await prefs.setBool(_keySessionActive, true);
      await prefs.remove(_keyPendingTerminatePunchOut);
      await prefs.remove(_keyPendingTerminateReason);
      await prefs.remove(_keyPendingTerminateAt);
    } catch (e) {
      LogConfig.logError('Error resetting punch out status', e);
    }
  }

  // =================== LOCATION ACQUISITION ===================

  /**
   * Get current location and save it to local storage
   */
  Future<LocationData?> getCurrentLocation() async {
    if (_hasUserPunchedOut) {
      LogConfig.logWarning('Cannot get location - user has punched out');
      return null;
    }

    try {
      final location = await bg.BackgroundGeolocation.getCurrentPosition(
          samples: 1, persist: true, extras: {'manual': true});

      if (!_isAccuracyAcceptable(location.coords.accuracy)) {
        LogConfig.logWarning(
            'Manual location rejected — accuracy ${location.coords.accuracy}m');
        return null;
      }

      await _cacheLastKnownLocation(
          location.coords.latitude, location.coords.longitude);
      final locationTimestamp = DateTime.now().millisecondsSinceEpoch;

      LogConfig.logLocation(
          'Manual location acquired: ${location.coords.latitude}, ${location.coords.longitude}');

      _updateLastSentLocation(location.coords.latitude,
          location.coords.longitude, locationTimestamp);

      final locationData = LocationData(
        id: locationTimestamp,
        latitude: location.coords.latitude,
        longitude: location.coords.longitude,
        timestamp: DateTime.now(),
        locationFrom: await _getLocationSource(),
      );

      _addLocation(locationData);
      _sendLocationsToApi();

      return locationData;
    } catch (e) {
      LogConfig.logError('Error getting current location', e);
      return null;
    }
  }

  /**
   * Refresh location data and send to API
   */
  Future<void> refreshData() async {
    if (_hasUserPunchedOut) {
      LogConfig.logWarning('Cannot refresh data - user has punched out');
      return;
    }

    LogConfig.logBackground('Refreshing location data');

    try {
      await _loadSavedLocations();

      final state = await bg.BackgroundGeolocation.state;
      if (state.enabled) {
        await _getCurrentPositionAndSave(isManualRefresh: true);
      }

      await _sendLocationsToApi();
    } catch (e) {
      LogConfig.logError('Error refreshing location data', e);
    }
  }

  // =================== LOCATION CACHING ===================

  /**
   * Cache user data for headless mode operations
   */
  Future<void> _cacheUserDataForHeadless() async {
    try {
      final prefs = await SharedPreferences.getInstance();

      final userId = await TokenStorage.getUserId();
      final branchId = await TokenStorage.getBranchId();
      final organizationId = await TokenStorage.getOrganisationId();
      final token = await TokenStorage.getToken();

      if (userId != null) {
        await prefs.setString('cached_user_id', userId.toString());
        if (branchId != null) await prefs.setString('cached_branch_id', branchId.toString());
        if (organizationId != null) await prefs.setString(
            'cached_organization_id', organizationId.toString());
        if (token != null) await prefs.setString('cached_token', token);
        await prefs.setString('cached_base_url', BaseUrls.punchOut);

        LogConfig.logCaching('User data cached for headless mode');
      }
    } catch (e) {
      LogConfig.logError('Error caching user data for headless mode', e);
    }
  }

  /**
   * Cache location data for offline use
   */
  Future<void> _cacheLastKnownLocation(
      double latitude, double longitude) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final locationData = {
        'latitude': latitude,
        'longitude': longitude,
        'timestamp': DateTime.now().millisecondsSinceEpoch,
      };
      await prefs.setString('last_known_location', jsonEncode(locationData));
      LogConfig.logCaching('Location cached: $latitude, $longitude');
    } catch (e) {
      LogConfig.logError('Error caching location', e);
    }
  }

  /**
   * Get cached location data
   */
  Future<Map<String, double>> _getCachedLocation() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final cachedLocationStr = prefs.getString('last_known_location');

      if (cachedLocationStr != null) {
        final cachedLocation = jsonDecode(cachedLocationStr);
        final double latitude = cachedLocation['latitude']?.toDouble() ?? 0.0;
        final double longitude = cachedLocation['longitude']?.toDouble() ?? 0.0;
        final int timestamp = cachedLocation['timestamp'] ?? 0;

        final now = DateTime.now().millisecondsSinceEpoch;
        final isOld = (now - timestamp) > LOCATION_CACHE_VALIDITY;

        if (!isOld && latitude != 0.0 && longitude != 0.0) {
          LogConfig.logCaching('Using cached location: $latitude, $longitude');
          return {'latitude': latitude, 'longitude': longitude};
        } else {
          LogConfig.logWarning('Cached location is expired or invalid');
        }
      }
    } catch (e) {
      LogConfig.logError('Error getting cached location', e);
    }
    return {'latitude': 0.0, 'longitude': 0.0};
  }

  // =================== LOCATION PROCESSING ===================

  /**
   * Get current position and save it (internal method)
   */
  Future<void> _getCurrentPositionAndSave(
      {bool isManualRefresh = false}) async {
    if (_hasUserPunchedOut) return;

    try {
      final location = await bg.BackgroundGeolocation.getCurrentPosition(
          samples: 1,
          persist: true,
          extras: {'timer': true, 'manual': isManualRefresh});

      if (!_isAccuracyAcceptable(location.coords.accuracy)) {
        LogConfig.logWarning(
            'Skipping low-accuracy location: ${location.coords.accuracy}m > ${LocationConfig.LOCATION_ACCURACY_THRESHOLD_METERS}m');
        await _issueTracker.reportIssue(
          LocationIssueTracker.ISSUE_LOCATION_ACCURACY_LOW,
          'Accuracy ${location.coords.accuracy}m exceeds threshold ${LocationConfig.LOCATION_ACCURACY_THRESHOLD_METERS}m',
        );
        return;
      }

      await _cacheLastKnownLocation(
          location.coords.latitude, location.coords.longitude);
      final locationTimestamp = DateTime.now().millisecondsSinceEpoch;

      final isDuplicate = _checkIfDuplicateLocation(location.coords.latitude,
          location.coords.longitude, locationTimestamp);

      if (isDuplicate && !isManualRefresh) {
        LogConfig.logDuplicate('Skipping duplicate location');
        return;
      }

      _updateLastSentLocation(location.coords.latitude,
          location.coords.longitude, locationTimestamp);

      LogConfig.logLocation(
          '${isManualRefresh ? "Manual" : "Timer"} location: ${location.coords.latitude}, ${location.coords.longitude}');

      final locationData = LocationData(
        id: locationTimestamp,
        latitude: location.coords.latitude,
        longitude: location.coords.longitude,
        timestamp: DateTime.now(),
        locationFrom: await _getLocationSource(),
      );

      _addLocation(locationData);
    } catch (e) {
      LogConfig.logError(
          'Error getting ${isManualRefresh ? "manual" : "timer"} location', e);
    }
  }

  /**
   * Check if location is duplicate using dashboard duplicateLocationRadius (meters).
   */
  bool _checkIfDuplicateLocation(
      double latitude, double longitude, int timestamp) {
    if (_lastSentLatitude == null || _lastSentLongitude == null) {
      return false;
    }

    final distanceMeters = Geolocator.distanceBetween(
      _lastSentLatitude!,
      _lastSentLongitude!,
      latitude,
      longitude,
    );

    final withinRadius =
        distanceMeters <= LocationConfig.DUPLICATE_LOCATION_THRESHOLD_METERS;

    final shortTimeWindow =
        timestamp - _lastSentTimestamp < LOCATION_STALENESS_THRESHOLD;

    return withinRadius && shortTimeWindow;
  }

  /// Reject points whose horizontal accuracy exceeds dashboard gpsAccuracyThreshold.
  bool _isAccuracyAcceptable(double accuracyMeters) {
    if (accuracyMeters <= 0) return true; // unknown accuracy — allow
    return accuracyMeters <= LocationConfig.LOCATION_ACCURACY_THRESHOLD_METERS;
  }

  /**
   * Update last sent location for deduplication
   */
  void _updateLastSentLocation(
      double latitude, double longitude, int timestamp) {
    _lastSentLatitude = latitude;
    _lastSentLongitude = longitude;
    _lastSentTimestamp = timestamp;
  }

  // =================== API INTEGRATION ===================

  /**
   * Set up periodic API calls using configurable interval
   */
  void _setupApiTimer() {
    if (_hasUserPunchedOut) return;

    _apiTimer?.cancel();

    _apiTimer = Timer.periodic(LocationConfig.apiCallInterval, (timer) {
      if (_hasUserPunchedOut) {
        timer.cancel();
        return;
      }

      LogConfig.logTimer('API timer triggered');
      _sendLocationsToApi();

      if (DateTime.now().millisecondsSinceEpoch - _lastSentTimestamp >
          LOCATION_STALENESS_THRESHOLD) {
        _getCurrentPositionAndSave();
      }
    });

    _sendLocationsToApi();
  }

  /**
   * Send pending locations to API with immediate processing
   */
  Future<void> _sendLocationsToApi() async {
    if (_locationHistory.isEmpty || _hasUserPunchedOut) return;

    try {
      final unsentLocations =
          _locationHistory.where((loc) => !loc.isSynced).toList();
      if (unsentLocations.isEmpty) {
        LogConfig.logApi('No pending locations to send');
        return;
      }

      LogConfig.logApi('Sending ${unsentLocations.length} locations to API');

      final locationsToSend = unsentLocations.length >
              LocationConfig.MAX_BATCH_SIZE
          ? unsentLocations
              .sublist(unsentLocations.length - LocationConfig.MAX_BATCH_SIZE)
          : unsentLocations;

      final success = await _apiService.sendLocationsImmediate(locationsToSend);

      if (success) {
        final sentIds = locationsToSend.map((loc) => loc.id).toList();
        await _dbHelper.markLocationsAsSynced(sentIds);

        for (final location in locationsToSend) {
          final index =
              _locationHistory.indexWhere((loc) => loc.id == location.id);
          if (index != -1) {
            _locationHistory[index] = LocationData(
              id: location.id,
              latitude: location.latitude,
              longitude: location.longitude,
              timestamp: location.timestamp,
              isSynced: true,
              locationFrom: location.locationFrom,
            );
          }
        }

        _lastApiCallTimestamp = DateTime.now().millisecondsSinceEpoch;
        _locationStreamController.add(_locationHistory);

        _issueTracker.updateSuccessfulSend();

        LogConfig.logSuccess(
            'Successfully sent ${locationsToSend.length} locations');

        if (LocationConfig.AUTO_CLEANUP_ON_PUNCH_OUT) {
          await _cleanupSyncedLocations();
        }
      } else {
        LogConfig.logError('Failed to send locations to API');
      }
    } catch (e) {
      LogConfig.logError('Error sending locations to API', e);
    }
  }

  // =================== AUTO PUNCH OUT FUNCTIONALITY ===================

  /**
   * Get current time with proper timezone handling
   */
  Map<String, String> _getCurrentDateTime() {
    final now = DateTime.now();
    final dateFormat = DateFormat('yyyy-MM-dd');
    final timeFormat = DateFormat('HH:mm:ss');

    return {
      'date': dateFormat.format(now),
      'time': timeFormat.format(now),
      'datetime': '${dateFormat.format(now)} ${timeFormat.format(now)}',
      'timestamp': now.toIso8601String(),
    };
  }

  /**
   * Determine punch out reason based on current state
   */
  Future<String> _determinePunchOutReason({
    required bool isGpsEnabled,
    required bool isLocationEnabled,
    required bool isBackgroundMode,
  }) async {
    bool hasInternet = await _hasInternetConnection();

    if (isBackgroundMode || !_isInForeground) {
      if (!isGpsEnabled) {
        return hasInternet
            ? PunchOutReasons.GPS_DISABLED_BACKGROUND_WITH_INTERNET
            : PunchOutReasons.GPS_DISABLED_BACKGROUND_WITHOUT_INTERNET;
      } else if (!isLocationEnabled) {
        return PunchOutReasons.LOCATION_SERVICES_DISABLED_BACKGROUND;
      }
    } else {
      if (!isGpsEnabled) {
        return hasInternet
            ? PunchOutReasons.GPS_DISABLED_FOREGROUND_WITH_INTERNET
            : PunchOutReasons.GPS_DISABLED_FOREGROUND_WITHOUT_INTERNET;
      } else if (!isLocationEnabled) {
        return PunchOutReasons.LOCATION_SERVICES_DISABLED_FOREGROUND;
      }
    }

    return PunchOutReasons.UNKNOWN_REASON;
  }

  /**
   * Get best available location for punch out
   */
  Future<Map<String, double>> _getBestAvailableLocation() async {
    if (_locationHistory.isNotEmpty) {
      final lastLocation = _locationHistory.last;
      LogConfig.logLocation(
          'Using last location from history: ${lastLocation.latitude}, ${lastLocation.longitude}');
      return {
        'latitude': lastLocation.latitude,
        'longitude': lastLocation.longitude
      };
    }

    final cachedLocation = await _getCachedLocation();
    if (cachedLocation['latitude'] != 0.0 ||
        cachedLocation['longitude'] != 0.0) {
      LogConfig.logLocation(
          'Using cached location: ${cachedLocation['latitude']}, ${cachedLocation['longitude']}');
      return cachedLocation;
    }

    try {
      final location = await bg.BackgroundGeolocation.getCurrentPosition(
        samples: 1,
        timeout: 10,
      );

      final coords = {
        'latitude': location.coords.latitude,
        'longitude': location.coords.longitude
      };
      await _cacheLastKnownLocation(coords['latitude']!, coords['longitude']!);

      LogConfig.logLocation(
          'Got current location for punch out: ${coords['latitude']}, ${coords['longitude']}');
      return coords;
    } catch (e) {
      LogConfig.logError(
          'Could not get current location (GPS likely disabled)', e);
      return {'latitude': 0.0, 'longitude': 0.0};
    }
  }

  /**
   * Perform automatic punch out
   */
  Future<void> _performAutoPunchOut({required String reason}) async {
    try {
      final dateTime = _getCurrentDateTime();
      final location = await _getBestAvailableLocation();
      final reasonText = PunchOutReasons.getReasonDescription(reason);

      LogConfig.logPunchOut(
          'Performing auto punch out at: ${dateTime['datetime']}, Reason: $reasonText');

      final result = await AttendanceService.submitAutoPunchOut(
        punchOutReason: reasonText,
        punchTime: DateTime.tryParse(dateTime['datetime']!),
        latitude: location['latitude'],
        longitude: location['longitude'],
      );

      if (result.success) {
        LogConfig.logSuccess('Auto punch out successful: ${result.message}');
        await _saveAutoPunchOutEvent(reason, dateTime);
        await stopTracking();
      } else {
        LogConfig.logError('Auto punch out failed: ${result.message}');
        await _savePunchOutAttempt(
          reason: reason,
          latitude: location['latitude']!,
          longitude: location['longitude']!,
          punchOutTime: dateTime['datetime']!,
          attendanceDate: dateTime['date']!,
          timestamp: dateTime['timestamp']!,
        );
      }
    } catch (e) {
      LogConfig.logError('Error performing auto punch out', e);

      final dateTime = _getCurrentDateTime();
      final location = await _getBestAvailableLocation();

      await _savePunchOutAttempt(
        reason: reason,
        latitude: location['latitude']!,
        longitude: location['longitude']!,
        punchOutTime: dateTime['datetime']!,
        attendanceDate: dateTime['date']!,
        timestamp: dateTime['timestamp']!,
      );
    }
  }

  /**
   * Save successful auto punch out event
   */
  Future<void> _saveAutoPunchOutEvent(
      String reason, Map<String, String> dateTime) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final existingEvents = prefs.getStringList('auto_punch_out_events') ?? [];

      final eventData = jsonEncode({
        'reason': reason,
        'reason_description': PunchOutReasons.getReasonDescription(reason),
        'date': dateTime['date'],
        'time': dateTime['time'],
        'datetime': dateTime['datetime'],
        'timestamp': dateTime['timestamp'],
        'foreground_mode': _isInForeground,
      });

      existingEvents.add(eventData);

      if (existingEvents.length > 50) {
        existingEvents.removeRange(0, existingEvents.length - 50);
      }

      await prefs.setStringList('auto_punch_out_events', existingEvents);
      LogConfig.logDatabase('Auto punch out event saved locally');
    } catch (e) {
      LogConfig.logError('Error saving auto punch out event', e);
    }
  }

  /**
   * Save failed punch out attempt for retry
   */
  static Future<void> _savePunchOutAttempt({
    required String reason,
    required double latitude,
    required double longitude,
    required String punchOutTime,
    required String attendanceDate,
    required String timestamp,
  }) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final existingAttempts =
          prefs.getStringList('failed_punch_out_attempts') ?? [];

      final userId = await TokenStorage.getUserId();

      if (userId == null) {
        LogConfig.logError(
            'Cannot save punch out attempt - user credentials not available');
        return;
      }

      final attemptData = jsonEncode({
        'userId': userId.toString(),
        'punch_out_time': punchOutTime,
        'longitude': longitude.toString(),
        'latitude': latitude.toString(),
        'attendance_date': attendanceDate,
        'Manual': 'false',
        'PunchOutReason': PunchOutReasons.getReasonDescription(reason),
        'punch_out_reason': reason,
        'timestamp': timestamp,
        'retry_count': 0,
      });

      existingAttempts.add(attemptData);
      await prefs.setStringList('failed_punch_out_attempts', existingAttempts);

      LogConfig.logDatabase('Saved failed punch out attempt for retry');
    } catch (e) {
      LogConfig.logError('Error saving punch out attempt', e);
    }
  }

  // =================== EVENT HANDLERS ===================

  /**
   * Handle location updates from background geolocation
   */
  void _onLocation(bg.Location location) async {
    if (_hasUserPunchedOut) return;

    if (!_isAccuracyAcceptable(location.coords.accuracy)) {
      LogConfig.logWarning(
          'Skipping low-accuracy location update: ${location.coords.accuracy}m > ${LocationConfig.LOCATION_ACCURACY_THRESHOLD_METERS}m');
      await _issueTracker.reportIssue(
        LocationIssueTracker.ISSUE_LOCATION_ACCURACY_LOW,
        'Accuracy ${location.coords.accuracy}m exceeds threshold ${LocationConfig.LOCATION_ACCURACY_THRESHOLD_METERS}m',
      );
      return;
    }

    final locationTimestamp = DateTime.now().millisecondsSinceEpoch;

    await _cacheLastKnownLocation(
        location.coords.latitude, location.coords.longitude);

    final position = Position(
      latitude: location.coords.latitude,
      longitude: location.coords.longitude,
      timestamp: DateTime.now(),
      accuracy: location.coords.accuracy,
      altitude: location.coords.altitude,
      heading: location.coords.heading,
      speed: location.coords.speed,
      speedAccuracy: 0.0,
      altitudeAccuracy: 0.0,
      headingAccuracy: 0.0,
    );
    _issueTracker.updateLocationReceived(position);

    final isDuplicate = _checkIfDuplicateLocation(
        location.coords.latitude, location.coords.longitude, locationTimestamp);

    if (isDuplicate) {
      LogConfig.logDuplicate('Skipping duplicate location update');
      return;
    }

    _updateLastSentLocation(
        location.coords.latitude, location.coords.longitude, locationTimestamp);

    LogConfig.logLocation(
        'Location update: ${location.coords.latitude}, ${location.coords.longitude}');

    final locationData = LocationData(
      id: locationTimestamp,
      latitude: location.coords.latitude,
      longitude: location.coords.longitude,
      timestamp: DateTime.now(),
      locationFrom: await _getLocationSource(),
    );

    _addLocation(locationData);
    _sendLocationsToApi();
  }

  /**
   * Handle heartbeat events for periodic location updates
   */
  void _onHeartbeat(bg.HeartbeatEvent event) async {
    if (_hasUserPunchedOut) return;

    LogConfig.logHeartbeat('Heartbeat received');

    final now = DateTime.now().millisecondsSinceEpoch;
    final shouldGetLocation =
        now - _lastSentTimestamp > LOCATION_STALENESS_THRESHOLD;

    if (shouldGetLocation) {
      bg.BackgroundGeolocation.getCurrentPosition(
          samples: 1,
          persist: true,
          extras: {'heartbeat': true}).then((bg.Location location) async {
        if (!_isAccuracyAcceptable(location.coords.accuracy)) {
          LogConfig.logWarning(
              'Skipping low-accuracy heartbeat location: ${location.coords.accuracy}m');
          return;
        }

        final locationTimestamp = now;

        final isDuplicate = _checkIfDuplicateLocation(location.coords.latitude,
            location.coords.longitude, locationTimestamp);
        if (isDuplicate) {
          LogConfig.logDuplicate('Skipping duplicate heartbeat location');
          _sendLocationsToApi();
          return;
        }

        _updateLastSentLocation(location.coords.latitude,
            location.coords.longitude, locationTimestamp);

        LogConfig.logHeartbeat(
            'Heartbeat location: ${location.coords.latitude}, ${location.coords.longitude}');

        final locationData = LocationData(
          id: locationTimestamp,
          latitude: location.coords.latitude,
          longitude: location.coords.longitude,
          timestamp: DateTime.now(),
          locationFrom: await _getLocationSource(),
        );

        _addLocation(locationData);
        _sendLocationsToApi();
      }).catchError((error) {
        LogConfig.logError('Error getting heartbeat location', error);
      });
    } else {
      LogConfig.logHeartbeat('Heartbeat - sending pending locations');
      _sendLocationsToApi();
    }

    _apiService.retryFailedRequests();
  }

  /**
   * Handle GPS/location provider state changes
   */
  void _onProviderChange(bg.ProviderChangeEvent event) async {
    LogConfig.logBackground(
        'Provider changed: GPS=${event.gps}, Network=${event.network}, Enabled=${event.enabled}');

    await _cacheUserDataForHeadless();

    if (!event.gps) {
      await _issueTracker.reportIssue(
        LocationIssueTracker.ISSUE_GPS_DISABLED,
        'GPS was disabled by user or system',
      );
    } else if (!event.enabled) {
      await _issueTracker.reportIssue(
        LocationIssueTracker.ISSUE_LOCATION_SERVICES_DISABLED,
        'Location services were disabled by user or system',
      );
    }

    // Permission status (Android/iOS status code on provider event when available)
    await _handlePossiblePermissionRevokedPunchOut();

    if (!_hasUserPunchedOut) {
      if (!LocationConfig.AUTO_PUNCH_OUT_ON_GPS_OFF &&
          !LocationConfig.AUTO_PUNCH_OUT_ON_LOCATION_OFF) {
        LogConfig.logInfo(
            'Provider change ignored — dashboard auto punch-out disabled');
        return;
      }

      final reason = await _determinePunchOutReason(
        isGpsEnabled: event.gps,
        isLocationEnabled: event.enabled,
        isBackgroundMode: !_isInForeground,
      );

      if ((!event.gps && LocationConfig.AUTO_PUNCH_OUT_ON_GPS_OFF) ||
          (!event.enabled && LocationConfig.AUTO_PUNCH_OUT_ON_LOCATION_OFF)) {
        LogConfig.logWarning(
            'Location services disabled - triggering auto punch out');
        await _performAutoPunchOut(reason: reason);
      }
    }
  }

  Future<void> _handlePossiblePermissionRevokedPunchOut() async {
    try {
      if (_hasUserPunchedOut) return;
      if (!LocationConfig.PERMISSION_REVOKED_AUTO_PUNCH_OUT) return;

      final permission = await Geolocator.checkPermission();
      final revoked = permission == LocationPermission.denied ||
          permission == LocationPermission.deniedForever;

      if (!revoked) return;

      LogConfig.logWarning(
          'Location permission revoked while punched in — auto punch-out');
      await _issueTracker.reportIssue(
        LocationIssueTracker.ISSUE_LOCATION_PERMISSION_DENIED,
        'Permission revoked: $permission',
      );
      await _performAutoPunchOut(
          reason: PunchOutReasons.LOCATION_PERMISSION_REVOKED);
    } catch (e) {
      LogConfig.logError('Error handling permission-revoked punch-out', e);
    }
  }

  /**
   * Handle network connectivity changes
   */
  void _onConnectivityChange(bg.ConnectivityChangeEvent event) {
    LogConfig.logNetwork('Connectivity changed: ${event.connected}');

    if (event.connected) {
      _apiService.sendAllPendingLocations();
      _retryFailedPunchOutAttempts();
      _sendLocationsToApi();
      processPendingTerminatePunchOut();
    } else {
      // May be airplane mode — check native flag (Android).
      _handlePossibleAirplaneModePunchOut();
    }
  }

  Future<void> _handlePossibleAirplaneModePunchOut() async {
    try {
      if (_hasUserPunchedOut) return;
      if (!LocationConfig.AUTO_PUNCH_OUT_ON_AIRPLANE_MODE) return;

      final airplane = await DeviceStateService.isAirplaneModeOn();
      if (!airplane) {
        LogConfig.logNetwork(
            'Connectivity lost but airplane mode off — skip airplane punch-out');
        return;
      }

      LogConfig.logWarning(
          'Airplane mode ON while punched in — auto punch-out');
      await _performAutoPunchOut(reason: PunchOutReasons.AIRPLANE_MODE_ENABLED);
    } catch (e) {
      LogConfig.logError('Error handling airplane-mode punch-out', e);
    }
  }

  // =================== OTHER EVENT HANDLERS ===================

  void _onMotionChange(bg.Location location) {
    LogConfig.logBackground('Motion changed: ${location.isMoving}');
  }

  void _onActivityChange(bg.ActivityChangeEvent event) {
    LogConfig.logBackground(
        'Activity changed: ${event.activity}, confidence: ${event.confidence}');
  }

  void _onEnabledChange(bool enabled) {
    LogConfig.logBackground('Enabled changed: $enabled');

    _loadSavedLocations();

    if (enabled && !_hasUserPunchedOut) {
      _setupApiTimer();
    } else {
      _apiTimer?.cancel();
      _apiTimer = null;
    }
  }

  void _onNotificationAction(String action) {
    LogConfig.logBackground('Notification action: $action');
    if (action == 'Stop Tracking') {
      stopTracking();
    }
  }

  // =================== LOCATION MANAGEMENT ===================

  /**
   * Add location to history and update database
   */
  void _addLocation(LocationData location) async {
    await _cacheLastKnownLocation(location.latitude, location.longitude);

    final existingIndex =
        _locationHistory.indexWhere((loc) => loc.id == location.id);

    if (existingIndex != -1) {
      LogConfig.logDatabase('Updating existing location: ${location.id}');
      _locationHistory[existingIndex] = location;
      await _dbHelper.updateLocation(location);
    } else {
      _locationHistory.add(location);
      await _dbHelper.insertLocation(location);
      LogConfig.logDatabase(
          'Added new location: ${location.latitude}, ${location.longitude}');
    }

    _locationStreamController.add(_locationHistory);
  }

  /**
   * Load saved locations from database
   */
  Future<void> _loadSavedLocations() async {
    try {
      final locations = await _dbHelper.getRecentLocations(limit: 100);

      if (locations.isNotEmpty) {
        final existingIds = Set.from(_locationHistory.map((loc) => loc.id));
        final newLocations =
            locations.where((loc) => !existingIds.contains(loc.id)).toList();

        if (newLocations.isNotEmpty) {
          _locationHistory.addAll(newLocations);
          LogConfig.logDatabase(
              'Loaded ${newLocations.length} new locations from database');
        }

        _locationHistory.sort((a, b) => a.timestamp.compareTo(b.timestamp));
        _locationStreamController.add(_locationHistory);
      }
    } catch (e) {
      LogConfig.logError('Error loading saved locations', e);
    }
  }

  // =================== PUNCH OUT RETRY MECHANISM ===================

  /**
   * Retry failed punch out attempts
   */
  static Future<void> _retryFailedPunchOutAttempts() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final failedAttempts =
          prefs.getStringList('failed_punch_out_attempts') ?? [];

      if (failedAttempts.isEmpty) {
        return;
      }

      LogConfig.logInfo(
          '🔄 Retrying ${failedAttempts.length} failed punch out attempts');

      List<String> stillFailedAttempts = [];

      for (String attemptStr in failedAttempts) {
        try {
          final attemptData = jsonDecode(attemptStr) as Map<String, dynamic>;

          final retryCount = (attemptData['retry_count'] ?? 0) + 1;
          attemptData['retry_count'] = retryCount;

          if (retryCount > 5) {
            LogConfig.logWarning(
                'Skipping punch out attempt - too many retries');
            continue;
          }

          final reason = (attemptData['PunchOutReason'] ??
                  attemptData['punch_out_reason'] ??
                  PunchOutReasons.UNKNOWN_REASON)
              .toString();

          final result = await AttendanceService.submitAutoPunchOut(
            punchOutReason: PunchOutReasons.getReasonDescription(reason),
            punchTime:
                DateTime.tryParse(attemptData['punch_out_time']?.toString() ?? ''),
            latitude:
                double.tryParse(attemptData['latitude']?.toString() ?? ''),
            longitude:
                double.tryParse(attemptData['longitude']?.toString() ?? ''),
          );

          if (result.success) {
            LogConfig.logSuccess('Retry punch out successful: $reason');
          } else {
            LogConfig.logError('Retry punch out failed: ${result.message}');
            stillFailedAttempts.add(jsonEncode(attemptData));
          }
        } catch (e) {
          LogConfig.logError('Error retrying punch out attempt', e);
          stillFailedAttempts.add(attemptStr);
        }
      }

      await prefs.setStringList(
          'failed_punch_out_attempts', stillFailedAttempts);

      if (stillFailedAttempts.isEmpty) {
        LogConfig.logSuccess(
            'All failed punch out attempts retried successfully');
      } else {
        LogConfig.logWarning(
            '${stillFailedAttempts.length} punch out attempts still pending');
      }
    } catch (e) {
      LogConfig.logError('Error retrying failed punch out attempts', e);
    }
  }

  static Future<void> retryFailedPunchOutAttempts() =>
      _retryFailedPunchOutAttempts();

  // =================== HEADLESS MODE SUPPORT ===================

  /**
   * Get cached user data for headless operations
   */
  static Future<Map<String, dynamic>?> getCachedUserData() async {
    try {
      final prefs = await SharedPreferences.getInstance();

      final userId = prefs.getString('cached_user_id');
      final branchId = prefs.getString('cached_branch_id') ?? '0';
      final organizationId = prefs.getString('cached_organization_id') ?? '0';

      if (userId != null) {
        return {
          'user_id': int.parse(userId),
          'branch_id': int.parse(branchId),
          'organization_id': int.parse(organizationId),
        };
      }

      return null;
    } catch (e) {
      LogConfig.logError('Error getting cached user data', e);
      return null;
    }
  }

  /**
   * Headless task processor - handles events when app is killed
   */
  static Future<void> headlessTask(bg.HeadlessEvent event) async {
    final String eventName = event.name;
    LogConfig.logHeadless('Processing event: $eventName');

    await bg.BackgroundGeolocation.setConfig(bg.Config(debug: false));

    final cachedUserData = await getCachedUserData();

    switch (eventName) {
      case bg.Event.LOCATION:
        await _handleHeadlessLocation(event);
        break;
      case bg.Event.HEARTBEAT:
        await _handleHeadlessHeartbeat(event);
        break;
      case bg.Event.PROVIDERCHANGE:
        await _handleHeadlessProviderChange(event, cachedUserData);
        break;
      case bg.Event.CONNECTIVITYCHANGE:
        await _handleHeadlessConnectivityChange(event);
        break;
      case bg.Event.TERMINATE:
        await _handleHeadlessTerminate();
        break;
      case bg.Event.BOOT:
        await _handleHeadlessBoot();
        break;
      case bg.Event.POWERSAVECHANGE:
        await _handleHeadlessPowerSaveChange(event);
        break;
    }
  }

  /**
   * Handle location events in headless mode
   */
  static Future<void> _handleHeadlessLocation(bg.HeadlessEvent event) async {
    try {
      final bg.Location location = event.event;
      LogConfig.logHeadless(
          'Location: ${location.coords.latitude}, ${location.coords.longitude}');

      final apiService = ApiService();
      final dbHelper = DatabaseHelper.instance;

      final locationData = LocationData(
        id: DateTime.now().millisecondsSinceEpoch,
        latitude: location.coords.latitude,
        longitude: location.coords.longitude,
        timestamp: DateTime.now(),
        locationFrom: await _getLocationSourceStatic(),
      );

      await dbHelper.insertLocation(locationData);
      await apiService.sendLocationData(locationData);

      LogConfig.logHeadless('Location saved and sent');
    } catch (e) {
      LogConfig.logError('[Headless] Error handling location', e);
    }
  }

  /**
   * Handle heartbeat events in headless mode
   */
  static Future<void> _handleHeadlessHeartbeat(bg.HeadlessEvent event) async {
    try {
      LogConfig.logHeadless('Heartbeat received');

      final apiService = ApiService();
      final dbHelper = DatabaseHelper.instance;

      final location = await bg.BackgroundGeolocation.getCurrentPosition(
          samples: 1, persist: true, extras: {'heartbeat': true});

      final locationData = LocationData(
        id: DateTime.now().millisecondsSinceEpoch,
        latitude: location.coords.latitude,
        longitude: location.coords.longitude,
        timestamp: DateTime.now(),
        locationFrom: await _getLocationSourceStatic(),
      );

      await dbHelper.insertLocation(locationData);
      await apiService.sendLocationData(locationData);

      final unsentLocations = await dbHelper.getUnsentLocations();
      if (unsentLocations.isNotEmpty) {
        final success = await apiService.sendBatchLocationData(unsentLocations);
        if (success) {
          final sentIds = unsentLocations.map((loc) => loc.id).toList();
          await dbHelper.markLocationsAsSynced(sentIds);
        }
      }

      LogConfig.logHeadless('Heartbeat processed');
    } catch (e) {
      LogConfig.logError('[Headless] Error handling heartbeat', e);
    }
  }

  /**
   * Handle provider changes in headless mode
   */
  static Future<void> _handleHeadlessProviderChange(
      bg.HeadlessEvent event, Map<String, dynamic>? cachedUserData) async {
    try {
      final bg.ProviderChangeEvent providerEvent = event.event;
      LogConfig.logHeadless(
          'Provider change - GPS: ${providerEvent.gps}, Enabled: ${providerEvent.enabled}');

      if (!providerEvent.gps || !providerEvent.enabled) {
        if (!providerEvent.gps && !LocationConfig.AUTO_PUNCH_OUT_ON_GPS_OFF) {
          LogConfig.logHeadless('GPS off ignored — dashboard flag disabled');
          return;
        }
        if (!providerEvent.enabled &&
            !LocationConfig.AUTO_PUNCH_OUT_ON_LOCATION_OFF) {
          LogConfig.logHeadless(
              'Location services off ignored — dashboard flag disabled');
          return;
        }

        final reason = !providerEvent.gps
            ? PunchOutReasons.GPS_DISABLED_APP_KILLED
            : PunchOutReasons.LOCATION_SERVICES_DISABLED_APP_KILLED;

        LogConfig.logHeadless('Performing auto punch out');
        await _performHeadlessAutoPunchOut(
            reason: reason, cachedUserData: cachedUserData);
      }
    } catch (e) {
      LogConfig.logError('[Headless] Error handling provider change', e);
    }
  }

  /**
   * Handle connectivity changes in headless mode
   */
  static Future<void> _handleHeadlessConnectivityChange(
      bg.HeadlessEvent event) async {
    try {
      final bg.ConnectivityChangeEvent connectivityEvent = event.event;

      if (connectivityEvent.connected) {
        LogConfig.logHeadless('Network restored - retrying requests');

        final apiService = ApiService();
        final dbHelper = DatabaseHelper.instance;

        await apiService.retryFailedRequests();
        await _retryFailedPunchOutAttempts();
        await processPendingTerminatePunchOut();

        final unsentLocations = await dbHelper.getUnsentLocations();
        if (unsentLocations.isNotEmpty) {
          final success =
              await apiService.sendBatchLocationData(unsentLocations);
          if (success) {
            final sentIds = unsentLocations.map((loc) => loc.id).toList();
            await dbHelper.markLocationsAsSynced(sentIds);
          }
        }
      } else {
        LogConfig.logHeadless('Network lost — checking airplane mode');
        await _handleHeadlessAirplanePunchOut();
      }
    } catch (e) {
      LogConfig.logError('[Headless] Error handling connectivity change', e);
    }
  }

  static Future<void> _handleHeadlessAirplanePunchOut() async {
    try {
      if (!LocationConfig.AUTO_PUNCH_OUT_ON_AIRPLANE_MODE) return;

      final prefs = await SharedPreferences.getInstance();
      final sessionActive = prefs.getBool(_keySessionActive) ?? false;
      final punchedOut = prefs.getBool('user_punched_out') ?? false;
      if (!sessionActive || punchedOut) return;

      final airplane = await DeviceStateService.isAirplaneModeOn();
      if (!airplane) return;

      LogConfig.logHeadless('Airplane mode ON — auto punch-out');
      await _performHeadlessAutoPunchOut(
        reason: PunchOutReasons.AIRPLANE_MODE_ENABLED,
      );
    } catch (e) {
      LogConfig.logError('[Headless] Airplane punch-out error', e);
    }
  }

  /// App / process terminating (swipe kill or phone powering off).
  static Future<void> _handleHeadlessTerminate() async {
    try {
      if (!LocationConfig.AUTO_PUNCH_OUT_ON_APP_KILLED) {
        LogConfig.logHeadless(
            'Terminate ignored — app-killed auto out disabled');
        return;
      }

      final prefs = await SharedPreferences.getInstance();
      final sessionActive = prefs.getBool(_keySessionActive) ?? false;
      final punchedOut = prefs.getBool('user_punched_out') ?? false;
      if (!sessionActive || punchedOut) return;

      final now = DateTime.now().toIso8601String();
      await prefs.setBool(_keyPendingTerminatePunchOut, true);
      await prefs.setString(
          _keyPendingTerminateReason, PunchOutReasons.PHONE_POWERED_OFF);
      await prefs.setString(_keyPendingTerminateAt, now);

      LogConfig.logHeadless(
          'Terminate while punched in — queued PHONE_POWERED_OFF punch-out');

      await _performHeadlessAutoPunchOut(
        reason: PunchOutReasons.PHONE_POWERED_OFF,
      );
    } catch (e) {
      LogConfig.logError('[Headless] Error on terminate', e);
    }
  }

  /// Device rebooted — complete any pending power-off punch-out.
  static Future<void> _handleHeadlessBoot() async {
    try {
      LogConfig.logHeadless(
          'Boot event — processing pending terminate punch-out');
      await processPendingTerminatePunchOut();
    } catch (e) {
      LogConfig.logError('[Headless] Error on boot', e);
    }
  }

  static Future<void> _handleHeadlessPowerSaveChange(
      bg.HeadlessEvent event) async {
    try {
      if (!LocationConfig.AUTO_PUNCH_OUT_ON_POWER_SAVING) return;
      final enabled = event.event == true;
      if (!enabled) return;

      final prefs = await SharedPreferences.getInstance();
      final sessionActive = prefs.getBool(_keySessionActive) ?? false;
      final punchedOut = prefs.getBool('user_punched_out') ?? false;
      if (!sessionActive || punchedOut) return;

      LogConfig.logHeadless('Power saving ON — auto punch-out');
      await _performHeadlessAutoPunchOut(
        reason: PunchOutReasons.GPS_DISABLED_POWER_SAVING,
      );
    } catch (e) {
      LogConfig.logError('[Headless] Power-save punch-out error', e);
    }
  }

  /// Public: send queued punch-out after phone reboot / app restart.
  static Future<void> processPendingTerminatePunchOut() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final pending = prefs.getBool(_keyPendingTerminatePunchOut) ?? false;
      if (!pending) return;

      final sessionActive = prefs.getBool(_keySessionActive) ?? false;
      final punchedOut = prefs.getBool('user_punched_out') ?? false;
      if (punchedOut || !sessionActive) {
        await prefs.remove(_keyPendingTerminatePunchOut);
        await prefs.remove(_keyPendingTerminateReason);
        await prefs.remove(_keyPendingTerminateAt);
        return;
      }

      final reason = prefs.getString(_keyPendingTerminateReason) ??
          PunchOutReasons.PHONE_POWERED_OFF;

      LogConfig.logPunchOut('Processing pending terminate punch-out: $reason');

      await _performHeadlessAutoPunchOut(reason: reason);

      await prefs.remove(_keyPendingTerminatePunchOut);
      await prefs.remove(_keyPendingTerminateReason);
      await prefs.remove(_keyPendingTerminateAt);
    } catch (e) {
      LogConfig.logError('Error processing pending terminate punch-out', e);
    }
  }

  /**
   * Perform auto punch out in headless mode
   */
  static Future<void> _performHeadlessAutoPunchOut({
    required String reason,
    Map<String, dynamic>? cachedUserData,
  }) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final now = DateTime.now();
      final dateFormat = DateFormat('yyyy-MM-dd');
      final timeFormat = DateFormat('HH:mm:ss');
      final currentDate = dateFormat.format(now);
      final currentTime = timeFormat.format(now);

      double latitude = 0.0, longitude = 0.0;
      try {
        final cachedLocationStr = prefs.getString('last_known_location');
        if (cachedLocationStr != null) {
          final cachedLocation = jsonDecode(cachedLocationStr);
          latitude = cachedLocation['latitude'] ?? 0.0;
          longitude = cachedLocation['longitude'] ?? 0.0;
        }
      } catch (e) {
        LogConfig.logError('[Headless] Error getting cached location', e);
      }

      final result = await AttendanceService.submitAutoPunchOut(
        punchOutReason: PunchOutReasons.getReasonDescription(reason),
        punchTime: now,
        latitude: latitude,
        longitude: longitude,
      );

      if (result.success) {
        LogConfig.logSuccess('[Headless] Auto punch out successful');
        await _saveHeadlessAutoPunchOutEvent(reason);
        final prefs2 = await SharedPreferences.getInstance();
        await prefs2.setBool(_keySessionActive, false);
        await prefs2.setBool('user_punched_out', true);
        await prefs2.remove(_keyPendingTerminatePunchOut);
        await prefs2.remove(_keyPendingTerminateReason);
        await prefs2.remove(_keyPendingTerminateAt);
      } else {
        LogConfig.logError(
            '[Headless] Auto punch out failed: ${result.message}');
        await _savePunchOutAttempt(
          reason: reason,
          latitude: latitude,
          longitude: longitude,
          punchOutTime: '$currentDate $currentTime',
          attendanceDate: currentDate,
          timestamp: now.toIso8601String(),
        );
      }
    } catch (e) {
      LogConfig.logError('[Headless] Error performing auto punch out', e);

      try {
        final now = DateTime.now();
        final dateFormat = DateFormat('yyyy-MM-dd');
        final timeFormat = DateFormat('HH:mm:ss');
        final currentDate = dateFormat.format(now);
        final currentTime = timeFormat.format(now);

        double latitude = 0.0, longitude = 0.0;
        try {
          final prefs = await SharedPreferences.getInstance();
          final cachedLocationStr = prefs.getString('last_known_location');
          if (cachedLocationStr != null) {
            final cachedLocation = jsonDecode(cachedLocationStr);
            latitude = cachedLocation['latitude'] ?? 0.0;
            longitude = cachedLocation['longitude'] ?? 0.0;
          }
        } catch (locationError) {
          LogConfig.logError(
              '[Headless] Error getting cached location for failed attempt',
              locationError);
        }

        await _savePunchOutAttempt(
          reason: reason,
          latitude: latitude,
          longitude: longitude,
          punchOutTime: '$currentDate $currentTime',
          attendanceDate: currentDate,
          timestamp: now.toIso8601String(),
        );

        LogConfig.logInfo(
            '[Headless] Saved failed punch out attempt for retry');
      } catch (saveError) {
        LogConfig.logError(
            '[Headless] Error saving failed punch out attempt', saveError);
      }
    }
  }

  /**
   * Save headless auto punch out event
   */
  static Future<void> _saveHeadlessAutoPunchOutEvent(String reason) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final existingEvents = prefs.getStringList('auto_punch_out_events') ?? [];

      final now = DateTime.now();
      final eventData = jsonEncode({
        'reason': reason,
        'reason_description': PunchOutReasons.getReasonDescription(reason),
        'date': DateFormat('yyyy-MM-dd').format(now),
        'time': DateFormat('HH:mm:ss').format(now),
        'timestamp': now.toIso8601String(),
        'headless_mode': true,
      });

      existingEvents.add(eventData);

      if (existingEvents.length > 50) {
        existingEvents.removeRange(0, existingEvents.length - 50);
      }

      await prefs.setStringList('auto_punch_out_events', existingEvents);
      LogConfig.logHeadless('Auto punch out event saved');
    } catch (e) {
      LogConfig.logError('[Headless] Error saving auto punch out event', e);
    }
  }

  // =================== UTILITY METHODS ===================

  /**
   * Delete specific locations by IDs
   */
  Future<void> deleteLocations(List<int> locationIds) async {
    if (locationIds.isEmpty) return;

    try {
      await _dbHelper.deleteLocations(locationIds);
      _locationHistory.removeWhere((loc) => locationIds.contains(loc.id));
      _locationStreamController.add(_locationHistory);

      LogConfig.logCleanup('Deleted ${locationIds.length} locations');
    } catch (e) {
      LogConfig.logError('Error deleting locations', e);
    }
  }

  /**
   * Clean up synced locations from database and memory
   */
  Future<void> _cleanupSyncedLocations() async {
    try {
      final syncedLocations =
          _locationHistory.where((loc) => loc.isSynced).toList();

      if (syncedLocations.isEmpty) return;

      final now = DateTime.now();
      final locationsToDelete = syncedLocations.where((loc) {
        final age = now.difference(loc.timestamp);
        return age > LocationConfig.locationRetentionPeriod;
      }).toList();

      if (locationsToDelete.isNotEmpty) {
        final idsToDelete = locationsToDelete.map((loc) => loc.id).toList();
        await _dbHelper.deleteLocations(idsToDelete);

        _locationHistory.removeWhere((loc) => idsToDelete.contains(loc.id));
        _locationStreamController.add(_locationHistory);

        LogConfig.logCleanup(
            'Cleaned up ${idsToDelete.length} synced locations');
      }
    } catch (e) {
      LogConfig.logError('Error cleaning up synced locations', e);
    }
  }

  /**
   * Delete all locations
   */
  Future<void> deleteAllLocations() async {
    try {
      await _dbHelper.deleteAllLocations();
      _locationHistory.clear();
      _locationStreamController.add(_locationHistory);

      LogConfig.logCleanup('All locations deleted');
    } catch (e) {
      LogConfig.logError('Error deleting all locations', e);
    }
  }

  /**
   * Get storage information
   */
  Future<Map<String, dynamic>> getStorageInfo() async {
    try {
      final sqliteSize = await _dbHelper.getDatabaseSize();
      final locationCount = await _dbHelper.getLocationCount();

      final prefs = await SharedPreferences.getInstance();
      final locationStr = prefs.getString('locations');
      final sharedPrefSize =
          locationStr != null ? locationStr.length / 1024 : 0;

      return {
        'sqlite_size_kb': sqliteSize,
        'sqlite_location_count': locationCount,
        'shared_pref_size_kb': sharedPrefSize,
        'storage_comparison': sqliteSize > 0 && sharedPrefSize > 0
            ? 'SQLite is ${(sharedPrefSize / sqliteSize).toStringAsFixed(2)}x ${sqliteSize > sharedPrefSize ? "smaller" : "larger"} than SharedPreferences'
            : 'No comparison available',
      };
    } catch (e) {
      LogConfig.logError('Error getting storage info', e);
      return {'error': e.toString()};
    }
  }

  /**
   * Get paginated locations
   */
  Future<List<LocationData>> getLocationsPaginated(
      {int page = 0, int pageSize = 20}) async {
    try {
      return await _dbHelper.getLocationsPaginated(
          page: page, pageSize: pageSize);
    } catch (e) {
      LogConfig.logError('Error getting paginated locations', e);
      return [];
    }
  }

  /**
   * Get total location count
   */
  Future<int> getLocationCount() async {
    try {
      return await _dbHelper.getLocationCount();
    } catch (e) {
      LogConfig.logError('Error getting location count', e);
      return 0;
    }
  }

  /**
   * Dispose of resources
   */
  void dispose() {
    _locationStreamController.close();
    _apiTimer?.cancel();
  }
}
