import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../../../feature/Dispute/dispute_category.dart';
import '../../Urls/urls.dart';
import '../token_storage.dart';

class DisputeService {
  static Future<List<DisputeCategory>> fetchCategories() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) throw Exception("Token Missing");

      final response = await http.get(
        Uri.parse(BaseUrls.disputeCategories),
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final decoded = jsonDecode(response.body);
        if (decoded['success'] == true) {
          final List list = decoded['data'] ?? [];
          return list.map((e) => DisputeCategory.fromJson(e)).toList();
        }
      }
      return [];
    } catch (e) {
      print("CATEGORY ERROR: $e");
      return [];
    }
  }

  static Future<void> createDispute({
    required DateTime disputeDate,
    required String description,
    required int categoryId,
  }) async {
    try {
      final token = await TokenStorage.getToken();
      final userId = await TokenStorage.getUserId();

      if (token == null || userId == null) {
        throw Exception("Authentication required");
      }

      final dateStr = disputeDate.toUtc().toIso8601String();

      final body = {
        "userId": userId,
        "disputeCategoryId": categoryId,
        "disputeDate": dateStr,
        "description": description
      };

      print("DISPUTE BODY: ${jsonEncode(body)}");

      final response = await http.post(
        Uri.parse(BaseUrls.createDispute),
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode(body),
      );

      print("DISPUTE RESPONSE CODE: ${response.statusCode}");
      print("DISPUTE RESPONSE BODY: ${response.body}");

      if (response.statusCode == 200 || response.statusCode == 201) {
        final decoded = jsonDecode(response.body);
        if (decoded is Map && decoded.containsKey('success') && decoded['success'] == false) {
          throw Exception(decoded['message'] ?? 'API Success False');
        }
        return;
      }

      throw Exception('HTTP ${response.statusCode}: ${response.body}');
    } catch (e) {
      print("DISPUTE ERROR: $e");
      throw Exception(e.toString().replaceAll('Exception:', '').trim());
    }
  }
}
