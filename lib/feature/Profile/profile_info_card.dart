import 'package:flutter/material.dart';
import '../../../core/Theme/app_colors.dart';


class ProfileInfoCard extends StatelessWidget {
  final List<ProfileInfoItem> items;

  const ProfileInfoCard({
    super.key,
    required this.items,
  });

  @override
  Widget build(BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;
    const designWidth = 402.0;
    final scale = screenWidth / designWidth;

    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16 * scale),
        boxShadow: const [
          BoxShadow(
            color: Color(0x14000000),
            blurRadius: 10,
          ),
        ],
      ),
      child: Column(
        children: List.generate(items.length * 2 - 1, (index) {
          if (index.isOdd) {
            return Divider(
              height: 0,
              thickness: 0.6 * scale,
              color: AppColors.textGrey.withOpacity(0.6),
              indent: 6.5 * scale,
              endIndent: 6.5 * scale,
            );
          }

          return _ProfileInfoRow(
            item: items[index ~/ 2],
            scale: scale,
          );
        }),
      ),
    );
  }
}

class _ProfileInfoRow extends StatelessWidget {
  final ProfileInfoItem item;
  final double scale;

  const _ProfileInfoRow({
    required this.item,
    required this.scale,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: item.isAction ? item.onTap : null,
      borderRadius: BorderRadius.circular(12 * scale),
      child: Padding(
        padding: EdgeInsets.symmetric(
          horizontal: 16 * scale,
          vertical: 14 * scale,
        ),
        child: Row(
          children: [
            if (item.imagePath != null)
              Image.asset(
                item.imagePath!,
                width: 20 * scale,
                height: 20 * scale,
                fit: BoxFit.contain,
              )
            else
              Icon(
                item.icon,
                size: 20 * scale,
                color: item.color ?? AppColors.textDark,
              ),
            SizedBox(width: 12 * scale),

            /// LABEL
            Expanded(
              child: Text(
                item.label,
                style: TextStyle(
                  fontSize: 14 * scale,
                  fontWeight: FontWeight.w600,
                  color: item.color ?? AppColors.textDark,
                ),
              ),
            ),

            /// VALUE
            if (item.value != null)
              Text(
                item.value!,
                style: TextStyle(
                  fontSize: 12 * scale,
                  color: const Color(0xFF5D6063),                ),
              ),

            /// ARROW (for action rows)
            if (item.isAction)
              Padding(
                padding: EdgeInsets.only(left: 6 * scale),
                child: Icon(
                  Icons.arrow_forward_ios,
                  size: 14 * scale,
                  color: item.color ?? AppColors.textGrey,
                ),
              ),
          ],
        ),
      ),
    );
  }
}


class ProfileInfoItem {
  final IconData? icon;
  final String? imagePath;
  final String label;
  final String? value;
  final bool isAction;
  final Color? color;
  final VoidCallback? onTap;

  const ProfileInfoItem({
    this.icon,
    this.imagePath,
    required this.label,
    this.value,
    this.isAction = false,
    this.color,
    this.onTap,
  });
}
