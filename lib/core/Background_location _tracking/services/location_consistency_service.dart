/**
 * Location Consistency Service
 * 
 * This service monitors location tracking consistency between punch-in and punch-out
 * to prevent users from bypassing attendance by shutting down their phone.
 * 
 * Key Features:
 * - Detects location gaps > 30 minutes
 * - Filters duplicate consecutive locations
 * - Validates location consistency for punch-out eligibility
 * - Prevents bypass by tracking first-hour location density
 */

import 'dart:async';
import 'dart:convert';
import 'dart:math';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:intl/intl.dart';
import '../../constants/location_config.dart';
import '../../constants/punch_out_reasons.dart';
import '../../constants/log_levels.dart';
import '../../Utils/services/token_storage.dart';
import '../models/location_model.dart';
import 'database_helper.dart';

class LocationConsistencyService {
  static final LocationConsistencyService _instance =
      LocationConsistencyService._internal();
  static LocationConsistencyService get instance => _instance;
  LocationConsistencyService._internal();

  // Tracking state
  bool _isTrackingConsistency = false;
  DateTime? _punchInTime;
  List<LocationData> _todayLocations = [];
  LocationData? _lastProcessedLocation;
  int _consecutiveDuplicateCount = 0;

  // Storage keys
  static const String _keyPunchInTime = 'punch_in_time_tracking';
  static const String _keyTodayLocations = 'today_locations_tracking';

  // Callback for gap detection
  Function(String reason, Map<String, dynamic> gapInfo)? _onLocationGapDetected;

  /**
   * Initialize the consistency service
   */
  Future<void> initialize() async {
    LogConfig.logInfo('🔍 Initializing Location Consistency Service');
    await _loadTrackingState();
  }

  /**
   * Start consistency tracking (call on punch-in)
   */
  Future<void> startConsistencyTracking(DateTime punchInTime) async {
    _isTrackingConsistency = true;
    _punchInTime = punchInTime;
    _todayLocations.clear();
    _lastProcessedLocation = null;
    _consecutiveDuplicateCount = 0;

    // Save to persistence
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_keyPunchInTime, punchInTime.toIso8601String());
    await prefs.setString(_keyTodayLocations, jsonEncode([]));

    LogConfig.logInfo(
        '🔍 Started location consistency tracking from ${DateFormat('HH:mm').format(punchInTime)}');
  }

  /**
   * Stop consistency tracking (call on punch-out)
   */
  Future<void> stopConsistencyTracking() async {
    _isTrackingConsistency = false;
    _punchInTime = null;
    _todayLocations.clear();
    _lastProcessedLocation = null;
    _consecutiveDuplicateCount = 0;

    // Clear persistence
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_keyPunchInTime);
    await prefs.remove(_keyTodayLocations);

    LogConfig.logInfo('🔍 Stopped location consistency tracking');
  }

  /**
   * Process new location for consistency tracking
   */
  Future<void> processLocation(LocationData location) async {
    if (!_isTrackingConsistency || _punchInTime == null) return;

    // Filter duplicate locations
    if (_isDuplicateLocation(location)) {
      _consecutiveDuplicateCount++;

      if (_consecutiveDuplicateCount >=
          LocationConfig.MAX_CONSECUTIVE_DUPLICATES) {
        LogConfig.logWarning(
            '🔍 Reached max consecutive duplicates (${LocationConfig.MAX_CONSECUTIVE_DUPLICATES}), allowing this location');
        _consecutiveDuplicateCount = 0;
        _lastProcessedLocation = location;
      } else {
        LogConfig.logInfo(
            '🔍 Filtered duplicate location ${_consecutiveDuplicateCount}/${LocationConfig.MAX_CONSECUTIVE_DUPLICATES}');
        return;
      }
    } else {
      _consecutiveDuplicateCount = 0;
      _lastProcessedLocation = location;
    }

    // Add location to tracking
    _todayLocations.add(location);
    await _saveTrackingState();

    // Check for gaps
    await _checkLocationGaps();

    LogConfig.logInfo(
        '🔍 Processed location: ${location.latitude}, ${location.longitude} at ${DateFormat('HH:mm:ss').format(location.timestamp)}');
  }

  /**
   * Check if location is duplicate of previous
   */
  bool _isDuplicateLocation(LocationData location) {
    if (_lastProcessedLocation == null) return false;

    final distance = _calculateDistance(
      _lastProcessedLocation!.latitude,
      _lastProcessedLocation!.longitude,
      location.latitude,
      location.longitude,
    );

    return distance <= LocationConfig.DUPLICATE_LOCATION_THRESHOLD_METERS;
  }

  /**
   * Calculate distance between two coordinates (Haversine formula)
   */
  double _calculateDistance(
      double lat1, double lon1, double lat2, double lon2) {
    const double earthRadius = 6371000; // Earth radius in meters

    final double dLat = _degreesToRadians(lat2 - lat1);
    final double dLon = _degreesToRadians(lon2 - lon1);

    final double a = sin(dLat / 2) * sin(dLat / 2) +
        cos(_degreesToRadians(lat1)) *
            cos(_degreesToRadians(lat2)) *
            sin(dLon / 2) *
            sin(dLon / 2);

    final double c = 2 * atan2(sqrt(a), sqrt(1 - a));
    return earthRadius * c;
  }

  double _degreesToRadians(double degrees) {
    return degrees * pi / 180;
  }

  /**
   * Check for location gaps that exceed the threshold
   */
  Future<void> _checkLocationGaps() async {
    if (_todayLocations.length < 2) return;

    // Sort locations by timestamp
    _todayLocations.sort((a, b) => a.timestamp.compareTo(b.timestamp));

    DateTime? largestGapStart;
    DateTime? largestGapEnd;
    Duration largestGap = Duration.zero;

    // Check gaps between consecutive locations
    for (int i = 1; i < _todayLocations.length; i++) {
      final previousLocation = _todayLocations[i - 1];
      final currentLocation = _todayLocations[i];

      final gap =
          currentLocation.timestamp.difference(previousLocation.timestamp);

      if (gap > largestGap) {
        largestGap = gap;
        largestGapStart = previousLocation.timestamp;
        largestGapEnd = currentLocation.timestamp;
      }

      // Check if gap exceeds threshold
      if (gap.inMinutes > LocationConfig.MAX_LOCATION_GAP_MINUTES) {
        LogConfig.logWarning(
            '🚨 Location gap detected: ${gap.inMinutes} minutes (${DateFormat('HH:mm').format(previousLocation.timestamp)} - ${DateFormat('HH:mm').format(currentLocation.timestamp)})');

        await _handleLocationGap(
            gap, previousLocation.timestamp, currentLocation.timestamp);
        return;
      }
    }

    // Check gap from last location to current time
    if (_todayLocations.isNotEmpty) {
      final lastLocation = _todayLocations.last;
      final currentGap = DateTime.now().difference(lastLocation.timestamp);

      if (currentGap.inMinutes > LocationConfig.MAX_LOCATION_GAP_MINUTES) {
        LogConfig.logWarning(
            '🚨 Current location gap detected: ${currentGap.inMinutes} minutes since ${DateFormat('HH:mm').format(lastLocation.timestamp)}');

        await _handleLocationGap(
            currentGap, lastLocation.timestamp, DateTime.now());
        return;
      }
    }
  }

  /**
   * Handle detected location gap
   */
  Future<void> _handleLocationGap(
      Duration gap, DateTime gapStart, DateTime gapEnd) async {
    final gapInfo = {
      'gap_duration_minutes': gap.inMinutes,
      'gap_start': gapStart.toIso8601String(),
      'gap_end': gapEnd.toIso8601String(),
      'gap_start_formatted': DateFormat('HH:mm:ss').format(gapStart),
      'gap_end_formatted': DateFormat('HH:mm:ss').format(gapEnd),
      'punch_in_time': _punchInTime?.toIso8601String(),
      'total_locations_received': _todayLocations.length,
    };

    const reason =
        'Location tracking stopped for more than 30 mins (possible device shutdown/restart or other)';

    LogConfig.logError(
        '🚨 Auto punch-out triggered due to location gap: $reason');

    // Trigger callback if set
    _onLocationGapDetected?.call(reason, gapInfo);
  }

  /**
   * Check if user is eligible for manual punch-out
   */
  Future<LocationConsistencyResult> checkPunchOutEligibility() async {
    if (!_isTrackingConsistency || _punchInTime == null) {
      return LocationConsistencyResult(
        isEligible: true,
        reason: 'Consistency tracking not active',
      );
    }

    // Load all locations from database for comprehensive check
    await _loadTodayLocationsFromDatabase();

    // Check for gaps
    final gapResult = await _checkForGaps();
    if (!gapResult.isEligible) {
      return gapResult;
    }

    // Check first-hour location density
    final densityResult = await _checkFirstHourDensity();
    if (!densityResult.isEligible) {
      return densityResult;
    }

    // Check overall consistency
    final consistencyResult = await _checkOverallConsistency();
    return consistencyResult;
  }

  /**
   * Check for location gaps in the entire shift
   */
  Future<LocationConsistencyResult> _checkForGaps() async {
    if (_todayLocations.isEmpty) {
      return LocationConsistencyResult(
        isEligible: false,
        reason: 'No location data found since punch-in',
        details:
            'Please ensure location services are enabled and wait for location updates',
      );
    }

    // Sort by timestamp
    _todayLocations.sort((a, b) => a.timestamp.compareTo(b.timestamp));

    // Check gaps between locations
    for (int i = 1; i < _todayLocations.length; i++) {
      final gap = _todayLocations[i]
          .timestamp
          .difference(_todayLocations[i - 1].timestamp);

      if (gap.inMinutes > LocationConfig.MAX_LOCATION_GAP_MINUTES) {
        final gapStart =
            DateFormat('HH:mm').format(_todayLocations[i - 1].timestamp);
        final gapEnd = DateFormat('HH:mm').format(_todayLocations[i].timestamp);

        return LocationConsistencyResult(
          isEligible: false,
          reason: 'Location gap detected',
          details:
              'Gap of ${gap.inMinutes} minutes found between $gapStart and $gapEnd. Maximum allowed gap is ${LocationConfig.MAX_LOCATION_GAP_MINUTES} minutes.',
        );
      }
    }

    // Check gap from punch-in to first location
    final firstLocation = _todayLocations.first;
    final initialGap = firstLocation.timestamp.difference(_punchInTime!);

    if (initialGap.inMinutes > LocationConfig.MAX_LOCATION_GAP_MINUTES) {
      return LocationConsistencyResult(
        isEligible: false,
        reason: 'Initial location gap',
        details:
            'Gap of ${initialGap.inMinutes} minutes between punch-in and first location. This suggests location tracking was interrupted.',
      );
    }

    return LocationConsistencyResult(
        isEligible: true, reason: 'No gaps detected');
  }

  /**
   * Check first-hour location density to prevent bypass
   */
  Future<LocationConsistencyResult> _checkFirstHourDensity() async {
    final firstHourEnd = _punchInTime!.add(Duration(hours: 1));
    final firstHourLocations = _todayLocations
        .where((location) => location.timestamp.isBefore(firstHourEnd))
        .toList();

    if (firstHourLocations.length < LocationConfig.MIN_LOCATIONS_FIRST_HOUR) {
      return LocationConsistencyResult(
        isEligible: false,
        reason: 'Insufficient first-hour tracking',
        details:
            'Only ${firstHourLocations.length} locations in first hour. Minimum ${LocationConfig.MIN_LOCATIONS_FIRST_HOUR} required to prevent bypass.',
      );
    }

    return LocationConsistencyResult(
        isEligible: true, reason: 'First-hour location density sufficient');
  }

  /**
   * Check overall location consistency
   */
  Future<LocationConsistencyResult> _checkOverallConsistency() async {
    final workingHours = DateTime.now().difference(_punchInTime!).inHours;
    final expectedLocations = (workingHours *
            60 /
            LocationConfig.LOCATION_UPDATE_INTERVAL_SECONDS *
            60)
        .round();
    final actualLocations = _todayLocations.length;

    // Allow 30% variance for normal operation
    final minExpectedLocations = (expectedLocations * 0.7).round();

    if (actualLocations < minExpectedLocations) {
      return LocationConsistencyResult(
        isEligible: false,
        reason: 'Insufficient location data',
        details:
            'Only $actualLocations locations in ${workingHours}h shift. Expected at least $minExpectedLocations locations.',
      );
    }

    return LocationConsistencyResult(
      isEligible: true,
      reason: 'Location tracking consistent',
      details:
          '$actualLocations locations tracked over ${workingHours}h shift.',
    );
  }

  /**
   * Load today's locations from database
   */
  Future<void> _loadTodayLocationsFromDatabase() async {
    if (_punchInTime == null) return;

    try {
      final dbHelper = DatabaseHelper.instance;
      final today = DateFormat('yyyy-MM-dd').format(DateTime.now());

      // Get all locations since punch-in
      final allLocations = await dbHelper.getAllLocations();
      _todayLocations = allLocations
          .where((location) =>
              location.timestamp.isAfter(_punchInTime!) &&
              DateFormat('yyyy-MM-dd').format(location.timestamp) == today)
          .toList();

      LogConfig.logInfo(
          '🔍 Loaded ${_todayLocations.length} locations from database since punch-in');
    } catch (e) {
      LogConfig.logError('Error loading today locations from database: $e', e);
    }
  }

  /**
   * Save tracking state to persistence
   */
  Future<void> _saveTrackingState() async {
    if (_punchInTime == null) return;

    try {
      final prefs = await SharedPreferences.getInstance();
      final locationsJson = _todayLocations.map((loc) => loc.toJson()).toList();
      await prefs.setString(_keyTodayLocations, jsonEncode(locationsJson));
    } catch (e) {
      LogConfig.logError('Error saving tracking state: $e', e);
    }
  }

  /**
   * Load tracking state from persistence
   */
  Future<void> _loadTrackingState() async {
    try {
      final prefs = await SharedPreferences.getInstance();

      // Load punch-in time
      final punchInTimeStr = prefs.getString(_keyPunchInTime);
      if (punchInTimeStr != null) {
        _punchInTime = DateTime.parse(punchInTimeStr);
        _isTrackingConsistency = true;
        LogConfig.logInfo(
            '🔍 Loaded punch-in time from persistence: ${DateFormat('HH:mm').format(_punchInTime!)}');
      }

      // Load today's locations
      final locationsJsonStr = prefs.getString(_keyTodayLocations);
      if (locationsJsonStr != null) {
        final locationsJson = jsonDecode(locationsJsonStr) as List;
        _todayLocations =
            locationsJson.map((json) => LocationData.fromJson(json)).toList();
        LogConfig.logInfo(
            '🔍 Loaded ${_todayLocations.length} locations from persistence');
      }
    } catch (e) {
      LogConfig.logError('Error loading tracking state: $e', e);
    }
  }

  /**
   * Set gap detection callback
   */
  void setOnLocationGapDetected(
      Function(String reason, Map<String, dynamic> gapInfo) callback) {
    _onLocationGapDetected = callback;
  }

  /**
   * Get current consistency status
   */
  Map<String, dynamic> getConsistencyStatus() {
    return {
      'is_tracking': _isTrackingConsistency,
      'punch_in_time': _punchInTime?.toIso8601String(),
      'total_locations': _todayLocations.length,
      'last_location_time': _todayLocations.isNotEmpty
          ? _todayLocations.last.timestamp.toIso8601String()
          : null,
      'consecutive_duplicates': _consecutiveDuplicateCount,
    };
  }

  /**
   * Force process all pending locations (for bulk upload)
   */
  Future<List<LocationData>> getFilteredLocationsForUpload() async {
    await _loadTodayLocationsFromDatabase();

    if (_todayLocations.isEmpty) return [];

    // Apply duplicate filtering to entire list
    final filteredLocations = <LocationData>[];
    LocationData? previousLocation;
    int consecutiveDuplicates = 0;

    for (final location in _todayLocations) {
      if (previousLocation == null) {
        filteredLocations.add(location);
        previousLocation = location;
        continue;
      }

      final distance = _calculateDistance(
        previousLocation.latitude,
        previousLocation.longitude,
        location.latitude,
        location.longitude,
      );

      if (distance <= LocationConfig.DUPLICATE_LOCATION_THRESHOLD_METERS) {
        consecutiveDuplicates++;

        if (consecutiveDuplicates >=
            LocationConfig.MAX_CONSECUTIVE_DUPLICATES) {
          filteredLocations.add(location);
          previousLocation = location;
          consecutiveDuplicates = 0;
        }
      } else {
        filteredLocations.add(location);
        previousLocation = location;
        consecutiveDuplicates = 0;
      }
    }

    LogConfig.logInfo(
        '🔍 Filtered ${_todayLocations.length} locations to ${filteredLocations.length} for upload');
    return filteredLocations;
  }
}

/**
 * Result of location consistency check
 */
class LocationConsistencyResult {
  final bool isEligible;
  final String reason;
  final String? details;

  LocationConsistencyResult({
    required this.isEligible,
    required this.reason,
    this.details,
  });

  Map<String, dynamic> toJson() {
    return {
      'is_eligible': isEligible,
      'reason': reason,
      'details': details,
    };
  }
}
