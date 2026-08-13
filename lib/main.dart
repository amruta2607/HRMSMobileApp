import 'package:flutter/material.dart';
// import 'package:firebase_core/firebase_core.dart';
import 'package:provider/provider.dart';
import 'package:overlay_support/overlay_support.dart';
import 'package:flutter_background_geolocation/flutter_background_geolocation.dart' as bg;

import 'core/Theme/app_colors.dart';
import 'core/Utils/services/ navigation_service.dart';
// import 'firebase_options.dart';
import 'feature/Profile/controller/profile_controller.dart';
import 'feature/Splash/splash_screen.dart';
import 'feature/Login/login_screen.dart';
import 'core/Utils/services/Attendance service/attendance_service.dart';
import 'core/Utils/services/connectivity_service.dart';
import 'feature/Tenant/controller/tenant_controller.dart';
import 'core/Background_location _tracking/providers/location_provider.dart';
import 'core/Background_location _tracking/services/location_service.dart';
import 'core/Background_location _tracking/services/gps_monitor_service.dart';
import 'core/Background_location _tracking/services/location_gap_detector.dart';
import 'core/Background_location _tracking/services/location_config_service.dart';

// Register headless task handler
@pragma('vm:entry-point')
void headlessTaskCallback(bg.HeadlessEvent headlessEvent) async {
  await LocationService.headlessTask(headlessEvent);
}

Future<void> main() async {
  print('[BOOT] 1/6 main() entered');
  WidgetsFlutterBinding.ensureInitialized();
  print('[BOOT] 2/6 WidgetsFlutterBinding initialised');

  // Register the headless task BEFORE any configuration
  try {
    bg.BackgroundGeolocation.registerHeadlessTask(headlessTaskCallback);
    print('[BOOT] 3/6 HeadlessTask registered');
  } catch (e) {
    print('[BOOT] 3/6 HeadlessTask registration FAILED: $e');
  }

  ConnectivityService.initialize();
  print('[BOOT] 4/6 ConnectivityService initialised');

  // Register global "Data Restore" / Sync tasks
  ConnectivityService.onReconnected(() {
    print(' GLOBAL SYNC: Connection restored, syncing pending data...');
    AttendanceService.syncPendingPunches();
  });

  print('[BOOT] 5/6 calling runApp()');
  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider<ProfileController>(
          create: (_) {
            final c = ProfileController();
            // Defer network so first frame / splash aren't blocked.
            Future.delayed(const Duration(seconds: 2), () {
              c.fetchProfileOnce();
            });
            return c;
          },
        ),
        ChangeNotifierProvider<TenantController>(
          create: (_) {
            final c = TenantController();
            Future.delayed(const Duration(seconds: 2), () {
              c.fetchCompanyLogo();
            });
            return c;
          },
        ),
        ChangeNotifierProvider<LocationProvider>(
          create: (_) => LocationProvider(),
        ),
      ],
      child: const OverlaySupport.global(
        child: MyApp(),
      ),
    ),
  );

  // Defer heavy BG init until after first paint so splash opens quickly.
  WidgetsBinding.instance.addPostFrameCallback((_) {
    print('[BOOT] 6/6 deferred BG init scheduled');
    Future.delayed(const Duration(milliseconds: 1500), () async {
      await _initializeBackgroundServices();
      AttendanceService.syncPendingPunches();
    });
  });
}

Future<void> _initializeBackgroundServices() async {
  try {
    // Config first (cached prefs / short HTTP), then heavy TrackingService.
    await LocationConfigService.initialize();
    LocationConfigService.setOnConfigFetchedCallback(() {
      LocationService.instance.onConfigUpdated();
    });

    await LocationService.instance.initialize();
    await GpsMonitorService.instance.initialize();
    await LocationGapDetector.instance.initialize();

    print('🚀 Background location services initialized (deferred).');
  } catch (e) {
    print('Error initializing background tracking services: $e');
  }
}

class MyApp extends StatefulWidget {
  const MyApp({super.key});

  @override
  State<MyApp> createState() => _MyAppState();
}

class _MyAppState extends State<MyApp> with WidgetsBindingObserver {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    LocationService.instance.isInForeground = true;
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    super.didChangeAppLifecycleState(state);
    LocationService.instance.isInForeground = state == AppLifecycleState.resumed;
  }

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
