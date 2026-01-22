import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../../core/Theme/app_colors.dart';
import '../../../../core/Utils/services/Time_Location/location_service.dart';
import '../../../../feature/Reuse_Widgets/action_card.dart';
import '../../../../feature/Reuse_Widgets/header_bg.dart';
import 'clock_action_button.dart';

class AttendanceHeader extends StatefulWidget {
  final String title;
  final VoidCallback onBack;
  final VoidCallback onClockTap;
  final bool isClockedIn;
  final Duration workedDuration;

  const AttendanceHeader({
    super.key,
    required this.title,
    required this.onBack,
    required this.onClockTap,
    required this.isClockedIn,
    required this.workedDuration,
  });

  @override
  State<AttendanceHeader> createState() => _AttendanceHeaderState();
}

class _AttendanceHeaderState extends State<AttendanceHeader>
    with WidgetsBindingObserver {

  late Future<String> _locationFuture;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    _refreshLocation();
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      _refreshLocation();
    }
  }

  void _refreshLocation() {
    LocationService.clearCache();
    setState(() {
      _locationFuture = LocationService.getLocation(forceRefresh: true);
    });
  }

  String _format(Duration d) {
    final m = d.inMinutes.remainder(60).toString().padLeft(2, '0');
    final s = d.inSeconds.remainder(60).toString().padLeft(2, '0');
    return '$m:$s';
  }

  @override
  Widget build(BuildContext context) {
    final scale =
    (MediaQuery.of(context).size.width / 402).clamp(0.85, 1.1);

    return HeaderBackground(
      scale: scale,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              InkWell(
                onTap: widget.onBack,
                child: Icon(
                  Icons.arrow_back_ios,
                  size: 20 * scale,
                  color: AppColors.textDark,
                ),
              ),
              const SizedBox(width: 8),
              Text(
                DateFormat('MMM dd, EEEE').format(DateTime.now()),
                style: TextStyle(
                  fontSize: 14 * scale,
                  color: AppColors.textGrey,
                ),
              ),
            ],
          ),

          const SizedBox(height: 6),

          Text(
            widget.title,
            style: TextStyle(
              fontSize: 27 * scale,
              fontWeight: FontWeight.w700,
              color: AppColors.textDark,
            ),
          ),

          const SizedBox(height: 18),

          ActionCard(
            left: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                FutureBuilder<String>(
                  future: _locationFuture,
                  builder: (_, snapshot) {
                    final text = snapshot.connectionState ==
                        ConnectionState.waiting
                        ? 'Fetching...'
                        : snapshot.data ?? 'Location unavailable';

                    return Text(
                      'Location: $text',
                      style: TextStyle(
                        fontSize: 14 * scale,
                        color: AppColors.textGrey,
                      ),
                    );
                  },
                ),


                Text(
                  _format(widget.workedDuration),
                  style: TextStyle(
                    fontSize: 20 * scale,
                    fontWeight: FontWeight.w500,
                    color: AppColors.presentGreen,
                  ),
                ),
              ],
            ),
            right: ClockActionButton(
              isClockedIn: widget.isClockedIn,
              onTap: widget.onClockTap,
            ),
          ),
        ],
      ),
    );
  }
}
