// Conditional import: picks native (sql_conn) or web (HTTP proxy) at compile
// time, so the rest of the app just uses `DatabaseService()` everywhere.
import 'database_service_stub.dart'
    if (dart.library.io) 'database_service_io.dart'
    if (dart.library.html) 'database_service_html.dart';

import 'database_service_interface.dart';

// Re-export so callers can still `import 'database_service.dart'` and get
// DatabaseException without changing their imports.
export 'database_service_interface.dart' show DatabaseException;

/// Main MSSQL database service – replaces VB.NET Data_Con module.
///
/// On Android/iOS this uses `sql_conn` for direct TCP connections.
/// On Web this sends HTTP requests to the API proxy server.
///
/// The class is a thin wrapper around [DatabaseServiceInterface] so that
/// all existing code using `DatabaseService()` continues to work unchanged.
class DatabaseService implements DatabaseServiceInterface {
  // ---------------------------------------------------------------------------
  // Singleton
  // ---------------------------------------------------------------------------
  static final DatabaseService _instance = DatabaseService._internal();
  factory DatabaseService() => _instance;
  DatabaseService._internal();

  /// The platform-specific implementation (native or web).
  final DatabaseServiceInterface _delegate = createDatabaseService();

  // ---------------------------------------------------------------------------
  // Delegated methods
  // ---------------------------------------------------------------------------

  @override
  bool get isConnected => _delegate.isConnected;

  @override
  Future<void> connect() => _delegate.connect();

  @override
  Future<void> disconnect() => _delegate.disconnect();

  @override
  Future<List<Map<String, dynamic>>> query(
    String sql, [
    Map<String, dynamic>? params,
  ]) =>
      _delegate.query(sql, params);

  @override
  Future<bool> execute(
    String sql, [
    Map<String, dynamic>? params,
  ]) =>
      _delegate.execute(sql, params);

  @override
  Future<dynamic> executeScalar(
    String sql, [
    Map<String, dynamic>? params,
  ]) =>
      _delegate.executeScalar(sql, params);

  @override
  Future<bool> connectedToNetwork() => _delegate.connectedToNetwork();
}

