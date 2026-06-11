import 'package:flutter/material.dart';

class ToggleItem extends StatelessWidget {
  final String text;
  final bool selected;
  final VoidCallback onTap;

  const ToggleItem({
    super.key,
    required this.text,
    required this.selected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: GestureDetector(
        onTap: onTap,
        behavior: HitTestBehavior.opaque,
        child: Container(
          margin: const EdgeInsets.all(4),
          alignment: Alignment.center,
          decoration: BoxDecoration(
            color: selected ? Colors.white : Colors.transparent,
            borderRadius: BorderRadius.circular(12),
          ),
          child: Text(
            text,
            textAlign: TextAlign.center,
            style: TextStyle(
              fontFamily: 'Inter',
              fontSize: 14.93,
              fontWeight: FontWeight.w500,
              height: 19.91 / 14.93,
              letterSpacing: 0,
              color: selected ? Colors.black : Color(0xFF64748B),
            ),
          ),
        ),
      ),
    );
  }
}
