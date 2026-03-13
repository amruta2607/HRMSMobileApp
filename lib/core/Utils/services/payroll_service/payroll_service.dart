import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../../../../feature/payroll/model/provident_fund_model.dart';
import '../../../../feature/payroll/model/monthly_summary_model.dart';
import '../../Urls/urls.dart';
import '../token_storage.dart';
import 'dart:io';
import 'package:path_provider/path_provider.dart';
import 'package:open_file/open_file.dart';
import '../../../../feature/payroll/model/pay_slip_model.dart';

class PayrollService {
  static Future<int?> _getUserId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('userId');
  }

  static Future<int?> _getOrgId() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt('organisationId');
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

  static Future<MonthlySummaryModel?> getMonthlySummary({
    required int month,
    required int year,
  }) async {
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

      final uri = Uri.parse(
          '${BaseUrls.monthlySummary}?user=$userId&month=$month&year=$year');

      print(' MONTHLY SUMMARY API URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print(' MONTHLY SUMMARY STATUS => ${response.statusCode}');
      print(' MONTHLY SUMMARY RESPONSE => ${response.body}');

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
        return MonthlySummaryModel.fromJson(decoded['data']);
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

  static Future<LastMonthPayrollModel?> getLastMonthPayroll() async {
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

      final uri = Uri.parse('${BaseUrls.lastMonthPayroll}?user=$userId');

      print(' LAST MONTH PAYROLL API URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print(' LAST MONTH PAYROLL STATUS => ${response.statusCode}');
      print(' LAST MONTH PAYROLL RESPONSE => ${response.body}');

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
        return LastMonthPayrollModel.fromJson(decoded);
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

  /// Returns the most recent PaySlipModel available for the user.
  /// Tries current year first, then falls back to the previous year.
  static Future<PaySlipModel?> getLatestPaySlip() async {
    final currentYear = DateTime.now().year;

    List<PaySlipModel>? slips = await getPaySlips(year: currentYear);

    if (slips == null || slips.isEmpty) {
      slips = await getPaySlips(year: currentYear - 1);
    }

    if (slips == null || slips.isEmpty) return null;

    // Sort descending to get the most recent payslip
    slips.sort((a, b) {
      final yearCmp = b.payrollYear.compareTo(a.payrollYear);
      return yearCmp != 0 ? yearCmp : b.payrollMonth.compareTo(a.payrollMonth);
    });

    return slips.first;
  }

  static Future<List<PaySlipModel>?> getPaySlips({int? year}) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        print(' PAYROLL SERVICE: Token is NULL');
        return null;
      }

      final userId = await _getUserId();
      final orgId = await _getOrgId();

      if (userId == null || orgId == null) {
        print(' PAYROLL SERVICE: userId or orgId is NULL');
        return null;
      }

      // Default to current year if not provided
      final queryYear = year ?? DateTime.now().year;

      var uriString = '${BaseUrls.paySlipList}?user=$userId&organization=$orgId&year=$queryYear';
      final uri = Uri.parse(uriString);

      print(' PAY SLIP LIST API URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print(' PAY SLIP LIST STATUS => ${response.statusCode}');

      if (response.statusCode == 401) {
        await TokenStorage.logoutAndNavigate();
        return null;
      }

      if (response.statusCode == 200) {
        final decoded = jsonDecode(response.body);
        if (decoded['success'] == true) {
          final List<dynamic> data = decoded['data'];
          return data.map((json) => PaySlipModel.fromJson(json)).toList();
        }
      }
      return null;

    } catch (e, s) {
      print(' PAYROLL SERVICE ERROR (getPaySlips) => $e');
      print(' STACKTRACE => $s');
      return null;
    }
  }

  static Future<bool> downloadPaySlip({
    required int month,
    required int year,
    required String fileName,
  }) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) return false;

      final userId = await _getUserId();
      if (userId == null) return false;

      final uri = Uri.parse(
          '${BaseUrls.downloadPaySlip}?user=$userId&month=$month&year=$year');

      print(' DOWNLOAD PAY SLIP API URL => $uri');

      final response = await http.get(
        uri,
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final dir = await getApplicationDocumentsDirectory();
        final file = File('${dir.path}/$fileName');
        await file.writeAsBytes(response.bodyBytes);

        print(' FILE SAVED => ${file.path}');

        final result = await OpenFile.open(file.path);
        print(' OPEN FILE RESULT => ${result.type}');

        return result.type == ResultType.done;
      }

      print(' DOWNLOAD FAILED => ${response.statusCode}');
      return false;

    } catch (e, s) {
      print(' PAYROLL SERVICE ERROR (downloadPaySlip) => $e');
      print(' STACKTRACE => $s');
      return false;
    }
  }

  static Future<List<int>?> getPaySlipYears() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) return null;

      final response = await http.get(
        Uri.parse(BaseUrls.paySlipYears),
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final decoded = jsonDecode(response.body);
        if (decoded['success'] == true) {
          return List<int>.from(decoded['years']);
        }
      }
      return null;
    } catch (e) {
      print(' PAYROLL SERVICE ERROR (getPaySlipYears) => $e');
      return null;
    }
  }

  static Future<List<PaySlipMonthModel>?> getPaySlipMonths(int year) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) return null;

      final response = await http.get(
        Uri.parse('${BaseUrls.paySlipMonths}?year=$year'),
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final decoded = jsonDecode(response.body);
        if (decoded['success'] == true) {
          final List<dynamic> data = decoded['months'];
          return data.map((json) => PaySlipMonthModel.fromJson(json)).toList();
        }
      }
      return null;
    } catch (e) {
      print(' PAYROLL SERVICE ERROR (getPaySlipMonths) => $e');
      return null;
    }
  }
}

