class Birthday {
  final int employeeId;
  final String employeeName;
  final DateTime dateOfBrith;
  final DateTime birthdayDate;
  final String? picture;
  final int? designationId;
  final int? departmentId;

  Birthday({
  required this.employeeId,
  required this.employeeName,
  required this.dateOfBrith,
  required this.birthdayDate,
  this.picture,
  this.designationId,
  this.departmentId
 });

  factory Birthday.fromJson(Map<String, dynamic> json) => Birthday(
      employeeId: json['employeeId'] as int,
      employeeName: json['employeeName'] as String,
      dateOfBrith: DateTime.parse(json['dateOfBirth'] as String),
      birthdayDate: DateTime.parse(json['birthdayDate'] as String),
      picture: json['picture'] as String?,
      designationId: json['designationId'] as int?,
      departmentId: json ['departmentId'] as int?,
  );
}