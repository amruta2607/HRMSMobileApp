import 'package:flutter/material.dart';

class FloatingActionButtonEx extends StatelessWidget {
  final String title;
  final VoidCallback onTap;

  const FloatingActionButtonEx({
    super.key,
    required this.title,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final bottomInset = MediaQuery.of(context).padding.bottom;

    return Positioned(
      right: 20,
      bottom: 16 + bottomInset + 30,
      child: GestureDetector(
        onTap: onTap,
        child: Container(
          width: 125,
          height: 56,
          alignment: Alignment.center,
          decoration: BoxDecoration(
            color: const Color(0xFF42A5F5),
            borderRadius: BorderRadius.circular(16),
            boxShadow: const [
              BoxShadow(
                color: Color(0x1A1B2414),
                offset: Offset(0, 4),
                blurRadius: 8,
              ),
              BoxShadow(
                color: Color(0x1A1B241F),
                offset: Offset(0, 4),
                blurRadius: 8,
                spreadRadius: -2,
              ),
            ],
          ),
          child: Text(
            title,
            style: const TextStyle(
              color: Colors.white,
              fontSize: 14,
              fontWeight: FontWeight.w600,
            ),
          ),
        ),
      ),
    );
  }
}
