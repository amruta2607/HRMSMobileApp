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

    final displayLeaves =
    showLimited && leaves.length > 4 ? leaves.take(4).toList() : leaves;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(height: 8 * scale),

        Padding(
          padding: EdgeInsets.symmetric(horizontal: 20 * scale),
          child: Text(
            "RECENT LEAVE APPLICATION",
            style: TextStyle(
              fontFamily: 'Inter',
              fontWeight: FontWeight.w600,
              fontSize: 14 * scale,
              color: Colors.grey,
            ),
          ),
        ),

        SizedBox(height: 14 * scale),

        /// ✅ Main Scrollable Area (Prevents Overflow)
        Expanded(
          child: ListView.builder(
            padding: EdgeInsets.symmetric(horizontal: 20 * scale),
            itemCount: displayLeaves.length +
                ((showLimited && leaves.length > 4) ? 1 : 0),
            itemBuilder: (context, index) {
              /// Show View All button as last item
              if (showLimited &&
                  leaves.length > 4 &&
                  index == displayLeaves.length) {
                return Padding(
                  padding: EdgeInsets.only(top: 8 * scale),
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
                );
              }

              final leave = displayLeaves[index];

              return Padding(
                padding: EdgeInsets.only(bottom: 12 * scale),
                child: InkWell(
                  onTap: () async {
                    final result = await Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (_) =>
                            LeaveDetailsScreen(leaveData: leave),
                      ),
                    );

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
              );
            },
          ),
        ),
      ],
    );
  }

  String _formatDateRange(DateTime from, DateTime to) {
    final start = DateFormat("dd MMM").format(from);
    final end = DateFormat("dd MMM").format(to);
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
      constraints: BoxConstraints(minHeight: 62 * scale),
      padding: EdgeInsets.symmetric(
        horizontal: 16 * scale,
        vertical: 10 * scale,
      ),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(10 * scale),
        boxShadow: const [
          BoxShadow(
            color: Color(0x59000000),
            blurRadius: 4.5,
            offset: Offset.zero,
          ),
        ],
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          /// Left Side Content
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Flexible(
                      child: Text(
                        title,
                        overflow: TextOverflow.ellipsis,
                        style: TextStyle(
                          fontFamily: 'Inter',
                          fontWeight: FontWeight.w600,
                          fontSize: 14 * scale,
                          color: Colors.black,
                        ),
                      ),
                    ),
                    SizedBox(width: 8 * scale),
                    _StatusBadge(status: status, scale: scale),
                  ],
                ),
                SizedBox(height: 6 * scale),
                Text(
                  date,
                  style: TextStyle(
                    fontFamily: 'Inter',
                    fontWeight: FontWeight.w500,
                    fontSize: 12 * scale,
                    color: Colors.black,
                  ),
                ),
              ],
            ),
          ),

          /// Right Side View Button
          Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(
                "View",
                style: TextStyle(
                  fontFamily: 'Inter',
                  fontWeight: FontWeight.bold,
                  fontSize: 12 * scale,
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
}

class _StatusBadge extends StatelessWidget {
  final String status;
  final double scale;

  const _StatusBadge({
    required this.status,
    required this.scale,
  });

  @override
  Widget build(BuildContext context) {
    final color = _getStatusColor(status);

    return Container(
      padding: EdgeInsets.symmetric(
        horizontal: 8 * scale,
        vertical: 2 * scale,
      ),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(4 * scale),
        border: Border.all(color: color, width: 1),
      ),
      child: Text(
        status,
        style: TextStyle(
          fontFamily: 'Inter',
          fontWeight: FontWeight.w600,
          fontSize: 10 * scale,
          color: color,
        ),
      ),
    );
  }

  Color _getStatusColor(String status) {
    final lower = status.toLowerCase();
    if (lower.contains('approved')) return Colors.green;
    if (lower.contains('reject')) return Colors.red;
    if (lower.contains('withdraw')) return Colors.blue;
    return Colors.orange;
  }
}
