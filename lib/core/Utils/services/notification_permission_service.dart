import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter/cupertino.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:app_settings/app_settings.dart';

class NotificationPermissionService {
  static Future<void> handle(BuildContext context) async {
    final status = await Permission.notification.status;

    if (status.isGranted) {
      _showSnack(context, 'Notifications already enabled');
      return;
    }

    if (status.isDenied) {
      final result = await Permission.notification.request();

      if (result.isGranted) {
        _showSnack(context, 'Notifications enabled');
      } else {
        _showSettingsDialog(context);
      }
      return;
    }

    if (status.isPermanentlyDenied) {
      _showSettingsDialog(context);
    }
  }


  static void _showSettingsDialog(BuildContext context) {
    if (Platform.isIOS) {
      _iosDialog(context);
    } else {
      _androidDialog(context);
    }
  }

  static void _androidDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Enable Notifications'),
        content: const Text(
          'Notifications are disabled. Please enable them from app settings.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancel'),
          ),
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              AppSettings.openAppSettings();
            },
            child: const Text('Open Settings'),
          ),
        ],
      ),
    );
  }

  static void _iosDialog(BuildContext context) {
    showCupertinoDialog(
      context: context,
      builder: (_) => CupertinoAlertDialog(
        title: const Text('Enable Notifications'),
        content: const Text(
          'Please enable notifications from Settings to stay updated.',
        ),
        actions: [
          CupertinoDialogAction(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancel'),
          ),
          CupertinoDialogAction(
            isDefaultAction: true,
            onPressed: () {
              Navigator.pop(context);
              AppSettings.openAppSettings();
            },
            child: const Text('Open Settings'),
          ),
        ],
      ),
    );
  }

  static void _showSnack(BuildContext context, String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message)),
    );
  }
}
