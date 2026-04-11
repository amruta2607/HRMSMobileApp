import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'dart:typed_data';
import 'package:http/http.dart' as http;

import '../../../core/Theme/app_colors.dart';
import '../../../core/Utils/services/token_storage.dart';
import 'controller/profile_controller.dart';
import '../Reuse_Widgets/authenticated_image.dart';

class ProfileHeader extends StatelessWidget {
  const ProfileHeader({super.key});

  @override
  Widget build(BuildContext context) {
    final controller = context.watch<ProfileController>();
    final profile = controller.profile;

    final screenWidth = MediaQuery.of(context).size.width;
    const designWidth = 402.0;
    final scale = (screenWidth / designWidth).clamp(0.85, 1.1);
    final radius = 36 * scale;

    final imageUrl = controller.profileImageUrl;

    return SliverToBoxAdapter(
      child: Container(
        decoration: BoxDecoration(
          color: AppColors.primaryBlue.withOpacity(0.08),
          borderRadius: BorderRadius.only(
            bottomLeft: Radius.circular(radius),
            bottomRight: Radius.circular(radius),
          ),
        ),
        child: SafeArea(
          bottom: false,
          child: Column(
            children: [
              // Spacer for fixed navigation icons

              // Profile content
              Padding(
                padding: EdgeInsets.only(top: 0 * scale, bottom: 24 * scale),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    /// Profile Image - Only show if exists
                    CircleAvatar(
                      radius: 44 * scale,
                      backgroundColor: Colors.white,
                      child: imageUrl.isNotEmpty
                          ? AuthenticatedImage(
                        imageUrl: imageUrl,
                        width: 88 * scale,
                        height: 88 * scale,
                        scale: scale,
                        fallbackLetter: profile?.name != null &&
                            profile!.name.isNotEmpty
                            ? profile!.name[0].toUpperCase()
                            : 'U',
                      )
                          : Center(
                        child: Text(
                          profile?.name != null &&
                              profile!.name.isNotEmpty
                              ? profile!.name[0].toUpperCase()
                              : 'U',
                          style: TextStyle(
                            fontSize: 30 * scale,
                            fontWeight: FontWeight.bold,
                            color: AppColors.primaryBlue,
                          ),
                        ),
                      ),
                    ),

                    SizedBox(height: 8 * scale),

                    Text(
                      profile?.name ?? '--',
                      style: const TextStyle(
                        fontSize: 24,
                        fontWeight: FontWeight.w700,
                      ),
                    ),


                    Text(
                      profile?.designation ?? '--',
                      style: const TextStyle(color: Color(0xFF5D6063),
                        fontSize: 15,
                        fontWeight: FontWeight.w500,

                      ),
                    ),

                    SizedBox(height: 8 * scale),

                    Container(
                      padding: EdgeInsets.symmetric(
                        horizontal: 12 * scale,
                        vertical: 2,
                      ),
                      decoration: BoxDecoration(
                        color: AppColors.primaryBlueSoft,
                        borderRadius: BorderRadius.circular(12 * scale),
                      ),
                      child: Text(
                        profile?.empId ?? '--',
                        style: const TextStyle(fontWeight: FontWeight.w600),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

