import 'package:flutter/material.dart';
import '../../../../core/Theme/app_colors.dart';
import 'attendance_status.dart';


class AttendanceMonthOverview extends StatefulWidget {
  final Map<int, AttendanceStatus> dayStatus;
  final DateTime today;
  final int currentMonth;
  final int currentYear;
  final void Function(int year, int month)? onMonthChanged;

  const AttendanceMonthOverview({
    super.key,
    required this.dayStatus,
    required this.today,
    required this.currentMonth,
    required this.currentYear,
    this.onMonthChanged,
  });

  @override
  State<AttendanceMonthOverview> createState() =>
      _AttendanceMonthOverviewState();
}

class _AttendanceMonthOverviewState extends State<AttendanceMonthOverview> {
  late int selectedMonth;
  late int selectedYear;

  @override
  void initState() {
    super.initState();
    selectedMonth = widget.currentMonth;
    selectedYear = widget.currentYear;
  }

  @override
  void didUpdateWidget(covariant AttendanceMonthOverview oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.currentMonth != oldWidget.currentMonth ||
        widget.currentYear != oldWidget.currentYear) {
      setState(() {
        selectedMonth = widget.currentMonth;
        selectedYear = widget.currentYear;
      });
    }
  }

  static const months = [
    'January','February','March','April','May','June',
    'July','August','September','October','November','December'
  ];

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        const designWidth = 402.0;
        final scale =
        (constraints.maxWidth / designWidth).clamp(0.85, 1.1);

        final daysInMonth =
        DateUtils.getDaysInMonth(selectedYear, selectedMonth);

        final firstDayOfMonth = DateTime(selectedYear, selectedMonth, 1);

        final int weekdayOfFirstDay = firstDayOfMonth.weekday % 7;

        final totalGridItems = weekdayOfFirstDay + daysInMonth;

        final currentYearBase = DateTime.now().year;

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
                mainAxisAlignment: MainAxisAlignment.start,
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
                      if (v == null) return;
                      // Optimistic update
                      setState(() => selectedMonth = v);
                      widget.onMonthChanged?.call(selectedYear, v);
                    },
                  ),
                  DropdownButton<int>(
                    value: selectedYear,
                    underline: const SizedBox(),
                    items: List.generate(
                      5,
                          (i) {
                        final y = (currentYearBase - 1) + i;
                        return DropdownMenuItem(
                          value: y,
                          child: Text(
                            '$y',
                            style: TextStyle(
                              fontSize: 18 * scale,
                              fontWeight: FontWeight.w700,
                            ),
                          ),
                        );
                      },
                    ),
                    onChanged: (v) {
                      if (v == null) return;
                      // Optimistic update
                      setState(() => selectedYear = v);
                      widget.onMonthChanged?.call(v, selectedMonth);
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
                  mainAxisSpacing: 12 * scale,
                  crossAxisSpacing: 12 * scale,
                ),
                itemBuilder: (context, index) {

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
        return AppColors.presentGreen.withOpacity(0.35);
      case AttendanceStatus.absent:
        return AppColors.absentOrange.withOpacity(0.35);
      case AttendanceStatus.leave:
        return Colors.red.withOpacity(0.4);
      case AttendanceStatus.holiday:
        return Colors.blue.withOpacity(0.35);
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
          fontSize: 14 * scale,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }
}
