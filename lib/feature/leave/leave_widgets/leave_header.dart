import 'package:flutter/material.dart';
import '../../../../core/Utils/services/leave_service/leave_service.dart';
import '../../Reuse_Widgets/header_bg.dart';
import '../../Navigation/main_navigation_screen.dart';
import '../model/leave_balence_model.dart';
import 'leave_summary_card.dart';

class LeaveHeader extends StatefulWidget {
  const LeaveHeader({super.key});

  @override
  State<LeaveHeader> createState() => LeaveHeaderState();
}

class LeaveHeaderState extends State<LeaveHeader> {
  Future<List<LeaveBalanceModel>?>? leaveBalanceFuture;

  @override
  void initState() {
    super.initState();
    refreshData();
  }

  void refreshData() {
    setState(() {
      leaveBalanceFuture = LeaveService.getLeaveBalance();
    });
  }

  Color _getColorForLeaveType(String name) {
    final lower = name.toLowerCase().trim();
    if (lower.contains('sick')) {
      return const Color(0xFFE53935);
    } else if (lower.contains('annual') || lower.contains('casual')) {
      return const Color(0xFF43A047);
    } else if (lower.contains('personal') || lower.contains('earned')) {
      return const Color(0xFFFB8C00);
    } else if (lower.contains('unpaid')) {
      return const Color(0xFF1976D2);
    }
    return const Color(0xFF757575);
  }

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return HeaderBackground(
      scale: scale,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          /// Back + Title
          Row(
            children: [
              InkWell(
                onTap: () {
                  Navigator.pushAndRemoveUntil(
                    context,
                    MaterialPageRoute(
                      builder: (context) => const MainNavigationScreen(initialIndex: 0),
                    ),
                        (route) => false,
                  );
                },
                child: const Icon(Icons.arrow_back_ios, size: 18),
              ),
              Text(
                "Leave",
                style: TextStyle(
                  fontSize: 24 * scale,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ],
          ),

          SizedBox(height: 20 * scale),

          /// Centered Cards
          FutureBuilder<List<LeaveBalanceModel>?>(
            future: leaveBalanceFuture,
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Center(child: CircularProgressIndicator());
              }


              final data = snapshot.data;

              if (data == null || data.isEmpty) {
                return const Center(child: Text("No Data"));
              }

              return SingleChildScrollView(
                scrollDirection: Axis.horizontal,
                padding: EdgeInsets.only(left: 8 * scale, right: 8 * scale),
                child: Row(
                  children: data.map((item) {
                    return Padding(
                      padding: EdgeInsets.only(right: 12 * scale),
                      child: LeaveSummaryCard(
                        title: item.leaveTypeName,
                        used: item.remainingBalance,
                        total: item.totalBalance,
                        color: _getColorForLeaveType(item.leaveTypeName),
                      ),
                    );
                  }).toList(),
                ),
              );
            },
          ),
        ],
      ),
    );
  }
}
