import 'package:altroz/feature/Holiday/widget/holiday_body.dart';
import 'package:flutter/material.dart';
import '../Reuse_Widgets/home_screen_constent.dart';
import '../Navigation/navigation_bar.dart';
import '../Navigation/main_navigation_screen.dart';

class HolidayScreen extends StatelessWidget {
  const HolidayScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return HomeScreenConstent(
      body: const HolidayBody(),
      bottomNavigationBar: CustomNavigationBar(
        currentIndex: 3, // Assuming it's part of Menu/Home context
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
