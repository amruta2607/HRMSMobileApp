import 'dart:convert';
import 'dart:io';
import 'package:flutter/foundation.dart';
import 'package:geolocator/geolocator.dart';
import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../../../../feature/Attendance/model/geofencing_model.dart';
import '../../../../feature/Attendance/model/attendance_punch_in_out_model.dart';
import '../../../../feature/Attendance/model/weekoverview.dart';
import '../../../../feature/Home/model/attendance_status_model.dart';
import '../../Urls/urls.dart';
import '../Time_Location/location_service.dart';
import '../token_storage.dart';
import '../authenticated_http.dart';
import '../../../Background_location _tracking/services/location_service.dart' as bg_tracking;
import '../../../Background_location _tracking/services/location_config_service.dart';
import '../../../Background_location _tracking/services/gps_monitor_service.dart';
import '../../../constants/location_config.dart';
import '../holiday_service/holiday_service.dart';

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

  /// Avoid notifying listeners when punch-in time is effectively unchanged
  /// (new DateTime instances were causing Attendance tab to re-fetch repeatedly).
  static void _setPunchInTimeIfChanged(DateTime? next) {
    final prev = punchInTimeNotifier.value;
    if (prev == null && next == null) return;
    if (prev != null &&
        next != null &&
        prev.millisecondsSinceEpoch == next.millisecondsSinceEpoch) {
      return;
    }
    punchInTimeNotifier.value = next;
  }

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
    bool isManual = true,
    String punchOutReason = 'Normal Punch Out',
  }) async {
    final token = await TokenStorage.getToken();
    if (token == null) return (success: false, message: 'Token Missing');

    final userId = await _getUserId();
    if (userId == null) return (success: false, message: 'User ID Missing');

    late final Position position;
    try {
      position = await LocationService.getLatLng();
    } catch (e) {
      return (success: false, message: 'Location error: $e');
    }

    // Android emulator default (Googleplex).
    final isEmulatorDefaultLocation =
        (position.latitude - 37.421998).abs() < 0.002 &&
            (position.longitude + 122.084).abs() < 0.002;
    if (isEmulatorDefaultLocation) {
      return (
        success: false,
        message:
            'Invalid location (emulator default USA). Set office location in emulator, then retry.',
      );
    }

    // Sync status for UI. Allow another punch-in after punch-out.
    await LocationConfigService.fetchConfig();

    if (!LocationConfig.ATTENDANCE_ENABLED) {
      return (
        success: false,
        message: 'Attendance is disabled by admin configuration.',
      );
    }

    final todayStatus = await getTodayStatus();
    print('📋 TODAY STATUS before punch => '
        'punchIn=${todayStatus?.punchIn}, punchOut=${todayStatus?.punchOut}, '
        'status=${todayStatus?.status}, '
        'duplicateSessionCheck=${LocationConfig.DUPLICATE_SESSION_CHECK} '
        '(fresh config)');

    // Block punch-in on holiday when dashboard flag is on.
    if (isPunchIn && LocationConfig.BLOCK_PUNCH_ON_HOLIDAY) {
      final isHoliday = await HolidayService.isTodayHoliday();
      if (isHoliday) {
        return (
          success: false,
          message: 'Punch-in is blocked on holidays.',
        );
      }
    }

    // Duplicate session: open punch-in on server without punch-out.
    if (isPunchIn && LocationConfig.DUPLICATE_SESSION_CHECK) {
      if (todayStatus?.punchIn != null && todayStatus?.punchOut == null) {
        return (
          success: false,
          message: 'Already punched in. Use Punch-Out first.',
        );
      }
    } else if (isPunchIn &&
        todayStatus?.punchIn != null &&
        todayStatus?.punchOut == null &&
        isClockedIn) {
      // Legacy local-only guard when duplicateSessionCheck is off.
      return (
        success: false,
        message: 'Already punched in. Use Punch-Out first.',
      );
    }

    final url = isPunchIn ? BaseUrls.punchIn : BaseUrls.punchOut;

    final dateStr = "${punchTime.year.toString().padLeft(4, '0')}-"
        "${punchTime.month.toString().padLeft(2, '0')}-"
        "${punchTime.day.toString().padLeft(2, '0')}";
    final timeStr = DateFormat("yyyy-MM-dd'T'HH:mm:ss").format(punchTime);

    print('punchTime => $punchTime | date=$dateStr time=$timeStr');

    // Geofence only when server config disallows punch from anywhere.
    try {
      final geoConfig = await getGeofencingDetails();
      final fromAnywhere = LocationConfig.ENABLE_FROM_ANYWHERE;
      final radius = resolveGeofenceRadius(geoConfig?.radius);
      print(
          '📍 GEOFENCE => enabled=${geoConfig?.isEnabled}, fromAnywhere=$fromAnywhere, radius=$radius');
      if (!fromAnywhere && geoConfig != null && geoConfig.isEnabled) {
        final within = isWithinRadius(
          currentLat: position.latitude,
          currentLng: position.longitude,
          branchLat: geoConfig.latitude,
          branchLng: geoConfig.longitude,
          radius: radius,
        );
        if (!within) {
          return (
            success: false,
            message: 'You are not in the office range. Radius: ${radius}m',
          );
        }
      }
    } catch (e) {
      print('Geofencing lookup failed: $e');
    }

    try {
      final request = http.MultipartRequest('POST', Uri.parse(url));
      request.headers.addAll({
        'accept': '*/*',
        'Authorization': 'Bearer $token',
      });

      // Exact swagger fields only (extra fields / real image binary break this API).
      request.fields['userId'] = userId.toString();
      request.fields['attendance_date'] = dateStr;
      request.fields['longitude'] = position.longitude.toString();
      request.fields['latitude'] = position.latitude.toString();

      if (isPunchIn) {
        request.fields['punch_in_time'] = timeStr;
      } else {
        request.fields['punch_out_time'] = timeStr;
        request.fields['Manual'] = isManual.toString();
        request.fields['PunchOutReason'] = punchOutReason;
      }

      // Server returns 400 "Error while processing punch in" when a real image
      // binary is uploaded. Swagger/curl succeeds with an empty image field.
      request.files.add(
        http.MultipartFile.fromBytes('image', const <int>[], filename: ''),
      );

      print('📤 PUNCH REQUEST URL => $url');
      print('📤 PUNCH FIELDS => ${request.fields}');
      print('📤 PUNCH FILES => empty image field (server rejects real binaries)');

      final streamedResponse = await request.send();
      final response = await http.Response.fromStream(streamedResponse);

      print('📥 PUNCH STATUS => ${response.statusCode}');
      print('📥 PUNCH BODY => ${response.body}');

      if (response.statusCode == 401) {
        return (success: false, message: 'Session expired');
      }

      Map<String, dynamic> decoded = {};
      try {
        final body = jsonDecode(response.body);
        if (body is Map<String, dynamic>) decoded = body;
      } catch (_) {}

      final String message = decoded['message']?.toString() ??
          decoded['title']?.toString() ??
          (response.body.isNotEmpty ? response.body : 'Punch failed');
      final msgLower = message.toLowerCase();

      // API often returns only {message: "Punch In/Out Successful"} (no success:true).
      final bool ok = decoded['success'] == true ||
          msgLower.contains('successful') ||
          (response.statusCode == 200 && msgLower.contains('success'));

      if (ok) {
        if (isPunchIn) {
          // Persist open session so refresh/status sync cannot drop the timer.
          punchInTimeNotifier.value = punchTime;
          isClockedInNotifier.value = true;
          isPunchedOutForTodayNotifier.value = false;
          try {
            final prefs = await SharedPreferences.getInstance();
            await prefs.remove('auto_punched_out');
            await prefs.remove('auto_punched_out_date');
            await prefs.setString(
              'local_open_punch_in',
              punchTime.toIso8601String(),
            );
          } catch (_) {}
          try {
            // Required so GPS-off auto punch-out can run again after prior punch-out.
            await bg_tracking.LocationService.instance.resetPunchOutStatus();
            await bg_tracking.LocationService.instance.startTracking();
            await GpsMonitorService.instance.startMonitoring();
          } catch (e) {
            print('Error starting background tracking/GPS monitor: $e');
          }
        } else {
          // Punch-out ends the open session — always allow another punch-in.
          await _clearLocalOpenSession();
        }
        // Refresh lists only; do not re-sync status in a way that clears timer.
        if (!isPunchIn) {
          triggerRefresh();
        }
        return (success: true, message: message);
      }

      // Server already punched out (auto/GPS) — sync app to clocked-out.
      if (!isPunchIn && msgLower.contains('already')) {
        await _clearLocalOpenSession();
        triggerRefresh();
        return (success: true, message: 'Already punched out. Status synced.');
      }

      // Keep UI clocked out if server rejects re-punch after a completed session.
      if (isPunchIn && msgLower.contains('already')) {
        await getTodayStatus();
        return (success: false, message: message);
      }

      return (
        success: false,
        message: message.isNotEmpty ? message : 'Punch failed (${response.statusCode})',
      );
    } catch (e) {
      if (e is http.ClientException || e.toString().contains('SocketException')) {
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
        if (isPunchIn) {
          punchInTimeNotifier.value = punchTime;
          isClockedInNotifier.value = true;
          isPunchedOutForTodayNotifier.value = false;
          try {
            final prefs = await SharedPreferences.getInstance();
            await prefs.setString(
              'local_open_punch_in',
              punchTime.toIso8601String(),
            );
            await bg_tracking.LocationService.instance.resetPunchOutStatus();
            await bg_tracking.LocationService.instance.startTracking();
            await GpsMonitorService.instance.startMonitoring();
          } catch (_) {}
        } else {
          await _clearLocalOpenSession();
        }
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

        final response = await AuthenticatedHttp.post(
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

      final response = await AuthenticatedHttp.get(
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

      final response = await AuthenticatedHttp.get(
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

      final response = await AuthenticatedHttp.get(
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

      // Restore local open session before server sync (multi punch-in).
      try {
        final prefs = await SharedPreferences.getInstance();
        final localOpen = prefs.getString('local_open_punch_in');
        if (localOpen != null && localOpen.isNotEmpty) {
          final localIn = DateTime.tryParse(localOpen);
          if (localIn != null) {
            _setPunchInTimeIfChanged(localIn);
            isClockedInNotifier.value = true;
            isPunchedOutForTodayNotifier.value = false;
          }
        }
      } catch (_) {}

      final response = await getAttendanceStatus(userId: userId, date: DateTime.now());
      if (response != null && response.success) {
        final data = response.data;
        if (data != null) {
          if (data.punchIn != null && data.punchOut == null) {
            isClockedInNotifier.value = true;
            _setPunchInTimeIfChanged(data.punchIn);
            isPunchedOutForTodayNotifier.value = false;
            try {
              final prefs = await SharedPreferences.getInstance();
              await prefs.setString(
                'local_open_punch_in',
                data.punchIn!.toIso8601String(),
              );
            } catch (_) {}
            try {
              bg_tracking.LocationService.instance.startTracking();
            } catch (e) {}
          } else if (data.punchIn != null && data.punchOut != null) {
            final localIn = punchInTimeNotifier.value;
            // Keep only a NEW local session started after server's last punch-out.
            final keepLocal = isClockedInNotifier.value &&
                localIn != null &&
                localIn.isAfter(data.punchOut!);

            if (keepLocal) {
              print('📋 Keeping local open session punchIn=$localIn '
                  '(server punchOut=${data.punchOut})');
            } else {
              // Server already closed this session (manual/auto punch-out).
              print('📋 Clearing local session — server punchOut=${data.punchOut} '
                  'localIn=$localIn');
              await _clearLocalOpenSession();
            }
          } else {
            // No server attendance — keep local open session if present.
            if (!(isClockedInNotifier.value && punchInTimeNotifier.value != null)) {
              isClockedInNotifier.value = false;
              punchInTimeNotifier.value = null;
              isPunchedOutForTodayNotifier.value = false;
              try {
                bg_tracking.LocationService.instance.stopTracking();
              } catch (e) {}
            }
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

      final response = await AuthenticatedHttp.get(
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

      final response = await AuthenticatedHttp.get(
        Uri.parse(BaseUrls.geofencingByTenant),
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final decoded = jsonDecode(response.body);
        if (decoded is Map<String, dynamic>) {
          if (decoded.containsKey('data') && decoded['data'] != null) {
            return GeofencingModel.fromJson(decoded['data']);
          }
          return GeofencingModel.fromJson(decoded);
        }
      }
    } catch (e) {
      print('GEOFENCING ERROR: $e');
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
    final distanceInMeters = Geolocator.distanceBetween(currentLat, currentLng, branchLat, branchLng);
    return distanceInMeters <= radius;
  }

  /// Effective geofence radius: dashboard Location Tracking value wins when > 0,
  /// otherwise site geofencing API radius.
  static double resolveGeofenceRadius(double? siteRadius) {
    final dashboard = LocationConfig.GEOFENCE_RADIUS_METERS;
    if (dashboard > 0) return dashboard;
    if (siteRadius != null && siteRadius > 0) return siteRadius;
    return 100.0;
  }

  static Future<void> _clearLocalOpenSession() async {
    isClockedInNotifier.value = false;
    punchInTimeNotifier.value = null;
    isPunchedOutForTodayNotifier.value = false;
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.remove('auto_punched_out');
      await prefs.remove('auto_punched_out_date');
      await prefs.remove('local_open_punch_in');
    } catch (_) {}
    try {
      await GpsMonitorService.instance.stopMonitoring();
    } catch (_) {}
    try {
      bg_tracking.LocationService.instance.stopTracking();
    } catch (_) {}
  }

  /// Auto punch-out (GPS off / gap / headless). Always `Manual=false`.
  /// Pass last-known lat/lng — live GPS is often unavailable.
  static Future<({bool success, String? message})> submitAutoPunchOut({
    required String punchOutReason,
    DateTime? punchTime,
    double? latitude,
    double? longitude,
  }) async {
    final token = await TokenStorage.getToken();
    if (token == null) return (success: false, message: 'Token Missing');

    final userId = await _getUserId();
    if (userId == null) return (success: false, message: 'User ID Missing');

    final when = punchTime ?? DateTime.now();
    final dateStr = "${when.year.toString().padLeft(4, '0')}-"
        "${when.month.toString().padLeft(2, '0')}-"
        "${when.day.toString().padLeft(2, '0')}";
    final timeStr = DateFormat("yyyy-MM-dd'T'HH:mm:ss").format(when);

    double lat = latitude ?? 0.0;
    double lng = longitude ?? 0.0;
    if (latitude == null || longitude == null) {
      try {
        final prefs = await SharedPreferences.getInstance();
        final cached = prefs.getString('last_known_location');
        if (cached != null) {
          final m = jsonDecode(cached);
          lat = (m['latitude'] as num?)?.toDouble() ?? lat;
          lng = (m['longitude'] as num?)?.toDouble() ?? lng;
        }
      } catch (_) {}
    }

    try {
      final request = http.MultipartRequest('POST', Uri.parse(BaseUrls.punchOut));
      request.headers.addAll({
        'accept': '*/*',
        'Authorization': 'Bearer $token',
      });

      // Proven curl shape: Manual=false + PunchOutReason → timeline manual:false.
      request.fields['userId'] = userId.toString();
      request.fields['attendance_date'] = dateStr;
      request.fields['punch_out_time'] = timeStr;
      request.fields['longitude'] = lng.toString();
      request.fields['latitude'] = lat.toString();
      request.fields['Manual'] = 'false';
      request.fields['PunchOutReason'] = punchOutReason;
      request.files.add(
        http.MultipartFile.fromBytes('image', const <int>[], filename: ''),
      );

      print('📤 AUTO PUNCH-OUT URL => ${BaseUrls.punchOut}');
      print('📤 AUTO PUNCH-OUT FIELDS => ${request.fields}');

      final streamedResponse = await request.send();
      final response = await http.Response.fromStream(streamedResponse);
      print('📥 AUTO PUNCH-OUT => ${response.statusCode} ${response.body}');

      Map<String, dynamic> decoded = {};
      try {
        final body = jsonDecode(response.body);
        if (body is Map<String, dynamic>) decoded = body;
      } catch (_) {}

      final message = decoded['message']?.toString() ??
          (response.body.isNotEmpty ? response.body : 'Auto punch-out failed');
      final msgLower = message.toLowerCase();
      final ok = decoded['success'] == true ||
          msgLower.contains('successful') ||
          msgLower.contains('already') ||
          (response.statusCode == 200 && msgLower.contains('success'));

      if (ok) {
        await syncAutoPunchedOut();
        try {
          await bg_tracking.LocationService.instance.stopTracking();
        } catch (_) {}
        return (success: true, message: message);
      }
      return (success: false, message: message);
    } catch (e) {
      return (success: false, message: e.toString());
    }
  }

  /// Called by GPS auto punch-out so UI/timer sync immediately.
  static Future<void> syncAutoPunchedOut() async {
    isClockedInNotifier.value = false;
    punchInTimeNotifier.value = null;
    isPunchedOutForTodayNotifier.value = false;
    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.remove('local_open_punch_in');
      final today =
          '${DateTime.now().year}-${DateTime.now().month.toString().padLeft(2, '0')}-${DateTime.now().day.toString().padLeft(2, '0')}';
      await prefs.setBool('auto_punched_out', true);
      await prefs.setString('auto_punched_out_date', today);
      await prefs.setBool('user_punched_out', true);
      await prefs.setBool('attendance_session_active', false);
      await prefs.remove('pending_terminate_punch_out');
      await prefs.remove('pending_terminate_reason');
      await prefs.remove('pending_terminate_at');
    } catch (_) {}
    try {
      await GpsMonitorService.instance.stopMonitoring();
    } catch (_) {}
    triggerRefresh();
  }
}