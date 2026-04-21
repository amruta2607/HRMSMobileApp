import 'package:flutter/material.dart';
import '../../core/Utils/services/LogIn_out/auth_service.dart';
import '../login/widgets/input_field.dart';

class ResetPasswordScreen extends StatefulWidget {
  final String email;

  const ResetPasswordScreen({
    super.key,
    required this.email,
  });

  @override
  State<ResetPasswordScreen> createState() => _ResetPasswordScreenState();
}

class _ResetPasswordScreenState extends State<ResetPasswordScreen> {
  final _otpController = TextEditingController();
  final _passwordController = TextEditingController();
  final AuthService _authService = AuthService();

  bool _isLoading = false;

  Future<void> _submitReset() async {
    final otp = _otpController.text.trim();
    final password = _passwordController.text.trim();

    if (otp.isEmpty) {
      _showSnackBar('Please enter the OTP');
      return;
    }
    if (password.isEmpty) {
      _showSnackBar('Please enter new password');
      return;
    }

    setState(() => _isLoading = true);

    try {
      final response = await _authService.resetPassword(
        model: {
          "email": widget.email,
          "otp": otp,
          "new_password": password,
        },
      );

      if (!mounted) return;
      setState(() => _isLoading = false);

      if (response['success'] == true) {
        _showSnackBar('Password reset successful!');
        Navigator.pop(context);
        Navigator.pop(context);
      } else {
        _showSnackBar(response['message'] ?? 'Reset failed');
      }
    } catch (e) {
      _handleError(e);
    }
  }

  void _showSnackBar(String message) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
  }

  void _handleError(dynamic e) {
    if (!mounted) return;
    setState(() => _isLoading = false);
    _showSnackBar('Error: ${e.toString()}');
  }

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;
    const designWidth = 402.0;
    final scale = (width / designWidth).clamp(0.85, 1.1);

    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios, color: Colors.black),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      body: SafeArea(
        child: Padding(
          padding: EdgeInsets.all(25 * scale),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                "Reset Password",
                style: TextStyle(
                  fontSize: 28 * scale,
                  fontWeight: FontWeight.bold,
                ),
              ),
              SizedBox(height: 10 * scale),
              Text(
                "Enter the OTP sent to ${widget.email} and your new password.",
                style: TextStyle(
                  color: const Color(0xFF64748B),
                  fontSize: 16 * scale,
                ),
              ),
              SizedBox(height: 40 * scale),

              const Text(
                "EMAIL",
                style: TextStyle(fontSize: 12, fontWeight: FontWeight.w600, color: Colors.grey, letterSpacing: 0.6),
              ),
              const SizedBox(height: 6),
              Container(
                width: double.infinity,
                padding: EdgeInsets.symmetric(
                  horizontal: 16 * scale,
                  vertical: 16 * scale,
                ),
                decoration: BoxDecoration(
                  color: const Color(0xffF1F4F9),
                  borderRadius: BorderRadius.circular(14 * scale),
                ),
                child: Text(
                  widget.email,
                  style: TextStyle(
                    fontSize: 16 * scale,
                    color: Colors.black87,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ),
              SizedBox(height: 20 * scale),

              const Text(
                "OTP",
                style: TextStyle(fontSize: 12, fontWeight: FontWeight.w600, color: Colors.grey, letterSpacing: 0.6),
              ),
              const SizedBox(height: 6),
              InputField(
                hint: "Enter OTP",
                icon: Icons.lock_clock,
                controller: _otpController,
                keyboardType: TextInputType.number,
              ),
              SizedBox(height: 20 * scale),

              const Text(
                "NEW PASSWORD",
                style: TextStyle(fontSize: 12, fontWeight: FontWeight.w600, color: Colors.grey, letterSpacing: 0.6),
              ),
              const SizedBox(height: 6),
              InputField(
                hint: "Enter new password",
                icon: Icons.lock_outline,
                isPassword: true,
                controller: _passwordController,
                keyboardType: TextInputType.visiblePassword,
              ),

              SizedBox(height: 40 * scale),

              SizedBox(
                width: double.infinity,
                height: 55 * scale,
                child: ElevatedButton(
                  onPressed: _isLoading ? null : _submitReset,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: const Color(0xff3563F3),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(16 * scale),
                    ),
                  ),
                  child: _isLoading
                      ? const CircularProgressIndicator(color: Colors.white)
                      : const Text(
                    "Reset Password",
                    style: TextStyle(
                      color: Colors.white,
                      fontWeight: FontWeight.bold,
                      fontSize: 16,
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
