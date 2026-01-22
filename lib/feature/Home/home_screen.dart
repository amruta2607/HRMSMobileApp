import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';
import '../../core/Utils/services/Time_Location/location_service.dart';
import 'widgets/home_header_section.dart';
import 'widgets/home_workspace_section.dart';
import 'widgets/home_up_next_section.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen>
    with WidgetsBindingObserver {

  bool _permissionChecked = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);

    WidgetsBinding.instance.addPostFrameCallback((_) {
      _checkLocationPermission();
    });
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  // 🔁 CALLED WHEN USER RETURNS FROM SETTINGS
  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      LocationService.clearCache();
      _permissionChecked = false;
      _checkLocationPermission();
    }
  }

  Future<void> _checkLocationPermission() async {
    if (_permissionChecked) return;
    _permissionChecked = true;

    // 1️⃣ Permission
    final permissionGranted =
    await LocationService.ensurePermissionGranted();

    if (!permissionGranted && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Location permission is required'),
        ),
      );
      return;
    }

    // 2️⃣ GPS SERVICE (re-checked properly)
    final serviceOn =
    await LocationService.isLocationServiceOn();

    if (!serviceOn && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Please turn on location services'),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;
    const designWidth = 402.0;
    final scale = (screenWidth / designWidth).clamp(0.9, 1.05);

    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const HomeHeaderSection(),

            Expanded(
              child: SingleChildScrollView(
                physics: const BouncingScrollPhysics(),
                padding: EdgeInsets.fromLTRB(
                  20 * scale,
                  8 * scale,
                  20 * scale,
                  24 * scale,
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    SizedBox(height: 12 * scale),
                    const HomeWorkspaceSection(),
                    SizedBox(height: 12 * scale),
                    const Text(
                      'UP NEXT',
                      style: TextStyle(
                        letterSpacing: 1.4,
                        fontSize: 13,
                        fontWeight: FontWeight.w600,
                        color: AppColors.textGrey,
                      ),
                    ),
                    SizedBox(height: 8 * scale),
                    const HomeUpNextSection(),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
