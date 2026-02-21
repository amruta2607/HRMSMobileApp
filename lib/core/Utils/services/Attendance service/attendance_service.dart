import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import 'package:intl/intl.dart';

import 'package:geolocator/geolocator.dart';
import '../../../../feature/Attendance/model/geofencing_model.dart';
import '../../../../feature/Attendance/model/attendance_punch_in_out_model.dart';
import '../../../../feature/Attendance/model/weekoverview.dart';
import '../../../../feature/Home/model/attendance_status_model.dart';
import '../../Urls/urls.dart';
import '../Time_Location/location_service.dart';
import '../token_storage.dart';

class AttendanceService {
  static Future<int?> _getUserId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('userId');
  }

  static Future<int?> _getOrgId() async {
    final prefs = await SharedPreferences.getInstance();
    final id = prefs.getInt('organisationId');
    print('DEBUG ORG ID FROM PREFS = $id');
    return id;
  }

  // ===================================================
  // CLOCK IN / CLOCK OUT
  // ===================================================
  static Future<bool> submitAttendance({
    required bool isPunchIn,
    required DateTime punchTime,
  }) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        print(' ATTENDANCE: Token is NULL');
        return false;
      }

      final userId = await _getUserId();
      if (userId == null) {
        print(' ATTENDANCE: userId is NULL');
        return false;
      }

      late final position;
      try {
        position = await LocationService.getLatLng();
      } catch (e) {
        print(' ATTENDANCE: Location error => $e');
        return false;
      }

      final model = Attendance_in_out_Model(
        userId: userId,
        attendanceDate: punchTime,
        punchInTime: isPunchIn ? punchTime : null,
        punchOutTime: isPunchIn ? null : punchTime,
        latitude: position.latitude,
        longitude: position.longitude,
      );

      final url = isPunchIn ? BaseUrls.punchIn : BaseUrls.punchOut;
      final body =
      isPunchIn ? model.toPunchInJson() : model.toPunchOutJson();

      print(' ATTENDANCE API URL => $url');
      print(' ATTENDANCE REQUEST BODY => ${jsonEncode(body)}');

      final response = await http.post(
        Uri.parse(url),
        headers: {
          'accept': '*/*',
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
        body: jsonEncode(body),
      );

      print(' ATTENDANCE STATUS => ${response.statusCode}');
      print(' ATTENDANCE RESPONSE => ${response.body}');

      if (response.statusCode == 401) {
        print(' ATTENDANCE: Token expired, logging out');
        await TokenStorage.logoutAndNavigate();
        return false;
      }

      if (response.statusCode != 200 &&
          response.statusCode != 201) {
        print(' ATTENDANCE: Non-success HTTP status');
        return false;
      }

      final decoded = jsonDecode(response.body);
      final success = decoded['success'] == true;
      final message =
          decoded['message']?.toString().toLowerCase() ?? '';

      if (success) return true;
      if (message.contains('already')) return true;
      if (message.contains('successful')) return true;

      print(' ATTENDANCE: success=false, message=$message');
      return false;
    } catch (e, s) {
      print(' ATTENDANCE ERROR => $e');
      print(' STACKTRACE => $s');
      return false;
    }
  }

  // ===================================================
  // CALENDAR
  // ===================================================
  static Future<Map<String, dynamic>?> getAttendanceByCalendar({
    required int month,
    required int year,
  }) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        print(' CALENDAR: Token is NULL');
        return null;
      }

      final userId = await _getUserId();
      if (userId == null) {
        print(' CALENDAR: userId is NULL');
        return null;
      }

      final uri = Uri.parse(
        '${BaseUrls
            .attendanceCalendar}?user_id=$userId&month=$month&year=$year',
      );

      print(' CALENDAR API URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print(' CALENDAR STATUS => ${response.statusCode}');
      print(' CALENDAR RESPONSE => ${response.body}');

      if (response.statusCode == 401) {
        print(' CALENDAR: Token expired, logging out');
        await TokenStorage.logoutAndNavigate();
        return null;
      }

      if (response.statusCode != 200) {
        print(' CALENDAR: Non-200 status');
        return null;
      }

      final decoded = jsonDecode(response.body);

      if (decoded['success'] != true) {
        print(' CALENDAR: success=false');
        return null;
      }

      return decoded;
    } catch (e, s) {
      print(' CALENDAR ERROR => $e');
      print(' STACKTRACE => $s');
      return null;
    }
  }

  // ===================================================
  // CURRENT WEEK OVERVIEW
  // ===================================================
  static Future<WeekOverview?> getCurrentWeekOverview() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) throw Exception("Token Missing");

      final userId = await _getUserId();
      final orgId = await _getOrgId();

      if (userId == null || orgId == null) throw Exception(
          "User/Org ID Missing");

      final now = DateTime.now();

      // Monday → Sunday (LOCAL TIME)
      final startOfWeek = DateTime(
        now.year,
        now.month,
        now.day - (now.weekday - 1),
        0,
        0,
        0,
      );

      final endOfWeek = DateTime(
        startOfWeek.year,
        startOfWeek.month,
        startOfWeek.day + 6,
        23,
        59,
        59,
      );

      final f = DateFormat("yyyy-MM-dd'T'HH:mm:ss");
      final fromDate = f.format(startOfWeek);
      final toDate = f.format(endOfWeek);

      final uri = Uri.parse(
        '${BaseUrls.attendanceOverview}'
            '?userId=$userId'
            '&organisationId=$orgId'
            '&fromDate=$fromDate'
            '&toDate=$toDate',
      );

      print(' OVERVIEW API URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print(' OVERVIEW STATUS => ${response.statusCode}');
      print(' OVERVIEW RESPONSE => ${response.body}');

      if (response.statusCode == 401) {
        await TokenStorage.logoutAndNavigate();
        throw Exception("Session Expired (401)");
      }

      if (response.statusCode != 200) {
        throw Exception("HTTP ${response.statusCode}");
      }

      final decoded = jsonDecode(response.body);

      if (decoded['success'] != true) {
        final msg = decoded['message'] ?? 'Unknown API Error';
        throw Exception("API Error: $msg");
      }

      print(' OVERVIEW DATA => ${decoded['data']}');

      return WeekOverview.fromJson(decoded['data']);
    } catch (e, s) {
      print(' OVERVIEW ERROR => $e');
      print(' STACKTRACE => $s');
      // Rethrow so FutureBuilder sees the error
      throw e;
    }
  }


  // ===================================================
  // ATTENDANCE STATUS
  // ===================================================
  static Future<AttendanceStatusResponse?> getAttendanceStatus({
    required int userId,
    required DateTime date,
  }) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        print('🔴 ATTENDANCE STATUS: Token is NULL');
        return null;
      }

      // Format the date as ISO 8601 string
      final f = DateFormat("yyyy-MM-dd'T'HH:mm:ss");
      final dateStr = f.format(date);

      final uri = Uri.parse(
        '${BaseUrls.attendanceStatus}?userId=$userId&date=$dateStr',
      );

      print('🔵 ATTENDANCE STATUS API URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print('🔵 ATTENDANCE STATUS STATUS => ${response.statusCode}');
      print('🔵 ATTENDANCE STATUS RESPONSE => ${response.body}');

      if (response.statusCode == 401) {
        print('🔴 ATTENDANCE STATUS: Token expired, logging out');
        await TokenStorage.logoutAndNavigate();
        return null;
      }

      if (response.statusCode != 200) {
        print('🔴 ATTENDANCE STATUS: Non-200 status');
        return null;
      }

      final decoded = jsonDecode(response.body);

      return AttendanceStatusResponse.fromJson(decoded);
    } catch (e, s) {
      print(' ATTENDANCE STATUS ERROR => $e');
      print(' STACKTRACE => $s');
      return null;
    }
  }

  // ===================================================
  // GET TODAY STATUS (Convenience)
  // ===================================================
  static Future<AttendanceStatusData?> getTodayStatus() async {
    try {
      final userId = await _getUserId();
      if (userId == null) return null;

      final response = await getAttendanceStatus(
          userId: userId,
          date: DateTime.now()
      );

      if (response != null && response.success && response.data != null) {
        return response.data;
      }
    } catch (e) {
      print('Error fetching today status: $e');
    }
    return null;
  }

  // ATTENDANCE SUMMARY (RECORDS)
  // ===================================================
  static Future<Map<String, dynamic>?> getAttendanceSummary({
    required int month,
    required int year,
  }) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) return null;

      final userId = await _getUserId();
      final orgId = await _getOrgId();

      if (userId == null || orgId == null) return null;

      // Calculate first and last day of the month
      final fromDate = DateTime(year, month, 1);
      final toDate = DateTime(year, month + 1, 0); // Last day of month

      final f = DateFormat("yyyy-MM-dd'T'HH:mm:ss");
      final fromStr = f.format(fromDate);
      final toStr = f.format(toDate);

      final uri = Uri.parse(
        '${BaseUrls.attendanceSummary}'
            '?organization_id=$orgId'
            '&user_id=$userId'
            '&from_date=$fromStr'
            '&to_date=$toStr',
      );

      print(' SUMMARY API URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print(' SUMMARY STATUS => ${response.statusCode}');
      if (response.statusCode != 200) return null;

      final decoded = jsonDecode(response.body);
      if (decoded['success'] != true) return null;

      return decoded;
    } catch (e) {
      print(' SUMMARY ERROR => $e');
      return null;
    }
  }

  // ===================================================
  // GEOFENCING
  // ===================================================
  static Future<GeofencingModel?> getGeofencingDetails() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) return null;

      final uri = Uri.parse(BaseUrls.geofencingByTenant);
      print(' GEOFENCING API URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print(' GEOFENCING STATUS => ${response.statusCode}');
      print(' GEOFENCING RESPONSE => ${response.body}');

      if (response.statusCode == 200) {
        final decoded = jsonDecode(response.body);
        return GeofencingModel.fromJson(decoded);
      }
    } catch (e) {
      print(' GEOFENCING ERROR => $e');
    }
    return null;
  }

  static bool isWithinRadius({
    required double currentLat,
    required double currentLng,
    required double branchLat,
    required double branchLng,
    required double radius,
  }) {
    final distanceInMeters = Geolocator.distanceBetween(
      currentLat,
      currentLng,
      branchLat,
      branchLng,
    );

    print(' DISTANCE: $distanceInMeters meters, RADIUS: $radius meters');
    return distanceInMeters <= radius;
  }
}