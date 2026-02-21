class PaySlipModel {
  final int id;
  final int payrollId;
  final int payrollMonth;
  final int payrollYear;
  final String? payrollMonthName;
  final double gross;
  final double totalIncome;
  final double totalDeduction;
  final double takeHomePay;
  final String currency;

  PaySlipModel({
    required this.id,
    required this.payrollId,
    required this.payrollMonth,
    required this.payrollYear,
    this.payrollMonthName,
    required this.gross,
    required this.totalIncome,
    required this.totalDeduction,
    required this.takeHomePay,
    required this.currency,
  });

  factory PaySlipModel.fromJson(Map<String, dynamic> json) {
    return PaySlipModel(
      id: json['id'] ?? 0,
      payrollId: json['payrollId'] ?? 0,
      payrollMonth: json['payrollMonth'] ?? 0,
      payrollYear: json['payrollYear'] ?? 0,
      payrollMonthName: json['payrollMonthName'],
      gross: (json['gross'] ?? 0).toDouble(),
      totalIncome: (json['totalIncome'] ?? 0).toDouble(),
      totalDeduction: (json['totalDeduction'] ?? 0).toDouble(),
      takeHomePay: (json['takeHomePay'] ?? 0).toDouble(),
      currency: json['currency'] ?? 'INR',
    );
  }
}
