import 'package:flutter/material.dart';
import '../../Reuse_Widgets/leave_primary_button.dart';

class ApplyLeaveButton extends StatelessWidget {
  final VoidCallback onTap;

  const ApplyLeaveButton({
    super.key,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return AppPrimaryButton(
      onTap: onTap,
      child: Text(
        "Apply Leave",
        textAlign: TextAlign.center,
        style: TextStyle(
          fontFamily: 'Roboto',
          fontWeight: FontWeight.w500,
          fontSize: 20.27 * scale,
          height: 28.96 / 20.27,
          letterSpacing: 0.14 * scale,
          color: Colors.white,
        ),
      ),
    );
  }
}
