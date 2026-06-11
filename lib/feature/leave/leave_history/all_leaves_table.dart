import 'package:flutter/material.dart';
import '../model/leave_history_model.dart';

class AllLeavesTable extends StatelessWidget {
  final List<LeaveHistoryItem> leaves;

  const AllLeavesTable({
    super.key,
    required this.leaves,
  });

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    // ── Fixed column widths ───────────────────────────────────────────
    const double dateW   = 155; // enough for "18-03-2026 - 20-03-2026"
    const double typeW   = 110;
    const double reasonW = 130;
    const double statusW = 90;

    return Container(
      height: 400 * scale, // Fixed height for vertical scrolling
      margin: EdgeInsets.symmetric(horizontal: 20 * scale),
      padding: EdgeInsets.all(16 * scale),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(23 * scale),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.08),
            blurRadius: 14 * scale,
            offset: Offset(0, 4.6 * scale),
          ),
        ],
      ),
      child: SingleChildScrollView(
        scrollDirection: Axis.horizontal,
        child: SizedBox(
          width: dateW + typeW + reasonW + statusW + 8, // +8 for horizontal padding
          child: Column(
            children: [
              // ── Header ────────────────────────────────────────────────
              Container(
                padding: const EdgeInsets.symmetric(vertical: 10, horizontal: 4),
                decoration: BoxDecoration(
                  color: const Color(0xFFF1F5F9),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Row(
                  children: [
                    _headerCell("Leave Date",  dateW),
                    _headerCell("Leave Type",  typeW),
                    _headerCell("Reason",      reasonW),
                    _headerCell("Status",      statusW),
                  ],
                ),
              ),

              const SizedBox(height: 4),

              // ── Rows (Vertical Scrollable) ────────────────────────────
              Expanded(
                child: ListView.separated(
                  itemCount: leaves.length,
                  separatorBuilder: (_, __) => const Divider(
                    height: 1,
                    color: Color(0xFFE2E8F0),
                  ),
                  itemBuilder: (context, index) {
                    final leave = leaves[index];
                    return Padding(
                      padding: const EdgeInsets.symmetric(
                          vertical: 10, horizontal: 4),
                      child: Row(
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: [
                          // Date — full string, no wrap, single line
                          _dataCell(leave.leaveDates, dateW, scale),
                          _dataCell(leave.leaveType,  typeW, scale),
                          _dataCell(
                            leave.reason.isEmpty ? "—" : leave.reason,
                            reasonW,
                            scale,
                          ),
                          _statusCell(leave.status, statusW, scale),
                        ],
                      ),
                    );
                  },
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  // ── Widgets ───────────────────────────────────────────────────────────────

  Widget _headerCell(String text, double width) => SizedBox(
    width: width,
    child: Text(
      text,
      textAlign: TextAlign.center,
      style: const TextStyle(
        fontFamily: 'Inter',
        fontWeight: FontWeight.w700,
        fontSize: 12,
        color: Color(0xFF1E293B),
      ),
    ),
  );

  Widget _dataCell(String text, double width, double scale) => SizedBox(
    width: width,
    child: Text(
      text,
      textAlign: TextAlign.center,
      softWrap: false,          // ← single line always
      overflow: TextOverflow.visible,
      style: TextStyle(
        fontFamily: 'Inter',
        fontWeight: FontWeight.w500,
        fontSize: 12 * scale,
        color: const Color(0xFF64748B),
      ),
    ),
  );

  Widget _statusCell(String status, double width, double scale) {
    Color bg, fg;
    final s = status.toLowerCase();
    if (s.contains('approved')) {
      bg = const Color(0xFFDCFCE7); fg = const Color(0xFF16A34A);
    } else if (s.contains('reject')) {
      bg = const Color(0xFFFFE4E4); fg = const Color(0xFFC62828);
    } else if (s.contains('pending')) {
      bg = const Color(0xFFFEF3C7); fg = const Color(0xFFD97706);
    } else {
      bg = const Color(0xFFF1F5F9); fg = const Color(0xFF64748B);
    }

    return SizedBox(
      width: width,
      child: Center(
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
          decoration: BoxDecoration(
            color: bg,
            borderRadius: BorderRadius.circular(20),
          ),
          child: Text(
            status,
            textAlign: TextAlign.center,
            style: TextStyle(
              fontFamily: 'Inter',
              fontWeight: FontWeight.w600,
              fontSize: 11 * scale,
              color: fg,
            ),
          ),
        ),
      ),
    );
  }
}