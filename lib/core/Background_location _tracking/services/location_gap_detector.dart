/**
 * Location Gap Detector Service
 * 
 * This service detects gaps of 30+ minutes in location tracking and triggers
 * automatic punch-out to prevent users from bypassing attendance tracking.
 * 
 * Key Features:
 * - Detects location gaps > 30 minutes from database or memory
 * - Triggers auto punch-out at first timestamp of gap
 * - Prevents manual punch-out when gaps exist
 * - Runs on app initialization and periodically
 * - Integrates with existing location tracking system
 */

import 'dart:async';
import 'dart:convert';
import 'dart:math' as math;
import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../constants/location_config.dart';
import '../../constants/log_levels.dart';
import '../../Utils/services/token_storage.dart';
import '../../Utils/services/Attendance service/attendance_service.dart';
import '../../Utils/Urls/urls.dart';
import '../models/location_model.dart';
import 'database_helper.dart';

class LocationGapDetector {
  static final LocationGapDetector _instance = LocationGapDetector._internal();
  static LocationGapDetector get instance => _instance;
  LocationGapDetector._internal();

  // State management
  bool _isGapDetectionActive = false;
  DateTime? _punchInTime;
  LocationGapResult? _lastGapDetected;

  // Storage keys
  static const String _keyGapDetected = 'location_gap_detected';
  static const String _keyGapData = 'location_gap_data';
  static const String _keyPunchInTime = 'gap_detector_punch_in_time';

  /**
   * Initialize the gap detector on app startup
   */
  Future<void> initialize() async {
    LogConfig.logInit('🔍 Initializing Location Gap Detector');

    try {
      // Load saved state
      await _loadSavedState();

      // Check for existing gaps on startup
      await checkForLocationGaps();

      LogConfig.logSuccess('✅ Location Gap Detector initialized');
    } catch (e) {
      LogConfig.logError('Error initializing Location Gap Detector', e);
    }
  }

  /**
   * Start gap detection when user punches in
   */
  Future<void> startGapDetection(DateTime punchInTime) async {
    if (!LocationConfig.ENABLE_LOCATION_GAP_VALIDATION) {
      LogConfig.logInfo('Gap validation disabled by dashboard config');
      _isGapDetectionActive = false;
      return;
    }

    _isGapDetectionActive = true;
    _punchInTime = punchInTime;
    _lastGapDetected = null;

    // Save state
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool('gap_detection_active', true);
    await prefs.setString(_keyPunchInTime, punchInTime.toIso8601String());
    await prefs.remove(_keyGapDetected);
    await prefs.remove(_keyGapData);

    LogConfig.logInfo(
        '🔍 Started location gap detection from ${DateFormat('HH:mm:ss').format(punchInTime)}');
  }

  /**
   * Stop gap detection when user punches out
   */
  Future<void> stopGapDetection() async {
    _isGapDetectionActive = false;
    _punchInTime = null;
    _lastGapDetected = null;

    // Clear saved state
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool('gap_detection_active', false);
    await prefs.remove(_keyPunchInTime);
    await prefs.remove(_keyGapDetected);
    await prefs.remove(_keyGapData);

    LogConfig.logInfo('🔍 Stopped location gap detection');
  }

  /**
   * Main method to check for location gaps
   * Called on app startup, periodically, and before manual punch-out
   */
  Future<LocationGapResult> checkForLocationGaps() async {
    if (!_isGapDetectionActive || _punchInTime == null) {
      return LocationGapResult(
        hasGap: false,
        message: 'Gap detection not active or punch-in time not set',
      );
    }

    try {
      LogConfig.logInfo('🔍 Checking for location gaps...');

      // Get all locations since punch-in from database
      final dbHelper = DatabaseHelper.instance;
      final allLocations = await dbHelper.getAllLocations();

      // Filter locations since punch-in
      final relevantLocations = allLocations
          .where((loc) => loc.timestamp.isAfter(_punchInTime!))
          .toList();

      // Sort by timestamp
      relevantLocations.sort((a, b) => a.timestamp.compareTo(b.timestamp));

      LogConfig.logInfo(
          '🔍 Analyzing ${relevantLocations.length} locations since punch-in');

      if (relevantLocations.isEmpty) {
        // No locations since punch-in - check time elapsed
        final timeSincePunchIn = DateTime.now().difference(_punchInTime!);
        if (timeSincePunchIn.inMinutes >
            LocationConfig.MAX_LOCATION_GAP_MINUTES) {
          return await _createGapResult(
            gapStart: _punchInTime!,
            gapEnd: DateTime.now(),
            locations: [],
          );
        }
        return LocationGapResult(
            hasGap: false,
            message: 'No gap detected - insufficient time elapsed');
      }

      // Check gap from punch-in to first location
      final firstLocation = relevantLocations.first;
      final initialGap = firstLocation.timestamp.difference(_punchInTime!);

      if (initialGap.inMinutes > LocationConfig.MAX_LOCATION_GAP_MINUTES) {
        return await _createGapResult(
          gapStart: _punchInTime!,
          gapEnd: firstLocation.timestamp,
          locations: relevantLocations,
        );
      }

      // Check gaps between consecutive locations
      for (int i = 1; i < relevantLocations.length; i++) {
        final previousLocation = relevantLocations[i - 1];
        final currentLocation = relevantLocations[i];
        final gap =
            currentLocation.timestamp.difference(previousLocation.timestamp);

        if (gap.inMinutes > LocationConfig.MAX_LOCATION_GAP_MINUTES) {
          return await _createGapResult(
            gapStart: previousLocation.timestamp,
            gapEnd: currentLocation.timestamp,
            locations: relevantLocations,
          );
        }
      }

      // Check gap from last location to now
      final lastLocation = relevantLocations.last;
      final currentGap = DateTime.now().difference(lastLocation.timestamp);

      if (currentGap.inMinutes > LocationConfig.MAX_LOCATION_GAP_MINUTES) {
        return await _createGapResult(
          gapStart: lastLocation.timestamp,
          gapEnd: DateTime.now(),
          locations: relevantLocations,
        );
      }

      LogConfig.logSuccess('✅ No location gaps detected');
      return LocationGapResult(hasGap: false, message: 'No gaps detected');
    } catch (e) {
      LogConfig.logError('Error checking for location gaps', e);
      return LocationGapResult(
        hasGap: false,
        message: 'Error during gap detection: $e',
      );
    }
  }

  /**
   * Create gap result and trigger auto punch-out
   */
  Future<LocationGapResult> _createGapResult({
    required DateTime gapStart,
    required DateTime gapEnd,
    required List<LocationData> locations,
  }) async {
    final gapDuration = gapEnd.difference(gapStart);

    final gapResult = LocationGapResult(
      hasGap: true,
      gapStart: gapStart,
      gapEnd: gapEnd,
      gapDurationMinutes: gapDuration.inMinutes,
      totalLocations: locations.length,
      message:
          'Location gap detected: ${gapDuration.inMinutes} minutes (${DateFormat('HH:mm:ss').format(gapStart)} - ${DateFormat('HH:mm:ss').format(gapEnd)})',
      autoPunchOutTime: gapStart, // Use first timestamp of gap
      reason:
          'Location tracking stopped for more than ${LocationConfig.MAX_LOCATION_GAP_MINUTES} minutes (e.g., device shutdown or battery restrictions)',
    );

    // Cache the gap result
    _lastGapDetected = gapResult;
    await _saveGapResult(gapResult);

    LogConfig.logWarning('🚨 ${gapResult.message}');

    // Trigger auto punch-out
    await _triggerAutoPunchOut(gapResult);

    return gapResult;
  }

  /**
   * Trigger automatic punch-out due to location gap
   */
  Future<void> _triggerAutoPunchOut(LocationGapResult gapResult) async {
    try {
      final punchOutTime = gapResult.autoPunchOutTime!;
      final reason = gapResult.reason ?? 'Location gap detected';

      double latitude = 0.0, longitude = 0.0;
      try {
        final prefs = await SharedPreferences.getInstance();
        final cachedLocationStr = prefs.getString('last_known_location');
        if (cachedLocationStr != null) {
          final cachedLocation = jsonDecode(cachedLocationStr);
          latitude = cachedLocation['latitude']?.toDouble() ?? 0.0;
          longitude = cachedLocation['longitude']?.toDouble() ?? 0.0;
        }
      } catch (e) {
        LogConfig.logWarning(
            'Could not get cached location for auto punch-out');
      }

      LogConfig.logPunchOut(
          '🚨 Triggering auto punch-out at ${punchOutTime.toIso8601String()}');

      final result = await AttendanceService.submitAutoPunchOut(
        punchOutReason: reason,
        punchTime: punchOutTime,
        latitude: latitude,
        longitude: longitude,
      );

      if (result.success) {
        LogConfig.logSuccess('✅ Auto punch-out successful due to location gap');
        await stopGapDetection();
        final prefs = await SharedPreferences.getInstance();
        await prefs.setBool('user_punched_out', true);
      } else {
        LogConfig.logError('❌ Auto punch-out failed: ${result.message}');
        await _savePunchOutAttempt(gapResult, 0);
      }
    } catch (e) {
      LogConfig.logError('Error triggering auto punch-out', e);
      await _savePunchOutAttempt(gapResult, 0);
    }
  }

  /**
   * Save failed punch-out attempt for retry
   */
  Future<void> _savePunchOutAttempt(
      LocationGapResult gapResult, int userId) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final existingAttempts =
          prefs.getStringList('failed_punch_out_attempts') ?? [];

      final punchOutTime = gapResult.autoPunchOutTime!;
      final dateFormat = DateFormat('yyyy-MM-dd');
      final timeFormat = DateFormat('HH:mm:ss');
      final attendanceDate = dateFormat.format(punchOutTime);
      final punchOutTimeStr =
          '${attendanceDate}T${timeFormat.format(punchOutTime)}';

      // Get cached location
      double latitude = 0.0, longitude = 0.0;
      try {
        final cachedLocationStr = prefs.getString('last_known_location');
        if (cachedLocationStr != null) {
          final cachedLocation = jsonDecode(cachedLocationStr);
          latitude = cachedLocation['latitude']?.toDouble() ?? 0.0;
          longitude = cachedLocation['longitude']?.toDouble() ?? 0.0;
        }
      } catch (e) {
        // Use default coordinates
      }

      final reason = gapResult.reason ?? 'Location gap detected';
      final attemptData = jsonEncode({
        'userId': userId.toString(),
        'punch_out_time': punchOutTimeStr,
        'longitude': longitude.toString(),
        'latitude': latitude.toString(),
        'attendance_date': attendanceDate,
        'Manual': 'false',
        'PunchOutReason': reason,
        'punch_out_reason': reason,
        'gap_duration_minutes': gapResult.gapDurationMinutes.toString(),
        'timestamp': punchOutTime.toIso8601String(),
        'retry_count': 0,
      });

      existingAttempts.add(attemptData);
      await prefs.setStringList('failed_punch_out_attempts', existingAttempts);

      LogConfig.logDatabase(
          '💾 Saved failed gap-based punch-out attempt for retry');
    } catch (e) {
      LogConfig.logError('Error saving failed punch-out attempt', e);
    }
  }

  /**
   * Check if manual punch-out should be prevented due to detected gaps
   */
  Future<bool> shouldPreventManualPunchOut() async {
    // Always check for gaps before allowing manual punch-out
    final gapResult = await checkForLocationGaps();

    if (gapResult.hasGap) {
      LogConfig.logWarning(
          '🚫 Preventing manual punch-out due to detected location gap');
      return true;
    }

    return false;
  }

  /**
   * Get current gap status for UI display
   */
  Future<LocationGapResult?> getCurrentGapStatus() async {
    if (_lastGapDetected != null) {
      return _lastGapDetected;
    }

    // Check for saved gap result
    return await _loadGapResult();
  }

  /**
   * Save gap result to persistence
   */
  Future<void> _saveGapResult(LocationGapResult gapResult) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setBool(_keyGapDetected, true);
      await prefs.setString(_keyGapData, jsonEncode(gapResult.toJson()));
      LogConfig.logDatabase('💾 Saved gap result to cache');
    } catch (e) {
      LogConfig.logError('Error saving gap result', e);
    }
  }

  /**
   * Load gap result from persistence
   */
  Future<LocationGapResult?> _loadGapResult() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final hasGap = prefs.getBool(_keyGapDetected) ?? false;

      if (hasGap) {
        final gapDataStr = prefs.getString(_keyGapData);
        if (gapDataStr != null) {
          final gapData = jsonDecode(gapDataStr);
          return LocationGapResult.fromJson(gapData);
        }
      }

      return null;
    } catch (e) {
      LogConfig.logError('Error loading gap result', e);
      return null;
    }
  }

  /**
   * Load saved state on initialization
   */
  Future<void> _loadSavedState() async {
    try {
      final prefs = await SharedPreferences.getInstance();

      // Load gap detection state
      _isGapDetectionActive = prefs.getBool('gap_detection_active') ?? false;

      // Load punch-in time
      final punchInTimeStr = prefs.getString(_keyPunchInTime);
      if (punchInTimeStr != null) {
        _punchInTime = DateTime.parse(punchInTimeStr);
      }

      // Load last gap result
      _lastGapDetected = await _loadGapResult();

      if (_isGapDetectionActive && _punchInTime != null) {
        LogConfig.logInfo(
            '🔍 Restored gap detection state - active since ${DateFormat('HH:mm:ss').format(_punchInTime!)}');
      }
    } catch (e) {
      LogConfig.logError('Error loading saved state', e);
    }
  }

  /**
   * Get detection status for debugging
   */
  Map<String, dynamic> getDetectionStatus() {
    return {
      'is_active': _isGapDetectionActive,
      'punch_in_time': _punchInTime?.toIso8601String(),
      'has_gap_detected': _lastGapDetected?.hasGap ?? false,
      'last_gap_message': _lastGapDetected?.message,
    };
  }

  /**
   * Force gap check (for testing/debugging)
   */
  Future<LocationGapResult> forceGapCheck() async {
    LogConfig.logInfo('🔍 Force checking for location gaps...');

    // Debug: Show current state
    final status = getDetectionStatus();
    LogConfig.logInfo('🔍 Gap detection status: $status');

    // Debug: Show database content
    await _debugLocationData();

    return await checkForLocationGaps();
  }

  /**
   * Debug method to show location data from database
   */
  Future<void> _debugLocationData() async {
    try {
      final dbHelper = DatabaseHelper.instance;
      final allLocations = await dbHelper.getAllLocations();

      LogConfig.logInfo(
          '📊 Database contains ${allLocations.length} total locations');

      if (_punchInTime != null) {
        final relevantLocations = allLocations
            .where((loc) => loc.timestamp.isAfter(_punchInTime!))
            .toList();
        relevantLocations.sort((a, b) => a.timestamp.compareTo(b.timestamp));

        LogConfig.logInfo(
            '📊 ${relevantLocations.length} locations since punch-in at ${DateFormat('HH:mm:ss').format(_punchInTime!)}');

        // Show first 5 and last 5 locations for debugging
        for (int i = 0; i < relevantLocations.length && i < 5; i++) {
          final loc = relevantLocations[i];
          LogConfig.logInfo(
              '📍 Location ${i + 1}: ${DateFormat('HH:mm:ss').format(loc.timestamp)}');
        }

        if (relevantLocations.length > 5) {
          LogConfig.logInfo(
              '📍 ... (${relevantLocations.length - 5} more locations)');

          // Show last 2 locations
          for (int i = math.max(0, relevantLocations.length - 2);
              i < relevantLocations.length;
              i++) {
            final loc = relevantLocations[i];
            LogConfig.logInfo(
                '📍 Location ${i + 1}: ${DateFormat('HH:mm:ss').format(loc.timestamp)}');
          }
        }
      }
    } catch (e) {
      LogConfig.logError('Error debugging location data', e);
    }
  }
}

/**
 * Result of location gap detection
 */
class LocationGapResult {
  final bool hasGap;
  final DateTime? gapStart;
  final DateTime? gapEnd;
  final int gapDurationMinutes;
  final int totalLocations;
  final String message;
  final DateTime? autoPunchOutTime;
  final String? reason;

  LocationGapResult({
    required this.hasGap,
    this.gapStart,
    this.gapEnd,
    this.gapDurationMinutes = 0,
    this.totalLocations = 0,
    required this.message,
    this.autoPunchOutTime,
    this.reason,
  });

  Map<String, dynamic> toJson() {
    return {
      'has_gap': hasGap,
      'gap_start': gapStart?.toIso8601String(),
      'gap_end': gapEnd?.toIso8601String(),
      'gap_duration_minutes': gapDurationMinutes,
      'total_locations': totalLocations,
      'message': message,
      'auto_punch_out_time': autoPunchOutTime?.toIso8601String(),
      'reason': reason,
    };
  }

  static LocationGapResult fromJson(Map<String, dynamic> json) {
    return LocationGapResult(
      hasGap: json['has_gap'] ?? false,
      gapStart:
          json['gap_start'] != null ? DateTime.parse(json['gap_start']) : null,
      gapEnd: json['gap_end'] != null ? DateTime.parse(json['gap_end']) : null,
      gapDurationMinutes: json['gap_duration_minutes'] ?? 0,
      totalLocations: json['total_locations'] ?? 0,
      message: json['message'] ?? '',
      autoPunchOutTime: json['auto_punch_out_time'] != null
          ? DateTime.parse(json['auto_punch_out_time'])
          : null,
      reason: json['reason'],
    );
  }
}
