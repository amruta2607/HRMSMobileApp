import 'package:flutter/material.dart';
import '../../Attendance/dialogs/dialog_button.dart';
import '../../../core/Theme/app_colors.dart';

class WithdrawLeaveDialog extends StatelessWidget {
  final VoidCallback onWithdraw;

  const WithdrawLeaveDialog({
    super.key,
    required this.onWithdraw,
  });

  @override
  Widget build(BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;

    return Dialog(
      insetPadding: const EdgeInsets.symmetric(horizontal: 24),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
      ),
      child: ConstrainedBox(
        constraints: BoxConstraints(
          maxWidth: screenWidth * 0.9,
        ),
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 18, 20, 16),
          child: IntrinsicHeight(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                /// HEADER
                const Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [

                    SizedBox(width: 10),
                    Text(
                      'Withdraw Leave?',
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 14),

                /// CONTENT
                const Text(
                  "Are you sure you want to withdraw this\nleave request?",
                  textAlign: TextAlign.center,
                  style: TextStyle(
                    fontSize: 14,
                    color: Color(0xFF5D6063),
                  ),
                ),

                const SizedBox(height: 18),

                /// ACTION BUTTONS
                Row(
                  children: [
                    Expanded(
                      child: DialogButton(
                        text: 'Cancel',
                        onTap: () => Navigator.pop(context),
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: DialogButton(
                        text: 'Yes,Withdraw',
                        filled: true,
                        onTap: () {
                          Navigator.pop(context);
                          onWithdraw();
                        },
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
