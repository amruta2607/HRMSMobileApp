import 'package:flutter/material.dart';

class AppPrimaryButton extends StatelessWidget {
  final Widget child;
  final VoidCallback onTap;

  const AppPrimaryButton({
    super.key,
    required this.child,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return SizedBox(
      width: double.infinity,
      height: 56 * scale,
      child: Container(
        decoration: BoxDecoration(
          color: const Color(0xFF42A5F5),
          borderRadius: BorderRadius.circular(12 * scale),
          boxShadow: [
            // 0px 4px 8px 0px #1A1B2414
            BoxShadow(
              color: const Color(0x141A1B24),
              offset: Offset(0, 4 * scale),
              blurRadius: 8 * scale,
              spreadRadius: 0,
            ),

            // 0px 4px 8px -2px #1A1B241F
            BoxShadow(
              color: const Color(0x1F1A1B24),
              offset: Offset(0, 4 * scale),
              blurRadius: 8 * scale,
              spreadRadius: -2 * scale,
            ),
          ],
        ),
        child: Material(
          color: Colors.transparent,
          child: InkWell(
            borderRadius: BorderRadius.circular(12 * scale),
            onTap: onTap,
            child: Center(child: child),
          ),
        ),
      ),
    );
  }
}

