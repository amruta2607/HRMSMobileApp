class Holiday {
  final int id;
  final String name;
  final DateTime date;
  final String? description;
  final int? tenantId;
  final bool isActive;

  Holiday({
    required this.id,
    required this.name,
    required this.date,
    this.description,
    this.tenantId,
    required this.isActive,
  });

  factory Holiday.fromJson(Map<String, dynamic> json) {
    return Holiday(
      id: json['id'],
      name: json['name'],
      date: DateTime.parse(json['date']),
      description: json['description'],
      tenantId: json['tenantId'],
      isActive: json['isActive'] ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'date': date.toIso8601String(),
      'description': description,
      'tenantId': tenantId,
      'isActive': isActive,
    };
  }
}
