class LoginModel {
  final bool success;
  final String message;
  final String token;
  final String tokenExpiry;
  final int userId;
  final String username;
  final int organisationId;
  final Map<String, dynamic>? moduleAccess;

  LoginModel({
    required this.success,
    required this.message,
    required this.token,
    required this.tokenExpiry,
    required this.userId,
    required this.username,
    required this.organisationId,
    this.moduleAccess,
  });

  factory LoginModel.fromJson(Map<String, dynamic> json) {
    return LoginModel(
      success: json['success'] == true,
      message: json['message'] ?? '',
      token: json['token'] ?? '',
      tokenExpiry: json['tokenExpiry'] ?? '',
      userId: json['userId'] ?? 0,
      username: json['username'] ?? '',
      organisationId: json['organisationId'] ?? 0,
      moduleAccess: json['moduleAccess'],
    );
  }
}
