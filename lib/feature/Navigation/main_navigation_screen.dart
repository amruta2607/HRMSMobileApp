import 'package:flutter/material.dart';
import '../Home/home_screen.dart';
import '../Attendance/attendance_screen.dart';
import 'package:altroz/feature/Alerts/alerts_screen.dart';
import 'navigation_bar.dart';

class MainNavigationScreen extends StatefulWidget {
  final int? initialIndex;

  const MainNavigationScreen({
    super.key,
    this.initialIndex,
  });

  @override
  State<MainNavigationScreen> createState() => _MainNavigationScreenState();
}

class _MainNavigationScreenState extends State<MainNavigationScreen> {
  late int _currentIndex;

  @override
  void initState() {
    super.initState();
    _currentIndex = widget.initialIndex ?? 0;
  }

  final List<Widget> _screens = const [
    HomeScreen(),
    AlertsScreen(),
    AttendanceScreen(),
    Center(child: Text("Menu Screen")),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      resizeToAvoidBottomInset: false,
      body: IndexedStack(
        index: _currentIndex,
        children: _screens,
      ),
      bottomNavigationBar: CustomNavigationBar(
        currentIndex: _currentIndex,
        onChanged: (index) {
          setState(() => _currentIndex = index);
        },
      ),
    );
  }
}