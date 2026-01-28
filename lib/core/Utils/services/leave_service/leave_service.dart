import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../../../../feature/leave/model/leave_balence_model.dart';
import '../../Urls/urls.dart';
import '../token_storage.dart';

class LeaveService {
  static Future<int?> _getUserId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('userId');
  }

  static Future<int?> _getOrgId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('organisationId');
  }

  static Future<List<LeaveBalanceModel>?> getLeaveBalance() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        print(' LEAVE SERVICE: Token is NULL');
        return null;
      }

      final userId = await _getUserId();
      final orgId = await _getOrgId();

      if (userId == null || orgId == null) {
        print(' LEAVE SERVICE: userId or orgId is NULL');
        return null;
      }

      final uri = Uri.parse(
        '${BaseUrls.leaveBalance}?user=$userId&organization=$orgId',
      );

      print(' LEAVE BALANCE API URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print(' LEAVE BALANCE STATUS => ${response.statusCode}');
      print(' LEAVE BALANCE RESPONSE => ${response.body}');

      if (response.statusCode == 401) {
        print(' LEAVE SERVICE: Token expired, logging out');
        await TokenStorage.logoutAndNavigate();
        return null;
      }

      if (response.statusCode != 200) {
        print(' LEAVE SERVICE: Non-200 status');
        return null;
      }

      final decoded = jsonDecode(response.body);

      if (decoded['success'] == true) {
        final List<dynamic> data = decoded['data'];
        return data.map((json) => LeaveBalanceModel.fromJson(json)).toList();
      } else {
        print('🔴 LEAVE SERVICE: API returned success=false');
        return null;
      }

    } catch (e, s) {
      print(' LEAVE SERVICE ERROR => $e');
      print(' STACKTRACE => $s');
      return null;
    }
  }

  static Future<bool> submitLeaveApplication({
    required int leaveTypeId,
    required DateTime startDate,
    required DateTime endDate,
    required String reason,
    required bool isHalfDay,
    String? attachmentPath,
  }) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        print(' APPLY LEAVE: Token is NULL');
        return false;
      }

      final userId = await _getUserId();
      final orgId = await _getOrgId();

      if (userId == null || orgId == null) {
        print(' APPLY LEAVE: userId or orgId is NULL');
        return false;
      }

      // Prepare attachment string (Base64) if provided
      String attachmentString = "string"; // Default as per curl example if empty?
      // actually curl says "attachment": "string". If real file, likely base64.
      // If no file, maybe empty string or null?
      // Let's assume if file exists, we convert. Else "string" or "" to be safe with the API expectation.

      if (attachmentPath != null && attachmentPath.isNotEmpty) {
        try {
          final bytes = await File(attachmentPath).readAsBytes();
          attachmentString = base64Encode(bytes);
        } catch (e) {
          print('🔴 APPLY LEAVE: File read error: $e');
        }
      }

      final url = Uri.parse(BaseUrls.applyLeave);

      final body = {
        "organization": orgId,
        "leave_type": leaveTypeId,
        "startdate": startDate.toIso8601String(),
        "enddate": endDate.toIso8601String(),
        "is_half_day": isHalfDay,
        "duration": 0, // As per curl
        "reason": reason,
        "attachment": attachmentString,
        "user": userId,
      };

      print('🔵 APPLY LEAVE API URL => $url');
      print('🔵 APPLY LEAVE BODY => ${jsonEncode(body)}');

      final response = await http.post(
        url,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode(body),
      );

      print('🔵 APPLY LEAVE STATUS => ${response.statusCode}');
      print('🔵 APPLY LEAVE RESPONSE => ${response.body}');

      if (response.statusCode == 401) {
        await TokenStorage.logoutAndNavigate();
        return false;
      }

      if (response.statusCode == 200 || response.statusCode == 201) {
        final decoded = jsonDecode(response.body);
        // Check manually if success is inside or just implied by 200
        // The curl example response format wasn't explicitly pasted but usually it follows existing pattern
        if (decoded is Map<String, dynamic>) {
          if (decoded.containsKey('success')) {
            return decoded['success'] == true;
          }
        }
        return true;
      }

      return false;

    } catch (e, s) {
      print('🔴 APPLY LEAVE ERROR => $e');
      print('🔴 STACKTRACE => $s');
      return false;
    }
  }

  // --- Local Persistence for Recent Leaves ---
  static const String _recentLeavesKey = 'recent_leaves_local';

  static Future<List<Map<String, String>>> getRecentLeaves() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final String? jsonString = prefs.getString(_recentLeavesKey);

      if (jsonString == null) {
        return [];
      }

      final List<dynamic> decoded = jsonDecode(jsonString);
      return decoded.map((e) => Map<String, String>.from(e)).toList();
    } catch (e) {
      print('🔴 LEAVE SERVICE: Error loading recent leaves: $e');
      return [];
    }
  }

  static Future<void> saveRecentLeave(Map<String, String> leaveData) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      List<Map<String, String>> currentList = await getRecentLeaves();

      // Add to top
      currentList.insert(0, leaveData);

      // Limit to last 10 or 20 to avoid unlimited growth
      if (currentList.length > 20) {
        currentList = currentList.sublist(0, 20);
      }

      await prefs.setString(_recentLeavesKey, jsonEncode(currentList));
    } catch (e) {
      print('🔴 LEAVE SERVICE: Error saving recent leave: $e');
    }
  }
}
