import 'package:altroz/feature/Announcement/widget/announcement_body.dart';
import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';
import '../Reuse_Widgets/home_screen_constent.dart';
import '../Reuse_Widgets/header_bg.dart';
import '../Navigation/main_navigation_screen.dart';
import '../Navigation/navigation_bar.dart';

class AnnouncementScreen extends StatelessWidget {
  final VoidCallback? onBack;
  const AnnouncementScreen({super.key, this.onBack});

  @override
  Widget build(BuildContext context) {
    final scale = (MediaQuery.of(context).size.width / 402).clamp(0.85, 1.1);

    return HomeScreenConstent(
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // ── Header ──────────────────────────────────────────────────
          HeaderBackground(
            scale: scale,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Material(
                  color: Colors.transparent,
                  child: InkWell(
                    onTap: () {
                      if (onBack != null) {
                        onBack!();
                      } else {
                        Navigator.pushAndRemoveUntil(
                          context,
                          MaterialPageRoute(
                            builder: (context) => const MainNavigationScreen(initialIndex: 0),
                          ),
                              (route) => false,
                        );
                      }
                    },
                    borderRadius: BorderRadius.circular(8),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(
                          Icons.arrow_back_ios,
                          size: 18,
                          color: AppColors.textDark,
                        ),
                        const SizedBox(width: 8),
                        Text(
                          'Announcements',
                          style: TextStyle(
                            fontSize: 24 * scale,
                            fontWeight: FontWeight.w700,
                            color: AppColors.textDark,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                SizedBox(height: 4 * scale),
                Padding(
                  padding: EdgeInsets.only(left: 26 * scale),
                  child: Text(
                    'Stay updated with company news',
                    style: TextStyle(
                      fontSize: 14 * scale,
                      fontWeight: FontWeight.w400,
                      color: AppColors.textGrey,
                    ),
                  ),
                ),
              ],
            ),
          ),

          Expanded(
            child: AnnouncementBody(scale: scale),
          ),
        ],
      ),
      bottomNavigationBar: CustomNavigationBar(
        currentIndex: -1,
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
