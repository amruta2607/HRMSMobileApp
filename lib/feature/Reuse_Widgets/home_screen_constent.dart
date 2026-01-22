import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';

class HomeScreenConstent extends StatelessWidget {
  final Widget body;
  final Widget? floating;

  const HomeScreenConstent({
    super.key,
    required this.body,
    this.floating,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: Stack(
        children: [
          SafeArea(child: body),
          if (floating != null) floating!,
        ],
      ),
    );
  }
}
