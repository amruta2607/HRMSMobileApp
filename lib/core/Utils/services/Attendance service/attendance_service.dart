import 'dart:convert';
import 'dart:io';
import 'package:geolocator/geolocator.dart';
import 'package:http/http.dart' as http;
import 'package:http_parser/http_parser.dart';
import 'package:intl/intl.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../../../../feature/Attendance/model/geofencing_model.dart';
import '../../../../feature/Attendance/model/attendance_punch_in_out_model.dart';
import '../../../../feature/Attendance/model/weekoverview.dart';
import '../../../../feature/Home/model/attendance_status_model.dart';
import '../../Urls/urls.dart';
import '../Time_Location/location_service.dart';
import 'package:flutter/foundation.dart';
import '../token_storage.dart';

class AttendanceService {
  // Global State Notifiers for real-time synchronization
  static final ValueNotifier<bool> isClockedInNotifier = ValueNotifier<bool>(false);
  static final ValueNotifier<DateTime?> punchInTimeNotifier = ValueNotifier<DateTime?>(null);
  static final ValueNotifier<bool> isPunchedOutForTodayNotifier = ValueNotifier<bool>(false);
  static final ValueNotifier<int> attendanceRefreshNotifier = ValueNotifier<int>(0);

  // Convenience getters
  static bool get isClockedIn => isClockedInNotifier.value;
  static DateTime? get punchInTime => punchInTimeNotifier.value;
  static bool get isPunchedOutForToday => isPunchedOutForTodayNotifier.value;
  static int get refreshCount => attendanceRefreshNotifier.value;

  static void triggerRefresh() => attendanceRefreshNotifier.value++;

  static Future<int?> _getUserId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('userId');
  }

  static Future<int?> _getOrgId() async {
    final prefs = await SharedPreferences.getInstance();
    final id = prefs.getInt('organisationId');
    // print('DEBUG ORG ID FROM PREFS = $id');
    return id;
  }

  // ===================================================
  // CLOCK IN / CLOCK OUT
  // ===================================================
  static Future<({bool success, String? message})> submitAttendance({
    required bool isPunchIn,
    required DateTime punchTime,
    File? image,
  }) async {
    final token = await TokenStorage.getToken();
    if (token == null) return (success: false, message: 'Token Missing');

    final userId = await _getUserId();
    if (userId == null) return (success: false, message: 'User ID Missing');

    late final position;
    try {
      position = await LocationService.getLatLng();
    } catch (e) {
      return (success: false, message: 'Location error: $e');
    }

    final url = isPunchIn ? BaseUrls.punchIn : BaseUrls.punchOut;

    print("punchTime----------------------------");
    print(punchTime);

    // Format date as yyyy-MM-dd
    final dateStr = "${punchTime.year.toString().padLeft(4, '0')}-"
        "${punchTime.month.toString().padLeft(2, '0')}-"
        "${punchTime.day.toString().padLeft(2, '0')}";

    // Format time as UTC ISO8601 (without microseconds)
    final timeStr = DateFormat("yyyy-MM-dd'T'HH:mm:ss").format(punchTime);
    print("Check date and time ____________________________");
    print(dateStr);
    print(timeStr);

    try {
      final request = http.MultipartRequest('POST', Uri.parse(url));

      request.headers.addAll({
        'accept': '*/*',
        'Authorization': 'Bearer $token',
      });

      // Add form fields
      request.fields['userId'] = userId.toString();
      request.fields['attendance_date'] = dateStr;
      request.fields['longitude'] = position.longitude.toString();
      request.fields['latitude'] = position.latitude.toString();

      if (isPunchIn) {
        request.fields['punch_in_time'] = timeStr;
      } else {
        request.fields['punch_out_time'] = timeStr;
      }

      // Add image file
      if (image != null) {
        final mimeType = image.path.toLowerCase().endsWith('.png')
            ? 'image/png'
            : 'image/jpeg';
        final ext = image.path.toLowerCase().endsWith('.png') ? 'png' : 'jpg';
        request.files.add(
          http.MultipartFile.fromBytes(
            'image',
            await image.readAsBytes(),
            filename: 'photo.$ext',
            contentType: MediaType.parse(mimeType),
          ),
        );
      }

      // print('📤 PUNCH REQUEST URL => $url');
      // print('📤 PUNCH FIELDS => ${request.fields}');
      // print('📤 PUNCH FILES => ${request.files.map((f) => '${f.field}: ${f.filename} (${f.length} bytes)').toList()}');

      final streamedResponse = await request.send();
      final response = await http.Response.fromStream(streamedResponse);

      print('📥 PUNCH STATUS => ${response.statusCode}');
      print('📥 PUNCH BODY => ${response.body}');

      if (response.statusCode == 401) {
        await TokenStorage.logoutAndNavigate();
        return (success: false, message: 'Session expired');
      }

      final decoded = jsonDecode(response.body);
      final bool success = decoded['success'] == true;
      final String message = decoded['message']?.toString() ?? '';
      final msgLower = message.toLowerCase();

      // If already done or successfully done
      if (success || msgLower.contains('already') || msgLower.contains('successful')) {
        if (isPunchIn) {
          isClockedInNotifier.value = true;
          punchInTimeNotifier.value = punchTime;
          isPunchedOutForTodayNotifier.value = false;
        } else {
          isClockedInNotifier.value = false;
          punchInTimeNotifier.value = null;
          isPunchedOutForTodayNotifier.value = true;
        }

        if (msgLower.contains('already')) {
          return (success: false, message: message); // Return false but with message
        }
        return (success: true, message: message);
      }

      return (success: false, message: message);
    } catch (e) {
      if (e is http.ClientException || e.toString().contains('SocketException')) {
        // For offline mode, save without image (image can't be serialized to prefs)
        final model = Attendance_in_out_Model(
          userId: userId,
          attendanceDate: punchTime,
          punchInTime: isPunchIn ? punchTime : null,
          punchOutTime: isPunchIn ? null : punchTime,
          latitude: position.latitude,
          longitude: position.longitude,
        );
        final body = isPunchIn ? model.toPunchInJson() : model.toPunchOutJson();
        await _savePendingPunch(body, isPunchIn);
        return (success: true, message: 'Offline: Punch saved for later sync');
      }
      return (success: false, message: e.toString());
    }
  }

  // ===================================================
  // OFFLINE SYNC LOGIC
  // ===================================================
  static Future<void> _savePendingPunch(Map<String, dynamic> body, bool isPunchIn) async {
    final prefs = await SharedPreferences.getInstance();
    final List<String> pending = prefs.getStringList('pending_punches') ?? [];

    final item = {
      'isPunchIn': isPunchIn,
      'body': body,
      'timestamp': DateTime.now().toIso8601String(),
    };

    pending.add(jsonEncode(item));
    await prefs.setStringList('pending_punches', pending);
    // print('✅ Saved pending punch locally. Total pending: ${pending.length}');
  }

  static Future<void> syncPendingPunches() async {
    final prefs = await SharedPreferences.getInstance();
    final List<String> pending = prefs.getStringList('pending_punches') ?? [];

    if (pending.isEmpty) return;

    // print('🌐 SYNC: Found ${pending.length} pending punches. Starting sync...');

    final List<String> failed = [];
    final token = await TokenStorage.getToken();
    if (token == null) return;

    for (final itemStr in pending) {
      try {
        final item = jsonDecode(itemStr);
        final bool isPunchIn = item['isPunchIn'];
        final Map<String, dynamic> body = item['body'];
        final url = isPunchIn ? BaseUrls.punchIn : BaseUrls.punchOut;

        final response = await http.post(
          Uri.parse(url),
          headers: {
            'accept': '*/*',
            'Content-Type': 'application/json',
            'Authorization': 'Bearer $token',
          },
          body: jsonEncode(body),
        );

        if (response.statusCode == 200 || response.statusCode == 201) {
          print('✅ SYNC SUCCESS');
        } else {
          failed.add(itemStr);
        }
      } catch (e) {
        failed.add(itemStr);
      }
    }

    await prefs.setStringList('pending_punches', failed);
    // print('🌐 SYNC COMPLETE. Remaining: ${failed.length}');
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
      if (token == null) return null;

      final userId = await _getUserId();
      if (userId == null) return null;

      final uri = Uri.parse('${BaseUrls.attendanceCalendar}?user_id=$userId&month=$month&year=$year');
      // print('CALENDAR URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      // print('CALENDAR STATUS => ${response.statusCode}');
      // print('CALENDAR BODY => ${response.body}');

      if (response.statusCode == 200) {
        return jsonDecode(response.body);
      }
    } catch (e) {
      print('CALENDAR ERROR => $e');
    }
    return null;
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

      if (userId == null || orgId == null) throw Exception("User/Org ID Missing");

      final now = DateTime.now();
      final startOfWeek = DateTime(now.year, now.month, now.day - (now.weekday - 1), 0, 0, 0);
      final endOfWeek = DateTime(startOfWeek.year, startOfWeek.month, startOfWeek.day + 6, 23, 59, 59);

      final f = DateFormat("yyyy-MM-dd'T'HH:mm:ss");
      final uri = Uri.parse('${BaseUrls.attendanceOverview}?userId=$userId&organisationId=$orgId&fromDate=${f.format(startOfWeek)}&toDate=${f.format(endOfWeek)}');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final decoded = jsonDecode(response.body);
        if (decoded['success'] == true) {
          return WeekOverview.fromJson(decoded['data']);
        }
      }
    } catch (e) {
      print(' OVERVIEW ERROR => $e');
    }
    return null;
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
      if (token == null) return null;

      final f = DateFormat("yyyy-MM-dd'T'HH:mm:ss");
      final uri = Uri.parse('${BaseUrls.attendanceStatus}?userId=$userId&date=${f.format(date)}');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        return AttendanceStatusResponse.fromJson(jsonDecode(response.body));
      }
    } catch (e) {
      print(' ATTENDANCE STATUS ERROR => $e');
    }
    return null;
  }

  static Future<AttendanceStatusData?> getTodayStatus() async {
    try {
      final userId = await _getUserId();
      if (userId == null) return null;

      final response = await getAttendanceStatus(userId: userId, date: DateTime.now());
      if (response != null && response.success) {
        final data = response.data;
        if (data != null) {
          if (data.punchIn != null && data.punchOut == null) {
            isClockedInNotifier.value = true;
            punchInTimeNotifier.value = data.punchIn;
            isPunchedOutForTodayNotifier.value = false;
          } else if (data.punchIn != null && data.punchOut != null) {
            isClockedInNotifier.value = false;
            punchInTimeNotifier.value = null;
            isPunchedOutForTodayNotifier.value = true;
          } else {
            isClockedInNotifier.value = false;
            punchInTimeNotifier.value = null;
            isPunchedOutForTodayNotifier.value = false;
          }
        }
        return response.data;
      }
    } catch (e) {}
    return null;
  }

  static Future<Map<String, dynamic>?> getAttendanceSummary({
    required int month,
    required int year,
  }) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) return null;

      final userId = await _getUserId();
      final orgId = await _getOrgId();
      if (userId == null || orgId == null) {
        // print('ATTENDANCE SUMMARY: userId=$userId orgId=$orgId — aborting');
        return null;
      }

      final fromDate = DateTime(year, month, 1);
      // Pure-Dart way to get the last day of the month (day-0 of next month)
      final lastDay = DateTime(year, month + 1, 0).day;
      final toDate = DateTime(year, month, lastDay, 23, 59, 59);
      final f = DateFormat('yyyy-MM-dd');

      final uri = Uri.parse(
        '${BaseUrls.attendanceSummary}'
            '?organization_id=$orgId'
            '&user_id=$userId'
            '&from_date=${f.format(fromDate)}'
            '&to_date=${f.format(toDate)}',
      );

      print('ATTENDANCE SUMMARY URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print('ATTENDANCE SUMMARY STATUS => ${response.statusCode}');
      // print('ATTENDANCE SUMMARY BODY => ${response.body}');

      if (response.statusCode == 200) {
        final decoded = jsonDecode(response.body);
        if (decoded['success'] == true) return decoded;
      }
    } catch (e) {
      print('ATTENDANCE SUMMARY ERROR => $e');
    }
    return null;
  }

  static Future<GeofencingModel?> getGeofencingDetails() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) return null;

      final response = await http.get(
        Uri.parse(BaseUrls.geofencingByTenant),
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        return GeofencingModel.fromJson(jsonDecode(response.body));
      }
    } catch (e) {}
    return null;
  }

  static bool isWithinRadius({
    required double currentLat,
    required double currentLng,
    required double branchLat,
    required double branchLng,
    required double radius,
  }) {
    final distanceInMeters = Geolocator.distanceBetween(currentLat, currentLng, branchLat, branchLng);
    return distanceInMeters <= radius;
  }
}