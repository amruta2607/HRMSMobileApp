// import 'package:flutter/material.dart';
// import '../../../../core/Theme/app_colors.dart';
//
// enum DayStatus { present, absent, leave, holiday }
//
// class AttendanceCalendar extends StatelessWidget {
//   final int month;
//   final int year;
//   final DateTime today;
//   final Map<DateTime, DayStatus> dayStatus;
//
//   const AttendanceCalendar({
//     super.key,
//     required this.month,
//     required this.year,
//     required this.today,
//     required this.dayStatus,
//   });
//
//   @override
//   Widget build(BuildContext context) {
//     return Container(
//       padding: const EdgeInsets.all(16),
//       decoration: BoxDecoration(
//         color: Colors.white,
//         borderRadius: BorderRadius.circular(24),
//         boxShadow: [
//           BoxShadow(
//             color: Colors.black.withOpacity(0.05),
//             blurRadius: 12,
//           ),
//         ],
//       ),
//       child: Column(
//         children: [
//           /// MONTH + YEAR
//           Text(
//             'December $year',
//             style: const TextStyle(
//               fontSize: 18,
//               fontWeight: FontWeight.w600,
//             ),
//           ),
//
//           const SizedBox(height: 12),
//
//           /// DAYS GRID
//           GridView.builder(
//             shrinkWrap: true,
//             physics: const NeverScrollableScrollPhysics(),
//             gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
//               crossAxisCount: 7,
//               mainAxisSpacing: 10,
//               crossAxisSpacing: 10,
//             ),
//             itemCount: DateUtils.getDaysInMonth(year, month),
//             itemBuilder: (context, index) {
//               final day = index + 1;
//               final date = DateTime(year, month, day);
//               final status = dayStatus[date];
//
//               return _DayCell(
//                 day: day,
//                 isToday: DateUtils.isSameDay(date, today),
//                 status: status,
//               );
//             },
//           ),
//         ],
//       ),
//     );
//   }
// }
//
// class _DayCell extends StatelessWidget {
//   final int day;
//   final bool isToday;
//   final DayStatus? status;
//
//   const _DayCell({
//     required this.day,
//     required this.isToday,
//     this.status,
//   });
//
//   Color get bgColor {
//     switch (status) {
//       case DayStatus.present:
//         return AppColors.presentGreen.withOpacity(0.25);
//       case DayStatus.absent:
//         return AppColors.absentOrange.withOpacity(0.35);
//       case DayStatus.leave:
//         return Colors.red.withOpacity(0.35);
//       case DayStatus.holiday:
//         return Colors.blue.withOpacity(0.25);
//       default:
//         return Colors.transparent;
//     }
//   }
//
//   @override
//   Widget build(BuildContext context) {
//     return Container(
//       decoration: BoxDecoration(
//         color: bgColor,
//         shape: BoxShape.circle,
//         border: isToday
//             ? Border.all(
//           color: AppColors.primaryBlue,
//           width: 3,
//         )
//             : null,
//       ),
//       alignment: Alignment.center,
//       child: Text(
//         '$day',
//         style: const TextStyle(
//           fontWeight: FontWeight.w600,
//         ),
//       ),
//     );
//   }
// }