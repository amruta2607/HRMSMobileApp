import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../../../../feature/Home/model/holiday.dart';
import '../../Urls/urls.dart';
import '../token_storage.dart';
import '../authenticated_http.dart';

class HolidayService {
  static List<Holiday>? _upcomingCache;
  static DateTime? _upcomingCacheAt;
  static const _cacheTtl = Duration(minutes: 10);

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
      final response = await AuthenticatedHttp.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 401) {
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
  static Future<List<Holiday>?> getUpcomingHolidays({bool force = false}) async {
    try {
      if (!force &&
          _upcomingCache != null &&
          _upcomingCacheAt != null &&
          DateTime.now().difference(_upcomingCacheAt!) < _cacheTtl) {
        return _upcomingCache;
      }

      final token = await TokenStorage.getToken();
      if (token == null) return _upcomingCache;

      final uri = Uri.parse(BaseUrls.upcoming);
      print(' HOLIDAY SERVICE: Calling upcoming API => $uri');
      final response = await AuthenticatedHttp.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 401) {
        return _upcomingCache;
      }

      if (response.statusCode != 200) return _upcomingCache;

      final decoded = jsonDecode(response.body);
      if (decoded is List) {
        _upcomingCache =
            decoded.map((json) => Holiday.fromJson(json)).toList();
        _upcomingCacheAt = DateTime.now();
        return _upcomingCache;
      }
      return _upcomingCache;
    } catch (e) {
      print(' HOLIDAY SERVICE ERROR (Upcoming) => $e');
      return null;
    }
  }

  /// Returns true when today is an active company holiday (for blockPunchOnHoliday).
  static Future<bool> isTodayHoliday() async {
    try {
      final now = DateTime.now();
      final today = DateTime(now.year, now.month, now.day);

      final holidays = await getHolidays(year: now.year) ??
          await getUpcomingHolidays() ??
          const <Holiday>[];

      for (final h in holidays) {
        if (!h.isActive) continue;
        final d = DateTime(h.date.year, h.date.month, h.date.day);
        if (d == today) return true;
      }
      return false;
    } catch (e) {
      print(' HOLIDAY SERVICE ERROR (isTodayHoliday) => $e');
      return false;
    }
  }
}
