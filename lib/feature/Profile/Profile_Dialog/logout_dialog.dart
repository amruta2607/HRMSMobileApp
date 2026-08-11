import 'dart:io';
import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';

import '../../../core/Utils/services/LogIn_out/auth_service.dart';
import '../../../core/Utils/services/token_storage.dart';
import '../../../core/Utils/services/Attendance service/attendance_service.dart';
import '../../../core/Background_location _tracking/services/location_service.dart'
    as bg_tracking;
import '../../../core/Background_location _tracking/services/gps_monitor_service.dart';
import '../../Login/login_screen.dart';
import 'package:provider/provider.dart';
import '../../Profile/controller/profile_controller.dart';
import '../../Reuse_Widgets/authenticated_image.dart';
import '../../Tenant/controller/tenant_controller.dart';

class LogoutDialog {
  static void show(BuildContext context) {
    // Must punch out before logout while still clocked in.
    if (AttendanceService.isClockedIn) {
      _showPunchOutRequired(context);
      return;
    }

    if (Platform.isIOS) {
      showCupertinoDialog(
        context: context,
        builder: (_) => CupertinoAlertDialog(
          title: const Text("Log out?"),
          content: const Text("Are you sure you want to logout?"),
          actions: [
            CupertinoDialogAction(
              isDestructiveAction: true,
              onPressed: () async {
                Navigator.pop(context);
                await _performLogout(context);
              },
              child: const Text("Log out"),
            ),
            CupertinoDialogAction(
              onPressed: () => Navigator.pop(context),
              child: const Text("Cancel"),
            ),
          ],
        ),
      );
    } else {
      showDialog(
        context: context,
        barrierColor: Colors.black.withOpacity(0.35),
        builder: (_) => Dialog(
          backgroundColor: Colors.white,
          surfaceTintColor: Colors.transparent,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(24),
          ),
          child: Padding(
            padding: const EdgeInsets.fromLTRB(20, 28, 20, 16),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Text(
                  "Log out?",
                  style: TextStyle(
                    fontSize: 15,
                    fontWeight: FontWeight.w600,
                    letterSpacing: -0.2,
                  ),
                ),
                const SizedBox(height: 6),
                Text(
                  "Are you sure you want to logout?",
                  textAlign: TextAlign.center,
                  style: TextStyle(
                    fontSize: 13,
                    color: Colors.grey.shade500,
                    height: 1.5,
                  ),
                ),
                const SizedBox(height: 20),
                _DialogButton(
                  label: "Log out",
                  background: const Color(0xFFFEE2E2),
                  foreground: const Color(0xFFDC2626),
                  onTap: () async {
                    Navigator.pop(context);
                    await _performLogout(context);
                  },
                ),
                const SizedBox(height: 8),
                _DialogButton(
                  label: "Cancel",
                  background: const Color(0xFFF4F4F5),
                  foreground: Colors.grey.shade600,
                  onTap: () => Navigator.pop(context),
                ),
              ],
            ),
          ),
        ),
      );
    }
  }

  static void _showPunchOutRequired(BuildContext context) {
    const message =
        'You are still punched in. Please Punch-Out first, then log out.';

    if (Platform.isIOS) {
      showCupertinoDialog(
        context: context,
        builder: (_) => CupertinoAlertDialog(
          title: const Text('Punch-Out required'),
          content: const Text(message),
          actions: [
            CupertinoDialogAction(
              onPressed: () => Navigator.pop(context),
              child: const Text('OK'),
            ),
          ],
        ),
      );
    } else {
      showDialog(
        context: context,
        builder: (_) => AlertDialog(
          title: const Text('Punch-Out required'),
          content: const Text(message),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('OK'),
            ),
          ],
        ),
      );
    }
  }

  static Future<void> _performLogout(BuildContext context) async {

    try {
      context.read<TenantController>().clearData();
      context.read<ProfileController>().clearData();
    } catch (_) {}
    AuthenticatedImage.clearCache();
    if (AttendanceService.isClockedIn) {
      if (context.mounted) _showPunchOutRequired(context);
      return;
    }

    try {
      await GpsMonitorService.instance.stopMonitoring();
    } catch (_) {}
    try {
      await bg_tracking.LocationService.instance.stopTracking();
    } catch (_) {}

    await AuthService.logout();
    await TokenStorage.logout();

    AttendanceService.isClockedInNotifier.value = false;
    AttendanceService.punchInTimeNotifier.value = null;
    AttendanceService.isPunchedOutForTodayNotifier.value = false;

    if (!context.mounted) return;

    Navigator.pushAndRemoveUntil(
      context,
      MaterialPageRoute(builder: (_) => const LoginScreen()),
      (route) => false,
    );
  }
}

class _DialogButton extends StatelessWidget {
  const _DialogButton({
    required this.label,
    required this.background,
    required this.foreground,
    required this.onTap,
  });

  final String label;
  final Color background;
  final Color foreground;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: double.infinity,
      child: TextButton(
        onPressed: onTap,
        style: TextButton.styleFrom(
          backgroundColor: background,
          foregroundColor: foreground,
          padding: const EdgeInsets.symmetric(vertical: 12),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
        ),
        child: Text(
          label,
          style: const TextStyle(
            fontSize: 14,
            fontWeight: FontWeight.w500,
          ),
        ),
      ),
    );
  }
}
