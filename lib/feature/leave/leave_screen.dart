import 'package:flutter/material.dart';
import '../Reuse_Widgets/home_screen_constent.dart';
import 'leave_widgets/leave_body.dart';
import 'leave_widgets/leave_header.dart';


import '../Navigation/main_navigation_screen.dart';
import '../Navigation/navigation_bar.dart';

class LeaveScreen extends StatelessWidget {
  const LeaveScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return HomeScreenConstent(
      body: Column(
        children: const [
          LeaveHeader(),
          Expanded(child: LeaveBody()),
        ],
      ),
      bottomNavigationBar: CustomNavigationBar(
        currentIndex: 0, // Assuming Home or Leave context; usually 0 for Home if not a separate tab
        onChanged: (index) {
          Navigator.pushAndRemoveUntil(
            context,
            MaterialPageRoute(
              builder: (context) => MainNavigationScreen(initialIndex: index),
            ),
                (route) => false,
          );
        },
      ),
    );
  }
}
