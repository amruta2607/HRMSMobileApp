class GeofencingModel {
  final bool isEnabled;
  final int branchId;
  final String branchName;
  final double latitude;
  final double longitude;
  final double radius;
  final int organisationId;

  GeofencingModel({
    required this.isEnabled,
    required this.branchId,
    required this.branchName,
    required this.latitude,
    required this.longitude,
    required this.radius,
    required this.organisationId,
  });

  factory GeofencingModel.fromJson(Map<String, dynamic> json) {
    return GeofencingModel(
      isEnabled: json['isEnabled'] ?? false,
      branchId: json['branchId'] ?? 0,
      branchName: json['branchName'] ?? '',
      latitude: double.tryParse(json['latitude'].toString()) ?? 0.0,
      longitude: double.tryParse(json['longitude'].toString()) ?? 0.0,
      radius: double.tryParse(json['radius'].toString()) ?? 0.0,
      organisationId: json['organisationId'] ?? 0,
    );
  }
}
