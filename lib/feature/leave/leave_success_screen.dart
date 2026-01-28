import 'package:flutter/material.dart';
import '../Navigation/navigation_bar.dart';
import '../Navigation/main_navigation_screen.dart';
import '../Reuse_Widgets/leave_primary_button.dart';
import 'leave_screen.dart';

class LeaveSuccessScreen extends StatelessWidget {
  const LeaveSuccessScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;
    final scale = width / 375;

    return Scaffold(
      backgroundColor: Colors.white,
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
      body: SafeArea(
        child: Padding(
          padding: EdgeInsets.symmetric(horizontal: 16 * scale),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              const Spacer(),
              // Success Icon
              Image.asset(
                "img/success_check.png",
                width: 100 * scale,
                height: 100 * scale,
              ),
              SizedBox(height: 32 * scale),

              // Title
              Text(
                "Leave Applied Successfully",
                style: TextStyle(
                  fontFamily: 'Inter',
                  fontWeight: FontWeight.w700,
                  fontSize: 20 * scale,
                  color: const Color(0xFF0F172A),
                ),
                textAlign: TextAlign.center,
              ),
              SizedBox(height: 8 * scale),

              // Subtitle
              Text(
                "Your request has been sent for approval",
                style: TextStyle(
                  fontFamily: 'Inter',
                  fontWeight: FontWeight.w500,
                  fontSize: 16 * scale,
                  color: const Color(0xFF64748B),
                ),
                textAlign: TextAlign.center,
              ),

              const Spacer(),

              // Button
              AppPrimaryButton(
                onTap: () {
                  Navigator.pop(context, true);
                },
                child: Text(
                  "Go to Leave Dashboard",
                  style: TextStyle(
                    fontFamily: 'Roboto',
                    fontWeight: FontWeight.w500,
                    fontSize: 18 * scale,
                    letterSpacing: 0.14 * scale,
                    color: Colors.white,
                  ),
                ),
              ),
              SizedBox(height: 30 * scale),
            ],
          ),
        ),
      ),
    );
  }
}
