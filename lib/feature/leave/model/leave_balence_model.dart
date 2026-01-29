class LeaveBalanceModel {
  final int leaveTypeId;
  final String leaveTypeName;
  final int totalBalance;
  final int usedBalance;
  final int remainingBalance;

  LeaveBalanceModel({
    required this.leaveTypeId,
    required this.leaveTypeName,
    required this.totalBalance,
    required this.usedBalance,
    required this.remainingBalance,
  });

  factory LeaveBalanceModel.fromJson(Map<String, dynamic> json) {
    return LeaveBalanceModel(
      leaveTypeId: json['leaveTypeId'] ?? 0,
      leaveTypeName: json['leaveTypeName'] ?? '',
      totalBalance: json['totalBalance'] ?? 0,
      usedBalance: json['usedBalance'] ?? 0,
      remainingBalance: json['remainingBalance'] ?? 0,
    );
  }
}