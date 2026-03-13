import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';
import 'package:font_awesome_flutter/font_awesome_flutter.dart';

class CustomNavigationBar extends StatelessWidget {
  final int currentIndex;
  final ValueChanged<int> onChanged;

  const CustomNavigationBar({
    super.key,
    required this.currentIndex,
    required this.onChanged,
  });

  @override
  Widget build(BuildContext context) {
    final bottomInset = MediaQuery.of(context).padding.bottom;
    final screenWidth = MediaQuery.of(context).size.width;

    const designWidth = 402.0;
    final scale = (screenWidth / designWidth).clamp(0.9, 1.05);

    return Container(
      height: 65 + bottomInset, // Increased height slightly
      padding: EdgeInsets.only(bottom: bottomInset + 8), // Added extra bottom padding to lift content
      decoration: const BoxDecoration(
        color: AppColors.navBarBg,
        boxShadow: [
          BoxShadow(
            color: AppColors.navBarShadow,
            offset: Offset(0, -1),
            blurRadius: 4,
          ),
        ],
      ),
      child: Row(
        children: [
          _NavItem(
            icon: Icons.home_filled,


            label: 'Home',
            active: currentIndex == 0,
            scale: scale,
            onTap: () => onChanged(0),
          ),
          _NavItem(
            imagePath: 'img/alert.png',
            label: 'Alerts',
            active: currentIndex == 1,
            scale: scale,
            onTap: () => onChanged(1),
          ),

          _NavItem(
            imagePath: 'img/AttendanceNav.png',
            label: 'Attendance',
            active: currentIndex == 2,
            scale: scale,
            onTap: () => onChanged(2),
          ),

          _NavItem(
            imagePath: 'img/menu.png',
            label: 'Menu',
            active: currentIndex == 3,
            scale: scale,
            onTap: () => onChanged(3),
          ),


        ],
      ),
    );
  }
}

class _NavItem extends StatelessWidget {
  final IconData? icon;
  final String? imagePath;
  final String label;
  final bool active;
  final double scale;
  final VoidCallback onTap;

  const _NavItem({
    this.icon,
    this.imagePath,
    required this.label,
    required this.active,
    required this.scale,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: InkWell(
        onTap: onTap,
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            if (imagePath != null)
              Image.asset(
                imagePath!,
                width: 22 * scale,
                height: 22 * scale,
                color: active ? AppColors.primaryBlue : AppColors.iconInactive,
              )
            else if (icon != null)
              Icon(
                icon,
                size: 22 * scale,
                color:
                active ? AppColors.primaryBlue : AppColors.iconInactive,
              ),
            const SizedBox(height: 2),
            Text(
              label,
              style: TextStyle(
                fontSize: 10.5 * scale,
                fontWeight: FontWeight.w500,
                color:
                active ? AppColors.primaryBlue : AppColors.textGrey,
              ),
            ),
          ],
        ),
      ),
    );
  }
}