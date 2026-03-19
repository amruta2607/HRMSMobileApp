import 'package:flutter/material.dart';
import '../../../../core/Utils/services/payroll_service/payroll_service.dart';
import '../all_payslips_screen/all_payslips_screen.dart';
import '../model/pay_slip_model.dart';
import 'package:intl/intl.dart';

class RecentPaySlipsSection extends StatefulWidget {
  final double scale;
  final List<PaySlipModel> paySlips;
  final bool isLoading;

  const RecentPaySlipsSection({
    super.key,
    required this.scale,
    required this.paySlips,
    this.isLoading = false,
  });

  @override
  State<RecentPaySlipsSection> createState() => _RecentPaySlipsSectionState();
}

class _RecentPaySlipsSectionState extends State<RecentPaySlipsSection> {
  // To track which item is currently downloading
  int? _downloadingId;

  Future<void> _handleDownload(PaySlipModel paySlip) async {
    if (_downloadingId != null) return; // Prevent multiple concurrent downloads

    setState(() {
      _downloadingId = paySlip.id;
    });

    try {
      final monthName = _getMonthName(paySlip.payrollMonth);
      final fileName = "PaySlip_${monthName}_${paySlip.payrollYear}.pdf";

      final success = await PayrollService.downloadPaySlip(
        month: paySlip.payrollMonth,
        year: paySlip.payrollYear,
        fileName: fileName,
      );

      if (!mounted) return;

      if (!success) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Failed to download Pay Slip')),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _downloadingId = null;
        });
      }
    }
  }

  String _getMonthName(int month) {
    const months = [
      "January", "February", "March", "April", "May", "June",
      "July", "August", "September", "October", "November", "December"
    ];
    if (month >= 1 && month <= 12) {
      return months[month - 1];
    }
    return "";
  }


  Widget _paySlipTile(PaySlipModel paySlip, BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;
    final isDownloading = _downloadingId == paySlip.id;
    final monthName = paySlip.payrollMonthName ?? _getMonthName(paySlip.payrollMonth);
    final title = "$monthName ${paySlip.payrollYear}";

    return Container(
      width: screenWidth - (48 * widget.scale), // 24px left + 24px right
      height: 35 * widget.scale,
      margin: EdgeInsets.only(bottom: 12 * widget.scale),
      padding: EdgeInsets.symmetric(horizontal: 12 * widget.scale),
      decoration: BoxDecoration(
        color: const Color(0xFFFFFFFF),
        borderRadius: BorderRadius.circular(4 * widget.scale),
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
            title,
            style: TextStyle(
              fontFamily: 'Inter',
              fontSize: 12 * widget.scale,
              fontWeight: FontWeight.w500,
            ),
          ),
          InkWell(
            onTap: isDownloading ? null : () => _handleDownload(paySlip),
            child: isDownloading
                ? SizedBox(
              width: 16 * widget.scale,
              height: 16 * widget.scale,
              child: CircularProgressIndicator(
                strokeWidth: 2,
                color: const Color(0xFF0F62FE),
              ),
            )
                : Image.asset(
              "img/download.png",
              width: 16 * widget.scale,
              height: 16 * widget.scale,
              color: const Color(0xFF0F62FE),
            ),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    // If loading, show skeletons or loader?
    // For now, let's just show header and maybe loader if strictly loading
    // But usually passed list might be empty initially.

    // Sort pay slips by year descending, then month descending
    final sortedPaySlips = List<PaySlipModel>.from(widget.paySlips);
    sortedPaySlips.sort((a, b) {
      if (a.payrollYear != b.payrollYear) {
        return b.payrollYear.compareTo(a.payrollYear);
      }
      return b.payrollMonth.compareTo(a.payrollMonth);
    });

    // Take top 3 for "Recent"
    final recentPaySlips = sortedPaySlips.take(3).toList();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(height: 24 * widget.scale),

        Text(
          "RECENT PAY SLIPS",
          style: TextStyle(
            fontFamily: 'Inter',
            fontWeight: FontWeight.w600,
            fontSize: 12 * widget.scale,
            letterSpacing: 1,
            color: Colors.grey.shade600,
          ),
        ),

        SizedBox(height: 16 * widget.scale),

        if (widget.isLoading)
          const Center(child: Padding(
            padding: EdgeInsets.all(8.0),
            child: CircularProgressIndicator(),
          ))
        else if (recentPaySlips.isEmpty)
          Padding(
            padding: EdgeInsets.all(8.0 * widget.scale),
            child: Text(
              "No pay slips available properly.",
              style: TextStyle(
                fontFamily: 'Inter',
                fontSize: 12 * widget.scale,
                color: Colors.grey,
              ),
            ),
          )
        else
          ...recentPaySlips.map((paySlip) => _paySlipTile(paySlip, context)),

        SizedBox(height: 8 * widget.scale),

        if (sortedPaySlips.isNotEmpty)
          Align(
            alignment: Alignment.centerRight,
            child: InkWell(
              onTap: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => const AllPayslipsScreen(),
                  ),
                );
              },
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    "View All",
                    style: TextStyle(
                      fontFamily: 'Inter',
                      fontWeight: FontWeight.w700,
                      fontSize: 12 * widget.scale,
                      height: 14.07 / 12,
                      color: const Color(0xFF0F62FE),
                    ),
                  ),
                  SizedBox(width: 4 * widget.scale),
                  Icon(
                    Icons.arrow_forward_ios,
                    size: 12 * widget.scale,
                    color: const Color(0xFF0F62FE),
                  ),
                ],
              ),
            ),
          ),


        SizedBox(height: 24 * widget.scale),
      ],
    );
  }
}