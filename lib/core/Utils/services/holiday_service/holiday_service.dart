import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../../../../feature/Home/model/holiday.dart';

import '../token_storage.dart';

class HolidayService {
  static const String _baseUrl = 'http://103.123.74.160:81/apipunch/holidays';

  static Future<int?> _getUserId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('userId');
  }

  static Future<int?> _getOrgId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('organisationId');
  }

  static Future<List<Holiday>?> getHolidays({int? year}) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        print(' HOLIDAY SERVICE: Token is NULL');
        return null;
      }

      final userId = await _getUserId();
      final orgId = await _getOrgId();

      if (userId == null || orgId == null) {
        print(' HOLIDAY SERVICE: userId or orgId is NULL');
        return null;
      }

      final queryYear = year ?? DateTime.now().year;

      final uri = Uri.parse(
          '$_baseUrl/get-holidays?user_id=$userId&organization_id=$orgId&year=$queryYear');

      print(' HOLIDAY API URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print(' HOLIDAY STATUS => ${response.statusCode}');

      if (response.statusCode == 401) {
        print(' HOLIDAY SERVICE: Token expired, logging out');
        await TokenStorage.logoutAndNavigate();
        return null;
      }

      if (response.statusCode != 200) {
        print(' HOLIDAY SERVICE: Non-200 status');
        return null;
      }

      final decoded = jsonDecode(response.body);

      if (decoded['success'] == true) {
        final List<dynamic> data = decoded['data'];
        return data.map((json) => Holiday.fromJson(json)).toList();
      } else {
        print(' HOLIDAY SERVICE: API returned success=false');
        return null;
      }
    } catch (e, s) {
      print(' HOLIDAY SERVICE ERROR => $e');
      print(' STACKTRACE => $s');
      return null;
    }
  }
}
