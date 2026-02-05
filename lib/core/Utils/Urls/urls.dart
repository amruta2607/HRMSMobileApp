// In urls.dart
class BaseUrls {
  static const String base = "http://103.123.74.160:81";

  // AUTH
  static const String loginWithEmail = "$base/api/Auth/login-email";
  static const String loginWithMobile = "$base/api/Auth/login-mobile";
  static const String logout = "$base/api/Auth/logout";

  static const String forgotPassword = "$base/api/Auth/forgot-password";


  // ATTENDANCE
  static const String punchIn = "$base/attendance/punch-in";
  static const String punchOut = "$base/attendance/punch-out";
  static const String attendanceCalendar =
      "$base/apipunch/attendance/get-attendance-by-calendar";
  static const String attendanceOverview = "$base/attendance/overview";
  static const String attendanceSummary =
      "$base/apipunch/attendance/get-attendance-summery";

  static const String attendanceStatus = "$base/api/attendance/status";


  // PROFILE
  static const String profileByUser =
      "$base/api/personal-details/Personal-Details-by-user";

  static const String createDispute = "$base/api/disputes";
  static const String disputeCategories = "$base/api/disputes/categories";


  //Leave
  static const String leaveBalance = "$base/api/Leave/balance";
  static const String applyLeave = "$base/api/Leave/request";
  static const String leaveRequests = "$base/apipunch/leave/request/get";
  static const String withdrawLeave = "$base/api/Leave/withdraw";

}