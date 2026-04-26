import 'package:sql_conn/sql_conn.dart';

import '../config/database_config.dart';
import 'database_service_interface.dart';

/// Native (Android/iOS) implementation of [DatabaseServiceInterface].
///
/// Uses the `sql_conn` package for direct TCP connections to SQL Server.
/// This is the original DatabaseService logic, unchanged.
class DatabaseServiceNative implements DatabaseServiceInterface {
  // ---------------------------------------------------------------------------
  // Singleton
  // ---------------------------------------------------------------------------
  static final DatabaseServiceNative _instance =
      DatabaseServiceNative._internal();
  factory DatabaseServiceNative() => _instance;
  DatabaseServiceNative._internal();

  // ---------------------------------------------------------------------------
  // Connection state
  // ---------------------------------------------------------------------------
  static const String _connectionId = 'mainDB';
  bool _connected = false;

  @override
  bool get isConnected => _connected;

  // ---------------------------------------------------------------------------
  // connect / disconnect
  // ---------------------------------------------------------------------------

  @override
  Future<void> connect() async {
    if (_connected) return;

    try {
      // Append jTDS JDBC properties to the database name so the URL becomes
      // jdbc:jtds:sqlserver://host:port/DB;charset=Cp1252
      // This forces correct Windows-1252 ↔ Unicode conversion for varchar
      // columns, preventing garbled characters (e.g. "Â£" instead of "£").
      final jdbcDatabase =
          '${DatabaseConfig.databaseName};charset=Cp1252';

      final result = await SqlConn.connect(
        connectionId: _connectionId,
        host: DatabaseConfig.serverHost,
        port: int.parse(DatabaseConfig.serverPort),
        database: jdbcDatabase,
        username: DatabaseConfig.username,
        password: DatabaseConfig.password,
      );

      _connected = result;

      if (!_connected) {
        throw DatabaseException('Failed to connect to SQL Server '
            '(${DatabaseConfig.serverHost}:${DatabaseConfig.serverPort})');
      }
    } catch (e) {
      _connected = false;
      if (e is DatabaseException) rethrow;
      throw DatabaseException('Connection error: $e');
    }
  }

  @override
  Future<void> disconnect() async {
    if (_connected) {
      try {
        await SqlConn.disconnect(_connectionId);
      } catch (_) {
        // Ignore disconnect errors
      }
      _connected = false;
    }
  }

  // ---------------------------------------------------------------------------
  // Query helpers
  // ---------------------------------------------------------------------------

  @override
  Future<List<Map<String, dynamic>>> query(
    String sql, [
    Map<String, dynamic>? params,
  ]) async {
    await _ensureConnected();

    final converted = _convertToPositionalParams(sql, params);

    try {
      final result = await SqlConn.read(
        _connectionId,
        converted.sql,
        params: converted.params,
      );
      return result.map((row) {
        final map = <String, dynamic>{};
        row.forEach((k, v) => map[k.toString()] = v);
        return map;
      }).toList();
    } catch (e) {
      if (e.toString().contains('connection') ||
          e.toString().contains('closed')) {
        _connected = false;
      }
      throw DatabaseException('Query error: $e');
    }
  }

  @override
  Future<bool> execute(
    String sql, [
    Map<String, dynamic>? params,
  ]) async {
    await _ensureConnected();

    final converted = _convertToPositionalParams(sql, params);

    try {
      final affectedRows = await SqlConn.write(
        _connectionId,
        converted.sql,
        params: converted.params,
      );
      return affectedRows >= 0;
    } catch (e) {
      if (e.toString().contains('connection') ||
          e.toString().contains('closed')) {
        _connected = false;
      }
      throw DatabaseException('Execute error: $e');
    }
  }

  @override
  Future<dynamic> executeScalar(
    String sql, [
    Map<String, dynamic>? params,
  ]) async {
    final rows = await query(sql, params);

    if (rows.isEmpty) return null;

    final firstRow = rows.first;
    if (firstRow.isEmpty) return null;

    return firstRow.values.first;
  }

  // ---------------------------------------------------------------------------
  // Network connectivity check
  // ---------------------------------------------------------------------------

  @override
  Future<bool> connectedToNetwork() async {
    try {
      if (_connected) return true;
      await connect();
      return _connected;
    } catch (_) {
      return false;
    }
  }

  // ---------------------------------------------------------------------------
  // Private helpers
  // ---------------------------------------------------------------------------

  Future<void> _ensureConnected() async {
    if (!_connected) {
      await connect();
    }
  }

  static _ConvertedQuery _convertToPositionalParams(
    String sql,
    Map<String, dynamic>? params,
  ) {
    if (params == null || params.isEmpty) {
      return _ConvertedQuery(sql, null);
    }

    final normalized = <String, dynamic>{};
    for (final entry in params.entries) {
      final key = entry.key.startsWith('@') ? entry.key : '@${entry.key}';
      normalized[key] = entry.value;
    }

    final sortedKeys = normalized.keys.toList()
      ..sort((a, b) => b.length.compareTo(a.length));

    final paramPositions = <_ParamPosition>[];
    var workingSql = sql;

    for (final key in sortedKeys) {
      int searchFrom = 0;
      while (true) {
        final index = workingSql.indexOf(key, searchFrom);
        if (index == -1) break;
        paramPositions.add(_ParamPosition(index, key.length, key));
        searchFrom = index + key.length;
      }
    }

    paramPositions.sort((a, b) => a.offset.compareTo(b.offset));

    final positionalParams = <Object?>[];
    var result = '';
    var lastEnd = 0;

    for (final pos in paramPositions) {
      result += workingSql.substring(lastEnd, pos.offset);
      result += '?';
      positionalParams.add(normalized[pos.key]);
      lastEnd = pos.offset + pos.length;
    }
    result += workingSql.substring(lastEnd);

    return _ConvertedQuery(
      result,
      positionalParams.isEmpty ? null : positionalParams,
    );
  }
}

// -----------------------------------------------------------------------------
// Helper classes
// -----------------------------------------------------------------------------

class _ConvertedQuery {
  final String sql;
  final List<Object?>? params;
  const _ConvertedQuery(this.sql, this.params);
}

class _ParamPosition {
  final int offset;
  final int length;
  final String key;
  const _ParamPosition(this.offset, this.length, this.key);
}
