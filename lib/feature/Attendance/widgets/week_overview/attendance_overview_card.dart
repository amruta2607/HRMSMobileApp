import 'package:flutter/material.dart';
import '../../../../core/Theme/app_colors.dart';
import '../../../../core/Utils/services/Attendance service/attendance_service.dart';
import '../../../../feature/Attendance/model/weekoverview.dart';

class AttendanceOverviewCard extends StatefulWidget {
  const AttendanceOverviewCard({super.key});

  @override
  State<AttendanceOverviewCard> createState() =>
      _AttendanceOverviewCardState();
}

class _AttendanceOverviewCardState extends State<AttendanceOverviewCard> {
  late Future<WeekOverview?> _future;

  @override
  void initState() {
    super.initState();
    _future = AttendanceService.getCurrentWeekOverview();
  }

  @override
  Widget build(BuildContext context) {
    final scale =
    (MediaQuery.of(context).size.width / 402).clamp(0.85, 1.1);

    return FutureBuilder<WeekOverview?>(
      future: _future,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return _buildCard(
            scale,
            week: 'This Week',
            expected: '...',
            actual: '...',
            shortfall: '...',
          );
        }

        if (snapshot.hasError || snapshot.data == null) {
          return GestureDetector(
            onTap: () {
              setState(() {
                _future = AttendanceService.getCurrentWeekOverview();
              });
            },
            child: _buildCard(
              scale,
              week: 'Tap to Retry',
              expected: '--',
              actual: '--',
              shortfall: '${snapshot.error ?? "No Data"}',
            ),
          );
        }

        final data = snapshot.data!;
        return _buildCard(
          scale,
          week: data.week,
          expected: '${data.expectedHours}:00',
          actual: '${data.actualHours}:00',
          shortfall: '${data.shortfallHours}:00',
        );
      },
    );
  }

  Widget _buildCard(
      double scale, {
        required String week,
        required String expected,
        required String actual,
        required String shortfall,
      }) {
    return Container(
      width: double.infinity,
      padding: EdgeInsets.all(18.45 * scale),
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
          // Week row (reduced gap)
          _Row('Week:', week, scale, labelWidth: 70),

          // Hours rows
          _Row(
            'Expected Hours:',
            expected,
            scale,
            suffix: ' Hours',
          ),
          _Row(
            'Actual Hours:',
            actual,
            scale,
            suffix: ' Hours',
          ),
          _Row(
            'Shortfall:',
            shortfall,
            scale,
            suffix: ' Hours',
          ),
        ],
      ),
    );
  }
}

class _Row extends StatelessWidget {
  final String label;
  final String value;
  final double scale;
  final double? labelWidth;
  final String? suffix;

  const _Row(
      this.label,
      this.value,
      this.scale, {
        this.labelWidth,
        this.suffix,
      });

  @override
  Widget build(BuildContext context) {
    final textStyle = TextStyle(
      fontFamily: 'Inter',
      fontSize: 15.89 * scale,
      fontWeight: FontWeight.w400,
      height: 22.69 / 15.89, // ✅ exact Figma line-height
      letterSpacing: 0,
      color: AppColors.textDark,
    );

    return Padding(
      padding: EdgeInsets.symmetric(vertical: 3 * scale),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          SizedBox(
            width: (labelWidth ?? 135) * scale,
            child: Text(
              label,
              style: textStyle.copyWith(fontWeight: FontWeight.w900),
            ),
          ),
          Expanded(
            child: Text(
              '$value${suffix ?? ''}',
              style: textStyle,
            ),
          ),
        ],
      ),
    );
  }
}
