import 'package:flutter/material.dart';
import '../../../core/Utils/services/home_service/home_service.dart';
import '../model/attendance_status_model.dart';
import '../../Profile/model/profile_model.dart';

class HomeController extends ChangeNotifier {
  AttendanceStatusData? _attendanceStatus;
  ProfileModel? _userProfile;
  bool _isLoading = false;
  String _errorMessage = '';

  // Getters
  AttendanceStatusData? get attendanceStatus => _attendanceStatus;
  ProfileModel? get userProfile => _userProfile;
  bool get isLoading => _isLoading;
  String get errorMessage => _errorMessage;

  // Get user's first name
  String get firstName {
    if (_userProfile?.name == null) return 'User';
    final nameParts = _userProfile!.name.split(' ');
    return nameParts.isNotEmpty ? nameParts.first : 'User';
  }

  // Get profile picture URL
  String? get profilePicture {
    if (_userProfile?.picture == null || _userProfile!.picture!.isEmpty) {
      return null;
    }

    final picture = _userProfile!.picture!;

    // If it's already a full URL, return as is
    if (picture.startsWith('http://') || picture.startsWith('https://')) {
      return picture;
    }

    // Otherwise, prepend the base URL
    return 'http://103.123.74.160:81/upload/$picture';
  }

  // Fetch all home data
  Future<void> fetchHomeData() async {
    try {
      _isLoading = true;
      _errorMessage = '';
      notifyListeners();

      // Fetch both attendance status and profile concurrently
      final results = await Future.wait([
        HomeService.getAttendanceStatus(),
        HomeService.getUserProfile(),
      ]);

      final attendanceResponse = results[0] as AttendanceStatusResponse?;
      final profile = results[1] as ProfileModel?;

      // Update attendance status
      if (attendanceResponse != null && attendanceResponse.success) {
        _attendanceStatus = attendanceResponse.data;
      } else {
        _errorMessage = attendanceResponse?.message ?? 'Failed to fetch attendance status';
      }

      // Update user profile
      _userProfile = profile;
    } catch (e) {
      print('🔴 HOME CONTROLLER ERROR => $e');
      _errorMessage = 'An error occurred';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  // Keep old method for backwards compatibility
  Future<void> fetchAttendanceStatus() async {
    await fetchHomeData();
  }

  // Refresh method
  Future<void> refreshAttendanceStatus() async {
    await fetchHomeData();
  }
}
