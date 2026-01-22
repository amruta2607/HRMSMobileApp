class ProfileModel {
  final String empId;
  final String name;
  final String? picture;
  final String phone;
  final String email;
  final String designation;
  final String? address;
  final String? reportingManager;

  ProfileModel({
    required this.empId,
    required this.name,
    this.picture,
    required this.phone,
    required this.email,
    required this.designation,
    this.address,
    this.reportingManager,
  });

  factory ProfileModel.fromJson(Map<String, dynamic> json) {
    return ProfileModel(
      empId: json['empId'] ?? '',
      name: json['name'] ?? '',
      picture: json['picture'] ?? json['userPhoto'] ?? json['photo'] ?? json['profilePhoto'],
      phone: json['phone'] ?? '',
      email: json['email'] ?? '',
      designation: json['designation'] ?? '',
      address: json['address'],
      reportingManager: json['reportingManager'],
    );
  }
}
