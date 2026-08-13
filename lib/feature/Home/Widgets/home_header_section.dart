import 'dart:async';
import 'dart:io';
import '../../../core/Utils/services/token_storage.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../../core/Theme/app_colors.dart';
import '../home_controller/home_controller.dart';
import '../../../core/Utils/services/connectivity_service.dart';
import '../../Profile/profile_screen.dart';
import '../../Reuse_Widgets/authenticated_image.dart';
import '../../Tenant/controller/tenant_controller.dart';

import '../../../core/Utils/services/Attendance service/attendance_service.dart';
import '../../../core/Utils/services/Time_Location/location_service.dart';
import '../../../core/Utils/services/app_permission_service.dart';
import '../../../core/constants/location_config.dart';
import '../../Attendance/dialogs/clock_in_dialog.dart';
import '../../Attendance/dialogs/clock_out_dialog.dart';
import '../../Attendance/dialogs/already_punched_dialog.dart';
import '../../Attendance/widgets/Attendance Header/clock_action_button.dart';

class HomeHeaderSection extends StatefulWidget {
  const HomeHeaderSection({super.key});

  @override
  State<HomeHeaderSection> createState() => _HomeHeaderSectionState();
}

class _HomeHeaderSectionState extends State<HomeHeaderSection>
    with WidgetsBindingObserver {
  late Future<String> _locationFuture;

  Timer? _timer;
  Duration _workedDuration = Duration.zero;
  bool _isLoading = false;
  DateTime? _lastResumeRefresh;
  static const _resumeThrottle = Duration(seconds: 30);

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    _refreshLocation();

    // Listen to attendance changes from other screens
    AttendanceService.isClockedInNotifier.addListener(_onAttendanceUpdate);
    AttendanceService.punchInTimeNotifier.addListener(_onAttendanceUpdate);
    AttendanceService.isPunchedOutForTodayNotifier.addListener(_onAttendanceUpdate);

    _restoreAttendanceState();

    // Auto-refresh when internet is back
    ConnectivityService.onReconnected(_handleReconnection);
  }

  void _onAttendanceUpdate() {
    if (mounted) {
      _updateTimerFromService();
    }
  }

  Future<void> _restoreAttendanceState() async {
    // Don't blank the attendance card on every resume.
    final hadState = AttendanceService.punchInTime != null ||
        AttendanceService.isClockedIn;
    if (!hadState) setState(() => _isLoading = true);
    await AttendanceService.getTodayStatus();
    _updateTimerFromService();
    if (mounted) setState(() => _isLoading = false);
  }

  void _updateTimerFromService() {
    _timer?.cancel();
    if (AttendanceService.isClockedIn && AttendanceService.punchInTime != null) {
      final now = DateTime.now();
      _workedDuration = now.difference(AttendanceService.punchInTime!);
      if (_workedDuration.isNegative) _workedDuration = Duration.zero;
      _startTimer();
      if (mounted) setState(() {});
    } else {
      _workedDuration = Duration.zero;
      if (mounted) setState(() {});
    }
  }

  void _startTimer() {
    _timer?.cancel();
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (mounted) {
        setState(() {
          _workedDuration += const Duration(seconds: 1);
        });
      }
    });
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      final now = DateTime.now();
      if (_lastResumeRefresh != null &&
          now.difference(_lastResumeRefresh!) < _resumeThrottle) {
        return;
      }
      _lastResumeRefresh = now;
      _refreshLocation();
      _restoreAttendanceState();
    }
  }

  void _refreshLocation() {
    setState(() {
      _locationFuture = LocationService.getLocation(
        forceRefresh: false,
        requestPermissionIfDenied: false,
      );
    });
  }

  String _format(Duration d) {
    if (d.inSeconds <= 0) return '00:00 Hours';
    if (d.inHours == 0) {
      final m = d.inMinutes.toString().padLeft(2, '0');
      final s = d.inSeconds.remainder(60).toString().padLeft(2, '0');
      return '$m:$s Min';
    } else {
      final h = d.inHours.toString().padLeft(2, '0');
      final m = d.inMinutes.remainder(60).toString().padLeft(2, '0');
      return '$h:$m Hours';
    }
  }

  void _handleClockTap(BuildContext context) async {
    if (_isLoading) return;

    if (!AttendanceService.isClockedIn) {
      final ok =
          await AppPermissionService.ensureTrackingPermissionsWithPopup(
        context,
        requestSystemDialogs: true,
      );
      if (!ok) {
        _showError(
            'Location tracking permissions are required for punch-in.');
        return;
      }
    }

    if (!mounted) return;
    showDialog(
      context: context,
      builder: (_) => AttendanceService.isClockedIn
          ? ClockOutDialog(onConfirm: _clockOut)
          : ClockInDialog(onConfirm: _clockIn),
    );
  }

  Future<void> _clockIn(DateTime punchTime, File image) async {
    setState(() => _isLoading = true);

    try {
      final geoConfig = await AttendanceService.getGeofencingDetails();
      if (!LocationConfig.ENABLE_FROM_ANYWHERE &&
          geoConfig != null &&
          geoConfig.isEnabled) {
        final position = await LocationService.getLatLng(
          requestPermissionIfDenied: false,
        );
        final radius = AttendanceService.resolveGeofenceRadius(geoConfig.radius);
        final isWithin = AttendanceService.isWithinRadius(
          currentLat: position.latitude,
          currentLng: position.longitude,
          branchLat: geoConfig.latitude,
          branchLng: geoConfig.longitude,
          radius: radius,
        );

        if (!isWithin) {
          setState(() => _isLoading = false);
          _showError('You are not in the office range. Radius: ${radius}m');
          return;
        }
      }
    } catch (e) {
      print('Geofencing check failed (home): $e');
    }

    print("Home header punch in");
    print(punchTime);

    final result = await AttendanceService.submitAttendance(
      isPunchIn: true,
      punchTime: punchTime,
      image: image,
    );

    if (!result.success) {
      final msg = result.message ?? 'Clock-in failed / Range Issue';
      final lower = msg.toLowerCase();
      if (lower.contains('already punch') || lower.contains('already marked')) {
        showDialog(
          context: context,
          builder: (_) => AlreadyPunchedDialog(
            title: 'Action Already Done',
            message: msg,
          ),
        );
      } else {
        _showError(msg);
      }
    } else {
      _updateTimerFromService();
    }
    setState(() => _isLoading = false);
  }

  Future<void> _clockOut(DateTime punchTime, File image) async {
    setState(() => _isLoading = true);

    try {
      final geoConfig = await AttendanceService.getGeofencingDetails();
      if (!LocationConfig.ENABLE_FROM_ANYWHERE &&
          geoConfig != null &&
          geoConfig.isEnabled) {
        final position = await LocationService.getLatLng(
          requestPermissionIfDenied: false,
        );
        final radius = AttendanceService.resolveGeofenceRadius(geoConfig.radius);
        final isWithin = AttendanceService.isWithinRadius(
          currentLat: position.latitude,
          currentLng: position.longitude,
          branchLat: geoConfig.latitude,
          branchLng: geoConfig.longitude,
          radius: radius,
        );

        if (!isWithin) {
          setState(() => _isLoading = false);
          _showError('You are not in the office range. Radius: ${radius}m');
          return;
        }
      }
    } catch (e) {
      print('Geofencing check failed (home clockOut): $e');
    }

    print("Home header punch out");
    print(punchTime);
    final result = await AttendanceService.submitAttendance(
      isPunchIn: false,
      punchTime: punchTime,
      image: image,
    );

    if (!result.success) {
      final msg = result.message ?? 'Clock-out failed';
      final lower = msg.toLowerCase();
      if (lower.contains('already punch') || lower.contains('already marked')) {
        showDialog(
          context: context,
          builder: (_) => AlreadyPunchedDialog(
            title: 'Action Already Done',
            message: msg,
          ),
        );
      } else {
        _showError(msg);
      }
    } else {
      _updateTimerFromService();
    }
    setState(() => _isLoading = false);
  }

  void _showError(String msg) {
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(msg)));
  }

  void _handleReconnection() {
    if (mounted) {
      Provider.of<HomeController>(context, listen: false).fetchHomeData();
      _restoreAttendanceState();
    }
  }

  @override
  void dispose() {
    _timer?.cancel();
    AttendanceService.isClockedInNotifier.removeListener(_onAttendanceUpdate);
    AttendanceService.punchInTimeNotifier.removeListener(_onAttendanceUpdate);
    AttendanceService.isPunchedOutForTodayNotifier.removeListener(_onAttendanceUpdate);
    WidgetsBinding.instance.removeObserver(this);
    ConnectivityService.removeOnReconnected(_handleReconnection);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;
    const designWidth = 402.0;
    final scale = (screenWidth / designWidth).clamp(0.85, 1.1);
    final String liveDate = DateFormat('MMM dd, EEEE').format(DateTime.now());

    return Stack(
      children: [
        Container(
          width: double.infinity,
          padding: EdgeInsets.fromLTRB(20 * scale, 24 * scale, 20 * scale, 28 * scale),
          decoration: BoxDecoration(
            color: AppColors.HeaderBg,
            borderRadius: BorderRadius.only(
              bottomLeft: Radius.circular(36 * scale),
              bottomRight: Radius.circular(36 * scale),
            ),
          ),
          child: Column(
            children: [
              Consumer<HomeController>(
                builder: (context, controller, child) {
                  final firstName = controller.firstName;
                  final profilePicture = controller.profilePicture;

                  return Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(liveDate, style: TextStyle(fontSize: 14 * scale, color: AppColors.textLight, fontWeight: FontWeight.w500)),
                          SizedBox(height: 6 * scale),
                          Text('Hi, $firstName', style: TextStyle(fontSize: 28 * scale, fontWeight: FontWeight.w700, color: AppColors.textDark)),
                        ],
                      ),
                      const Spacer(),
                      GestureDetector(
                        onTap: () {
                          Navigator.push(context, MaterialPageRoute(builder: (context) => const ProfileScreen()));
                        },
                        child: Container(
                          padding: EdgeInsets.all(2 * scale),
                          decoration: const BoxDecoration(color: AppColors.background, shape: BoxShape.circle),
                          child: profilePicture != null
                              ? AuthenticatedImage(
                            imageUrl: profilePicture,
                            width: 44 * scale,
                            height: 44 * scale,
                            scale: scale,
                            fallbackLetter: firstName.isNotEmpty ? firstName[0].toUpperCase() : 'U',
                            backgroundColor: AppColors.background,
                          )
                              : CircleAvatar(
                            radius: 22 * scale,
                            backgroundColor: AppColors.homeStatusTextGreen,
                            child: Text(firstName.isNotEmpty ? firstName[0].toUpperCase() : 'U', style: TextStyle(fontSize: 20 * scale, fontWeight: FontWeight.bold, color: Colors.white)),
                          ),
                        ),
                      ),
                    ],
                  );
                },
              ),
              SizedBox(height: 24 * scale),
              if (TokenStorage.isModuleEnabled('attendance'))
                Consumer<HomeController>(
                  builder: (context, controller, child) {
                    if (controller.isLoading) {
                      return Container(
                        padding: EdgeInsets.symmetric(horizontal: 16 * scale, vertical: 14 * scale),
                        decoration: BoxDecoration(color: AppColors.homeStatusCardBg, borderRadius: BorderRadius.circular(18 * scale)),
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            SizedBox(width: 20 * scale, height: 20 * scale, child: CircularProgressIndicator(strokeWidth: 2, valueColor: AlwaysStoppedAnimation<Color>(AppColors.homeStatusTextGreen))),
                            SizedBox(width: 12 * scale),
                            Text('Loading attendance...', style: TextStyle(fontFamily: 'Inter', fontSize: 14 * scale, color: AppColors.textGrey)),
                          ],
                        ),
                      );
                    }

                    final status = controller.attendanceStatus;
                    final bool clockedIn = AttendanceService.isClockedIn;
                    final bool hasPunch = status?.punchIn != null;

                    return Container(
                      padding: EdgeInsets.symmetric(horizontal: 16 * scale, vertical: 14 * scale),
                      decoration: BoxDecoration(color: AppColors.homeStatusCardBg, borderRadius: BorderRadius.circular(18 * scale)),
                      child: Row(
                        children: [
                          Container(
                            width: 45 * scale,
                            height: 45 * scale,
                            decoration: BoxDecoration(
                              color: clockedIn || hasPunch ? AppColors.homeStatusIconBg : Colors.red.withOpacity(0.3),
                              borderRadius: BorderRadius.circular(16 * scale),
                            ),
                            child: clockedIn || hasPunch
                                ? Center(child: Image.asset('img/presentd.png', width: 24 * scale, height: 24 * scale))
                                : Icon(Icons.error_outline, size: 20 * scale, color: AppColors.textGrey),
                          ),
                          SizedBox(width: 12 * scale),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text('Current Status', style: TextStyle(fontFamily: 'Inter', fontSize: 13 * scale, color: AppColors.textGrey)),
                                Text(
                                  clockedIn
                                      ? _format(_workedDuration)
                                      : (hasPunch ? status!.status : 'Not marked yet'),
                                  style: TextStyle(
                                    fontFamily: 'Inter',
                                    fontSize: 15 * scale,
                                    fontWeight: FontWeight.w500,
                                    color: clockedIn || hasPunch ? AppColors.homeStatusTextGreen : AppColors.textGrey,
                                  ),
                                ),
                                SizedBox(height: 4 * scale),
                                FutureBuilder<String>(
                                  future: _locationFuture,
                                  builder: (context, snapshot) {
                                    return Text(
                                      snapshot.data ?? 'Fetching location...',
                                      maxLines: 1,
                                      overflow: TextOverflow.ellipsis,
                                      style: TextStyle(fontFamily: 'Inter', fontSize: 11 * scale, color: AppColors.textGrey, fontWeight: FontWeight.w400),
                                    );
                                  },
                                ),
                              ],
                            ),
                          ),
                          ClockActionButton(
                            isClockedIn: clockedIn,
                            onTap: () => _handleClockTap(context),
                          ),
                        ],
                      ),
                    );
                  },
                ),
            ],
          ),
        ),
        Positioned(
          top: 3 * scale,
          right: 21 * scale,
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Consumer<TenantController>(
                builder: (context, tenantController, child) {
                  final logoUrl = tenantController.companyLogoUrl;
                  if (logoUrl == null || logoUrl.isEmpty) {
                    return const SizedBox.shrink();
                  }
                  return AuthenticatedImage(
                    imageUrl: logoUrl,
                    width: 65 * scale,
                    height: 22 * scale,
                    scale: scale,
                    isCircle: false,
                    fit: BoxFit.contain,
                    fallbackLetter: '',
                    backgroundColor: Colors.transparent,
                  );
                },
              ),


              Image.asset(
                'img/altrozhrm_logo.png',
                width: 65 * scale,
                height: 22 * scale,
                fit: BoxFit.contain,
                errorBuilder: (context, error, stackTrace) => const SizedBox.shrink(),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
