class MonthlySummaryModel {
  final double basicSalary;
  final double gross;
  final double totalIncome;
  final double totalDeduction;
  final double takeHomePay;
  final List<SummaryItem> incomes;
  final List<SummaryItem> deductions;

  MonthlySummaryModel({
    required this.basicSalary,
    required this.gross,
    required this.totalIncome,
    required this.totalDeduction,
    required this.takeHomePay,
    required this.incomes,
    required this.deductions,
  });

  factory MonthlySummaryModel.fromJson(Map<String, dynamic> json) {
    return MonthlySummaryModel(
      basicSalary: (json['basicSalary'] as num?)?.toDouble() ?? 0.0,
      gross: (json['gross'] as num?)?.toDouble() ?? 0.0,
      totalIncome: (json['totalIncome'] as num?)?.toDouble() ?? 0.0,
      totalDeduction: (json['totalDeduction'] as num?)?.toDouble() ?? 0.0,
      takeHomePay: (json['takeHomePay'] as num?)?.toDouble() ?? 0.0,
      incomes: (json['incomes'] as List<dynamic>?)
          ?.map((e) => SummaryItem.fromJson(e))
          .toList() ??
          [],
      deductions: (json['deductions'] as List<dynamic>?)
          ?.map((e) => SummaryItem.fromJson(e))
          .toList() ??
          [],
    );
  }
}

class SummaryItem {
  final String name;
  final double amount;
  final String? deductionCode;

  SummaryItem({
    required this.name,
    required this.amount,
    this.deductionCode,
  });

  factory SummaryItem.fromJson(Map<String, dynamic> json) {
    return SummaryItem(
      name: json['name'] ?? '',
      amount: (json['amount'] as num?)?.toDouble() ?? 0.0,
      deductionCode: json['deductionCode'],
    );
  }
}
class LastMonthPayrollModel {
  final MonthlySummaryModel data;
  final int payrollMonth;
  final int payrollYear;

  LastMonthPayrollModel({
    required this.data,
    required this.payrollMonth,
    required this.payrollYear,
  });

  factory LastMonthPayrollModel.fromJson(Map<String, dynamic> json) {
    return LastMonthPayrollModel(
      data: MonthlySummaryModel.fromJson(json['data'] ?? {}),
      payrollMonth: json['payrollMonth'] ?? 0,
      payrollYear: json['payrollYear'] ?? 0,
    );
  }
}
