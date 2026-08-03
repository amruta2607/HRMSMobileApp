/**
 * GPS Monitor Service
 *
 * This service monitors GPS/Location services status and handles automatic
 * punch out when GPS is manually disabled by the user while they are punched in.
 *
 * Features:
 * - Detects when GPS is manually turned off
 * - Stores last known location and timestamp
 * - Automatically punches out when internet comes back
 * - Handles offline scenarios with proper caching
 * - Integrates with main app startup flow
 */

import 'dart:async';
import 'dart:convert';
import 'package:geolocator/geolocator.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';
import '../../constants/punch_out_reasons.dart';
import '../../constants/log_levels.dart';
import '../../constants/location_config.dart';
import '../../Utils/Urls/urls.dart';
import '../../Utils/services/token_storage.dart';
import '../../Utils/services/Attendance service/attendance_service.dart';
import 'package:connectivity_plus/connectivity_plus.dart';

class GpsMonitorService {
  static final GpsMonitorService _instance = GpsMonitorService._internal();
  static GpsMonitorService get instance => _instance;

  GpsMonitorService._internal();

  // Monitoring state
  Timer? _gpsCheckTimer;
  bool _isMonitoring = false;
  bool _lastGpsState = true;
  Position? _lastKnownPosition;
  DateTime? _lastPositionTimestamp;
  bool _isUserPunchedIn = false;

  // Storage keys for cache
  static const String _keyPendingPunchOut = 'pending_punch_out_cache';
  static const String _keyPunchOutData = 'punch_out_data_cache';
  static const String _keyLastPosition = 'last_known_position_cache';
  static const String _keyCacheDate = 'punch_out_cache_date';

  /**
   * Start monitoring GPS status
   * Call this when user punches in
   */
  Future<void> startMonitoring() async {
    // Re-arm even if previously started (multi punch-in same day).
    _gpsCheckTimer?.cancel();
    _isMonitoring = true;
    _isUserPunchedIn = true;

    LogConfig.logInfo('🔍 Starting GPS monitoring for automatic punch out');

    _lastGpsState = await Geolocator.isLocationServiceEnabled();
    await _loadLastPosition();

    // Immediate check + frequent polls (was 5 min — too slow to notice GPS off).
    await _checkGpsStatus();
    _gpsCheckTimer = Timer.periodic(const Duration(seconds: 15), (timer) {
      _checkGpsStatus();
    });
  }

  /**
   * Stop monitoring GPS status
   * Call this when user punches out manually
   */
  Future<void> stopMonitoring() async {
    _isMonitoring = false;
    _isUserPunchedIn = false;
    _gpsCheckTimer?.cancel();
    _gpsCheckTimer = null;

    LogConfig.logInfo('🔍 Stopped GPS monitoring');

    // Clear any pending punch out data since user manually punched out
    await _clearPendingPunchOut();
  }

  /**
   * Update last known position
   * Call this whenever location is updated
   */
  void updateLastKnownPosition(Position position) {
    _lastKnownPosition = position;
    _lastPositionTimestamp = DateTime.now();
    _saveLastPosition();
  }

  /**
   * Check for pending punch out (call on app start/internet restore)
   */
  Future<void> checkPendingPunchOut() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final hasPendingPunchOut = prefs.getBool(_keyPendingPunchOut) ?? false;

      if (hasPendingPunchOut) {
        final cacheDate = prefs.getString(_keyCacheDate) ?? '';
        final today = DateFormat('yyyy-MM-dd').format(DateTime.now());

        // Only process if cache is for today
        if (cacheDate == today) {
          LogConfig.logInfo(
              '📍 Found pending punch out for today, processing...');
          await _processPendingPunchOut();
        } else {
          LogConfig.logInfo('📍 Found old punch out cache, clearing...');
          await _clearPendingPunchOut();
        }
      } else {
        LogConfig.logInfo('📍 No pending punch out cache found');
      }
    } catch (e) {
      LogConfig.logError('Error checking pending punch out: $e', e);
    }
  }

  /**
   * Monitor GPS status changes
   */
  Future<void> _checkGpsStatus() async {
    if (!_isMonitoring || !_isUserPunchedIn) return;

    try {
      final currentGpsState = await Geolocator.isLocationServiceEnabled();

      // GPS was turned off
      if (_lastGpsState && !currentGpsState) {
        if (!LocationConfig.AUTO_PUNCH_OUT_ON_GPS_OFF) {
          LogConfig.logInfo('GPS off ignored — dashboard auto punch-out disabled');
        } else {
          LogConfig.logWarning('🚨 GPS turned off while user is punched in!');
          await _handleGpsDisabled();
        }
      }

      _lastGpsState = currentGpsState;
    } catch (e) {
      LogConfig.logError('Error checking GPS status: $e', e);
    }
  }

  /**
   * Handle GPS being disabled - save to cache for offline processing
   */
  Future<void> _handleGpsDisabled() async {
    try {
      LogConfig.logInfo(
          '🔍 Handling GPS disabled manually added --------------------------------');
      final currentTime = DateTime.now();
      final today = DateFormat('yyyy-MM-dd').format(currentTime);

      // Create comprehensive punch out data for cache
      final punchOutData = {
        'timestamp': currentTime.toIso8601String(),
        'punch_out_time':
        '${today}T${DateFormat('HH:mm:ss').format(currentTime)}',
        'attendance_date': today,
        'latitude': _lastKnownPosition?.latitude.toString() ?? '0.0',
        'longitude': _lastKnownPosition?.longitude.toString() ?? '0.0',
        'reason': PunchOutReasons.GPS_DISABLED_BY_USER,
        'auto_punch_out': 'true',
        'gps_disabled_timestamp': currentTime.toIso8601String(),
        'cached_at': currentTime.toIso8601String(),
      };

      // Save to cache
      final prefs = await SharedPreferences.getInstance();
      await prefs.setBool(_keyPendingPunchOut, true);
      await prefs.setString(_keyPunchOutData, jsonEncode(punchOutData));
      await prefs.setString(_keyCacheDate, today);

      LogConfig.logInfo('💾 Stored pending punch out cache: $punchOutData');

      // Try immediate punch out if internet is available
      final hasInternet = await _hasInternetConnection();
      if (hasInternet) {
        LogConfig.logInfo(
            '🌐 Internet available - processing punch out immediately');
        await _processPendingPunchOut();
      } else {
        LogConfig.logInfo(
            '📱 No internet - punch out cached and will be processed when connection is restored-------------------');
      }
    } catch (e) {
      LogConfig.logError('Error handling GPS disabled: $e', e);
    }
  }

  /**
   * Process pending punch out from cache
   */
  Future<void> _processPendingPunchOut() async {
    try {
      LogConfig.logInfo(
          '🔍 Processing pending punch out from cache --------------------------------');
      final prefs = await SharedPreferences.getInstance();
      final punchOutDataStr = prefs.getString(_keyPunchOutData);

      if (punchOutDataStr == null) {
        LogConfig.logWarning('No punch out data found in cache');
        return;
      }

      final punchOutData = jsonDecode(punchOutDataStr);
      final hasInternet = await _hasInternetConnection();

      if (!hasInternet) {
        LogConfig.logInfo(
            '📱 No internet connection - keeping cache for later processing');
        return;
      }

      LogConfig.logInfo(
          '🌐 Processing cached punch out with internet connection');

      // Execute punch out API call
      final success = await _executePunchOut(punchOutData);

      if (success) {
        LogConfig.logSuccess(
            '✅ Automatic punch out completed successfully from cache');
        await _clearPendingPunchOut();

        // Stop monitoring as user is now punched out
        await stopMonitoring();
      } else {
        LogConfig.logError(
            '❌ Failed to execute automatic punch out from cache');
      }
    } catch (e) {
      LogConfig.logError('Error processing pending punch out: $e', e);
    }
  }

  /**
   * Execute punch out API call
   */
  Future<bool> _executePunchOut(Map<String, dynamic> punchOutData) async {
    try {
      final reason = PunchOutReasons.getReasonDescription(
        punchOutData['reason']?.toString() ??
            PunchOutReasons.GPS_DISABLED_BY_USER,
      );
      final lat = double.tryParse('${punchOutData['latitude']}') ?? 0.0;
      final lng = double.tryParse('${punchOutData['longitude']}') ?? 0.0;
      DateTime? when;
      try {
        final raw = punchOutData['punch_out_time']?.toString();
        if (raw != null && raw.isNotEmpty) {
          when = DateTime.tryParse(raw);
        }
      } catch (_) {}

      LogConfig.logApi('🌐 GPS auto punch-out via AttendanceService: $reason');
      final result = await AttendanceService.submitAutoPunchOut(
        punchOutReason: reason,
        punchTime: when,
        latitude: lat,
        longitude: lng,
      );
      if (result.success) {
        LogConfig.logSuccess('✅ Automatic punch out API call successful');
        return true;
      }
      LogConfig.logError('❌ Punch out API failed: ${result.message}');
      return false;
    } catch (e) {
      LogConfig.logError('Error executing punch out from cache: $e', e);
      return false;
    }
  }

  /**
   * Check internet connectivity
   */
  Future<bool> _hasInternetConnection() async {
    try {
      final connectivityResult = await Connectivity().checkConnectivity();
      if (connectivityResult == ConnectivityResult.none || (connectivityResult is List && connectivityResult.contains(ConnectivityResult.none))) {
        return false;
      }

      final response = await http
          .get(Uri.parse('https://www.google.com'))
          .timeout(Duration(seconds: 5));
      return response.statusCode == 200;
    } catch (e) {
      LogConfig.logError('Error checking internet connection: $e', e);
      return false;
    }
  }

  /**
   * Save last known position to storage
   */
  Future<void> _saveLastPosition() async {
    if (_lastKnownPosition == null) return;

    try {
      final prefs = await SharedPreferences.getInstance();
      final positionData = {
        'latitude': _lastKnownPosition!.latitude,
        'longitude': _lastKnownPosition!.longitude,
        'timestamp':
        (_lastPositionTimestamp ?? DateTime.now()).toIso8601String(),
      };

      await prefs.setString(_keyLastPosition, jsonEncode(positionData));
    } catch (e) {
      LogConfig.logError('Error saving last position: $e', e);
    }
  }

  /**
   * Load last known position from storage
   */
  Future<void> _loadLastPosition() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final positionDataStr = prefs.getString(_keyLastPosition);

      if (positionDataStr != null) {
        final positionData = jsonDecode(positionDataStr);
        _lastKnownPosition = Position(
          latitude: positionData['latitude'],
          longitude: positionData['longitude'],
          timestamp: DateTime.parse(positionData['timestamp']),
          accuracy: 0.0,
          altitude: 0.0,
          altitudeAccuracy: 0.0,
          heading: 0.0,
          headingAccuracy: 0.0,
          speed: 0.0,
          speedAccuracy: 0.0,
        );
        _lastPositionTimestamp = DateTime.parse(positionData['timestamp']);
        LogConfig.logInfo('📍 Loaded last known position from cache');
      }
    } catch (e) {
      LogConfig.logError('Error loading last position: $e', e);
    }
  }

  /**
   * Clear pending punch out data from cache
   */
  Future<void> _clearPendingPunchOut() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.remove(_keyPendingPunchOut);
      await prefs.remove(_keyPunchOutData);
      await prefs.remove(_keyCacheDate);
      LogConfig.logInfo('🧹 Cleared pending punch out cache');
    } catch (e) {
      LogConfig.logError('Error clearing pending punch out cache: $e', e);
    }
  }

  /**
   * Initialize service (call on app start)
   */
  Future<void> initialize() async {
    try {
      LogConfig.logInfo('🚀 Initializing GPS Monitor Service...');

      await _loadLastPosition();
      await checkPendingPunchOut();

      LogConfig.logInfo('✅ GPS Monitor Service initialized successfully');
    } catch (e) {
      LogConfig.logError('Error initializing GPS Monitor Service: $e', e);
    }
  }

  /**
   * Get cache status for debugging
   */
  Future<Map<String, dynamic>> getCacheStatus() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      return {
        'hasPendingPunchOut': prefs.getBool(_keyPendingPunchOut) ?? false,
        'cacheDate': prefs.getString(_keyCacheDate) ?? 'None',
        'hasPositionCache': prefs.getString(_keyLastPosition) != null,
        'hasDataCache': prefs.getString(_keyPunchOutData) != null,
      };
    } catch (e) {
      return {'error': e.toString()};
    }
  }

  /**
   * Force process any pending punch out (for debugging)
   */
  Future<void> forceProcessPendingPunchOut() async {
    await _processPendingPunchOut();
  }

  /**
   * Check if monitoring is active
   */
  bool get isMonitoring => _isMonitoring;

  /**
   * Check if user is punched in
   */
  bool get isUserPunchedIn => _isUserPunchedIn;
}
