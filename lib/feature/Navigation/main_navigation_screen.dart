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
  bool _alertShowTasks = false; // 👈 lifted state
  bool _fromMenu = false; // 👈 Track navigation source

  @override
  void initState() {
    super.initState();
    _currentIndex = widget.initialIndex ?? 0;
    _alertShowTasks = widget.initialAlertShowTasks;

    // Auto-refresh attendance if starting there
    if (_currentIndex == 2) {
      AttendanceService.triggerRefresh();
    }
  }

  void _navigateTo(int index, {bool fromMenu = false}) {
    setState(() {
      _currentIndex = index;
      _fromMenu = fromMenu;
      // Reset to Notifications tab when switching via bottom nav
      if (index == 1) _alertShowTasks = false;

      // Auto-refresh attendance when switching to it
      if (index == 2) {
        AttendanceService.triggerRefresh();
      }
    });
  }

  // 👇 New method for navigating to Alerts with Tasks tab
  void _navigateToAlertsTasks() {
    setState(() {
      _currentIndex = 1;
      _alertShowTasks = true;
      _fromMenu = true; // Alerts from Menu
    });
  }

  @override
  Widget build(BuildContext context) {
    final screens = [
      const HomeScreen(),
      AlertsScreen(
        initialShowTasks: _alertShowTasks,
        onBack: () {
          if (_fromMenu) {
            _navigateTo(3);
          } else {
            _navigateTo(0);
          }
        },
      ), // 👈 dynamic now
      AttendanceScreen(
        onBack: () {
          if (_fromMenu) {
            _navigateTo(3);
          } else {
            _navigateTo(0);
          }
        },
      ),
      MenuScreen(
        onNavigate: (index) => _navigateTo(index, fromMenu: true),
        onNavigateToTasks: _navigateToAlertsTasks, // 👈 pass new callback
      ),
    ];

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
          children: screens,
        ),
        bottomNavigationBar: CustomNavigationBar(
          currentIndex: _currentIndex,
          onChanged: (index) => _navigateTo(index, fromMenu: false),
        ),
      ),
    );
  }
}