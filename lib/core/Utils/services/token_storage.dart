import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:http/http.dart' as http;

import '../Urls/urls.dart';
import ' navigation_service.dart';
import '../../../../feature/Profile/controller/profile_controller.dart';

class TokenStorage {
  static Map<String, bool> moduleAccessCache = {
    'attendance': true,
    'leave': true,
    'payroll': true,
  };

  // ==================== REFRESH TOKEN LOCK ====================
  // Prevents multiple simultaneous refresh calls
  static bool _isRefreshing = false;
  static Future<String?>? _refreshFuture;

  static Future<void> saveModuleAccess(Map<String, dynamic>? moduleAccess) async {
    if (moduleAccess == null) return;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool('module_isEnableMobile', moduleAccess['isEnableMobile'] == true);
    await prefs.setBool('module_attendance', moduleAccess['attendance'] == true);
    await prefs.setBool('module_leave', moduleAccess['leave'] == true);
    await prefs.setBool('module_payroll', moduleAccess['payroll'] == true);
  }

  static Future<void> loadModuleAccessRaw() async {
    final prefs = await SharedPreferences.getInstance();
    bool isEnableMobile = prefs.getBool('module_isEnableMobile') ?? false;
    bool attendance = prefs.getBool('module_attendance') ?? true;
    bool leave = prefs.getBool('module_leave') ?? true;
    bool payroll = prefs.getBool('module_payroll') ?? true;
    _updateCache(isEnableMobile, attendance, leave, payroll);
  }

  static Future<void> loadModuleAccess() async {
    await loadModuleAccessRaw();
    final prefs = await SharedPreferences.getInstance();

    try {
      final orgId = prefs.getInt('organisationId');
      final token = await getValidToken();
      if (orgId != null && token != null) {
        final response = await http.get(
          Uri.parse('${BaseUrls.moduleAccess}/$orgId'),
          headers: {
            'Authorization': 'Bearer $token',
            'Content-Type': 'application/json',
          },
        );
        if (response.statusCode == 200) {
          final data = jsonDecode(response.body);
          await saveModuleAccess(data);
          await loadModuleAccessRaw();
        }
      }
    } catch (e) {
      print('Error fetching module access dynamically: $e');
    }
  }

  static void _updateCache(bool isEnableMobile, bool attendance, bool leave, bool payroll) {
    if (!isEnableMobile) {
      moduleAccessCache = {
        'attendance': true,
        'leave': true,
        'payroll': true,
      };
    } else {
      moduleAccessCache = {
        'attendance': attendance,
        'leave': leave,
        'payroll': payroll,
      };
    }
  }

  static bool isModuleEnabled(String moduleName) {
    return moduleAccessCache[moduleName] ?? true;
  }

  // ==================== SAVE LOGIN DATA ====================
  static Future<void> saveLoginData({
    required String token,
    required String refreshToken,
    required String tokenExpiry,
    required int userId,
    required String username,
    required int organisationId,
  }) async {
    final prefs = await SharedPreferences.getInstance();

    await prefs.setString('token', token);
    await prefs.setString('refreshToken', refreshToken);
    await prefs.setString('tokenExpiry', tokenExpiry);
    await prefs.setInt('userId', userId);
    await prefs.setInt('organisationId', organisationId);
    await prefs.setString('username', username);
    await prefs.setBool('loginStatus', true);
    final nowStr = DateTime.now().toString().split('.').first;
    print('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
    print('🔑 [NEW TOKENS SAVED ON LOGIN]');
    print('⏰ Logged In At : $nowStr');
    print('⏳ Expiry Time  : $tokenExpiry');
    print('🧑 User ID: $userId | Org ID: $organisationId');
    print('🛡️ Access Token : ${token.substring(0, token.length > 30 ? 30 : token.length)}...');
    print('🔄 Refresh Token: ${refreshToken.substring(0, refreshToken.length > 30 ? 30 : refreshToken.length)}...');
    print('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
  }

  // ==================== GETTERS ====================
  /// Gets a valid access token (auto-refreshes if expired)
  static Future<String?> getToken() async {
    return await getValidToken();
  }

  static Future<String?> _getRawToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('token');
  }

  static Future<String?> getRefreshToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('refreshToken');
  }

  static Future<String?> getUsername() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('username');
  }

  static Future<int?> getUserId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('userId');
  }

  static Future<int?> getBranchId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('branchId');
  }

  static Future<int?> getOrganisationId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('organisationId');
  }

  static Future<bool> getLoginStatus() async {
    final prefs = await SharedPreferences.getInstance();
    final hasToken = prefs.getString('token') != null && prefs.getString('token')!.isNotEmpty;
    final isLoggedIn = (prefs.getBool('loginStatus') ?? false) || hasToken;
    return isLoggedIn;
  }

  static Future<bool> isTokenExpired() async {
    // Delegated to server 401 response to avoid local mobile clock desync
    return false;
  }

  // ==================== AUTO REFRESH TOKEN ====================
  /// Returns existing access token. Actual refresh is triggered automatically
  /// by AuthenticatedHttp whenever the backend server returns 401 Unauthorized.
  static Future<String?> getValidToken() async {
    return await _getRawToken();
  }

  /// Calls the refresh-token API to get a new access token.
  /// Uses a lock to prevent multiple concurrent refresh calls.
  static Future<String?> refreshAccessToken() async {
    // If already refreshing, wait for the existing refresh to complete
    if (_isRefreshing && _refreshFuture != null) {
      print('⏳ [REFRESH IN PROGRESS] Waiting for existing refresh call...');
      return _refreshFuture;
    }

    _isRefreshing = true;
    _refreshFuture = _doRefresh();

    try {
      final result = await _refreshFuture;
      return result;
    } finally {
      _isRefreshing = false;
      _refreshFuture = null;
    }
  }

  static Future<String?> _doRefresh() async {
    try {
      final refreshToken = await getRefreshToken();
      if (refreshToken == null || refreshToken.isEmpty) {
        print('❌ [REFRESH FAILED] No refresh token found in storage. Logging out...');
        await logoutAndNavigate();
        return null;
      }
      final prefs = await SharedPreferences.getInstance();
      final currentToken = prefs.getString('token') ?? '';
      final userId = prefs.getInt('userId');

      print('📡 [REFRESH API CALL] POST ${BaseUrls.refreshToken}');
      print('🔑 [REFRESH TOKEN SENT] ${refreshToken.substring(0, refreshToken.length > 15 ? 15 : refreshToken.length)}...');

      final requestBody = {
        'refreshToken': refreshToken,
        'accessToken': currentToken,
        'token': currentToken,
        if (userId != null) 'userId': userId,
      };

      final response = await http.post(
        Uri.parse(BaseUrls.refreshToken),
        headers: {
          'accept': '*/*',
          'Content-Type': 'application/json',
        },
        body: jsonEncode(requestBody),
      );

      print('📥 [REFRESH API RESPONSE STATUS] → ${response.statusCode}');
      print('📥 [REFRESH API RESPONSE BODY] → ${response.body}');

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        final newAccessToken = (data['accessToken'] ?? data['token'] ?? (data['data'] != null ? (data['data']['accessToken'] ?? data['data']['token']) : null))?.toString();

        if (newAccessToken != null && newAccessToken.isNotEmpty) {
          await prefs.setString('token', newAccessToken);

          // Save token expiry if provided by server
          String newExpiryStr = (data['tokenExpiry'] ?? data['expiry'] ?? data['expiresAt'] ?? data['expiration'] ?? (data['data'] != null ? (data['data']['tokenExpiry'] ?? data['data']['expiry'] ?? data['data']['expiresAt']) : null))?.toString() ?? 'Server Managed (Not provided by API)';
          if (newExpiryStr != 'Server Managed (Not provided by API)') {
            await prefs.setString('tokenExpiry', newExpiryStr);
          }

          // Update refresh token if a new one is returned
          if (data['refreshToken'] != null) {
            await prefs.setString('refreshToken', data['refreshToken']);
          }

          final nowStr = DateTime.now().toString().split('.').first;
          print('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
          print('🎉 [GOT NEW TOKENS FROM REFRESH API]');
          print('⏰ Refreshed At : $nowStr');
          print('⏳ Expiry Time  : $newExpiryStr');
          print('🛡️ Access Token : ${newAccessToken.substring(0, newAccessToken.length > 30 ? 30 : newAccessToken.length)}...');
          if (data['refreshToken'] != null) {
            final newRefresh = data['refreshToken'].toString();
            print('🔄 Refresh Token: ${newRefresh.substring(0, newRefresh.length > 30 ? 30 : newRefresh.length)}...');
          } else {
            print('🔄 Refresh Token: (Unchanged by server)');
          }
          print('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
          return newAccessToken;
        }
      }

      // If server explicitly returns 401 or 403, refresh token is rejected/expired
      if (response.statusCode == 401 || response.statusCode == 403) {
        print('❌ [REFRESH REJECTED] Server rejected refresh token (${response.statusCode}). Logging out...');
        await logoutAndNavigate();
        return null;
      }

      print('⚠️ [REFRESH ERROR] Status ${response.statusCode}. Keeping session active.');
      return null;
    } catch (e) {
      print('🌐 [REFRESH NETWORK ERROR] Exception during token refresh: $e. Keeping session active.');
      rethrow;
    }
  }

  // ==================== LOGOUT ====================
  static Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.clear();
    print('LOGOUT → STORAGE CLEARED');
  }

  static Future<void> logoutAndNavigate() async {
    final context = NavigationService.navigatorKey.currentContext;
    if (context != null) {
      try {
        Provider.of<ProfileController>(context, listen: false).clearData();
      } catch (e) {
        print('LOGOUT ERROR CLEARING DATA: $e');
      }
    }

    await logout();

    if (context != null) {
      Navigator.of(context).pushNamedAndRemoveUntil(
        '/login', // Assuming named route or use MaterialPageRoute
            (route) => false,
      );
    } else {
      NavigationService.navigatorKey.currentState?.pushNamedAndRemoveUntil(
        '/login',
            (route) => false,
      );
    }
    print('LOGOUT → NAVIGATED TO LOGIN');
  }
}
