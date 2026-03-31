import 'package:flutter/material.dart';
import '../../Reuse_Widgets/header_bg.dart';


class PayrollHeader extends StatelessWidget {
  const PayrollHeader({super.key});

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return HeaderBackground(
      scale: scale,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          /// ✅ Material + InkWell wraps both arrow + "Payroll" text
          Material(
            color: Colors.transparent,
            child: InkWell(
              onTap: () {
                Navigator.pop(context);
              },
              borderRadius: BorderRadius.circular(8),
              splashColor: Colors.black.withOpacity(0.1),
              highlightColor: Colors.black.withOpacity(0.05),
              child: Row(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.center,
                children: [
                  const Padding(
                    padding: EdgeInsets.only(right: 8.0, top: 4, bottom: 4),
                    child: Icon(Icons.arrow_back_ios, size: 18),
                  ),
                  Text(
                    "Payroll",
                    style: TextStyle(
                      fontSize: 24 * scale,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}