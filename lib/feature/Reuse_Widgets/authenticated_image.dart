import 'dart:typed_data';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import '../../core/Theme/app_colors.dart';
import '../../core/Utils/services/token_storage.dart';

/// A reusable widget to fetch and display images that require a Bearer token.
class AuthenticatedImage extends StatefulWidget {
  final String imageUrl;
  final double width;
  final double height;
  final double scale;
  final String fallbackLetter;
  final bool isCircle;
  final Color? backgroundColor;
  final BoxFit fit;

  const AuthenticatedImage({
    super.key,
    required this.imageUrl,
    required this.width,
    required this.height,
    required this.scale,
    required this.fallbackLetter,
    this.isCircle = true,
    this.backgroundColor = Colors.white,
    this.fit = BoxFit.contain,
  });

  @override
  State<AuthenticatedImage> createState() => _AuthenticatedImageState();
}

class _AuthenticatedImageState extends State<AuthenticatedImage> {
  Uint8List? _imageBytes;
  bool _isLoading = true;
  bool _hasError = false;

  @override
  void initState() {
    super.initState();
    _loadImage();
  }

  @override
  void didUpdateWidget(AuthenticatedImage oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.imageUrl != widget.imageUrl) {
      _loadImage();
    }
  }

  Future<void> _loadImage() async {
    if (!mounted) return;
    setState(() {
      _isLoading = true;
      _hasError = false;
    });

    try {
      debugPrint('AUTHENTICATED IMAGE: Loading ${widget.imageUrl}');
      final token = await TokenStorage.getToken();

      if (token == null) {
        debugPrint('AUTHENTICATED IMAGE: No token found');
        if (!mounted) return;
        setState(() {
          _isLoading = false;
          _hasError = true;
        });
        return;
      }

      final response = await http.get(
        Uri.parse(widget.imageUrl),
        headers: {
          'Authorization': 'Bearer $token',
        },
      );

      debugPrint('AUTHENTICATED IMAGE: Status ${response.statusCode}');

      if (!mounted) return;
      if (response.statusCode == 200) {
        setState(() {
          _imageBytes = response.bodyBytes;
          _isLoading = false;
          _hasError = false;
        });
        debugPrint('AUTHENTICATED IMAGE: Loaded successfully');
      } else {
        debugPrint('AUTHENTICATED IMAGE: Failed with status ${response.statusCode}');
        setState(() {
          _isLoading = false;
          _hasError = true;
        });
      }
    } catch (e) {
      debugPrint('AUTHENTICATED IMAGE: Error loading image: $e');
      if (!mounted) return;
      setState(() {
        _isLoading = false;
        _hasError = true;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    Widget content;

    if (_isLoading) {
      content = Center(
        child: SizedBox(
          width: 20 * widget.scale,
          height: 20 * widget.scale,
          child: const CircularProgressIndicator(
            strokeWidth: 2,
            valueColor: AlwaysStoppedAnimation<Color>(AppColors.primaryBlue),
          ),
        ),
      );
    } else if (_hasError || _imageBytes == null) {
      content = Center(
        child: Text(
          widget.fallbackLetter,
          style: TextStyle(
            fontSize: (widget.width * 0.4) * widget.scale,
            fontWeight: FontWeight.bold,
            color: AppColors.primaryBlue,
          ),
        ),
      );
    } else {
      content = Image.memory(
        _imageBytes!,
        width: widget.width,
        height: widget.height,
        fit: widget.fit,
        errorBuilder: (context, error, stackTrace) {
          return Center(
            child: Text(
              widget.fallbackLetter,
              style: TextStyle(
                fontSize: (widget.width * 0.4) * widget.scale,
                fontWeight: FontWeight.bold,
                color: AppColors.primaryBlue,
              ),
            ),
          );
        },
      );
    }

    if (widget.isCircle) {
      return Container(
        width: widget.width,
        height: widget.height,
        decoration: BoxDecoration(
          color: widget.backgroundColor,
          shape: BoxShape.circle,
        ),
        child: ClipOval(child: content),
      );
    } else {
      return Container(
        width: widget.width,
        height: widget.height,
        color: widget.backgroundColor,
        child: content,
      );
    }
  }
}