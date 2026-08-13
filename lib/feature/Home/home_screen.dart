import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/Theme/app_colors.dart';
import '../../core/Utils/services/app_permission_service.dart';
import '../../core/Utils/services/token_storage.dart';
import 'Widgets/home_up_next_section.dart';
import 'widgets/home_header_section.dart';
import 'widgets/home_workspace_section.dart';
import 'home_controller/home_controller.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen>
    with WidgetsBindingObserver {

  final HomeController _homeController = HomeController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);

    _homeController.fetchHomeData();

    WidgetsBinding.instance.addPostFrameCallback((_) {
      _requestPermissionsOnOpen();
    });
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _homeController.dispose();
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.resumed) {
      // Soft session check only — full home reload is throttled inside controller.
      TokenStorage.ensureSession().then((_) {
        if (mounted) _homeController.fetchHomeData();
      });
      // If user turned off any tracking permission in Settings, show popup again.
      if (mounted) {
        AppPermissionService.ensureTrackingPermissionsWithPopup(context);
      }
    }
  }

  Future<void> _requestPermissionsOnOpen() async {
    if (!mounted) return;
    await AppPermissionService.requestAllRequired(context);
  }

  @override
  Widget build(BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;
    const designWidth = 402.0;
    final scale = (screenWidth / designWidth).clamp(0.9, 1.05);

    return ChangeNotifierProvider.value(
      value: _homeController,
      child: Scaffold(
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
                      SizedBox(height: 13 * scale),
                      const HomeWorkspaceSection(),
                      SizedBox(height: 13 * scale),
                      const Text(
                        'UP NEXT',
                        style: TextStyle(
                          letterSpacing: 1.4,
                          fontSize: 13,
                          fontWeight: FontWeight.w600,
                          color: AppColors.textGrey,
                        ),
                      ),
                      SizedBox(height: 13 * scale),
                      const HomeUpNextSection(),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
