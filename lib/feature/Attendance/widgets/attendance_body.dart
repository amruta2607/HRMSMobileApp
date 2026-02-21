import 'dart:async';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

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
import 'Attendance Header/attendance_header.dart';
import 'week_overview/attendance_overview_card.dart';
import 'daywise_Record/attendance_daywise_card.dart';
import 'daywise_Record/attendance_daywise_model.dart';

class AttendanceBody extends StatefulWidget {
  const AttendanceBody({super.key});

  @override
  State<AttendanceBody> createState() => _AttendanceBodyState();
}

class _AttendanceBodyState extends State<AttendanceBody> {
  AttendanceRowData? selectedRow;

  bool isClockedIn = false;
  bool _isLoading = false;

  final List<AttendanceRowData> _rows = [];
  Map<int, AttendanceStatus> _monthStatus = {};

  Timer? _timer;
  Duration _workedDuration = Duration.zero;

  int _currentYear = DateTime.now().year;
  int _currentMonth = DateTime.now().month;

  String _date(DateTime d) => DateFormat('dd/MM/yyyy').format(d);
  String _time(DateTime d) => DateFormat('hh:mm a').format(d);

  @override
  void initState() {
    super.initState();
    _fetchCalendar(_currentYear, _currentMonth);
    _fetchCurrentMonthRecords(); // Load records for the ACTUAL current month
    _restoreState();
  }

  Future<void> _restoreState() async {
    final status = await AttendanceService.getTodayStatus();
    if (status != null) {
      if (status.punchIn != null && status.punchOut == null) {
        // Currently clocked in
        final now = DateTime.now();
        final diff = now.difference(status.punchIn!);

        setState(() {
          isClockedIn = true;
          _workedDuration = diff;
        });
        _startTimer();
      } else {
        // Not clocked in
        setState(() {
          isClockedIn = false;
        });
      }
    }
  }

  // CALENDAR CARD - Fetches dots for selected month
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

    final response = await AttendanceService.getAttendanceByCalendar(
      month: month,
      year: year,
    );

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

  // RECORDS CARD - Always shows data for the literal current month
  Future<void> _fetchCurrentMonthRecords() async {
    final now = DateTime.now();
    final response = await AttendanceService.getAttendanceSummary(
      month: now.month,
      year: now.year,
    );

    if (response != null) {
      final List details = response['attendanceDetails'] ?? [];
      final List<AttendanceRowData> newRows = details
          .map((item) => AttendanceRowData.fromJson(item))
          .where((row) => row.clockIn != null || row.clockOut != null)
          .toList()
          .reversed // Newest first
          .toList();

      setState(() {
        _rows.clear();
        _rows.addAll(newRows);
      });
    }
  }

  // TIMER
  void _startTimer() {
    _timer?.cancel();
    _timer = Timer.periodic(
      const Duration(seconds: 1),
          (_) => setState(() {
        _workedDuration += const Duration(seconds: 1);
      }),
    );
  }

  void _stopTimer() => _timer?.cancel();

  // CLOCK IN
  Future<void> _clockIn(DateTime punchTime) async {
    setState(() => _isLoading = true);

    // GEOFENCING CHECK
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
          _showError(
              'You are not in the office range. Radius: ${geoConfig.radius}m');
          return;
        }
      }
    } catch (e) {
      print('Geofencing check failed: $e');
      // Decide if you want to block or allow if check fails.
      // For now, we'll log it and let it proceed or show error.
      // Usually strict geofencing would return here.
      // setState(() => _isLoading = false);
      // _showError("Location check failed: $e");
      // return;
    }

    final success = await AttendanceService.submitAttendance(
      isPunchIn: true,
      punchTime: punchTime,
    );

    setState(() => _isLoading = false);

    if (!success) {
      _showError('Clock-in failed');
      return;
    }

    setState(() {
      isClockedIn = true;
      _workedDuration = Duration.zero;
    });

    _startTimer();
    _fetchCalendar(_currentYear, _currentMonth);
    _fetchCurrentMonthRecords();
  }


  // CLOCK OUT
  Future<void> _clockOut(DateTime punchTime) async {
    setState(() => _isLoading = true);

    final success = await AttendanceService.submitAttendance(
      isPunchIn: false,
      punchTime: punchTime,
    );

    setState(() => _isLoading = false);

    if (!success) {
      _showError('Clock-out failed');
      return;
    }

    setState(() {
      isClockedIn = false;
      _stopTimer();
      _workedDuration = Duration.zero;
    });

    _fetchCalendar(_currentYear, _currentMonth);
    _fetchCurrentMonthRecords();
  }

  void _handleClockTap(BuildContext context) {
    if (_isLoading) return;

    showDialog(
      context: context,
      builder: (_) => isClockedIn
          ? ClockOutDialog(onConfirm: _clockOut)
          : ClockInDialog(onConfirm: _clockIn),
    );
  }

  void _showError(String msg) {
    ScaffoldMessenger.of(context)
        .showSnackBar(SnackBar(content: Text(msg)));
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  // UI
  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        Column(
          children: [
            AttendanceHeader(
              title: 'Attendance',
              onBack: () {
                Navigator.pushAndRemoveUntil(
                  context,
                  MaterialPageRoute(
                    builder: (_) => const MainNavigationScreen(),
                  ),
                      (route) => false,
                );
              },
              onClockTap: () => _handleClockTap(context),
              isClockedIn: isClockedIn,
              workedDuration: _workedDuration,
            ),

            Expanded(
              child: SingleChildScrollView(
                padding: const EdgeInsets.fromLTRB(20, 24, 20, 120),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'OVERVIEW',
                      style: TextStyle(
                        letterSpacing: 1.4,
                        fontWeight: FontWeight.w600,
                        color: AppColors.textGrey,
                      ),
                    ),
                    const SizedBox(height: 12),

                    const AttendanceOverviewCard(),

                    const SizedBox(height: 24),

                    const Text(
                      'MONTH OVERVIEW',
                      style: TextStyle(
                        letterSpacing: 1.4,
                        fontWeight: FontWeight.w600,
                        color: AppColors.textGrey,
                      ),
                    ),
                    const SizedBox(height: 16),

                    AttendanceMonthOverview(
                      today: DateTime.now(),
                      dayStatus: _monthStatus,
                      onMonthChanged: _fetchCalendar,
                    ),

                    const SizedBox(height: 12),
                    const AttendanceLegend(),

                    const SizedBox(height: 24),

                    const Text(
                      'RECORDS',
                      style: TextStyle(
                        letterSpacing: 1.4,
                        fontWeight: FontWeight.w600,
                        color: AppColors.textGrey,
                      ),
                    ),
                    const SizedBox(height: 16),

                    AttendanceDayWiseCard(
                      rows: _rows,
                      onRowTap: (row) =>
                          setState(() => selectedRow = row),
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
                  MaterialPageRoute(
                    builder: (_) => DisputeScreen(
                      selectedDate: selectedRow!.date,
                    ),
                  ),
                );
                // Hide button when returning
                if (mounted) {
                  setState(() => selectedRow = null);
                }
              },
              label: const Text(
                'Raise Dispute',
                style: TextStyle(
                  fontWeight: FontWeight.w600,
                  color: Colors.white,
                ),
              ),
            ),
          ),
      ],
    );
  }
}
