import 'package:flutter/material.dart';
import '../../Attendance/dialogs/dialog_button.dart';

class WithdrawLeaveDialog extends StatefulWidget {
  final Function(String reason) onWithdraw;

  const WithdrawLeaveDialog({
    super.key,
    required this.onWithdraw,
  });

  @override
  State<WithdrawLeaveDialog> createState() => _WithdrawLeaveDialogState();
}

class _WithdrawLeaveDialogState extends State<WithdrawLeaveDialog> {
  final TextEditingController _reasonController = TextEditingController();

  @override
  void dispose() {
    _reasonController.dispose();
    super.dispose();
  }

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
          padding: const EdgeInsets.fromLTRB(20, 24, 20, 20),
          child: IntrinsicHeight(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                /// HEADER
                const Center(
                  child: Text(
                    'Withdraw Leave?',
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.w700,
                      fontFamily: 'Inter',
                      color: Color(0xFF0F172A),
                    ),
                  ),
                ),

                const SizedBox(height: 12),

                /// CONTENT
                const Center(
                  child: Text(
                    "Are you sure you want to withdraw this\nleave request?",
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      fontSize: 14,
                      fontFamily: 'Inter',
                      fontWeight: FontWeight.w400,
                      color: Color(0xFF64748B),
                    ),
                  ),
                ),

                const SizedBox(height: 24),

                /// REASON LABEL
                const Text(
                  'Reason for withdrawl:',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                    fontFamily: 'Inter',
                    color: Color(0xFF0F172A),
                  ),
                ),

                const SizedBox(height: 12),

                /// REASON TEXT FIELD
                Container(
                  decoration: BoxDecoration(
                    border: Border.all(
                      color: const Color(0xFF0F172A),
                      width: 1,
                    ),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: TextField(
                    controller: _reasonController,
                    maxLines: 4,
                    decoration: const InputDecoration(
                      hintText: 'Enter reason...',
                      hintStyle: TextStyle(
                        color: Color(0xFF94A3B8),
                        fontSize: 14,
                        fontFamily: 'Inter',
                      ),
                      border: InputBorder.none,
                      contentPadding: EdgeInsets.all(12),
                    ),
                    style: const TextStyle(
                      fontSize: 14,
                      fontFamily: 'Inter',
                      color: Color(0xFF0F172A),
                    ),
                  ),
                ),

                const SizedBox(height: 24),

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
                        text: 'Submit',
                        filled: true,
                        onTap: () {
                          final reason = _reasonController.text.trim();
                          if (reason.isNotEmpty) {
                            Navigator.pop(context);
                            widget.onWithdraw(reason);
                          } else {
                            ScaffoldMessenger.of(context).showSnackBar(
                              const SnackBar(
                                content: Text('Please enter a reason for withdrawal'),
                                duration: Duration(seconds: 2),
                              ),
                            );
                          }
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
