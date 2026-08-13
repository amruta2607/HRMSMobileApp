import 'dart:async';
import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:flutter_background_geolocation/flutter_background_geolocation.dart' as bg;
import 'package:shared_preferences/shared_preferences.dart';

import '../models/location_model.dart';
import '../services/location_service.dart';
import '../services/api_service.dart';

class LocationProvider extends ChangeNotifier {
  final LocationService _locationService = LocationService.instance;
  final ApiService _apiService = ApiService();
  bool _isTracking = false;
  List<LocationData> _locations = [];
  bool _isRefreshing = false;
  bool _isSyncing = false;
  Timer? _periodicRefreshTimer;

  // Pagination variables
  int _currentPage = 0;
  final int _pageSize = 20;
  bool _hasMoreData = true;
  bool _isLoadingMore = false;
  DateTime? _lastRefreshAt;

  bool get isTracking => _isTracking;
  List<LocationData> get locations => _locations;
  bool get isRefreshing => _isRefreshing;
  bool get isSyncing => _isSyncing;
  bool get hasMoreData => _hasMoreData;
  bool get isLoadingMore => _isLoadingMore;
  int get pageSize => _pageSize;

  // Statistics
  int get totalLocations => _totalLocationCount;
  int get syncedCount =>
      _locations.where((location) => location.isSynced).length;
  int get pendingCount =>
      _locations.where((location) => !location.isSynced).length;
  int _totalLocationCount = 0;

  // Getter for location service
  Future<LocationService?> getLocationService() async {
    return _locationService;
  }

  LocationProvider() {
    // Defer heavy TrackingService work so cold start / splash stay responsive.
    Future.delayed(const Duration(milliseconds: 1800), _initialize);
  }

  Future<void> _initialize() async {
    if (_isRefreshing) return;

    // Explicitly disable debug mode to prevent sounds
    try {
      await bg.BackgroundGeolocation.setConfig(bg.Config(debug: false));
    } catch (e) {
      print('Error disabling debug mode: $e');
    }

    try {
      await _locationService.initialize();

      // Load first page of locations
      await _loadPaginatedLocations(resetData: true);

      // Check if tracking was active before app termination
      await _checkTrackingStatus();

      // Listen for location updates
      _locationService.locationStream.listen((updatedLocations) {
        // Get the total count first
        _updateTotalLocationCount();

        // When streaming updates, we reload the first page to see new locations
        _loadPaginatedLocations(resetData: true);
      });

      // Setup periodic refresh when app is in foreground
      _setupPeriodicRefresh();
    } catch (e) {
      print('Error initializing location provider: $e');
      _isRefreshing = false;
      notifyListeners();
    }
  }

  // Setup a timer to refresh locations every 20 minutes while app is in foreground
  void _setupPeriodicRefresh() {
    _periodicRefreshTimer?.cancel();
    _periodicRefreshTimer = Timer.periodic(const Duration(minutes: 20), (timer) {
      // Only refresh if not already refreshing
      if (!_isRefreshing) {
        refreshData(showLoadingIndicator: false);
      }
    });
  }

  // Load paginated locations
  Future<void> _loadPaginatedLocations({bool resetData = false}) async {
    try {
      if (resetData) {
        _currentPage = 0;
        _hasMoreData = true;
        _locations = [];
      }

      if (!_hasMoreData) return;

      _isLoadingMore = true;
      notifyListeners();

      final newLocations = await _locationService.getLocationsPaginated(
        page: _currentPage,
        pageSize: _pageSize,
      );

      // If we got fewer items than the page size, we've reached the end
      if (newLocations.length < _pageSize) {
        _hasMoreData = false;
      }

      // Append or replace locations based on reset parameter
      if (resetData) {
        _locations = newLocations;
      } else {
        _locations.addAll(newLocations);
      }

      _currentPage++;
      _isLoadingMore = false;
      notifyListeners();
    } catch (e) {
      print('❌ Error loading paginated locations: $e');
      _isLoadingMore = false;
      notifyListeners();
    }
  }

  // Load more locations when user scrolls
  Future<void> loadMore() async {
    if (!_isLoadingMore && _hasMoreData) {
      await _loadPaginatedLocations();
    }
  }

  // Update the total count of locations
  Future<void> _updateTotalLocationCount() async {
    try {
      final count = await _locationService.getLocationCount();
      _totalLocationCount = count;
      notifyListeners();
    } catch (e) {
      print('❌ Error getting total location count: $e');
    }
  }

  Future<void> _checkTrackingStatus() async {
    // Get tracking state from the plugin directly
    final state = await bg.BackgroundGeolocation.state;
    _isTracking = state.enabled;

    // If tracking is active but our state doesn't match, synchronize
    if (_isTracking) {
      print("Tracking was already active, syncing state");
    }

    notifyListeners();
  }

  Future<void> startTracking() async {
    if (!_isTracking) {
      await _locationService.startTracking();
      _isTracking = true;

      // Save tracking state
      final prefs = await SharedPreferences.getInstance();
      await prefs.setBool('isTracking', true);

      // Also refresh to get current location
      await refreshData();

      notifyListeners();
    }
  }

  Future<void> stopTracking() async {
    if (_isTracking) {
      await _locationService.stopTracking();
      _isTracking = false;

      // Save tracking state
      final prefs = await SharedPreferences.getInstance();
      await prefs.setBool('isTracking', false);

      // Also refresh to make sure UI is updated
      await refreshData();

      notifyListeners();
    }
  }

  Future<void> refreshData({bool showLoadingIndicator = true}) async {
    if (_isRefreshing) return;

    // Soft throttle — Track Location / lifecycle resume was stacking refreshes.
    final now = DateTime.now();
    if (_lastRefreshAt != null &&
        now.difference(_lastRefreshAt!) < const Duration(seconds: 15) &&
        !showLoadingIndicator) {
      return;
    }
    _lastRefreshAt = now;

    if (showLoadingIndicator) {
      _isRefreshing = true;
      notifyListeners();
    }

    try {
      // First refresh data from storage
      await _locationService.refreshData();

      // Process any batch queue
      await _apiService.processBatchQueue();

      // Retry any failed requests
      await _apiService.retryFailedRequests();

      // Then try to get current location if tracking is active
      if (_isTracking) {
        await _locationService.getCurrentLocation();
      }

      // Update total count
      await _updateTotalLocationCount();

      // Reload first page
      await _loadPaginatedLocations(resetData: true);
    } catch (e) {
      print('Error refreshing data: $e');
    } finally {
      _isRefreshing = false;
      notifyListeners();
    }
  }

  // Get storage information
  Future<Map<String, dynamic>> getStorageInfo() async {
    try {
      return await _locationService.getStorageInfo();
    } catch (e) {
      print('Error getting storage info: $e');
      return {'error': e.toString()};
    }
  }

  // Get pending locations count - useful for UI indicators
  Future<int> getPendingLocationsCount() async {
    try {
      final prefs = await SharedPreferences.getInstance();

      // Get failed requests
      final String? failedRequestsJson =
          prefs.getString('failed_location_requests');
      int failedCount = 0;
      if (failedRequestsJson != null) {
        final List<dynamic> failedRequests = json.decode(failedRequestsJson);
        failedCount = failedRequests.length;
      }

      // Get batch queue
      final String? batchQueueJson = prefs.getString('batch_queue');
      int batchCount = 0;
      if (batchQueueJson != null) {
        final List<dynamic> batchQueue = json.decode(batchQueueJson);
        batchCount = batchQueue.length;
      }

      // Get unsent locations from main storage
      int unsentCount = pendingCount;

      // Return total
      return failedCount + batchCount + unsentCount;
    } catch (e) {
      print('❌ Error getting pending locations count: $e');
      return 0;
    }
  }

  // Delete specified locations
  Future<void> deleteLocations(List<int> locationIds) async {
    try {
      await _locationService.deleteLocations(locationIds);

      // Refresh locations after deletion
      await refreshData();

      notifyListeners();
    } catch (e) {
      print('Error deleting locations: $e');
    }
  }

  // Delete all locations
  Future<void> deleteAllLocations() async {
    try {
      await _locationService.deleteAllLocations();

      // Refresh locations after deletion
      await refreshData();

      notifyListeners();
    } catch (e) {
      print('Error deleting all locations: $e');
    }
  }

  // Manually sync current location immediately
  Future<bool> syncCurrentLocation() async {
    if (_isSyncing) return false;

    _isSyncing = true;
    notifyListeners();

    try {
      // Get current location and send it immediately
      final location = await _locationService.getCurrentLocation();

      if (location != null) {
        // Process any batch queue
        await _apiService.processBatchQueue();

        // Retry any failed requests
        await _apiService.retryFailedRequests();

        // Update total count
        await _updateTotalLocationCount();

        // Reload first page to show updated sync status
        await _loadPaginatedLocations(resetData: true);

        return true;
      }

      return false;
    } catch (e) {
      print('Error syncing current location: $e');
      return false;
    } finally {
      _isSyncing = false;
      notifyListeners();
    }
  }

  @override
  void dispose() {
    _periodicRefreshTimer?.cancel();
    _locationService.dispose();
    super.dispose();
  }
}
