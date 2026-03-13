import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../../core/Theme/app_colors.dart';
import 'package:altroz/feature/alerts/model/alert_model.dart';

class AlertCard extends StatelessWidget {
  final AlertModel alert;
  final VoidCallback onView;
  final VoidCallback onApprove;
  final VoidCallback onReject;
  final bool isTask;

  const AlertCard({
    super.key,
    required this.alert,
    required this.onView,
    required this.onApprove,
    required this.onReject,
    required this.isTask,
  });

  @override
  Widget build(BuildContext context) {
    final scale = (MediaQuery.of(context).size.width / 402).clamp(0.85, 1.1);
    final isActionable = isTask && !alert.isRead;

    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.workspaceCardBorder, width: 1.5),
        boxShadow: [
          BoxShadow(
            color: AppColors.workspaceCardShadow.withOpacity(0.05),
            offset: const Offset(0, 4),
            blurRadius: 10,
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            alert.title,
            style: TextStyle(
              fontSize: 16 * scale,
              fontWeight: FontWeight.bold,
              color: AppColors.textDark,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            alert.message,
            style: TextStyle(
              fontSize: 14 * scale,
              color: AppColors.textGrey,
              height: 1.4,
            ),
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              Expanded(
                child: SizedBox(
                  height: 36 * scale,
                  child: OutlinedButton(
                    onPressed: onView,
                    style: OutlinedButton.styleFrom(
                      side: const BorderSide(color: AppColors.primaryBlue),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(8),
                      ),
                    ),
                    child: Text(
                      'View',
                      style: TextStyle(
                        color: AppColors.primaryBlue,
                        fontWeight: FontWeight.w600,
                        fontSize: 14 * scale,
                      ),
                    ),
                  ),
                ),
              ),
              if (isActionable) ...[
                const SizedBox(width: 8),
                Expanded(
                  child: SizedBox(
                    height: 36 * scale,
                    child: ElevatedButton(
                      onPressed: onApprove,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xff98f1b4),
                        elevation: 0,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8),
                        ),
                      ),
                      child: Text(
                        'Approve',
                        style: TextStyle(
                          color: const Color(0xff15803d),
                          fontWeight: FontWeight.w600,
                          fontSize: 14 * scale,
                        ),
                      ),
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: SizedBox(
                    height: 36 * scale,
                    child: ElevatedButton(
                      onPressed: onReject,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xffffa1a1),
                        elevation: 0,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8),
                        ),
                      ),
                      child: Text(
                        'Reject',
                        style: TextStyle(
                          color: const Color(0xffc62828),
                          fontWeight: FontWeight.w600,
                          fontSize: 14 * scale,
                        ),
                      ),
                    ),
                  ),
                ),
              ],
            ],
          ),
          const SizedBox(height: 12),
          Align(
            alignment: Alignment.bottomRight,
            child: Text(
              _formatDate(alert.insertDate),
              style: TextStyle(
                fontSize: 12 * scale,
                color: AppColors.textLight,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
        ],
      ),
    );
  }

  String _formatDate(String dateStr) {
    try {
      if (dateStr == "0001-01-01T00:00:00") return "N/A";
      final dt = DateTime.parse(dateStr);
      return DateFormat('d/M/yyyy, hh:mm:ss a').format(dt);
    } catch (_) {
      return dateStr;
    }
  }
}
