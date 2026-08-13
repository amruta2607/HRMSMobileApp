import 'package:flutter/material.dart';
import 'package:geolocator/geolocator.dart';
import 'package:geocoding/geocoding.dart';

import '../../../Theme/app_colors.dart';
import '../../../constants/location_config.dart';
import '../../../constants/log_levels.dart';

class LocationService {
  static String? _cachedLocation;
  static Position? _cachedPosition;

  /// When dashboard [alwaysAllowPermissionCheck] is true, require Always /
  /// while-in-use is not enough for background tracking punch-in.
  static bool _needsAlwaysPermission() =>
      LocationConfig.ALWAYS_ALLOW_PERMISSION_CHECK;

  static bool _isPermissionSufficient(LocationPermission permission) {
    if (permission == LocationPermission.denied ||
        permission == LocationPermission.deniedForever) {
      return false;
    }
    if (_needsAlwaysPermission()) {
      return permission == LocationPermission.always;
    }
    return permission == LocationPermission.whileInUse ||
        permission == LocationPermission.always;
  }

  static Future<bool> ensurePermissionGranted(
      {bool requestIfDenied = true}) async {
    LocationPermission permission = await Geolocator.checkPermission();

    if (!_isPermissionSufficient(permission) && requestIfDenied) {
      permission = await Geolocator.requestPermission();
    }

    if (_needsAlwaysPermission() &&
        permission == LocationPermission.whileInUse &&
        requestIfDenied) {
      LogConfig.logWarning(
          'Always-allow required by dashboard — requesting upgrade from whileInUse');
      permission = await Geolocator.requestPermission();
    }

    if (!_isPermissionSufficient(permission)) {
      return false;
    }

    return true;
  }

  /// True when permission was granted previously but is now denied/revoked.
  static Future<bool> isPermissionRevoked() async {
    final permission = await Geolocator.checkPermission();
    return permission == LocationPermission.denied ||
        permission == LocationPermission.deniedForever;
  }

  /// Ensures GPS + location permission. Shows a dialog with Open Settings when off.
  static Future<bool> ensurePermissionWithDialog(BuildContext context) async {
    if (!await isLocationServiceOn()) {
      if (!context.mounted) return false;
      final opened = await _showEnableDialog(
        context,
        title: 'Location is off',
        message:
            'Please turn on GPS / Location services to continue attendance tracking.',
        onOpenSettings: Geolocator.openLocationSettings,
      );
      if (!opened || !context.mounted) return false;
      if (!await isLocationServiceOn()) return false;
    }

    var granted = await ensurePermissionGranted(requestIfDenied: true);
    if (granted) return true;

    if (!context.mounted) return false;

    final needsAlways = _needsAlwaysPermission();
    final opened = await _showEnableDialog(
      context,
      title: 'Location permission required',
      message: needsAlways
          ? 'Please allow “Allow all the time” / Always location permission for background tracking.'
          : 'Location permission is required for attendance tracking. Please turn it on.',
      onOpenSettings: Geolocator.openAppSettings,
    );
    if (!opened || !context.mounted) return false;

    return ensurePermissionGranted(requestIfDenied: false);
  }

  static Future<bool> _showEnableDialog(
    BuildContext context, {
    required String title,
    required String message,
    required Future<bool> Function() onOpenSettings,
  }) async {
    final result = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (dialogContext) {
        return AlertDialog(
          icon: const Icon(
            Icons.location_off,
            color: Colors.orange,
            size: 40,
          ),
          title: Text(
            title,
            style: const TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 18,
            ),
            textAlign: TextAlign.center,
          ),
          content: Text(
            message,
            style: const TextStyle(fontSize: 15, fontWeight: FontWeight.w500),
            textAlign: TextAlign.center,
          ),
          actionsPadding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
          actions: [
            Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                ElevatedButton.icon(
                  onPressed: () async {
                    await onOpenSettings();
                    if (dialogContext.mounted) {
                      Navigator.of(dialogContext).pop(true);
                    }
                  },
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppColors.primaryBlue,
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 12),
                  ),
                  icon: const Icon(Icons.settings, size: 18),
                  label: const Text(
                    'Open Settings',
                    style: TextStyle(fontWeight: FontWeight.bold),
                  ),
                ),
                TextButton(
                  onPressed: () => Navigator.of(dialogContext).pop(false),
                  child: const Text('Cancel'),
                ),
              ],
            ),
          ],
        );
      },
    );
    return result ?? false;
  }

  static Future<bool> isLocationServiceOn() async {
    return Geolocator.isLocationServiceEnabled();
  }

  static Future<bool> ensureLocationReadyOnAppStart() async {
    final permissionGranted = await ensurePermissionGranted();
    if (!permissionGranted) return false;

    final serviceOn = await isLocationServiceOn();
    if (!serviceOn) {
      await Geolocator.openLocationSettings();
      return false;
    }

    return true;
  }

  static Future<String> getLocation({
    bool forceRefresh = false,
    bool requestPermissionIfDenied = true,
  }) async {
    try {
      if (_cachedLocation != null && !forceRefresh) {
        return _cachedLocation!;
      }

      final permissionGranted = await ensurePermissionGranted(
        requestIfDenied: requestPermissionIfDenied,
      );
      if (!permissionGranted) {
        return 'Permission denied';
      }

      final serviceOn = await isLocationServiceOn();
      if (!serviceOn) {
        return 'Location is turned off';
      }

      final position = await _getPosition(forceRefresh: forceRefresh);

      final placemarks = await placemarkFromCoordinates(
        position.latitude,
        position.longitude,
      ).timeout(const Duration(seconds: 5));

      if (placemarks.isEmpty) {
        return 'Location unavailable';
      }

      final place = placemarks.first;
      final area = place.subLocality ?? place.thoroughfare ?? '';
      final city = place.locality ?? place.subAdministrativeArea ?? '';

      final location = area.isNotEmpty && city.isNotEmpty
          ? '$area, $city'
          : city.isNotEmpty
              ? city
              : 'Location found';

      _cachedLocation = location;
      return location;
    } catch (e) {
      print('Location error in getLocation: $e');
      if (_cachedLocation != null) return _cachedLocation!;
      return 'Location unavailable';
    }
  }

  static Future<Position> getLatLng({
    bool forceRefresh = false,
    bool requestPermissionIfDenied = true,
  }) async {
    final permissionGranted = await ensurePermissionGranted(
      requestIfDenied: requestPermissionIfDenied,
    );
    if (!permissionGranted) {
      throw Exception(
        _needsAlwaysPermission()
            ? 'Always location permission required'
            : 'Permission denied',
      );
    }

    final serviceOn = await isLocationServiceOn();
    if (!serviceOn) {
      throw Exception('Location service off');
    }

    return _getPosition(forceRefresh: forceRefresh);
  }

  static Future<Position> _getPosition({bool forceRefresh = false}) async {
    if (_cachedPosition != null && !forceRefresh) {
      return _cachedPosition!;
    }

    try {
      final lastKnown = await Geolocator.getLastKnownPosition();
      if (lastKnown != null && !forceRefresh) {
        _cachedPosition = lastKnown;
        return lastKnown;
      }

      final position = await Geolocator.getCurrentPosition(
        desiredAccuracy: LocationAccuracy.high,
        timeLimit: const Duration(seconds: 5),
      );

      _cachedPosition = position;
      return position;
    } catch (e) {
      print('Location error in _getPosition: $e');
      final lastKnown = await Geolocator.getLastKnownPosition();
      if (lastKnown != null) {
        return lastKnown;
      }
      rethrow;
    }
  }

  static void clearCache() {
    _cachedLocation = null;
    _cachedPosition = null;
  }
}
