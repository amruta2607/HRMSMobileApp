import 'package:flutter/material.dart';

class LeaveSummaryCard extends StatelessWidget {
  final String title;
  final double used;
  final double total;
  final Color color;

  const LeaveSummaryCard({
    super.key,
    required this.title,
    required this.used,
    required this.total,
    required this.color,
  });

  String _formatValue(double val) {
    if (val % 1 == 0) {
      return val.toInt().toString();
    }
    return val.toString();
  }

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return Container(
      width: 107 * scale,
      height: 88 * scale,
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(10 * scale),
        border: Border.all(color: color.withOpacity(.3)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(.05),
            blurRadius: 8 * scale,
            offset: Offset(0, 4 * scale),
          ),
        ],
      ),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          /// Title
          Text(
            title,
            textAlign: TextAlign.center,
            style: TextStyle(
              fontSize: 12 * scale,
              fontWeight: FontWeight.w500,
              color: color,
            ),
          ),

          SizedBox(height: 6 * scale),

          /// 4 /12 (Different Sizes, Centered)
          RichText(
            textAlign: TextAlign.center,
            text: TextSpan(
              children: [
                TextSpan(
                  text: _formatValue(used),
                  style: TextStyle(
                    fontFamily: 'Inter',
                    fontWeight: FontWeight.w500,
                    fontSize: 30.72 * scale,
                    height: 34.39 / 30.72, // line-height ratio
                    color: color,
                  ),
                ),
                TextSpan(
                  text: "/${_formatValue(total)}",
                  style: TextStyle(
                    fontFamily: 'Inter',
                    fontWeight: FontWeight.w500,
                    fontSize: 17.75 * scale,
                    height: 19.87 / 17.75,
                    color: color,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
