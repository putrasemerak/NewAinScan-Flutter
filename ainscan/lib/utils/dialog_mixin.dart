import 'package:flutter/material.dart';

/// Mixin that provides common dialog helpers for State classes.
///
/// Provides [showErrorDialog] and [showMessageDialog] to eliminate
/// duplicate `_showError` / `_showMessage` methods across screens.
mixin DialogMixin<T extends StatefulWidget> on State<T> {
  /// Shows an error dialog with the given [message].
  ///
  /// Title defaults to `'Error'` but can be overridden.
  void showErrorDialog(String message, {String title = 'Error'}) {
    if (!mounted) return;
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(title),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }

  /// Shows an informational dialog with the given [message].
  void showMessageDialog(String message, {String title = 'Maklumat'}) {
    if (!mounted) return;
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(title),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }
}
