import 'package:flutter/material.dart';
import '../../../core/Theme/app_colors.dart';

class HomeUpNextSection extends StatelessWidget {
  const HomeUpNextSection({super.key});

  @override
  Widget build(BuildContext context) {
    // Design reference width
    const designWidth = 402.0;
    final screenWidth = MediaQuery.of(context).size.width;

    // Scale factor
    final scale = (screenWidth / designWidth).clamp(0.85, 1.1);

    return Container(
      padding: EdgeInsets.all(16 * scale),
      decoration: BoxDecoration(
        color: AppColors.upNextCardBg,
        borderRadius: BorderRadius.circular(20 * scale),
      ),
      child: Row(
        children: [
          /// DATE BOX
          Container(
            padding: EdgeInsets.symmetric(
              vertical: 12 * scale,
              horizontal: 16 * scale,
            ),
            decoration: BoxDecoration(
              color: AppColors.upNextDateBg,
              borderRadius: BorderRadius.circular(14 * scale),
            ),
            child: Column(
              children: [
                Text(
                  'NOV',
                  style: TextStyle(
                    fontSize: 12 * scale,
                    fontWeight: FontWeight.w600,
                    color: AppColors.textDark,
                  ),
                ),
                SizedBox(height: 0 * scale),
                Text(
                  '14',
                  style: TextStyle(
                    fontSize: 20 * scale,
                    fontWeight: FontWeight.bold,
                    color: AppColors.textDark,
                  ),
                ),
              ],
            ),
          ),

          SizedBox(width: 18 * scale),

          /// DETAILS
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Company Retreat',
                style: TextStyle(
                  fontSize: 16 * scale,
                  fontWeight: FontWeight.w700,
                  color: AppColors.textDark,
                ),
              ),
              SizedBox(height: 6 * scale),
              Text(
                'Holiday • All Hands',
                style: TextStyle(
                  fontSize: 14 * scale,
                  color: AppColors.textGrey,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
