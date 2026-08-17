import 'dart:async';
import 'package:altroz/core/Utils/services/event_service/event_service.dart';
import 'package:altroz/core/Utils/services/holiday_service/holiday_service.dart';
import 'package:altroz/feature/Home/model/award.dart';
import 'package:altroz/feature/Home/model/birthday.dart';
import 'package:altroz/feature/Home/model/event.dart';
import 'package:altroz/feature/Home/model/work_anniversary.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../core/Theme/app_colors.dart';
import '../../../core/Utils/services/announcement_service/announcement_service.dart';
import '../../../core/Utils/services/award_service/award_service.dart';
import '../../../core/Utils/services/birthday_service/birthday_service.dart';
import '../../../core/Utils/services/work_anniversary_service/work_anniversary_service.dart';
import '../../Announcement/model/announcement_model.dart';
import '../model/holiday.dart';

class HomeUpNextSection extends StatefulWidget {
  const HomeUpNextSection({super.key});

  @override
  State<HomeUpNextSection> createState() => _HomeUpNextSectionState();
}

class _HomeUpNextSectionState extends State<HomeUpNextSection> {
  Holiday? upcomingHoliday;
  AnnouncementModel? upcomingAnnouncement;
  List<Event> upcomingEvents = [];
  List<Birthday> upcomingBirthdays = [];
  List<WorkAnniversary> upcomingAnniversaries = [];
  List<Award> upcomingAwards = [];

  bool isLoading = true;

  // Controllers for sliding infinite loops
  final PageController _eventsPageController = PageController(initialPage: 1000);
  final PageController _birthdaysPageController = PageController(initialPage: 1000);
  final PageController _anniversariesPageController = PageController(initialPage: 1000);
  final PageController _awardsPageController = PageController(initialPage: 1000);

  Timer? _autoSlideTimer;
  int _currentEventPage = 1000;
  int _currentBirthdayPage = 1000;
  int _currentAnniversaryPage = 1000;
  int _currentAwardsPage = 1000;

  static const int maxEventsToShow = 3;

  @override
  void initState() {
    super.initState();
    _fetchData();
  }

  Future<void> _fetchData() async {
    await Future.wait([
      _fetchUpcomingHoliday(),
      _fetchUpcomingEvents(),
      _fetchUpcomingAnnouncement(),
      _fetchUpcomingBirthdays(),
      _fetchUpcomingAnniversaries(),
      _fetchUpcomingAwards(),
    ]);

    // Start sliding the events after data loads
    _startAutoSlide();
  }

  // ============================================================
  // UPCOMING ANNOUNCEMENT API
  // ============================================================
  Future<void> _fetchUpcomingAnnouncement() async {
    try {
      final List<AnnouncementModel>? announcements = await AnnouncementService.getAnnouncements();

      if (!mounted) return;

      if (announcements != null && announcements.isNotEmpty) {
        setState(() {
          upcomingAnnouncement = announcements.first;
        });
      }
    } catch (e) {
      debugPrint('ANNOUNCEMENT API ERROR: $e');
    }
  }

  // ============================================================
  // UPCOMING EVENT API
  // ============================================================
  Future<void> _fetchUpcomingEvents() async {
    try {
      final List<Event>? events = await EventService.getUpcomingEvents();

      if (!mounted) return;
      final DateTime now = DateTime.now();

      if (events != null && events.isNotEmpty) {
        final List<Event> upcoming = events
            .where((event) => !event.endDate.isBefore(now))
            .toList();

        upcoming.sort((a, b) => a.startDate.compareTo(b.startDate));

        setState(() {
          upcomingEvents = upcoming.take(maxEventsToShow).toList();
        });
      }
    } catch (e) {
      debugPrint('EVENT API ERROR: $e');
    } finally {
      if (mounted) _checkLoadingComplete();
    }
  }

  // ============================================================
  // UPCOMING HOLIDAY API
  // ============================================================
  Future<void> _fetchUpcomingHoliday() async {
    try {
      final List<Holiday>? holidays = await HolidayService.getUpcomingHolidays();

      if (!mounted) return;
      final DateTime today = DateTime(DateTime.now().year, DateTime.now().month, DateTime.now().day);

      if (holidays != null && holidays.isNotEmpty) {
        final List<Holiday> upcoming = holidays
            .where((holiday) => holiday.isActive && !holiday.date.isBefore(today))
            .toList();

        upcoming.sort((a, b) => a.date.compareTo(b.date));

        setState(() {
          if (upcoming.isNotEmpty) {
            upcomingHoliday = upcoming.first;
          }
        });
      }
    } catch (e) {
      debugPrint('HOLIDAY API ERROR: $e');
    }
  }

  // ==============================================================
  // UPCOMING BIRTHDAY API
  // ===============================================================
  Future<void> _fetchUpcomingBirthdays() async {
    try {
      final List<Birthday>? birthdays = await BirthdayService.getUpcomingBirthdays();

      if (!mounted) return;

      if (birthdays != null && birthdays.isNotEmpty) {
        final DateTime today = DateTime.now();
        final List<Birthday> upcoming = birthdays
            .where((b) => b.birthdayDate.month == today.month && b.birthdayDate.year == today.year)
            .toList();

        upcoming.sort((a, b) => a.birthdayDate.compareTo(b.birthdayDate));

        setState(() => upcomingBirthdays = upcoming);
      }
    } catch (e) {
      debugPrint('BIRTHDAY API ERROR: $e');
    } finally {
      if (mounted) _checkLoadingComplete();
    }
  }

  // =================================================================
  // UPCOMING WORK ANNIVERSARIES API
  // =====================================================================
  Future<void> _fetchUpcomingAnniversaries() async{
    try {
      final List<WorkAnniversary>? anniversaries = await WorkAnniversaryService.getUpcomingAnniversaries();
      if (!mounted) return;

      if (anniversaries != null && anniversaries.isNotEmpty){
        final DateTime today = DateTime.now();
        final List<WorkAnniversary> upcoming = anniversaries
            .where((a) => a.anniversaryDate.month == today.month && a.anniversaryDate.year == today.year)
            .toList();

        upcoming.sort((a, b) => a.anniversaryDate.compareTo(b.anniversaryDate));

        setState(() => upcomingAnniversaries = upcoming );
      }
    } catch (e) {
      debugPrint('ANNIVERSARY ERROR: $e');
    } finally {
      if (mounted) _checkLoadingComplete();
    }
  }

  // =================================================================
  // UPCOMING AWARDS API
  // =====================================================================
  Future<void> _fetchUpcomingAwards() async {
    try {
      final List<Award>? awards = await AwardService.getUpcomingAwards();
      if (!mounted) return;

      if (awards != null && awards.isNotEmpty) {
        final DateTime today = DateTime(DateTime.now().year, DateTime.now().month, DateTime.now().day);
        final List<Award> upcoming = awards.where((a) => !a.date.isBefore(today)).toList();

        upcoming.sort((a, b) => a.date.compareTo(b.date));

        setState(() => upcomingAwards = upcoming);
      }
    } catch (e) {
      debugPrint('AWARD ERROR: $e');
    } finally {
      if (mounted) _checkLoadingComplete();
    }
  }

  void _checkLoadingComplete(){
    setState(() {
      isLoading = false;
    });
  }

  // ============================================================
  // INFINITE AUTO SLIDER TIMER (3 Seconds)
  // ============================================================
  void _startAutoSlide() {
    _autoSlideTimer?.cancel();

    _autoSlideTimer = Timer.periodic(const Duration(seconds: 3), (timer) {

      // Helper function to handle the sliding logic cleanly
      void slide(PageController controller, int currentPage, Function(int) updatePage) {
        if (controller.hasClients) {
          updatePage(currentPage + 1);
          controller.animateToPage(
            currentPage + 1,
            duration: const Duration(milliseconds: 600),
            curve: Curves.easeInOut,
          );
        }
      }

      // Slide each section if it has more than 1 item
      if (upcomingEvents.length > 1) slide(_eventsPageController, _currentEventPage, (p) => _currentEventPage = p);
      if (upcomingBirthdays.length > 1) slide(_birthdaysPageController, _currentBirthdayPage, (p) => _currentBirthdayPage = p);
      if (upcomingAnniversaries.length > 1) slide(_anniversariesPageController, _currentAnniversaryPage, (p) => _currentAnniversaryPage = p);
      if (upcomingAwards.length > 1) slide(_awardsPageController, _currentAwardsPage, (p) => _currentAwardsPage = p);
    });
  }

  @override
  void dispose() {
    _autoSlideTimer?.cancel();
    _eventsPageController.dispose();
    _birthdaysPageController.dispose();
    _anniversariesPageController.dispose();
    _awardsPageController.dispose();
    super.dispose();
  }

  // ============================================================
  // DATE BOX
  // ============================================================
  Widget _dateBox({required DateTime date, required double scale}) {
    return Container(
      padding: EdgeInsets.symmetric(
        vertical: 14 * scale,
        horizontal: 16 * scale,
      ),
      decoration: BoxDecoration(
        color: AppColors.upNextDateBg,
        borderRadius: BorderRadius.circular(18 * scale),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            DateFormat('MMM').format(date).toUpperCase(),
            style: TextStyle(
              fontSize: 12 * scale,
              fontWeight: FontWeight.w600,
              color: AppColors.textDark,
            ),
          ),
          Text(
            DateFormat('dd').format(date),
            style: TextStyle(
              fontSize: 20 * scale,
              fontWeight: FontWeight.bold,
              color: AppColors.textDark,
            ),
          ),
        ],
      ),
    );
  }

  // ============================================================
  // UNIFIED FULL-WIDTH CARD
  // ============================================================
  Widget _buildUnifiedCard({
    required DateTime date,
    required String type,
    required String title,
    String? description,
    Color? descriptionColor,
    String? timeInfo,
    required double scale,
    bool hasBottomMargin = false,
  }) {
    return Container(
      width: double.infinity,
      padding: EdgeInsets.all(14 * scale),
      margin: EdgeInsets.only(bottom: hasBottomMargin ? 12 * scale : 0),
      decoration: BoxDecoration(
        color: AppColors.upNextCardBg,
        borderRadius: BorderRadius.circular(20 * scale),
      ),
      child: Row(
        children: [
          _dateBox(date: date, scale: scale),
          SizedBox(width: 12 * scale),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Row(
                  children: [
                    Text(
                      type,
                      style: TextStyle(
                        fontSize: 11 * scale,
                        fontWeight: FontWeight.w600,
                        color: AppColors.textGrey,
                      ),
                    ),
                    if (timeInfo != null && timeInfo.isNotEmpty)
                      Text(
                        ' • $timeInfo',
                        style: TextStyle(
                          fontSize: 11 * scale,
                          fontWeight: FontWeight.w600,
                          color: AppColors.textGrey,
                        ),
                      ),
                  ],
                ),
                SizedBox(height: 3 * scale),
                Text(
                  title,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    fontSize: 15 * scale,
                    fontWeight: FontWeight.w700,
                    color: AppColors.textDark,
                  ),
                ),
                if (description != null && description.isNotEmpty) ...[
                  SizedBox(height: 4 * scale),
                  Text(
                    description,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: TextStyle(
                      fontSize: 12 * scale,
                      color: descriptionColor ?? AppColors.textGrey,
                      fontWeight: descriptionColor != null ? FontWeight.w600 : FontWeight.normal,
                    ),
                  ),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }

  // ============================================================
  // BUILD METHOD
  // ============================================================
  @override
  Widget build(BuildContext context) {
    const designWidth = 402.0;
    final screenWidth = MediaQuery.of(context).size.width;
    final scale = (screenWidth / designWidth).clamp(0.85, 1.1);

    if (isLoading) {
      return Container(
        width: double.infinity,
        padding: EdgeInsets.all(16 * scale),
        decoration: BoxDecoration(
          color: AppColors.upNextCardBg,
          borderRadius: BorderRadius.circular(20 * scale),
        ),
        child: Center(
          child: SizedBox(
            width: 24 * scale,
            height: 24 * scale,
            child: const CircularProgressIndicator(strokeWidth: 2),
          ),
        ),
      );
    }

    if (upcomingHoliday == null && upcomingEvents.isEmpty && upcomingAnnouncement == null &&
        upcomingBirthdays.isEmpty && upcomingAnniversaries.isEmpty && upcomingAwards.isEmpty) {
      return Container(
        width: double.infinity,
        padding: EdgeInsets.all(16 * scale),
        decoration: BoxDecoration(
          color: AppColors.upNextCardBg,
          borderRadius: BorderRadius.circular(20 * scale),
        ),
        child: Text(
          'No upcoming events or holidays',
          style: TextStyle(
            fontSize: 14 * scale,
            color: AppColors.textGrey,
          ),
        ),
      );
    }

    // Increased heights to prevent clipping issues with padding
    final double sliderContainerHeight = 135 * scale;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // 1. ANNOUNCEMENT (Static, Fixed on Top)
        if (upcomingAnnouncement != null)
          _buildUnifiedCard(
            date: upcomingAnnouncement!.date,
            type: 'Announcement',
            title: upcomingAnnouncement!.name,
            description: upcomingAnnouncement!.message,
            scale: scale,
            hasBottomMargin: upcomingHoliday != null || upcomingEvents.isNotEmpty || upcomingBirthdays.isNotEmpty || upcomingAnniversaries.isNotEmpty || upcomingAwards.isNotEmpty,
          ),

        // 2. HOLIDAY (Static, Fixed below Announcement)
        if (upcomingHoliday != null)
          _buildUnifiedCard(
            date: upcomingHoliday!.date,
            type: 'Holiday',
            title: upcomingHoliday!.name,
            description: upcomingHoliday!.description,
            scale: scale,
            hasBottomMargin: upcomingEvents.isNotEmpty || upcomingBirthdays.isNotEmpty || upcomingAnniversaries.isNotEmpty || upcomingAwards.isNotEmpty,
          ),

        // 3. EVENTS (Dynamic Infinite 3-Second Slider)
        if (upcomingEvents.isNotEmpty)
          Container(
            height: sliderContainerHeight,
            margin: EdgeInsets.only(bottom: (upcomingBirthdays.isNotEmpty || upcomingAnniversaries.isNotEmpty || upcomingAwards.isNotEmpty) ? 12 * scale : 0),
            child: PageView.builder(
              controller: _eventsPageController,
              physics: const BouncingScrollPhysics(),
              onPageChanged: (index) {
                _currentEventPage = index;
              },
              itemBuilder: (context, index) {
                final actualIndex = index % upcomingEvents.length;
                final event = upcomingEvents[actualIndex];

                final String formattedTime = DateFormat('h:mm a').format(event.startDate);

                return _buildUnifiedCard(
                  date: event.startDate,
                  type: 'Event',
                  title: event.name,
                  description: event.description,
                  timeInfo: formattedTime,
                  scale: scale,
                  hasBottomMargin: false,
                );
              },
            ),
          ),

        // 4. BIRTHDAYS (Dynamic Infinite 3-Second Slider)
        if (upcomingBirthdays.isNotEmpty)
          Container(
            height: sliderContainerHeight,
            margin: EdgeInsets.only(bottom: (upcomingAnniversaries.isNotEmpty || upcomingAwards.isNotEmpty) ? 12 * scale : 0),
            child: PageView.builder(
              controller: _birthdaysPageController,
              physics: const BouncingScrollPhysics(),
              onPageChanged: (index) => _currentBirthdayPage = index,
              itemBuilder: (context, index) {
                final actualIndex = index % upcomingBirthdays.length;
                final birthday = upcomingBirthdays[actualIndex];

                final DateTime today = DateTime(DateTime.now().year, DateTime.now().month, DateTime.now().day);
                final bool isToday = birthday.birthdayDate.year == today.year &&
                    birthday.birthdayDate.month == today.month &&
                    birthday.birthdayDate.day == today.day;

                return _buildUnifiedCard(
                  date: birthday.birthdayDate,
                  type: 'Birthday 🎂',
                  title: birthday.employeeName,
                  description:  isToday ? 'Wish ${birthday.employeeName} a very Happy Birthday! 🎉' : 'Upcoming Birthday',
                  descriptionColor: isToday ? Colors.indigo : null,
                  scale: scale,
                  hasBottomMargin: false,
                );
              },
            ),
          ),

        // 5. WORK ANNIVERSARIES (Dynamic Infinite 3-Second Slider)
        if (upcomingAnniversaries.isNotEmpty)
          Container(
            height: sliderContainerHeight,
            margin: EdgeInsets.only(bottom: upcomingAwards.isNotEmpty ? 12 * scale : 0),
            child: PageView.builder(
              controller: _anniversariesPageController,
              physics: const BouncingScrollPhysics(),
              onPageChanged: (index) => _currentAnniversaryPage = index,
              itemBuilder: (context, index) {
                final actualIndex = index % upcomingAnniversaries.length;
                final anniversary = upcomingAnniversaries[actualIndex];

                final DateTime today = DateTime(DateTime.now().year, DateTime.now().month, DateTime.now().day);
                final bool isToday = anniversary.anniversaryDate.year == today.year &&
                    anniversary.anniversaryDate.month == today.month &&
                    anniversary.anniversaryDate.day == today.day;

                return _buildUnifiedCard(
                  date: anniversary.anniversaryDate,
                  type: 'Work Anniversary 🎊',
                  title: anniversary.employeeName,
                  description: isToday
                      ? 'Happy ${anniversary.yearsCompleted} Year Anniversary! 🎉'
                      : '${anniversary.yearsCompleted} Years Completed',
                  descriptionColor: isToday ? Colors.teal : null,
                  scale: scale,
                  hasBottomMargin: false,
                );
              },
            ),
          ),

        // 6. AWARDS (Dynamic Infinite 3-Second Slider)
        if (upcomingAwards.isNotEmpty)
          SizedBox(
            height: sliderContainerHeight,
            child: PageView.builder(
              controller: _awardsPageController,
              physics: const BouncingScrollPhysics(),
              onPageChanged: (index) => _currentAwardsPage = index,
              itemBuilder: (context, index) {
                final actualIndex = index % upcomingAwards.length;
                final award = upcomingAwards[actualIndex];

                return _buildUnifiedCard(
                  date: award.date,
                  type: 'Award 🏆',
                  title: award.awardeeName,
                  description: award.awardName,
                  descriptionColor: Colors.deepPurple,
                  scale: scale,
                  hasBottomMargin: false,
                );
              },
            ),
          ),
      ],
    );
  }
}