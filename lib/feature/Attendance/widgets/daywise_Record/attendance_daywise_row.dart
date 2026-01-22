import 'package:flutter/material.dart';
import '../../../../core/Theme/app_colors.dart';
import 'attendance_daywise_model.dart';

class AttendanceDayWiseRow extends StatelessWidget {
  final AttendanceRowData data;

  const AttendanceDayWiseRow({super.key, required this.data});

  String _formatDuration(Duration d) {
    final h = d.inHours.toString().padLeft(2, '0');
    final m = d.inMinutes.remainder(60).toString().padLeft(2, '0');
    final s = d.inSeconds.remainder(60).toString().padLeft(2, '0');
    return '$h:$m:$s';
  }

  Color get hourColor {
    if (data.workedDuration == null) return AppColors.textGrey;
    if (data.workedDuration!.inHours < 4) return Colors.deepOrange;
    return AppColors.presentGreen;
  }

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        SizedBox(width: 5),

        _Cell(data.date),
        SizedBox(width: 10),

        _Cell(data.clockIn ?? '-', color: data.late ? Colors.deepOrange : null),
        SizedBox(width: 15),

        _Cell(data.clockOut ?? '-'),

        _Cell(
          data.workedDuration != null
              ? _formatDuration(data.workedDuration!)
              : '-',
          alignRight: true,
          bold: true,
          color: hourColor,
        ),
        SizedBox(width: 5),

      ],
    );
  }
}

class _Cell extends StatelessWidget {
  final String text;
  final bool alignRight;
  final bool bold;
  final Color? color;

  const _Cell(
      this.text, {
        this.alignRight = false,
        this.bold = false,
        this.color,
      });

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: Text(
        text,
        textAlign: alignRight ? TextAlign.right : TextAlign.left,
        style: TextStyle(
          fontSize: 11,
          fontWeight: bold ? FontWeight.w600 : FontWeight.w500,
          color: color ?? AppColors.textGrey,
        ),
      ),
    );
  }
}
