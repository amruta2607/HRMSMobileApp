import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../leave_details/leave_details.dart';
import '../model/leave_reuest_model.dart';

class RecentLeaveSection extends StatelessWidget {
  final List<LeaveRequestModel> leaves;

  const RecentLeaveSection({
    super.key,
    this.leaves = const [],
  });

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

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
                    onTap: () {
                      Navigator.push(
                        context,
                        MaterialPageRoute(
                          builder: (_) => LeaveDetailsScreen(leaveData: leave),
                        ),
                      );
                    },

                    child: LeaveRequestTile(
                      title: leave.leaveTypeName,
                      date: _formatDuration(leave.fromDate, leave.toDate, leave.duration),
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

  String _formatDuration(DateTime from, DateTime to, int duration) {
    final start = DateFormat("dd MMM yyyy").format(from);
    final end = DateFormat("dd MMM yyyy").format(to);
    return "$start - $end ($duration days)";
  }
}

class LeaveRequestTile extends StatelessWidget {
  final String title;
  final String date;

  const LeaveRequestTile({
    super.key,
    required this.title,
    required this.date,
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
    if (status.toLowerCase() == 'approved') return Colors.green;
    if (status.toLowerCase().contains('reject')) return Colors.red;
    return Colors.orange;
  }
}