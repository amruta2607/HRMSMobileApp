class LeaveHistoryItem {
  final int leaveRequestId;
  final String leaveDates;
  final String leaveType;
  final String reason;
  final int usedDays;
  final String status;

  LeaveHistoryItem({
    required this.leaveRequestId,
    required this.leaveDates,
    required this.leaveType,
    required this.reason,
    required this.usedDays,
    required this.status,
  });

  factory LeaveHistoryItem.fromJson(Map<String, dynamic> json) {
    return LeaveHistoryItem(
      leaveRequestId: json['leaveRequestId'] ?? 0,
      leaveDates: json['leaveDates'] ?? '',
      leaveType: json['leaveType'] ?? '',
      reason: json['reason'] ?? '',
      usedDays: json['usedDays'] ?? 0,
      status: json['status'] ?? '',
    );
  }
}

class LeaveHistoryModel {
  final bool success;
  final String message;
  final int employeeId;
  final int availableLeaves;
  final int usedLeaves;
  final int year;
  final List<LeaveHistoryItem> leaveHistory;

  LeaveHistoryModel({
    required this.success,
    required this.message,
    required this.employeeId,
    required this.availableLeaves,
    required this.usedLeaves,
    required this.year,
    required this.leaveHistory,
  });

  factory LeaveHistoryModel.fromJson(Map<String, dynamic> json) {
    var list = json['leaveHistory'] as List? ?? [];
    List<LeaveHistoryItem> historyList = list.map((i) => LeaveHistoryItem.fromJson(i)).toList();

    return LeaveHistoryModel(
      success: json['success'] ?? false,
      message: json['message'] ?? '',
      employeeId: json['employeeId'] ?? 0,
      availableLeaves: json['availableLeaves'] ?? 0,
      usedLeaves: json['usedLeaves'] ?? 0,
      year: json['year'] ?? DateTime.now().year,
      leaveHistory: historyList,
    );
  }
}
