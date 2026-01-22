import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../core/Theme/app_colors.dart';
import '../../../core/Utils/services/Time_Location/live_time.dart';
import '../../../core/Utils/services/Time_Location/location_service.dart';
import 'dialog_button.dart';

class ClockOutDialog extends StatefulWidget {
  final ValueChanged<DateTime> onConfirm;

  const ClockOutDialog({
    super.key,
    required this.onConfirm,
  });

  @override
  State<ClockOutDialog> createState() => _ClockOutDialogState();
}

class _ClockOutDialogState extends State<ClockOutDialog> {
  late Future<String> _locationFuture;
  DateTime _currentTime = DateTime.now();

  @override
  void initState() {
    super.initState();
    _locationFuture = LocationService.getLocation();
  }

  @override
  Widget build(BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;

    return Dialog(
      insetPadding: const EdgeInsets.symmetric(horizontal: 24),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
      ),
      child: ConstrainedBox(
        constraints: BoxConstraints(
          maxWidth: screenWidth * 0.9,
        ),
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 18, 20, 16),
          child: IntrinsicHeight(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                /// HEADER
                const Column(
                  children: [
                    Icon(
                      Icons.access_time,
                      size: 28,
                      color: AppColors.popOrange,
                    ),
                    SizedBox(height: 10),
                    Text(
                      'Do you want to Clock-Out?',
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 14),

                /// TIME + LOCATION
                Column(
                  children: [
                    StreamBuilder<DateTime>(
                      stream: LiveTime.stream(),
                      builder: (context, snapshot) {
                        _currentTime = snapshot.data ?? DateTime.now();
                        return Text(
                          'Time : ${DateFormat('hh:mm a').format(_currentTime)}',
                          style: const TextStyle(
                            fontSize: 14,
                            color: AppColors.textGrey,
                          ),
                        );
                      },
                    ),
                    const SizedBox(height: 6),
                    FutureBuilder<String>(
                      future: _locationFuture,
                      builder: (_, s) => Text(
                        'Place : ${s.data ?? 'Fetching...'}',
                        textAlign: TextAlign.center,
                        style: const TextStyle(
                          fontSize: 14,
                          color: AppColors.textGrey,
                        ),
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 18),

                /// ACTION BUTTONS
                Row(
                  children: [
                    Expanded(
                      child: DialogButton(
                        text: 'Cancel',
                        borderColor: AppColors.popOrange,
                        textColor: AppColors.popOrange,
                        onTap: () => Navigator.pop(context),
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: DialogButton(
                        text: 'Confirm',
                        filled: true,
                        filledColor: AppColors.popOrange,
                        textColor: Colors.white,
                        onTap: () {
                          Navigator.pop(context);
                          widget.onConfirm(_currentTime);
                        },
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
