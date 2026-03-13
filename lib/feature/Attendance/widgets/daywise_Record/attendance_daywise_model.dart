import 'package:intl/intl.dart';

class AttendanceRowData {
  final String date;
  final String? clockIn;
  final String? clockOut;
  final Duration? workedDuration;
  final bool late;

  AttendanceRowData({
    required this.date,
    this.clockIn,
    this.clockOut,
    this.workedDuration,
    this.late = false,
  });

  factory AttendanceRowData.fromJson(Map<String, dynamic> json) {
    // API Format: 2026-01-12T16:10:00
    final rawDate = json['date'] ?? '';
    final rawIn = json['punchIn'];
    final rawOut = json['punchOut'];
    final minutes = json['workingHours'] ?? 0;

    String formattedDate = '';
    try {
      if (rawDate.isNotEmpty) {
        final dt = DateTime.parse(rawDate);
        formattedDate = DateFormat('dd/MM/yyyy').format(dt);
      }
    } catch (_) {}

    String? formattedIn;
    try {
      if (rawIn != null) {
        final dt = DateTime.parse(rawIn);
        formattedIn = DateFormat('hh:mm a').format(dt);
      }
    } catch (_) {}

    String? formattedOut;
    try {
      if (rawOut != null) {
        final dt = DateTime.parse(rawOut);
        formattedOut = DateFormat('hh:mm a').format(dt);
      }
    } catch (_) {}

    return AttendanceRowData(
      date: formattedDate,
      clockIn: formattedIn,
      clockOut: formattedOut,
      workedDuration: minutes != 0 ? Duration(minutes: minutes) : null,
      late: json['status'] == 'Late',
    );
  }
}

// class AttendanceRowData {
//   final String date;
//   final String? clockIn;
//   final String? clockOut;
//   final Duration? workedDuration;
//
//   AttendanceRowData({
//     required this.date,
//     this.clockIn,
//     this.clockOut,
//     this.workedDuration,
//   });
// }