/**
 * Battery Optimization Service
 *
 * Android: ignore-battery-optimizations exemption.
 * iOS: Low Power Mode check (Apple has no per-app battery exemption API).
 *
 * Behaviour is driven by dashboard config:
 *   enableBatteryOptimizationCheck
 *   batteryOptimizationMode: 0=Warning Only, 1=Strict, 2=Lenient
 */

import 'dart:io';
import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../Theme/app_colors.dart';
import '../../constants/location_config.dart';

class BatteryOptimizationService {
  static const MethodChannel _channel = MethodChannel('battery_optimization');

  /// Android: true when app is exempt from battery optimization.
  /// iOS: true when Low Power Mode is OFF.
  static Future<bool> isBatteryOptimizationDisabled() async {
    try {
      if (Platform.isAndroid) {
        final bool isDisabled =
            await _channel.invokeMethod('isBatteryOptimizationDisabled');
        return isDisabled;
      }
      if (Platform.isIOS) {
        final bool lowPower =
            await _channel.invokeMethod('isLowPowerModeEnabled') ?? false;
        return !lowPower;
      }
      return true;
    } on PlatformException catch (e) {
      print('Error checking battery optimization: ${e.message}');
      return false;
    }
  }

  static Future<void> requestDisableBatteryOptimization() async {
    try {
      if (Platform.isAndroid) {
        await _channel.invokeMethod('requestDisableBatteryOptimization');
      } else if (Platform.isIOS) {
        await _channel.invokeMethod('openBatterySettings');
      }
    } on PlatformException catch (e) {
      print('Error requesting battery optimization: ${e.message}');
    }
  }

  /// Dashboard-driven battery check (Android + iOS) after login / before punch.
  static Future<bool> showMandatoryBatteryOptimizationDialog(
      BuildContext context) async {
    if (!Platform.isAndroid && !Platform.isIOS) return true;

    if (!LocationConfig.ENABLE_BATTERY_OPTIMIZATION_CHECK) {
      return true;
    }

    final mode = LocationConfig.BATTERY_OPTIMIZATION_MODE;
    // Lenient: do not interrupt flow.
    if (mode == 2) return true;

    final bool isOk = await isBatteryOptimizationDisabled();
    if (isOk) return true;

    // Warning Only (0): non-blocking reminder.
    if (mode == 0) {
      await showBatteryOptimizationDialog(context);
      return true;
    }

    // Strict (1): must resolve settings before continuing.
    if (Platform.isIOS) {
      return _showIosStrictDialog(context);
    }
    return _showAndroidStrictDialog(context);
  }

  static Future<bool> _showAndroidStrictDialog(BuildContext context) async {
    final bool? completed = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (BuildContext context) {
        return PopScope(
          canPop: false,
          child: AlertDialog(
            icon: const Icon(
              Icons.battery_alert,
              color: Colors.orange,
              size: 40,
            ),
            title: const Text(
              'Battery Settings Required',
              style: TextStyle(
                fontWeight: FontWeight.bold,
                fontSize: 18,
                color: Colors.black87,
              ),
              textAlign: TextAlign.center,
            ),
            content: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Text(
                  'Please disable battery optimization for this app to ensure reliable punch in/out functionality.',
                  style: TextStyle(
                    fontWeight: FontWeight.w500,
                    fontSize: 16,
                    color: Colors.black87,
                  ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 16),
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: Colors.blue.shade50,
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: Colors.blue.shade200),
                  ),
                  child: Row(
                    children: [
                      const Icon(Icons.info_outline, color: Colors.blue, size: 18),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          'This setting is required for attendance system.',
                          style: TextStyle(
                            fontSize: 13,
                            fontWeight: FontWeight.w500,
                            color: Colors.blue.shade800,
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            actionsPadding: const EdgeInsets.all(16),
            actions: [
              Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  ElevatedButton.icon(
                    onPressed: () async {
                      await requestDisableBatteryOptimization();
                    },
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.primaryBlue,
                      foregroundColor: Colors.white,
                      padding: const EdgeInsets.symmetric(vertical: 12),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(8),
                      ),
                    ),
                    icon: const Icon(Icons.settings, size: 18),
                    label: const Text(
                      'Open Settings',
                      style: TextStyle(
                        fontWeight: FontWeight.bold,
                        fontSize: 16,
                      ),
                    ),
                  ),
                  const SizedBox(height: 10),
                  OutlinedButton.icon(
                    onPressed: () async {
                      final bool isNowOk =
                          await isBatteryOptimizationDisabled();
                      if (isNowOk) {
                        Navigator.of(context).pop(true);
                      } else {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(
                            content: Text(
                                'Please disable battery optimization to continue.'),
                            backgroundColor: Colors.orange,
                            duration: Duration(seconds: 2),
                          ),
                        );
                      }
                    },
                    style: OutlinedButton.styleFrom(
                      foregroundColor: AppColors.primaryBlue,
                      side: const BorderSide(color: AppColors.primaryBlue),
                      padding: const EdgeInsets.symmetric(vertical: 12),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(8),
                      ),
                    ),
                    icon: const Icon(Icons.check, size: 18),
                    label: const Text(
                      'Done',
                      style: TextStyle(
                        fontWeight: FontWeight.bold,
                        fontSize: 16,
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ),
        );
      },
    );

    return completed ?? false;
  }

  static Future<bool> _showIosStrictDialog(BuildContext context) async {
    final bool? completed = await showCupertinoDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (dialogContext) {
        return CupertinoAlertDialog(
          title: const Text('Battery Settings Required'),
          content: const Text(
            'Low Power Mode can pause background location tracking.\n\n'
            'Please turn off Low Power Mode:\n'
            'Settings → Battery → Low Power Mode',
          ),
          actions: [
            CupertinoDialogAction(
              onPressed: () async {
                await requestDisableBatteryOptimization();
              },
              child: const Text('Open Settings'),
            ),
            CupertinoDialogAction(
              isDefaultAction: true,
              onPressed: () async {
                final bool isNowOk = await isBatteryOptimizationDisabled();
                if (!dialogContext.mounted) return;
                if (isNowOk) {
                  Navigator.of(dialogContext).pop(true);
                } else {
                  // Keep dialog open; brief snack via root messenger if available.
                  ScaffoldMessenger.maybeOf(context)?.showSnackBar(
                    const SnackBar(
                      content: Text(
                          'Please turn off Low Power Mode to continue.'),
                      backgroundColor: Colors.orange,
                      duration: Duration(seconds: 2),
                    ),
                  );
                }
              },
              child: const Text('Done'),
            ),
          ],
        );
      },
    );

    return completed ?? false;
  }

  static Future<void> showBatteryOptimizationDialog(
      BuildContext context) async {
    if (!Platform.isAndroid && !Platform.isIOS) return;
    if (!LocationConfig.ENABLE_BATTERY_OPTIMIZATION_CHECK) return;

    final bool isOk = await isBatteryOptimizationDisabled();
    if (isOk) return;

    if (Platform.isIOS) {
      await showCupertinoDialog<void>(
        context: context,
        builder: (dialogContext) {
          return CupertinoAlertDialog(
            title: const Text('Optimize Attendance'),
            content: const Text(
              'Low Power Mode can interrupt attendance tracking. '
              'Turn it off under Settings → Battery for better reliability.',
            ),
            actions: [
              CupertinoDialogAction(
                onPressed: () => Navigator.of(dialogContext).pop(),
                child: const Text('Later'),
              ),
              CupertinoDialogAction(
                isDefaultAction: true,
                onPressed: () async {
                  await requestDisableBatteryOptimization();
                  if (dialogContext.mounted) {
                    Navigator.of(dialogContext).pop();
                  }
                },
                child: const Text('Open Settings'),
              ),
            ],
          );
        },
      );
      return;
    }

    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          icon: const Icon(
            Icons.battery_alert,
            color: Colors.orange,
            size: 40,
          ),
          title: const Text(
            'Optimize Attendance',
            style: TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 18,
            ),
          ),
          content: const Text(
            'Disable battery optimization for better attendance tracking reliability.',
            style: TextStyle(
              fontWeight: FontWeight.w500,
              fontSize: 16,
            ),
            textAlign: TextAlign.center,
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text(
                'LATER',
                style: TextStyle(
                  color: Colors.grey,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ),
            ElevatedButton(
              onPressed: () async {
                await requestDisableBatteryOptimization();
                Navigator.of(context).pop();
              },
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.primaryBlue,
                foregroundColor: Colors.white,
              ),
              child: const Text(
                'OPTIMIZE',
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ],
        );
      },
    );
  }
}
