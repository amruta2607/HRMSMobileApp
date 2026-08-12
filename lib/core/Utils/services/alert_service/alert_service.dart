import 'dart:convert';
import '../../Urls/urls.dart';
import '../token_storage.dart';
import '../authenticated_http.dart';
import 'package:altroz/feature/alerts/model/alert_model.dart';

class AlertService {
  static Set<int> _taskTemplateIds = {};
  static final Set<int> _defaultTaskTemplateIds = {
    33,   // LeaveRequest
    38,   // OvertimeRequest
    43,   // ReimbursementRequest
    48,   // ResignationRequest
    53,   // CancelLeave
    58,   // PayrollSubmission
    1061, // RequisitionRequest
    1066, // RegularizationRequest
    1478, // TicketRequest
  };

  static Future<Set<int>> getTemplates() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) return _taskTemplateIds;

      final url = Uri.parse(BaseUrls.templates);
      final response = await AuthenticatedHttp.get(
        url,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> decoded = jsonDecode(response.body);
        _taskTemplateIds = decoded.map<int>((item) => (item['id'] as num).toInt()).toSet();
        print('DEBUG: Fetched ${_taskTemplateIds.length} Task Templates from API => $_taskTemplateIds');
      }
    } catch (e) {
      print('GET TEMPLATES ERROR => $e');
    }
    return _taskTemplateIds;
  }

  static bool isTask(AlertModel a) {
    if (_taskTemplateIds.isNotEmpty) {
      if (_taskTemplateIds.contains(a.eventId)) return true;
    }
    if (_defaultTaskTemplateIds.contains(a.eventId)) return true;

    // Fallback to title matching
    return a.title.contains("Leave Request") ||
        a.title.contains("Payroll Submitted") ||
        a.title.contains("Reimbursement Request") ||
        a.title.contains("Resignation Request") ||
        a.title.contains("Cancel Leave Request") ||
        a.title.contains("Cancel Payroll Request") ||
        a.title.contains("Overtime Request") ||
        a.title.contains("Regularization Request") ||
        a.title.contains("Requisition Request") ||
        a.title.contains("Ticket Request");
  }

  static Future<Map<String, dynamic>?> getAlerts() async {
    try {
      if (_taskTemplateIds.isEmpty) {
        await getTemplates();
      }

      final token = await TokenStorage.getToken();
      if (token == null) return null;

      final url = Uri.parse(BaseUrls.alerts);

      final response = await AuthenticatedHttp.get(
        url,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 401) {
        return null;
      }

      if (response.statusCode == 200) {
        // print('DEBUG: Raw Alerts Response => ${response.body}');
        final decoded = jsonDecode(response.body);
        if (decoded['success'] == true) {
          final List<dynamic> data = decoded['data'];
          final alerts = data.map((json) => AlertModel.fromJson(json)).toList();
          return {
            'alerts': alerts,
            'unreadCount': decoded['unreadCount'] ?? 0,
          };
        }
      }
      return null;
    } catch (e) {
      print('ALERT SERVICE ERROR => $e');
      return null;
    }
  }

  static Future<Map<String, dynamic>> processRequest({
    required int alertId,
    required int eventId,
    required String eventName,
    required String reason,
    required bool isApprove,
    required int insertUserId,
  }) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) return {'success': false, 'message': 'Auth token missing'};

      final url = Uri.parse(isApprove ? BaseUrls.approveAlert : BaseUrls.rejectAlert);

      final body = {
        "alertId": alertId,
        "eventId": eventId,
        "eventName": eventName,
        "reason": reason,
        "insertUserId": insertUserId,
      };

      print('DEBUG: Sending Approval/Rejection Payload => ${jsonEncode(body)}');

      final response = await AuthenticatedHttp.put(
        url,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode(body),
      );

      if (response.statusCode == 401) {
        return {'success': false, 'message': 'Session expired'};
      }

      final decoded = jsonDecode(response.body);
      print('DEBUG: Process Request Response => $decoded');
      return {
        'success': decoded['success'] == true || response.statusCode == 200,
        'message': decoded['message'] ?? (isApprove ? 'Approved' : 'Rejected'),
      };
    } catch (e) {
      return {'success': false, 'message': 'Error: $e'};
    }
  }

  static Future<bool> markAsRead(int alertId) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) return false;

      final url = Uri.parse(BaseUrls.markReadAlert);
      final body = {
        "id": alertId,
        "updateUserId": 0
      };

      final response = await AuthenticatedHttp.put(
        url,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode(body),
      );

      print('DEBUG: Mark As Read Response (${response.statusCode}) => ${response.body}');

      if (response.statusCode == 200) {
        final decoded = jsonDecode(response.body);
        return decoded['success'] == true;
      }
      return false;
    } catch (e) {
      print('MARK AS READ ERROR => $e');
      return false;
    }
  }
}
