import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';
import '../Reuse_Widgets/home_screen_constent.dart';
import '../Reuse_Widgets/header_bg.dart';
import '../Navigation/navigation_bar.dart';
import '../Navigation/main_navigation_screen.dart';
import 'dispute_form.dart';

class DisputeScreen extends StatelessWidget {
  final String selectedDate;
  final int? punchId;

  const DisputeScreen({
    super.key,
    required this.selectedDate,
    this.punchId,
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
                  /// ✅ Material + InkWell wraps both arrow + title text
                  Material(
                    color: Colors.transparent,
                    child: InkWell(
                      onTap: () => Navigator.pop(context),
                      borderRadius: BorderRadius.circular(8),
                      splashColor: AppColors.textDark.withOpacity(0.1),
                      highlightColor: AppColors.textDark.withOpacity(0.05),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: [
                          Padding(
                            padding: const EdgeInsets.only(
                                right: 8.0, top: 4, bottom: 4),
                            child: Icon(
                              Icons.arrow_back_ios,
                              size: 20 * scale,
                              color: AppColors.textDark,
                            ),
                          ),
                          Text(
                            'Regularization',
                            style: TextStyle(
                              fontSize: 18 * scale,
                              fontWeight: FontWeight.w700,
                              color: AppColors.textDark,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),

                  SizedBox(height: 18 * scale),

                  /// 🔹 FORM CARD
                  DisputeForm(
                    defaultDate: selectedDate,
                    scale: scale,
                    punchId: punchId,
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
