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
    try {
      // API Format: 2026-01-12T16:10:00
      final rawDate = json['date'] ?? '';
      final rawIn = json['punchIn'];
      final rawOut = json['punchOut'];
      // workingHours can be int OR double from the API — always convert to int
      final minutes = (json['workingHours'] as num?)?.toInt() ?? 0;

      String formattedDate = '';
      try {
        if ((rawDate as String).isNotEmpty) {
          final dt = DateTime.parse(rawDate);
          formattedDate = DateFormat('dd/MM/yyyy').format(dt);
        }
      } catch (_) {}

      String? formattedIn;
      try {
        if (rawIn != null) {
          final dt = DateTime.parse(rawIn as String);
          formattedIn = DateFormat('hh:mm a').format(dt);
        }
      } catch (_) {}

      String? formattedOut;
      try {
        if (rawOut != null) {
          final dt = DateTime.parse(rawOut as String);
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
    } catch (e) {
      print('AttendanceRowData.fromJson ERROR: $e  |  json=$json');
      // Return a safe empty row instead of crashing the whole list
      return AttendanceRowData(date: '');
    }
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