import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';

class HeaderBackground extends StatelessWidget {
  final Widget child;
  final double scale;
  final EdgeInsetsGeometry? padding;
  final BorderRadiusGeometry? borderRadius;
  final Color? backgroundColor;

  const HeaderBackground({
    super.key,
    required this.child,
    required this.scale,
    this.padding,
    this.borderRadius,
    this.backgroundColor,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: padding ??
          EdgeInsets.fromLTRB(
            20 * scale,
            24 * scale,
            20 * scale,
            28 * scale,
          ),
      decoration: BoxDecoration(
        color: backgroundColor ?? AppColors.HeaderBg,
        borderRadius: borderRadius ??
            BorderRadius.only(
              bottomLeft: Radius.circular(36 * scale),
              bottomRight: Radius.circular(36 * scale),
            ),
      ),
      child: child,
    );
  }
}
