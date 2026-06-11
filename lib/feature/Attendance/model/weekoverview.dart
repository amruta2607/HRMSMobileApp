class WeekOverview {
  final String week;
  final num expectedHours;
  final num actualHours;
  final num shortfallHours;

  WeekOverview({
    required this.week,
    required this.expectedHours,
    required this.actualHours,
    required this.shortfallHours,
  });

  factory WeekOverview.fromJson(Map<String, dynamic> json) {
    return WeekOverview(
      week: json['week'] ?? '',
      expectedHours: json['expectedHours'] ?? 0,
      actualHours: json['actualHours'] ?? 0,
      shortfallHours: json['shortfallHours'] ?? 0,
    );
  }
}
