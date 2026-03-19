import 'package:flutter/material.dart';
import '../../../../core/Theme/app_colors.dart';
import 'attendance_daywise_model.dart';

class AttendanceDayWiseRow extends StatelessWidget {
  final AttendanceRowData data;
  final double scale;

  const AttendanceDayWiseRow({
    super.key,
    required this.data,
    required this.scale,
  });

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

        _Cell(data.date, scale: scale),
        SizedBox(width: 10),

        _Cell(data.clockIn ?? '-', scale: scale, color: data.late ? Colors.deepOrange : null),
        SizedBox(width: 15),

        _Cell(data.clockOut ?? '-', scale: scale),

        _Cell(
          data.workedDuration != null
              ? _formatDuration(data.workedDuration!)
              : '-',
          scale: scale,
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
  final double scale;

  const _Cell(
      this.text, {
        required this.scale,
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
          fontFamily: 'Inter',
          fontSize: 11 * scale, // Scaled font size
          fontWeight: bold ? FontWeight.w600 : FontWeight.w500,
          height: 22.69 / 12, // Preserve Original Line Height Ratio
          letterSpacing: 0,
          color: color ?? AppColors.textGrey,
        ),
      ),
    );
  }
}
