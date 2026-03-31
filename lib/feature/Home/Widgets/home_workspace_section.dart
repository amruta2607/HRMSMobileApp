import 'package:flutter/material.dart';
import '../../../core/Theme/app_colors.dart';
import '../../Announcement/announcement.dart';
import '../../Holiday/holiday_screen.dart';
import '../../leave/leave_screen.dart';
import '../../payroll/payroll_screen.dart';
import 'package:altroz/feature/Navigation/main_navigation_screen.dart'; // 👈 Import
import 'workspace_card.dart';

class HomeWorkspaceSection extends StatelessWidget {
  const HomeWorkspaceSection({super.key});

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        const spacing = 14.0;

        final items = [
          WorkspaceCard(
            title: 'Payroll',
            subtitle: 'Slip',
            imagePath: 'img/payrolll.png',
            iconBgColor: Color(0x8FAFF8CC),
            iconSize: 16,
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (context) => const PayrollScreen()),
              );
            },
          ),

          WorkspaceCard(
            title: 'Tasks',
            subtitle: '3 Pending',
            imagePath: 'img/taskk.png',
            iconColor: AppColors.tasksRed,
            iconBgColor: Color(0x8FAFF8CC),
            iconSize: 16,
            onTap: () {
              // 👇 Navigate to MainNavigationScreen with Alerts tab + Tasks pre-selected
              Navigator.pushAndRemoveUntil(
                context,
                MaterialPageRoute(
                  builder: (context) => const MainNavigationScreen(
                    initialIndex: 1,
                    initialAlertShowTasks: true,
                  ),
                ),
                    (route) => false,
              );
            },
          ),

          WorkspaceCard(
            title: 'Leave',
            subtitle: '12 days',
            imagePath: 'img/leave.png',
            iconBgColor: Color(0x8FAFF8CC),
            iconSize: 16,
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (context) => const LeaveScreen()),
              );
            },
          ),

          WorkspaceCard(
            title: 'Attendance',
            subtitle: 'Punch In/Out',
            icon: Icons.access_time,
            iconColor: AppColors.attendanceBlue,
            iconBgColor: const Color(0x8FAFF8CC),
            iconSize: 22,
            onTap: () {
              Navigator.pushAndRemoveUntil(
                context,
                MaterialPageRoute(
                  builder: (context) => const MainNavigationScreen(initialIndex: 2),
                ),
                    (route) => false,
              );
            },
          ),

          WorkspaceCard(
            title: 'Holiday',
            subtitle: 'Holidays',
            icon: Icons.calendar_today,
            iconColor: AppColors.holidayBlue,
            iconBgColor: const Color(0x8FAFF8CC),
            iconSize: 16,
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (context) => const HolidayScreen()),
              );
            },
          ),

          WorkspaceCard(
            title: 'Announcement',
            subtitle: 'Company News',
            icon: Icons.notifications_active,
            iconColor: AppColors.accentBlue,
            iconBgColor: const Color(0x8FAFF8CC),
            iconSize: 16,
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (_) => const AnnouncementScreen()),
              );
            },
          ),
        ];

        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'WORKSPACE',
              style: TextStyle(
                letterSpacing: 1.4,
                fontSize: 13,
                fontWeight: FontWeight.w700,
                color: AppColors.textGrey,
              ),
            ),
            const SizedBox(height: 13),
            GridView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              itemCount: items.length,
              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 3,
                crossAxisSpacing: spacing,
                mainAxisSpacing: spacing,
                childAspectRatio: 110 / 105, // Adjusted to give more vertical space
              ),
              itemBuilder: (_, index) => items[index],
            ),
          ],
        );
      },
    );
  }
}