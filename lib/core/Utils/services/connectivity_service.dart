import 'dart:async';
import 'dart:ui';
import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:flutter/material.dart';
import 'package:overlay_support/overlay_support.dart';

class ConnectivityService {
  static final Connectivity _connectivity = Connectivity();
  static StreamSubscription<List<ConnectivityResult>>? _subscription;
  static OverlaySupportEntry? _overlayEntry;
  static final List<VoidCallback> _onReconnectedCallbacks = [];
  static bool _isDisconnected = false;

  static void initialize() {
    // Initial check
    _connectivity.checkConnectivity().then(_updateConnectionStatus);

    // Start listening
    _subscription = _connectivity.onConnectivityChanged.listen(_updateConnectionStatus);
  }

  /// Register a callback to be executed when the internet is restored
  static void onReconnected(VoidCallback callback) {
    if (!_onReconnectedCallbacks.contains(callback)) {
      _onReconnectedCallbacks.add(callback);
    }
  }

  /// Remove a registered reconnection callback
  static void removeOnReconnected(VoidCallback callback) {
    _onReconnectedCallbacks.remove(callback);
  }

  static void _updateConnectionStatus(List<ConnectivityResult> results) {
    bool disconnected = results.isEmpty || results.every((r) => r == ConnectivityResult.none);

    if (disconnected && !_isDisconnected) {
      _isDisconnected = true;
      // Small delay to ensure OverlaySupport is ready
      Future.delayed(const Duration(milliseconds: 1000), () {
        if (_isDisconnected) _showDisconnectedOverlay();
      });
    } else if (!disconnected && _isDisconnected) {
      _isDisconnected = false;
      _dismissDisconnectedOverlay();

      // Execute all registered "Data Restore" or "Sync" callbacks
      for (final callback in _onReconnectedCallbacks) {
        try {
          callback();
        } catch (e) {
          print('🔴 ERROR EXECUTING RECONNECTED CALLBACK: $e');
        }
      }
    }
  }

  static void _showDisconnectedOverlay() {
    _overlayEntry?.dismiss();
    _overlayEntry = showOverlay(
          (context, t) {
        return Opacity(
          opacity: t,
          child: Scaffold(
            backgroundColor: Colors.transparent,
            body: Stack(
              children: [
                Positioned.fill(
                  child: BackdropFilter(
                    filter: ImageFilter.blur(sigmaX: 10, sigmaY: 10),
                    child: Container(
                      color: Colors.black.withOpacity(0.5),
                    ),
                  ),
                ),
                Center(
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 48),
                    margin: const EdgeInsets.symmetric(horizontal: 30),
                    decoration: BoxDecoration(
                      color: Colors.white.withOpacity(0.9),
                      borderRadius: BorderRadius.circular(30),
                      boxShadow: [
                        BoxShadow(
                          color: Colors.black.withOpacity(0.2),
                          blurRadius: 20,
                          spreadRadius: 5,
                        ),
                      ],
                    ),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Container(
                          padding: const EdgeInsets.all(20),
                          decoration: BoxDecoration(
                            color: Colors.redAccent.withOpacity(0.1),
                            shape: BoxShape.circle,
                          ),
                          child: const Icon(
                            Icons.wifi_off_rounded,
                            size: 80,
                            color: Colors.redAccent,
                          ),
                        ),
                        const SizedBox(height: 32),
                        const Text(
                          'Connection Lost',
                          textAlign: TextAlign.center,
                          style: TextStyle(
                            fontSize: 26,
                            fontWeight: FontWeight.bold,
                            color: Colors.black87,
                            letterSpacing: -0.5,
                          ),
                        ),
                        const SizedBox(height: 16),
                        const Text(
                          'Your internet connection is currently offline. Please restore it to continue using the application.',
                          textAlign: TextAlign.center,
                          style: TextStyle(
                            fontSize: 16,
                            color: Colors.black54,
                            height: 1.5,
                          ),
                        ),
                        const SizedBox(height: 40),
                        const SizedBox(
                          width: 40,
                          height: 40,
                          child: CircularProgressIndicator(
                            strokeWidth: 3,
                            valueColor: AlwaysStoppedAnimation<Color>(Colors.redAccent),
                          ),
                        ),
                        const SizedBox(height: 12),
                        const Text(
                          'Reconnecting...',
                          style: TextStyle(
                            fontSize: 14,
                            fontWeight: FontWeight.w500,
                            color: Colors.redAccent,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
        );
      },
      duration: Duration.zero,
    );
  }

  static void _dismissDisconnectedOverlay() {
    _overlayEntry?.dismiss();
    _overlayEntry = null;
  }

  static void dispose() {
    _subscription?.cancel();
    _overlayEntry?.dismiss();
  }
}
