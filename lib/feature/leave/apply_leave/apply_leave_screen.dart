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

  // ===================== SELECT LEAVE TYPE (NEW DESIGN) =====================


  Widget _leaveTypeDropdown(double scale) {
    return GestureDetector(
      onTap: _showLeaveTypeDialog,
      child: Container(
        height: 52 * scale,
        padding: EdgeInsets.symmetric(horizontal: 16 * scale),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12 * scale),
          border: Border.all(color: const Color(0xFF0F172A), width: 1),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Expanded(
              child: _selectedLeaveType == null
                  ? Text(
                "Select Leave Type",
                style: TextStyle(
                  fontFamily: 'Inter',
                  fontWeight: FontWeight.w500,
                  fontSize: 14 * scale,
                  color: Colors.grey,
                ),
                overflow: TextOverflow.ellipsis,
              )
                  : Text.rich(
                TextSpan(
                  children: [
                    TextSpan(
                      text: "${_selectedLeaveType!.leaveTypeName} ",
                      style: TextStyle(
                        fontFamily: 'Inter',
                        fontWeight: FontWeight.w500,
                        fontSize: 14 * scale,
                        color: const Color(0xFF0F172A),
                      ),
                    ),
                    TextSpan(
                      text:
                      "(${_selectedLeaveType!.remainingBalance} days left)",
                      style: TextStyle(
                        fontFamily: 'Inter',
                        fontWeight: FontWeight.w600, // Semi Bold
                        fontSize: 14 * scale,
                        height: 1.0, // close to 14.07px line-height
                        letterSpacing: 0,
                        color: const Color(0xFF808080), // #808080
                      ),
                    ),
                  ],
                ),
                overflow: TextOverflow.ellipsis,
                maxLines: 1,
              ),

            ),
            const Icon(Icons.keyboard_arrow_down),
          ],
        ),
      ),
    );
  }

  void _showLeaveTypeDialog() {
    showDialog(
      context: context,
      builder: (context) {
        return Dialog(
          backgroundColor: Colors.white,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16), // Outer radius 16
          ),
          child: SizedBox(
            width: 316, // Outer card width
            child: Padding(
              padding: const EdgeInsets.symmetric(
                vertical: 20,
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [

                  // Title
                  const Padding(
                    padding: EdgeInsets.symmetric(horizontal: 24),
                    child: Text(
                      "Select Leave Type:",
                      style: TextStyle(
                        fontFamily: 'Inter',
                        fontWeight: FontWeight.w700,
                        fontSize: 16,
                        color: Color(0xFF0F172A),
                      ),
                    ),
                  ),

                  const SizedBox(height: 20),

                  // Leave Type List
                  ..._leaveTypes.map((type) {
                    return Padding(
                      padding: const EdgeInsets.only(
                        left: 24,
                        bottom: 22,
                        right: 24,

                      ),
                      child: GestureDetector(
                        onTap: () {
                          setState(() {
                            _selectedLeaveType = type;
                          });
                          Navigator.pop(context);
                        },
                        child: Container(
                          width: 258,  // Inner width
                          height: 49,  // Inner height
                          padding: const EdgeInsets.symmetric(
                            horizontal: 15,
                          ),
                          decoration: BoxDecoration(
                            borderRadius: BorderRadius.circular(10), // 10px
                            border: Border.all(

                              color: const Color(0xFF0F172A),
                              width: 1, // 1px border
                            ),
                          ),
                          child: Row(
                            mainAxisAlignment:
                            MainAxisAlignment.spaceBetween,
                            crossAxisAlignment:
                            CrossAxisAlignment.center,
                            children: [

                              // Leave Type Name
                              Expanded(
                                child: Text(
                                  type.leaveTypeName,
                                  style: const TextStyle(
                                    fontFamily: 'Inter',
                                    fontWeight: FontWeight.w600,
                                    fontSize: 14,
                                    color: Color(0xFF0F172A),
                                  ),
                                  overflow: TextOverflow.ellipsis,
                                ),
                              ),

                              // Remaining Days
                              Text(
                                "${type.remainingBalance} days left",
                                style: const TextStyle(
                                  fontFamily: 'Inter',
                                  fontWeight: FontWeight.w500,
                                  fontSize: 12,
                                  color: Colors.black,
                                ),
                              ),
                            ],
                          ),

                        ),
                      ),
                    );
                  }).toList(),

                  const SizedBox(height: 8),
                ],
              ),
            ),
          ),
        );
      },
    );
  }

  // ===================== REST OF YOUR ORIGINAL CODE =====================

  Future<void> _selectDate(BuildContext context, bool isStart) async {
    final DateTime? picked = await showDatePicker(
      context: context,
      initialDate: isStart
          ? DateTime.now()
          : (_startDate ?? DateTime.now()),
      firstDate: isStart
          ? DateTime.now()
          : (_startDate ?? DateTime.now()),
      lastDate: DateTime(2101),
      builder: (context, child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: const ColorScheme.light(
              primary: Color(0xFF0F172A),
              onPrimary: Colors.white,
              onSurface: Color(0xFF0F172A),
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
          if (_endDate != null &&
              _endDate!.isBefore(_startDate!)) {
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
      fp.FilePickerResult? result =
      await fp.FilePicker.platform.pickFiles(
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

    int duration =
        _endDate!.difference(_startDate!).inDays + 1;

    final success =
    await LeaveService.submitLeaveApplication(
      leaveTypeId: _selectedLeaveType!.leaveTypeId,
      startDate: _startDate!,
      endDate: _endDate!,
      reason: _reasonController.text,
      isHalfDay: false,
      duration: duration,
      attachmentPath: _selectedFile?.path,
    );

    setState(() {
      _isSubmitting = false;
    });

    if (success) {
      Navigator.push(
        context,
        MaterialPageRoute(
            builder: (context) =>
            const LeaveSuccessScreen()),
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
        currentIndex: 0,
        onChanged: (index) {
          Navigator.pushAndRemoveUntil(
            context,
            MaterialPageRoute(
              builder: (context) =>
                  MainNavigationScreen(initialIndex: index),
            ),
                (route) => false,
          );
        },
      ),
      body: SafeArea(
        child: Column(
          children: [
            Padding(
              padding:
              EdgeInsets.symmetric(horizontal: 16 * scale),
              child: Column(
                children: [
                  SizedBox(height: 10 * scale),
                  SizedBox(
                    height: 37 * scale,
                    child: Row(
                      children: [
                        SizedBox(
                          width: 24 * scale,
                          height: 25 * scale,
                          child: InkWell(
                            onTap: () =>
                                Navigator.pop(context),
                            child: Icon(
                              Icons.arrow_back_ios,
                              size: 15 * scale,
                              color:
                              const Color(0xFF0F172A),
                            ),
                          ),
                        ),
                        Text(
                          "Apply a Leave",
                          style: TextStyle(
                            fontFamily: 'Inter',
                            fontWeight: FontWeight.w700,
                            fontSize: 20 * scale,
                            color:
                            const Color(0xFF0F172A),
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
                padding: EdgeInsets.symmetric(
                    horizontal: 16 * scale),
                child: Column(
                  crossAxisAlignment:
                  CrossAxisAlignment.start,
                  children: [
                    SizedBox(height: 17 * scale),

                    _sectionTitle(
                        "Select Leave Type", scale),
                    SizedBox(height: 16 * scale),

                    if (_isLoadingTypes)
                      const Center(
                          child:
                          CircularProgressIndicator())
                    else if (_leaveTypes.isEmpty)
                      const Text(
                          "No leave types available")
                    else
                      _leaveTypeDropdown(scale),

                    SizedBox(height: 28 * scale),

                    Row(
                      children: [
                        Text(
                          "Select Days",
                          style: TextStyle(
                            fontFamily: 'Inter',
                            fontWeight: FontWeight.w600,
                            fontSize: 18 * scale,
                            letterSpacing: -0.68 * scale,
                            color: const Color(0xFF0F172A),
                          ),
                        ),
                        SizedBox(width: 8 * scale),
                        if (_startDate != null && _endDate != null)
                          Text(
                            "(-${_endDate!.difference(_startDate!).inDays + 1} "
                                "${(_endDate!.difference(_startDate!).inDays + 1) == 1 ? 'day' : 'days'})",
                            style: TextStyle(
                              fontFamily: 'Inter',
                              fontWeight: FontWeight.w500,
                              fontSize: 16 * scale,
                              color: const Color(0xFF808080), // grey only for count
                            ),
                          ),
                      ],
                    ),

                    SizedBox(height: 12 * scale),

                    Row(
                      children: [
                        Expanded(
                            child: _dateField("Start Date",
                                _startDate, scale, true)),
                        SizedBox(width: 12 * scale),
                        Expanded(
                            child: _dateField("End Date",
                                _endDate, scale, false)),
                      ],
                    ),

                    SizedBox(height: 28 * scale),

                    _sectionTitle(
                        "Reason for Leave", scale),
                    SizedBox(height: 12 * scale),

                    Container(
                      constraints: BoxConstraints(
                          minHeight: 99 * scale),
                      padding: EdgeInsets.all(12 * scale),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius:
                        BorderRadius.circular(
                            10 * scale),
                        border: Border.all(
                            color:
                            const Color(0xFF5D6063)),
                        boxShadow: [
                          BoxShadow(
                            color:
                            const Color(0x40000000),
                            blurRadius: 4 * scale,
                          ),
                        ],
                      ),
                      child: TextField(
                        controller:
                        _reasonController,
                        maxLines: null,
                        decoration: InputDecoration(
                          border: InputBorder.none,
                          hintText:
                          "Optional note for your manager",
                          hintStyle: TextStyle(
                            fontFamily: 'Inter',
                            fontWeight:
                            FontWeight.w500,
                            fontSize:
                            14 * scale,
                            color: Colors.grey,
                          ),
                        ),
                      ),
                    ),

                    SizedBox(height: 28 * scale),

                    RichText(
                      text: TextSpan(
                        children: [
                          TextSpan(
                            text: "Attach Document ",
                            style: TextStyle(
                              fontFamily: 'Inter',
                              fontWeight: FontWeight.w600,
                              fontSize: 18 * scale, // adjust if needed
                              color: const Color(0xFF0F172A),
                            ),
                          ),
                          TextSpan(
                            text: "(Optional)",
                            style: TextStyle(
                              fontFamily: 'Inter',
                              fontWeight: FontWeight.w500,
                              fontSize: 18 * scale,
                              color: const Color(0xFF808080), // #808080
                            ),
                          ),
                        ],
                      ),
                    ),

                    SizedBox(height: 16 * scale),
                    Column(
                      crossAxisAlignment:
                      CrossAxisAlignment.start,
                      children: [
                        _uploadButton(scale),
                        SizedBox(height: 8 * scale),
                        Padding(
                          padding: EdgeInsets.only(
                              left: 8 * scale),
                          child: Text(
                            "PDF, JPG, PNG",
                            style: TextStyle(
                              fontFamily: 'Inter',
                              fontWeight:
                              FontWeight.w300,
                              fontSize:
                              12 * scale,
                              color: Colors.black,
                            ),
                          ),
                        ),
                        if (_selectedFile != null)
                          Text(
                            "Selected: ${_selectedFile!.name}",
                            style: TextStyle(
                              fontFamily: 'Inter',
                              fontWeight:
                              FontWeight.w500,
                              fontSize:
                              12 * scale,
                              color:
                              const Color(
                                  0xFF0F172A),
                            ),
                          ),
                      ],
                    ),

                    SizedBox(height: 45 * scale),

                    AppPrimaryButton(
                      onTap: _isSubmitting
                          ? () {}
                          : _submitApplication,
                      child: _isSubmitting
                          ? const Center(
                          child:
                          CircularProgressIndicator(
                              color:
                              Colors.white))
                          : Text(
                        "Submit Leave Application",
                        style: TextStyle(
                          fontFamily:
                          'Roboto',
                          fontWeight:
                          FontWeight.w500,
                          fontSize:
                          20 * scale,
                          color: Colors.white,
                        ),
                      ),
                    ),

                    const SizedBox(height: 15),
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
        color: const Color(0xFF0F172A),
      ),
    );
  }

  Widget _dateField(
      String label, DateTime? date, double scale, bool isStart) {
    return GestureDetector(
      onTap: () => _selectDate(context, isStart),
      child: Container(
        height: 49 * scale,
        padding: EdgeInsets.symmetric(
            horizontal: 12 * scale),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius:
          BorderRadius.circular(10 * scale),
          border: Border.all(
              color: const Color(0xFF5D6063)),
          boxShadow: [
            BoxShadow(
              color: const Color(0x40000000),
              blurRadius: 4 * scale,
            ),
          ],
        ),
        child: Row(
          mainAxisAlignment:
          MainAxisAlignment.spaceBetween,
          children: [
            Text(
              date != null
                  ? DateFormat('dd MMM yyyy')
                  .format(date)
                  : label,
            ),
            Icon(Icons.calendar_today,
                size: 18 * scale,
                color:
                const Color(0xFF0F172A)),
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
          padding: EdgeInsets.symmetric(
              horizontal: 16 * scale),
          decoration: BoxDecoration(
            color: const Color(0x295D6063),
            borderRadius:
            BorderRadius.circular(
                10 * scale),
            border: Border.all(
                color:
                const Color(0xFF5D6063)),
          ),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Image.asset(
                "img/upload_icon.png",
                width: 18 * scale,
                height: 18 * scale,
                color:
                const Color(0xFF0F172A),
              ),
              SizedBox(width: 8 * scale),
              Text(
                "Upload Document",
                style: TextStyle(
                  fontFamily: 'Inter',
                  fontWeight:
                  FontWeight.w600,
                  fontSize:
                  14 * scale,
                  color:
                  const Color(
                      0xFF0F172A),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
