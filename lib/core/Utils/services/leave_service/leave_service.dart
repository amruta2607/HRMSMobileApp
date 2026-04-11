import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../../../../feature/leave/model/leave_balence_model.dart';
import '../../../../feature/leave/model/leave_reuest_model.dart';
import '../../../../feature/leave/model/leave_history_model.dart';
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
        print(' LEAVE SERVICE: API returned success=false');
        return null;
      }
    } catch (e, s) {
      print(' LEAVE SERVICE ERROR => $e');
      print(' STACKTRACE => $s');
      return null;
    }
  }

  static Future<Map<String, dynamic>> submitLeaveApplication({
    required int leaveTypeId,
    required DateTime startDate,
    required DateTime endDate,
    required String reason,
    required bool isHalfDay,
    required int duration,
    String? attachmentPath,
  }) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        print(' APPLY LEAVE: Token is NULL');
        return {'success': false, 'message': 'Authentication token is missing'};
      }

      final userId = await _getUserId();
      final orgId = await _getOrgId();

      if (userId == null || orgId == null) {
        print(' APPLY LEAVE: userId or orgId is NULL');
        return {
          'success': false,
          'message': 'User ID or Organization ID is missing'
        };
      }

      String attachmentString =
          "string"; // Default as per curl example if empty?

      if (attachmentPath != null && attachmentPath.isNotEmpty) {
        try {
          final bytes = await File(attachmentPath).readAsBytes();
          attachmentString = base64Encode(bytes);
        } catch (e) {
          print(' APPLY LEAVE: File read error: $e');
        }
      }

      final url = Uri.parse(BaseUrls.applyLeave);

      final body = {
        "organization": orgId,
        "leave_type": leaveTypeId,
        "startdate": startDate.toIso8601String(),
        "enddate": endDate.toIso8601String(),
        "is_half_day": isHalfDay,
        "duration": duration,
        "reason": reason,
        "attachment": attachmentString,
        "user": userId,
      };

      print(' APPLY LEAVE API URL => $url');
      print(' APPLY LEAVE BODY => ${jsonEncode(body)}');

      final response = await http.post(
        url,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode(body),
      );

      print('APPLY LEAVE STATUS => ${response.statusCode}');
      print(' APPLY LEAVE RESPONSE => ${response.body}');

      if (response.statusCode == 401) {
        await TokenStorage.logoutAndNavigate();
        return {
          'success': false,
          'message': 'Session expired. Please login again.'
        };
      }

      final decoded = jsonDecode(response.body);

      if (response.statusCode == 200 || response.statusCode == 201) {
        if (decoded is Map<String, dynamic>) {
          if (decoded.containsKey('success')) {
            return {
              'success': decoded['success'] == true,
              'message': decoded['message'] ?? 'Leave applied successfully',
            };
          }
        }
        return {'success': true, 'message': 'Leave applied successfully'};
      }

      return {
        'success': false,
        'message':
        decoded['message'] ?? 'Failed with status ${response.statusCode}'
      };
    } catch (e, s) {
      print(' APPLY LEAVE ERROR => $e');
      print(' STACKTRACE => $s');
      return {'success': false, 'message': 'An unexpected error occurred: $e'};
    }
  }

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
      print(' LEAVE SERVICE: Error loading recent leaves: $e');
      return [];
    }
  }

  static Future<void> saveRecentLeave(Map<String, String> leaveData) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      List<Map<String, String>> currentList = await getRecentLeaves();

      currentList.insert(0, leaveData);

      if (currentList.length > 20) {
        currentList = currentList.sublist(0, 20);
      }

      await prefs.setString(_recentLeavesKey, jsonEncode(currentList));
    } catch (e) {
      print(' LEAVE SERVICE: Error saving recent leave: $e');
    }
  }

  static Future<List<LeaveRequestModel>?> getLeaveRequests() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        print(' LEAVE REQUESTS: Token is NULL');
        return null; // Or empty list?
      }

      final userId = await _getUserId();
      final orgId = await _getOrgId();

      if (userId == null || orgId == null) {
        print(' LEAVE REQUESTS: userId or orgId is NULL');
        return null;
      }

      final uri = Uri.parse(
        '${BaseUrls.leaveRequests}?user_id=$userId&organization_id=$orgId',
      );

      print(' LEAVE REQUESTS API URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print(' LEAVE REQUESTS STATUS => ${response.statusCode}');
      print(' LEAVE REQUESTS RESPONSE => ${response.body}');

      if (response.statusCode == 401) {
        await TokenStorage.logoutAndNavigate();
        return null;
      }

      if (response.statusCode == 200) {
        final decoded = jsonDecode(response.body);
        if (decoded['success'] == true) {
          final List<dynamic> data = decoded['data'];
          print('DEBUG: Leave Requests IDs => ${data.map((e) => {"id": e['id'], "employeeName": e['employeeName'], "startDate": e['fromDate']}).toList()}');
          return data.map((json) => LeaveRequestModel.fromJson(json)).toList();
        }
      }
      return null;
    } catch (e, s) {
      print(' LEAVE REQUESTS ERROR => $e');
      print(' STACKTRACE => $s');
      return null;
    }
  }

  static Future<Map<String, dynamic>?> withdrawLeave({
    required int leaveId,
    required String reason,
  }) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        print(' WITHDRAW LEAVE: Token is NULL');
        return null;
      }

      final userId = await _getUserId();
      if (userId == null) {
        print(' WITHDRAW LEAVE: userId is NULL');
        return null;
      }

      final url = Uri.parse(BaseUrls.withdrawLeave);

      final body = {
        "id": leaveId,
        "userId": userId,
        "reason": reason,
      };

      print(' WITHDRAW LEAVE API URL => $url');
      print(' WITHDRAW LEAVE BODY => ${jsonEncode(body)}');

      final response = await http.put(
        url,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode(body),
      );

      print(' WITHDRAW LEAVE STATUS => ${response.statusCode}');
      print(' WITHDRAW LEAVE RESPONSE => ${response.body}');

      if (response.statusCode == 401) {
        await TokenStorage.logoutAndNavigate();
        return null;
      }

      if (response.statusCode == 200) {
        final decoded = jsonDecode(response.body);
        return decoded;
      }

      return null;
    } catch (e, s) {
      print(' WITHDRAW LEAVE ERROR => $e');
      print(' STACKTRACE => $s');
      return null;
    }
  }

  static Future<LeaveHistoryModel?> getLeaveHistory() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        print(' LEAVE HISTORY: Token is NULL');
        return null;
      }

      final url = Uri.parse(BaseUrls.leaveHistory);

      print(' LEAVE HISTORY API URL => $url');

      final response = await http.get(
        url,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print(' LEAVE HISTORY STATUS => ${response.statusCode}');
      // print(' LEAVE HISTORY RESPONSE => ${response.body}');

      if (response.statusCode == 401) {
        await TokenStorage.logoutAndNavigate();
        return null;
      }

      if (response.statusCode == 200) {
        final decoded = jsonDecode(response.body);
        if (decoded is Map<String, dynamic> && decoded['success'] == true) {
          return LeaveHistoryModel.fromJson(decoded);
        }
      }
      return null;
    } catch (e, s) {
      print(' LEAVE HISTORY ERROR => $e');
      print(' STACKTRACE => $s');
      return null;
    }
  }
}
