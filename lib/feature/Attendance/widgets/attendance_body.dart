import 'dart:async';
import 'package:flutter/material.dart';

import '../../../core/Theme/app_colors.dart';
import '../../../core/Utils/services/Attendance service/attendance_service.dart';
import '../../../core/Utils/services/Time_Location/location_service.dart';

import '../../Navigation/main_navigation_screen.dart';
import 'mothOverview/attendance_month_overview.dart';
import 'mothOverview/attendance_status.dart';
import 'mothOverview/attendance_legend.dart';

import '../../Dispute/dispute_screen.dart';
import '../dialogs/clock_in_dialog.dart';
import '../dialogs/clock_out_dialog.dart';
import '../dialogs/already_punched_dialog.dart';
import '../../../core/Utils/services/connectivity_service.dart';
import 'Attendance Header/attendance_header.dart';
import 'week_overview/attendance_overview_card.dart';
import 'daywise_Record/attendance_daywise_card.dart';
import 'daywise_Record/attendance_daywise_model.dart';

class AttendanceBody extends StatefulWidget {
  final VoidCallback? onBack;
  const AttendanceBody({super.key, this.onBack});

  @override
  State<AttendanceBody> createState() => _AttendanceBodyState();
}

class _AttendanceBodyState extends State<AttendanceBody> {
  AttendanceRowData? selectedRow;
  Map<int, AttendanceStatus> _monthStatus = {};
  bool _isLoading = false;
  final List<AttendanceRowData> _rows = [];

  int _currentYear = DateTime.now().year;
  int _currentMonth = DateTime.now().month;

  Timer? _timer;
  Duration _workedDuration = Duration.zero;

  @override
  void initState() {
    super.initState();
    _fetchCalendar(_currentYear, _currentMonth);
    _fetchCurrentMonthRecords();

    // Listen to attendance changes from other screens (e.g. Home Header)
    AttendanceService.isClockedInNotifier.addListener(_onAttendanceUpdate);
    AttendanceService.punchInTimeNotifier.addListener(_onAttendanceUpdate);
    AttendanceService.isPunchedOutForTodayNotifier.addListener(_onAttendanceUpdate);

    // Listen to manual triggers (e.g. from Bottom Nav)
    AttendanceService.attendanceRefreshNotifier.addListener(_onManualRefresh);

    _restoreAttendanceState();

    ConnectivityService.onReconnected(_handleReconnection);
  }

  void _onManualRefresh() {
    if (mounted) {
      _fetchCalendar(_currentYear, _currentMonth);
      _fetchCurrentMonthRecords();
      _restoreAttendanceState();
    }
  }

  void _onAttendanceUpdate() {
    if (mounted) {
      _updateTimerFromService();
      // Optionally refresh records if state changed to clocked out
      if (!AttendanceService.isClockedIn) {
        _fetchCalendar(_currentYear, _currentMonth);
        _fetchCurrentMonthRecords();
      }
    }
  }

  Future<void> _restoreAttendanceState() async {
    setState(() => _isLoading = true);
    await AttendanceService.getTodayStatus();
    _updateTimerFromService();
    setState(() => _isLoading = false);
  }

  void _updateTimerFromService() {
    _timer?.cancel();
    if (AttendanceService.isClockedIn && AttendanceService.punchInTime != null) {
      final now = DateTime.now();
      _workedDuration = now.difference(AttendanceService.punchInTime!);
      _startTimer();
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

  void _handleReconnection() {
    if (mounted) {
      _fetchCalendar(_currentYear, _currentMonth);
      _fetchCurrentMonthRecords();
      _restoreAttendanceState();
    }
  }

  Future<void> _fetchCalendar(int year, int month) async {
    final now = DateTime.now();
    final today = DateTime(now.year, now.month, now.day);

    if (year > now.year || (year == now.year && month > now.month)) {
      _showError('Future month attendance not available');
      return;
    }

    setState(() {
      _isLoading = true;
      _currentYear = year;
      _currentMonth = month;
    });

    final response = await AttendanceService.getAttendanceByCalendar(month: month, year: year);
    setState(() => _isLoading = false);

    if (response != null) {
      final List calendarData = response['calendarData'] ?? [];
      final Map<int, AttendanceStatus> map = {};

      for (final item in calendarData) {
        final int day = item['day'];
        final date = DateTime(year, month, day);
        if (date.isAfter(today)) continue;

        if (item['isWeekend'] == true || item['isHoliday'] == true) {
          map[day] = AttendanceStatus.holiday;
        } else if (item['isLeave'] == true) {
          map[day] = AttendanceStatus.leave;
        } else if (item['isAbsent'] == true) {
          map[day] = AttendanceStatus.absent;
        } else if (item['isPresent'] == true) {
          map[day] = AttendanceStatus.present;
        }
      }
      setState(() => _monthStatus = map);
    } else {
      _showError('Failed to load calendar');
    }
  }

  Future<void> _fetchCurrentMonthRecords() async {
    final now = DateTime.now();
    final response = await AttendanceService.getAttendanceSummary(month: now.month, year: now.year);

    if (response != null) {
      final List details = response['attendanceDetails'] ?? [];
      final List<AttendanceRowData> newRows = details
          .map((item) => AttendanceRowData.fromJson(item))
          .where((row) => row.clockIn != null || row.clockOut != null)
          .toList()
          .reversed
          .toList();

      setState(() {
        _rows.clear();
        _rows.addAll(newRows);
      });
    }
  }

  void _handleClockTap(BuildContext context) {
    if (_isLoading) return;

    if (AttendanceService.isPunchedOutForToday) {
      showDialog(
        context: context,
        builder: (_) => const AlreadyPunchedDialog(
          title: 'Already Punched Out',
          message: 'You have already completed your attendance for today. No further actions allowed.',
        ),
      );
      return;
    }

    showDialog(
      context: context,
      builder: (_) => AttendanceService.isClockedIn
          ? ClockOutDialog(onConfirm: _clockOut)
          : ClockInDialog(onConfirm: _clockIn),
    );
  }

  Future<void> _clockIn(DateTime punchTime) async {
    setState(() => _isLoading = true);
    try {
      final geoConfig = await AttendanceService.getGeofencingDetails();
      if (geoConfig != null && geoConfig.isEnabled) {
        final position = await LocationService.getLatLng();
        final isWithin = AttendanceService.isWithinRadius(
          currentLat: position.latitude,
          currentLng: position.longitude,
          branchLat: geoConfig.latitude,
          branchLng: geoConfig.longitude,
          radius: geoConfig.radius,
        );

        if (!isWithin) {
          setState(() => _isLoading = false);
          _showError('You are not in the office range. Radius: ${geoConfig.radius}m');
          return;
        }
      }
    } catch (e) {
      print('Geofencing check failed (body): $e');
    }

    final result = await AttendanceService.submitAttendance(
      isPunchIn: true,
      punchTime: punchTime,
    );

    if (!result.success) {
      if (result.message?.toLowerCase().contains('already') == true) {
        showDialog(
          context: context,
          builder: (_) => AlreadyPunchedDialog(
            title: 'Action Already Done',
            message: result.message ?? 'You have already performed this action.',
          ),
        );
      } else {
        _showError(result.message ?? 'Clock-in failed');
      }
    }
    setState(() => _isLoading = false);
  }

  Future<void> _clockOut(DateTime punchTime) async {
    setState(() => _isLoading = true);
    final result = await AttendanceService.submitAttendance(
      isPunchIn: false,
      punchTime: punchTime,
    );

    if (!result.success) {
      if (result.message?.toLowerCase().contains('already') == true) {
        showDialog(
          context: context,
          builder: (_) => AlreadyPunchedDialog(
            title: 'Action Already Done',
            message: result.message ?? 'You have already performed this action.',
          ),
        );
      } else {
        _showError(result.message ?? 'Clock-out failed');
      }
    }
    setState(() => _isLoading = false);
  }

  void _showError(String msg) {
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(msg)));
  }

  @override
  void dispose() {
    _timer?.cancel();
    AttendanceService.isClockedInNotifier.removeListener(_onAttendanceUpdate);
    AttendanceService.punchInTimeNotifier.removeListener(_onAttendanceUpdate);
    AttendanceService.isPunchedOutForTodayNotifier.removeListener(_onAttendanceUpdate);
    AttendanceService.attendanceRefreshNotifier.removeListener(_onManualRefresh);
    ConnectivityService.removeOnReconnected(_handleReconnection);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        Column(
          children: [
            AttendanceHeader(
              title: 'Attendance',
              onBack: widget.onBack ?? () {
                Navigator.pushAndRemoveUntil(
                  context,
                  MaterialPageRoute(builder: (_) => const MainNavigationScreen()),
                      (route) => false,
                );
              },
              onClockTap: () => _handleClockTap(context),
              isClockedIn: AttendanceService.isClockedIn,
              workedDuration: _workedDuration,
            ),
            Expanded(
              child: SingleChildScrollView(
                padding: const EdgeInsets.fromLTRB(20, 24, 20, 120),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text('OVERVIEW', style: TextStyle(letterSpacing: 1.4, fontWeight: FontWeight.w600, color: AppColors.textGrey)),
                    const SizedBox(height: 12),
                    const AttendanceOverviewCard(),
                    const SizedBox(height: 24),
                    const Text('MONTH OVERVIEW', style: TextStyle(letterSpacing: 1.4, fontWeight: FontWeight.w600, color: AppColors.textGrey)),
                    const SizedBox(height: 16),
                    AttendanceMonthOverview(
                      today: DateTime.now(),
                      dayStatus: _monthStatus,
                      onMonthChanged: _fetchCalendar,
                    ),
                    const SizedBox(height: 12),
                    const AttendanceLegend(),
                    const SizedBox(height: 24),
                    const Text('RECORDS', style: TextStyle(letterSpacing: 1.4, fontWeight: FontWeight.w600, color: AppColors.textGrey)),
                    const SizedBox(height: 16),
                    AttendanceDayWiseCard(
                      rows: _rows,
                      onRowTap: (row) => setState(() => selectedRow = row),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
        if (_isLoading)
          const Center(child: CircularProgressIndicator()),
        if (selectedRow != null)
          Positioned(
            right: 20,
            bottom: 20,
            child: FloatingActionButton.extended(
              backgroundColor: const Color(0xFF42A5F5),
              onPressed: () async {
                await Navigator.push(
                  context,
                  MaterialPageRoute(builder: (_) => DisputeScreen(selectedDate: selectedRow!.date)),
                );
                if (mounted) setState(() => selectedRow = null);
              },
              label: const Text('Raise Dispute', style: TextStyle(fontWeight: FontWeight.w600, color: Colors.white)),
            ),
          ),
      ],
    );
  }
}
