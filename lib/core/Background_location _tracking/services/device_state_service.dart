import 'dart:io';
import 'package:flutter/services.dart';

/// Native device-state helpers (airplane mode, etc.).
class DeviceStateService {
  static const MethodChannel _channel = MethodChannel('device_state');

  /// Android: reads Settings.Global.AIRPLANE_MODE_ON.
  /// iOS: not reliably available — returns false.
  static Future<bool> isAirplaneModeOn() async {
    if (!Platform.isAndroid) return false;
    try {
      final result = await _channel.invokeMethod<bool>('isAirplaneModeOn');
      return result == true;
    } catch (_) {
      return false;
    }
  }
}
