import 'package:altroz/feature/payroll/widgets/ payroll_body.dart';
import 'package:altroz/feature/payroll/widgets/payroll_header.dart';
import 'package:flutter/material.dart';
import '../Reuse_Widgets/home_screen_constent.dart';

import '../Navigation/main_navigation_screen.dart';
import '../Navigation/navigation_bar.dart';

class PayrollScreen extends StatelessWidget {
  const PayrollScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return HomeScreenConstent(
      body: Column(
        children: const [
          PayrollHeader(),
          Expanded(child: PayrollBody()),
        ],
      ),
      bottomNavigationBar: CustomNavigationBar(
        currentIndex: 0,
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
