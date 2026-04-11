import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:http_parser/http_parser.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../../../../feature/Profile/model/profile_model.dart';
import '../../Urls/urls.dart';
import '../token_storage.dart';

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

      final response = await http.get(
        Uri.parse(url),
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print(' PROFILE STATUS => ${response.statusCode}');
      print(' PROFILE RESPONSE => ${response.body}');

      if (response.statusCode == 401) {
        print(' PROFILE: Token expired, logging out');
        await TokenStorage.logoutAndNavigate();
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

      final profile = ProfileModel.fromJson(decoded['data']);

      print(' PROFILE FETCHED SUCCESSFULLY');
      print('    Name        : ${profile.name}');
      print('   Emp ID      : ${profile.empId}');
      print('   Email       : ${profile.email}');
      print('   Phone       : ${profile.phone}');
      print('   Designation : ${profile.designation}');
      print('   Picture     : ${profile.picture}');
      print('    address        : ${profile.address}');
      print('    reporting manager     : ${profile.reportingManager}');

      return profile;
    } catch (e, s) {
      print(' PROFILE FETCH ERROR => $e');
      print('STACKTRACE => $s');
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

      // Use the correct PUT endpoint (not the GET endpoint)
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
        print(' UPDATE PROFILE: Token expired, logging out');
        await TokenStorage.logoutAndNavigate();
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
