import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';
import '../Reuse_Widgets/header_bg.dart';
import '../Login/Widgets/toggle_item.dart';
import 'package:altroz/core/Utils/services/alert_service/alert_service.dart';
import 'package:altroz/feature/alerts/model/alert_model.dart';
import 'package:altroz/feature/alerts/widgets/alert_card.dart';
import '../Reuse_Widgets/home_screen_constent.dart';
import 'package:altroz/feature/Navigation/main_navigation_screen.dart';

class AlertsScreen extends StatefulWidget {
  const AlertsScreen({super.key});

  @override
  State<AlertsScreen> createState() => _AlertsScreenState();
}

class _AlertsScreenState extends State<AlertsScreen> {
  bool isTasksSelected = false;
  List<AlertModel> _alerts = [];
  bool _isLoading = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadAlerts();
  }

  Future<void> _loadAlerts() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final result = await AlertService.getAlerts();
      if (mounted) {
        setState(() {
          if (result != null) {
            _alerts = result;
          } else {
            _error = "Failed to load alerts";
          }
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _error = e.toString();
          _isLoading = false;
        });
      }
    }
  }

  int get _unreadCount => _alerts.where((a) => !a.isRead).length;

  Future<void> _handleAction(AlertModel alert, bool isApprove) async {
    setState(() => _isLoading = true);

    final result = await AlertService.processRequest(
      alertId: alert.id,
      eventId: alert.eventId,
      eventName: alert.title,
      reason: isApprove ? "Approved via Alerts" : "Rejected via Alerts",
      isApprove: isApprove,
      insertUserId: alert.insertUserId,
    );

    if (mounted) {
      setState(() => _isLoading = false);

      if (result['success']) {
        setState(() {
          _alerts.removeWhere((a) => a.id == alert.id);
        });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text("${isApprove ? 'Approved' : 'Rejected'} successfully"),
            backgroundColor: Colors.green,
          ),
        );
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text("Operation failed: ${result['message']}"),
            backgroundColor: Colors.red,
          ),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final scale = (MediaQuery.of(context).size.width / 402).clamp(0.85, 1.1);

    return HomeScreenConstent(
      body: Column(
        children: [
          HeaderBackground(
            scale: scale,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                /// Back + Title (Payroll Style)
                Row(
                  crossAxisAlignment: CrossAxisAlignment.center,
                  children: [
                    InkWell(
                      onTap: () {
                        Navigator.pushAndRemoveUntil(
                          context,
                          MaterialPageRoute(
                            builder: (context) => const MainNavigationScreen(initialIndex: 0),
                          ),
                              (route) => false,
                        );
                      },
                      child: const Padding(
                        padding: EdgeInsets.only(right: 8.0),
                        child: Icon(
                          Icons.arrow_back_ios,
                          size: 18, // Payroll icon size
                          color: AppColors.textDark,
                        ),
                      ),
                    ),
                    Text(
                      'Alerts',
                      style: TextStyle(
                        fontSize: 24 * scale, // Payroll title size
                        fontWeight: FontWeight.w700,
                        color: AppColors.textDark,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 4),
                Padding(
                  padding: EdgeInsets.only(left: 18 * scale), // Aligned under title text
                  child: Text(
                    '$_unreadCount New notifications',
                    style: TextStyle(
                      fontSize: 14 * scale,
                      fontWeight: FontWeight.w500,
                      color: AppColors.textGrey,
                    ),
                  ),
                ),
              ],
            ),
          ),

          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
            child: Container(
              height: 52 * scale,
              decoration: BoxDecoration(
                color: const Color(0xffF1F4F9),
                borderRadius: BorderRadius.circular(14 * scale),
              ),
              child: Row(
                children: [
                  ToggleItem(
                    text: "Tasks",
                    selected: isTasksSelected,
                    onTap: () => setState(() => isTasksSelected = true),
                  ),
                  ToggleItem(
                    text: "Notifications",
                    selected: !isTasksSelected,
                    onTap: () => setState(() => isTasksSelected = false),
                  ),
                ],
              ),
            ),
          ),

          Expanded(
            child: _isLoading
                ? const Center(child: CircularProgressIndicator())
                : _error != null
                ? Center(child: Text(_error!))
                : _alerts.isEmpty
                ? const Center(child: Text("No notifications found"))
                : () {
              final filtered = _alerts.where((a) {
                final isLeave = a.title.contains("Leave Request");
                // In future, more task types can be added here
                return isTasksSelected ? isLeave : !isLeave;
              }).toList();

              if (filtered.isEmpty) {
                return Center(child: Text("No ${isTasksSelected ? 'tasks' : 'notifications'} found"));
              }

              return ListView.builder(
                padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
                itemCount: filtered.length,
                itemBuilder: (context, index) {
                  final alert = filtered[index];
                  return AlertCard(
                    alert: alert,
                    isTask: isTasksSelected,
                    onView: () {
                      // Implement view logic
                    },
                    onApprove: () => _handleAction(alert, true),
                    onReject: () => _handleAction(alert, false),
                  );
                },
              );
            }(),
          ),
        ],
      ),
    );
  }
}
