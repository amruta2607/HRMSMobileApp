import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';

class ActionCard extends StatelessWidget {
  final Widget left;
  final Widget? right;
  final EdgeInsets padding;
  final Color backgroundColor;

  const ActionCard({
    super.key,
    required this.left,
    this.right,
    this.padding = const EdgeInsets.all(16),
    this.backgroundColor = Colors.white,
  });

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        return Container(
          padding: padding,
          decoration: BoxDecoration(
            color: backgroundColor,
            borderRadius: BorderRadius.circular(20),
          ),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              /// LEFT CONTENT — flexible
              Expanded(
                child: left,
              ),

              /// GAP
              if (right != null) const SizedBox(width: 12),

              /// RIGHT CONTENT — fixed width
              if (right != null)
                ConstrainedBox(
                  constraints: const BoxConstraints(
                    minWidth: 90,
                    maxWidth: 140,
                  ),
                  child: right!,
                ),
            ],
          ),
        );
      },
    );
  }
}
