import 'package:flutter/material.dart';

class RecentLeaveSection extends StatelessWidget {
  const RecentLeaveSection({super.key});

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(height: 24 * scale),

        Padding(
          padding: EdgeInsets.symmetric(horizontal: 20 * scale),
          child: Text(
            "RECENT LEAVE REQUESTS",
            style: TextStyle(
              fontFamily: 'Inter',
              fontWeight: FontWeight.w600,
              fontSize: 14 * scale,
              height: 14.07 / 14,
              color: Colors.grey,
            ),
          ),
        ),

        SizedBox(height: 16 * scale),

        Expanded(
          child: ListView(
            padding: EdgeInsets.symmetric(horizontal: 20 * scale),
            children: [
              SizedBox(height: 5 * scale),
              LeaveRequestTile(

                title: "Casual Leave",
                date: "2nd Mar – 4th Mar",
              ),
              SizedBox(height: 8 * scale),
              LeaveRequestTile(
                title: "Casual Leave",
                date: "8th Jan",
              ),
              SizedBox(height: 8 * scale),

              LeaveRequestTile(
                title: "Sick Leave",
                date: "20th Nov – 24th Nov",
              ),
              SizedBox(height: 8 * scale),

            ],
          ),
        ),
      ],
    );
  }
}

class LeaveRequestTile extends StatelessWidget {
  final String title;
  final String date;

  const LeaveRequestTile({
    super.key,
    required this.title,
    required this.date,
  });

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return Container(
      constraints: BoxConstraints(
        minHeight: 62 * scale,
      ),
      margin: EdgeInsets.only(bottom: 12 * scale),
      padding: EdgeInsets.symmetric(
        horizontal: 16 * scale,
        vertical: 10 * scale,
      ),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(10 * scale),
        boxShadow: [
          BoxShadow(
            color: const Color(0x59000000),
            blurRadius: 4.5 * scale,
            offset: Offset.zero,
          ),
        ],
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Expanded(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: TextStyle(
                    fontFamily: 'Inter',
                    fontWeight: FontWeight.w600,
                    fontSize: 14 * scale,
                    height: 14.07 / 14,
                    color: Colors.black,
                  ),
                ),
                SizedBox(height: 6 * scale),
                Text(
                  date,
                  style: TextStyle(
                    fontFamily: 'Inter',
                    fontWeight: FontWeight.w500,
                    fontSize: 12 * scale,
                    height: 14.07 / 12,
                    color: Colors.black,
                  ),
                ),
              ],
            ),
          ),

          Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(
                "View",
                style: TextStyle(
                  fontFamily: 'Inter',
                  fontWeight: FontWeight.bold,
                  fontSize: 12 * scale,
                  height: 14.07 / 12,
                  color: const Color(0xFF0F62FE),
                ),
              ),
              SizedBox(width: 3 * scale),
              Icon(
                Icons.arrow_forward_ios,
                size: 12 * scale,
                color: const Color(0xFF0F62FE),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
