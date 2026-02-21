import 'package:flutter/material.dart';

import 'package:intl/intl.dart';

import '../model/monthly_summary_model.dart';

class MonthlyPaySummaryCard extends StatefulWidget {
  final double scale;
  final MonthlySummaryModel? data;
  final bool isLoading;
  final DateTime monthYear;

  const MonthlyPaySummaryCard({
    super.key,
    required this.scale,
    this.data,
    this.isLoading = false,
    required this.monthYear,
  });

  @override
  State<MonthlyPaySummaryCard> createState() =>
      _MonthlyPaySummaryCardState();
}

class _MonthlyPaySummaryCardState extends State<MonthlyPaySummaryCard> {
  bool isExpanded = false;

  String _formatAmount(double amount) {
    // Convert to string to avoid floating point math issues
    String str = amount.toString();
    if (str.contains('.')) {
      List<String> parts = str.split('.');
      String whole = parts[0];
      String decimal = parts[1];

      // Truncate to 2 decimal places
      if (decimal.length > 2) {
        decimal = decimal.substring(0, 2);
      }

      // Remove trailing zeros from decimal part if any
      if (decimal == "00" || decimal == "0") {
        return whole;
      } else if (decimal.endsWith('0')) {
        return "$whole.${decimal.substring(0, 1)}";
      }

      return "$whole.$decimal";
    }
    return str;
  }

  Widget _row(String title, String value, double scale,
      {bool bold = false}) {
    return Padding(
      padding: EdgeInsets.symmetric(vertical: 6 * scale),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Expanded(
            child: Text(
              title,
              style: TextStyle(
                fontFamily: 'Inter',
                fontWeight:
                bold ? FontWeight.w600 : FontWeight.w400,
                fontSize: 14 * scale,
              ),
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
    final data = widget.data;

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
                  "Monthly Pay Summary ",
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
          widget.isLoading
              ? const CircularProgressIndicator()
              : Text(
            data != null
                ? "₹${_formatAmount(data!.takeHomePay)}"
                : "₹0",
            style: TextStyle(
              fontFamily: 'Inter',
              fontWeight: FontWeight.w700,
              fontSize: 36 * scale,
            ),
          ),

          if (isExpanded && data != null) ...[
            SizedBox(height: 20 * scale),

            /// Earnings
            ...data!.incomes.map((income) => _row(
                income.name, "₹${_formatAmount(income.amount)}", scale)),

            SizedBox(height: 8 * scale),
            Divider(),
            _row("Total Earnings", "₹${_formatAmount(data!.gross)}", scale,
                bold: true),

            SizedBox(height: 16 * scale),

            /// Deductions
            ...data!.deductions.map((deduction) => _row(deduction.name,
                "₹${_formatAmount(deduction.amount)}", scale)),

            SizedBox(height: 8 * scale),
            Divider(),
            _row("Total Deductions", "₹${_formatAmount(data!.totalDeduction)}",
                scale,
                bold: true),
          ],
        ],
      ),
    );
  }
}
