import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'dart:io';

import 'package:provider/provider.dart';
import '../../feature/Profile/controller/profile_controller.dart';
import '../../core/validators/validation.dart';
import '../../feature/Login/Widgets/toggle_item.dart';
import '../../feature/Login/Widgets/input_field.dart';
import '../Navigation/main_navigation_screen.dart';
import 'Controller/login_controller.dart';
import 'forgot_password_screen.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  bool isEmailSelected = true;

  final fieldController = TextEditingController();
  final passwordController = TextEditingController();

  String? fieldError;
  String? passwordError;

  final LoginController _loginController = LoginController();
  bool _isLoading = false;

  void _switchMode(bool isEmail) {
    if (isEmailSelected == isEmail) return;
    setState(() {
      isEmailSelected = isEmail;
      fieldController.clear();
      passwordController.clear();
      fieldError = null;
      passwordError = null;
    });
  }

  Future<void> onLogin() async {
    final result = Validator.validateLogin(
      isEmail: isEmailSelected,
      fieldValue: fieldController.text,
      passwordValue: passwordController.text,
    );

    setState(() {
      fieldError = result.fieldError;
      passwordError = result.passwordError;
    });

    if (fieldError != null || passwordError != null) return;

    setState(() => _isLoading = true);

    try {
      if (isEmailSelected) {
        await _loginController.loginWithEmail(
          email: fieldController.text.trim(),
          password: passwordController.text.trim(),
        );
      } else {
        await _loginController.loginWithMobile(
          mobile: fieldController.text.trim(),
          pin: passwordController.text.trim(),
        );
      }

      if (!mounted) return;

      // Force refresh profile for new user
      if (mounted) {
        context.read<ProfileController>().refreshProfile();
      }

      Navigator.pushReplacement(
        context,
        MaterialPageRoute(
          builder: (_) => const MainNavigationScreen(),
        ),
      );
    } catch (e) {
      if (e is SocketException) {
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text("Connect to the Internet to Proceed"),
            backgroundColor: Colors.redAccent,
          ),
        );
        return;
      }

      final msg = e.toString().toLowerCase();

      String? newFieldError;
      String? newPasswordError;

      if (msg.contains("password") || msg.contains("otp")) {
        newPasswordError =
            e.toString().replaceAll('Exception:', '').trim();
      } else if (msg.contains("user") ||
          msg.contains("email") ||
          msg.contains("account") ||
          msg.contains("mobile")) {
        newFieldError =
            e.toString().replaceAll('Exception:', '').trim();
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
              e.toString().replaceAll('Exception:', '').trim(),
            ),
          ),
        );
      }

      setState(() {
        fieldError = newFieldError;
        passwordError = newPasswordError;
      });
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  @override
  void dispose() {
    fieldController.dispose();
    passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;
    const designWidth = 402.0;
    final scale = (width / designWidth).clamp(0.85, 1.1);

    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: EdgeInsets.symmetric(horizontal: 25 * scale),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Image.asset(
                  'img/app_icon.png',
                  height: 105 * scale,
                  width: 145 * scale,
                ),


                Text(
                  "Welcome Back",
                  style: TextStyle(
                    fontSize: 29.87 * scale,
                    fontWeight: FontWeight.bold,
                  ),
                ),

                SizedBox(height: 8 * scale),

                Text(
                  "Enter your credentials to access your workspace.",
                  style: TextStyle(
                    color: const Color(0xFF64748B),
                    fontSize: 18 * scale,
                  ),
                ),

                SizedBox(height: 23 * scale),

                Container(
                  height: 52 * scale,
                  decoration: BoxDecoration(
                    color: const Color(0xffF1F4F9),
                    borderRadius: BorderRadius.circular(14 * scale),
                  ),
                  child: Row(
                    children: [
                      ToggleItem(
                        text: "Email",
                        selected: isEmailSelected,
                        onTap: () => _switchMode(true),
                      ),
                      ToggleItem(
                        text: "Mobile",
                        selected: !isEmailSelected,
                        onTap: () => _switchMode(false),
                      ),
                    ],
                  ),
                ),

                SizedBox(height: 20 * scale),

                Text(
                  isEmailSelected
                      ? "WORK EMAIL / USER ID"
                      : "MOBILE NUMBER",
                  style: const TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.w600,
                    color: const Color(0xFF94A3B8),
                    letterSpacing: 0.6,
                  ),
                ),

                const SizedBox(height: 6),

                InputField(
                  hint: isEmailSelected
                      ? "name@altroz.com"
                      : "Mobile Number",
                  icon: isEmailSelected
                      ? Icons.email_outlined
                      : Icons.phone_outlined,
                  iconPath: isEmailSelected ? 'img/workMail.png' : null,
                  controller: fieldController,
                  errorText: fieldError,
                  keyboardType: isEmailSelected
                      ? TextInputType.emailAddress
                      : TextInputType.number,
                  inputFormatters: isEmailSelected
                      ? []
                      : [
                    FilteringTextInputFormatter.digitsOnly,
                    LengthLimitingTextInputFormatter(10),
                  ],
                ),

                SizedBox(height: 15 * scale),

                Text(
                  isEmailSelected ? "PASSWORD" : "OTP",
                  style: const TextStyle(
                    fontSize: 13,
                    fontWeight: FontWeight.w600,
                    color: const Color(0xFF94A3B8),                    letterSpacing: 0.6,
                  ),
                ),

                const SizedBox(height: 6),

                InputField(
                  hint: isEmailSelected ? "Enter Password" : "OTP",
                  icon: Icons.lock_outline,

                  iconPath: 'img/passwordP.png',
                  isPassword: true,
                  controller: passwordController,
                  errorText: passwordError,
                  keyboardType: isEmailSelected
                      ? TextInputType.visiblePassword
                      : TextInputType.number,
                  inputFormatters: isEmailSelected
                      ? []
                      : [
                    FilteringTextInputFormatter.digitsOnly,
                    LengthLimitingTextInputFormatter(6),
                  ],
                ),

                SizedBox(height: 12 * scale),

                Align(
                  alignment: Alignment.centerRight,
                  child: GestureDetector(
                    onTap: () {
                      if (isEmailSelected) {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (_) => const ForgotPasswordScreen(),
                          ),
                        );
                      } else {
                        // Resend OTP logic
                      }
                    },
                    child: Container(
                      padding: const EdgeInsets.only(bottom: 1.0),
                      decoration: const BoxDecoration(
                        border: Border(
                          bottom: BorderSide(
                            color: Color(0xFFCCCCCC),
                            width: 1.0,
                          ),
                        ),
                      ),
                      child: Text(
                        isEmailSelected ? "Forgot Password?" : "Resend OTP",
                        style: TextStyle(
                          fontFamily: 'Inter',
                          color: const Color(0xFFCCCCCC),
                          fontSize: 17.42 * scale,
                          fontWeight: FontWeight.w500,
                          height: 19.89 / 17.42,
                        ),
                      ),
                    ),
                  ),
                ),

                SizedBox(height: 48 * scale),

                SizedBox(
                  width: double.infinity,
                  height: 55 * scale,
                  child: ElevatedButton(
                    onPressed: _isLoading ? null : onLogin,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color(0xFF0F62FE),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(16 * scale),
                      ),
                    ),
                    child: _isLoading
                        ? const CircularProgressIndicator(
                      color: Colors.white,
                    )
                        : Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      mainAxisSize: MainAxisSize.min,
                      children: const [
                        Text(
                          "Login Securely",
                          style: TextStyle(
                            color: Colors.white,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        SizedBox(width: 8),
                        Icon(
                          Icons.arrow_forward,
                          color: const Color(0xFFFFFFFF),
                          size: 20,
                        ),
                      ],
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
