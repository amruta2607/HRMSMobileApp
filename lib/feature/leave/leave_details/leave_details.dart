import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../Navigation/navigation_bar.dart';
import '../../Navigation/main_navigation_screen.dart';
import '../../../core/Theme/app_colors.dart';
import '../dilog/withdraw_leave_dialog.dart';
import '../model/leave_reuest_model.dart';

class LeaveDetailsScreen extends StatelessWidget {
  final LeaveRequestModel leaveData;

  const LeaveDetailsScreen({
    super.key,
    required this.leaveData,
  });

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    final String title = leaveData.leaveTypeName;
    final String date = _formatDuration(leaveData.fromDate, leaveData.toDate, leaveData.duration);
    final String reason = leaveData.description ?? 'No reason provided';
    final String status = leaveData.status;

    Color statusBgColor = AppColors.statusApprovedBg;
    Color statusBorderColor = AppColors.statusApprovedBorder;
    Color statusTextColor = AppColors.statusApprovedText;

    if (status.toLowerCase().contains('submit') || status.toLowerCase().contains('pending')) {
      statusBgColor = AppColors.statusPendingBg;
      statusBorderColor = AppColors.statusPendingBorder;
      statusTextColor = AppColors.statusPendingText;
    } else if (status.toLowerCase().contains('reject')) {
      statusBgColor = AppColors.statusRejectedBg;
      statusBorderColor = AppColors.statusRejectedBorder;
      statusTextColor = AppColors.statusRejectedText;
    } else if (status.toLowerCase().contains('approved')) {
      statusBgColor = AppColors.statusApprovedBg;
      statusBorderColor = AppColors.statusApprovedBorder;
      statusTextColor = AppColors.statusApprovedText;
    } else {
      statusBgColor = AppColors.grey96;
      statusBorderColor = Colors.grey;
      statusTextColor = Colors.black;
    }

    return Scaffold(
      backgroundColor: Colors.white,
      bottomNavigationBar: CustomNavigationBar(
        currentIndex: 0,
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
      body: SafeArea(
        child: Padding(
          padding: EdgeInsets.symmetric(horizontal: 20 * scale),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              SizedBox(height: 10 * scale),

              Row(
                children: [
                  InkWell(
                    onTap: () => Navigator.pop(context),
                    child: const Icon(Icons.arrow_back_ios, size: 18),
                  ),
                  SizedBox(width: 8 * scale),
                  Text(
                    "Leave Details",
                    style: TextStyle(
                      fontSize: 20 * scale,
                      fontWeight: FontWeight.w700,
                      color: const Color(0xFF0F172A),
                    ),
                  ),
                ],
              ),

              SizedBox(height: 18 * scale),

              Text(
                title,
                style: TextStyle(
                  fontSize: 20 * scale,
                  fontWeight: FontWeight.w700,
                  color: const Color(0xFF0F172A),
                ),
              ),
              SizedBox(height: 8 * scale), // Added spacing
              Text(
                date,
                style: TextStyle(
                  fontSize: 18 * scale,
                  fontFamily: 'inter',
                  fontWeight: FontWeight.w500,
                  color: const Color(0xFF0F172A),
                ),
              ),

              const Divider(thickness: 1),

              Text(
                "Reason : $reason",
                style: TextStyle(
                  fontSize: 18 * scale,
                  fontWeight: FontWeight.w500,
                  color: const Color(0xFF0F172A),
                ),
              ),

              SizedBox(height: 7 * scale),

              const Divider(thickness: 1),

              SizedBox(height: 14 * scale),

              Container(
                padding: EdgeInsets.symmetric(
                  horizontal: 18 * scale,
                  vertical: 4 * scale,
                ),
                decoration: BoxDecoration(
                  color: statusBgColor,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(
                    color: statusBorderColor,
                  ),
                ),
                child: Text(
                  status,
                  style: TextStyle(
                    fontSize: 16 * scale,
                    fontWeight: FontWeight.w600,
                    color: statusTextColor,
                  ),
                ),
              ),

              SizedBox(height: 25 * scale),



              if (status.toLowerCase().contains('submit') || status.toLowerCase().contains('pending')) ...[
                SizedBox(
                  width: double.infinity,
                  height: 55 * scale,
                  child: ElevatedButton(
                    onPressed: () {
                      showDialog(
                        context: context,
                        builder: (context) {
                          return WithdrawLeaveDialog(
                            onWithdraw: () {
                              // api implementation for withdraw if needed
                            },
                          );
                        },
                      );
                    },
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color(0xFF5CA9F8),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(18),
                      ),
                      elevation: 6,
                    ),
                    child: Text(
                      "Withdraw Leave",
                      style: TextStyle(
                        fontSize: 18 * scale,
                        fontWeight: FontWeight.w600,
                        color: Colors.white,
                      ),
                    ),
                  ),
                ),
              ],


            ],
          ),
        ),
      ),
    );
  }

  String _formatDuration(DateTime from, DateTime to, int duration) {
    final start = DateFormat("dd MMM yyyy").format(from);
    final end = DateFormat("dd MMM yyyy").format(to);
    return "$start - $end ($duration days)";
  }
}
