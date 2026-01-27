import 'package:flutter/material.dart';
import '../../Reuse_Widgets/header_bg.dart';
import '../../Navigation/main_navigation_screen.dart';
import 'leave_summary_card.dart';

class LeaveHeader extends StatelessWidget {
  const LeaveHeader({super.key});

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return HeaderBackground(
      scale: scale,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          /// Back + Title
          Row(
            children: [
              InkWell(
                onTap: () {
                  Navigator.pushAndRemoveUntil(
                    context,
                    MaterialPageRoute(
                      builder: (context) => const MainNavigationScreen(initialIndex: 0),
                    ),
                        (route) => false,
                  );
                },
                child: const Icon(Icons.arrow_back_ios, size: 18),
              ),
              SizedBox(width: 8 * scale),
              Text(
                "Leave",
                style: TextStyle(
                  fontSize: 24 * scale,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ],
          ),

          SizedBox(height: 24 * scale),

          /// Centered Cards
          Center(
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                LeaveSummaryCard(
                  title: "Casual Leave",
                  used: 4,
                  total: 12,
                  color: Colors.blue,
                ),
                SizedBox(width: 4 * scale),
                LeaveSummaryCard(
                  title: "Sick Leave",
                  used: 6,
                  total: 10,
                  color: Colors.green,
                ),
                SizedBox(width: 4 * scale),
                LeaveSummaryCard(
                  title: "Earned Leave",
                  used: 10,
                  total: 20,
                  color: Colors.orange,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
