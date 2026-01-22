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

  // ================= MOBILE LOGIN =================
  Future<void> loginWithMobile({
    required String mobile,
    required String pin,
  }) async {
    final response = await _auth.loginWithMobile(
      mobileNumber: mobile,
      pin: pin,
    );

    print("CONTROLLER MOBILE RESPONSE → $response");

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
      "LOGIN SUCCESS (MOBILE) → userId=${model.userId}, orgId=${model.organisationId}",



    );
  }
}
