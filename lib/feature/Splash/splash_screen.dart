import 'dart:async';
import 'package:flutter/material.dart';

import '../../core/Utils/services/token_storage.dart';
import '../Login/login_screen.dart';
import '../Navigation/main_navigation_screen.dart';

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> {
  @override
  void initState() {
    super.initState();

    WidgetsBinding.instance.addPostFrameCallback((_) {
      _navigate();
    });
  }

  Future<void> _navigate() async {
    await Future.delayed(const Duration(seconds: 2));

    final bool isLoggedIn = await TokenStorage.getLoginStatus();
    debugPrint("LOGIN STATUS = $isLoggedIn");

    if (!mounted) return;

    if (!isLoggedIn) {
      _goToLogin();
      return;
    }

    final isExpired = await TokenStorage.isTokenExpired();
    if (isExpired) {
      debugPrint("TOKEN EXPIRED → LOGOUT");
      await TokenStorage.logout();
      _goToLogin();
      return;
    }

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
      MaterialPageRoute(
        builder: (_) => const MainNavigationScreen(),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      backgroundColor: Colors.white,
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Image(
              image: AssetImage('img/app_icon.png'),
              width: 120,
              height: 120,
            ),
            SizedBox(height: 20),
            CircularProgressIndicator(),
          ],
        ),
      ),
    );
  }
}
