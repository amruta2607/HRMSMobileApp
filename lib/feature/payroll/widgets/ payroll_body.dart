import 'package:flutter/material.dart';

import '../../../core/Utils/services/payroll_service/payroll_service.dart';
import '../model/provident_fund_model.dart';
import '../model/monthly_summary_model.dart';
import '../model/pay_slip_model.dart';
import 'monthly_pay_summary_card.dart';
import 'employee_provident_fund_card.dart';
import 'recent_pay_slips_section.dart';

class PayrollBody extends StatefulWidget {
  const PayrollBody({super.key});

  @override
  State<PayrollBody> createState() => _PayrollBodyState();
}

class _PayrollBodyState extends State<PayrollBody> {
  Future<ProvidentFundModel?>? _providentFundFuture;
  Future<List<PaySlipModel>?>? _paySlipsFuture;

  // Holds the resolved monthly summary (loaded after pay slips are fetched)
  MonthlySummaryModel? _monthlySummary;
  bool _isSummaryLoading = true;

  // The month/year of the last processed payroll (null = not yet known)
  DateTime? _lastPayrollDate;

  @override
  void initState() {
    super.initState();
    _providentFundFuture = PayrollService.getProvidentFund();
    _paySlipsFuture = PayrollService.getPaySlips(year: DateTime.now().year);
    _loadMonthlySummaryFromLastPayroll();
  }

  /// Fetches the latest available payslip for this user (across years),
  /// then loads the monthly summary for that specific month/year.
  Future<void> _loadMonthlySummaryFromLastPayroll() async {
    setState(() => _isSummaryLoading = true);

    try {
      // Get the user's most recent available payslip
      final latestSlip = await PayrollService.getLatestPaySlip();

      if (latestSlip != null) {
        _lastPayrollDate = DateTime(latestSlip.payrollYear, latestSlip.payrollMonth);

        final summary = await PayrollService.getMonthlySummary(
          month: latestSlip.payrollMonth,
          year: latestSlip.payrollYear,
        );

        if (mounted) {
          setState(() {
            _monthlySummary = summary;
            _isSummaryLoading = false;
          });
        }
      } else {
        // No payslips found at all
        if (mounted) {
          setState(() {
            _monthlySummary = null;
            _lastPayrollDate = DateTime.now();
            _isSummaryLoading = false;
          });
        }
      }
    } catch (e) {
      print(' PAYROLL BODY ERROR => $e');
      if (mounted) {
        setState(() => _isSummaryLoading = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return SingleChildScrollView(
      padding: EdgeInsets.symmetric(horizontal: 20 * scale),
      child: Column(
        children: [
          SizedBox(height: 20 * scale),

          MonthlyPaySummaryCard(
            scale: scale,
            data: _monthlySummary,
            isLoading: _isSummaryLoading,
            monthYear: _lastPayrollDate ?? DateTime.now(),
          ),

          SizedBox(height: 20 * scale),

          FutureBuilder<ProvidentFundModel?>(
            future: _providentFundFuture,
            builder: (context, snapshot) {
              final data = snapshot.data;
              final myShare = data?.myShare ?? 0;
              final employerShare = data?.employerShare ?? 0;
              final total = data?.totalProvidentFund ?? 0;

              return EmployeeProvidentFundCard(
                scale: scale,
                totalAmount: "₹$total",
                myShare: "₹$myShare",
                employerShare: "₹$employerShare",
              );
            },
          ),

          FutureBuilder<List<PaySlipModel>?>(
            future: _paySlipsFuture,
            builder: (context, snapshot) {
              return RecentPaySlipsSection(
                scale: scale,
                paySlips: snapshot.data ?? [],
                isLoading: snapshot.connectionState == ConnectionState.waiting,
              );
            },
          ),

          SizedBox(height: 40 * scale),
        ],
      ),
    );
  }
}
