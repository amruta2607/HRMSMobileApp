import 'package:shared_preferences/shared_preferences.dart';
import '../Attendance service/attendance_service.dart';
import '../profile_service/profile_service.dart';
import '../../../../feature/Home/model/attendance_status_model.dart';
import '../../../../feature/Profile/model/profile_model.dart';

class HomeService {
  // Get attendance status for home header
  static Future<AttendanceStatusResponse?> getAttendanceStatus() async {
    try {
      // Get userId from SharedPreferences
      final prefs = await SharedPreferences.getInstance();
      final userId = prefs.getInt('userId');

      if (userId == null) {
        print('🔴 HOME SERVICE: User ID not found');
        return null;
      }

      // Call attendance service to get status
      final response = await AttendanceService.getAttendanceStatus(
        userId: userId,
        date: DateTime.now(),
      );

      return response;
    } catch (e) {
      print('🔴 HOME SERVICE ERROR => $e');
      return null;
    }
  }

  // Get user profile for home header
  static Future<ProfileModel?> getUserProfile() async {
    try {
      final profile = await ProfileService.fetchProfile();
      return profile;
    } catch (e) {
      print('🔴 HOME SERVICE PROFILE ERROR => $e');
      return null;
    }
  }
}
