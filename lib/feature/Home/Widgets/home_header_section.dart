import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../../core/Theme/app_colors.dart';
import '../home_controller/home_controller.dart';
import '../../Profile/profile_screen.dart';

class HomeHeaderSection extends StatefulWidget {
  const HomeHeaderSection({super.key});

  @override
  State<HomeHeaderSection> createState() => _HomeHeaderSectionState();
}

class _HomeHeaderSectionState extends State<HomeHeaderSection> {
  late HomeController _controller;

  @override
  void initState() {
    super.initState();
    _controller = HomeController();
    _controller.fetchHomeData();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;

    // Design reference width (Figma mobile)
    const designWidth = 402.0;

    // Scale factor
    final scale = (screenWidth / designWidth).clamp(0.85, 1.1);

    final String liveDate = DateFormat('MMM dd, EEEE').format(DateTime.now());

    return ChangeNotifierProvider.value(
      value: _controller,
      child: Container(
        width: double.infinity,
        padding: EdgeInsets.fromLTRB(
          20 * scale,
          24 * scale,
          20 * scale,
          28 * scale,
        ),
        decoration: BoxDecoration(
          color: AppColors.HeaderBg,
          borderRadius: BorderRadius.only(
            bottomLeft: Radius.circular(36 * scale),
            bottomRight: Radius.circular(36 * scale),
          ),
        ),
        child: Column(
          children: [
            // User Info Section - Dynamic
            Consumer<HomeController>(
              builder: (context, controller, child) {
                final firstName = controller.firstName;
                final profilePicture = controller.profilePicture;

                return Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          liveDate,
                          style: TextStyle(
                            fontSize: 14 * scale,
                            color: AppColors.textLight,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                        SizedBox(height: 6 * scale),
                        Text(
                          'Hi, $firstName',
                          style: TextStyle(
                            fontSize: 28 * scale,
                            fontWeight: FontWeight.w700,
                            color: AppColors.textDark,
                          ),
                        ),
                      ],
                    ),
                    const Spacer(),
                    GestureDetector(
                      onTap: () {
                        // Navigate to Profile Screen
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (context) => const ProfileScreen(),
                          ),
                        );
                      },
                      child: Container(
                        padding: EdgeInsets.all(2 * scale),
                        decoration: const BoxDecoration(
                          color: AppColors.background,
                          shape: BoxShape.circle,
                        ),
                        child: profilePicture != null
                            ? CircleAvatar(
                          radius: 22 * scale,
                          backgroundColor: AppColors.homeStatusTextGreen,
                          child: ClipOval(
                            child: Image.network(
                              profilePicture,
                              width: 44 * scale,
                              height: 44 * scale,
                              fit: BoxFit.cover,
                              loadingBuilder: (context, child, loadingProgress) {
                                if (loadingProgress == null) return child;
                                // Show loading state
                                return Center(
                                  child: SizedBox(
                                    width: 20 * scale,
                                    height: 20 * scale,
                                    child: CircularProgressIndicator(
                                      strokeWidth: 2,
                                      valueColor: const AlwaysStoppedAnimation<Color>(
                                        Colors.white,
                                      ),
                                    ),
                                  ),
                                );
                              },
                              errorBuilder: (context, error, stackTrace) {
                                // Show user initial on error (consistent with Profile Screen)
                                return Center(
                                  child: Text(
                                    firstName.isNotEmpty
                                        ? firstName[0].toUpperCase()
                                        : 'U',
                                    style: TextStyle(
                                      fontSize: 20 * scale,
                                      fontWeight: FontWeight.bold,
                                      color: Colors.white,
                                    ),
                                  ),
                                );
                              },
                            ),
                          ),
                        )
                            : CircleAvatar(
                          radius: 22 * scale,
                          backgroundColor: AppColors.homeStatusTextGreen,
                          child: Text(
                            firstName.isNotEmpty
                                ? firstName[0].toUpperCase()
                                : 'U',
                            style: TextStyle(
                              fontSize: 20 * scale,
                              fontWeight: FontWeight.bold,
                              color: Colors.white,
                            ),
                          ),
                        ),
                      ),
                    ),
                  ],
                );
              },
            ),
            SizedBox(height: 24 * scale),

            // Attendance Status Card - Using Consumer
            Consumer<HomeController>(
              builder: (context, controller, child) {
                // Loading state
                if (controller.isLoading) {
                  return Container(
                    padding: EdgeInsets.symmetric(
                      horizontal: 16 * scale,
                      vertical: 14 * scale,
                    ),
                    decoration: BoxDecoration(
                      color: AppColors.homeStatusCardBg,
                      borderRadius: BorderRadius.circular(18 * scale),
                    ),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        SizedBox(
                          width: 20 * scale,
                          height: 20 * scale,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            valueColor: AlwaysStoppedAnimation<Color>(
                              AppColors.homeStatusTextGreen,
                            ),
                          ),
                        ),
                        SizedBox(width: 12 * scale),
                        Text(
                          'Loading attendance...',
                          style: TextStyle(
                            fontFamily: 'Inter',
                            fontSize: 14 * scale,
                            color: AppColors.textGrey,
                          ),
                        ),
                      ],
                    ),
                  );
                }

                final status = controller.attendanceStatus;

                // Error or no data
                if (status == null || controller.errorMessage.isNotEmpty) {
                  return Container(
                    padding: EdgeInsets.symmetric(
                      horizontal: 16 * scale,
                      vertical: 14 * scale,
                    ),
                    decoration: BoxDecoration(
                      color: AppColors.homeStatusCardBg,
                      borderRadius: BorderRadius.circular(18 * scale),
                    ),
                    child: Row(
                      children: [
                        Container(
                          padding: EdgeInsets.all(12 * scale),
                          decoration: BoxDecoration(
                            shape: BoxShape.circle,
                            color: Colors.grey.withOpacity(0.2),
                          ),
                          child: Icon(
                            Icons.info_outline,
                            size: 20 * scale,
                            color: AppColors.textGrey,
                          ),
                        ),
                        SizedBox(width: 12 * scale),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                'Current Status',
                                style: TextStyle(
                                  fontFamily: 'Inter',
                                  fontSize: 13 * scale,
                                  color: AppColors.textGrey,
                                ),
                              ),
                              Text(
                                'Not marked yet',
                                style: TextStyle(
                                  fontFamily: 'Inter',
                                  fontSize: 15 * scale,
                                  fontWeight: FontWeight.w500,
                                  color: AppColors.textGrey,
                                ),
                              ),
                            ],
                          ),
                        ),
                        TextButton(
                          onPressed: () {},
                          style: TextButton.styleFrom(
                            backgroundColor: AppColors.log,
                            padding: EdgeInsets.symmetric(
                              horizontal: 16 * scale,
                              vertical: 8 * scale,
                            ),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12 * scale),
                            ),
                          ),
                          child: Text(
                            'Log',
                            style: TextStyle(
                              fontFamily: 'Inter',
                              fontWeight: FontWeight.w600,
                              fontSize: 14 * scale,
                              color: Colors.black,
                            ),
                          ),
                        ),
                      ],
                    ),
                  );
                }

                // Success - Show attendance data
                final String displayStatus = status.isMarked
                    ? '${status.status} • On Time'
                    : 'Not marked yet';

                final Color statusColor = status.isMarked
                    ? AppColors.homeStatusTextGreen
                    : AppColors.textGrey;

                final IconData statusIcon =
                status.isMarked ? Icons.check : Icons.error_outline;

                final Color iconBgColor = status.isMarked
                    ? AppColors.homeStatusIconBg // Green when marked
                    : Colors.red.withOpacity(0.3);

                return Container(
                  padding: EdgeInsets.symmetric(
                    horizontal: 16 * scale,
                    vertical: 14 * scale,
                  ),
                  decoration: BoxDecoration(
                    color: AppColors.homeStatusCardBg,
                    borderRadius: BorderRadius.circular(18 * scale),
                  ),
                  child: Row(
                    children: [
                      Container(
                        padding: EdgeInsets.all(12 * scale),
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          color: iconBgColor,
                        ),
                        child: Icon(
                          statusIcon,
                          size: 20 * scale,
                          color: statusColor,
                        ),
                      ),
                      SizedBox(width: 12 * scale),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Current Status',
                              style: TextStyle(
                                fontFamily: 'Inter',
                                fontSize: 13 * scale,
                                color: AppColors.textGrey,
                              ),
                            ),
                            Text(
                              displayStatus,
                              style: TextStyle(
                                fontFamily: 'Inter',
                                fontSize: 15 * scale,
                                fontWeight: FontWeight.w500,
                                color: statusColor,
                              ),
                            ),
                          ],
                        ),
                      ),
                      TextButton(
                        onPressed: () {},
                        style: TextButton.styleFrom(
                          backgroundColor: AppColors.log,
                          padding: EdgeInsets.symmetric(
                            horizontal: 16 * scale,
                            vertical: 8 * scale,
                          ),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(12 * scale),
                          ),
                        ),
                        child: Text(
                          'Log',
                          style: TextStyle(
                            fontFamily: 'Inter',
                            fontWeight: FontWeight.w600,
                            fontSize: 14 * scale,
                            color: Colors.black,
                          ),
                        ),
                      ),
                    ],
                  ),
                );
              },
            ),
          ],
        ),
      ),
    );
  }
}
