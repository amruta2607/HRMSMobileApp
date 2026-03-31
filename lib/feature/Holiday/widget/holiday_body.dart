import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../core/Theme/app_colors.dart';
import '../../../core/Utils/services/holiday_service/holiday_service.dart';
import '../../Home/model/holiday.dart';
import '../../Reuse_Widgets/header_bg.dart';
import '../../../core/Utils/services/connectivity_service.dart';
class HolidayBody extends StatefulWidget {
  const HolidayBody({super.key});

  @override
  State<HolidayBody> createState() => _HolidayBodyState();
}

class _HolidayBodyState extends State<HolidayBody> {
  List<Holiday>? _holidays;
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _fetchHolidays();
    ConnectivityService.onReconnected(_handleReconnection);
  }

  void _handleReconnection() {
    if (mounted) {
      print('🌐 Connection restored, refreshing holidays...');
      _fetchHolidays();
    }
  }

  Future<void> _fetchHolidays() async {
    try {
      final data = await HolidayService.getHolidays();
      setState(() {
        _holidays = data;
        _isLoading = false;
        if (data == null) {
          _error = 'Failed to load holidays';
        }
      });
    } catch (e) {
      setState(() {
        _isLoading = false;
        _error = e.toString();
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

    return Column(
      children: [
        HeaderBackground(
          scale: scale,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Material(
                color: Colors.transparent,
                child: InkWell(
                  onTap: () => Navigator.pop(context),
                  borderRadius: BorderRadius.circular(8),
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      const Icon(Icons.arrow_back_ios, size: 18, color: AppColors.textDark),
                      const SizedBox(width: 4),
                      Text(
                        'Holidays',
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
                padding: EdgeInsets.only(left: 22 * scale),
                child: Text(
                  'List of company holidays',
                  style: TextStyle(
                    fontSize: 14 * scale,
                    fontWeight: FontWeight.w400,
                    color: AppColors.textGrey,
                  ),
                ),
              ),
            ],
          ),
        ),
        Expanded(
          child: _isLoading
              ? const Center(child: CircularProgressIndicator())
              : _error != null
              ? Center(child: Text(_error!))
              : _holidays == null || _holidays!.isEmpty
              ? const Center(child: Text('No holidays listed'))
              : ListView.separated(
            padding: EdgeInsets.all(20 * scale),
            itemCount: _holidays!.length,
            separatorBuilder: (_, __) => SizedBox(height: 12 * scale),
            itemBuilder: (context, index) {
              return _HolidayTile(holiday: _holidays![index], scale: scale);
            },
          ),
        ),
      ],
    );
  }
}

class _HolidayTile extends StatelessWidget {
  final Holiday holiday;
  final double scale;

  const _HolidayTile({required this.holiday, required this.scale});

  @override
  Widget build(BuildContext context) {
    final dateStr = DateFormat('MMM dd, EEEE').format(holiday.date);

    return Container(
      padding: EdgeInsets.symmetric(horizontal: 16 * scale, vertical: 18 * scale),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14 * scale),
        border: Border.all(color: const Color(0xFFE8ECF2), width: 1.5),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 8,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            width: 52 * scale,
            height: 52 * scale,
            decoration: BoxDecoration(
              color: AppColors.holidayBlue.withOpacity(0.1),
              borderRadius: BorderRadius.circular(12 * scale),
            ),
            child: const Center(
              child: Icon(Icons.calendar_today, color: AppColors.holidayBlue, size: 24),
            ),
          ),
          SizedBox(width: 16 * scale),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  holiday.name,
                  style: TextStyle(
                    fontSize: 15 * scale,
                    fontWeight: FontWeight.w700,
                    color: AppColors.textDark,
                  ),
                ),
                SizedBox(height: 3 * scale),
                Text(
                  dateStr,
                  style: TextStyle(
                    fontSize: 12.5 * scale,
                    fontWeight: FontWeight.w400,
                    color: AppColors.textGrey,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
