import 'package:shared_preferences/shared_preferences.dart';

class TokenStorage {
  static Future<void> saveLoginData({
    required String token,
    required String tokenExpiry,
    required int userId,
    required String username,
    required int organisationId,
  }) async {
    final prefs = await SharedPreferences.getInstance();

    await prefs.setString('token', token);
    await prefs.setString('tokenExpiry', tokenExpiry);
    await prefs.setInt('userId', userId);
    await prefs.setInt('organisationId', organisationId);
    await prefs.setString('username', username);
    await prefs.setBool('loginStatus', true);

    print('TOKEN SAVED → userId=$userId orgId=$organisationId');
  }

  static Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('token');
  }

  static Future<String?> getUsername() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('username');
  }

  static Future<int?> getUserId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('userId');
  }

  static Future<int?> getOrganisationId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('organisationId');
  }

  static Future<bool> getLoginStatus() async {
    final prefs = await SharedPreferences.getInstance();
    final isLoggedIn = prefs.getBool('loginStatus') ?? false;
    final hasUserId = prefs.containsKey('userId');
    final hasOrgId = prefs.containsKey('organisationId');

    if (isLoggedIn && (!hasUserId || !hasOrgId)) {
      // Data is missing/corrupted, force logout state
      await logout();
      return false;
    }

    return isLoggedIn;
  }

  static Future<bool> isTokenExpired() async {
    final prefs = await SharedPreferences.getInstance();
    final expiry = prefs.getString('tokenExpiry');
    if (expiry == null) return true;
    return DateTime.now().isAfter(DateTime.parse(expiry));
  }

  static Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.clear();
    print('LOGOUT → STORAGE CLEARED');
  }
}
