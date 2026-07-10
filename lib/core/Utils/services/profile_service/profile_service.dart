import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:http_parser/http_parser.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../../../../feature/Profile/model/profile_model.dart';
import '../../Urls/urls.dart';
import '../token_storage.dart';
import '../authenticated_http.dart';

class ProfileService {
  static Future<ProfileModel?> fetchProfile() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        print(' PROFILE: Token is NULL');
        return null;
      }

      final prefs = await SharedPreferences.getInstance();
      final userId = prefs.getInt('userId');
      if (userId == null) {
        print(' PROFILE: userId is NULL');
        return null;
      }

      final url = '${BaseUrls.profileByUser}/$userId';
      print(' PROFILE API URL => $url');

      final response = await AuthenticatedHttp.get(
        Uri.parse(url),
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 401) {
        print(' PROFILE: 401 returned (logout handled centrally if auth rejected)');
        return null;
      }

      if (response.statusCode != 200) {
        print(' PROFILE: Non-200 status');
        return null;
      }

      final decoded = jsonDecode(response.body);

      if (decoded['success'] != true) {
        print(' PROFILE: success=false, message=${decoded['message']}');
        return null;
      }

      return ProfileModel.fromJson(decoded['data']);
    } catch (e, s) {
      if (e.toString().contains('SocketException') || e.toString().contains('Network is unreachable')) {
        print('🌐 [PROFILE SERVICE] Offline: Cannot fetch profile.');
      } else {
        print(' PROFILE FETCH ERROR => $e');
        print('STACKTRACE => $s');
      }
      return null;
    }
  }

  static Future<bool> updateProfile({
    String? phone,
    File? pictureFile,
  }) async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) {
        print(' UPDATE PROFILE: Token is NULL');
        return false;
      }

      final prefs = await SharedPreferences.getInstance();
      final userId = prefs.getInt('userId');
      if (userId == null) {
        print(' UPDATE PROFILE: userId is NULL');
        return false;
      }

      // Use the correct PUT endpoint
      final url = Uri.parse('${BaseUrls.base}/api/personal-details');
      print(' UPDATE PROFILE API URL => $url');

      var request = http.MultipartRequest('PUT', url);

      // Add headers
      request.headers['accept'] = '*/*';
      request.headers['Authorization'] = 'Bearer $token';

      // Add UserId (always required)
      request.fields['UserId'] = userId.toString();

      // Add phone only if provided and not empty
      if (phone != null && phone.trim().isNotEmpty) {
        request.fields['Phone'] = phone.trim();
        print(' UPDATE PROFILE: Phone = $phone');
      }

      // Add picture file only if provided
      if (pictureFile != null) {
        final extension = pictureFile.path.split('.').last.toLowerCase();
        String mimeType = 'image/jpeg';
        if (extension == 'png') {
          mimeType = 'image/png';
        }

        request.files.add(
          await http.MultipartFile.fromPath(
            'Picture',
            pictureFile.path,
            contentType: MediaType.parse(mimeType),
          ),
        );
        print(' UPDATE PROFILE: Added image file');
      }

      print(' UPDATE PROFILE: Sending request...');
      final streamedResponse = await request.send();
      final response = await http.Response.fromStream(streamedResponse);

      print(' UPDATE PROFILE STATUS => ${response.statusCode}');
      print(' UPDATE PROFILE RESPONSE => ${response.body}');

      if (response.statusCode == 401) {
        print(' UPDATE PROFILE: 401 returned (logout handled centrally if auth rejected)');
        return false;
      }

      if (response.statusCode == 200) {
        print(' UPDATE PROFILE: Success');
        return true;
      }

      print(' UPDATE PROFILE: Failed with status ${response.statusCode}');
      return false;
    } catch (e, s) {
      print(' UPDATE PROFILE ERROR => $e');
      print(' STACKTRACE => $s');
      return false;
    }
  }
}
