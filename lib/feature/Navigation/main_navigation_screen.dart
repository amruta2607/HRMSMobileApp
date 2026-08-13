import 'package:flutter/material.dart';
import '../Home/home_screen.dart';
import '../Attendance/attendance_screen.dart';
import 'package:altroz/feature/alerts/alerts_screen.dart';
import '../menu/menu_screen.dart';
import 'navigation_bar.dart';
import '../../core/Utils/services/Attendance service/attendance_service.dart';

class MainNavigationScreen extends StatefulWidget {
  final int? initialIndex;
  final bool initialAlertShowTasks;

  const MainNavigationScreen({
    super.key,
    this.initialIndex,
    this.initialAlertShowTasks = false,
  });

  @override
  State<MainNavigationScreen> createState() => _MainNavigationScreenState();
}

class _MainNavigationScreenState extends State<MainNavigationScreen> {
  late int _currentIndex;
  bool _fromMenu = false;
  late final List<Widget> _screens;
  DateTime? _lastAttendanceTabRefresh;
  static const _attendanceTabThrottle = Duration(seconds: 30);

  @override
  void initState() {
    super.initState();
    _currentIndex = widget.initialIndex ?? 0;

    _screens = [
      const HomeScreen(),
      AlertsScreen(
        key: const ValueKey('alerts'),
        initialShowTasks: widget.initialAlertShowTasks,
        onBack: _onAlertsBack,
      ),
      AttendanceScreen(
        key: const ValueKey('attendance'),
        onBack: _onAttendanceBack,
      ),
      MenuScreen(
        key: const ValueKey('menu'),
        onNavigate: (index) => _navigateTo(index, fromMenu: true),
        onNavigateToTasks: _navigateToAlertsTasks,
      ),
    ];

    if (_currentIndex == 2) {
      _maybeRefreshAttendanceTab();
    }
  }

  void _onAlertsBack() {
    if (_fromMenu) {
      _navigateTo(3);
    } else {
      _navigateTo(0);
    }
  }

  void _onAttendanceBack() {
    if (_fromMenu) {
      _navigateTo(3);
    } else {
      _navigateTo(0);
    }
  }

  void _maybeRefreshAttendanceTab() {
    final now = DateTime.now();
    if (_lastAttendanceTabRefresh != null &&
        now.difference(_lastAttendanceTabRefresh!) < _attendanceTabThrottle) {
      return;
    }
    _lastAttendanceTabRefresh = now;
    AttendanceService.triggerRefresh();
  }

  void _setAlertsShowTasks(bool showTasks) {
    _screens[1] = AlertsScreen(
      key: const ValueKey('alerts'),
      initialShowTasks: showTasks,
      onBack: _onAlertsBack,
    );
  }

  void _navigateTo(int index, {bool fromMenu = false}) {
    setState(() {
      _currentIndex = index;
      _fromMenu = fromMenu;
      if (index == 1) {
        _setAlertsShowTasks(false);
      }
      if (index == 2) {
        _maybeRefreshAttendanceTab();
      }
    });
  }

  void _navigateToAlertsTasks() {
    setState(() {
      _currentIndex = 1;
      _fromMenu = true;
      _setAlertsShowTasks(true);
    });
  }

  @override
  Widget build(BuildContext context) {
    return PopScope(
      canPop: _currentIndex == 0,
      onPopInvokedWithResult: (didPop, result) {
        if (didPop) return;
        if (_currentIndex != 0) {
          if ((_currentIndex == 2 || _currentIndex == 1) && _fromMenu) {
            _navigateTo(3);
          } else {
            setState(() => _currentIndex = 0);
          }
        }
      },
      child: Scaffold(
        resizeToAvoidBottomInset: false,
        body: IndexedStack(
          index: _currentIndex,
          children: _screens,
        ),
        bottomNavigationBar: CustomNavigationBar(
          currentIndex: _currentIndex,
          onChanged: (index) => _navigateTo(index, fromMenu: false),
        ),
      ),
    );
  }
}
