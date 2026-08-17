import 'dart:convert';
import 'package:altroz/core/Utils/Urls/urls.dart';
import 'package:altroz/core/Utils/services/authenticated_http.dart';
import 'package:altroz/core/Utils/services/token_storage.dart';

import '../../../../feature/Home/model/event.dart';

class EventService {
  static Future<List<Event>?> getUpcomingEvents() async{
    try{
      final token = await TokenStorage.getToken();
      if (token == null) return null;

      final uri = Uri.parse(BaseUrls.upEvent);
      print('Event SERVICE: CALLING STANDARD API => $uri');
      final response = await AuthenticatedHttp.get(
          uri,
          headers: {
            'accept' : '*/*',
            'Authorization' : 'Bearer $token',
          },
      );
      if (response.statusCode == 401) {
        return null;
      }
      if (response.statusCode != 200) return null;

      final decoded = jsonDecode(response.body);

      if (decoded is List) {
        return decoded
            .map((json) =>  Event.fromJson(json as Map<String, dynamic>))
            .toList();
      }
      if (decoded is Map && decoded['success'] == true){
        final List<dynamic> data = decoded['data'];
        return data.map((json) => Event.fromJson(json)).toList();
      }

      return null;
    } catch (e) {
      print('EVENT SERVICE ERROR (standard) => $e');
      return null;
    }
  }
}