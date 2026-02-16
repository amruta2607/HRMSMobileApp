import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../../../../feature/payroll/model/provident_fund_model.dart';
import '../../Urls/urls.dart';
import '../token_storage.dart';

class PayrollService {
  static Future<int?> _getUserId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('userId');
  }

  static Future<ProvidentFundModel?> getProvidentFund() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        print(' PAYROLL SERVICE: Token is NULL');
        return null;
      }

      final userId = await _getUserId();
      if (userId == null) {
        print(' PAYROLL SERVICE: userId is NULL');
        return null;
      }

      final uri = Uri.parse('${BaseUrls.providentFund}?user=$userId');

      print(' PROVIDENT FUND API URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print(' PROVIDENT FUND STATUS => ${response.statusCode}');
      print(' PROVIDENT FUND RESPONSE => ${response.body}');

      if (response.statusCode == 401) {
        print(' PAYROLL SERVICE: Token expired, logging out');
        await TokenStorage.logoutAndNavigate();
        return null;
      }

      if (response.statusCode != 200) {
        print(' PAYROLL SERVICE: Non-200 status');
        return null;
      }

      final decoded = jsonDecode(response.body);

      if (decoded['success'] == true) {
        return ProvidentFundModel.fromJson(decoded['data']);
      } else {
        print(' PAYROLL SERVICE: API returned success=false');
        return null;
      }

    } catch (e, s) {
      print(' PAYROLL SERVICE ERROR => $e');
      print(' STACKTRACE => $s');
      return null;
    }
  }
}
