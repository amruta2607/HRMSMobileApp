// import 'dart:io';
// import 'package:flutter/cupertino.dart';
// import 'package:flutter/material.dart';
//
// import '../../../core/Utils/services/LogIn_out/auth_service.dart';
// import '../../../core/Utils/services/token_storage.dart';
// import '../../Login/login_screen.dart';
//
// class LogoutDialog {
//   static void show(BuildContext context) {
//     if (Platform.isIOS) {
//       showCupertinoDialog(
//         context: context,
//         builder: (_) => CupertinoAlertDialog(
//           title: const Text("Logout"),
//           content: const Text("Are you sure you want to logout?"),
//           actions: [
//             CupertinoDialogAction(
//               child: const Text("Cancel"),
//               onPressed: () => Navigator.pop(context),
//             ),
//             CupertinoDialogAction(
//               isDestructiveAction: true,
//               child: const Text("Logout"),
//               onPressed: () async {
//                 Navigator.pop(context);
//                 await _performLogout(context);
//               },
//             ),
//           ],
//         ),
//       );
//     } else {
//       showDialog(
//         context: context,
//         builder: (_) => AlertDialog(
//           title: const Text("Logout"),
//           content: const Text("Are you sure you want to logout?"),
//           actions: [
//             TextButton(
//               style: TextButton.styleFrom(
//                 foregroundColor: Colors.red,
//               ),
//               child: const Text("Cancel"),
//               onPressed: () => Navigator.pop(context),
//             ),
//             TextButton(
//               style: TextButton.styleFrom(
//                 foregroundColor: Colors.red,
//                 textStyle: const TextStyle(fontWeight: FontWeight.bold),
//               ),
//               child: const Text("Logout"),
//               onPressed: () async {
//                 Navigator.pop(context);
//                 await _performLogout(context);
//               },
//             ),
//           ],
//         ),
//       );
//     }
//   }
//
//   static Future<void> _performLogout(BuildContext context) async {
//     await AuthService.logout();
//
//     await TokenStorage.logout();
//
//     Navigator.pushAndRemoveUntil(
//       context,
//       MaterialPageRoute(builder: (_) => const LoginScreen()),
//           (route) => false,
//     );
//   }
// }



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

  static Future<void> _performLogout(BuildContext context) async {
    await AuthService.logout();
    await TokenStorage.logout();

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