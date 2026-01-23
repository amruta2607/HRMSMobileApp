import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';
import '../Reuse_Widgets/home_screen_scroll.dart';
import '../Navigation/navigation_bar.dart';
import '../Navigation/main_navigation_screen.dart';
import 'Profile_Dialog/edit_profile_dialog.dart';
import 'profile_body.dart';

class ProfileScreen extends StatelessWidget {
  final bool showNavigation;

  const ProfileScreen({
    super.key,
    this.showNavigation = true,
  });

  @override
  Widget build(BuildContext context) {
    if (showNavigation) {
      return Scaffold(
        backgroundColor: AppColors.background,
        body: Column(
          children: [

            /// Top White Navigation Bar
            Container(
              width: double.infinity,
              color: Colors.white,
              child: SafeArea(
                bottom: false,
                child: Padding(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 16, vertical: 12),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: [

                      /// Back Arrow
                      GestureDetector(
                        onTap: () => Navigator.pushAndRemoveUntil(
                          context,
                          MaterialPageRoute(
                            builder: (_) =>
                            const MainNavigationScreen(),
                          ),
                              (route) => false,
                        ),
                        child: const Icon(
                          Icons.arrow_back_ios,
                          size: 22,
                          color: AppColors.textDark,
                        ),
                      ),

                      const SizedBox(width: 8),

                      /// Profile Title
                      const Text(
                        "Profile",
                        style: TextStyle(
                          fontSize: 23,
                          fontWeight: FontWeight.w700,
                          color: AppColors.textDark,
                        ),
                      ),

                      const Spacer(),

                      /// Edit Icon
                      GestureDetector(
                        onTap: () {
                          showModalBottomSheet(
                            context: context,
                            isScrollControlled: true,
                            backgroundColor: Colors.transparent,
                            builder: (context) =>
                            const EditProfileDialog(),
                          );
                        },
                        child: const Icon(
                          Icons.edit,
                          size: 24,
                          color: AppColors.textDark,
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),

            /// Profile Body
            const Expanded(
              child: ProfileBody(),
            ),
          ],
        ),

        bottomNavigationBar: CustomNavigationBar(
          currentIndex: -1,
          onChanged: (index) {
            Navigator.pushAndRemoveUntil(
              context,
              MaterialPageRoute(
                builder: (_) =>
                    MainNavigationScreen(initialIndex: index),
              ),
                  (route) => false,
            );
          },
        ),
      );
    }

    return HomeScreenScroll(
      body: const ProfileBody(),
    );
  }
}
