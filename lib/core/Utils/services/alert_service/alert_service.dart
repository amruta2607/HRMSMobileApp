import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../Urls/urls.dart';
import '../token_storage.dart';
import 'package:altroz/feature/alerts/model/alert_model.dart';

class AlertService {
  static Future<List<AlertModel>?> getAlerts() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) return null;

      final url = Uri.parse(BaseUrls.alerts);

      final response = await http.get(
        url,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 401) {
        await TokenStorage.logoutAndNavigate();
        return null;
      }

      if (response.statusCode == 200) {
        final decoded = jsonDecode(response.body);
        if (decoded['success'] == true) {
          final List<dynamic> data = decoded['data'];
          return data.map((json) => AlertModel.fromJson(json)).toList();
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

      final response = await http.put(
        url,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode(body),
      );

      if (response.statusCode == 401) {
        await TokenStorage.logoutAndNavigate();
        return {'success': false, 'message': 'Session expired'};
      }

      final decoded = jsonDecode(response.body);
      return {
        'success': decoded['success'] == true || response.statusCode == 200,
        'message': decoded['message'] ?? (isApprove ? 'Approved' : 'Rejected'),
      };
    } catch (e) {
      return {'success': false, 'message': 'Error: $e'};
    }
  }
}
