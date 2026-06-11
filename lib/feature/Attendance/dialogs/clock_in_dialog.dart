import 'dart:io';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:intl/intl.dart';

import '../../../core/Theme/app_colors.dart';
import '../../../core/Utils/services/Time_Location/live_time.dart';
import '../../../core/Utils/services/Time_Location/location_service.dart';
import 'dialog_button.dart';

class ClockInDialog extends StatefulWidget {
  final void Function(DateTime time, File image) onConfirm;

  const ClockInDialog({
    super.key,
    required this.onConfirm,
  });

  @override
  State<ClockInDialog> createState() => _ClockInDialogState();
}

class _ClockInDialogState extends State<ClockInDialog> {
  late Future<String> _locationFuture;
  DateTime _currentTime = DateTime.now();
  File? _capturedImage;
  bool _isCapturing = false;

  @override
  void initState() {
    super.initState();
    _locationFuture = LocationService.getLocation();
  }

  Future<void> _captureImage() async {
    setState(() => _isCapturing = true);
    try {
      final picker = ImagePicker();
      final XFile? photo = await picker.pickImage(
        source: ImageSource.camera,
        preferredCameraDevice: CameraDevice.front,
        maxWidth: 800,
        maxHeight: 800,
        imageQuality: 80,
      );
      if (photo != null) {
        setState(() => _capturedImage = File(photo.path));
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Camera error: $e')),
        );
      }
    } finally {
      if (mounted) setState(() => _isCapturing = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;

    return Dialog(
      backgroundColor: Colors.white,
      insetPadding: const EdgeInsets.symmetric(horizontal: 24),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
      ),
      child: ConstrainedBox(
        constraints: BoxConstraints(
          maxWidth: screenWidth * 0.9,

        ),
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 18, 20, 16),

          child: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                /// HEADER
                const Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(
                      Icons.access_time,
                      size: 28,
                      color: AppColors.primaryBlue,
                    ),
                    SizedBox(width: 10),
                    Text(
                      'Do you want to Punch-in?',
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 14),

                /// TIME + LOCATION
                Column(
                  children: [
                    StreamBuilder<DateTime>(
                      stream: LiveTime.stream(),
                      builder: (context, snapshot) {
                        _currentTime = snapshot.data ?? DateTime.now();
                        return Text(
                          'Time : ${DateFormat('hh:mm a').format(_currentTime)}',
                          style: const TextStyle(
                            fontSize: 14,
                            color: Color(0xFF64748B),

                          ),
                        );
                      },
                    ),
                    const SizedBox(height: 6),
                    FutureBuilder<String>(
                      future: _locationFuture,
                      builder: (_, s) => Text(
                        'Place : ${s.data ?? 'Fetching...'}',
                        textAlign: TextAlign.center,
                        style: const TextStyle(
                          fontSize: 14,
                          color: Color(0xFF64748B),

                        ),
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 16),

                /// CAMERA CAPTURE SECTION
                GestureDetector(
                  onTap: _isCapturing ? null : _captureImage,
                  child: Container(
                    width: double.infinity,
                    constraints: const BoxConstraints(minHeight: 220),
                    decoration: BoxDecoration(
                      color: const Color(0xFFF8FAFC),
                      borderRadius: BorderRadius.circular(14),
                      border: Border.all(
                        color: _capturedImage != null
                            ? AppColors.primaryBlue.withOpacity(0.4)
                            : const Color(0xFFE2E8F0),
                        width: 1.5,
                      ),
                    ),
                    child: _isCapturing
                        ? const Center(
                      child: SizedBox(
                        width: 32,
                        height: 32,
                        child: CircularProgressIndicator(strokeWidth: 2.5),
                      ),
                    )
                        : _capturedImage != null
                        ? Stack(
                      children: [
                        ClipRRect(
                          borderRadius: BorderRadius.circular(12),
                          child: Image.file(
                            _capturedImage!,
                            width: double.infinity,
                            fit: BoxFit.contain,
                          ),
                        ),
                        Positioned(
                          bottom: 10,
                          right: 10,
                          child: GestureDetector(
                            onTap: _captureImage,
                            child: Container(
                              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                              decoration: BoxDecoration(
                                color: Colors.white,
                                borderRadius: BorderRadius.circular(20),
                                boxShadow: [
                                  BoxShadow(
                                    color: Colors.black.withOpacity(0.15),
                                    blurRadius: 8,
                                    offset: const Offset(0, 2),
                                  ),
                                ],
                              ),
                              child: const Row(
                                mainAxisSize: MainAxisSize.min,
                                children: [
                                  Icon(Icons.camera_alt, color: Color(0xFF334155), size: 16),
                                  SizedBox(width: 4),
                                  Text(
                                    'Retake',
                                    style: TextStyle(
                                      fontSize: 12,
                                      fontWeight: FontWeight.w600,
                                      color: Color(0xFF334155),
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ),
                        ),
                      ],
                    )
                        : Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Container(
                          padding: const EdgeInsets.all(16),
                          decoration: BoxDecoration(
                            color: AppColors.primaryBlue.withOpacity(0.08),
                            shape: BoxShape.circle,
                          ),
                          child: Icon(
                            Icons.camera_alt_rounded,
                            size: 36,
                            color: AppColors.primaryBlue.withOpacity(0.7),
                          ),
                        ),
                        const SizedBox(height: 12),
                        const Text(
                          'Tap to Capture Photo',
                          style: TextStyle(
                            fontSize: 14,
                            fontWeight: FontWeight.w600,
                            color: Color(0xFF475569),
                          ),
                        ),
                        const SizedBox(height: 4),
                        const Text(
                          'A selfie is required for attendance',
                          style: TextStyle(
                            fontSize: 12,
                            color: Color(0xFF94A3B8),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),

                const SizedBox(height: 18),

                /// ACTION BUTTONS
                Row(
                  children: [
                    Expanded(
                      child: DialogButton(
                        text: 'Cancel',
                        onTap: () => Navigator.pop(context),
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: DialogButton(
                        text: 'Confirm',
                        filled: true,
                        onTap: _capturedImage == null
                            ? () {
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(
                              content: Text('Please capture a photo before confirming'),
                            ),
                          );
                        }
                            : () {
                          Navigator.pop(context);
                          print("*************************** On confirm Punch IN");
                          print(_currentTime);
                          widget.onConfirm(_currentTime, _capturedImage!);
                        },
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
