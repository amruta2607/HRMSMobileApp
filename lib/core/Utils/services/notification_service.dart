import 'package:flutter/material.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:overlay_support/overlay_support.dart';

class NotificationService {
  static final FirebaseMessaging _messaging = FirebaseMessaging.instance;

  static Future<void> initialize() async {
    // Request Notification Permission
    await _messaging.requestPermission(
      alert: true,
      sound: true,
      badge: true,
    );

    // Foreground Notification Listener
    FirebaseMessaging.onMessage.listen((RemoteMessage message) {
      showSimpleNotification(
        Text(message.notification?.title ?? "Notification"),
        subtitle: Text(message.notification?.body ?? ""),
        background: Colors.blue,
      );
    });
    String? token = await _messaging.getToken();
    print("FCM TOKEN → $token");
    // Token Refresh Listener
    _messaging.onTokenRefresh.listen((token) {
      debugPrint("FCM Token refreshed: $token");
    });
  }

  static Future<String?> getToken() async {
    return await _messaging.getToken();
  }
}
