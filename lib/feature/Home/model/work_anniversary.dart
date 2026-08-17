class WorkAnniversary {
  final int employeeId;
  final String employeeName;
  final DateTime dateOfJoining;
  final DateTime anniversaryDate;
  final int yearsCompleted;
  final String? picture;
  final int? designationId;
  final int? departmentId;

  WorkAnniversary({
    required this.employeeId,
    required this.employeeName,
    required this.dateOfJoining,
    required this.anniversaryDate,
    required this.yearsCompleted,
    this.picture,
    this.designationId,
    this.departmentId
 });
  
  factory WorkAnniversary.fromJson(Map<String, dynamic>json) => WorkAnniversary(
      employeeId: json['employeeId'] as int,
      employeeName: json['employeeName'] as String,
      dateOfJoining: DateTime.parse(json['dateOfJoining'] as String),
      anniversaryDate: DateTime.parse(json['anniversaryDate'] as String),
      yearsCompleted: json['yearsCompleted'] as int,
      picture: json['picture'] as String?,
      designationId: json['designationId'] as int?,
      departmentId: json['departmentId'] as int?,
  );
}