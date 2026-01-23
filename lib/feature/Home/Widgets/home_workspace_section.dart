import 'package:flutter/material.dart';
import '../../../core/Theme/app_colors.dart';
import 'workspace_card.dart';

class HomeWorkspaceSection extends StatelessWidget {
  const HomeWorkspaceSection({super.key});

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final spacing = 14.0;

        final items = const [

          WorkspaceCard(
            title: 'Payroll',
            subtitle: 'Slip',
            imagePath: 'img/payroll.png',
            iconBgColor: Color(0x8FAFF8CC),

          ),

          WorkspaceCard(
            title: 'Tasks',
            subtitle: '3 Pending',
            imagePath: 'img/tasks.png',
            iconColor: AppColors.tasksRed,
            iconBgColor: Color(0x8FAFF8CC),

          ),

          WorkspaceCard(
            title: 'Leave',
            subtitle: '12 days',
            imagePath: 'img/leave.png',
            iconBgColor: Color(0x8FAFF8CC),
          ),

          WorkspaceCard(
            title: 'Attendance',
            subtitle: '08:42 hrs',
            icon: Icons.access_time,
            iconColor: AppColors.attendanceBlue,
            iconBgColor: Color(0x8FAFF8CC),

          ),

          WorkspaceCard(
            title: 'Attendance',
            subtitle: 'Today',
            icon: Icons.access_time,
            iconColor: AppColors.attendanceBlue,
            iconBgColor: Color(0x8FAFF8CC),

          ),
          WorkspaceCard(
            title: 'Attendance',
            subtitle: 'Today',
            icon: Icons.access_time,
            iconColor: AppColors.textGrey,
            iconBgColor: Color(0x8FAFF8CC),

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
                fontWeight: FontWeight.w600,
                color: AppColors.textGrey,
              ),
            ),

            const SizedBox(height: 16),

            GridView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              itemCount: items.length,
              gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 3,
                crossAxisSpacing: spacing,
                mainAxisSpacing: spacing,

                // 🔹 Figma ratio
                childAspectRatio: 113 / 98.3,
              ),
              itemBuilder: (_, index) => items[index],
            ),
          ],
        );
      },
    );
  }
}


//
// GridView.builder(
// shrinkWrap: true,
// physics: const NeverScrollableScrollPhysics(),
// itemCount: items.length,
// gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
// crossAxisCount: 3, // ✅ FIXED: 3 cards per row
// crossAxisSpacing: 14,
// mainAxisSpacing: 14,
// childAspectRatio: 113 / 98.3, // ✅ Figma ratio
// ),
// itemBuilder: (_, index) => items[index],
// ),
