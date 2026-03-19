import '../../../core/Utils/services/LogIn_out/auth_service.dart';
import '../../../core/Utils/services/token_storage.dart';
import '../Model/login_model.dart';

class LoginController {
  final AuthService _auth = AuthService();

  // ================= EMAIL LOGIN =================
  Future<void> loginWithEmail({
    required String email,
    required String password,
  }) async {
    final response = await _auth.loginWithEmail(
      email: email,
      password: password,
    );

    print("CONTROLLER EMAIL RESPONSE → $response");

    final model = LoginModel.fromJson(response);

    // LOGIN FAILED
    if (!model.success) {
      throw Exception(model.message);
    }

    await TokenStorage.saveLoginData(
      token: model.token,
      tokenExpiry: model.tokenExpiry,
      userId: model.userId,
      username: model.username,
      organisationId: model.organisationId,
    );

    print(
      "LOGIN SUCCESS (EMAIL) → userId=${model.userId}, orgId=${model.organisationId}",
    );
  }

  // ================= MOBILE LOGIN - STEP 1: SEND OTP =================
  /// Returns resendAfterSeconds from the response
  Future<int> sendOtp({required String mobile}) async {
    final response = await _auth.sendMobileOtp(mobileNumber: mobile);

    print("CONTROLLER SEND OTP RESPONSE → $response");

    if (response['success'] != true) {
      throw Exception(response['message'] ?? 'Failed to send OTP');
    }

    return (response['resendAfterSeconds'] as int?) ?? 30;
  }

  // ================= MOBILE LOGIN - STEP 2: VERIFY OTP =================
  Future<void> verifyOtp({
    required String mobile,
    required String otp,
  }) async {
    final response = await _auth.verifyMobileOtp(
      mobileNumber: mobile,
      otp: otp,
    );

    print("CONTROLLER VERIFY OTP RESPONSE → $response");

    final model = LoginModel.fromJson(response);

    if (!model.success) {
      throw Exception(model.message);
    }

    await TokenStorage.saveLoginData(
      token: model.token,
      tokenExpiry: model.tokenExpiry,
      userId: model.userId,
      username: model.username,
      organisationId: model.organisationId,
    );

    print(
      "LOGIN SUCCESS (MOBILE OTP) → userId=${model.userId}, orgId=${model.organisationId}",
    );
  }
}
