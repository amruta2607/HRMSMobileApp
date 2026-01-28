import 'package:flutter/material.dart';
import '../../../core/Utils/services/leave_service/leave_service.dart';
import '../apply_leave/apply_leave_screen.dart';
import 'recent_leave_section.dart';
import 'apply_leave_button.dart';

class LeaveBody extends StatefulWidget {
  final VoidCallback? onLeaveApplied;
  const LeaveBody({super.key, this.onLeaveApplied});

  @override
  State<LeaveBody> createState() => _LeaveBodyState();
}

class _LeaveBodyState extends State<LeaveBody> {

  List<Map<String, String>> _recentLeaves = [];

  @override
  void initState() {
    super.initState();
    _loadRecentLeaves();
  }

  Future<void> _loadRecentLeaves() async {
    final leaves = await LeaveService.getRecentLeaves();
    setState(() {
      _recentLeaves = leaves;
    });
  }

  void _addNewLeave(Map<String, dynamic> data) async {
    final newLeave = {
      "title": data['title'].toString(),
      "date": data['date'].toString(),
    };

    await LeaveService.saveRecentLeave(newLeave);

    setState(() {
      _recentLeaves.insert(0, newLeave);
    });

    widget.onLeaveApplied?.call();
  }

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(height: 24 * scale),

        Padding(
          padding: EdgeInsets.symmetric(horizontal: 20 * scale),
          child:ApplyLeaveButton(
            onTap: () async {
              final result = await Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (_) => const ApplyLeaveScreen(),
                ),
              );

              if (result != null && result is Map<String, dynamic>) {
                _addNewLeave(result);
              }
            },
          ),
        ),

        SizedBox(height: 28 * scale),

        /// Recent Leave Section
        Expanded(
          child: RecentLeaveSection(leaves: _recentLeaves),
        ),
      ],
    );
  }
}
