class ProvidentFundModel {
  final num myShare;
  final num employerShare;
  final num totalProvidentFund;

  ProvidentFundModel({
    required this.myShare,
    required this.employerShare,
    required this.totalProvidentFund,
  });

  factory ProvidentFundModel.fromJson(Map<String, dynamic> json) {
    return ProvidentFundModel(
      myShare: json['myShare'] ?? 0,
      employerShare: json['employerShare'] ?? 0,
      totalProvidentFund: json['totalProvidentFund'] ?? 0,
    );
  }
}
