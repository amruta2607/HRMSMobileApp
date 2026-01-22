import 'package:flutter/material.dart';
import '../Reuse_Widgets/home_screen_constent.dart';
import 'widgets/attendance_body.dart';

class AttendanceScreen extends StatelessWidget {
  const AttendanceScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return HomeScreenConstent(
      body: AttendanceBody(),
    );
  }
}
