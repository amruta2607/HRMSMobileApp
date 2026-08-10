class LeaveBalanceModel {
  final int leaveTypeId;
  final String leaveTypeName;
  final double totalBalance;
  final double usedBalance;
  final double remainingBalance;

  LeaveBalanceModel({
    required this.leaveTypeId,
    required this.leaveTypeName,
    required this.totalBalance,
    required this.usedBalance,
    required this.remainingBalance,
  });

  factory LeaveBalanceModel.fromJson(Map<String, dynamic> json) {
    final total = (json['totalBalance'] as num?)?.toDouble() ?? 0.0;
    final remaining = (json['remainingBalance'] as num?)?.toDouble() ?? 0.0;
    final used = (json['usedBalance'] as num?)?.toDouble() ?? (total - remaining);

    return LeaveBalanceModel(
      leaveTypeId: (json['leaveTypeId'] as num?)?.toInt() ?? 0,
      leaveTypeName: json['leaveTypeName']?.toString() ?? '',
      totalBalance: total,
      usedBalance: used,
      remainingBalance: remaining,
    );
  }

  String get remainingBalanceFormatted =>
      (remainingBalance % 1 == 0) ? remainingBalance.toInt().toString() : remainingBalance.toString();

  String get totalBalanceFormatted =>
      (totalBalance % 1 == 0) ? totalBalance.toInt().toString() : totalBalance.toString();

  String get usedBalanceFormatted =>
      (usedBalance % 1 == 0) ? usedBalance.toInt().toString() : usedBalance.toString();
}

