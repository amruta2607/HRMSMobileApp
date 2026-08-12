import 'dart:convert';
import 'package:http/http.dart' as http;

import '../../Urls/urls.dart';
import '../token_storage.dart';

class AuthService {
  // ================= EMAIL LOGIN =================
  Future<Map<String, dynamic>> loginWithEmail({
    required String email,
    required String password,
  }) async {
    final url = Uri.parse(BaseUrls.loginWithEmail);

    final body = {
      "usernameOrEmail": email.trim(),
      "email": email.trim(),
      "password": password.trim(),
    };

    print("LOGIN EMAIL URL => $url");
    print("LOGIN EMAIL BODY => $body");

    final response = await http.post(
      url,
      headers: {"Content-Type": "application/json"},
      body: jsonEncode(body),
    );

    print("LOGIN EMAIL STATUS => ${response.statusCode}");
    print("LOGIN EMAIL RESPONSE => ${response.body}");

    return jsonDecode(response.body);
  }

  // ================= FORGOT PASSWORD =================
  Future<Map<String, dynamic>> forgotPassword({
    required String email,
  }) async {
    final url = Uri.parse(BaseUrls.forgotPassword);

    final body = {
      "email": email.trim(),
    };

    print("FORGOT PASSWORD URL => $url");
    print("FORGOT PASSWORD BODY => $body");

    final response = await http.post(
      url,
      headers: {
        "accept": "*/*",
        "Content-Type": "application/json",
      },
      body: jsonEncode(body),
    );

    print("FORGOT PASSWORD STATUS => ${response.statusCode}");
    print("FORGOT PASSWORD RESPONSE => ${response.body}");

    return jsonDecode(response.body);
  }

  // ================= VERIFY OTP =================
  Future<Map<String, dynamic>> verifyOtp({
    required String email,
    required String otp,
  }) async {
    // TODO: Verify correct URL with user
    final url = Uri.parse("${BaseUrls.base}/api/Auth/verify-otp");

    final body = {
      "email": email.trim(),
      "otp": otp.trim(),
    };

    print("VERIFY OTP URL => $url");
    print("VERIFY OTP BODY => $body");

    final response = await http.post(
      url,
      headers: {
        "accept": "*/*",
        "Content-Type": "application/json",
      },
      body: jsonEncode(body),
    );

    print("VERIFY OTP STATUS => ${response.statusCode}");
    print("VERIFY OTP RESPONSE => ${response.body}");

    return jsonDecode(response.body);
  }



  // ================= MOBILE LOGIN - STEP 1: SEND OTP =================
  Future<Map<String, dynamic>> sendMobileOtp({
    required String mobileNumber,
  }) async {
    final url = Uri.parse(BaseUrls.loginWithMobile);

    final body = {
      "mobileNumber": mobileNumber.trim(),
      "otp": "",
    };

    print("SEND OTP URL => $url");
    print("SEND OTP BODY => $body");

    final response = await http.post(
      url,
      headers: {"Content-Type": "application/json"},
      body: jsonEncode(body),
    );

    print("SEND OTP STATUS => ${response.statusCode}");
    print("SEND OTP RESPONSE => ${response.body}");

    return jsonDecode(response.body);
  }

  // ================= MOBILE LOGIN - STEP 2: VERIFY OTP =================
  Future<Map<String, dynamic>> verifyMobileOtp({
    required String mobileNumber,
    required String otp,
  }) async {
    final url = Uri.parse(BaseUrls.loginWithMobile);

    final body = {
      "mobileNumber": mobileNumber.trim(),
      "otp": otp.trim(),
    };

    print("VERIFY MOBILE OTP URL => $url");
    print("VERIFY MOBILE OTP BODY => $body");

    final response = await http.post(
      url,
      headers: {"Content-Type": "application/json"},
      body: jsonEncode(body),
    );

    print("VERIFY MOBILE OTP STATUS => ${response.statusCode}");
    print("VERIFY MOBILE OTP RESPONSE => ${response.body}");

    return jsonDecode(response.body);
  }

  // ================= LOGOUT =================
  static Future<bool> logout() async {
    try {
      // Get a valid token (auto-refreshes if expired)
      final token = await TokenStorage.getValidToken();

      if (token == null) {
        print("LOGOUT: No valid token, clearing storage");
        await TokenStorage.logout();
        return true;
      }

      final response = await http.post(
        Uri.parse(BaseUrls.logout),
        headers: {
          "accept": "*/*",
          "Authorization": "Bearer $token",
        },
      );

      print("LOGOUT STATUS => ${response.statusCode}");
      print("LOGOUT RESPONSE => ${response.body}");

      // Always clear local storage
      await TokenStorage.logout();

      // 200 = success
      // 401 = already logged out / expired
      return response.statusCode == 200 || response.statusCode == 401;
    } catch (e, s) {
      print("LOGOUT ERROR => $e");
      print("STACKTRACE => $s");

      // Fail-safe cleanup
      await TokenStorage.logout();
      return false;
    }
  }

  // ================= RESET PASSWORD =================
  Future<Map<String, dynamic>> resetPassword({
    required Map<String, dynamic> model,
  }) async {
    // TODO: Verify correct URL with user
    final url = Uri.parse("${BaseUrls.base}/api/Auth/reset-password");

    print("RESET PASSWORD URL => $url");
    print("RESET PASSWORD BODY => $model");

    final response = await http.post(
      url,
      headers: {
        "accept": "*/*",
        "Content-Type": "application/json",
      },
      body: jsonEncode(model),
    );

    print("RESET PASSWORD STATUS => ${response.statusCode}");
    print("RESET PASSWORD RESPONSE => ${response.body}");

    return jsonDecode(response.body);
  }
}
