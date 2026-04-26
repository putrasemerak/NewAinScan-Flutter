import 'package:flutter/material.dart';

import 'activity_log_service.dart';

/// Centralised error handler that coordinates user-facing dialogs with
/// activity-log recording.
///
/// Provides a consistent pattern for all screens:
/// ```dart
/// await ErrorHandler.handle(
///   context: context,
///   error: e,
///   action: 'TST_INBOUND',
///   empNo: empNo,
///   detail: 'Pallet: PLT001',
/// );
/// ```
class ErrorHandler {
  ErrorHandler._();

  /// Shows an error dialog and logs the error to the activity log.
  ///
  /// If [logToActivity] is false, the error is shown to the user but not
  /// persisted to the activity log (for non-critical / validation errors).
  static Future<void> handle({
    required BuildContext context,
    required Object error,
    required String action,
    String empNo = '',
    String detail = '',
    bool logToActivity = true,
  }) async {
    final message = error.toString().replaceFirst('Exception: ', '');

    if (logToActivity) {
      await ActivityLogService.logError(
        action: action,
        empNo: empNo,
        detail: detail,
        errorMsg: message,
      );
    }

    if (!context.mounted) return;
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Error'),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }

  /// Shows a warning/validation dialog (no activity log recording).
  static Future<void> showValidation({
    required BuildContext context,
    required String message,
  }) async {
    if (!context.mounted) return;
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Error'),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }
}
