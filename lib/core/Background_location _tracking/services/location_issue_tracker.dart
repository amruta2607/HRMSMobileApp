/**
 * Location Issue Tracker Service
 * 
 * This service monitors location tracking issues and reports them to the server
 * when location data is not being sent properly. It helps track and debug
 * location tracking problems.
 * 
 * Features:
 * - Monitor location tracking status
 * - Report issues when location data is not being sent
 * - Track different types of location issues
 * - Automatic issue reporting every 2 minutes
 * - Store last known location for issue reporting
 */

import 'dart:async';
import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:geolocator/geolocator.dart';
import 'package:connectivity_plus/connectivity_plus.dart';

import '../../Utils/Urls/urls.dart';
import '../../constants/location_config.dart';
import '../../constants/log_levels.dart';
import '../../Utils/services/token_storage.dart';
import 'database_helper.dart';


class LocationIssueTracker {
  static final LocationIssueTracker _instance =
      LocationIssueTracker._internal();
  static LocationIssueTracker get instance => _instance;

  LocationIssueTracker._internal();

  // Issue tracking state
  Timer? _issueMonitorTimer;
  DateTime? _lastLocationUpdate;
  DateTime? _lastSuccessfulSend;
  DateTime? _lastIssueReport;
  Position? _lastKnownLocation;
  bool _isMonitoring = false;

  // Issue types
  static const String ISSUE_NO_LOCATION_UPDATES =
      'No location updates received';
  static const String ISSUE_LOCATION_PERMISSION_DENIED =
      'Location permission denied';
  static const String ISSUE_LOCATION_SERVICES_DISABLED =
      'Location services disabled';
  static const String ISSUE_GPS_DISABLED = 'GPS disabled';
  static const String ISSUE_NETWORK_ERROR = 'Network error while sending data';
  static const String ISSUE_API_ERROR = 'API error while sending data';
  static const String ISSUE_DATABASE_ERROR = 'Database error storing location';
  static const String ISSUE_LOCATION_ACCURACY_LOW = 'Location accuracy too low';
  static const String ISSUE_LOCATION_TIMEOUT = 'Location request timeout';
  static const String ISSUE_UNKNOWN_ERROR =
      'Unknown error in location tracking';

  /**
   * Start monitoring location tracking issues
   */
  Future<void> startMonitoring() async {
    if (_isMonitoring) return;

    _isMonitoring = true;
    LogConfig.logInfo('🚀 Location issue tracking started');

    // Start monitoring timer - check every 2 minutes
    _issueMonitorTimer = Timer.periodic(
      LocationConfig.apiCallInterval,
      (timer) => _checkForIssues(),
    );

    // Initialize last successful send time
    _lastSuccessfulSend = DateTime.now();
  }

  /**
   * Stop monitoring location tracking issues
   */
  void stopMonitoring() {
    if (!_isMonitoring) return;

    _isMonitoring = false;
    _issueMonitorTimer?.cancel();
    _issueMonitorTimer = null;

    LogConfig.logInfo('🛑 Location issue tracking stopped');
  }

  /**
   * Update last location update time
   */
  void updateLocationReceived(Position position) {
    _lastLocationUpdate = DateTime.now();
    _lastKnownLocation = position;
    LogConfig.logLocation('📍 Location update received for issue tracking');
  }

  /**
   * Update last successful send time
   */
  void updateSuccessfulSend() {
    _lastSuccessfulSend = DateTime.now();
    LogConfig.logApi('✅ Successful location send recorded');
  }

  /**
   * Report a specific issue immediately
   */
  Future<void> reportIssue(String issueType, String details) async {
    try {
      await _sendIssueReport(issueType, details);
    } catch (e) {
      LogConfig.logError('Error reporting issue immediately', e);
    }
  }

  /**
   * Check for location tracking issues
   */
  Future<void> _checkForIssues() async {
    try {
      if (!_isMonitoring) return;

      final now = DateTime.now();

      // Check if we should report an issue (limit to once per 5 minutes)
      if (_lastIssueReport != null &&
          now.difference(_lastIssueReport!).inMinutes < 5) {
        return;
      }

      // Check for no location updates
      if (_lastLocationUpdate == null ||
          now.difference(_lastLocationUpdate!).inMinutes > 5) {
        await _reportLocationIssue(ISSUE_NO_LOCATION_UPDATES,
            'No location updates received for ${_lastLocationUpdate != null ? now.difference(_lastLocationUpdate!).inMinutes : "unknown"} minutes');
        return;
      }

      // Check for no successful sends
      if (_lastSuccessfulSend == null ||
          now.difference(_lastSuccessfulSend!).inMinutes > 10) {
        await _reportLocationIssue(ISSUE_API_ERROR,
            'No successful location sends for ${_lastSuccessfulSend != null ? now.difference(_lastSuccessfulSend!).inMinutes : "unknown"} minutes');
        return;
      }

      // Check for unsent locations in database
      await _checkUnsentLocations();

      // Check location services status
      await _checkLocationServicesStatus();
    } catch (e) {
      LogConfig.logError('Error in location issue check', e);
    }
  }

  /**
   * Check for unsent locations in database
   */
  Future<void> _checkUnsentLocations() async {
    try {
      final dbHelper = DatabaseHelper.instance;
      final unsentLocations = await dbHelper.getUnsentLocations();

      // If we have more than 10 unsent locations, report an issue
      if (unsentLocations.length > 10) {
        await _reportLocationIssue(ISSUE_NETWORK_ERROR,
            'Too many unsent locations in database: ${unsentLocations.length} locations pending');
      }
    } catch (e) {
      LogConfig.logError('Error checking unsent locations', e);
    }
  }

  /**
   * Check location services status
   */
  Future<void> _checkLocationServicesStatus() async {
    try {
      // Check if location services are enabled
      final serviceEnabled = await Geolocator.isLocationServiceEnabled();
      if (!serviceEnabled) {
        await _reportLocationIssue(ISSUE_LOCATION_SERVICES_DISABLED,
            'Location services are disabled on device');
        return;
      }

      // Check location permissions
      final permission = await Geolocator.checkPermission();
      if (permission == LocationPermission.denied ||
          permission == LocationPermission.deniedForever) {
        await _reportLocationIssue(ISSUE_LOCATION_PERMISSION_DENIED,
            'Location permission is denied: $permission');
        return;
      }
    } catch (e) {
      LogConfig.logError('Error checking location services status', e);
    }
  }

  /**
   * Report a location issue to the API
   */
  Future<void> _reportLocationIssue(String issueType, String details) async {
    try {
      await _sendIssueReport(issueType, details);
      _lastIssueReport = DateTime.now();
    } catch (e) {
      LogConfig.logError('Error reporting location issue', e);
    }
  }

  /**
   * Send issue report to API
   */
  Future<void> _sendIssueReport(String issueType, String details) async {
    try {
      // Check internet connectivity
      final connectivity = await Connectivity().checkConnectivity();
      if (connectivity == ConnectivityResult.none || (connectivity is List && connectivity.contains(ConnectivityResult.none))) {
        LogConfig.logNetwork('No internet connection for issue reporting');
        return;
      }

      // Get user data
      final userId = await TokenStorage.getUserId();
      if (userId == null) {
        LogConfig.logError('User ID not found for issue reporting');
        return;
      }

      // Prepare location data
      double latitude = 0.0;
      double longitude = 0.0;
      if (_lastKnownLocation != null) {
        latitude = _lastKnownLocation!.latitude;
        longitude = _lastKnownLocation!.longitude;
      } else {
        // Try to get last location from database
        final dbHelper = DatabaseHelper.instance;
        final lastDbLocation = await dbHelper.getLastLocation();
        if (lastDbLocation != null) {
          latitude = lastDbLocation.latitude;
          longitude = lastDbLocation.longitude;
        }
      }

      // Prepare API payload matching swagger /apipunch/location-tracking/add-issue
      final now = DateTime.now();
      final stamp =
          '${now.year.toString().padLeft(4, '0')}-'
          '${now.month.toString().padLeft(2, '0')}-'
          '${now.day.toString().padLeft(2, '0')}T'
          '${now.hour.toString().padLeft(2, '0')}:'
          '${now.minute.toString().padLeft(2, '0')}:'
          '${now.second.toString().padLeft(2, '0')}';
      final payload = {
        'user_id': userId,
        'issue_type': issueType,
        'issue_description': details,
        'timestamp': stamp,
        'last_known_latitude': latitude,
        'last_known_longitude': longitude,
        'device_id': Platform.isAndroid
            ? 'android'
            : (Platform.isIOS ? 'ios' : 'unknown'),
      };

      final token = await TokenStorage.getToken();
      final headers = {
        'Content-Type': 'application/json',
        if (token != null && token.isNotEmpty) 'Authorization': 'Bearer $token',
      };

      // Send API request
      final response = await http
          .post(
            Uri.parse(BaseUrls.addLocationTrackingIssue),
            headers: headers,
            body: jsonEncode(payload),
          )
          .timeout(Duration(seconds: LocationConfig.NETWORK_TIMEOUT_SECONDS));

      if (response.statusCode >= 200 && response.statusCode < 300) {
        LogConfig.logSuccess('✅ Location issue report sent successfully');
        final responseData = jsonDecode(response.body);
        LogConfig.logApi('Issue report response: $responseData');
      } else {
        LogConfig.logError(
            '❌ Failed to send issue report', 'Status: ${response.statusCode}');
      }
    } catch (e) {
      LogConfig.logError('Error sending location issue report', e);
    }
  }

  /**
   * Get monitoring status
   */
  bool get isMonitoring => _isMonitoring;

  /**
   * Get last location update time
   */
  DateTime? get lastLocationUpdate => _lastLocationUpdate;

  /**
   * Get last successful send time
   */
  DateTime? get lastSuccessfulSend => _lastSuccessfulSend;

  /**
   * Get last issue report time
   */
  DateTime? get lastIssueReport => _lastIssueReport;

  /**
   * Get last known location
   */
  Position? get lastKnownLocation => _lastKnownLocation;
}
