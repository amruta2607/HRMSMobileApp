import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:flutter_widget_from_html/flutter_widget_from_html.dart';
import '../../../core/Theme/app_colors.dart';
import '../../../core/Utils/services/announcement_service/announcement_service.dart';
import '../model/announcement_model.dart';

class AnnouncementBody extends StatefulWidget {
  final double scale;
  const AnnouncementBody({super.key, required this.scale});

  @override
  State<AnnouncementBody> createState() => _AnnouncementBodyState();
}

class _AnnouncementBodyState extends State<AnnouncementBody> {
  List<AnnouncementModel>? _announcements;
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadAnnouncements();
  }

  Future<void> _loadAnnouncements() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final results = await AnnouncementService.getAnnouncements();
      if (mounted) {
        setState(() {
          _announcements = results;
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

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_error != null) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.error_outline_rounded, size: 64, color: Colors.red.withOpacity(0.5)),
            const SizedBox(height: 16),
            Text(
              'Oops! Something went wrong',
              style: TextStyle(fontSize: 16 * widget.scale, color: AppColors.textGrey, fontWeight: FontWeight.w500),
            ),
            TextButton(
              onPressed: _loadAnnouncements,
              child: const Text('Retry'),
            ),
          ],
        ),
      );
    }

    if (_announcements == null || _announcements!.isEmpty) {
      return RefreshIndicator(
        onRefresh: _loadAnnouncements,
        child: ListView(
          children: [
            SizedBox(
              height: MediaQuery.of(context).size.height * 0.6,
              child: Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.notifications_none_rounded, size: 64, color: AppColors.textGrey.withOpacity(0.5)),
                    const SizedBox(height: 16),
                    Text(
                      'No announcements yet',
                      style: TextStyle(fontSize: 16 * widget.scale, color: AppColors.textGrey, fontWeight: FontWeight.w500),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _loadAnnouncements,
      child: ListView.separated(
        padding: EdgeInsets.fromLTRB(20 * widget.scale, 10 * widget.scale, 20 * widget.scale, 100 * widget.scale),
        itemCount: _announcements!.length,
        separatorBuilder: (_, __) => SizedBox(height: 16 * widget.scale),
        itemBuilder: (context, index) {
          final item = _announcements![index];
          return _AnnouncementTile(item: item, scale: widget.scale);
        },
      ),
    );
  }
}

class _AnnouncementTile extends StatelessWidget {
  final AnnouncementModel item;
  final double scale;

  const _AnnouncementTile({required this.item, required this.scale});

  @override
  Widget build(BuildContext context) {
    // Formatting date to MMM dd, yyyy
    final dateStr = DateFormat('MMM dd, yyyy').format(item.date);

    return Container(
      padding: EdgeInsets.all(16 * scale),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18 * scale),
        border: Border.all(color: const Color(0xFFF1F5F9), width: 1.5),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 40 * scale,
                height: 40 * scale,
                decoration: const BoxDecoration(
                  color: Color(0xFFE3F2FD),
                  shape: BoxShape.circle,
                ),
                child: Icon(Icons.notifications_active_rounded, size: 20 * scale, color: const Color(0xFF1565C0)),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      item.name,
                      style: TextStyle(
                        fontSize: 16 * scale,
                        fontWeight: FontWeight.w700,
                        color: AppColors.textDark,
                      ),
                    ),
                    Text(
                      dateStr,
                      style: TextStyle(
                        fontSize: 12 * scale,
                        fontWeight: FontWeight.w400,
                        color: AppColors.textGrey,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          HtmlWidget(
            item.message,
            textStyle: TextStyle(
              fontSize: 13 * scale,
              fontWeight: FontWeight.w400,
              color: const Color(0xFF475569),
              height: 1.5,
            ),
          ),
        ],
      ),
    );
  }
}
