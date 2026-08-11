import 'dart:async';import 'dart:ui';
import 'package:flutter/material.dart';
import '../../core/Utils/services/token_storage.dart';
import '../Login/login_screen.dart';
import '../Navigation/main_navigation_screen.dart';

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> with SingleTickerProviderStateMixin {
  late final AnimationController _controller;
  late final Animation<double> _fadeIn;
  late final Animation<double> _slideUp;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(milliseconds: 1200),
      vsync: this,
    );

    _fadeIn = CurvedAnimation(
      parent: _controller,
      curve: Curves.easeOut,
    );

    _slideUp = Tween<double>(begin: 20, end: 0).animate(
      CurvedAnimation(
        parent: _controller,
        curve: Curves.easeOutCubic,
      ),
    );

    WidgetsBinding.instance.addPostFrameCallback((_) {
      _controller.forward();
      _navigate();
    });
  }

  Future<void> _navigate() async {
    await Future.delayed(const Duration(milliseconds: 800));

    final bool isLoggedIn = await TokenStorage.getLoginStatus();
    if (!mounted) return;

    if (!isLoggedIn) {
      _goToLogin();
      return;
    }

    // -------- OLD (session bug): getValidToken did NOT refresh --------
    // // Try to get a valid token (auto-refreshes if expired)
    // final validToken = await TokenStorage.getValidToken();
    // if (validToken == null) {
    //   // Both access token and refresh token are invalid → must re-login
    //   print('SPLASH → Token refresh failed, redirecting to login');
    //   await TokenStorage.logout();
    //   _goToLogin();
    //   return;
    // }
    //
    // await TokenStorage.loadModuleAccess();
    // _goToMainNavigation();

    // Cold start: force refresh so session stays alive after hours in background
    final validToken = await TokenStorage.ensureSession(forceRefresh: true);
    if (!mounted) return;

    if (validToken == null || validToken.isEmpty) {
      print('SPLASH → Session invalid, redirecting to login');
      await TokenStorage.logout();
      _goToLogin();
      return;
    }

    print('SPLASH → Session OK, going to home');
    await TokenStorage.loadModuleAccess();
    if (!mounted) return;
    _goToMainNavigation();
  }

  void _goToLogin() {
    Navigator.pushReplacement(
      context,
      MaterialPageRoute(builder: (_) => const LoginScreen()),
    );
  }

  void _goToMainNavigation() {
    Navigator.pushReplacement(
      context,
      MaterialPageRoute(builder: (_) => const MainNavigationScreen()),
    );
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [
              Color(0xFF1565C0),
              Color(0xFF0D47A1),
            ],
          ),
        ),
        child: FadeTransition(
          opacity: _fadeIn,
          child: Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                // Clean Logo Container
                SlideTransition(
                  position: Tween<Offset>(
                    begin: const Offset(0, 0.1),
                    end: Offset.zero,
                  ).animate(_controller),
                  child: Container(
                    width: 120,
                    height: 120,
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(28),
                      boxShadow: [
                        BoxShadow(
                          color: Colors.black.withOpacity(0.15),
                          blurRadius: 20,
                          offset: const Offset(0, 8),
                        ),
                      ],
                    ),
                    child: ClipRRect(
                      borderRadius: BorderRadius.circular(28),
                      child: Image.asset(
                        'img/app_icon.png',
                        width: 60,
                        height: 60,
                      ),
                    ),
                  ),
                ),

                const SizedBox(height: 40),

                // App Name
                SlideTransition(
                  position: Tween<Offset>(
                    begin: const Offset(0, 0.2),
                    end: Offset.zero,
                  ).animate(_controller),
                  child: const Text(
                    'Altroz HRM',
                    style: TextStyle(
                      fontSize: 32,
                      fontWeight: FontWeight.w600,
                      color: Colors.white,
                      letterSpacing: 1,
                    ),
                  ),
                ),

                const SizedBox(height: 60),

                // Simple Loader
                SizedBox(
                  width: 20,
                  height: 20,
                  child: CircularProgressIndicator(
                    strokeWidth: 2.5,
                    valueColor: AlwaysStoppedAnimation<Color>(
                      Colors.white.withOpacity(0.8),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}