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
        body: Stack(
          children: [
            const Positioned.fill(
              child: SafeArea(
                child: ProfileBody(),
              ),
            ),
            // Fixed Navigation Icons
            Positioned(
              top: 0,
              left: 0,
              right: 0,
              child: SafeArea(
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 8),
                  child: Row(
                    children: [
                      IconButton(
                        icon: const Icon(Icons.arrow_back_ios, color: AppColors.textDark),
                        onPressed: () => Navigator.pushAndRemoveUntil(
                          context,
                          MaterialPageRoute(builder: (_) => const MainNavigationScreen()),
                              (route) => false,
                        ),
                      ),
                      const Spacer(),
                      Padding(
                        padding: const EdgeInsets.only(right: 16),
                        child: GestureDetector(
                          onTap: () {
                            showModalBottomSheet(
                              context: context,
                              isScrollControlled: true,
                              backgroundColor: Colors.transparent,
                              builder: (context) => const EditProfileDialog(),
                            );
                          },
                          child: const Icon(Icons.edit, color: AppColors.textDark),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ],
        ),
        bottomNavigationBar: CustomNavigationBar(
          currentIndex: -1, // No tab selected when viewing from home
          onChanged: (index) {
            // Pop current profile screen and navigate to selected tab
            Navigator.pop(context);
            // The main navigation screen will handle the index change
          },
        ),
      );
    }

    return HomeScreenScroll(
      body: const ProfileBody(),
    );
  }
}
