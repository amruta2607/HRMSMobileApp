import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../leave_details/leave_details.dart';
import '../model/leave_reuest_model.dart';

class RecentLeaveSection extends StatelessWidget {
  final List<LeaveRequestModel> leaves;
  final bool showLimited;
  final VoidCallback? onViewAllTap;
  final VoidCallback? onRefreshNeeded;

  const RecentLeaveSection({
    super.key,
    this.leaves = const [],
    this.showLimited = true,
    this.onViewAllTap,
    this.onRefreshNeeded,
  });

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;
    final displayLeaves = showLimited && leaves.length > 3
        ? leaves.take(3).toList()
        : leaves;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(height: 8 * scale),

        Padding(
          padding: EdgeInsets.symmetric(horizontal: 20 * scale),
          child: Text(
            "Recent Leave Request",
            style: TextStyle(
              fontFamily: 'Inter',
              fontWeight: FontWeight.w600,
              fontSize: 14 * scale,
              height: 14.07 / 14,
              color: Colors.grey,
            ),
          ),
        ),

        SizedBox(height: 14 * scale),

        if (showLimited)
        // Limited mode - no Expanded
          Column(
            children: [
              ...displayLeaves.map((leave) => Column(
                children: [
                  SizedBox(height: 4 * scale),
                  Padding(
                    padding: EdgeInsets.symmetric(horizontal: 20 * scale),
                    child: InkWell(
                      onTap: () async {
                        final result = await Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => LeaveDetailsScreen(leaveData: leave),
                          ),
                        );

                        // If leave was withdrawn, trigger refresh
                        if (result == true) {
                          onRefreshNeeded?.call();
                        }
                      },
                      child: LeaveRequestTile(
                        title: leave.leaveTypeName,
                        date: _formatDateRange(leave.fromDate, leave.toDate),
                        status: leave.leaveRequestStatusText,
                      ),
                    ),
                  ),
                ],
              )).toList(),

              // View All button (only show if more than 3 items)
              if (leaves.length > 3 && onViewAllTap != null) ...[
                SizedBox(height: 12 * scale),
                Padding(
                  padding: EdgeInsets.symmetric(horizontal: 20 * scale),
                  child: GestureDetector(
                    onTap: onViewAllTap,
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.end,
                      children: [
                        Text(
                          "View All",
                          style: TextStyle(
                            fontFamily: 'Inter',
                            fontWeight: FontWeight.bold,
                            fontSize: 14 * scale,
                            color: const Color(0xFF0F62FE),
                          ),
                        ),
                        SizedBox(width: 4 * scale),
                        Icon(
                          Icons.arrow_forward_ios,
                          size: 14 * scale,
                          color: const Color(0xFF0F62FE),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ],
          )
        else
        // Full mode - use Expanded for scrolling
          Expanded(
            child: ListView.builder(
              padding: EdgeInsets.symmetric(horizontal: 20 * scale),
              itemCount: leaves.length,
              itemBuilder: (context, index) {
                final leave = leaves[index];
                return Column(
                  children: [
                    SizedBox(height: 4 * scale),

                    InkWell(
                      onTap: () async {
                        final result = await Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => LeaveDetailsScreen(leaveData: leave),
                          ),
                        );

                        // If leave was withdrawn, trigger refresh
                        if (result == true) {
                          onRefreshNeeded?.call();
                        }
                      },

                      child: LeaveRequestTile(
                        title: leave.leaveTypeName,
                        date: _formatDateRange(leave.fromDate, leave.toDate),
                        status: leave.leaveRequestStatusText,
                      ),
                    ),
                  ],
                );
              },
            ),
          ),
      ],
    );
  }

  String _formatDateRange(DateTime from, DateTime to) {
    final start = DateFormat("dd MMM yyyy").format(from);
    final end = DateFormat("dd MMM yyyy").format(to);
    return "$start - $end";
  }
}

class LeaveRequestTile extends StatelessWidget {
  final String title;
  final String date;
  final String status;

  const LeaveRequestTile({
    super.key,
    required this.title,
    required this.date,
    required this.status,
  });

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return Container(
      constraints: BoxConstraints(
        minHeight: 62 * scale,
      ),
      margin: EdgeInsets.only(bottom: 12 * scale),
      padding: EdgeInsets.symmetric(
        horizontal: 16 * scale,
        vertical: 10 * scale,
      ),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(10 * scale),
        boxShadow: [
          BoxShadow(
            color: const Color(0x59000000),
            blurRadius: 4.5 * scale,
            offset: Offset.zero,
          ),
        ],
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Expanded(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Text(
                      title,
                      style: TextStyle(
                        fontFamily: 'Inter',
                        fontWeight: FontWeight.w600,
                        fontSize: 14 * scale,
                        height: 14.07 / 14,
                        color: Colors.black,
                      ),
                    ),
                    SizedBox(width: 8 * scale),
                    Container(
                      padding: EdgeInsets.symmetric(
                        horizontal: 8 * scale,
                        vertical: 2 * scale,
                      ),
                      decoration: BoxDecoration(
                        color: _getStatusColor(status).withOpacity(0.1),
                        borderRadius: BorderRadius.circular(4 * scale),
                        border: Border.all(
                          color: _getStatusColor(status),
                          width: 1,
                        ),
                      ),
                      child: Text(
                        status,
                        style: TextStyle(
                          fontFamily: 'Inter',
                          fontWeight: FontWeight.w600,
                          fontSize: 10 * scale,
                          color: _getStatusColor(status),
                        ),
                      ),
                    ),
                  ],
                ),
                SizedBox(height: 6 * scale),
                Text(
                  date,
                  style: TextStyle(
                    fontFamily: 'Inter',
                    fontWeight: FontWeight.w500,
                    fontSize: 12 * scale,
                    height: 14.07 / 12,
                    color: Colors.black,
                  ),
                ),
                SizedBox(height: 6 * scale),

              ],
            ),
          ),

          Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(
                "View",
                style: TextStyle(
                  fontFamily: 'Inter',
                  fontWeight: FontWeight.bold,
                  fontSize: 12 * scale,
                  height: 14.07 / 12,
                  color: const Color(0xFF0F62FE),
                ),
              ),
              SizedBox(width: 3 * scale),
              Icon(
                Icons.arrow_forward_ios,
                size: 12 * scale,
                color: const Color(0xFF0F62FE),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Color _getStatusColor(String status) {
    if (status.toLowerCase().contains('approved')) return Colors.green;
    if (status.toLowerCase().contains('reject')) return Colors.red;
    if (status.toLowerCase().contains('withdraw')) return Colors.blue;
    return Colors.orange;
  }
}