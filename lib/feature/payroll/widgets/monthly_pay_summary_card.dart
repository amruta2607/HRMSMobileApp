import 'package:flutter/material.dart';

class MonthlyPaySummaryCard extends StatefulWidget {
  final double scale;
  final String amount;

  const MonthlyPaySummaryCard({
    super.key,
    required this.scale,
    required this.amount,
  });

  @override
  State<MonthlyPaySummaryCard> createState() =>
      _MonthlyPaySummaryCardState();
}

class _MonthlyPaySummaryCardState extends State<MonthlyPaySummaryCard> {
  bool isExpanded = false;

  Widget _row(String title, String value, double scale,
      {bool bold = false}) {
    return Padding(
      padding: EdgeInsets.symmetric(vertical: 6 * scale),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            title,
            style: TextStyle(
              fontFamily: 'Inter',
              fontWeight:
              bold ? FontWeight.w600 : FontWeight.w400,
              fontSize: 14 * scale,
            ),
          ),
          Text(
            value,
            style: TextStyle(
              fontFamily: 'Inter',
              fontWeight:
              bold ? FontWeight.w600 : FontWeight.w500,
              fontSize: 14 * scale,
            ),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final scale = widget.scale;

    return Container(
      width: double.infinity,
      padding: EdgeInsets.all(16 * scale),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12 * scale),
        boxShadow: const [
          BoxShadow(
            color: Color(0x59000000),
            blurRadius: 4.5,
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [

          /// Header
          GestureDetector(
            onTap: () {
              setState(() {
                isExpanded = !isExpanded;
              });
            },
            child: Row(
              mainAxisAlignment:
              MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  "Monthly Pay Summary",
                  style: TextStyle(
                    fontFamily: 'Inter',
                    fontWeight: FontWeight.w600,
                    fontSize: 14 * scale,
                  ),
                ),
                Icon(
                  isExpanded
                      ? Icons.keyboard_arrow_up
                      : Icons.keyboard_arrow_down,
                  size: 20 * scale,
                ),
              ],
            ),
          ),

          SizedBox(height: 12 * scale),

          /// Main Amount
          Text(
            widget.amount,
            style: TextStyle(
              fontFamily: 'Inter',
              fontWeight: FontWeight.w700,
              fontSize: 36 * scale,
            ),
          ),

          if (isExpanded) ...[
            SizedBox(height: 20 * scale),

            /// Earnings
            _row("Basic Salary", "₹50,000", scale),
            _row("House Rent Allowance", "₹20,000", scale),
            _row("Travel Allowance", "₹5,000", scale),
            _row("Bonus", "₹10,000", scale),

            SizedBox(height: 8 * scale),
            Divider(),
            _row("Total Earnings", "₹85,000", scale, bold: true),

            SizedBox(height: 16 * scale),

            /// Deductions
            _row("Provident Fund", "₹9,000", scale),
            _row("Income Tax", "₹12,750", scale),
            _row("Professional Tax", "₹3,000", scale),

            SizedBox(height: 8 * scale),
            Divider(),
            _row("Total Deductions", "₹24,750", scale,
                bold: true),
          ],
        ],
      ),
    );
  }
}
