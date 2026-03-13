import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../model/leave_reuest_model.dart';

class AllLeavesTable extends StatelessWidget {
  final List<LeaveRequestModel> leaves;

  const AllLeavesTable({
    super.key,
    required this.leaves,
  });

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return Container(
      height: 400 * scale, // Fixed height for scrollability
      margin: EdgeInsets.symmetric(horizontal: 20 * scale),
      padding: EdgeInsets.all(16 * scale),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(23.07 * scale),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.08),
            blurRadius: 13.84 * scale,
            offset: Offset(0, 4.61 * scale),
          ),
        ],
      ),
      child: Column(
        children: [
          /// Table Header
          Padding(
            padding: EdgeInsets.only(bottom: 12 * scale),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                _buildHeaderCell("Leave\nDate", scale, flex: 2),
                _buildHeaderCell("Leave\nType", scale, flex: 2),
                _buildHeaderCell("Reason", scale, flex: 2),
                _buildHeaderCell("Status", scale, flex: 2),
              ],
            ),
          ),

          /// Table Rows
          Expanded(
            child: ListView.separated(
              itemCount: leaves.length,
              separatorBuilder: (context, index) => SizedBox(height: 16 * scale),
              itemBuilder: (context, index) {
                final leave = leaves[index];
                return Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    _buildDataCell(
                      DateFormat('dd/MM/yy').format(leave.fromDate),
                      scale,
                      flex: 2,
                    ),
                    _buildDataCell(leave.leaveTypeName, scale, flex: 2),
                    _buildDataCell(leave.description ?? "-", scale, flex: 2),
                    _buildStatusCell(leave.leaveRequestStatusText, scale, flex: 2),
                  ],
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildHeaderCell(String text, double scale, {int flex = 1}) {
    return Expanded(
      flex: flex,
      child: Text(
        text,
        textAlign: TextAlign.center,
        style: TextStyle(
          fontFamily: 'Inter',
          fontWeight: FontWeight.w700,
          fontSize: 14 * scale,
          color: const Color(0xFF1E293B),
        ),
      ),
    );
  }

  Widget _buildDataCell(String text, double scale, {int flex = 1}) {
    return Expanded(
      flex: flex,
      child: Text(
        text,
        textAlign: TextAlign.center,
        maxLines: 2,
        overflow: TextOverflow.ellipsis,
        style: TextStyle(
          fontFamily: 'Inter',
          fontWeight: FontWeight.w500,
          fontSize: 13 * scale,
          color: const Color(0xFF64748B),
        ),
      ),
    );
  }

  Widget _buildStatusCell(String status, double scale, {int flex = 1}) {
    Color statusColor;
    final lowerStatus = status.toLowerCase();
    if (lowerStatus.contains('approved')) {
      statusColor = const Color(0xFF22C55E);
    } else if (lowerStatus.contains('reject')) {
      statusColor = const Color(0xFFEF4444);
    } else if (lowerStatus.contains('pending')) {
      statusColor = const Color(0xFFF59E0B);
    } else {
      statusColor = const Color(0xFF64748B);
    }

    return Expanded(
      flex: flex,
      child: Text(
        status,
        textAlign: TextAlign.center,
        style: TextStyle(
          fontFamily: 'Inter',
          fontWeight: FontWeight.w600,
          fontSize: 13 * scale,
          color: statusColor,
        ),
      ),
    );
  }
}
