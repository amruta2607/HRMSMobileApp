import 'package:flutter/material.dart';

import '../../../core/Utils/services/payroll_service/payroll_service.dart';
import '../../Navigation/main_navigation_screen.dart';
import '../../Navigation/navigation_bar.dart';
import '../../Reuse_Widgets/header_bg.dart';
import '../model/pay_slip_model.dart';

class AllPayslipsScreen extends StatefulWidget {
  const AllPayslipsScreen({super.key});

  @override
  State<AllPayslipsScreen> createState() => _AllPayslipsScreenState();
}

class _AllPayslipsScreenState extends State<AllPayslipsScreen> {
  int? _selectedYear;
  List<PaySlipMonthModel> _months = [];
  List<int> _years = [];
  bool _isLoading = false;
  int? _downloadingMonth;

  @override
  void initState() {
    super.initState();
    _fetchInitialData();
  }

  Future<void> _fetchInitialData() async {
    setState(() => _isLoading = true);
    final years = await PayrollService.getPaySlipYears();
    if (mounted && years != null && years.isNotEmpty) {
      setState(() {
        _years = years;
        _selectedYear = years.first;
      });
      await _fetchMonths();
    } else {
      setState(() => _isLoading = false);
    }
  }

  Future<void> _fetchMonths() async {
    if (_selectedYear == null) return;
    setState(() => _isLoading = true);
    final months = await PayrollService.getPaySlipMonths(_selectedYear!);
    if (mounted) {
      setState(() {
        _months = months ?? [];
        _isLoading = false;
      });
    }
  }

  Future<void> _handleDownload(PaySlipMonthModel monthModel) async {
    if (_downloadingMonth != null || _selectedYear == null) return;

    setState(() {
      _downloadingMonth = monthModel.month;
    });

    try {
      final fileName = "PaySlip_${monthModel.monthName.replaceAll(' ', '_')}.pdf";

      final success = await PayrollService.downloadPaySlip(
        month: monthModel.month,
        year: _selectedYear!,
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
          _downloadingMonth = null;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return Scaffold(
      backgroundColor: Colors.white,
      bottomNavigationBar: CustomNavigationBar(
        currentIndex: 0,
        onChanged: (index) {
          Navigator.pushAndRemoveUntil(
            context,
            MaterialPageRoute(
              builder: (context) => MainNavigationScreen(initialIndex: index),
            ),
                (route) => false,
          );
        },
      ),
      body: SafeArea(
        child: Column(
          children: [
            HeaderBackground(
              scale: scale,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      InkWell(
                        onTap: () => Navigator.pop(context),
                        child: const Icon(Icons.arrow_back_ios, size: 18),
                      ),
                      Text(
                        "Payslips",
                        style: TextStyle(
                          fontSize: 24 * scale,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),

            Expanded(
              child: SingleChildScrollView(
                padding: EdgeInsets.symmetric(horizontal: 20 * scale),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    SizedBox(height: 20 * scale),

                    if (_selectedYear != null) _yearDropdown(scale),

                    SizedBox(height: 30 * scale),

                    if (_selectedYear != null)
                      Text(
                        "$_selectedYear PAY SLIPS",
                        style: TextStyle(
                          fontFamily: 'Inter',
                          fontWeight: FontWeight.w600,
                          fontSize: 12 * scale,
                          letterSpacing: 1,
                          color: const Color(0xFF94A3B8),
                        ),
                      ),

                    SizedBox(height: 16 * scale),

                    if (_isLoading)
                      const Center(child: CircularProgressIndicator())
                    else if (_months.isEmpty)
                      Center(
                        child: Text(
                          "No payslips found for ${_selectedYear ?? ''}",
                          style: TextStyle(
                            fontFamily: 'Inter',
                            fontSize: 14 * scale,
                            color: Colors.grey,
                          ),
                        ),
                      )
                    else
                      ..._months.map((month) => _paySlipTile(month, scale)),

                    SizedBox(height: 40 * scale),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _yearDropdown(double scale) {
    return GestureDetector(
      onTap: _showYearDialog,
      child: Container(
        height: 52 * scale,
        padding: EdgeInsets.symmetric(horizontal: 16 * scale),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12 * scale),
          border: Border.all(color: const Color(0xFF0F172A), width: 1),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              "$_selectedYear",
              style: TextStyle(
                fontFamily: 'Inter',
                fontWeight: FontWeight.w500,
                fontSize: 14 * scale,
                color: const Color(0xFF0F172A),
              ),
            ),
            const Icon(Icons.keyboard_arrow_down),
          ],
        ),
      ),
    );
  }

  void _showYearDialog() {
    final width = MediaQuery.of(context).size.width;
    final height = MediaQuery.of(context).size.height;
    final scale = width / 375;

    showDialog(
      context: context,
      builder: (dialogContext) {
        return Dialog(
          backgroundColor: Colors.transparent,
          insetPadding: EdgeInsets.symmetric(horizontal: 24 * scale),
          child: Material(
            color: Colors.white,
            borderRadius: BorderRadius.circular(16 * scale),
            elevation: 24,
            clipBehavior: Clip.antiAlias,
            child: Container(
              width: width * 0.85,
              constraints: BoxConstraints(maxHeight: height * 0.6),
              padding: EdgeInsets.symmetric(vertical: 20 * scale),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Padding(
                    padding: EdgeInsets.symmetric(horizontal: 24 * scale),
                    child: Text(
                      "Select Year:",
                      style: TextStyle(
                        fontFamily: 'Inter',
                        fontWeight: FontWeight.w700,
                        fontSize: 18 * scale,
                        color: const Color(0xFF0F172A),
                      ),
                    ),
                  ),
                  SizedBox(height: 20 * scale),
                  Flexible(
                    child: SingleChildScrollView(
                      child: Column(
                        mainAxisSize: MainAxisSize.min,
                        children: _years.map((year) {
                          final bool isSelected = _selectedYear == year;

                          return Padding(
                            padding: EdgeInsets.only(
                              left: 24 * scale,
                              right: 24 * scale,
                              bottom: 14 * scale,
                            ),
                            child: GestureDetector(
                              onTap: () {
                                setState(() {
                                  _selectedYear = year;
                                });
                                _fetchMonths();
                                Navigator.pop(dialogContext);
                              },
                              child: Container(
                                width: double.infinity,
                                padding: EdgeInsets.symmetric(
                                  horizontal: 16 * scale,
                                  vertical: 16 * scale,
                                ),
                                decoration: BoxDecoration(
                                  color: isSelected ? const Color(0xFFF1F5F9) : Colors.white,
                                  borderRadius: BorderRadius.circular(12 * scale),
                                  border: Border.all(
                                    color: const Color(0xFF5D6063),
                                    width: 1,
                                  ),
                                ),
                                child: Text(
                                  "$year",
                                  style: TextStyle(
                                    fontFamily: 'Inter',
                                    fontWeight: FontWeight.w600,
                                    fontSize: 14 * scale,
                                    color: const Color(0xFF0F172A),
                                  ),
                                ),
                              ),
                            ),
                          );
                        }).toList(),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        );
      },
    );
  }

  Widget _paySlipTile(PaySlipMonthModel monthModel, double scale) {
    final isDownloading = _downloadingMonth == monthModel.month;
    final title = monthModel.monthName;

    return Container(
      height: 48 * scale,
      margin: EdgeInsets.only(bottom: 12 * scale),
      padding: EdgeInsets.symmetric(horizontal: 16 * scale),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12 * scale),
        border: Border.all(color: const Color(0xFFE2E8F0)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 10,
            offset: const Offset(0, 2),
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
              fontSize: 14 * scale,
              fontWeight: FontWeight.w600,
              color: const Color(0xFF1E293B),
            ),
          ),
          InkWell(
            onTap: isDownloading ? null : () => _handleDownload(monthModel),
            child: isDownloading
                ? SizedBox(
              width: 18 * scale,
              height: 18 * scale,
              child: const CircularProgressIndicator(
                strokeWidth: 2,
                color: Color(0xFF0F62FE),
              ),
            )
                : Icon(
              Icons.download_outlined,
              size: 20 * scale,
              color: const Color(0xFF0F62FE),
            ),
          ),
        ],
      ),
    );
  }
}