import 'package:flutter/material.dart';
import '../../../core/Theme/app_colors.dart';

class DialogButton extends StatelessWidget {
  final String text;
  final VoidCallback onTap;
  final bool filled;

  final Color? borderColor;
  final Color? filledColor;
  final Color? textColor;

  const DialogButton({
    super.key,
    required this.text,
    required this.onTap,
    this.filled = false,
    this.borderColor,
    this.filledColor,
    this.textColor,
  });

  @override
  Widget build(BuildContext context) {
    final Color effectiveBorderColor =
        borderColor ?? AppColors.primaryBlue;

    final Color effectiveFilledColor =
        filledColor ?? AppColors.primaryBlue;

    final Color effectiveTextColor =
        textColor ??
            (filled ? Colors.white : effectiveBorderColor);

    return ConstrainedBox(
      constraints: const BoxConstraints(
        minHeight: 36,
        minWidth: 100,
      ),
      child: ElevatedButton(
        onPressed: onTap,
        style: ElevatedButton.styleFrom(
          elevation: 0,
          backgroundColor:
          filled ? effectiveFilledColor : Colors.transparent,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(4),
            side: filled
                ? BorderSide.none
                : BorderSide(
              color: effectiveBorderColor,
              width: 1.4,
            ),
          ),
        ),
        child: FittedBox(
          fit: BoxFit.scaleDown,
          child: Text(
            text,
            maxLines: 1,
            style: TextStyle(
              color: effectiveTextColor,
              fontWeight: FontWeight.w600,
            ),
          ),
        ),
      ),
    );
  }
}
