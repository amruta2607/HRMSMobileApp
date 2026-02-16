import 'package:flutter/material.dart';

class EmployeeProvidentFundCard extends StatefulWidget {
  final double scale;
  final String totalAmount;
  final String myShare;
  final String employerShare;

  const EmployeeProvidentFundCard({
    super.key,
    required this.scale,
    required this.totalAmount,
    required this.myShare,
    required this.employerShare,
  });

  @override
  State<EmployeeProvidentFundCard> createState() =>
      _EmployeeProvidentFundCardState();
}

class _EmployeeProvidentFundCardState
    extends State<EmployeeProvidentFundCard> {
  bool isExpanded = false;

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
                  "Employee Provident Fund",
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

          /// Total Amount
          Text(
            widget.totalAmount,
            style: TextStyle(
              fontFamily: 'Inter',
              fontWeight: FontWeight.w700,
              fontSize: 36 * scale,
            ),
          ),

          /// Expanded Content
          if (isExpanded) ...[
            SizedBox(height: 16 * scale),

            /// My Share
            Container(
              width: double.infinity,
              padding: EdgeInsets.symmetric(
                horizontal: 12 * scale,
                vertical: 10 * scale,
              ),
              decoration: BoxDecoration(
                color: const Color(0x4D42A5F5),
                borderRadius:
                BorderRadius.circular(8 * scale),
              ),
              child: Row(
                mainAxisAlignment:
                MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    "My Share",
                    style: TextStyle(
                      fontWeight: FontWeight.w600,
                      fontSize: 14 * scale,
                    ),
                  ),
                  Text(
                    widget.myShare,
                    style: TextStyle(
                      fontWeight: FontWeight.w600,
                      fontSize: 14 * scale,
                    ),
                  ),
                ],
              ),
            ),

            SizedBox(height: 10 * scale),

            /// Employer Share
            Container(
              width: double.infinity,
              padding: EdgeInsets.symmetric(
                horizontal: 12 * scale,
                vertical: 10 * scale,
              ),
              decoration: BoxDecoration(
                color: const Color(0x80AFF8CC),
                borderRadius:
                BorderRadius.circular(8 * scale),
              ),
              child: Row(
                mainAxisAlignment:
                MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    "Employer Share",
                    style: TextStyle(
                      fontWeight: FontWeight.w600,
                      fontSize: 14 * scale,
                    ),
                  ),
                  Text(
                    widget.employerShare,
                    style: TextStyle(
                      fontWeight: FontWeight.w600,
                      fontSize: 14 * scale,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }
}
