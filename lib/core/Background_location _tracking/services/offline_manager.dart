/**
 * Offline Manager Service
 * 
 * This service handles offline functionality for location tracking.
 * It manages offline state detection, local storage, and bulk uploads
 * when connectivity is restored.
 * 
 * Features:
 * - Monitor internet connectivity
 * - Store locations locally when offline
 * - Bulk upload locations when online (max 50 at a time)
 * - Handle retry logic for failed uploads
 * - Manage offline/online state transitions
 */

import 'dart:async';
import 'package:connectivity_plus/connectivity_plus.dart';

import '../../constants/location_config.dart';
import '../../constants/log_levels.dart';
import '../models/location_model.dart';
import '../services/database_helper.dart';
import '../services/api_service.dart';
import 'location_issue_tracker.dart';

class OfflineManager {
  static final OfflineManager _instance = OfflineManager._internal();
  static OfflineManager get instance => _instance;

  OfflineManager._internal();

  // Offline state
  bool _isOnline = true;
  StreamSubscription<List<ConnectivityResult>>? _connectivitySubscription;
  Timer? _bulkUploadTimer;
  bool _isUploading = false;

  // Callbacks
  Function(bool isOnline)? _onConnectivityChanged;

  /**
   * Initialize offline manager
   */
  Future<void> initialize() async {
    // Check initial connectivity
    await _checkInitialConnectivity();

    // Start monitoring connectivity changes
    _startConnectivityMonitoring();

    // Start bulk upload timer
    _startBulkUploadTimer();

    // LogConfig.logInit('🌐 Offline manager initialized');
  }

  /**
   * Stop offline manager
   */
  void dispose() {
    _connectivitySubscription?.cancel();
    _bulkUploadTimer?.cancel();
    // LogConfig.logCleanup('🌐 Offline manager disposed');
  }

  /**
   * Check initial connectivity
   */
  Future<void> _checkInitialConnectivity() async {
    try {
      final connectivityResults = await Connectivity().checkConnectivity();
      _updateOnlineStatus(connectivityResults);
    } catch (e) {
      // LogConfig.logError('Error checking initial connectivity', e);
      _isOnline = false; // Assume offline if check fails
    }
  }

  /**
   * Start monitoring connectivity changes
   */
  void _startConnectivityMonitoring() {
    _connectivitySubscription = Connectivity()
        .onConnectivityChanged
        .listen((List<ConnectivityResult> results) {
      _updateOnlineStatus(results);
    });
  }

  /**
   * Update online status and handle transitions
   */
  void _updateOnlineStatus(List<ConnectivityResult> results) {
    final wasOnline = _isOnline;
    _isOnline =
        results.isNotEmpty && !results.contains(ConnectivityResult.none);

    if (wasOnline != _isOnline) {
      // LogConfig.logNetwork('📡 Connectivity changed: ${_isOnline ? 'ONLINE' : 'OFFLINE'}');

      // Notify listeners
      _onConnectivityChanged?.call(_isOnline);

      // Handle transition from offline to online
      if (!wasOnline && _isOnline) {
        _handleOnlineTransition();
      }
    }
  }

  /**
   * Handle transition from offline to online
   */
  Future<void> _handleOnlineTransition() async {
    // LogConfig.logNetwork('🌐 Back online - starting bulk upload');

    // Start bulk upload immediately
    unawaited(_performBulkUpload());
  }

  /**
   * Start bulk upload timer
   */
  void _startBulkUploadTimer() {
    _bulkUploadTimer = Timer.periodic(
      Duration(minutes: 5), // Check every 5 minutes
      (timer) => _performBulkUpload(),
    );
  }

  /**
   * Perform bulk upload of pending locations
   */
  Future<void> _performBulkUpload() async {
    if (!_isOnline || _isUploading) return;

    _isUploading = true;

    try {
      final dbHelper = DatabaseHelper.instance;
      final apiService = ApiService();

      // Get all unsent locations
      final unsentLocations = await dbHelper.getUnsentLocations();

      if (unsentLocations.isEmpty) {
        // LogConfig.logApi('📤 No pending locations to upload');
        return;
      }

      // LogConfig.logApi('📤 Starting bulk upload of ${unsentLocations.length} locations');

      // Upload in batches of maximum 50
      const batchSize = 50;
      final batches = _createBatches(unsentLocations, batchSize);

      int successCount = 0;
      int failureCount = 0;

      for (final batch in batches) {
        try {
          // LogConfig.logApi('📤 Uploading batch of ${batch.length} locations');

          // Send batch to API
          final success = await apiService.sendBatchLocationData(batch);

          if (success) {
            // Mark batch as synced
            final batchIds = batch.map((loc) => loc.id).toList();
            await dbHelper.markLocationsAsSynced(batchIds);

            successCount += batch.length;
            // LogConfig.logSuccess('✅ Batch uploaded successfully: ${batch.length} locations');

            // Update issue tracker
            LocationIssueTracker.instance.updateSuccessfulSend();
          } else {
            failureCount += batch.length;
            // LogConfig.logError('❌ Failed to upload batch of ${batch.length} locations');
          }

          // Small delay between batches
          await Future.delayed(Duration(seconds: 1));
        } catch (e) {
          failureCount += batch.length;
          // LogConfig.logError('❌ Error uploading batch', e);
        }
      }

      LogConfig.logApi(
          '📤 Bulk upload completed: $successCount success, $failureCount failed');

      // Clean up synced locations if configured
      if (LocationConfig.AUTO_CLEANUP_ON_PUNCH_OUT) {
        await _cleanupSyncedLocations();
      }
    } catch (e) {
      LogConfig.logError('Error in bulk upload', e);
    } finally {
      _isUploading = false;
    }
  }

  /**
   * Create batches from location list
   */
  List<List<LocationData>> _createBatches(
      List<LocationData> locations, int batchSize) {
    final batches = <List<LocationData>>[];

    for (int i = 0; i < locations.length; i += batchSize) {
      final endIndex =
          (i + batchSize < locations.length) ? i + batchSize : locations.length;
      batches.add(locations.sublist(i, endIndex));
    }

    return batches;
  }

  /**
   * Clean up synced locations from database
   */
  Future<void> _cleanupSyncedLocations() async {
    try {
      final dbHelper = DatabaseHelper.instance;
      final allLocations = await dbHelper.getLocations();

      // Get locations older than retention period that are synced
      final now = DateTime.now();
      final locationsToDelete = allLocations.where((loc) {
        final age = now.difference(loc.timestamp);
        return loc.isSynced && age > LocationConfig.locationRetentionPeriod;
      }).toList();

      if (locationsToDelete.isNotEmpty) {
        final idsToDelete = locationsToDelete.map((loc) => loc.id).toList();
        await dbHelper.deleteLocations(idsToDelete);

        // LogConfig.logCleanup('🧹 Cleaned up ${idsToDelete.length} synced locations');
      }
    } catch (e) {
      LogConfig.logError('Error cleaning up synced locations', e);
    }
  }

  /**
   * Store location locally (for offline usage)
   */
  Future<bool> storeLocationLocally(LocationData location) async {
    try {
      final dbHelper = DatabaseHelper.instance;
      final result = await dbHelper.insertLocation(location);

      if (result > 0) {
        // LogConfig.logDatabase('💾 Location stored locally: ${location.latitude}, ${location.longitude}');
        return true;
      } else {
        LogConfig.logError('Failed to store location locally');
        return false;
      }
    } catch (e) {
      LogConfig.logError('Error storing location locally', e);
      return false;
    }
  }

  /**
   * Get pending locations count
   */
  Future<int> getPendingLocationsCount() async {
    try {
      final dbHelper = DatabaseHelper.instance;
      final unsentLocations = await dbHelper.getUnsentLocations();
      return unsentLocations.length;
    } catch (e) {
      // LogConfig.logError('Error getting pending locations count', e);
      return 0;
    }
  }

  /**
   * Force bulk upload now
   */
  Future<void> forceBulkUpload() async {
    if (!_isOnline) {
      // LogConfig.logNetwork('❌ Cannot force bulk upload - device is offline');
      return;
    }

    // LogConfig.logApi('🚀 Force bulk upload requested');
    await _performBulkUpload();
  }

  /**
   * Set connectivity change callback
   */
  void setConnectivityCallback(Function(bool isOnline) callback) {
    _onConnectivityChanged = callback;
  }

  /**
   * Check if device is online
   */
  bool get isOnline => _isOnline;

  /**
   * Check if currently uploading
   */
  bool get isUploading => _isUploading;
}

/// Extension method for Future.delayed without await
extension FutureExtensions on Future<void> {
  void unawaited() {}
}
