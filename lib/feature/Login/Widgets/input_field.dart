import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

class InputField extends StatefulWidget {
  final String hint;
  final IconData icon;
  final bool isPassword;
  final TextEditingController controller;
  final String? errorText;
  final TextInputType keyboardType;
  final List<TextInputFormatter>? inputFormatters;
  final String? iconPath;

  const InputField({
    super.key,
    required this.hint,
    required this.icon,
    required this.controller,
    this.isPassword = false,
    this.errorText,
    this.keyboardType = TextInputType.text,
    this.inputFormatters,
    this.iconPath,
  });

  @override
  State<InputField> createState() => _InputFieldState();
}

class _InputFieldState extends State<InputField> {
  bool _obscureText = true;

  bool get _showVisibilityToggle => widget.isPassword;

  @override
  Widget build(BuildContext context) {
    return TextField(

      controller: widget.controller,
      keyboardType: widget.keyboardType,
      inputFormatters: widget.inputFormatters,
      obscureText: widget.isPassword ? _obscureText : false,
      decoration: InputDecoration(
        filled: true,
        fillColor: const Color(0xFFF8FAFC),

        hintText: widget.hint,
        hintStyle: const TextStyle(color: Colors.grey),
        errorText: widget.errorText,
        prefixIcon: widget.iconPath != null
            ? Padding(
          padding: const EdgeInsets.all(12.0),
          child: Image.asset(
            widget.iconPath!,
            height: 24,
            width: 24,
          ),
        )
            : Icon(widget.icon, color: Colors.grey),
        suffixIcon: _showVisibilityToggle
            ? IconButton(
          icon: Icon(
            _obscureText ? Icons.visibility : Icons.visibility_off,
            color: Colors.grey.shade500,
          ),
          onPressed: () {
            setState(() {
              _obscureText = !_obscureText;
            });
          },
        )
            : null,
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(16),
          borderSide: BorderSide(color: Colors.grey.shade300),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(16),
          borderSide: const BorderSide(color: Color(0xff3563F3)),
        ),
      ),
    );
  }
}
