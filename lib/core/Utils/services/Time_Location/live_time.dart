import 'dart:async';

class LiveTime {
  static Stream<DateTime> stream() {
    return Stream.periodic(
      const Duration(seconds: 1),
          (_) => DateTime.now(),
    );
  }

  static String formatTime(DateTime now) {
    return '${now.hour.toString().padLeft(2, '0')}:'
        '${now.minute.toString().padLeft(2, '0')} Hours';
  }

  static String formatDate(DateTime now) {
    return '${now.day.toString().padLeft(2, '0')} '
        '${_month[now.month - 1]} '
        '${now.year}';
  }

  static const _month = [
    'Jan','Feb','Mar','Apr','May','Jun',
    'Jul','Aug','Sep','Oct','Nov','Dec'
  ];
}
