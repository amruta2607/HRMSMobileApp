import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';

class HomeScreenScroll extends StatelessWidget {
  final Widget body;
  final Widget? floating;

  const HomeScreenScroll({
    super.key,
    required this.body,
    this.floating,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(child: body),
      floatingActionButton: floating,
    );
  }
}
