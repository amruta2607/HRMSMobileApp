import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../../core/Theme/app_colors.dart';
import 'package:altroz/feature/alerts/model/alert_model.dart';

class AlertCard extends StatelessWidget {
  final AlertModel alert;
  final VoidCallback onView;
  final VoidCallback onApprove;
  final VoidCallback onReject;
  final VoidCallback onMarkRead;
  final bool isTask;

  const AlertCard({
    super.key,
    required this.alert,
    required this.onView,
    required this.onApprove,
    required this.onReject,
    required this.onMarkRead,
    required this.isTask,
  });

  void _showDetailDialog(BuildContext context, double scale) {
    showDialog(
      context: context,
      barrierDismissible: true,
      builder: (ctx) {
        return Dialog(
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(14 * scale),
          ),
          insetPadding:
          const EdgeInsets.symmetric(horizontal: 24, vertical: 40),
          child: Padding(
            padding: EdgeInsets.all(18 * scale),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Expanded(
                      child: Text(
                        alert.title,
                        style: TextStyle(
                          fontSize: 15 * scale,
                          fontWeight: FontWeight.w700,
                          color: AppColors.textDark,
                        ),
                      ),
                    ),
                    GestureDetector(
                      onTap: () {
                        Navigator.of(ctx).pop();
                        if (alert.status != "Read") onMarkRead();
                      },
                      child: const Icon(Icons.close,
                          size: 18, color: Colors.black38),
                    ),
                  ],
                ),
                const SizedBox(height: 10),
                const Divider(height: 1),
                const SizedBox(height: 10),
                Text(
                  alert.message,
                  style: TextStyle(
                    fontSize: 13 * scale,
                    color: AppColors.textGrey,
                    height: 1.5,
                  ),
                ),
                const SizedBox(height: 16),
                SizedBox(
                  width: double.infinity,
                  height: 38 * scale,
                  child: ElevatedButton(
                    onPressed: () {
                      Navigator.of(ctx).pop();
                      if (alert.status != "Read") onMarkRead();
                    },
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.primaryBlue,
                      elevation: 0,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(8 * scale),
                      ),
                    ),
                    child: Text(
                      'Close',
                      style: TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.w600,
                        fontSize: 13 * scale,
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final scale = (MediaQuery.of(context).size.width / 402).clamp(0.85, 1.1);
    final isActionable = isTask && !alert.isRead;
    final isUnread = alert.status == "Unread";

    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(10),
        border: Border.all(
          color: isUnread
              ? AppColors.primaryBlue.withOpacity(0.3)
              : AppColors.workspaceCardBorder,
          width: 1.2,
        ),
        boxShadow: [
          BoxShadow(
            color: AppColors.workspaceCardShadow.withOpacity(0.04),
            offset: const Offset(0, 3),
            blurRadius: 8,
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // ── Title Row ──────────────────────────────────────────────
          Row(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              if (isUnread)
                Container(
                  width: 7,
                  height: 7,
                  margin: const EdgeInsets.only(right: 6),
                  decoration: BoxDecoration(
                    color: AppColors.primaryBlue,
                    shape: BoxShape.circle,
                  ),
                ),
              Expanded(
                child: Text(
                  alert.title,
                  style: TextStyle(
                    fontSize: 13.5 * scale,
                    fontWeight: FontWeight.w700,
                    color: AppColors.textDark,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
            ],
          ),
          const SizedBox(height: 4),

          // ── Message ────────────────────────────────────────────────
          Text(
            alert.message,
            style: TextStyle(
              fontSize: 12 * scale,
              color: AppColors.textGrey,
              height: 1.4,
            ),
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
          ),
          const SizedBox(height: 8),

          // ── Bottom Row ─────────────────────────────────────────────
          Row(
            children: [
              // Date on left
              Expanded(
                child: Text(
                  _formatDate(alert.insertDate),
                  style: TextStyle(
                    fontSize: 10.5 * scale,
                    color: AppColors.textLight,
                    fontWeight: FontWeight.w400,
                  ),
                ),
              ),

              // Action buttons on right
              if (isActionable) ...[
                _ActionButton(
                  label: 'Approve',
                  onPressed: onApprove,
                  backgroundColor: const Color(0xffdcfce7),
                  textColor: const Color(0xff15803d),
                  scale: scale,
                ),
                const SizedBox(width: 6),
                _ActionButton(
                  label: 'Reject',
                  onPressed: onReject,
                  backgroundColor: const Color(0xffffe4e4),
                  textColor: const Color(0xffc62828),
                  scale: scale,
                ),
                const SizedBox(width: 6),
              ],

              // View button
              SizedBox(
                height: 28 * scale,
                width: 64 * scale,
                child: isUnread
                    ? ElevatedButton(
                  onPressed: () => _showDetailDialog(context, scale),
                  style: ElevatedButton.styleFrom(
                    padding: EdgeInsets.zero,
                    backgroundColor: AppColors.primaryBlue,
                    elevation: 0,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(6),
                    ),
                  ),
                  child: Text(
                    'View',
                    style: TextStyle(
                      color: Colors.white,
                      fontWeight: FontWeight.w600,
                      fontSize: 12 * scale,
                    ),
                  ),
                )
                    : OutlinedButton(
                  onPressed: () => _showDetailDialog(context, scale),
                  style: OutlinedButton.styleFrom(
                    padding: EdgeInsets.zero,
                    side: BorderSide(
                        color: AppColors.primaryBlue.withOpacity(0.6)),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(6),
                    ),
                  ),
                  child: Text(
                    'View',
                    style: TextStyle(
                      color: AppColors.primaryBlue,
                      fontWeight: FontWeight.w600,
                      fontSize: 12 * scale,
                    ),
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  String _formatDate(String dateStr) {
    try {
      if (dateStr == "0001-01-01T00:00:00") return "N/A";
      final dt = DateTime.parse(dateStr);
      return DateFormat('d/M/yyyy, hh:mm a').format(dt);
    } catch (_) {
      return dateStr;
    }
  }
}

// ─── Reusable Action Button ───────────────────────────────────────────────────
class _ActionButton extends StatelessWidget {
  final String label;
  final VoidCallback onPressed;
  final Color backgroundColor;
  final Color textColor;
  final double scale;

  const _ActionButton({
    required this.label,
    required this.onPressed,
    required this.backgroundColor,
    required this.textColor,
    required this.scale,
  });

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 28 * scale,
      child: ElevatedButton(
        onPressed: onPressed,
        style: ElevatedButton.styleFrom(
          padding: const EdgeInsets.symmetric(horizontal: 12),
          backgroundColor: backgroundColor,
          elevation: 0,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(6),
          ),
        ),
        child: Text(
          label,
          style: TextStyle(
            color: textColor,
            fontWeight: FontWeight.w600,
            fontSize: 12 * scale,
            height: 1.0,
          ),
        ),
      ),
    );
  }
}