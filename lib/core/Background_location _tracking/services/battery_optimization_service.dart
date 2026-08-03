/**
 * Battery Optimization Service
 *
 * Handles Android battery optimization settings to ensure reliable
 * punch in/out functionality and attendance system operation.
 *
 * Behaviour is driven by dashboard config:
 *   enableBatteryOptimizationCheck
 *   batteryOptimizationMode: 0=Warning Only, 1=Strict, 2=Lenient
 */

import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../Theme/app_colors.dart';
import '../../constants/location_config.dart';

class BatteryOptimizationService {
  static const MethodChannel _channel = MethodChannel('battery_optimization');

  static Future<bool> isBatteryOptimizationDisabled() async {
    if (!Platform.isAndroid) return true;
    try {
      final bool isDisabled =
          await _channel.invokeMethod('isBatteryOptimizationDisabled');
      return isDisabled;
    } on PlatformException catch (e) {
      print('Error checking battery optimization: ${e.message}');
      return false;
    }
  }

  static Future<void> requestDisableBatteryOptimization() async {
    if (!Platform.isAndroid) return;
    try {
      await _channel.invokeMethod('requestDisableBatteryOptimization');
    } on PlatformException catch (e) {
      print('Error requesting battery optimization: ${e.message}');
    }
  }

  /// Dashboard-driven check before punch.
  static Future<bool> showMandatoryBatteryOptimizationDialog(
      BuildContext context) async {
    if (!Platform.isAndroid) return true;

    if (!LocationConfig.ENABLE_BATTERY_OPTIMIZATION_CHECK) {
      return true;
    }

    final mode = LocationConfig.BATTERY_OPTIMIZATION_MODE;
    // Lenient: do not interrupt punch flow.
    if (mode == 2) return true;

    final bool isDisabled = await isBatteryOptimizationDisabled();
    if (isDisabled) return true;

    // Warning Only (0): non-blocking reminder.
    if (mode == 0) {
      await showBatteryOptimizationDialog(context);
      return true;
    }

    // Strict (1): must complete settings before punch.
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
                      final bool isNowDisabled =
                          await isBatteryOptimizationDisabled();
                      if (isNowDisabled) {
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

  static Future<void> showBatteryOptimizationDialog(
      BuildContext context) async {
    if (!Platform.isAndroid) return;
    if (!LocationConfig.ENABLE_BATTERY_OPTIMIZATION_CHECK) return;

    final bool isDisabled = await isBatteryOptimizationDisabled();
    if (isDisabled) return;

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
