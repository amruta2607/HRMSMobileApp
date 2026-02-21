import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';
import '../Reuse_Widgets/home_screen_constent.dart';
import '../Reuse_Widgets/header_bg.dart';
import '../Navigation/navigation_bar.dart';
import '../Navigation/main_navigation_screen.dart';
import 'dispute_form.dart';

class DisputeScreen extends StatelessWidget {
  final String selectedDate;

  const DisputeScreen({
    super.key,
    required this.selectedDate,
  });

  @override
  Widget build(BuildContext context) {
    final scale =
    (MediaQuery.of(context).size.width / 402).clamp(0.85, 1.1);

    return HomeScreenConstent(
      floating: Align(
        alignment: Alignment.bottomCenter,
        child: CustomNavigationBar(
          currentIndex: 3,
          onChanged: (index) {
            Navigator.pushAndRemoveUntil(
              context,
              MaterialPageRoute(
                builder: (context) =>
                    MainNavigationScreen(initialIndex: index),
              ),
                  (route) => false,
            );
          },
        ),
      ),
      body: SingleChildScrollView(
        padding: EdgeInsets.only(bottom: 80 * scale),
        child: Column(
          children: [
            /// 🔹 HEADER + CARD
            HeaderBackground(
              scale: scale,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  /// 🔹 Back + Title
                  Row(
                    children: [
                      InkWell(
                        onTap: () => Navigator.pop(context),
                        borderRadius: BorderRadius.circular(12),
                        child: Icon(
                          Icons.arrow_back_ios,
                          size: 20 * scale,
                          color: AppColors.textDark,
                        ),
                      ),
                      SizedBox(width: 8 * scale),
                      Text(
                        'Raise a Dispute',
                        style: TextStyle(
                          fontSize: 18 * scale,
                          fontWeight: FontWeight.w700,
                          color: AppColors.textDark,

                        ),
                      ),
                    ],
                  ),

                  SizedBox(height: 18 * scale),

                  /// 🔹 FORM CARD
                  DisputeForm(
                    defaultDate: selectedDate,
                    scale: scale,
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
