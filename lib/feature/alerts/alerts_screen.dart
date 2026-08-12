import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';
import '../Reuse_Widgets/header_bg.dart';
import '../Login/Widgets/toggle_item.dart';
import 'package:altroz/core/Utils/services/alert_service/alert_service.dart';
import 'package:altroz/feature/alerts/model/alert_model.dart';
import 'package:altroz/feature/alerts/widgets/alert_card.dart';
import '../Reuse_Widgets/home_screen_constent.dart';
import 'package:altroz/feature/Navigation/main_navigation_screen.dart';
import 'package:altroz/core/Utils/services/alert_service/alert_count_service.dart';
import 'package:altroz/core/Utils/services/connectivity_service.dart';

class AlertsScreen extends StatefulWidget {
  final bool initialShowTasks;
  final VoidCallback? onBack;
  const AlertsScreen({super.key, this.initialShowTasks = false, this.onBack});

  @override
  State<AlertsScreen> createState() => _AlertsScreenState();
}

class _AlertsScreenState extends State<AlertsScreen> {
  late bool isTasksSelected;
  List<AlertModel> _alerts = [];
  int _apiUnreadCount = 0;
  bool _isLoading = false;
  String? _error = null;

  @override
  void initState() {
    super.initState();
    isTasksSelected = widget.initialShowTasks;
    _loadAlerts();
    ConnectivityService.onReconnected(_handleReconnection);
  }

  void _handleReconnection() {
    if (mounted) {
      print('🌐 Connection restored, refreshing alerts...');
      _loadAlerts();
    }
  }

  // ✅ React when parent changes initialShowTasks
  @override
  void didUpdateWidget(AlertsScreen oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.initialShowTasks != widget.initialShowTasks) {
      setState(() => isTasksSelected = widget.initialShowTasks);
    }
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
            _alerts = result['alerts'] ?? [];
            _apiUnreadCount = result['unreadCount'] ?? 0;
            AlertCountService.updateCount(_apiUnreadCount);
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

  int get _unreadCount => _apiUnreadCount;

  bool _isTask(AlertModel a) => AlertService.isTask(a);

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
          // If it was unread, decrease count
          if (alert.status == "Unread") {
            _apiUnreadCount = (_apiUnreadCount - 1).clamp(0, 999);
            AlertCountService.updateCount(_apiUnreadCount);
          }
        });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
                "${isApprove ? 'Approved' : 'Rejected'} successfully"),
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

  Future<void> _handleMarkRead(AlertModel alert) async {
    if (alert.status != "Unread") return; // Already read

    final success = await AlertService.markAsRead(alert.id);
    if (success && mounted) {
      setState(() {
        // Find and update the alert status locally
        final index = _alerts.indexWhere((a) => a.id == alert.id);
        if (index != -1) {
          final oldAlert = _alerts[index];
          _alerts[index] = AlertModel(
            id: oldAlert.id,
            organisationId: oldAlert.organisationId,
            userId: oldAlert.userId,
            eventId: oldAlert.eventId,
            title: oldAlert.title,
            message: oldAlert.message,
            isRead: true, // Mark as read
            isActive: oldAlert.isActive,
            status: "Read",
            insertDate: oldAlert.insertDate,
            insertUserId: oldAlert.insertUserId,
            updateDate: oldAlert.updateDate,
            updateUserId: oldAlert.updateUserId,
          );
        }
        _apiUnreadCount = (_apiUnreadCount - 1).clamp(0, 999);
        AlertCountService.updateCount(_apiUnreadCount);
      });
    }
  }

  @override
  void dispose() {
    ConnectivityService.removeOnReconnected(_handleReconnection);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final scale = (MediaQuery.of(context).size.width / 402).clamp(0.85, 1.1);

    final tasksUnreadCount = _alerts.where((a) => _isTask(a) && a.status == "Unread").length;
    final notificationsUnreadCount = _alerts.where((a) => !_isTask(a) && a.status == "Unread").length;

    return HomeScreenConstent(
      body: Column(
        children: [
          HeaderBackground(
            scale: scale,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Material(
                  color: Colors.transparent,
                  child: InkWell(
                    onTap: widget.onBack ?? () {
                      Navigator.pushAndRemoveUntil(
                        context,
                        MaterialPageRoute(
                          builder: (context) =>
                          const MainNavigationScreen(initialIndex: 0),
                        ),
                            (route) => false,
                      );
                    },
                    borderRadius: BorderRadius.circular(8),
                    splashColor: AppColors.textDark.withOpacity(0.1),
                    highlightColor: AppColors.textDark.withOpacity(0.05),
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.center,
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Padding(
                          padding:
                          EdgeInsets.only(right: 8.0, top: 4, bottom: 4),
                          child: Icon(
                            Icons.arrow_back_ios,
                            size: 18,
                            color: AppColors.textDark,
                          ),
                        ),
                        Text(
                          'Alerts',
                          style: TextStyle(
                            fontSize: 24 * scale,
                            fontWeight: FontWeight.w700,
                            color: AppColors.textDark,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 4),
                Padding(
                  padding: EdgeInsets.only(left: 18 * scale),
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
                    text: "Tasks ($tasksUnreadCount)",
                    selected: isTasksSelected,
                    onTap: () => setState(() => isTasksSelected = true),
                  ),
                  ToggleItem(
                    text: "Notifications ($notificationsUnreadCount)",
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
                ? RefreshIndicator(
              onRefresh: _loadAlerts,
              child: ListView(
                children: [
                  SizedBox(
                    height: 300,
                    child: Center(child: Text(_error!)),
                  ),
                ],
              ),
            )
                : _alerts.isEmpty
                ? RefreshIndicator(
              onRefresh: _loadAlerts,
              child: ListView(
                children: const [
                  SizedBox(
                    height: 300,
                    child: Center(
                      child: Text("No notifications found"),
                    ),
                  ),
                ],
              ),
            )
                : () {
              final filtered = _alerts.where((a) {
                return isTasksSelected ? _isTask(a) : !_isTask(a);
              }).toList();

              if (filtered.isEmpty) {
                return RefreshIndicator(
                  onRefresh: _loadAlerts,
                  child: ListView(
                    children: [
                      SizedBox(
                        height: 300,
                        child: Center(
                          child: Text(
                            "No ${isTasksSelected ? 'tasks' : 'notifications'} found",
                          ),
                        ),
                      ),
                    ],
                  ),
                );
              }

              return RefreshIndicator(
                onRefresh: _loadAlerts,
                child: ListView.builder(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 20, vertical: 8),
                  itemCount: filtered.length,
                  itemBuilder: (context, index) {
                    final alert = filtered[index];
                    return AlertCard(
                      alert: alert,
                      isTask: isTasksSelected,
                      onView: () {},
                      onMarkRead: () => _handleMarkRead(alert),
                      onApprove: () =>
                          _handleAction(alert, true),
                      onReject: () =>
                          _handleAction(alert, false),
                    );
                  },
                ),
              );
            }(),
          ),
        ],
      ),
    );
  }
}