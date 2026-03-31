class AlertModel {
  final int id;
  final int organisationId;
  final int userId;
  final int eventId;
  final String title;
  final String message;
  final bool isRead;
  final bool isActive;
  final String status;
  final String insertDate;
  final int insertUserId;
  final String? updateDate;
  final int? updateUserId;

  AlertModel({
    required this.id,
    required this.organisationId,
    required this.userId,
    required this.eventId,
    required this.title,
    required this.message,
    required this.isRead,
    required this.isActive,
    required this.status,
    required this.insertDate,
    required this.insertUserId,
    this.updateDate,
    this.updateUserId,
  });

  factory AlertModel.fromJson(Map<String, dynamic> json) {
    return AlertModel(
      id: json['id'] ?? 0,
      organisationId: json['organisationId'] ?? 0,
      userId: json['userId'] ?? 0,
      eventId: json['eventId'] ?? 0,
      title: json['title'] ?? '',
      message: json['message'] ?? '',
      isRead: json['isRead'] ?? false,
     isActive: json['isActive'] ?? false,
      status: json['status'] ?? '',
      insertDate: json['insertDate'] ?? '',
      insertUserId: json['insertUserId'] ?? 0,
      updateDate: json['updateDate'],
      updateUserId: json['updateUserId'],
    );
  }
}
