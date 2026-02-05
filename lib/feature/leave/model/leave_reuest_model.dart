class LeaveRequestModel {
  final int id;
  final String number;
  final int employeeId;
  final String employeeName;
  final int leaveTypeId;
  final String leaveTypeName;
  final dynamic leaveBalance;
  final DateTime fromDate;
  final DateTime toDate;
  final int duration;
  final String? description;
  final String leaveRequestStatusText;
  final String currentAction;
  final DateTime insertDate;

  LeaveRequestModel({
    required this.id,
    required this.number,
    required this.employeeId,
    required this.employeeName,
    required this.leaveTypeId,
    required this.leaveTypeName,
    this.leaveBalance,
    required this.fromDate,
    required this.toDate,
    required this.duration,
    this.description,
    required this.leaveRequestStatusText,

    required this.currentAction,
    required this.insertDate,
  });

  factory LeaveRequestModel.fromJson(Map<String, dynamic> json) {
    return LeaveRequestModel(
      id: json['id'] ?? 0,
      number: json['number'] ?? '',
      employeeId: json['employeeId'] ?? 0,
      employeeName: json['employeeName'] ?? '',
      leaveTypeId: json['leaveTypeId'] ?? 0,
      leaveTypeName: json['leaveTypeName'] ?? '',
      leaveBalance: json['leaveBalance'],
      fromDate: DateTime.tryParse(json['fromDate'] ?? '') ?? DateTime.now(),
      toDate: DateTime.tryParse(json['toDate'] ?? '') ?? DateTime.now(),
      duration: json['duration'] ?? 0,
      description: json['description'],
      leaveRequestStatusText: json['leaveRequestStatusText'] ?? '',
      currentAction: json['currentAction'] ?? '',
      insertDate: DateTime.tryParse(json['insertDate'] ?? '') ?? DateTime.now(),
    );
  }
}
