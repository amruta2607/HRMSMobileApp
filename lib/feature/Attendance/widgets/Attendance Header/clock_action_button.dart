import 'package:flutter/material.dart';

class ClockActionButton extends StatelessWidget {
  final bool isClockedIn;
  final VoidCallback onTap;

  const ClockActionButton({
    super.key,
    required this.isClockedIn,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final scale =
    (MediaQuery.of(context).size.width / 402).clamp(0.85, 1.1);

    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 70 * scale,
        height: 32 * scale,
        alignment: Alignment.center,
        decoration: BoxDecoration(
          color: isClockedIn
              ? const Color(0xFFF8D1AF)
              : const Color(0xFFB2FFE3),
          borderRadius: BorderRadius.circular(8 * scale),
        ),
        child: Text(
          isClockedIn ? 'Clock-Out' : 'Clock-In',
          style: TextStyle(
            fontSize: 12 * scale,
            fontWeight: FontWeight.w600,
            color: Colors.black,
          ),
        ),
      ),
    );
  }
}
