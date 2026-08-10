import 'dart:convert';
import '../../../../feature/Dispute/dispute_category.dart';
import '../../Urls/urls.dart';
import '../token_storage.dart';
import '../authenticated_http.dart';

class DisputeService {
  static Future<List<DisputeCategory>> fetchCategories() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) throw Exception("Token Missing");

      final response = await AuthenticatedHttp.get(
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
    int? punchId,
    DateTime? requestedPunchInTime,
    DateTime? requestedPunchOutTime,
  }) async {
    try {
      final token = await TokenStorage.getToken();
      final userId = await TokenStorage.getUserId();

      if (token == null || userId == null) {
        throw Exception("Authentication required");
      }

      final dateStr = disputeDate.toIso8601String();

      final body = <String, dynamic>{
        "userId": userId,
        "disputeCategoryId": categoryId,
        "disputeDate": dateStr,
        "description": description,
        "punchId": punchId ?? 0,
      };

      if (requestedPunchInTime != null) {
        body["requestedPunchInTime"] = requestedPunchInTime.toIso8601String();
      }
      if (requestedPunchOutTime != null) {
        body["requestedPunchOutTime"] = requestedPunchOutTime.toIso8601String();
      }

      print("DISPUTE BODY: ${jsonEncode(body)}");

      final response = await AuthenticatedHttp.post(
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

