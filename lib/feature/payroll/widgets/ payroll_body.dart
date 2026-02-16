import 'package:flutter/material.dart';

import '../../../core/Utils/services/payroll_service/payroll_service.dart';
import '../model/provident_fund_model.dart';
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

  @override
  void initState() {
    super.initState();
    _providentFundFuture = PayrollService.getProvidentFund();
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
            amount: "₹60,250",
          ),

          SizedBox(height: 20 * scale),

          FutureBuilder<ProvidentFundModel?>(
            future: _providentFundFuture,
            builder: (context, snapshot) {
              final data = snapshot.data;
              // Default to 0 if data is loading or null
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

          RecentPaySlipsSection(scale: scale),

          SizedBox(height: 40 * scale),
        ],
      ),
    );
  }
}
