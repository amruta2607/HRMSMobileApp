import 'dart:convert';
import 'package:http/http.dart' as http;
import 'token_storage.dart';

/// A utility class that wraps HTTP calls with automatic token refresh.
/// 
/// When any API call returns 401 (Unauthorized), this client will:
/// 1. Automatically call the refresh-token API
/// 2. Retry the original request with the new access token
/// 3. Only fail if the refresh itself fails
/// 
/// Usage: Replace `http.get/post(...)` calls with `AuthenticatedHttp.get/post(...)`
class AuthenticatedHttp {
  /// GET request with auto token refresh on 401
  static Future<http.Response> get(
    Uri url, {
    Map<String, String>? headers,
  }) async {
    final token = await TokenStorage.getValidToken();
    final requestHeaders = {
      ...?headers,
      if (token != null) 'Authorization': 'Bearer $token',
    };

    var response = await http.get(url, headers: requestHeaders);

    // If 401, try refresh and retry
    if (response.statusCode == 401) {
      print('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
      print('🛡️ [AUTH HTTP] 401 Unauthorized caught on GET $url');
      print('🔄 Calling TokenStorage.refreshAccessToken() automatically...');
      print('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
      try {
        final newToken = await TokenStorage.refreshAccessToken();
        if (newToken != null) {
          requestHeaders['Authorization'] = 'Bearer $newToken';
          response = await http.get(url, headers: requestHeaders);
          print('✅ [AUTH HTTP] GET Silent retry completed! New Status: ${response.statusCode}');
        } else {
          print('❌ [AUTH HTTP] Refresh returned null on 401.');
        }
      } catch (e) {
        print('🌐 [AUTH HTTP] Network error during token refresh on 401: $e. Keeping session active.');
      }
    }

    return response;
  }

  /// POST request with auto token refresh on 401
  static Future<http.Response> post(
    Uri url, {
    Map<String, String>? headers,
    Object? body,
    Encoding? encoding,
  }) async {
    final token = await TokenStorage.getValidToken();
    final requestHeaders = {
      ...?headers,
      if (token != null) 'Authorization': 'Bearer $token',
    };

    var response = await http.post(
      url,
      headers: requestHeaders,
      body: body,
      encoding: encoding,
    );

    // If 401, try refresh and retry
    if (response.statusCode == 401) {
      print('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
      print('🛡️ [AUTH HTTP] 401 Unauthorized caught on POST $url');
      print('🔄 Calling TokenStorage.refreshAccessToken() automatically...');
      print('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
      try {
        final newToken = await TokenStorage.refreshAccessToken();
        if (newToken != null) {
          requestHeaders['Authorization'] = 'Bearer $newToken';
          response = await http.post(
            url,
            headers: requestHeaders,
            body: body,
            encoding: encoding,
          );
          print('✅ [AUTH HTTP] POST Silent retry completed! New Status: ${response.statusCode}');
        } else {
          print('❌ [AUTH HTTP] Refresh returned null on 401.');
        }
      } catch (e) {
        print('🌐 [AUTH HTTP] Network error during token refresh on 401: $e. Keeping session active.');
      }
    }

    return response;
  }

  /// PUT request with auto token refresh on 401
  static Future<http.Response> put(
    Uri url, {
    Map<String, String>? headers,
    Object? body,
    Encoding? encoding,
  }) async {
    final token = await TokenStorage.getValidToken();
    final requestHeaders = {
      ...?headers,
      if (token != null) 'Authorization': 'Bearer $token',
    };

    var response = await http.put(
      url,
      headers: requestHeaders,
      body: body,
      encoding: encoding,
    );

    // If 401, try refresh and retry
    if (response.statusCode == 401) {
      print('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
      print('🛡️ [AUTH HTTP] 401 Unauthorized caught on PUT $url');
      print('🔄 Calling TokenStorage.refreshAccessToken() automatically...');
      print('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
      try {
        final newToken = await TokenStorage.refreshAccessToken();
        if (newToken != null) {
          requestHeaders['Authorization'] = 'Bearer $newToken';
          response = await http.put(
            url,
            headers: requestHeaders,
            body: body,
            encoding: encoding,
          );
          print('✅ [AUTH HTTP] PUT Silent retry completed! New Status: ${response.statusCode}');
        } else {
          print('❌ [AUTH HTTP] Refresh returned null on 401.');
        }
      } catch (e) {
        print('🌐 [AUTH HTTP] Network error during token refresh on 401: $e. Keeping session active.');
      }
    }

    return response;
  }

  /// DELETE request with auto token refresh on 401
  static Future<http.Response> delete(
    Uri url, {
    Map<String, String>? headers,
    Object? body,
    Encoding? encoding,
  }) async {
    final token = await TokenStorage.getValidToken();
    final requestHeaders = {
      ...?headers,
      if (token != null) 'Authorization': 'Bearer $token',
    };

    var response = await http.delete(
      url,
      headers: requestHeaders,
      body: body,
      encoding: encoding,
    );

    // If 401, try refresh and retry
    if (response.statusCode == 401) {
      print('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
      print('🛡️ [AUTH HTTP] 401 Unauthorized caught on DELETE $url');
      print('🔄 Calling TokenStorage.refreshAccessToken() automatically...');
      print('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
      try {
        final newToken = await TokenStorage.refreshAccessToken();
        if (newToken != null) {
          requestHeaders['Authorization'] = 'Bearer $newToken';
          response = await http.delete(
            url,
            headers: requestHeaders,
            body: body,
            encoding: encoding,
          );
          print('✅ [AUTH HTTP] DELETE Silent retry completed! New Status: ${response.statusCode}');
        } else {
          print('❌ [AUTH HTTP] Refresh returned null on 401.');
        }
      } catch (e) {
        print('🌐 [AUTH HTTP] Network error during token refresh on 401: $e. Keeping session active.');
      }
    }

    return response;
  }
}
