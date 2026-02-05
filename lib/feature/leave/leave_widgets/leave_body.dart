
import 'package:flutter/material.dart';
import '../../../core/Utils/services/leave_service/leave_service.dart';
import '../apply_leave/apply_leave_screen.dart';
import '../model/leave_reuest_model.dart';
import '../leave_history/all_leave_requests_screen.dart';
import 'recent_leave_section.dart';
import 'apply_leave_button.dart';

class LeaveBody extends StatefulWidget {
  final VoidCallback? onLeaveApplied;
  const LeaveBody({super.key, this.onLeaveApplied});

  @override
  State<LeaveBody> createState() => _LeaveBodyState();
}

class _LeaveBodyState extends State<LeaveBody> {

  List<LeaveRequestModel> _recentLeaves = [];
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _loadRecentLeaves();
  }

  Future<void> _loadRecentLeaves() async {
    setState(() {
      _isLoading = true;
    });
    final leaves = await LeaveService.getLeaveRequests();
    setState(() {
      _recentLeaves = leaves ?? [];
      _isLoading = false;
    });
  }

  void _handleLeaveApplied() {
    _loadRecentLeaves();
    widget.onLeaveApplied?.call();
  }

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(height: 24 * scale),

        Padding(
          padding: EdgeInsets.symmetric(horizontal: 20 * scale),
          child:ApplyLeaveButton(
            onTap: () async {
              final result = await Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (_) => const ApplyLeaveScreen(),
                ),
              );

              if (result != null) {
                _handleLeaveApplied();
              }
            },
          ),
        ),

        SizedBox(height: 28 * scale),

        /// Recent Leave Section
        Expanded(
          child: _isLoading
              ? const Center(child: CircularProgressIndicator())
              : _recentLeaves.isEmpty
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
              : RecentLeaveSection(
            leaves: _recentLeaves,
            showLimited: true,
            onRefreshNeeded: _loadRecentLeaves,
            onViewAllTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (_) => const AllLeaveRequestsScreen(),
                ),
              );
            },
          ),
        ),
      ],
    );
  }
}

