/**
 * Battery Optimization Service
 * 
 * Handles Android battery optimization settings to ensure reliable
 * punch in/out functionality and attendance system operation.
 */

import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../Theme/app_colors.dart';

class BatteryOptimizationService {
  static const MethodChannel _channel = MethodChannel('battery_optimization');

  /**
   * Check if battery optimization is disabled for this app
   */
  static Future<bool> isBatteryOptimizationDisabled() async {
    if (!Platform.isAndroid) return true; // Only applicable on Android
    try {
      final bool isDisabled =
          await _channel.invokeMethod('isBatteryOptimizationDisabled');
      return isDisabled;
    } on PlatformException catch (e) {
      print('Error checking battery optimization: ${e.message}');
      return false;
    }
  }

  /**
   * Request to disable battery optimization for this app
   */
  static Future<void> requestDisableBatteryOptimization() async {
    if (!Platform.isAndroid) return;
    try {
      await _channel.invokeMethod('requestDisableBatteryOptimization');
    } on PlatformException catch (e) {
      print('Error requesting battery optimization: ${e.message}');
    }
  }

  /**
   * Show mandatory battery optimization dialog for punch functionality
   */
  static Future<bool> showMandatoryBatteryOptimizationDialog(
      BuildContext context) async {
    if (!Platform.isAndroid) return true;
    final bool isDisabled = await isBatteryOptimizationDisabled();

    if (isDisabled) {
      return true; // Already disabled, allow proceeding
    }

    // Show dialog and wait for user to complete the process
    final bool? completed = await showDialog<bool>(
      context: context,
      barrierDismissible: false, // Cannot dismiss by tapping outside
      builder: (BuildContext context) {
        return PopScope(
          canPop: false, // Cannot use back button to dismiss
          child: AlertDialog(
            icon: Icon(
              Icons.battery_alert,
              color: Colors.orange,
              size: 40,
            ),
            title: Text(
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
              Text(
                  'Please disable battery optimization for this app to ensure reliable punch in/out functionality.',
                  style: TextStyle(
                    fontWeight: FontWeight.w500,
                    fontSize: 16,
                    color: Colors.black87,
                  ),
                  textAlign: TextAlign.center,
                ),
                SizedBox(height: 16),
                Container(
                  padding: EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: Colors.blue.shade50,
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: Colors.blue.shade200),
                  ),
                  child: Row(
                    children: [
                      Icon(Icons.info_outline, color: Colors.blue, size: 18),
                      SizedBox(width: 8),
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
            actionsPadding: EdgeInsets.all(16),
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
                      padding: EdgeInsets.symmetric(vertical: 12),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(8),
                      ),
                    ),
                    icon: Icon(Icons.settings, size: 18),
                    label: Text(
                      'Open Settings',
                      style: TextStyle(
                        fontWeight: FontWeight.bold,
                        fontSize: 16,
                      ),
                    ),
                  ),
                  SizedBox(height: 10),
                  OutlinedButton.icon(
                    onPressed: () async {
                      final bool isNowDisabled =
                          await isBatteryOptimizationDisabled();
                      if (isNowDisabled) {
                        Navigator.of(context).pop(true);
                      } else {
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(
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
                      side: BorderSide(color: AppColors.primaryBlue),
                      padding: EdgeInsets.symmetric(vertical: 12),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(8),
                      ),
                    ),
                    icon: Icon(Icons.check, size: 18),
                    label: Text(
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

  /**
   * Show battery optimization dialog (non-mandatory version)
   */
  static Future<void> showBatteryOptimizationDialog(
      BuildContext context) async {
    if (!Platform.isAndroid) return;
    final bool isDisabled = await isBatteryOptimizationDisabled();

    if (isDisabled) {
      return; // Already disabled, no need to show dialog
    }

    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          icon: Icon(
            Icons.battery_alert,
            color: Colors.orange,
            size: 40,
          ),
          title: Text(
            'Optimize Attendance',
            style: TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 18,
            ),
          ),
          content: Text(
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
              child: Text(
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
              child: Text(
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
