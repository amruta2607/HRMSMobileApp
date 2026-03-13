import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../core/Theme/app_colors.dart';
import '../../../core/Utils/services/holiday_service/holiday_service.dart';
import '../model/holiday.dart';

class HomeUpNextSection extends StatefulWidget {
  const HomeUpNextSection({super.key});

  @override
  State<HomeUpNextSection> createState() => _HomeUpNextSectionState();
}

class _HomeUpNextSectionState extends State<HomeUpNextSection> {
  Holiday? upcomingHoliday;
  bool isLoading = true;
  String errorMessage = '';

  @override
  void initState() {
    super.initState();
    _fetchUpcomingHoliday();
  }

  Future<void> _fetchUpcomingHoliday() async {
    try {
      final List<Holiday>? holidays = await HolidayService.getHolidays();
      if (mounted) {
        setState(() {
          isLoading = false;
          if (holidays != null && holidays.isNotEmpty) {
            final DateTime now = DateTime.now();
            final DateTime today = DateTime(now.year, now.month, now.day);

            // Filter active holidays from today onwards, sort by date
            final List<Holiday> upcoming = holidays
                .where((h) => h.isActive && !h.date.isBefore(today))
                .toList();

            if (upcoming.isNotEmpty) {
              upcoming.sort((a, b) => a.date.compareTo(b.date));
              upcomingHoliday = upcoming.first;
            } else {
              errorMessage = 'No upcoming holidays';
            }
          } else {
            errorMessage = 'No upcoming holidays';
          }
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          isLoading = false;
          errorMessage = 'Error loading holiday';
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    // Design reference width
    const designWidth = 402.0;
    final screenWidth = MediaQuery.of(context).size.width;

    // Scale factor
    final scale = (screenWidth / designWidth).clamp(0.85, 1.1);

    return Container(
      width: double.infinity,
      padding: EdgeInsets.all(16 * scale),
      decoration: BoxDecoration(
        color: AppColors.upNextCardBg,
        borderRadius: BorderRadius.circular(20 * scale),
      ),
      child: isLoading
          ? Center(
        child: SizedBox(
          width: 24 * scale,
          height: 24 * scale,
          child: const CircularProgressIndicator(strokeWidth: 2),
        ),
      )
          : upcomingHoliday == null
          ? Text(
        errorMessage,
        style: TextStyle(
          fontSize: 14 * scale,
          color: AppColors.textGrey,
        ),
      )
          : Row(
        children: [
          /// DATE BOX
          Container(
            padding: EdgeInsets.symmetric(
              vertical: 12 * scale,
              horizontal: 16 * scale,
            ),
            decoration: BoxDecoration(
              color: AppColors.upNextDateBg,
              borderRadius: BorderRadius.circular(14 * scale),
            ),
            child: Column(
              children: [
                Text(
                  DateFormat('MMM')
                      .format(upcomingHoliday!.date)
                      .toUpperCase(),
                  style: TextStyle(
                    fontSize: 12 * scale,
                    fontWeight: FontWeight.w600,
                    color: AppColors.textDark,
                  ),
                ),
                SizedBox(height: 0 * scale),
                Text(
                  DateFormat('dd').format(upcomingHoliday!.date),
                  style: TextStyle(
                    fontSize: 20 * scale,
                    fontWeight: FontWeight.bold,
                    color: AppColors.textDark,
                  ),
                ),
              ],
            ),
          ),

          SizedBox(width: 18 * scale),

          /// DETAILS
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  upcomingHoliday!.name,
                  style: TextStyle(
                    fontSize: 16 * scale,
                    fontWeight: FontWeight.w700,
                    color: AppColors.textDark,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
                if (upcomingHoliday!.description != null &&
                    upcomingHoliday!.description!.isNotEmpty) ...[
                  SizedBox(height: 6 * scale),
                  Text(
                    upcomingHoliday!.description!,
                    style: TextStyle(
                      fontSize: 14 * scale,
                      color: AppColors.textGrey,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }
}
