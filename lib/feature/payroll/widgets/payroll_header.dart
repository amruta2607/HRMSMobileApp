import 'package:flutter/material.dart';
import '../../Reuse_Widgets/header_bg.dart';
import '../../Navigation/main_navigation_screen.dart';

class PayrollHeader extends StatelessWidget {
  const PayrollHeader({super.key});

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
              Text(
                "Payroll",
                style: TextStyle(
                  fontSize: 24 * scale,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
