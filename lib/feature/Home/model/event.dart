class Event {
  final int id;
  final String name;
  final String? description;
  final DateTime startDate;
  final DateTime endDate;
  final String? location;

  Event({
    required this.id,
    required this.name,
    required this.startDate,
    required this.endDate,
    this.description,
    this.location,
 });

  factory Event.fromJson(Map<String, dynamic> json) => Event(
      id: json['id'] as int,
      name: json['name'] as String,
      description: json['description'] as String?,
      startDate: DateTime.parse(json['startDate'] as String),
      endDate:  DateTime.parse(json['endDate'] as String),
      location: json['location'] as String?,
  );
}