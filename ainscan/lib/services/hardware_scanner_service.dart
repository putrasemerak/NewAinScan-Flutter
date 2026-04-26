import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/services.dart';

/// Receives barcode data from the device's built-in hardware scanner
/// via Android broadcast intents (EventChannel).
///
/// Industrial Android devices (Honeywell/Intermec, Zebra, Urovo, etc.)
/// broadcast scanned barcode data as intents. The native side
/// (MainActivity.kt) registers a BroadcastReceiver for common scanner
/// actions and forwards the barcode string through this channel.
///
/// Usage:
///   - Call [init] once at app startup.
///   - ScanField registers/unregisters [onScan] based on focus.
///   - When a scan arrives, the active callback receives the barcode.
class HardwareScannerService {
  static const _channel = EventChannel('com.example.ainscan/scanner');
  static StreamSubscription<dynamic>? _subscription;
  static void Function(String)? _activeCallback;

  /// Start listening for hardware scanner broadcasts.
  /// Safe to call multiple times — only the first call starts the stream.
  static void init() {
    if (_subscription != null) return;
    _subscription = _channel.receiveBroadcastStream().listen(
      (event) {
        if (event is String && event.isNotEmpty) {
          final barcode = event.trim();
          debugPrint('[HW Scanner] Received: $barcode');
          _activeCallback?.call(barcode);
        }
      },
      onError: (error) {
        debugPrint('[HW Scanner] Stream error: $error');
      },
    );
  }

  /// Register the currently focused field's callback.
  /// Pass null to unregister.
  static void setActiveCallback(void Function(String)? callback) {
    _activeCallback = callback;
  }

  /// Clean up the stream (call on app dispose if needed).
  static void dispose() {
    _subscription?.cancel();
    _subscription = null;
    _activeCallback = null;
  }
}
