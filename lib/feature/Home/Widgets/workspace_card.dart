import 'package:flutter/material.dart';
import '../../../core/Theme/app_colors.dart';

class WorkspaceCard extends StatelessWidget {
  final String title;
  final String subtitle;
  final IconData? icon;
  final String? imagePath;
  final Color? iconColor;
  final Color? iconBgColor;
  final double iconSize;
  final VoidCallback? onTap;

  const WorkspaceCard({
    super.key,
    required this.title,
    required this.subtitle,
    this.icon,
    this.imagePath,
    this.iconColor,
    this.iconBgColor,
    this.iconSize = 20, // default unchanged behaviour
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        const figmaWidth = 113.0;

        final scale = (constraints.maxWidth / figmaWidth).clamp(0.75, 1.2);

        return GestureDetector(
          onTap: onTap,
          child: Container(
            decoration: BoxDecoration(
              color: AppColors.workspaceCardBg,
              borderRadius: BorderRadius.circular(20.04 * scale),
              border: Border.all(
                color: AppColors.workspaceCardBorder,
                width: 0.83 * scale,
              ),
              boxShadow: const [
                BoxShadow(
                  color: AppColors.workspaceCardShadow,
                  offset: Offset(0, 2),
                  blurRadius: 8,
                  spreadRadius: -2,
                ),
              ],
            ),
            padding: EdgeInsets.symmetric(
              vertical: 8 * scale,
              horizontal: 6 * scale,
            ),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Container(
                  width: 27 * scale,
                  height: 27 * scale,
                  decoration: BoxDecoration(
                    color: iconBgColor ?? Colors.transparent,
                    shape: BoxShape.circle,
                  ),
                  alignment: Alignment.center,
                  child: imagePath != null
                      ? Image.asset(
                    imagePath!,
                    width: iconSize * scale,
                    height: iconSize * scale,
                  )
                      : Icon(
                    icon,
                    size: iconSize * scale,
                    color: iconColor,
                  ),
                ),

                SizedBox(height: 6 * scale),

                Text(
                  title,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  textAlign: TextAlign.center,
                  style: TextStyle(
                    fontSize: 10.8 * scale,
                    fontWeight: FontWeight.w700,
                    color: AppColors.textDark,
                  ),
                ),

                if (subtitle.isNotEmpty) ...[
                  SizedBox(height: 2 * scale),
                  Text(
                    subtitle,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      fontSize: 9 * scale,
                      color: AppColors.textLight,
                    ),
                  ),
                ],
              ],
            ),
          ),
        );
      },
    );
  }
}