class Attendance_in_out_Model {
  final int userId;
  final DateTime attendanceDate;
  final DateTime? punchInTime;
  final DateTime? punchOutTime;
  final double latitude;
  final double longitude;

  Attendance_in_out_Model({
    required this.userId,
    required this.attendanceDate,
    this.punchInTime,
    this.punchOutTime,
    required this.latitude,
    required this.longitude,
  });

  String _onlyDate(DateTime d) {
    return "${d.year.toString().padLeft(4, '0')}-"
        "${d.month.toString().padLeft(2, '0')}-"
        "${d.day.toString().padLeft(2, '0')}";
  }

  Map<String, dynamic> toPunchInJson() => {
    "userId": userId,
    "punch_in_time": punchInTime!.toUtc().toIso8601String(),
    "attendance_date": _onlyDate(attendanceDate),
    "latitude": latitude,
    "longitude": longitude,
  };

  Map<String, dynamic> toPunchOutJson() => {
    "userId": userId,
    "punch_out_time": punchOutTime!.toUtc().toIso8601String(),
    "attendance_date": _onlyDate(attendanceDate),
    "latitude": latitude,
    "longitude": longitude,
  };
}
