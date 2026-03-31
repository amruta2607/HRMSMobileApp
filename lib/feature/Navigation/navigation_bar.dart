import 'dart:async';
import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';
import '../../core/Utils/services/alert_service/alert_count_service.dart';

class CustomNavigationBar extends StatefulWidget {
  final int currentIndex;
  final ValueChanged<int> onChanged;

  const CustomNavigationBar({
    super.key,
    required this.currentIndex,
    required this.onChanged,
  });

  @override
  State<CustomNavigationBar> createState() => _CustomNavigationBarState();
}

class _CustomNavigationBarState extends State<CustomNavigationBar> {
  Timer? _timer;

  @override
  void initState() {
    super.initState();
    // Initial fetch
    AlertCountService.fetchCount();
    // Refresh every 2 minutes
    _timer = Timer.periodic(const Duration(minutes: 2), (_) {
      AlertCountService.fetchCount();
    });
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final bottomInset = MediaQuery.of(context).padding.bottom;
    final screenWidth = MediaQuery.of(context).size.width;
    const designWidth = 402.0;
    final scale = (screenWidth / designWidth).clamp(0.9, 1.05);

    return Container(
      height: 65 + bottomInset,
      padding: EdgeInsets.only(bottom: bottomInset + 8),
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
      child: ValueListenableBuilder<int>(
        valueListenable: AlertCountService.alertCountNotifier,
        builder: (context, alertCount, _) {
          return Row(
            children: [
              _NavItem(
                icon: Icons.home_filled,
                label: 'Home',
                active: widget.currentIndex == 0,
                scale: scale,
                onTap: () => widget.onChanged(0),
              ),
              _NavItem(
                imagePath: 'img/NotificationNav.png',
                label: 'Alerts',
                active: widget.currentIndex == 1,
                scale: scale,
                badgeCount: alertCount,
                onTap: () {
                  widget.onChanged(1);
                },
              ),
              _NavItem(
                imagePath: 'img/AttendanceNav.png',
                label: 'Attendance',
                active: widget.currentIndex == 2,
                scale: scale,
                onTap: () => widget.onChanged(2),
              ),
              _NavItem(
                imagePath: 'img/menu.png',
                label: 'Menu',
                active: widget.currentIndex == 3,
                scale: scale,
                onTap: () => widget.onChanged(3),
              ),
            ],
          );
        },
      ),
    );
  }
}

// ─────────────────────────────────────────────────────────────────────────────

class _NavItem extends StatelessWidget {
  final IconData? icon;
  final String? imagePath;
  final String label;
  final bool active;
  final double scale;
  final int badgeCount;
  final VoidCallback onTap;

  const _NavItem({
    this.icon,
    this.imagePath,
    required this.label,
    required this.active,
    required this.scale,
    this.badgeCount = 0,
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
            Stack(
              clipBehavior: Clip.none,
              children: [
                // Icon or Image
                if (imagePath != null)
                  Image.asset(
                    imagePath!,
                    width: 22 * scale,
                    height: 22 * scale,
                    color: active
                        ? AppColors.primaryBlue
                        : AppColors.iconInactive,
                  )
                else if (icon != null)
                  Icon(
                    icon,
                    size: 22 * scale,
                    color: active
                        ? AppColors.primaryBlue
                        : AppColors.iconInactive,
                  ),

                // Red Badge
                if (badgeCount > 0)
                  Positioned(
                    top: -5,
                    right: -6,
                    child: Container(
                      padding: const EdgeInsets.all(2),
                      constraints: const BoxConstraints(
                        minWidth: 16,
                        minHeight: 16,
                      ),
                      decoration: const BoxDecoration(
                        color: Colors.red,
                        shape: BoxShape.circle,
                      ),
                      child: Text(
                        badgeCount > 99 ? '99+' : '$badgeCount',
                        style: TextStyle(
                          color: Colors.white,
                          fontSize: 9 * scale,
                          fontWeight: FontWeight.bold,
                          height: 1.2,
                        ),
                        textAlign: TextAlign.center,
                      ),
                    ),
                  ),
              ],
            ),

            const SizedBox(height: 2),
            Text(
              label,
              style: TextStyle(
                fontSize: 10.5 * scale,
                fontWeight: FontWeight.w500,
                color: active ? AppColors.primaryBlue : AppColors.textGrey,
              ),
            ),
          ],
        ),
      ),
    );
  }
}