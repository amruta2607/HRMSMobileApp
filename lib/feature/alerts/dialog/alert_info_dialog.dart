import 'package:flutter/material.dart';
import '../../../../core/Theme/app_colors.dart';
import '../model/alert_model.dart';
import 'package:intl/intl.dart';

class AlertInfoDialog extends StatelessWidget {
  final AlertModel alert;

  const AlertInfoDialog({super.key, required this.alert});

  @override
  Widget build(BuildContext context) {
    final scale = (MediaQuery.of(context).size.width / 402).clamp(0.85, 1.1);

    return Dialog(
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16 * scale),
      ),
      elevation: 0,
      backgroundColor: Colors.transparent,
      child: Container(
        padding: EdgeInsets.all(20 * scale),
        decoration: BoxDecoration(
          color: Colors.white,
          shape: BoxShape.rectangle,
          borderRadius: BorderRadius.circular(16 * scale),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.24),
              blurRadius: 10.0,
              offset: const Offset(0.0, 10.0),
            ),
          ],
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Text(
                    alert.title,
                    style: TextStyle(
                      fontSize: 18 * scale,
                      fontWeight: FontWeight.bold,
                      color: AppColors.textDark,
                    ),
                  ),
                ),
                IconButton(
                  onPressed: () => Navigator.pop(context),
                  icon: const Icon(Icons.close),
                  padding: EdgeInsets.zero,
                  constraints: const BoxConstraints(),
                ),
              ],
            ),
            Divider(color: Colors.grey.withOpacity(0.2)),
            SizedBox(height: 12 * scale),
            Text(
              alert.message,
              style: TextStyle(
                fontSize: 15 * scale,
                color: AppColors.textGrey,
                height: 1.5,
              ),
            ),
            SizedBox(height: 20 * scale),
            Align(
              alignment: Alignment.centerRight,
              child: Text(
                _formatDate(alert.insertDate),
                style: TextStyle(
                  fontSize: 12 * scale,
                  color: AppColors.textLight,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ),
            SizedBox(height: 24 * scale),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: () => Navigator.pop(context),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primaryBlue,
                  foregroundColor: Colors.white,
                  padding: EdgeInsets.symmetric(vertical: 12 * scale),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8 * scale),
                  ),
                  elevation: 0,
                ),
                child: Text(
                  'Close',
                  style: TextStyle(
                    fontSize: 16 * scale,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
            ),
          ],
        ),
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
