import 'dart:io';
import 'package:flutter/material.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:provider/provider.dart';
import 'package:overlay_support/overlay_support.dart';

import 'core/Theme/app_colors.dart';
import 'core/Utils/services/ navigation_service.dart';
import 'core/Utils/services/notification_service.dart';
import 'firebase_options.dart';
import 'feature/Profile/controller/profile_controller.dart';
import 'feature/Splash/splash_screen.dart';
import 'feature/Login/login_screen.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  await Firebase.initializeApp(
    options: DefaultFirebaseOptions.currentPlatform,
  );

  if (Platform.isAndroid) {
    await NotificationService.initialize();
  }

  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider<ProfileController>(
          create: (_) => ProfileController()..fetchProfileOnce(),
        ),
      ],
      child: const OverlaySupport.global(
        child: MyApp(),
      ),
    ),
  );
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'ATROZ HRM',
      navigatorKey: NavigationService.navigatorKey,
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        scaffoldBackgroundColor: AppColors.background,
      ),
      home: const SplashScreen(),
      routes: {
        '/login': (context) => const LoginScreen(),
      },
    );
  }
}
