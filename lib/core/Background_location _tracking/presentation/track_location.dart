import 'dart:async';
import 'dart:convert';
import 'dart:math' as math;
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../providers/location_provider.dart';
import '../widgets/location_card.dart';
import '../services/location_service.dart';
import '../services/location_config_service.dart';
import '../models/location_model.dart';
import '../../constants/location_config.dart';
import '../../Utils/Urls/urls.dart';

class LocationTracker extends StatefulWidget {
  const LocationTracker({super.key});

  @override
  State<LocationTracker> createState() => _LocationTrackerState();
}

class _LocationTrackerState extends State<LocationTracker>
    with SingleTickerProviderStateMixin, WidgetsBindingObserver {
  bool _permissionsGranted = true;
  late final AnimationController _animationController;
  bool _isRefreshing = false;

  // Storage information
  Map<String, dynamic> _storageInfo = {};
  bool _isLoadingStorageInfo = false;

  // Selection mode for deletion
  bool _isInSelectionMode = false;
  final Set<int> _selectedLocationIds = {};

  // ScrollController for pagination
  final ScrollController _scrollController = ScrollController();

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 500),
      vsync: this,
    );
    _animationController.forward();

    // Register for lifecycle events
    WidgetsBinding.instance.addObserver(this);

    // Initially we are in foreground
    LocationService.instance.isInForeground = true;

    // Force refresh data when screen loads
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _refreshData();
      _loadStorageInfo();
    });

    // Add scroll listener for pagination
    _scrollController.addListener(_scrollListener);
  }

  void _scrollListener() {
    if (_scrollController.position.pixels >=
        _scrollController.position.maxScrollExtent - 500) {
      // Load more data when we're near the end
      final provider = Provider.of<LocationProvider>(context, listen: false);
      if (!provider.isLoadingMore && provider.hasMoreData) {
        provider.loadMore();
      }
    }
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _animationController.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    super.didChangeAppLifecycleState(state);

    // Update the foreground status for the location service
    if (state == AppLifecycleState.resumed) {
      LocationService.instance.isInForeground = true;
      // Refresh data when coming to foreground
      _refreshData();
      _loadStorageInfo();
    } else if (state == AppLifecycleState.paused) {
      LocationService.instance.isInForeground = false;
    }
  }

  Future<void> _refreshData() async {
    if (_isRefreshing) return;

    setState(() {
      _isRefreshing = true;
    });

    try {
      final provider = Provider.of<LocationProvider>(context, listen: false);
      await provider.refreshData();
    } catch (e) {
      print('Error refreshing data: $e');
    } finally {
      if (mounted) {
        setState(() {
          _isRefreshing = false;
        });
      }
    }
  }

  Future<void> _syncCurrentLocation() async {
    final provider = Provider.of<LocationProvider>(context, listen: false);
    if (provider.isSyncing) return;

    try {
      final success = await provider.syncCurrentLocation();

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(success
                ? 'Location synced successfully'
                : 'Failed to sync location'),
            duration: const Duration(seconds: 2),
            backgroundColor: success ? Colors.green : Colors.red,
          ),
        );
      }
    } catch (e) {
      print('Error syncing location: $e');
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Error syncing location: $e'),
            duration: const Duration(seconds: 2),
            backgroundColor: Colors.red,
          ),
        );
      }
    }
  }

  Future<void> _loadStorageInfo() async {
    if (_isLoadingStorageInfo) return;

    setState(() {
      _isLoadingStorageInfo = true;
    });

    try {
      final provider = Provider.of<LocationProvider>(context, listen: false);
      final storageInfo = await provider.getStorageInfo();

      if (mounted) {
        setState(() {
          _storageInfo = storageInfo;
          _isLoadingStorageInfo = false;
        });
      }
    } catch (e) {
      print('Error loading storage info: $e');
      if (mounted) {
        setState(() {
          _isLoadingStorageInfo = false;
        });
      }
    }
  }

  // Toggle selection mode
  void _toggleSelectionMode() {
    setState(() {
      _isInSelectionMode = !_isInSelectionMode;
      // Clear selections when exiting selection mode
      if (!_isInSelectionMode) {
        _selectedLocationIds.clear();
      }
    });
  }

  // Toggle location selection
  void _toggleLocationSelection(int locationId) {
    setState(() {
      if (_selectedLocationIds.contains(locationId)) {
        _selectedLocationIds.remove(locationId);
      } else {
        _selectedLocationIds.add(locationId);
      }

      // If no locations selected, exit selection mode
      if (_selectedLocationIds.isEmpty && _isInSelectionMode) {
        _isInSelectionMode = false;
      }
    });
  }

  // Delete selected locations
  Future<void> _deleteSelectedLocations() async {
    if (_selectedLocationIds.isEmpty) return;

    final provider = Provider.of<LocationProvider>(context, listen: false);
    final locationService = await provider.getLocationService();

    if (locationService != null) {
      await locationService.deleteLocations(_selectedLocationIds.toList());
      await _refreshData();
      await _loadStorageInfo();

      setState(() {
        _isInSelectionMode = false;
        _selectedLocationIds.clear();
      });

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('${_selectedLocationIds.length} location(s) deleted'),
            duration: const Duration(seconds: 2),
          ),
        );
      }
    }
  }

  // Delete a single location
  Future<void> _deleteLocation(int locationId) async {
    final provider = Provider.of<LocationProvider>(context, listen: false);
    final locationService = await provider.getLocationService();

    if (locationService != null) {
      await locationService.deleteLocations([locationId]);
      await _refreshData();
      await _loadStorageInfo();

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Location deleted'),
            duration: Duration(seconds: 2),
          ),
        );
      }
    }
  }

  // Delete all locations
  Future<void> _deleteAllLocations() async {
    final provider = Provider.of<LocationProvider>(context, listen: false);
    final locationService = await provider.getLocationService();

    if (locationService != null) {
      final shouldDelete = await showDialog<bool>(
        context: context,
        builder: (context) => AlertDialog(
          title: const Text('Delete All Locations?'),
          content: const Text(
              'Are you sure you want to delete all tracked locations? This cannot be undone.'),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(false),
              child: const Text('CANCEL'),
            ),
            TextButton(
              onPressed: () => Navigator.of(context).pop(true),
              style: TextButton.styleFrom(foregroundColor: Colors.red),
              child: const Text('DELETE ALL'),
            ),
          ],
        ),
      );

      if (shouldDelete == true) {
        await locationService.deleteAllLocations();
        await _refreshData();
        await _loadStorageInfo();

        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('All locations deleted'),
              duration: Duration(seconds: 2),
            ),
          );
        }
      }
    }
  }

  // Show storage information dialog
  void _showStorageInfoDialog() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Storage Information'),
        content: _isLoadingStorageInfo
            ? const Center(child: CircularProgressIndicator())
            : Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'SQLite Database:',
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                  const SizedBox(height: 8),
                  Text(
                      'Size: ${_storageInfo['sqlite_size_kb']?.toStringAsFixed(2) ?? '0'} KB'),
                  Text(
                      'Locations: ${_storageInfo['sqlite_location_count'] ?? '0'}'),
                  const SizedBox(height: 16),
                  Text(
                    'SharedPreferences:',
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                  const SizedBox(height: 8),
                  Text(
                      'Size: ${_storageInfo['shared_pref_size_kb']?.toStringAsFixed(2) ?? '0'} KB'),
                  const SizedBox(height: 16),
                  Text(
                    'Comparison:',
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                  const SizedBox(height: 8),
                  Text(_storageInfo['storage_comparison'] ??
                      'No comparison available'),
                ],
              ),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.of(context).pop();
            },
            child: const Text('CLOSE'),
          ),
          TextButton(
            onPressed: _loadStorageInfo,
            child: const Text('REFRESH'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final provider = Provider.of<LocationProvider>(context);
    final isTracking = provider.isTracking;
    final hasLocations = provider.locations.isNotEmpty;

    return Scaffold(
      appBar: AppBar(
        title: Row(
          children: [
            _isInSelectionMode
                ? Text('${_selectedLocationIds.length} selected')
                : const Text(
                    'Location Tracker',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
          ],
        ),
        actions: [
          // Show cancel button in selection mode
          if (_isInSelectionMode)
            IconButton(
              icon: const Icon(Icons.close),
              onPressed: _toggleSelectionMode,
              tooltip: 'Cancel selection',
            )
          // Show refresh indicator while refreshing
          else if (_isRefreshing)
            const Center(
              child: SizedBox(
                width: 20,
                height: 20,
                child: CircularProgressIndicator(
                  strokeWidth: 2,
                ),
              ),
            ),

          // Show delete button when items are selected
          if (_isInSelectionMode && _selectedLocationIds.isNotEmpty)
            IconButton(
              icon: const Icon(Icons.delete),
              onPressed: _deleteSelectedLocations,
              tooltip: 'Delete selected',
            )
          // Show sync and refresh buttons when not in selection mode
          else if (!_isInSelectionMode)
            Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                // Sync current location button
                Builder(
                  builder: (context) {
                    final provider = Provider.of<LocationProvider>(context);
                    return IconButton(
                      icon: provider.isSyncing
                          ? const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(
                                strokeWidth: 2,
                              ),
                            )
                          : const Icon(Icons.cloud_upload),
                      onPressed:
                          provider.isSyncing ? null : _syncCurrentLocation,
                      tooltip: 'Sync current location',
                    );
                  },
                ),
                // Refresh button
                IconButton(
                  icon: const Icon(Icons.refresh),
                  onPressed: _refreshData,
                  tooltip: 'Refresh data',
                ),
                IconButton(
                  icon: const Icon(Icons.tune),
                  onPressed: _showConfigDetailsDialog,
                  tooltip: 'Location config details',
                ),
              ],
            ),

          // Show menu button for additional options
          if (!_isInSelectionMode)
            PopupMenuButton<String>(
              onSelected: (value) {
                if (value == 'select') {
                  _toggleSelectionMode();
                } else if (value == 'delete_all') {
                  _deleteAllLocations();
                } else if (value == 'storage_info') {
                  _showStorageInfoDialog();
                } else if (value == 'config') {
                  _showConfigDetailsDialog();
                }
              },
              itemBuilder: (context) => [
                const PopupMenuItem(
                  value: 'config',
                  child: Row(
                    children: [
                      Icon(Icons.tune, size: 20),
                      SizedBox(width: 8),
                      Text('Config details'),
                    ],
                  ),
                ),
                if (hasLocations)
                  const PopupMenuItem(
                    value: 'select',
                    child: Row(
                      children: [
                        Icon(Icons.check_box_outlined, size: 20),
                        SizedBox(width: 8),
                        Text('Select multiple'),
                      ],
                    ),
                  ),
                const PopupMenuItem(
                  value: 'storage_info',
                  child: Row(
                    children: [
                      Icon(Icons.storage, size: 20),
                      SizedBox(width: 8),
                      Text('Storage info'),
                    ],
                  ),
                ),
                if (hasLocations)
                  const PopupMenuItem(
                    value: 'delete_all',
                    child: Row(
                      children: [
                        Icon(Icons.delete_sweep, size: 20, color: Colors.red),
                        SizedBox(width: 8),
                        Text('Delete all',
                            style: TextStyle(color: Colors.red)),
                      ],
                    ),
                  ),
              ],
            ),
        ],
      ),
      body: _buildBody(context, provider),
      floatingActionButton: _buildFloatingActionButton(context, provider),
    );
  }

  Widget _buildBody(BuildContext context, LocationProvider provider) {
    final isTracking = provider.isTracking;
    final locations = provider.locations;
    final totalLocations = provider.totalLocations;

    return Column(
      children: [
        // Status indicator
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
          child: Row(
            children: [
              Icon(
                isTracking ? Icons.gps_fixed : Icons.gps_off,
                color: isTracking ? Colors.green : Colors.grey,
                size: 20,
              ),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  isTracking ? 'Tracking active' : 'Tracking inactive',
                  style: TextStyle(
                    color: isTracking ? Colors.green : Colors.grey,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
              TextButton(
                onPressed: _showConfigDetailsDialog,
                child: const Text('Full config'),
              ),
            ],
          ),
        ),

        _buildConfigSummaryCard(context),

        // Stats row
        Padding(
          padding: const EdgeInsets.all(8.0),
          child: Card(
            elevation: 2,
            child: Padding(
              padding:
                  const EdgeInsets.symmetric(vertical: 8.0, horizontal: 16.0),
              child: Row(
                children: [
                  Expanded(
                    child: _buildStatItem(
                      context,
                      'Total',
                      totalLocations.toString(),
                      Icons.location_on,
                      Colors.blue,
                    ),
                  ),
                  Expanded(
                    child: _buildStatItem(
                      context,
                      'Synced',
                      provider.syncedCount.toString(),
                      Icons.cloud_done,
                      Colors.green,
                    ),
                  ),
                  Expanded(
                    child: _buildStatItem(
                      context,
                      'Pending',
                      provider.pendingCount.toString(),
                      Icons.cloud_upload,
                      Colors.orange,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),

        // Pagination info - shows when we have locations and pagination is happening
        if (locations.isNotEmpty && totalLocations > provider.pageSize)
          Padding(
            padding:
                const EdgeInsets.symmetric(horizontal: 16.0, vertical: 4.0),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Showing ${locations.length} of $totalLocations locations',
                  style: TextStyle(
                    fontSize: 12,
                    color: Colors.grey[600],
                  ),
                ),
                if (provider.isLoadingMore)
                  const SizedBox(
                    width: 16,
                    height: 16,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                    ),
                  ),
              ],
            ),
          ),

        // Location list
        Expanded(
          child: locations.isEmpty
              ? _buildEmptyView(context)
              : _buildLocationsList(context, provider),
        ),
      ],
    );
  }

  Widget _buildConfigSummaryCard(BuildContext context) {
    final pollMin = LocationConfigService.getInt('gpsPollingInterval', 20);
    final dupRadius = LocationConfig.DUPLICATE_LOCATION_THRESHOLD_METERS;
    final dupSession = LocationConfig.DUPLICATE_SESSION_CHECK;
    final trackingOn = LocationConfig.IS_LOCATION_TRACKING_ENABLED;

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 8.0),
      child: Card(
        elevation: 1,
        color: Colors.blueGrey.shade50,
        child: InkWell(
          onTap: _showConfigDetailsDialog,
          child: Padding(
            padding: const EdgeInsets.all(12.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    const Icon(Icons.settings_input_antenna, size: 18),
                    const SizedBox(width: 8),
                    const Expanded(
                      child: Text(
                        'Live tracking config',
                        style: TextStyle(fontWeight: FontWeight.bold),
                      ),
                    ),
                    Text(
                      trackingOn ? 'ON' : 'OFF',
                      style: TextStyle(
                        color: trackingOn ? Colors.green : Colors.red,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 8),
                Text(
                  'Send / poll: every $pollMin min  •  '
                  'Move filter: ${LocationConfig.DISTANCE_FILTER_METERS}m\n'
                  'Duplicate location radius: ${dupRadius}m (near-same GPS skipped)\n'
                  'Duplicate session check: ${dupSession ? "Yes (punch rules)" : "No"}',
                  style: TextStyle(fontSize: 12, color: Colors.grey[800]),
                ),
                const SizedBox(height: 4),
                Text(
                  'List marks DUPLICATE when within ${dupRadius}m of previous point. Tap for full config.',
                  style: TextStyle(fontSize: 11, color: Colors.blueGrey[600]),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _showConfigDetailsDialog() async {
    var refreshing = false;

    await showDialog<void>(
      context: context,
      builder: (ctx) {
        return StatefulBuilder(
          builder: (ctx, setDialogState) {
            final raw = LocationConfigService.cachedConfigSnapshot;
            final sortedKeys = raw.keys.toList()..sort();

            return AlertDialog(
              title: const Text('Location tracking config'),
              content: SizedBox(
                width: double.maxFinite,
                child: SingleChildScrollView(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'API: ${BaseUrls.locationTrackingConfig}',
                        style: const TextStyle(fontSize: 11, color: Colors.grey),
                      ),
                      const SizedBox(height: 8),
                      if (!LocationConfigService.hasCachedConfig)
                        const Text(
                          'No cached config yet. Tap Refresh config.',
                          style: TextStyle(color: Colors.orange),
                        ),
                      _configSection('Tracking switches', [
                        _kv('attendanceEnabled',
                            LocationConfigService.attendanceEnabled),
                        _kv('enableLocationTracking',
                            LocationConfigService.enableLocationTracking),
                        _kv(
                            'enableEmployeeLevelLocationTracking',
                            LocationConfigService
                                .enableEmployeeLevelLocationTracking),
                        _kv(
                            'employeeLocationTrackingEnabled',
                            LocationConfigService
                                .employeeLocationTrackingEnabled),
                        _kv('IS_LOCATION_TRACKING_ENABLED (app)',
                            LocationConfig.IS_LOCATION_TRACKING_ENABLED),
                      ]),
                      _configSection('Intervals & distance', [
                        _kv(
                            'gpsPollingInterval (API minutes)',
                            LocationConfigService.getInt(
                                'gpsPollingInterval', 20)),
                        _kv(
                            'gpsPollingInterval → app seconds',
                            LocationConfig
                                .LOCATION_UPDATE_INTERVAL_SECONDS),
                        _kv(
                            'API send interval seconds',
                            LocationConfig.API_CALL_INTERVAL_SECONDS),
                        _kv('minimumDisplacement (m)',
                            LocationConfig.DISTANCE_FILTER_METERS),
                        _kv('gpsAccuracyThreshold (m)',
                            LocationConfigService.gpsAccuracyThresholdMeters),
                        _kv(
                            'duplicateLocationRadius (m) — skip near-same points',
                            LocationConfig.DUPLICATE_LOCATION_THRESHOLD_METERS),
                        _kv(
                            'MAX_CONSECUTIVE_DUPLICATES (app)',
                            LocationConfig.MAX_CONSECUTIVE_DUPLICATES),
                        _kv('retryInterval (API minutes)',
                            LocationConfigService.getInt('retryInterval', 5)),
                        _kv('retryInterval → app seconds',
                            LocationConfigService.retryIntervalSeconds),
                        _kv('locationTimeoutDuration (s)',
                            LocationConfigService
                                .locationTimeoutDurationSeconds),
                        _kv('serverSyncBatchSize',
                            LocationConfigService.serverSyncBatchSize),
                        _kv('offlineStorageLimit',
                            LocationConfigService.offlineStorageLimit),
                        _kv('autoDataCleanupDays',
                            LocationConfigService.autoDataCleanupDays),
                      ]),
                      _configSection('Duplicate & session policy', [
                        _kv(
                            'duplicateLocationRadius (raw API)',
                            LocationConfigService.getDouble(
                                'duplicateLocationRadius', 5.0)),
                        _kv('duplicateSessionCheck (block re-punch)',
                            LocationConfig.DUPLICATE_SESSION_CHECK),
                        _kv(
                            'duplicateSessionCheck (raw)',
                            LocationConfigService.duplicateSessionCheck),
                      ]),
                      _configSection('Geofence & punch policy', [
                        _kv('geofenceRadius (m)',
                            LocationConfig.GEOFENCE_RADIUS_METERS),
                        _kv('enableFromAnywhere',
                            LocationConfig.ENABLE_FROM_ANYWHERE),
                        _kv('blockPunchOnHoliday',
                            LocationConfig.BLOCK_PUNCH_ON_HOLIDAY),
                        _kv('enableLocationGapValidation',
                            LocationConfig.ENABLE_LOCATION_GAP_VALIDATION),
                        _kv(
                            'autoPunchOutTimeout / gap minutes',
                            LocationConfig.MAX_LOCATION_GAP_MINUTES),
                        _kv('enableBatteryOptimizationCheck',
                            LocationConfig.ENABLE_BATTERY_OPTIMIZATION_CHECK),
                        _kv(
                            'batteryOptimizationMode (0=warn,1=strict,2=lenient)',
                            LocationConfig.BATTERY_OPTIMIZATION_MODE),
                        _kv('alwaysAllowPermissionCheck',
                            LocationConfigService.alwaysAllowPermissionCheck),
                      ]),
                      _configSection('Auto punch-out flags', [
                        _kv('autoPunchOutOnGPSTurnOff',
                            LocationConfig.AUTO_PUNCH_OUT_ON_GPS_OFF),
                        _kv('autoPunchOutOnLocationServicesOff',
                            LocationConfig.AUTO_PUNCH_OUT_ON_LOCATION_OFF),
                        _kv('autoPunchOutOnAppKilled',
                            LocationConfig.AUTO_PUNCH_OUT_ON_APP_KILLED),
                        _kv('autoPunchOutOnPowerSavingMode',
                            LocationConfig.AUTO_PUNCH_OUT_ON_POWER_SAVING),
                        _kv('autoPunchOutOnAirplaneMode',
                            LocationConfig.AUTO_PUNCH_OUT_ON_AIRPLANE_MODE),
                        _kv(
                            'autoPunchOutOnAppKilled / phone power-off',
                            LocationConfig.AUTO_PUNCH_OUT_ON_APP_KILLED),
                        _kv('permissionRevokedAutoPunchOut',
                            LocationConfig.PERMISSION_REVOKED_AUTO_PUNCH_OUT),
                      ]),
                      const SizedBox(height: 12),
                      const Text(
                        'Raw API cache (all keys)',
                        style: TextStyle(fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 6),
                      if (sortedKeys.isEmpty)
                        const Text('— empty —')
                      else
                        ...sortedKeys.map((k) => _kv(k, raw[k])),
                      const SizedBox(height: 8),
                      SelectableText(
                        const JsonEncoder.withIndent('  ').convert(
                          raw.isEmpty ? {'note': 'no cache'} : raw,
                        ),
                        style: const TextStyle(
                          fontSize: 11,
                          fontFamily: 'monospace',
                        ),
                      ),
                    ],
                  ),
                ),
              ),
              actions: [
                TextButton(
                  onPressed: refreshing
                      ? null
                      : () async {
                          setDialogState(() => refreshing = true);
                          final ok =
                              await LocationConfigService.fetchConfig();
                          if (mounted) setState(() {});
                          setDialogState(() => refreshing = false);
                          if (ctx.mounted) {
                            ScaffoldMessenger.of(context).showSnackBar(
                              SnackBar(
                                content: Text(ok
                                    ? 'Config refreshed from server'
                                    : 'Config refresh failed'),
                              ),
                            );
                          }
                        },
                  child: refreshing
                      ? const SizedBox(
                          width: 18,
                          height: 18,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Text('Refresh config'),
                ),
                TextButton(
                  onPressed: () => Navigator.pop(ctx),
                  child: const Text('Close'),
                ),
              ],
            );
          },
        );
      },
    );
  }

  Widget _configSection(String title, List<Widget> rows) {
    return Padding(
      padding: const EdgeInsets.only(top: 12.0, bottom: 4.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            title,
            style: const TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 14,
            ),
          ),
          const Divider(height: 8),
          ...rows,
        ],
      ),
    );
  }

  Widget _kv(String key, Object? value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 3.0),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            flex: 3,
            child: Text(
              key,
              style: TextStyle(fontSize: 12, color: Colors.grey[700]),
            ),
          ),
          Expanded(
            flex: 2,
            child: Text(
              '$value',
              style: const TextStyle(
                fontSize: 12,
                fontWeight: FontWeight.w600,
              ),
              textAlign: TextAlign.end,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatItem(BuildContext context, String label, String value,
      IconData icon, Color color) {
    return Column(
      children: [
        Icon(icon, color: color, size: 20),
        const SizedBox(height: 4),
        Text(
          label,
          style: TextStyle(
            fontSize: 12,
            color: Colors.grey[600],
          ),
        ),
        Text(
          value,
          style: TextStyle(
            fontSize: 18,
            fontWeight: FontWeight.bold,
            color: color,
          ),
        ),
      ],
    );
  }

  Widget _buildEmptyView(BuildContext context) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.location_off, size: 64, color: Colors.grey),
          const SizedBox(height: 16),
          Text(
            'No locations tracked yet',
            style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  color: Colors.grey,
                ),
          ),
          const SizedBox(height: 8),
          ElevatedButton.icon(
            onPressed: _refreshData,
            icon: const Icon(Icons.refresh),
            label: const Text('Refresh'),
          ),
          const SizedBox(height: 16),
          // Show storage info button
          TextButton.icon(
            onPressed: _showStorageInfoDialog,
            icon: const Icon(Icons.storage),
            label: const Text('View Storage Info'),
          ),
          TextButton.icon(
            onPressed: _showConfigDetailsDialog,
            icon: const Icon(Icons.tune),
            label: const Text('View Full Config'),
          ),
        ],
      ),
    );
  }

  Widget _buildLocationsList(BuildContext context, LocationProvider provider) {
    final locations = provider.locations;
    final hasMoreData = provider.hasMoreData;
    final isLoadingMore = provider.isLoadingMore;
    final dupRadius = LocationConfig.DUPLICATE_LOCATION_THRESHOLD_METERS;

    // Chronological order (oldest → newest) to compare each point with previous.
    final chronological = List<LocationData>.from(locations)
      ..sort((a, b) => a.timestamp.compareTo(b.timestamp));
    final Map<int, double?> distanceById = {};
    final Map<int, bool> duplicateById = {};
    for (var i = 0; i < chronological.length; i++) {
      final loc = chronological[i];
      if (i == 0) {
        distanceById[loc.id] = null;
        duplicateById[loc.id] = false;
        continue;
      }
      final prev = chronological[i - 1];
      final meters = _distanceMeters(
        prev.latitude,
        prev.longitude,
        loc.latitude,
        loc.longitude,
      );
      distanceById[loc.id] = meters;
      duplicateById[loc.id] = meters <= dupRadius;
    }

    final duplicateCount =
        duplicateById.values.where((d) => d).length;

    return Column(
      children: [
        if (locations.isNotEmpty)
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
            child: Row(
              children: [
                const Icon(Icons.copy_all, size: 16, color: Colors.deepOrange),
                const SizedBox(width: 6),
                Expanded(
                  child: Text(
                    'Duplicates in list: $duplicateCount / ${locations.length} '
                    '(radius ${dupRadius}m)',
                    style: TextStyle(fontSize: 12, color: Colors.grey[800]),
                  ),
                ),
              ],
            ),
          ),
        Expanded(
          child: RefreshIndicator(
            onRefresh: _refreshData,
            child: ListView.builder(
              controller: _scrollController,
              physics: const AlwaysScrollableScrollPhysics(),
              padding: const EdgeInsets.only(bottom: 100),
              itemCount: locations.length + (hasMoreData ? 1 : 0),
              itemBuilder: (context, index) {
                if (index == locations.length) {
                  return Center(
                    child: Padding(
                      padding: const EdgeInsets.symmetric(vertical: 16.0),
                      child: isLoadingMore
                          ? const CircularProgressIndicator()
                          : TextButton(
                              onPressed: provider.loadMore,
                              child: const Text('Load more'),
                            ),
                    ),
                  );
                }

                final location = locations[index];
                final isSelected = _selectedLocationIds.contains(location.id);
                final isDup = duplicateById[location.id] ?? false;
                final dist = distanceById[location.id];

                return LocationCard(
                  location: location,
                  index: index,
                  total: provider.totalLocations,
                  onRefresh: _refreshData,
                  isSelected: isSelected,
                  isDuplicate: isDup,
                  distanceFromPreviousMeters: dist,
                  onSelectionChanged: _isInSelectionMode
                      ? () => _toggleLocationSelection(location.id)
                      : _isInSelectionMode
                          ? null
                          : () => _startSelectionWithLocation(location.id),
                );
              },
            ),
          ),
        ),
      ],
    );
  }

  /// Haversine distance in meters between two lat/lng points.
  double _distanceMeters(
      double lat1, double lon1, double lat2, double lon2) {
    const earthRadius = 6371000.0;
    final dLat = _degToRad(lat2 - lat1);
    final dLon = _degToRad(lon2 - lon1);
    final a = math.sin(dLat / 2) * math.sin(dLat / 2) +
        math.cos(_degToRad(lat1)) *
            math.cos(_degToRad(lat2)) *
            math.sin(dLon / 2) *
            math.sin(dLon / 2);
    final c = 2 * math.atan2(math.sqrt(a), math.sqrt(1 - a));
    return earthRadius * c;
  }

  double _degToRad(double deg) => deg * math.pi / 180;

  void _startSelectionWithLocation(int locationId) {
    setState(() {
      _isInSelectionMode = true;
      _selectedLocationIds.add(locationId);
    });
  }

  Widget _buildFloatingActionButton(
      BuildContext context, LocationProvider provider) {
    final isTracking = provider.isTracking;

    if (_isInSelectionMode) {
      return FloatingActionButton(
        onPressed: _deleteSelectedLocations,
        backgroundColor: Colors.red,
        child: const Icon(Icons.delete),
      );
    } else {
      return FloatingActionButton.extended(
        onPressed: () {
          if (isTracking) {
            _confirmStopTracking(context);
          } else {
            provider.startTracking();
          }
        },
        backgroundColor: isTracking ? Colors.red : Colors.green,
        icon: Icon(isTracking ? Icons.stop : Icons.play_arrow),
        label: Text(isTracking ? 'Stop Tracking' : 'Start Tracking'),
      );
    }
  }

  Future<void> _confirmStopTracking(BuildContext context) async {
    final shouldStop = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Stop Tracking?'),
        content:
            const Text('Are you sure you want to stop tracking your location?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: const Text('CANCEL'),
          ),
          TextButton(
            onPressed: () => Navigator.of(context).pop(true),
            child: const Text('STOP'),
          ),
        ],
      ),
    );

    if (shouldStop == true) {
      final provider = Provider.of<LocationProvider>(context, listen: false);
      provider.stopTracking();
    }
  }

  void _showTrackingInfo(
      BuildContext context, bool isTracking, int locationCount) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('Background Tracking Info'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Current Status: ${isTracking ? "ACTIVE" : "INACTIVE"}',
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  color: isTracking ? Colors.green : Colors.red,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                'Tracked Locations: $locationCount',
                style: const TextStyle(fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 16),
              const Text(
                'This app tracks your location every 10 seconds and sends updates to the server. '
                'Tracking continues even when the app is closed or in the background.',
              ),
              const SizedBox(height: 16),
              if (isTracking)
                const Text(
                  'Note: You can also stop tracking from the notification.',
                  style: TextStyle(fontStyle: FontStyle.italic),
                ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () {
                Navigator.of(context).pop(); // Close the dialog
              },
              child: const Text('OK'),
            ),
          ],
        );
      },
    );
  }

  void _showLegendInfo(BuildContext context) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('Status Legend'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'Each location card shows the status of the data:',
                style: TextStyle(fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 16),
              Row(
                children: [
                  const Icon(Icons.cloud_done, color: Colors.green, size: 20),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: const [
                        Text('Synced',
                            style: TextStyle(fontWeight: FontWeight.bold)),
                        Text(
                          'Location has been successfully sent to the server',
                          style: TextStyle(fontSize: 12),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              Row(
                children: [
                  const Icon(Icons.cloud_off, color: Colors.red, size: 20),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: const [
                        Text('Pending',
                            style: TextStyle(fontWeight: FontWeight.bold)),
                        Text(
                          'Location is saved locally but not yet sent to the server',
                          style: TextStyle(fontSize: 12),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              const Text(
                'Note: Pending locations will be automatically sent every 10 seconds or when connectivity is restored.',
                style: TextStyle(fontStyle: FontStyle.italic, fontSize: 12),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () {
                Navigator.of(context).pop(); // Close the dialog
              },
              child: const Text('Got it'),
            ),
          ],
        );
      },
    );
  }
}
