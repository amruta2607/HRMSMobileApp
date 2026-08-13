import 'dart:io';

import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:geolocator/geolocator.dart';
import 'package:permission_handler/permission_handler.dart';

import '../../Background_location _tracking/services/battery_optimization_service.dart';
import '../../Theme/app_colors.dart';
import '../../constants/location_config.dart';
import 'Time_Location/location_service.dart';
import 'token_storage.dart';

/// Requests + validates attendance / location-tracking permissions on Android & iOS.
/// If anything required is still off, shows a popup with Open Settings.
class AppPermissionService {
  static bool _requestedThisSession = false;
  static bool _popupShowing = false;

  /// Call once when the logged-in home UI is first shown.
  static Future<bool> requestAllRequired(BuildContext context) async {
    if (!TokenStorage.isModuleEnabled('attendance')) return true;

    if (!_requestedThisSession) {
      _requestedThisSession = true;

      // System permission prompts first.
      await _requestLocationPermissions();

      final camera = await Permission.camera.status;
      if (!camera.isGranted && !camera.isPermanentlyDenied) {
        await Permission.camera.request();
      }

      final notification = await Permission.notification.status;
      if (!notification.isGranted && !notification.isPermanentlyDenied) {
        await Permission.notification.request();
      }

      if (!context.mounted) return false;

      await BatteryOptimizationService.showMandatoryBatteryOptimizationDialog(
        context,
      );
    }

    if (!context.mounted) return false;

    // After system prompts: if anything is still off, show popup (both platforms).
    return ensureTrackingPermissionsWithPopup(context);
  }

  /// Checks GPS + location (+ Always) + notification + battery.
  /// Shows a popup listing missing items when any are off.
  static Future<bool> ensureTrackingPermissionsWithPopup(
    BuildContext context, {
    bool requestSystemDialogs = false,
  }) async {
    if (!TokenStorage.isModuleEnabled('attendance')) return true;
    if (_popupShowing) return false;

    if (requestSystemDialogs) {
      await _requestLocationPermissions();
      final notification = await Permission.notification.status;
      if (!notification.isGranted && !notification.isPermanentlyDenied) {
        await Permission.notification.request();
      }
    }

    final missing = await _collectMissingTrackingPermissions();
    if (missing.isEmpty) return true;
    if (!context.mounted) return false;

    return _showMissingPermissionsPopup(context, missing);
  }

  static Future<List<String>> _collectMissingTrackingPermissions() async {
    final missing = <String>[];

    final serviceOn = await LocationService.isLocationServiceOn();
    if (!serviceOn) {
      missing.add(Platform.isIOS
          ? 'Location Services (Settings → Privacy → Location Services)'
          : 'GPS / Location services');
    }

    final permission = await Geolocator.checkPermission();
    final needsAlways = LocationConfig.ALWAYS_ALLOW_PERMISSION_CHECK;

    if (permission == LocationPermission.denied ||
        permission == LocationPermission.deniedForever) {
      missing.add('Location permission');
    } else if (needsAlways && permission != LocationPermission.always) {
      missing.add(Platform.isIOS
          ? 'Location set to “Always”'
          : 'Location “Allow all the time”');
    }

    final notification = await Permission.notification.status;
    if (!notification.isGranted) {
      missing.add('Notifications');
    }

    if (LocationConfig.ENABLE_BATTERY_OPTIMIZATION_CHECK &&
        LocationConfig.BATTERY_OPTIMIZATION_MODE != 2) {
      final batteryOk =
          await BatteryOptimizationService.isBatteryOptimizationDisabled();
      if (!batteryOk) {
        missing.add(Platform.isIOS
            ? 'Low Power Mode must be Off'
            : 'Battery optimization must be disabled');
      }
    }

    return missing;
  }

  static Future<bool> _showMissingPermissionsPopup(
    BuildContext context,
    List<String> missing,
  ) async {
    _popupShowing = true;
    try {
      final bulletList = missing.map((e) => '• $e').join('\n');

      if (Platform.isIOS) {
        final result = await showCupertinoDialog<bool>(
          context: context,
          barrierDismissible: false,
          builder: (dialogContext) {
            return CupertinoAlertDialog(
              title: const Text('Permissions Required'),
              content: Text(
                'Location tracking needs these settings turned on:\n\n'
                '$bulletList\n\n'
                'Please enable them to continue attendance tracking.',
              ),
              actions: [
                CupertinoDialogAction(
                  onPressed: () => Navigator.of(dialogContext).pop(false),
                  child: const Text('Cancel'),
                ),
                CupertinoDialogAction(
                  isDefaultAction: true,
                  onPressed: () async {
                    final needsLocationSettings = missing.any((m) =>
                        m.contains('Location Services') || m.contains('GPS'));
                    if (needsLocationSettings) {
                      await Geolocator.openLocationSettings();
                    } else {
                      await openAppSettings();
                    }
                    if (dialogContext.mounted) {
                      Navigator.of(dialogContext).pop(true);
                    }
                  },
                  child: const Text('Open Settings'),
                ),
              ],
            );
          },
        );
        return result ?? false;
      }

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
            title: const Text(
              'Permissions Required',
              style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18),
              textAlign: TextAlign.center,
            ),
            content: Text(
              'Location tracking needs these settings turned on:\n\n'
              '$bulletList\n\n'
              'Please enable them to continue attendance tracking.',
              style: const TextStyle(fontSize: 15, fontWeight: FontWeight.w500),
              textAlign: TextAlign.left,
            ),
            actionsPadding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
            actions: [
              Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  ElevatedButton.icon(
                    onPressed: () async {
                      final needsLocationSettings = missing.any((m) =>
                          m.contains('Location Services') ||
                          m.contains('GPS'));
                      if (needsLocationSettings) {
                        await Geolocator.openLocationSettings();
                      } else {
                        await openAppSettings();
                      }
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
    } finally {
      _popupShowing = false;
    }
  }

  /// iOS: When In Use first, then Always. Android: Geolocator upgrade path.
  static Future<void> _requestLocationPermissions() async {
    if (Platform.isIOS) {
      var whenInUse = await Permission.locationWhenInUse.status;
      if (!whenInUse.isGranted && !whenInUse.isPermanentlyDenied) {
        whenInUse = await Permission.locationWhenInUse.request();
      }

      if (LocationConfig.ALWAYS_ALLOW_PERMISSION_CHECK &&
          whenInUse.isGranted) {
        final always = await Permission.locationAlways.status;
        if (!always.isGranted && !always.isPermanentlyDenied) {
          await Permission.locationAlways.request();
        }
      }

      await LocationService.ensurePermissionGranted(requestIfDenied: false);
      return;
    }

    await LocationService.ensurePermissionGranted(requestIfDenied: true);
  }

  /// Allow a fresh request after logout → login in the same process.
  static void resetSession() {
    _requestedThisSession = false;
  }
}
