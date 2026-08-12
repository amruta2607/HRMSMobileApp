import 'package:flutter/material.dart';
import '../../../core/Utils/services/home_service/home_service.dart';
import '../model/attendance_status_model.dart';
import '../../Profile/model/profile_model.dart';
import '../../../core/Utils/Urls/urls.dart';
import '../../../core/Utils/services/leave_service/leave_service.dart';
import '../../../core/Utils/services/alert_service/alert_service.dart';
import '../../alerts/model/alert_model.dart';
import '../../leave/model/leave_history_model.dart';

class HomeController extends ChangeNotifier {
  AttendanceStatusData? _attendanceStatus;
  ProfileModel? _userProfile;
  int _availedLeaves = 0;
  int _taskCount = 0;
  bool _isLoading = false;
  String _errorMessage = '';

  // Getters
  AttendanceStatusData? get attendanceStatus => _attendanceStatus;
  ProfileModel? get userProfile => _userProfile;
  int get availedLeaves => _availedLeaves;
  int get taskCount => _taskCount;
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
    return '${BaseUrls.base}/upload/$picture';
  }

  // Fetch all home data
  Future<void> fetchHomeData() async {
    try {
      _isLoading = true;
      _errorMessage = '';
      notifyListeners();

      // Fetch all required data concurrently
      final results = await Future.wait([
        HomeService.getAttendanceStatus(),
        HomeService.getUserProfile(),
        LeaveService.getLeaveHistory(),
        AlertService.getAlerts(),
      ]);

      final attendanceResponse = results[0] as AttendanceStatusResponse?;
      final profile = results[1] as ProfileModel?;
      final leaveHistoryModel = results[2] as LeaveHistoryModel?;
      final alertsResult = results[3] as Map<String, dynamic>?;

      // Update attendance status
      if (attendanceResponse != null && attendanceResponse.success) {
        _attendanceStatus = attendanceResponse.data;
      } else {
        _errorMessage = attendanceResponse?.message ?? 'Failed to fetch attendance status';
      }

      // Update user profile
      _userProfile = profile;

      // Update availed leaves count from API
      if (leaveHistoryModel != null && leaveHistoryModel.success) {
        print('DEBUG: Leave History Count => ${leaveHistoryModel.leaveHistory.length}');
        _availedLeaves = leaveHistoryModel.usedLeaves;
        print('DEBUG: Total Availed Leaves => $_availedLeaves');
      } else {
        print('DEBUG: Leave History Model is NULL or success=false');
      }

      // Update task count (Only for Managers)
      if (_userProfile != null && _userProfile!.designation.toLowerCase().contains("manager")) {
        if (alertsResult != null) {
          final List<AlertModel> alerts = alertsResult['alerts'] ?? [];
          _taskCount = alerts.where((a) => _isTask(a) && a.status == "Unread").length;
        }
      } else {
        _taskCount = 0; // Reset to 0 for non-managers
      }

    } catch (e) {
      print('🔴 HOME CONTROLLER ERROR => $e');
      _errorMessage = 'An error occurred';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  bool _isTask(AlertModel a) => AlertService.isTask(a);

  // Keep old method for backwards compatibility
  Future<void> fetchAttendanceStatus() async {
    await fetchHomeData();
  }

  // Refresh method
  Future<void> refreshAttendanceStatus() async {
    await fetchHomeData();
  }
}