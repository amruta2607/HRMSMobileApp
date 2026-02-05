import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../Navigation/navigation_bar.dart';
import '../../Navigation/main_navigation_screen.dart';
import '../../../core/Theme/app_colors.dart';
import '../../../core/Utils/services/leave_service/leave_service.dart';
import '../dilog/withdraw_leave_dialog.dart';
import '../model/leave_reuest_model.dart';


class LeaveDetailsScreen extends StatefulWidget {
  final LeaveRequestModel leaveData;

  const LeaveDetailsScreen({
    super.key,
    required this.leaveData,
  });

  @override
  State<LeaveDetailsScreen> createState() => _LeaveDetailsScreenState();
}

class _LeaveDetailsScreenState extends State<LeaveDetailsScreen> {
  bool _isLoading = false;

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    final String title = widget.leaveData.leaveTypeName;
    final String date = _formatDuration(widget.leaveData.fromDate, widget.leaveData.toDate, widget.leaveData.duration);
    final String reason = widget.leaveData.description ?? 'No reason provided';
    final String status = widget.leaveData.leaveRequestStatusText;

    Color statusBgColor = AppColors.statusApprovedBg;
    Color statusBorderColor = AppColors.statusApprovedBorder;
    Color statusTextColor = AppColors.statusApprovedText;

    if (status.toLowerCase().contains('submit') || status.toLowerCase().contains('pending Approval')) {
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
    }
   else if (status.toLowerCase().contains('withdraw')) {
      statusBgColor = Colors.blue.shade50;
      statusBorderColor = Colors.blue;
      statusTextColor = Colors.blue;
  }
    else {
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
                    onPressed: _isLoading ? null : () {
                      showDialog(
                        context: context,
                        builder: (context) {
                          return WithdrawLeaveDialog(
                            onWithdraw: (String reason) async {
                              // Save references BEFORE any async operations
                              final messenger = ScaffoldMessenger.of(context);
                              final navigator = Navigator.of(context);

                              setState(() {
                                _isLoading = true;
                              });

                              try {
                                final result = await LeaveService.withdrawLeave(
                                  leaveId: widget.leaveData.id,
                                  reason: reason,
                                );

                                if (!mounted) return;

                                setState(() {
                                  _isLoading = false;
                                });

                                if (result != null && result['success'] == true) {
                                  messenger.showSnackBar(
                                    SnackBar(
                                      content: Text(result['message'] ?? 'Leave request withdrawn successfully'),
                                      backgroundColor: Colors.green,
                                      duration: const Duration(seconds: 2),
                                    ),
                                  );

                                  // Navigate back after a brief delay to ensure snackbar is shown
                                  Future.delayed(const Duration(milliseconds: 300), () {
                                    // Return true to indicate leave was withdrawn successfully
                                    navigator.pop(true);
                                  });
                                } else {
                                  messenger.showSnackBar(
                                    const SnackBar(
                                      content: Text('Failed to withdraw leave request. Please try again.'),
                                      backgroundColor: Colors.red,
                                      duration: Duration(seconds: 2),
                                    ),
                                  );
                                }
                              } catch (e) {
                                if (!mounted) return;

                                setState(() {
                                  _isLoading = false;
                                });

                                messenger.showSnackBar(
                                  SnackBar(
                                    content: Text('Error: $e'),
                                    backgroundColor: Colors.red,
                                    duration: const Duration(seconds: 2),
                                  ),
                                );
                              }
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
                    child: _isLoading
                        ? SizedBox(
                      height: 20 * scale,
                      width: 20 * scale,
                      child: const CircularProgressIndicator(
                        color: Colors.white,
                        strokeWidth: 2,
                      ),
                    )
                        : Text(
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
