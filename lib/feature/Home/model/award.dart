class Award {
  final int awardId;
  final String awardName;
  final DateTime date;
  final String? description;
  final String? reward;
  final String? achievement;
  final int awardeeId;
  final String awardeeName;
  final String? picture;
  final int? branchId;
  final int? departmentId;

  Award({
    required this.awardId,
    required this.awardName,
    required this.date,
    this.description,
    this.reward,
    this.achievement,
    required this.awardeeId,
    required this.awardeeName,
    this.picture,
    this.branchId,
    this.departmentId,
  });
  factory Award.fromJson(Map<String, dynamic>json) => Award(
      awardId: json['awardId'] as int,
      awardName: json['awardName'] as String,
      date: DateTime.parse(json['date'] as String),
      description: json['description']as String?,
      reward: json['reward'] as String?,
      achievement: json['achievement'] as String?,
      awardeeId: json['awardeeId'] as int,
      awardeeName: json['awardeeName'] as String,
      picture: json['picture'] as String?,
      branchId: json['branchId'] as int?,
      departmentId: json['departmentId'] as int?
  );
}