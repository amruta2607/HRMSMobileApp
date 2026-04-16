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
      final token = prefs.getString('token');
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
