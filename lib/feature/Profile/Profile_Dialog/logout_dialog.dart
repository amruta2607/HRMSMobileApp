import 'dart:io';
import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';

import '../../../core/Utils/services/LogIn_out/auth_service.dart';
import '../../../core/Utils/services/token_storage.dart';
import '../../Login/login_screen.dart';

class LogoutDialog {
  static void show(BuildContext context) {
    if (Platform.isIOS) {
      showCupertinoDialog(
        context: context,
        builder: (_) => CupertinoAlertDialog(
          title: const Text("Logout"),
          content: const Text("Are you sure you want to logout?"),
          actions: [
            CupertinoDialogAction(
              child: const Text("Cancel"),
              onPressed: () => Navigator.pop(context),
            ),
            CupertinoDialogAction(
              isDestructiveAction: true,
              child: const Text("Logout"),
              onPressed: () async {
                Navigator.pop(context);
                await _performLogout(context);
              },
            ),
          ],
        ),
      );
    } else {
      showDialog(
        context: context,
        builder: (_) => AlertDialog(
          title: const Text("Logout"),
          content: const Text("Are you sure you want to logout?"),
          actions: [
            TextButton(
              child: const Text("Cancel"),
              onPressed: () => Navigator.pop(context),
            ),
            TextButton(
              child: const Text("Logout"),
              onPressed: () async {
                Navigator.pop(context);
                await _performLogout(context);
              },
            ),
          ],
        ),
      );
    }
  }

  static Future<void> _performLogout(BuildContext context) async {
    await AuthService.logout();

    await TokenStorage.logout();

    Navigator.pushAndRemoveUntil(
      context,
      MaterialPageRoute(builder: (_) => const LoginScreen()),
          (route) => false,
    );
  }
}
