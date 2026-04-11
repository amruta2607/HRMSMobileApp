import 'package:flutter/material.dart';
import '../../../../core/Theme/app_colors.dart';

class AttendanceLegend extends StatelessWidget {
  const AttendanceLegend({super.key});

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        // 🔹 Figma base width
        const designWidth = 402.0;
        final scale =
        (constraints.maxWidth / designWidth).clamp(0.85, 1.1);

        return Center(
          child: Padding(
            padding: EdgeInsets.symmetric(vertical: 8 * scale),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceEvenly,
              children: const [
                _LegendItem(
                  label: 'Present',
                  color: AppColors.presentColorx,
                ),
                _LegendItem(
                  label: 'Absent',
                  color: AppColors.absentOrange,
                ),
                _LegendItem(
                  label: 'Leave',
                  color: Color(0xFFF44336),
                ),
                _LegendItem(
                  label: 'Holiday',
                  color: Colors.blue,
                ),
              ],
            ),
          ),
        );
      },
    );
  }
}

class _LegendItem extends StatelessWidget {
  final String label;
  final Color color;

  const _LegendItem({
    required this.label,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;
    const designWidth = 402.0;
    final scale = (screenWidth / designWidth).clamp(0.85, 1.1);

    return SizedBox(
      width: 85 * scale,
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Container(
            width: 13 * scale,
            height: 13 * scale,
            decoration: BoxDecoration(
              color: color,
              shape: BoxShape.circle,
            ),
          ),
          SizedBox(width: 3 * scale),
          Flexible(
            child: Text(
              label,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: TextStyle(
                fontSize: 15 * scale,
                fontWeight: FontWeight.w500,
                color: AppColors.textDark,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
