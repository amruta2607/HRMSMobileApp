import 'package:altroz/feature/leave/leave_widgets/recent_leave_section.dart';
import 'package:flutter/material.dart';
import '../../Navigation/main_navigation_screen.dart';
import '../../Navigation/navigation_bar.dart';
import '../../Reuse_Widgets/header_bg.dart';
import '../../../core/Utils/services/leave_service/leave_service.dart';
import '../model/leave_history_model.dart';
import 'all_leaves_table.dart';

class AllLeaveRequestsScreen extends StatefulWidget {
  const AllLeaveRequestsScreen({
    super.key,
  });

  @override
  State<AllLeaveRequestsScreen> createState() => _AllLeaveRequestsScreenState();
}

class _AllLeaveRequestsScreenState extends State<AllLeaveRequestsScreen> {
  LeaveHistoryModel? _leaveData;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _loadLeaves();
  }

  Future<void> _loadLeaves() async {
    setState(() {
      _isLoading = true;
    });
    final data = await LeaveService.getLeaveHistory();
    setState(() {
      _leaveData = data;
      _isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

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
        child: Column(
          children: [
            /// Header
            HeaderBackground(
              scale: scale,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  /// Back + Title
                  Row(
                    children: [
                      InkWell(
                        onTap: () {
                          Navigator.pop(context);
                        },
                        child: const Icon(Icons.arrow_back_ios, size: 18),
                      ),
                      Text(
                        "Leave History",
                        style: TextStyle(
                          fontSize: 24 * scale,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ],
                  ),

                  SizedBox(height: 8 * scale),

                  /// Subtitle
                  Text(
                    "${_leaveData?.usedLeaves ?? 0} Leaves availed in ${_leaveData?.year ?? DateTime.now().year}",
                    style: TextStyle(
                      fontFamily: 'Inter',
                      fontWeight: FontWeight.w500,
                      fontSize: 16 * scale,
                      height: 1.1,
                      color: const Color(0xFF94A3B8),
                    ),
                  ),
                ],
              ),
            ),

            /// Content
            Expanded(
              child: SingleChildScrollView(
                child: _isLoading
                    ? const Center(child: CircularProgressIndicator())
                    : (_leaveData == null || _leaveData!.leaveHistory.isEmpty)
                    ? Center(
                  child: Text(
                    "No leave application found",
                    style: TextStyle(
                      fontFamily: 'Inter',
                      fontSize: 14 * scale,
                      color: Colors.grey,
                    ),
                  ),
                )
                    : Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Padding(
                      padding: EdgeInsets.fromLTRB(20 * scale, 24 * scale, 20 * scale, 16 * scale),
                      child: Text(
                        "ALL LEAVES:",
                        style: TextStyle(
                          fontFamily: 'Inter',
                          fontWeight: FontWeight.w700,
                          fontSize: 14 * scale,
                          color: const Color(0xFF94A3B8),
                          letterSpacing: 0.5,
                        ),
                      ),
                    ),
                    AllLeavesTable(leaves: _leaveData!.leaveHistory),
                    SizedBox(height: 24 * scale),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
