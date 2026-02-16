import 'package:flutter/material.dart';

class RecentPaySlipsSection extends StatelessWidget {
  final double scale;

  const RecentPaySlipsSection({
    super.key,
    required this.scale,
  });

  Widget _paySlipTile(String month, BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;

    return Container(
      width: screenWidth - (48 * scale), // 24px left + 24px right
      height: 35 * scale,
      margin: EdgeInsets.only(bottom: 12 * scale),
      padding: EdgeInsets.symmetric(horizontal: 12 * scale),
      decoration: BoxDecoration(
        color: const Color(0xFFFFFFFF),
        borderRadius: BorderRadius.circular(4 * scale),
        boxShadow: const [
          BoxShadow(
            color: Color(0x59000000),
            blurRadius: 4.5,
            offset: Offset(0, 0),
          ),
        ],
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            month,
            style: TextStyle(
              fontFamily: 'Inter',
              fontSize: 12 * scale,
              fontWeight: FontWeight.w500,
            ),
          ),
          Image.asset(
            "img/download.png",
            width: 16 * scale,
            height: 16 * scale,
            color: const Color(0xFF0F62FE),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(height: 24 * scale),

        Text(
          "RECENT PAY SLIPS",
          style: TextStyle(
            fontFamily: 'Inter',
            fontWeight: FontWeight.w600,
            fontSize: 12 * scale,
            letterSpacing: 1,
            color: Colors.grey.shade600,
          ),
        ),

        SizedBox(height: 16 * scale),

        _paySlipTile("March 2026", context),
        _paySlipTile("February 2026", context),
        _paySlipTile("January 2026", context),

        SizedBox(height: 8 * scale),

        Align(
          alignment: Alignment.centerRight,
          child: InkWell(
            onTap: () {
              // Navigate to full payslip screen
            },
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(
                  "View All",
                  style: TextStyle(
                    fontFamily: 'Inter',
                    fontWeight: FontWeight.w700,
                    fontSize: 12 * scale,
                    height: 14.07 / 12,
                    color: const Color(0xFF0F62FE),
                  ),
                ),
                SizedBox(width: 4 * scale),
                Icon(
                  Icons.arrow_forward_ios,
                  size: 12 * scale,
                  color: const Color(0xFF0F62FE),
                ),
              ],
            ),
          ),
        ),


        SizedBox(height: 24 * scale),
      ],
    );
  }
}
