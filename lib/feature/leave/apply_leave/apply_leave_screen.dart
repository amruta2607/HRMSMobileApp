import 'package:flutter/material.dart';
import '../../Reuse_Widgets/leave_primary_button.dart';
import '../../Navigation/navigation_bar.dart';
import '../../Navigation/main_navigation_screen.dart';
import '../../../../core/Utils/services/leave_service/leave_service.dart';

import 'package:file_picker/file_picker.dart' as fp;
import 'package:intl/intl.dart';
import '../leave_success_screen.dart';
import '../model/leave_balence_model.dart';

class ApplyLeaveScreen extends StatefulWidget {
  const ApplyLeaveScreen({super.key});

  @override
  State<ApplyLeaveScreen> createState() => _ApplyLeaveScreenState();
}

class _ApplyLeaveScreenState extends State<ApplyLeaveScreen> {
  DateTime? _startDate;
  DateTime? _endDate;
  fp.PlatformFile? _selectedFile;

  LeaveBalanceModel? _selectedLeaveType;

  final TextEditingController _reasonController = TextEditingController();

  List<LeaveBalanceModel> _leaveTypes = [];
  bool _isLoadingTypes = true;
  bool _isSubmitting = false;

  @override
  void initState() {
    super.initState();
    _fetchLeaveTypes();
  }

  Future<void> _fetchLeaveTypes() async {
    setState(() {
      _isLoadingTypes = true;
    });

    final types = await LeaveService.getLeaveBalance();

    setState(() {
      _leaveTypes = types ?? [];
      _isLoadingTypes = false;
    });
  }

  @override
  void dispose() {
    _reasonController.dispose();
    super.dispose();
  }

  Future<void> _selectDate(BuildContext context, bool isStart) async {
    final DateTime? picked = await showDatePicker(
      context: context,
      initialDate: DateTime.now(),
      firstDate: DateTime.now(),
      lastDate: DateTime(2101),
      builder: (context, child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: const ColorScheme.light(
              primary: Color(0xFF0F172A),
              onPrimary: Colors.white,
              onSurface: Color(0xFF0F172A),
            ),
            textButtonTheme: TextButtonThemeData(
              style: TextButton.styleFrom(
                foregroundColor: const Color(0xFF0F172A),
              ),
            ),
          ),
          child: child!,
        );
      },
    );
    if (picked != null) {
      setState(() {
        if (isStart) {
          _startDate = picked;
          if (_endDate != null && _endDate!.isBefore(_startDate!)) {
            _endDate = null;
          }
        } else {
          _endDate = picked;
        }
      });
    }
  }

  Future<void> _pickFile() async {
    try {
      fp.FilePickerResult? result = await fp.FilePicker.platform.pickFiles(
        type: fp.FileType.custom,
        allowedExtensions: ['pdf', 'jpg', 'jpeg', 'png'],
      );

      if (result != null) {
        setState(() {
          _selectedFile = result.files.first;
        });
      }
    } catch (e) {
      debugPrint("Error picking file: $e");
    }
  }

  Future<void> _submitApplication() async {
    if (_selectedLeaveType == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Please select a Leave Type")),
      );
      return;
    }
    if (_startDate == null || _endDate == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Please select Start and End Date")),
      );
      return;
    }

    setState(() {
      _isSubmitting = true;
    });

    final success = await LeaveService.submitLeaveApplication(
      leaveTypeId: _selectedLeaveType!.leaveTypeId,
      startDate: _startDate!,
      endDate: _endDate!,
      reason: _reasonController.text,
      isHalfDay: false, // Hardcoded to false for now unless UI has toggle
      attachmentPath: _selectedFile?.path,
    );

    setState(() {
      _isSubmitting = false;
    });

    if (success) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text("Leave Application Submitted Successfully"),
          backgroundColor: Colors.green,
        ),
      );

      Navigator.push(
        context,
        MaterialPageRoute(builder: (context) => const LeaveSuccessScreen()),
      ).then((value) {
        if (value == true) {
          // Prepare data to return
          // Format date for display: "2nd Mar" or "2nd Mar - 4th Mar"
          // We'll use a simple format for now
          String dateStr = DateFormat("d MMM").format(_startDate!);
          int days = 1;

          if (_endDate != null) {
            days = _endDate!.difference(_startDate!).inDays + 1;
            if (_startDate != _endDate) {
              dateStr += " - ${DateFormat("d MMM").format(_endDate!)}";
            }
          }

          // Add day count in brackets
          dateStr += " ($days ${days == 1 ? 'day' : 'days'})";

          final leaveData = {
            "title": _selectedLeaveType?.leaveTypeName ?? "Leave",
            "date": dateStr,
            "reason": _reasonController.text,
            "status": "Pending",
          };

          Navigator.pop(context, leaveData);
        }
      });
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text("Failed to submit leave application"),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;
    final scale = width / 375;

    return Scaffold(
      backgroundColor: Colors.white,
      bottomNavigationBar: CustomNavigationBar(
        currentIndex: 0, // Highlight Home
        onChanged: (index) {
          Navigator.pushAndRemoveUntil(
            context,
            MaterialPageRoute(
              builder: (context) => MainNavigationScreen(initialIndex: index),
            ),
                (route) => false,
          );
        },
      ),
      body: SafeArea(
        child: Column(
          children: [
            Padding(
              padding: EdgeInsets.symmetric(horizontal: 16 * scale),
              child: Column(
                children: [
                  SizedBox(height: 10 * scale),

                  /// HEADER
                  SizedBox(
                    height: 37 * scale,
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.center,
                      children: [
                        SizedBox(
                          width: 24 * scale,
                          height: 25 * scale,
                          child: InkWell(
                            onTap: () => Navigator.pop(context),
                            child: Icon(
                              Icons.arrow_back_ios,
                              size: 15 * scale,
                              color: const Color(0xFF0F172A),
                            ),
                          ),
                        ),
                        Text(
                          "Apply a Leave",
                          style: TextStyle(
                            fontFamily: 'Inter',
                            fontWeight: FontWeight.w700,
                            fontSize: 20 * scale,
                            height: 36.31 / 20,
                            letterSpacing: -0.68 * scale,
                            color: const Color(0xFF0F172A),
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            Expanded(
              child: SingleChildScrollView(
                padding: EdgeInsets.symmetric(horizontal: 16 * scale),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    SizedBox(height: 17 * scale),

                    /// SELECT LEAVE TYPE
                    _sectionTitle("Select Leave Type", scale),
                    SizedBox(height: 16 * scale),

                    if (_isLoadingTypes)
                      const Center(child: CircularProgressIndicator())
                    else if (_leaveTypes.isEmpty)
                      const Text("No leave types available")
                    else
                      GridView.count(
                        shrinkWrap: true,
                        physics: const NeverScrollableScrollPhysics(),
                        crossAxisCount: 2,
                        crossAxisSpacing: 12 * scale,
                        mainAxisSpacing: 12 * scale,
                        childAspectRatio: 174 / 75,
                        children: _leaveTypes.map((type) {
                          return _LeaveTypeCard(
                            title: type.leaveTypeName,
                            days: type.remainingBalance,
                            isSelected: _selectedLeaveType == type,
                            onTap: () => setState(() => _selectedLeaveType = type),
                          );
                        }).toList(),
                      ),

                    SizedBox(height: 28 * scale),

                    /// SELECT DAYS
                    _sectionTitle("Select Days", scale),
                    SizedBox(height: 12 * scale),

                    Row(
                      children: [
                        Expanded(child: _dateField("Start Date", _startDate, scale, true)),
                        SizedBox(width: 12 * scale),
                        Expanded(child: _dateField("End Date", _endDate, scale, false)),
                      ],
                    ),

                    SizedBox(height: 28 * scale),

                    /// REASON
                    _sectionTitle("Reason for Leave", scale),
                    SizedBox(height: 12 * scale),

                    Container(
                      constraints: BoxConstraints(minHeight: 99 * scale),
                      padding: EdgeInsets.all(12 * scale),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(10 * scale),
                        border: Border.all(color: const Color(0xFF5D6063)),
                        boxShadow: [
                          BoxShadow(
                            color: const Color(0x40000000),
                            blurRadius: 4 * scale,
                          ),
                        ],
                      ),
                      child: TextField(
                        controller: _reasonController,
                        maxLines: null,
                        decoration: InputDecoration(
                          border: InputBorder.none,
                          hintText: "Optional note for your manager",
                          hintStyle: TextStyle(
                            fontFamily: 'Inter',
                            fontWeight: FontWeight.w500,
                            fontSize: 14 * scale,
                            color: Colors.grey,
                          ),
                        ),
                      ),
                    ),

                    SizedBox(height: 28 * scale),

                    /// ATTACH DOCUMENT
                    _sectionTitle("Attach Document", scale),
                    SizedBox(height: 16 * scale),

                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        _uploadButton(scale),
                        SizedBox(height: 0 * scale),
                        Padding(
                          padding: EdgeInsets.only(left: 8 * scale),
                          child: Text(
                            "PDF, JPG, PNG",
                            style: TextStyle(
                              fontFamily: 'Inter',
                              fontWeight: FontWeight.w300,
                              fontSize: 12 * scale,
                              height: 36.31 / 12,
                              letterSpacing: -0.68 * scale,
                              color: Colors.black,
                            ),
                          ),
                        ),
                        if (_selectedFile != null) ...[
                          SizedBox(height: 4 * scale),
                          Text(
                            "Selected: ${_selectedFile!.name}",
                            style: TextStyle(
                              fontFamily: 'Inter',
                              fontWeight: FontWeight.w500,
                              fontSize: 12 * scale,
                              color: const Color(0xFF0F172A),
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ],
                      ],
                    ),

                    SizedBox(height: 26 * scale),

                    /// SUBMIT BUTTON
                    AppPrimaryButton(
                      onTap: _isSubmitting ? () {} : _submitApplication,
                      child: _isSubmitting
                          ? const Center(child: CircularProgressIndicator(color: Colors.white))
                          : Text(
                        "Submit Leave Application",
                        style: TextStyle(
                          fontFamily: 'Roboto',
                          fontWeight: FontWeight.w500,
                          fontSize: 20 * scale,
                          letterSpacing: 0.14 * scale,
                          color: Colors.white,
                        ),
                      ),
                    ),


                    SizedBox(height: 30 * scale),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _sectionTitle(String text, double scale) {
    return Text(
      text,
      style: TextStyle(
        fontFamily: 'Inter',
        fontWeight: FontWeight.w600,
        fontSize: 18 * scale,
        letterSpacing: -0.68 * scale,
        color: const Color(0xFF0F172A),
      ),
    );
  }

  Widget _dateField(String label, DateTime? date, double scale, bool isStart) {
    return GestureDetector(
      onTap: () => _selectDate(context, isStart),
      child: Container(
        height: 49 * scale,
        padding: EdgeInsets.symmetric(horizontal: 12 * scale),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(10 * scale),
          border: Border.all(color: const Color(0xFF5D6063)),
          boxShadow: [
            BoxShadow(
              color: const Color(0x40000000),
              blurRadius: 4 * scale,
            ),
          ],
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              date != null ? DateFormat('dd MMM yyyy').format(date) : label,
              style: TextStyle(
                fontFamily: 'Inter',
                fontWeight: FontWeight.w500,
                fontSize: 14 * scale,
                color: date != null ? const Color(0xFF0F172A) : Colors.grey,
              ),
            ),
            Icon(Icons.calendar_today,
                size: 18 * scale,
                color: const Color(0xFF0F172A)),
          ],
        ),
      ),
    );
  }

  Widget _uploadButton(double scale) {
    return GestureDetector(
      onTap: _pickFile,
      child: IntrinsicWidth(
        child: Container(
          height: 49 * scale,
          padding: EdgeInsets.symmetric(horizontal: 16 * scale),
          decoration: BoxDecoration(
            color: const Color(0x295D6063),
            borderRadius: BorderRadius.circular(10 * scale),
            border: Border.all(color: const Color(0xFF5D6063)),
            boxShadow: [
              BoxShadow(
                color: const Color(0x40000000).withOpacity(0.02),
                blurRadius: 4 * scale,
              ),
            ],
          ),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Image.asset(
                "img/upload_icon.png",
                width: 18 * scale,
                height: 18 * scale,
                color: const Color(0xFF0F172A),
              ),
              SizedBox(width: 8 * scale),
              Text(
                "Upload Document",
                style: TextStyle(
                  fontFamily: 'Inter',
                  fontWeight: FontWeight.w600,
                  fontSize: 14 * scale,
                  color: const Color(0xFF0F172A),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
class _LeaveTypeCard extends StatelessWidget {
  final String title;
  final int days;
  final bool isSelected;
  final VoidCallback onTap;

  const _LeaveTypeCard({
    required this.title,
    required this.days,
    required this.isSelected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final scale = MediaQuery.of(context).size.width / 375;

    return GestureDetector(
      onTap: onTap,
      child: Container(
        height: 75 * scale,
        padding: EdgeInsets.all(12 * scale),
        decoration: BoxDecoration(
          color: isSelected ? const Color(0xFFF1F5F9) : Colors.white,
          borderRadius: BorderRadius.circular(10 * scale),
          border: Border.all(
            color: isSelected ? const Color(0xFF0F172A) : const Color(0xFF808080),
            width: isSelected ? 2 : 1,
          ),
          boxShadow: [
            BoxShadow(
              color: const Color(0x0F000000),
              offset: Offset(0, 4 * scale),
              blurRadius: 4 * scale,
            ),
          ],
        ),
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
                color: const Color(0xFF0F172A),
              ),
            ),
            SizedBox(height: 6 * scale),
            RichText(
              text: TextSpan(
                children: [
                  TextSpan(
                    text: "$days ",
                    style: TextStyle(
                      fontFamily: 'Inter',
                      fontWeight: FontWeight.w700,
                      fontSize: 18 * scale,
                      height: 14.07 / 18,
                      color: const Color(0xFF0F172A),
                    ),
                  ),
                  TextSpan(
                    text: "Days left",
                    style: TextStyle(
                      fontFamily: 'Inter',
                      fontWeight: FontWeight.w500,
                      fontSize: 12 * scale,
                      color: Colors.grey,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
