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
import '../Tenant/controller/tenant_controller.dart';
import '../Reuse_Widgets/authenticated_image.dart';

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

  // Mobile OTP flow state
  bool _otpSent = false;
  int _resendCountdown = 0;

  void _switchMode(bool isEmail) {
    if (isEmailSelected == isEmail) return;
    setState(() {
      isEmailSelected = isEmail;
      fieldController.clear();
      passwordController.clear();
      fieldError = null;
      passwordError = null;
      _otpSent = false;
      _resendCountdown = 0;
    });
  }

  void _startCountdown(int seconds) {
    setState(() => _resendCountdown = seconds);
    Future.doWhile(() async {
      await Future.delayed(const Duration(seconds: 1));
      if (!mounted) return false;
      setState(() => _resendCountdown--);
      return _resendCountdown > 0;
    });
  }

  Future<void> _sendOtp() async {
    final mobile = fieldController.text.trim();
    if (mobile.isEmpty || mobile.length < 10) {
      setState(() => fieldError = 'Enter a valid 10-digit mobile number');
      return;
    }
    setState(() {
      fieldError = null;
      _isLoading = true;
    });
    try {
      final seconds = await _loginController.sendOtp(mobile: mobile);
      setState(() {
        _otpSent = true;
        passwordController.clear();
        passwordError = null;
      });
      _startCountdown(seconds);
    } catch (e) {
      setState(() {
        fieldError = e.toString().replaceAll('Exception:', '').trim();
      });
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> onLogin() async {
    // ── EMAIL path ──
    if (isEmailSelected) {
      final result = Validator.validateLogin(
        isEmail: true,
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
        await _loginController.loginWithEmail(
          email: fieldController.text.trim(),
          password: passwordController.text.trim(),
        );
        if (!mounted) return;
        context.read<ProfileController>().refreshProfile();
        context.read<TenantController>().fetchCompanyLogo();
        Navigator.pushReplacement(
          context,
          MaterialPageRoute(builder: (_) => const MainNavigationScreen()),
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
        if (msg.contains("password")) {
          newPasswordError =
              e.toString().replaceAll('Exception:', '').trim();
        } else if (msg.contains("user") ||
            msg.contains("email") ||
            msg.contains("account")) {
          newFieldError =
              e.toString().replaceAll('Exception:', '').trim();
        } else {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(
                  e.toString().replaceAll('Exception:', '').trim()),
            ),
          );
        }
        setState(() {
          fieldError = newFieldError;
          passwordError = newPasswordError;
        });
      } finally {
        if (mounted) setState(() => _isLoading = false);
      }
      return;
    }

    // ── MOBILE OTP path ──
    if (!_otpSent) {
      await _sendOtp();
      return;
    }

    final otp = passwordController.text.trim();
    if (otp.isEmpty || otp.length < 4) {
      setState(
              () => passwordError = 'Enter the OTP received on your mobile');
      return;
    }
    setState(() {
      passwordError = null;
      _isLoading = true;
    });
    try {
      await _loginController.verifyOtp(
        mobile: fieldController.text.trim(),
        otp: otp,
      );
      if (!mounted) return;
      context.read<ProfileController>().refreshProfile();
      context.read<TenantController>().fetchCompanyLogo();
      Navigator.pushReplacement(
        context,
        MaterialPageRoute(builder: (_) => const MainNavigationScreen()),
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
      setState(() {
        passwordError =
            e.toString().replaceAll('Exception:', '').trim();
      });
    } finally {
      if (mounted) setState(() => _isLoading = false);
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
                // ── Moved logos here to make them scrollable ────
                Row(
                  mainAxisAlignment: MainAxisAlignment.end,
                  children: [
                    Consumer<TenantController>(
                      builder: (context, tenantController, child) {
                        final logoUrl = tenantController.companyLogoUrl;
                        if (logoUrl == null || logoUrl.isEmpty) {
                          return const SizedBox.shrink();
                        }
                        return AuthenticatedImage(
                          imageUrl: logoUrl,
                          width: 65 * scale,
                          height: 22 * scale,
                          scale: scale,
                          isCircle: false,
                          fit: BoxFit.contain,
                          fallbackLetter: '',
                          backgroundColor: Colors.transparent,
                          showLoader: false,
                        );
                      },
                    ),
                    Image.asset(
                      'img/altrozhrm_logo.png',
                      width: 65 * scale,
                      height: 22 * scale,
                      fit: BoxFit.contain,
                    ),
                  ],
                ),
                SizedBox(height: 12 * scale), // space after logos
                // ───────────────────────────────────────────────
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
                  isEmailSelected ? "WORK EMAIL / USER ID" : "MOBILE NUMBER",
                  style: const TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.w600,
                    color: Color(0xFF94A3B8),
                    letterSpacing: 0.6,
                  ),
                ),

                const SizedBox(height: 6),

                InputField(
                  hint: isEmailSelected
                      ? "Email or Username"
                      : "Mobile Number",
                  icon: isEmailSelected
                      ? Icons.email_outlined
                      : Icons.phone_outlined,
                  iconPath: isEmailSelected ? 'img/workMail.png' : null,
                  controller: fieldController,
                  errorText: fieldError,
                  readOnly: !isEmailSelected && _otpSent,
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

                // OTP field – only show after OTP is sent
                if (!isEmailSelected && _otpSent) ...[
                  SizedBox(height: 15 * scale),
                  const Text(
                    "OTP",
                    style: TextStyle(
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                      color: Color(0xFF94A3B8),
                      letterSpacing: 0.6,
                    ),
                  ),
                  const SizedBox(height: 6),
                  InputField(
                    hint: "Enter OTP",
                    icon: Icons.lock_outline,
                    iconPath: 'img/passwordP.png',
                    isPassword: true,
                    controller: passwordController,
                    errorText: passwordError,
                    keyboardType: TextInputType.number,
                    inputFormatters: [
                      FilteringTextInputFormatter.digitsOnly,
                      LengthLimitingTextInputFormatter(6),
                    ],
                  ),
                ],

                // Email password field
                if (isEmailSelected) ...[
                  SizedBox(height: 15 * scale),
                  const Text(
                    "PASSWORD",
                    style: TextStyle(
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                      color: Color(0xFF94A3B8),
                      letterSpacing: 0.6,
                    ),
                  ),
                  const SizedBox(height: 6),
                  InputField(
                    hint: "Enter Password",
                    icon: Icons.lock_outline,
                    iconPath: 'img/passwordP.png',
                    isPassword: true,
                    controller: passwordController,
                    errorText: passwordError,
                    keyboardType: TextInputType.visiblePassword,
                  ),
                ],

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
                      } else if (_otpSent) {
                        if (_resendCountdown <= 0) {
                          _sendOtp();
                        }
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
                        isEmailSelected
                            ? "Forgot Password?"
                            : _otpSent
                            ? (_resendCountdown > 0
                            ? "Resend OTP in ${_resendCountdown}s"
                            : "Resend OTP")
                            : "",
                        style: TextStyle(
                          fontFamily: 'Inter',
                          color: (_otpSent && _resendCountdown <= 0)
                              ? const Color(0xFF0F62FE)
                              : const Color(0xFFCCCCCC),
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
                      children: [
                        Text(
                          isEmailSelected
                              ? "Login Securely"
                              : _otpSent
                              ? "Verify OTP"
                              : "Send OTP",
                          style: const TextStyle(
                            color: Colors.white,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(width: 8),
                        const Icon(
                          Icons.arrow_forward,
                          color: Colors.white,
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