import 'package:flutter/material.dart';
import '../../../core/Theme/app_colors.dart';

class AttendanceTitleBlock extends StatelessWidget {
  const AttendanceTitleBlock({super.key});

  @override
  Widget build(BuildContext context) {
    final size = MediaQuery.of(context).size;

    /// 🔹 Figma reference
    const figmaWidth = 402.0;
    const blockWidth = 324.53;
    const blockHeight = 59.01;

    final scale = size.width / figmaWidth;

    return SizedBox(
      width: blockWidth * scale,
      height: blockHeight * scale,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          /// DATE
          const Text(
            'Oct 24, Tuesday',
            style: TextStyle(
              color: AppColors.textGrey,
              fontSize: 14,
            ),
          ),
          const SizedBox(height: 4),


          const Text(
            '',
            style: TextStyle(
              fontSize: 28,
              fontWeight: FontWeight.bold,
              color: AppColors.textDark,
              height: 1.1,
            ),
          ),
        ],
      ),
    );
  }
}
