import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../Urls/urls.dart';
import '../token_storage.dart';
import '../authenticated_http.dart';

class TenantService {
  static Future<String?> getCompanyLogo() async {
    try {
      final token = await TokenStorage.getToken();
      if (token == null) return null;

      final response = await AuthenticatedHttp.get(
        Uri.parse(BaseUrls.companyLogo),
        headers: {
          'accept': '*/*',
          'Authorization': 'Bearer $token',
        },
      );

      print(' [COMPANY LOGO] Response body: ${response.body}');

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        if (data['success'] == true && data['data'] != null) {
          final logoPath = data['data']['logo'];
          if (logoPath != null && logoPath.isNotEmpty) {
            // Prepend base URL if it's a relative path
            if (logoPath.startsWith('http')) {
              return logoPath;
            }
            // Use the same base URL as the API and add /upload/
            final uri = Uri.parse(BaseUrls.companyLogo);
            final baseUrl = "${uri.scheme}://${uri.host}${uri.hasPort ? ':${uri.port}' : ''}";
            return '$baseUrl/upload/$logoPath';
          }
        }
      }
    } catch (e) {
      if (e.toString().contains('SocketException') || e.toString().contains('Network is unreachable')) {
        print('🌐 [TENANT SERVICE] Offline: Cannot fetch company logo.');
      } else {
        print('🔴 TENANT SERVICE ERROR => $e');
      }
    }
    return null;
  }
}
