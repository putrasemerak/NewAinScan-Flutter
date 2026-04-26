import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

/// Manages virtual keyboard visibility across the app.
///
/// In scanner-based workflows the virtual keyboard should stay hidden
/// by default so the physical scanner and numeric keypad can be used
/// without the soft keyboard blocking the UI.
///
///   - **Scanner mode** (default): soft keyboard is hidden on focus
///     via `SystemChannels.textInput.hide`. Physical keyboard and
///     scanner broadcasts still work normally.
///   - **Keyboard mode**: soft keyboard appears on focus, allowing
///     manual text entry (e.g., alphabetic input).
///
/// A toggle button rendered by [AppScaffold] lets the user switch
/// modes at any time. The state is app-wide and persists across screens.
class KeyboardService extends ChangeNotifier {
  bool _keyboardEnabled = false;

  /// Whether the virtual keyboard is currently enabled.
  bool get keyboardEnabled => _keyboardEnabled;

  /// The [TextInputType] that [ScanField] should use.
  ///
  /// Returns [TextInputType.none] when keyboard is disabled (scanner mode)
  /// which prevents the soft keyboard from appearing while still allowing
  /// focus and cursor display.
  TextInputType get inputType =>
      _keyboardEnabled ? TextInputType.text : TextInputType.none;

  /// Toggle keyboard on/off.
  /// When enabling, immediately shows the soft keyboard for the
  /// currently focused field. When disabling, immediately hides it.
  void toggle() {
    _keyboardEnabled = !_keyboardEnabled;
    if (_keyboardEnabled) {
      // Immediately show the keyboard for the current input connection
      SystemChannels.textInput.invokeMethod('TextInput.show');
    } else {
      // Immediately dismiss the keyboard when switching to scanner mode
      SystemChannels.textInput.invokeMethod('TextInput.hide');
    }
    notifyListeners();
  }

  /// Explicitly enable the keyboard.
  void enable() {
    if (!_keyboardEnabled) {
      _keyboardEnabled = true;
      notifyListeners();
    }
  }

  /// Explicitly disable the keyboard (scanner mode).
  void disable() {
    if (_keyboardEnabled) {
      _keyboardEnabled = false;
      SystemChannels.textInput.invokeMethod('TextInput.hide');
      notifyListeners();
    }
  }
}
