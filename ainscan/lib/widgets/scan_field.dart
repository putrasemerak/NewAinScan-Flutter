import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';

import '../services/keyboard_service.dart';
import '../services/hardware_scanner_service.dart';

/// A text input field with a barcode scan button.
///
/// Replaces the VB.NET text fields where barcodes were entered via
/// hardware scanner. In Flutter, the field accepts both keyboard input
/// and camera-based scanning via the suffix icon button. Text is
/// automatically converted to uppercase on submit, matching the VB.NET
/// UCase() behavior used throughout the original application.
///
/// When the global [KeyboardService] is in scanner mode (default), the
/// soft keyboard is hidden programmatically on focus while keeping the
/// input connection alive. This allows hardware barcode scanners
/// (keyboard-wedge mode) to type into the field.
/// Tapping the floating keyboard icon in [AppScaffold] switches to
/// keyboard mode, allowing manual text entry with the soft keyboard.
class ScanField extends StatefulWidget {
  /// Controller for the text field.
  final TextEditingController controller;

  /// Label text displayed above/inside the field.
  final String label;

  /// Callback invoked when the user submits the field (e.g., presses Done).
  /// The value passed is already converted to uppercase.
  final Function(String)? onSubmitted;

  /// Callback invoked when the scan icon button is pressed.
  /// Typically opens a barcode/QR scanner.
  final VoidCallback? onScanPressed;

  /// Whether the field is enabled for input. Defaults to true.
  final bool enabled;

  /// Whether the field is read-only. Defaults to false.
  final bool readOnly;

  /// Whether the field should auto-focus when the widget is built.
  final bool autofocus;

  /// Optional focus node for programmatic focus management.
  final FocusNode? focusNode;

  /// Override keyboard type regardless of [KeyboardService] state.
  /// When null, the field respects the global toggle.
  final TextInputType? keyboardType;

  /// Whether the text field should have a filled background.
  final bool filled;

  /// Background fill colour when [filled] is true.
  final Color? fillColor;

  /// Optional text style override for the input text.
  final TextStyle? style;

  /// Optional text style for the label.
  final TextStyle? labelStyle;

  /// Optional content padding override.
  final EdgeInsetsGeometry? contentPadding;

  /// Whether the field should be dense.
  final bool isDense;

  const ScanField({
    super.key,
    required this.controller,
    required this.label,
    this.onSubmitted,
    this.onScanPressed,
    this.enabled = true,
    this.readOnly = false,
    this.autofocus = false,
    this.focusNode,
    this.keyboardType,
    this.filled = false,
    this.fillColor,
    this.style,
    this.labelStyle,
    this.contentPadding,
    this.isDense = false,
  });

  @override
  State<ScanField> createState() => _ScanFieldState();
}

class _ScanFieldState extends State<ScanField> {
  FocusNode? _internalFocusNode;
  FocusNode get _effectiveFocusNode =>
      widget.focusNode ?? (_internalFocusNode ??= FocusNode());

  // ── Keyboard-wedge scanner detection ──────────────────────
  // Some devices send barcodes as rapid keystrokes without a trailing
  // ENTER. We detect bursts of fast input (< _wedgeCharGapMs between
  // chars) and auto-submit once the burst ends (quiet for _wedgeIdleMs).
  static const _wedgeCharGapMs = 80;  // max ms between chars in a burst
  static const _wedgeIdleMs = 150;    // quiet time before auto-submit
  static const _wedgeMinLength = 3;   // minimum chars to count as barcode
  Timer? _wedgeTimer;
  DateTime _lastCharTime = DateTime(0);
  int _burstLength = 0; // count of chars received in current burst

  @override
  void initState() {
    super.initState();
    _effectiveFocusNode.addListener(_onFocusChange);
    // Listen for keyboard mode changes so we can instantly show/hide
    // the soft keyboard when the toggle is pressed while this field
    // already has focus.
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        context.read<KeyboardService>().addListener(_onKeyboardModeChanged);
      }
    });
  }

  @override
  void didUpdateWidget(ScanField oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (oldWidget.focusNode != widget.focusNode) {
      (oldWidget.focusNode ?? _internalFocusNode)?.removeListener(_onFocusChange);
      _effectiveFocusNode.addListener(_onFocusChange);
    }
  }

  @override
  void dispose() {
    _wedgeTimer?.cancel();
    _effectiveFocusNode.removeListener(_onFocusChange);
    // Safe to read here — dispose is called while still in the tree
    try {
      context.read<KeyboardService>().removeListener(_onKeyboardModeChanged);
    } catch (_) {}
    _internalFocusNode?.dispose();
    super.dispose();
  }

  /// Called when KeyboardService toggles. If this field currently has
  /// focus, immediately show or hide the soft keyboard.
  void _onKeyboardModeChanged() {
    if (!mounted || !_effectiveFocusNode.hasFocus) return;
    final kbService = context.read<KeyboardService>();
    if (kbService.keyboardEnabled) {
      // Unfocus and refocus to force Flutter to re-establish the input
      // connection and raise the soft keyboard immediately.
      _effectiveFocusNode.unfocus();
      Future.delayed(const Duration(milliseconds: 50), () {
        if (mounted) _effectiveFocusNode.requestFocus();
      });
    } else {
      SystemChannels.textInput.invokeMethod('TextInput.hide');
    }
  }

  /// When the field gains focus in scanner mode, hide the soft keyboard
  /// while keeping the input connection alive for hardware scanners.
  /// Also registers as the active target for intent-based scanner input.
  void _onFocusChange() {
    if (_effectiveFocusNode.hasFocus) {
      // Register this field as the active hardware scanner target
      HardwareScannerService.setActiveCallback(_onHardwareScan);

      final kbService = context.read<KeyboardService>();
      if (!kbService.keyboardEnabled) {
        // Small delay to let the input connection establish first,
        // then dismiss the soft keyboard.
        Future.delayed(const Duration(milliseconds: 50), () {
          SystemChannels.textInput.invokeMethod('TextInput.hide');
        });
      }
    } else {
      // Unregister when losing focus
      HardwareScannerService.setActiveCallback(null);
    }
  }

  /// Called when the hardware scanner broadcasts a barcode via intent.
  /// Sets the field text and triggers the same submit flow as manual entry.
  void _onHardwareScan(String barcode) {
    if (!mounted) return;
    final upperValue = barcode.trim().toUpperCase();
    widget.controller.text = upperValue;
    widget.controller.selection = TextSelection.fromPosition(
      TextPosition(offset: upperValue.length),
    );
    widget.onSubmitted?.call(upperValue);
  }

  /// Detects keyboard-wedge scanner input by monitoring character timing.
  /// Scanners fire characters < 80ms apart; humans type > 100ms apart.
  /// After a burst of fast chars followed by quiet, auto-submit.
  void _onTextChanged(String value) {
    final now = DateTime.now();
    final gap = now.difference(_lastCharTime).inMilliseconds;
    _lastCharTime = now;

    if (gap < _wedgeCharGapMs) {
      _burstLength++;
    } else {
      // Gap too long — start a new burst from this character
      _burstLength = 1;
    }

    // Cancel any pending submit timer and start a new one.
    // When input stops for _wedgeIdleMs, check if it was a burst.
    _wedgeTimer?.cancel();
    _wedgeTimer = Timer(const Duration(milliseconds: _wedgeIdleMs), () {
      if (!mounted) return;
      final text = widget.controller.text.trim();
      if (_burstLength >= _wedgeMinLength && text.length >= _wedgeMinLength) {
        // Looks like a scanner burst — auto-submit
        _handleSubmitted(text);
      }
      _burstLength = 0;
    });
  }

  void _handleSubmitted(String value) {
    // Trim whitespace and convert to uppercase, matching VB.NET UCase(Trim()) behavior
    final upperValue = value.trim().toUpperCase();
    widget.controller.text = upperValue;
    // Move cursor to end after setting text
    widget.controller.selection = TextSelection.fromPosition(
      TextPosition(offset: upperValue.length),
    );
    widget.onSubmitted?.call(upperValue);
  }

  @override
  Widget build(BuildContext context) {
    // Always use TextInputType.text to keep the input connection alive
    // for hardware barcode scanners. The soft keyboard is hidden via
    // _onFocusChange when in scanner mode.
    final effectiveKeyboardType = widget.keyboardType ?? TextInputType.text;

    return TextField(
      controller: widget.controller,
      focusNode: _effectiveFocusNode,
      enabled: widget.enabled,
      readOnly: widget.readOnly,
      autofocus: widget.autofocus,
      keyboardType: effectiveKeyboardType,
      textInputAction: TextInputAction.done,
      textCapitalization: TextCapitalization.characters,
      style: widget.style,
      onChanged: _onTextChanged,
      onSubmitted: _handleSubmitted,
      decoration: InputDecoration(
        labelText: widget.label,
        labelStyle: widget.labelStyle,
        border: const OutlineInputBorder(),
        filled: widget.filled,
        fillColor: widget.fillColor,
        isDense: widget.isDense,
        suffixIcon: widget.onScanPressed != null
            ? IconButton(
                icon: const Icon(Icons.qr_code_scanner),
                onPressed: widget.enabled ? widget.onScanPressed : null,
                tooltip: 'Scan barcode',
              )
            : null,
        contentPadding: widget.contentPadding ?? const EdgeInsets.symmetric(
          horizontal: 12,
          vertical: 14,
        ),
      ),
    );
  }
}
