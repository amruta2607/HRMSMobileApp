class AttendanceStatusResponse {
  final bool success;
  final String message;
  final AttendanceStatusData? data;

  AttendanceStatusResponse({
    required this.success,
    required this.message,
    this.data,
  });

  factory AttendanceStatusResponse.fromJson(Map<String, dynamic> json) {
    return AttendanceStatusResponse(
      success: json['success'] ?? false,
      message: json['message'] ?? '',
      data: json['data'] != null
          ? AttendanceStatusData.fromJson(json['data'])
          : null,
    );
  }
}

class AttendanceStatusData {
  // isMarked / isAlreadyMarked removed — not used for punch flow.
  final String status;
  final DateTime? punchIn;
  final DateTime? punchOut;
  final double duration;
  final DateTime date;

  AttendanceStatusData({
    required this.status,
    this.punchIn,
    this.punchOut,
    required this.duration,
    required this.date,
  });

  factory AttendanceStatusData.fromJson(Map<String, dynamic> json) {
    return AttendanceStatusData(
      status: json['status'] ?? 'Unknown',
      punchIn: json['punchIn'] != null
          ? DateTime.parse(json['punchIn'])
          : null,
      punchOut: json['punchOut'] != null
          ? DateTime.parse(json['punchOut'])
          : null,
      duration: (json['duration'] ?? 0).toDouble(),
      date: DateTime.parse(json['date']),
    );
  }
}
