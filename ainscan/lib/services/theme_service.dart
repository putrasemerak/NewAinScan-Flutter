import 'package:flutter/material.dart';

/// Manages app theme mode (light/dark) with ChangeNotifier for Provider.
class ThemeService extends ChangeNotifier {
  ThemeMode _themeMode = ThemeMode.dark; // Default to dark

  ThemeMode get themeMode => _themeMode;

  bool get isDark => _themeMode == ThemeMode.dark;

  void toggleTheme() {
    _themeMode =
        _themeMode == ThemeMode.dark ? ThemeMode.light : ThemeMode.dark;
    notifyListeners();
  }

  void setDark() {
    _themeMode = ThemeMode.dark;
    notifyListeners();
  }

  void setLight() {
    _themeMode = ThemeMode.light;
    notifyListeners();
  }
}
