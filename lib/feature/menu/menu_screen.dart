import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';
import '../Alerts/alerts_screen.dart';
import '../Reuse_Widgets/home_screen_constent.dart';
import '../Reuse_Widgets/header_bg.dart';
import '../payroll/payroll_screen.dart';
import '../leave/leave_screen.dart';
import '../Navigation/main_navigation_screen.dart';

class MenuScreen extends StatelessWidget {
  final ValueChanged<int>? onNavigate;
  final VoidCallback? onNavigateToTasks; // 👈 new callback

  const MenuScreen({
    super.key,
    this.onNavigate,
    this.onNavigateToTasks, // 👈 new callback
  });

  @override
  Widget build(BuildContext context) {
    final scale = (MediaQuery.of(context).size.width / 402).clamp(0.85, 1.1);

    final menuItems = [
      _MenuItem(
        title: 'Payroll',
        subtitle: 'View payslips & salary details',
        imagePath: 'img/payrolll.png',
        color: const Color(0xFFE8F5E9),
        iconColor: const Color(0xFF2E7D32),
        onTap: () => Navigator.push(
          context,
          MaterialPageRoute(builder: (_) => const PayrollScreen()),
        ),
      ),
      _MenuItem(
        title: 'Leave',
        subtitle: 'Apply & track leave requests',
        imagePath: 'img/leave.png',
        color: const Color(0xFFFFF3E0),
        iconColor: const Color(0xFFE65100),
        onTap: () => Navigator.push(
          context,
          MaterialPageRoute(builder: (_) => const LeaveScreen()),
        ),
      ),
      _MenuItem(
        title: 'Attendance',
        subtitle: 'Check in, history & reports',
        imagePath: 'img/AttendanceNav.png',
        color: const Color(0xFFE3F2FD),
        iconColor: const Color(0xFF1565C0),
        onTap: () => onNavigate?.call(2),
      ),
      _MenuItem(
        title: 'Task',
        subtitle: 'Check in, notification & task',
        imagePath: 'img/taskk.png',
        color: const Color(0xFFE3F2FD),
        iconColor: const Color(0xFF1565C0),
        onTap: () => onNavigateToTasks?.call(), // 👈 use new callback
      ),
    ];

    return HomeScreenConstent(
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // ── Header ──────────────────────────────────────────────────
          HeaderBackground(
            scale: scale,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Material(
                  color: Colors.transparent,
                  child: InkWell(
                    onTap: () {
                      Navigator.pushAndRemoveUntil(
                        context,
                        MaterialPageRoute(
                          builder: (context) =>
                          const MainNavigationScreen(initialIndex: 0),
                        ),
                            (route) => false,
                      );
                    },
                    borderRadius: BorderRadius.circular(8),
                    splashColor: AppColors.textDark.withOpacity(0.1),
                    highlightColor: AppColors.textDark.withOpacity(0.05),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      crossAxisAlignment: CrossAxisAlignment.center,
                      children: [
                        const Padding(
                          padding:
                          EdgeInsets.only(right: 8.0, top: 4, bottom: 4),
                          child: Icon(
                            Icons.arrow_back_ios,
                            size: 18,
                            color: AppColors.textDark,
                          ),
                        ),
                        Text(
                          'Menu',
                          style: TextStyle(
                            fontSize: 24 * scale,
                            fontWeight: FontWeight.w700,
                            color: AppColors.textDark,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                SizedBox(height: 4 * scale),
                Padding(
                  padding: EdgeInsets.only(left: 18 * scale),
                  child: Text(
                    'Quick access to all features',
                    style: TextStyle(
                      fontSize: 14 * scale,
                      fontWeight: FontWeight.w400,
                      color: AppColors.textGrey,
                    ),
                  ),
                ),
              ],
            ),
          ),

          // ── Menu List ────────────────────────────────────────────────
          Expanded(
            child: ListView.separated(
              padding: EdgeInsets.all(20 * scale),
              itemCount: menuItems.length,
              separatorBuilder: (_, __) => SizedBox(height: 14 * scale),
              itemBuilder: (context, index) {
                final item = menuItems[index];
                return _MenuTile(item: item, scale: scale);
              },
            ),
          ),
        ],
      ),
    );
  }
}

// ── Data class ──────────────────────────────────────────────────────────────

class _MenuItem {
  final String title;
  final String subtitle;
  final String imagePath;
  final Color color;
  final Color iconColor;
  final VoidCallback onTap;

  const _MenuItem({
    required this.title,
    required this.subtitle,
    required this.imagePath,
    required this.color,
    required this.iconColor,
    required this.onTap,
  });
}

// ── Tile Widget ─────────────────────────────────────────────────────────────

class _MenuTile extends StatelessWidget {
  final _MenuItem item;
  final double scale;

  const _MenuTile({required this.item, required this.scale});

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.white,
      borderRadius: BorderRadius.circular(14 * scale),
      child: InkWell(
        onTap: item.onTap,
        borderRadius: BorderRadius.circular(14 * scale),
        child: Container(
          padding: EdgeInsets.symmetric(
            horizontal: 16 * scale,
            vertical: 18 * scale,
          ),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(14 * scale),
            border: Border.all(color: const Color(0xFFE8ECF2), width: 1.5),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withOpacity(0.04),
                blurRadius: 8,
                offset: const Offset(0, 3),
              ),
            ],
          ),
          child: Row(
            children: [
              // Icon box
              Container(
                width: 52 * scale,
                height: 52 * scale,
                decoration: BoxDecoration(
                  color: item.color,
                  borderRadius: BorderRadius.circular(12 * scale),
                ),
                child: Center(
                  child: Image.asset(
                    item.imagePath,
                    width: 26 * scale,
                    height: 26 * scale,
                    color: item.iconColor,
                  ),
                ),
              ),
              SizedBox(width: 16 * scale),

              // Text
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      item.title,
                      style: TextStyle(
                        fontSize: 15 * scale,
                        fontWeight: FontWeight.w700,
                        color: AppColors.textDark,
                      ),
                    ),
                    SizedBox(height: 3 * scale),
                    Text(
                      item.subtitle,
                      style: TextStyle(
                        fontSize: 12.5 * scale,
                        fontWeight: FontWeight.w400,
                        color: AppColors.textGrey,
                      ),
                    ),
                  ],
                ),
              ),

              // Arrow
              Icon(
                Icons.arrow_forward_ios_rounded,
                size: 15 * scale,
                color: AppColors.textGrey,
              ),
            ],
          ),
        ),
      ),
    );
  }
}