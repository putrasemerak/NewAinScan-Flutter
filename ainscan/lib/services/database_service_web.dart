import 'dart:convert';
import 'package:http/http.dart' as http;

import '../config/database_config.dart';
import 'database_service_interface.dart';

/// Web implementation of [DatabaseServiceInterface].
///
/// Instead of connecting directly to SQL Server (impossible from a browser),
/// this sends HTTP requests to the API proxy server running on the developer's
/// PC or on a server accessible to the network.
///
/// Start the proxy with:  dart run bin/api_server.dart
class DatabaseServiceWeb implements DatabaseServiceInterface {
  // ---------------------------------------------------------------------------
  // Singleton
  // ---------------------------------------------------------------------------
  static final DatabaseServiceWeb _instance = DatabaseServiceWeb._internal();
  factory DatabaseServiceWeb() => _instance;
  DatabaseServiceWeb._internal();

  // ---------------------------------------------------------------------------
  // State
  // ---------------------------------------------------------------------------
  bool _connected = false;

  @override
  bool get isConnected => _connected;

  String get _baseUrl => DatabaseConfig.apiUrl;

  // ---------------------------------------------------------------------------
  // connect / disconnect
  // ---------------------------------------------------------------------------

  @override
  Future<void> connect() async {
    if (_connected) return;

    try {
      // Tell the proxy which profile/database to use
      await http.post(
        Uri.parse('$_baseUrl/switch-db'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({'profile': DatabaseConfig.currentProfile.toLowerCase()}),
      );

      final response = await http.post(
        Uri.parse('$_baseUrl/connect'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({}),
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        _connected = data['connected'] == true;
        if (!_connected) {
          throw DatabaseException(
              'Failed to connect via API proxy: ${data['error'] ?? 'unknown'}');
        }
      } else {
        throw DatabaseException(
            'API proxy returned status ${response.statusCode}');
      }
    } catch (e) {
      _connected = false;
      if (e is DatabaseException) rethrow;
      throw DatabaseException(
          'Cannot reach API proxy at $_baseUrl. '
          'Make sure the server is running (dart run bin/api_server.dart). '
          'Error: $e');
    }
  }

  @override
  Future<void> disconnect() async {
    if (_connected) {
      try {
        await http.post(Uri.parse('$_baseUrl/disconnect'));
      } catch (_) {}
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

    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/query'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'sql': sql,
          'params': params,
        }),
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        final rows = data['rows'] as List<dynamic>;
        return rows.map((r) => Map<String, dynamic>.from(r as Map)).toList();
      } else {
        final data = jsonDecode(response.body);
        throw DatabaseException('Query error: ${data['error'] ?? response.body}');
      }
    } catch (e) {
      if (e is DatabaseException) rethrow;
      _connected = false;
      throw DatabaseException('Query error: $e');
    }
  }

  @override
  Future<bool> execute(
    String sql, [
    Map<String, dynamic>? params,
  ]) async {
    await _ensureConnected();

    try {
      final response = await http.post(
        Uri.parse('$_baseUrl/execute'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'sql': sql,
          'params': params,
        }),
      );

      if (response.statusCode == 200) {
        return true;
      } else {
        final data = jsonDecode(response.body);
        throw DatabaseException(
            'Execute error: ${data['error'] ?? response.body}');
      }
    } catch (e) {
      if (e is DatabaseException) rethrow;
      _connected = false;
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
}
