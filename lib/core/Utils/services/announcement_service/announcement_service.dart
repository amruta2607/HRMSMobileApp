import 'dart:convert';
import 'package:http/http.dart' as http;

import '../../../../feature/Announcement/model/announcement_model.dart';
import '../../Urls/urls.dart';
import '../token_storage.dart';

class AnnouncementService {
  static Future<List<AnnouncementModel>> getAnnouncements() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        throw Exception('Token is missing');
      }

      final response = await http.get(
        Uri.parse(BaseUrls.announcements),
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => AnnouncementModel.fromJson(json)).toList();
      } else if (response.statusCode == 401) {
        await TokenStorage.logoutAndNavigate();
        throw Exception('Session expired');
      } else {
        throw Exception('Failed to load announcements: ${response.statusCode}');
      }
    } catch (e) {
      print('ANNOUNCEMENT ERROR: $e');
      rethrow;
    }
  }
}
