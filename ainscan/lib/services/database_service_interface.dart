/// Abstract interface for database operations.
///
/// This allows platform-specific implementations:
/// - Native (Android/iOS): uses `sql_conn` for direct SQL Server connections
/// - Web: uses HTTP calls to the API proxy server
abstract class DatabaseServiceInterface {
  /// Whether we believe the connection is active.
  bool get isConnected;

  /// Opens a connection to SQL Server.
  Future<void> connect();

  /// Closes the current connection.
  Future<void> disconnect();

  /// Executes a SELECT statement and returns the result rows.
  Future<List<Map<String, dynamic>>> query(
    String sql, [
    Map<String, dynamic>? params,
  ]);

  /// Executes an INSERT / UPDATE / DELETE statement.
  Future<bool> execute(
    String sql, [
    Map<String, dynamic>? params,
  ]);

  /// Executes a query and returns the first column of the first row.
  Future<dynamic> executeScalar(
    String sql, [
    Map<String, dynamic>? params,
  ]);

  /// Quick connectivity probe.
  Future<bool> connectedToNetwork();
}

/// Exception thrown by database services on connection or query errors.
class DatabaseException implements Exception {
  final String message;
  const DatabaseException(this.message);

  @override
  String toString() => 'DatabaseException: $message';
}
