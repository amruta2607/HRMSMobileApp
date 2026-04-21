import 'package:flutter/material.dart';
import '../../core/Utils/services/LogIn_out/auth_service.dart';
import '../../core/validators/validation.dart';
import '../../feature/login/widgets/input_field.dart';
import 'reset_password_screen.dart';

class ForgotPasswordScreen extends StatefulWidget {
  const ForgotPasswordScreen({super.key});

  @override
  State<ForgotPasswordScreen> createState() => _ForgotPasswordScreenState();
}

class _ForgotPasswordScreenState extends State<ForgotPasswordScreen> {
  final _emailController = TextEditingController();
  final AuthService _authService = AuthService();

  bool _isLoading = false;
  String? _emailError; // 👈 Added state for inline error

  Future<void> _submitEmail() async {
    final email = _emailController.text.trim();

    // Reset error
    setState(() => _emailError = null);

    if (email.isEmpty) {
      setState(() => _emailError = 'Please enter your email');
      return;
    }
    if (!Validator.isValidEmail(email)) {
      setState(() => _emailError = 'Please enter a valid email');
      return;
    }

    setState(() => _isLoading = true);

    try {
      final response = await _authService.forgotPassword(email: email);

      if (!mounted) return;
      setState(() => _isLoading = false);

      if (response['success'] == true) {
        final message = response['message'] ?? 'OTP sent successfully';
        _showSnackBar(message);

        // Navigate to reset password screen
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (_) => ResetPasswordScreen(email: email),
          ),
        );
      } else {
        setState(() => _emailError = response['message'] ?? 'Failed to send OTP');
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
        child: SingleChildScrollView( // Added scroll view for small screens
          child: Padding(
            padding: EdgeInsets.all(25 * scale),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  "Forgot Password",
                  style: TextStyle(
                    fontSize: 28 * scale,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                SizedBox(height: 10 * scale),
                Text(
                  "Enter your registered email to reset your password.",
                  style: TextStyle(
                    color: const Color(0xFF64748B),
                    fontSize: 16 * scale,
                  ),
                ),
                SizedBox(height: 40 * scale),

                const Text(
                  "WORK EMAIL",
                  style: TextStyle(fontSize: 12, fontWeight: FontWeight.w600, color: Colors.grey, letterSpacing: 0.6),
                ),
                const SizedBox(height: 6),
                InputField(
                  hint: "name@company.com",
                  icon: Icons.email_outlined,
                  controller: _emailController,
                  errorText: _emailError, // 👈 Passing error to input field
                  keyboardType: TextInputType.emailAddress,
                ),

                SizedBox(height: 40 * scale),

                SizedBox(
                  width: double.infinity,
                  height: 55 * scale,
                  child: ElevatedButton(
                    onPressed: _isLoading ? null : _submitEmail,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color(0xff3563F3),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(16 * scale),
                      ),
                    ),
                    child: _isLoading
                        ? const CircularProgressIndicator(color: Colors.white)
                        : const Text(
                      "Send OTP",
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
      ),
    );
  }
}
