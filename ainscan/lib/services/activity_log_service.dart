import 'local_database_service.dart';

/// Convenience wrapper for logging user actions to the local SQLite
/// `ActivityLog` table.
///
/// Usage:
/// ```dart
/// await ActivityLogService.log(
///   action: 'DA_PREPARE',
///   empNo: '10097',
///   detail: 'DA: DA001, Pallet: PLT001, Qty: 100',
/// );
/// ```
class ActivityLogService {
  ActivityLogService._();

  static final LocalDatabaseService _db = LocalDatabaseService();

  /// Logs a successful action.
  static Future<void> log({
    required String action,
    String empNo = '',
    String detail = '',
  }) async {
    try {
      await _db.insertActivityLog(
        action: action,
        empNo: empNo,
        detail: detail,
        status: 'SUCCESS',
      );
    } catch (_) {
      // Logging should never crash the app.
    }
  }

  /// Logs a failed action with an error message.
  static Future<void> logError({
    required String action,
    String empNo = '',
    String detail = '',
    String errorMsg = '',
  }) async {
    try {
      await _db.insertActivityLog(
        action: action,
        empNo: empNo,
        detail: detail,
        status: 'FAIL',
        errorMsg: errorMsg,
      );
    } catch (_) {
      // Logging should never crash the app.
    }
  }
}
