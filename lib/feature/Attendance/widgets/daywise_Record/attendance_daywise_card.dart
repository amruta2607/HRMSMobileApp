import 'package:flutter/material.dart';
import '../../../../core/Theme/app_colors.dart';

import 'attendance_daywise_model.dart';
import 'attendance_daywise_row.dart';

class AttendanceDayWiseCard extends StatelessWidget {
  final List<AttendanceRowData> rows;
  final ValueChanged<AttendanceRowData> onRowTap;

  const AttendanceDayWiseCard({
    super.key,
    required this.rows,
    required this.onRowTap,
  });

  @override
  Widget build(BuildContext context) {
    final scale =
    (MediaQuery.of(context).size.width / 402).clamp(0.75, 1.1);

    return Container(
      height: 361 * scale,
      padding: EdgeInsets.fromLTRB(
        22 * scale,
        14 * scale,
        14 * scale,
        14 * scale,
      ),
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
          Row(
            children: [
              const SizedBox(width: 5),

              _HeaderCell('Date:', scale: scale),
             SizedBox(width: 10),

            _HeaderCell('ClockIn', scale: scale),
              SizedBox(width: 15),
          _HeaderCell('ClockOut', scale: scale),


               _HeaderCell('Hours', alignRight: true, scale: scale),
              const SizedBox(width: 5),

            ],
          ),
          SizedBox(height: 6 * scale),

          Expanded(
            child: ListView.separated(
              physics: const ClampingScrollPhysics(),
              itemCount: rows.length,
              separatorBuilder: (_, __) =>
                  Divider(color: AppColors.grey96),
              itemBuilder: (_, i) {
                final row = rows[i];
                return InkWell(
                  onTap: () => onRowTap(row),
                  child: AttendanceDayWiseRow(data: row, scale: scale),
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}

class _HeaderCell extends StatelessWidget {
  final String text;
  final bool alignRight;
  final double scale;

  const _HeaderCell(this.text, {required this.scale, this.alignRight = false});

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: Text(
        text,
        textAlign: alignRight ? TextAlign.right : TextAlign.left,
        style: TextStyle(
          fontWeight: FontWeight.w700,
          fontSize: 13 * scale, // Reduced base slightly to 13 for better fit + scale
          color: AppColors.textDark,
        ),
      ),
    );
  }
}