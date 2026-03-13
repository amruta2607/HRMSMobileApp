import 'dart:io';
import 'package:flutter/material.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:overlay_support/overlay_support.dart';

// Top-level function for background handling
@pragma('vm:entry-point')
Future<void> _firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  debugPrint("Handling a background message: ${message.messageId}");
}

class NotificationService {
  static final FirebaseMessaging _messaging = FirebaseMessaging.instance;

  static Future<void> initialize() async {
    // Register background handler
    FirebaseMessaging.onBackgroundMessage(_firebaseMessagingBackgroundHandler);

    // Platform-specific logic: Only for Android for now to avoid APNS errors on iOS
    if (Platform.isAndroid) {
      // Request Notification Permission
      NotificationSettings settings = await _messaging.requestPermission(
        alert: true,
        badge: true,
        sound: true,
      );

      if (settings.authorizationStatus == AuthorizationStatus.authorized) {
        debugPrint('User granted permission');
      } else {
        debugPrint('User declined or has not accepted permission');
      }

      // Get FCM Token
      String? token = await _messaging.getToken();
      debugPrint("FCM TOKEN → $token");

      // Token Refresh Listener
      _messaging.onTokenRefresh.listen((token) {
        debugPrint("FCM Token refreshed: $token");
      });

      // Foreground Notification Listener
      FirebaseMessaging.onMessage.listen((RemoteMessage message) {
        debugPrint('Got a message whilst in the foreground!');
        debugPrint('Message data: ${message.data}');

        if (message.notification != null) {
          debugPrint('Message also contained a notification: ${message.notification}');
          showSimpleNotification(
            Text(message.notification?.title ?? "Notification"),
            subtitle: Text(message.notification?.body ?? ""),
            background: Colors.blue,
            duration: const Duration(seconds: 5),
          );
        }
      });
    } else {
      debugPrint('Firebase Messaging skipped for platform: ${Platform.operatingSystem}');
    }

    // Handle notification open when app is in background but opened
    FirebaseMessaging.onMessageOpenedApp.listen((RemoteMessage message) {
      debugPrint('A new onMessageOpenedApp event was published!');
      // Navigate to specific screen if needed
    });

    // Check if app was opened from a terminated state
    RemoteMessage? initialMessage = await _messaging.getInitialMessage();
    if (initialMessage != null) {
      debugPrint('App opened from terminated state: ${initialMessage.messageId}');
      // Navigate to specific screen if needed
    }
  }

  static Future<String?> getToken() async {
    if (Platform.isAndroid) {
      return await _messaging.getToken();
    }
    return null;
  }
}
