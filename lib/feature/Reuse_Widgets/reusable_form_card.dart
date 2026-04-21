import 'package:flutter/material.dart';

class ReusableFormCard extends StatelessWidget {
  final Widget child;

  const ReusableFormCard({
    super.key,
    required this.child,
  });

  @override
  Widget build(BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;

    /// Figma base width
    const designWidth = 402.0;
    final scale = (screenWidth / designWidth).clamp(0.85, 1.1);

    return Center(
      child: Container(
        width: 347 * scale,
        constraints: BoxConstraints(
          minHeight: 222 * scale,
        ),
        padding: EdgeInsets.all(16 * scale),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(17 * scale),

        ),
        child: child,
      ),
    );
  }
}
