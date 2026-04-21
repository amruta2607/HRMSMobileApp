import 'package:flutter/material.dart';
import '../../../../core/Theme/app_colors.dart';
import 'attendance_status.dart';


class AttendanceMonthOverview extends StatefulWidget {
  final Map<int, AttendanceStatus> dayStatus;
  final DateTime today;
  final void Function(int year, int month)? onMonthChanged;

  const AttendanceMonthOverview({
    super.key,
    required this.dayStatus,
    required this.today,
    this.onMonthChanged,
  });

  @override
  State<AttendanceMonthOverview> createState() =>
      _AttendanceMonthOverviewState();
}

class _AttendanceMonthOverviewState extends State<AttendanceMonthOverview> {
  int selectedMonth = DateTime.now().month;
  int selectedYear = DateTime.now().year;

  static const months = [
    'January','February','March','April','May','June',
    'July','August','September','October','November','December'
  ];

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        /// Responsive scale
        const designWidth = 402.0;
        final scale =
        (constraints.maxWidth / designWidth).clamp(0.85, 1.1);

        final daysInMonth =
        DateUtils.getDaysInMonth(selectedYear, selectedMonth);

        /// First weekday of selected month
        final firstDayOfMonth = DateTime(selectedYear, selectedMonth, 1);

        /// Convert Monday-start to Sunday-start (0 = Sunday)
        final int weekdayOfFirstDay = firstDayOfMonth.weekday % 7;

        /// Total grid items needed
        final totalGridItems = weekdayOfFirstDay + daysInMonth;

        return Container(
          width: double.infinity,
          padding: EdgeInsets.fromLTRB(
            18.45 * scale,
            18.45 * scale,
            18.45 * scale,
            13.84 * scale,
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
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  DropdownButton<int>(
                    value: selectedMonth,
                    underline: const SizedBox(),
                    items: List.generate(
                      12,
                          (i) => DropdownMenuItem(
                        value: i + 1,
                        child: Text(
                          months[i],
                          style: TextStyle(
                            fontSize: 18 * scale,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                      ),
                    ),
                    onChanged: (v) {
                      setState(() => selectedMonth = v!);
                      widget.onMonthChanged?.call(selectedYear, selectedMonth);
                    },
                  ),
                  DropdownButton<int>(
                    value: selectedYear,
                    underline: const SizedBox(),
                    items: List.generate(
                      5,
                          (i) => DropdownMenuItem(
                        value: 2023 + i,
                        child: Text(
                          '${2023 + i}',
                          style: TextStyle(
                            fontSize: 18 * scale,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                      ),
                    ),
                    onChanged: (v) {
                      setState(() => selectedYear = v!);
                      widget.onMonthChanged?.call(selectedYear, selectedMonth);
                    },
                  ),
                ],
              ),

              SizedBox(height: 14 * scale),

              /// Week Days
              Row(
                children: const ['Sun','Mon','Tue','Wed','Thu','Fri','Sat']
                    .map(
                      (d) => Expanded(
                    child: Center(
                      child: Text(
                        d,
                        style: TextStyle(
                          color: AppColors.textGrey,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                    ),
                  ),
                )
                    .toList(),
              ),

              SizedBox(height: 12 * scale),


              GridView.builder(
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                itemCount: totalGridItems,
                gridDelegate:
                SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: 7,
                  mainAxisSpacing: 1 * scale,
                  crossAxisSpacing: 1 * scale,
                ),
                itemBuilder: (context, index) {
                  /// Empty cells before day 1
                  if (index < weekdayOfFirstDay) {
                    return const SizedBox.shrink();
                  }

                  final day = index - weekdayOfFirstDay + 1;
                  final status = widget.dayStatus[day];

                  final isToday =
                          widget.today.day == day &&
                          widget.today.month == selectedMonth &&
                          widget.today.year == selectedYear;

                  return _DayCell(
                    day: day,
                    status: status,
                    isToday: isToday,
                    scale: scale,
                  );
                },
              ),
            ],
          ),
        );
      },
    );
  }
}


class _DayCell extends StatelessWidget {
  final int day;
  final AttendanceStatus? status;
  final bool isToday;
  final double scale;

  const _DayCell({
    required this.day,
    this.status,
    required this.isToday,
    required this.scale,
  });

  Color get bgColor {
    switch (status) {
      case AttendanceStatus.present:
        return AppColors.presentColorx;
      case AttendanceStatus.absent:
        return AppColors.absentOrange;
      case AttendanceStatus.leave:
        return Color(0xFFF67373);
      case AttendanceStatus.holiday:
        return Colors.blue.withOpacity(0.71);
      default:
        return Colors.transparent;
    }
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      alignment: Alignment.center,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        color: bgColor,
        border: isToday
            ? Border.all(
          color: AppColors.primaryBlue,
          width: 2.5 * scale,
        )
            : null,
      ),
      child: Text(
        '$day',
        style: TextStyle(
          fontFamily: 'Roboto',
          fontSize: 17 * scale,
          fontWeight: FontWeight.w400,
        ),
      ),
    );
  }
}
