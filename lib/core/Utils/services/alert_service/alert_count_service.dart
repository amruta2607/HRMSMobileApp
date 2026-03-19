import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import '../../Urls/urls.dart';
import '../token_storage.dart';

class AlertCountService {
  static final ValueNotifier<int> alertCountNotifier = ValueNotifier<int>(0);

  static void updateCount(int count) {
    alertCountNotifier.value = count;
  }

  static Future<int> fetchCount() async {
    try {
      final token = await TokenStorage.getToken();

      if (token == null) return 0;

      final response = await http.get(
        Uri.parse(BaseUrls.alertCount),
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print("ALERT COUNT STATUS => ${response.statusCode}");
      print("ALERT COUNT RESPONSE => ${response.body}");

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success'] == true) {
          final count = (data['count'] as num?)?.toInt() ?? 0;
          alertCountNotifier.value = count;
          return count;
        }
      }

      return 0;
    } catch (e, s) {
      print("ALERT COUNT ERROR => $e");
      print("STACKTRACE => $s");
      return 0;
    }
  }
}