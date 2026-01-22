class DisputeCategory {
  final int id;
  final String categoryName;
  final bool isActive;

  DisputeCategory({
    required this.id,
    required this.categoryName,
    required this.isActive,
  });

  factory DisputeCategory.fromJson(Map<String, dynamic> json) {
    return DisputeCategory(
      id: json['id'] ?? 0,
      categoryName: json['categoryName'] ?? '',
      isActive: json['isActive'] ?? false,
    );
  }
}
