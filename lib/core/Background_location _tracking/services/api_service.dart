/**
 * API Service for Location Tracking
 *
 * This service handles all API communication for location tracking data.
 * It provides methods to send location data to the server, handle failures,
 * and manage retry mechanisms.
 *
 * Features:
 * - Single and batch location data sending
 * - Automatic retry mechanism for failed requests
 * - Queue management for concurrent requests
 * - Fallback to individual requests when batch fails
 * - Offline support with local storage for failed requests
 * - Sync status management
 */

import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

import '../../Utils/Urls/urls.dart';
import '../../constants/location_config.dart';
import '../../constants/log_levels.dart';
import '../../Utils/services/token_storage.dart';
import '../models/location_model.dart';

class ApiService {
  static int get _requestTimeout => LocationConfig.IMMEDIATE_API_TIMEOUT_SECONDS;
  static int get _batchRequestTimeout => LocationConfig.BATCH_API_TIMEOUT_SECONDS;
  static int get _maxBatchSize => LocationConfig.MAX_BATCH_SIZE;

  // Request state management
  bool _isCallInProgress = false;

  // SharedPreferences keys
  static const String _batchQueueKey = 'batch_queue';
  static const String _failedRequestsKey = 'failed_location_requests';
  static const String _syncStatusKey = 'location_sync_status';
  static const String _locationsKey = 'locations';

  /**
   * Send a single location data point to the API
   * @param locationData - LocationData object to send
   * @returns Future<bool> - Success status
   */
  Future<bool> sendLocationData(LocationData locationData) async {
    if (!LocationConfig.IS_LOCATION_TRACKING_ENABLED) {
      LogConfig.logWarning('Location tracking disabled by config — skip send');
      return false;
    }

    if (_isCallInProgress) {
      LogConfig.logWarning(
          'Another API call in progress, queuing this request');
      await _saveForBatchProcessing(locationData);
      return false;
    }

    _isCallInProgress = true;

    try {
      final userId = await TokenStorage.getUserId();
      if (userId == null) {
        LogConfig.logError('User ID is null, cannot send location');
        return false;
      }

      final requestBody = _buildSingleLocationPayload(locationData, userId);

      LogConfig.logApi(
          'Sending location to API: ${locationData.latitude}, ${locationData.longitude}');

      final response = await _makeHttpRequest(
        url: BaseUrls.addLocationTracking,
        body: requestBody,
        timeout: _requestTimeout,
      );

      if (_isSuccessResponse(response.statusCode)) {
        LogConfig.logSuccess('API request successful: ${response.statusCode}');
        await _markLocationAsSynced(locationData);
        return true;
      } else {
        // If location tracking is disabled by admin, stop retrying entirely
        final body = response.body.toLowerCase();
        if (body.contains('location tracking is disabled') || body.contains('tracking is disabled')) {
          LogConfig.logWarning('🚫 Location tracking disabled by admin — skipping retries & clearing queue');
          await _clearFailedRequests();
          return false;
        }
        LogConfig.logError(
            'API request failed with status: ${response.statusCode}');
        await _saveFailedRequest(locationData);
        return false;
      }
    } catch (e) {
      LogConfig.logError('Error sending location to API', e);
      await _saveFailedRequest(locationData);
      return false;
    } finally {
      _isCallInProgress = false;
    }
  }

  /**
   * Send multiple location data points in a single batch request
   * @param locations - List of LocationData objects to send
   * @returns Future<bool> - Success status
   */
  Future<bool> sendBatchLocationData(List<LocationData> locations) async {
    if (locations.isEmpty) return true;

    if (!LocationConfig.IS_LOCATION_TRACKING_ENABLED) {
      LogConfig.logWarning('Location tracking disabled by config — skip batch');
      return false;
    }

    if (_isCallInProgress) {
      LogConfig.logWarning(
          'Another API call in progress, queuing this batch request');
      for (final location in locations) {
        await _saveForBatchProcessing(location);
      }
      return false;
    }

    _isCallInProgress = true;

    try {
      final userId = await TokenStorage.getUserId();
      if (userId == null) {
        LogConfig.logError('User ID is null, cannot send batch locations');
        return false;
      }

      // Limit batch size to prevent oversized requests
      final locationsToSend = locations.length > _maxBatchSize
          ? locations.sublist(locations.length - _maxBatchSize)
          : locations;

      LogConfig.logApi(
          'Sending batch of ${locationsToSend.length} locations to API');

      final requestBody = _buildBatchLocationPayload(locationsToSend, userId);

      try {
        final response = await _makeHttpRequest(
          url: BaseUrls.addBatchLocation,
          body: requestBody,
          timeout: _batchRequestTimeout,
        );

        if (_isSuccessResponse(response.statusCode)) {
          LogConfig.logSuccess(
              'Batch API request successful: ${response.statusCode}');
          await _markLocationsAsSynced(locationsToSend);
          return true;
        } else {
          // If location tracking is disabled by admin, stop retrying entirely
          final body = response.body.toLowerCase();
          if (body.contains('location tracking is disabled') || body.contains('tracking is disabled')) {
            LogConfig.logWarning('🚫 Location tracking disabled by admin — skipping retries & clearing queue');
            await _clearFailedRequests();
            return false;
          }
          LogConfig.logWarning(
              'Batch API request failed with status: ${response.statusCode}. Falling back to individual sends');
          return await _sendLocationsIndividually(locationsToSend);
        }
      } catch (e) {
        LogConfig.logWarning(
            'Error sending batch to API, trying individual locations: $e');
        return await _sendLocationsIndividually(locationsToSend);
      }
    } catch (e) {
      LogConfig.logError('Error preparing batch request', e);
      for (final location in locations) {
        await _saveFailedRequest(location);
      }
      return false;
    } finally {
      _isCallInProgress = false;
    }
  }

  /**
   * Send locations immediately when internet is available
   * This method handles both single and batch uploads based on available data
   * @param locations - List of LocationData objects to send
   * @returns Future<bool> - Success status
   */
  Future<bool> sendLocationsImmediate(List<LocationData> locations) async {
    if (locations.isEmpty) return true;

    // Check if we have internet connectivity
    if (!await _hasInternetConnection()) {
      LogConfig.logNetwork(
          'No internet connection - queueing locations for later');
      for (final location in locations) {
        await _saveForBatchProcessing(location);
      }
      return false;
    }

    // If we have only one location, send it immediately
    if (locations.length == 1) {
      LogConfig.logApi('Sending single location immediately');
      return await sendLocationData(locations.first);
    }

    // If we have multiple locations, send as batch
    LogConfig.logApi(
        'Sending ${locations.length} locations immediately as batch');
    return await sendBatchLocationData(locations);
  }

  /**
   * Send all pending locations when internet is restored
   * This method handles bulk upload of all cached locations
   */
  Future<void> sendAllPendingLocations() async {
    if (_isCallInProgress) {
      LogConfig.logWarning(
          'Another API call in progress, skipping bulk upload');
      return;
    }

    if (!await _hasInternetConnection()) {
      LogConfig.logNetwork(
          'No internet connection - cannot send pending locations');
      return;
    }

    LogConfig.logNetwork('Internet restored - sending all pending locations');

    // Get all failed requests
    final failedRequests = await _getFailedRequests();
    if (failedRequests.isNotEmpty) {
      LogConfig.logApi('Sending ${failedRequests.length} failed requests');
      await retryFailedRequests();
    }

    // Process batch queue
    await processBatchQueue();
  }

  /**
   * Retry all failed location requests
   * Should be called when connectivity is restored
   */
  Future<void> retryFailedRequests() async {
    if (_isCallInProgress) {
      LogConfig.logWarning(
          'Another API call in progress, skipping retry of failed requests');
      return;
    }

    _isCallInProgress = true;

    try {
      final failedRequests = await _getFailedRequests();

      if (failedRequests.isEmpty) {
        return;
      }

      LogConfig.logBackground(
          'Retrying ${failedRequests.length} failed location requests');

      // Try batch first if multiple requests
      if (failedRequests.length > 1) {
        final batchSuccess = await sendBatchLocationData(failedRequests);
        if (batchSuccess) {
          await _clearFailedRequests();
          LogConfig.logSuccess(
              'Batch retry complete. All ${failedRequests.length} requests succeeded');
          return;
        }
      }

      // Retry individually
      final remainingFailed = <LocationData>[];

      for (final locationData in failedRequests) {
        final success = await _retrySingleLocation(locationData);
        if (!success) {
          remainingFailed.add(locationData);
        }
      }

      // Update failed requests with remaining failures
      await _saveFailedRequests(remainingFailed);

      LogConfig.logBackground(
          'Retry complete. ${failedRequests.length - remainingFailed.length} succeeded, ${remainingFailed.length} failed');
    } catch (e) {
      LogConfig.logError('Error retrying failed requests', e);
    } finally {
      _isCallInProgress = false;
    }
  }

  /**
   * Process any queued batch requests
   * Should be called periodically to process pending requests
   */
  Future<void> processBatchQueue() async {
    if (_isCallInProgress) {
      LogConfig.logWarning(
          'Another API call in progress, skipping batch queue processing');
      return;
    }

    _isCallInProgress = true;

    try {
      final batchQueue = await _getBatchQueue();

      if (batchQueue.isEmpty) {
        return;
      }

      LogConfig.logBackground(
          'Processing ${batchQueue.length} locations from batch queue');

      final batchSuccess = await sendBatchLocationData(batchQueue);
      if (batchSuccess) {
        await _clearBatchQueue();
        LogConfig.logSuccess(
            'Batch queue processing complete. All ${batchQueue.length} requests succeeded');
      } else {
        LogConfig.logError('Failed to process batch queue');
      }
    } catch (e) {
      LogConfig.logError('Error processing batch queue', e);
    } finally {
      _isCallInProgress = false;
    }
  }

  // =================== PRIVATE HELPER METHODS ===================

  /**
   * Make HTTP POST request with proper error handling and auth tokens
   */
  Future<http.Response> _makeHttpRequest({
    required String url,
    required String body,
    required int timeout,
  }) async {
    final token = await TokenStorage.getToken();
    final headers = {
      'Content-Type': 'application/json',
      if (token != null && token.isNotEmpty) 'Authorization': 'Bearer $token',
    };
    print('DEBUG API REQUEST URL: $url');
    print('DEBUG API REQUEST HEADERS: $headers');
    print('DEBUG API REQUEST BODY: $body');
    final response = await http
        .post(
      Uri.parse(url),
      headers: headers,
      body: body,
    )
        .timeout(Duration(seconds: timeout));
    print('DEBUG API RESPONSE STATUS: ${response.statusCode}');
    print('DEBUG API RESPONSE BODY: ${response.body}');
    return response;
  }

  /**
   * Check if HTTP response status indicates success
   */
  bool _isSuccessResponse(int statusCode) {
    return statusCode >= 200 && statusCode < 300;
  }

  /**
   * Check if internet connection is available
   */
  Future<bool> _hasInternetConnection() async {
    try {
      final result = await InternetAddress.lookup('google.com');
      return result.isNotEmpty && result[0].rawAddress.isNotEmpty;
    } catch (e) {
      return false;
    }
  }

  /**
   * Build JSON payload for single location request
   */
  String _buildSingleLocationPayload(LocationData locationData, int userId) {
    return json.encode({
      'user_id': userId,
      'latitude': locationData.latitude,
      'longitude': locationData.longitude,
      'timestamp': _formatDateTime(locationData.timestamp),
      'location_from': locationData.locationFrom,
    });
  }

  /**
   * Build JSON payload for batch location request
   */
  String _buildBatchLocationPayload(List<LocationData> locations, int userId) {
    final locationsList = locations
        .map((loc) => {
      'latitude': loc.latitude,
      'longitude': loc.longitude,
      'timestamp': _formatDateTime(loc.timestamp),
      'location_from': loc.locationFrom,
    })
        .toList();

    return json.encode({
      'user_id': userId,
      'locations': locationsList,
    });
  }

  /**
   * Format DateTime to yyyy-MM-ddTHH:mm:ss format (no milliseconds or timezone)
   */
  String _formatDateTime(DateTime dateTime) {
    final year = dateTime.year.toString();
    final month = dateTime.month.toString().padLeft(2, '0');
    final day = dateTime.day.toString().padLeft(2, '0');
    final hour = dateTime.hour.toString().padLeft(2, '0');
    final minute = dateTime.minute.toString().padLeft(2, '0');
    final second = dateTime.second.toString().padLeft(2, '0');
    return '$year-$month-${day}T$hour:$minute:$second';
  }

  /**
   * Send locations individually as fallback when batch fails
   */
  Future<bool> _sendLocationsIndividually(List<LocationData> locations) async {
    bool allSuccess = true;
    final userId = await TokenStorage.getUserId();
    if (userId == null) {
      LogConfig.logError('User ID is null, cannot send locations individually');
      return false;
    }

    // Only try the most recent locations to avoid too many requests
    final locationsToSend = locations.length > 3
        ? locations.sublist(locations.length - 3)
        : locations;

    for (final location in locationsToSend) {
      try {
        LogConfig.logApi(
            'Sending individual location to API: ${location.latitude}, ${location.longitude}');

        final requestBody = _buildSingleLocationPayload(location, userId);
        final response = await _makeHttpRequest(
          url: BaseUrls.addLocationTracking,
          body: requestBody,
          timeout: _requestTimeout,
        );

        if (_isSuccessResponse(response.statusCode)) {
          LogConfig.logSuccess(
              'Individual API request successful: ${response.statusCode}');
          await _markLocationAsSynced(location);
        } else {
          LogConfig.logError(
              'Individual API request failed with status: ${response.statusCode}');
          await _saveFailedRequest(location);
          allSuccess = false;
        }
      } catch (e) {
        LogConfig.logError('Error sending individual location to API', e);
        await _saveFailedRequest(location);
        allSuccess = false;
      }
    }

    return allSuccess;
  }

  /**
   * Retry a single location request
   */
  Future<bool> _retrySingleLocation(LocationData locationData) async {
    try {
      final userId = await TokenStorage.getUserId();
      if (userId == null) {
        LogConfig.logError('User ID is null, cannot retry location');
        return false;
      }

      final requestBody = _buildSingleLocationPayload(locationData, userId);

      LogConfig.logApi(
          'Retrying individual location: ${locationData.latitude}, ${locationData.longitude}');

      final response = await _makeHttpRequest(
        url: BaseUrls.addLocationTracking,
        body: requestBody,
        timeout: _requestTimeout,
      );

      if (_isSuccessResponse(response.statusCode)) {
        LogConfig.logSuccess(
            'Retry API request successful: ${response.statusCode}');
        await _markLocationAsSynced(locationData);
        return true;
      } else {
        LogConfig.logError(
            'Retry API request failed with status: ${response.statusCode}');
        return false;
      }
    } catch (e) {
      LogConfig.logError('Error retrying location', e);
      return false;
    }
  }

  /**
   * Mark a single location as synchronized
   */
  Future<void> _markLocationAsSynced(LocationData locationData) async {
    await _markLocationsAsSynced([locationData]);
  }

  /**
   * Mark multiple locations as synchronized
   */
  Future<void> _markLocationsAsSynced(List<LocationData> locations) async {
    try {
      final prefs = await SharedPreferences.getInstance();

      // Update sync status map
      final syncMap = await _getSyncStatusMap();
      for (final location in locations) {
        syncMap[location.id.toString()] = true;
      }
      await prefs.setString(_syncStatusKey, json.encode(syncMap));

      // Update locations list in shared preferences
      await _updateLocationsInSharedPreferences(locations);

      // Remove from failed requests and batch queue
      for (final location in locations) {
        await _removeFromFailedRequests(location.id);
        await _removeFromBatchQueue(location.id);
      }
    } catch (e) {
      LogConfig.logError('Error marking locations as synced', e);
    }
  }

  /**
   * Update locations sync status in SharedPreferences
   */
  Future<void> _updateLocationsInSharedPreferences(
      List<LocationData> locations) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final locationsStr = prefs.getString(_locationsKey) ?? '[]';
      final List<dynamic> locationsJson = jsonDecode(locationsStr);
      final locationsList =
      locationsJson.map((json) => LocationData.fromJson(json)).toList();

      // Update sync status for each location
      for (final location in locations) {
        final index = locationsList.indexWhere((loc) => loc.id == location.id);
        if (index != -1) {
          locationsList[index] = LocationData(
            id: location.id,
            latitude: location.latitude,
            longitude: location.longitude,
            timestamp: location.timestamp,
            isSynced: true,
            locationFrom: location.locationFrom,
          );
        }
      }

      await prefs.setString(_locationsKey,
          jsonEncode(locationsList.map((loc) => loc.toJson()).toList()));
    } catch (e) {
      LogConfig.logError('Error updating locations in SharedPreferences', e);
    }
  }

  /**
   * Save location for batch processing
   */
  Future<void> _saveForBatchProcessing(LocationData locationData) async {
    try {
      final batchQueue = await _getBatchQueue();

      // Check if location already exists in queue
      final existingIndex =
      batchQueue.indexWhere((loc) => loc.id == locationData.id);
      if (existingIndex != -1) {
        batchQueue[existingIndex] = locationData;
      } else {
        batchQueue.add(locationData);
      }

      await _saveBatchQueue(batchQueue);
      LogConfig.logDatabase('Saved location for batch processing');
    } catch (e) {
      LogConfig.logError('Error saving location for batch processing', e);
    }
  }

  /**
   * Save failed request for retry later
   */
  Future<void> _saveFailedRequest(LocationData locationData) async {
    try {
      final failedRequests = await _getFailedRequests();

      // Check if location already exists in failed requests
      final existingIndex =
      failedRequests.indexWhere((loc) => loc.id == locationData.id);
      if (existingIndex != -1) {
        failedRequests[existingIndex] = locationData;
      } else {
        failedRequests.add(locationData);
      }

      await _saveFailedRequests(failedRequests);
      LogConfig.logDatabase('Saved failed request for later retry');
    } catch (e) {
      LogConfig.logError('Error saving failed request', e);
    }
  }

  // =================== SHARED PREFERENCES HELPERS ===================

  /**
   * Get sync status map from SharedPreferences
   */
  Future<Map<String, bool>> _getSyncStatusMap() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final syncMapJson = prefs.getString(_syncStatusKey);
      if (syncMapJson != null) {
        return Map<String, bool>.from(json.decode(syncMapJson));
      }
      return {};
    } catch (e) {
      LogConfig.logError('Error getting sync status map', e);
      return {};
    }
  }

  /**
   * Get batch queue from SharedPreferences
   */
  Future<List<LocationData>> _getBatchQueue() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final batchQueueJson = prefs.getString(_batchQueueKey);
      if (batchQueueJson != null) {
        final List<dynamic> batchQueueRaw = json.decode(batchQueueJson);
        return batchQueueRaw
            .map((json) => LocationData.fromJson(json))
            .toList();
      }
      return [];
    } catch (e) {
      LogConfig.logError('Error getting batch queue', e);
      return [];
    }
  }

  /**
   * Save batch queue to SharedPreferences
   */
  Future<void> _saveBatchQueue(List<LocationData> batchQueue) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final batchQueueJson =
      json.encode(batchQueue.map((loc) => loc.toJson()).toList());
      await prefs.setString(_batchQueueKey, batchQueueJson);
    } catch (e) {
      LogConfig.logError('Error saving batch queue', e);
    }
  }

  /**
   * Clear batch queue from SharedPreferences
   */
  Future<void> _clearBatchQueue() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString(_batchQueueKey, '[]');
    } catch (e) {
      LogConfig.logError('Error clearing batch queue', e);
    }
  }

  /**
   * Get failed requests from SharedPreferences
   */
  Future<List<LocationData>> _getFailedRequests() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final failedRequestsJson = prefs.getString(_failedRequestsKey);
      if (failedRequestsJson != null) {
        final List<dynamic> failedRequestsRaw = json.decode(failedRequestsJson);
        return failedRequestsRaw
            .map((json) => LocationData.fromJson(json))
            .toList();
      }
      return [];
    } catch (e) {
      LogConfig.logError('Error getting failed requests', e);
      return [];
    }
  }

  /**
   * Save failed requests to SharedPreferences
   */
  Future<void> _saveFailedRequests(List<LocationData> failedRequests) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final failedRequestsJson =
      json.encode(failedRequests.map((loc) => loc.toJson()).toList());
      await prefs.setString(_failedRequestsKey, failedRequestsJson);
    } catch (e) {
      LogConfig.logError('Error saving failed requests', e);
    }
  }

  /**
   * Clear failed requests from SharedPreferences
   */
  Future<void> _clearFailedRequests() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString(_failedRequestsKey, '[]');
    } catch (e) {
      LogConfig.logError('Error clearing failed requests', e);
    }
  }

  /**
   * Remove location from failed requests
   */
  Future<void> _removeFromFailedRequests(int locationId) async {
    try {
      final failedRequests = await _getFailedRequests();
      failedRequests.removeWhere((loc) => loc.id == locationId);
      await _saveFailedRequests(failedRequests);
    } catch (e) {
      LogConfig.logError('Error removing location from failed requests', e);
    }
  }

  /**
   * Remove location from batch queue
   */
  Future<void> _removeFromBatchQueue(int locationId) async {
    try {
      final batchQueue = await _getBatchQueue();
      batchQueue.removeWhere((loc) => loc.id == locationId);
      await _saveBatchQueue(batchQueue);
    } catch (e) {
      LogConfig.logError('Error removing location from batch queue', e);
    }
  }
}
