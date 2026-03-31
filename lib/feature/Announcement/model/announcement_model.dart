class AnnouncementModel {
  final int id;
  final String name;
  final String message;
  final DateTime date;

  AnnouncementModel({
    required this.id,
    required this.name,
    required this.message,
    required this.date,
  });

  factory AnnouncementModel.fromJson(Map<String, dynamic> json) {
    return AnnouncementModel(
      id: json['id'],
      name: json['name'] ?? '',
      message: json['message'] ?? '',
      date: DateTime.parse(json['date']),
    );
  }
}
