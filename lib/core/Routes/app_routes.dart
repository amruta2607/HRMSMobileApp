import 'package:flutter/material.dart';

import '../../feature/Home/home_screen.dart';
import '../../feature/Login/login_screen.dart';

class AppRoutes {
  static const login = "/";
  static const home = "/home";

  static Map<String, WidgetBuilder> routes = {
    login: (_) => const LoginScreen(),
    home: (_) =>  HomeScreen(),
  };
}
