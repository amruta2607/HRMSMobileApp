import 'dart:convert';

import 'package:altroz/core/Utils/Urls/urls.dart';
import 'package:altroz/core/Utils/services/authenticated_http.dart';
import 'package:altroz/core/Utils/services/token_storage.dart';
import 'package:altroz/feature/Home/model/work_anniversary.dart';

class WorkAnniversaryService {
  static Future<List<WorkAnniversary>?> getUpcomingAnniversaries() async{
    try{
      final token = await TokenStorage.getToken();
      if (token == null) return null;

      final uri = Uri.parse(BaseUrls.upWorkAnniversaries);
      print('WorkAnniversaries SERVICE: CALLING STANDARD API => $uri');
      final response = await AuthenticatedHttp.get(
          uri,
          headers: {
            'accept' : "*/*",
            'Authorization' : "Bearer $token",
         }
      );

      if (response.statusCode != 200) return null;

      final decoded = jsonDecode(response.body);
      if (decoded is List){
        return decoded.map((json) => WorkAnniversary.fromJson(json as Map<String, dynamic>)).toList();
      }
     return null;
    }catch (e) {
      print('WORK ANNIVERSARY SERVICE ERROR => $e');
      return null;
    }
  }
}