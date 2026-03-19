import 'package:flutter/material.dart';
import '../Home/home_screen.dart';
import '../Attendance/attendance_screen.dart';
import 'package:altroz/feature/Alerts/alerts_screen.dart';
import '../menu/menu_screen.dart';
import 'navigation_bar.dart';

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

  @override
  void initState() {
    super.initState();
    _currentIndex = widget.initialIndex ?? 0;
    _alertShowTasks = widget.initialAlertShowTasks;
  }

  void _navigateTo(int index) {
    setState(() {
      _currentIndex = index;
      // Reset to Notifications tab when switching via bottom nav
      if (index == 1) _alertShowTasks = false;
    });
  }

  // 👇 New method for navigating to Alerts with Tasks tab
  void _navigateToAlertsTasks() {
    setState(() {
      _currentIndex = 1;
      _alertShowTasks = true;
    });
  }

  @override
  Widget build(BuildContext context) {
    final screens = [
      const HomeScreen(),
      AlertsScreen(initialShowTasks: _alertShowTasks), // 👈 dynamic now
      const AttendanceScreen(),
      MenuScreen(
        onNavigate: _navigateTo,
        onNavigateToTasks: _navigateToAlertsTasks, // 👈 pass new callback
      ),
    ];

    return PopScope(
      canPop: _currentIndex == 0,
      onPopInvokedWithResult: (didPop, result) {
        if (didPop) return;
        if (_currentIndex != 0) {
          setState(() => _currentIndex = 0);
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
          onChanged: _navigateTo,
        ),
      ),
    );
  }
}