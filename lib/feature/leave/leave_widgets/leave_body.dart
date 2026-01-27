import 'package:flutter/material.dart';
import '../apply_leave/apply_leave_screen.dart';
import 'recent_leave_section.dart';
import 'apply_leave_button.dart';

class LeaveBody extends StatelessWidget {
  const LeaveBody({super.key});

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(height: 24 * scale),

        Padding(
          padding: EdgeInsets.symmetric(horizontal: 20 * scale),
          child:ApplyLeaveButton(
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (_) => const ApplyLeaveScreen(),
                ),
              );
            },
          ),
        ),

        SizedBox(height: 28 * scale),

        /// Recent Leave Section
        const Expanded(
          child: RecentLeaveSection(),
        ),
      ],
    );
  }
}
