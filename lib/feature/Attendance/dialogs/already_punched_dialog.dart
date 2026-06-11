import 'package:flutter/material.dart';
import '../../../core/Theme/app_colors.dart';
import 'dialog_button.dart';

class AlreadyPunchedDialog extends StatelessWidget {
  final String title;
  final String message;

  const AlreadyPunchedDialog({
    super.key,
    required this.title,
    required this.message,
  });

  @override
  Widget build(BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;

    return Dialog(
      backgroundColor: Colors.transparent,
      insetPadding: const EdgeInsets.symmetric(horizontal: 24),
      child: Container(
        width: screenWidth * 0.9,
        padding: const EdgeInsets.fromLTRB(24, 32, 24, 24),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(28),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.1),
              blurRadius: 20,
              offset: const Offset(0, 10),
            ),
          ],
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            /// ICON
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: const Color(0xFFF1F5F9), // Slate 100
                shape: BoxShape.circle,
              ),
              child: const Icon(
                Icons.event_available_rounded,
                size: 40,
                color: AppColors.primaryBlue,
              ),
            ),

            const SizedBox(height: 24),

            /// HEADER
            Text(
              title,
              textAlign: TextAlign.center,
              style: const TextStyle(
                fontSize: 20,
                fontWeight: FontWeight.w800,
                color: AppColors.textDark,
                letterSpacing: -0.5,
              ),
            ),

            const SizedBox(height: 12),

            /// MESSAGE
            Text(
              message,
              textAlign: TextAlign.center,
              style: const TextStyle(
                fontSize: 15,
                color: AppColors.textGrey,
                height: 1.5,
              ),
            ),

            const SizedBox(height: 32),

            /// ACTION BUTTON
            SizedBox(
              width: double.infinity,
              height: 54,
              child: DialogButton(
                text: 'Dismiss',
                filled: true,
                onTap: () => Navigator.pop(context),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
