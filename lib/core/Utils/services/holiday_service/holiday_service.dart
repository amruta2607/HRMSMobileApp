import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../../../../feature/Home/model/holiday.dart';
import '../../Urls/urls.dart';
import '../token_storage.dart';

class HolidayService {
  static Future<int?> _getUserId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('userId');
  }

  static Future<int?> _getOrgId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('organisationId');
  }

  /// Original API for the Holidays screen - Needs params and is wrapped
  static Future<List<Holiday>?> getHolidays({int? year}) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) return null;

      final userId = await _getUserId();
      final orgId = await _getOrgId();
      if (userId == null || orgId == null) return null;

      final queryYear = year ?? DateTime.now().year;
      final uri = Uri.parse(
          '${BaseUrls.holidays}?user_id=$userId&organization_id=$orgId&year=$queryYear');

      print(' HOLIDAY SERVICE: Calling standard API => $uri');
      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 401) {
        await TokenStorage.logoutAndNavigate();
        return null;
      }

      if (response.statusCode != 200) return null;

      final decoded = jsonDecode(response.body);
      if (decoded['success'] == true) {
        final List<dynamic> data = decoded['data'];
        return data.map((json) => Holiday.fromJson(json)).toList();
      }
      return null;
    } catch (e) {
      print(' HOLIDAY SERVICE ERROR (Standard) => $e');
      return null;
    }
  }

  /// New API for Home Up Next - No params and is a direct list
  static Future<List<Holiday>?> getUpcomingHolidays() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) return null;

      final uri = Uri.parse(BaseUrls.upcoming);
      print(' HOLIDAY SERVICE: Calling upcoming API => $uri');
      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 401) {
        await TokenStorage.logoutAndNavigate();
        return null;
      }

      if (response.statusCode != 200) return null;

      final decoded = jsonDecode(response.body);
      if (decoded is List) {
        return decoded.map((json) => Holiday.fromJson(json)).toList();
      }
      return null;
    } catch (e) {
      print(' HOLIDAY SERVICE ERROR (Upcoming) => $e');
      return null;
    }
  }
}
