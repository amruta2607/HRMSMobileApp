class BaseUrls {

     static const String base = "http://20.44.57.126:82"; //development


     // static const String base = "http://103.123.74.159:5005"; //production

  // AUTH
  static const String loginWithEmail = "$base/api/Auth/login-email";
  static const String loginWithMobile = "$base/api/Auth/login-mobile";
  static const String logout = "$base/api/Auth/logout";

  static const String refreshToken = "$base/api/Auth/refresh-token";


  static const String forgotPassword = "$base/api/Auth/forgot-password";
     static const String moduleAccess = "$base/api/mobile/module-access";



     // ATTENDANCE
  static const String punchIn = "$base/attendance/punch-in";
  static const String punchOut = "$base/attendance/punch-out";
  static const String attendanceCalendar =
      "$base/apipunch/attendance/get-attendance-by-calendar";
  static const String attendanceOverview = "$base/attendance/overview";
  static const String attendanceSummary =
      "$base/apipunch/attendance/get-attendance-summery";

  static const String attendanceStatus = "$base/api/attendance/status";
  static const String geofencingByTenant = "$base/api/geofencing/by-tenant";

  static const String todayLogs = "$base/api/attendance/today-logs";


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
static const String leaveHistory = "$base/api/Leave/history";



// Payroll
  static const String providentFund = "$base/api/PaySlip/provident-fund";
  static const String monthlySummary = "$base/api/PaySlip/monthly-summary";
  static const String lastMonthPayroll = "$base/api/PaySlip/last-month-payroll";

  // Pay Slip
  static const String paySlipList = "$base/api/PaySlip";
  static const String paySlipYears = "$base/api/PaySlip/years";
  static const String paySlipMonths = "$base/api/PaySlip/months";
  static const String downloadPaySlip = "$base/api/PaySlip/download";


// Holidays
  static const String holidays = "$base/apipunch/holidays/get-holidays";
  // Holidays
  static const String upcoming = "$base/api/mobile/holidays";

  //Up Next
  static const String upEvent = "$base/api/mobile/events";
  static const String upBirthday = "$base/api/dashboard/birthdays";
  static const String upWorkAnniversaries = "$base/api/dashboard/work-anniversaries";
  static const String upAwards = "$base/api/dashboard/awards";

  // Alerts
  static const String alerts = "$base/api/Alert/user";
  static const String approveAlert = "$base/api/Alert/approve-request";
  static const String rejectAlert = "$base/api/Alert/reject-request";
  static const String markReadAlert = "$base/api/Alert/mark-read";
  static const String templates = "$base/api/templates";



  //menu
  static const String alertCount = "$base/api/Alert/user/count";

  // Announcements
  static const String announcements = "$base/api/mobile/announcements";

  // Tenant
  static const String companyLogo = "$base/api/Tenant/GetCompanyLogo";


}