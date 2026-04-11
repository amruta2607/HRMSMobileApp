import 'package:flutter/material.dart';
import '../Reuse_Widgets/home_screen_constent.dart';
import 'widgets/attendance_body.dart';

class AttendanceScreen extends StatelessWidget {
  final VoidCallback? onBack;
  const AttendanceScreen({super.key, this.onBack});

  @override
  Widget build(BuildContext context) {
    return HomeScreenConstent(
      body: AttendanceBody(onBack: onBack),
    );
  }
}
