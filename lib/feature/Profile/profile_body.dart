import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:font_awesome_flutter/font_awesome_flutter.dart';

import '../../core/Theme/app_colors.dart';
import '../../core/Utils/services/notification_permission_service.dart';
import 'Profile_Dialog/logout_dialog.dart';
import 'controller/profile_controller.dart';
import 'profile_header.dart';
import 'profile_info_card.dart';

class ProfileBody extends StatelessWidget {
  const ProfileBody({super.key});

  @override
  Widget build(BuildContext context) {
    final controller = context.watch<ProfileController>();
    final profile = controller.profile;

    return CustomScrollView(
      physics: const ClampingScrollPhysics(),
      slivers: [
        const ProfileHeader(),

        SliverPadding(
          padding: const EdgeInsets.fromLTRB(20, 24, 20, 32),
          sliver: SliverList(
            delegate: SliverChildListDelegate(
              [
                _sectionTitle('Personal Details'),

                ProfileInfoCard(
                  items: [
                    ProfileInfoItem(
                      icon: Icons.verified,
                      label: 'Designation',
                      value: profile?.designation ?? '--',
                    ),
                    ProfileInfoItem(
                      icon: Icons.badge,
                      label: 'Employee ID',
                      value: profile?.empId ?? '--',
                    ),
                    ProfileInfoItem(
                      icon: Icons.email,
                      label: 'Email',
                      value: profile?.email ?? '--',
                    ),
                    ProfileInfoItem(
                      icon: Icons.phone,
                      label: 'Phone',
                      value: profile?.phone ?? '--',
                    ),
                    ProfileInfoItem(
                      icon: FontAwesomeIcons.addressCard,
                      label: 'Address',
                      value: profile?.address ?? '--',
                    ),
                    ProfileInfoItem(
                      icon: FontAwesomeIcons.addressCard,
                      label: 'Reporting Manager',
                      value: profile?.reportingManager ?? '--',
                    ),
                  ],
                ),

                const SizedBox(height: 24),
                _sectionTitle('Settings'),

                ProfileInfoCard(
                  items: [
                    const ProfileInfoItem(
                      icon: Icons.lock,
                      label: 'Change Password',
                      isAction: true,
                    ),
                    ProfileInfoItem(
                      icon: Icons.notifications,
                      label: 'Notification Preferences',
                      isAction: true,
                      onTap: () =>
                          NotificationPermissionService.handle(context),
                    ),
                    ProfileInfoItem(
                      icon: Icons.logout,
                      label: 'Logout',
                      isAction: true,
                      color: Colors.red,
                      onTap: () => LogoutDialog.show(context),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _sectionTitle(String text) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Text(
        text,
        style: const TextStyle(
          fontSize: 18,
          fontWeight: FontWeight.w700,
          color: AppColors.textDark,
        ),
      ),
    );
  }
}
