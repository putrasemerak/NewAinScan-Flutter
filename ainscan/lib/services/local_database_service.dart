import 'package:flutter/foundation.dart' show debugPrint, kIsWeb;
import 'package:sqflite/sqflite.dart';
import 'package:path/path.dart';

import '../config/database_config.dart';

/// Local SQLite database service – replaces SQL Server CE (appDatabase.sdf).
///
/// Provides offline storage for DA confirmation data and local logging
/// (Receiving, RackIn) using the `sqflite` package.
class LocalDatabaseService {
  // ---------------------------------------------------------------------------
  // Singleton
  // ---------------------------------------------------------------------------
  static final LocalDatabaseService _instance =
      LocalDatabaseService._internal();
  factory LocalDatabaseService() => _instance;
  LocalDatabaseService._internal();

  Database? _db;

  /// Whether the local database is available. On web, sqflite WASM can be
  /// unreliable, so callers should check this before using [database].
  bool get isAvailable => _db != null;

  /// The opened database instance. Throws if [initialize] has not been called.
  Database get database {
    if (_db == null) {
      throw StateError(
        'LocalDatabaseService has not been initialised. '
        'Call initialize() first.',
      );
    }
    return _db!;
  }

  // ---------------------------------------------------------------------------
  // Initialisation
  // ---------------------------------------------------------------------------

  /// Creates (or opens) the local SQLite database and ensures all required
  /// tables exist.
  Future<void> initialize() async {
    if (_db != null) return;

    // On web, sqflite_common_ffi_web is unreliable — skip local DB entirely.
    // Local DB features (DA_Confirm tracking, activity log) are not critical
    // and only apply to the Android scanner device.
    if (kIsWeb) {
      debugPrint('Local DB: skipped on web platform');
      return;
    }

    final dbPath = await getDatabasesPath();
    final path = join(dbPath, DatabaseConfig.localDatabaseName);

    _db = await openDatabase(
      path,
      version: 2,
      onCreate: _onCreate,
      onUpgrade: _onUpgrade,
    );

    // Auto-purge activity log entries older than 30 days on startup
    await purgeActivityLog();
  }

  Future<void> _onCreate(Database db, int version) async {
    await db.execute('''
      CREATE TABLE IF NOT EXISTS DA_Confirm (
        id       INTEGER PRIMARY KEY AUTOINCREMENT,
        DANo     TEXT,
        Batch    TEXT,
        Run      TEXT,
        Qty      REAL,
        PCode    TEXT,
        Prepared   REAL DEFAULT 0,
        PreparedBy TEXT DEFAULT '',
        Status   TEXT DEFAULT '',
        AddDate  TEXT,
        AddTime  TEXT
      )
    ''');

    await db.execute('''
      CREATE TABLE IF NOT EXISTS Receiving (
        id       INTEGER PRIMARY KEY AUTOINCREMENT,
        PalletNo TEXT,
        PCode    TEXT,
        PName    TEXT,
        Batch    TEXT,
        Run      TEXT,
        PGroup   TEXT,
        Qty      REAL,
        Unit     TEXT,
        User     TEXT,
        Date     TEXT,
        Time     TEXT
      )
    ''');

    await db.execute('''
      CREATE TABLE IF NOT EXISTS RackIn (
        id       INTEGER PRIMARY KEY AUTOINCREMENT,
        PalletNo TEXT,
        RackNo   TEXT,
        PCode    TEXT,
        PName    TEXT,
        Batch    TEXT,
        Run      TEXT,
        PGroup   TEXT,
        Qty      REAL,
        Unit     TEXT,
        User     TEXT,
        Date     TEXT,
        Time     TEXT
      )
    ''');

    await _createActivityLogTable(db);
  }

  Future<void> _createActivityLogTable(Database db) async {
    await db.execute('''
      CREATE TABLE IF NOT EXISTS ActivityLog (
        id        INTEGER PRIMARY KEY AUTOINCREMENT,
        timestamp TEXT NOT NULL,
        empNo     TEXT NOT NULL DEFAULT '',
        action    TEXT NOT NULL,
        detail    TEXT DEFAULT '',
        status    TEXT NOT NULL DEFAULT 'SUCCESS',
        errorMsg  TEXT DEFAULT ''
      )
    ''');
  }

  Future<void> _onUpgrade(Database db, int oldVersion, int newVersion) async {
    if (oldVersion < 2) {
      await _createActivityLogTable(db);
    }
  }

  // ---------------------------------------------------------------------------
  // DA_Confirm operations
  // ---------------------------------------------------------------------------

  /// Inserts a row into `DA_Confirm`.
  ///
  /// Expected keys: `DANo`, `Batch`, `Run`, `Qty`, `PCode`, `Prepared`,
  /// `PreparedBy`, `Status`, `AddDate`, `AddTime`.
  Future<int> insertDAConfirm(Map<String, dynamic> data) async {
    if (!isAvailable) return 0;
    return await database.insert(
      'DA_Confirm',
      data,
      conflictAlgorithm: ConflictAlgorithm.replace,
    );
  }

  /// Updates the prepared quantity and preparer for a specific DA line.
  Future<int> updateDAConfirmPrepared(
    String daNo,
    String batch,
    String run,
    String pCode,
    double prepared,
    String preparedBy,
  ) async {
    if (!isAvailable) return 0;
    return await database.update(
      'DA_Confirm',
      {
        'Prepared': prepared,
        'PreparedBy': preparedBy,
      },
      where: 'DANo = ? AND Batch = ? AND Run = ? AND PCode = ?',
      whereArgs: [daNo, batch, run, pCode],
    );
  }

  /// Queries `DA_Confirm` rows matching the given composite key.
  Future<List<Map<String, dynamic>>> getDAConfirm(
    String daNo,
    String batch,
    String run,
    String pCode,
  ) async {
    if (!isAvailable) return [];
    return await database.query(
      'DA_Confirm',
      where: 'DANo = ? AND Batch = ? AND Run = ? AND PCode = ?',
      whereArgs: [daNo, batch, run, pCode],
    );
  }

  /// Returns the sum of the `Prepared` column for a specific DA line.
  Future<double> getSumPrepared(
    String daNo,
    String batch,
    String run,
    String pCode,
  ) async {
    if (!isAvailable) return 0.0;
    final result = await database.rawQuery(
      '''
      SELECT COALESCE(SUM(Prepared), 0) AS total
      FROM DA_Confirm
      WHERE DANo = ? AND Batch = ? AND Run = ? AND PCode = ?
      ''',
      [daNo, batch, run, pCode],
    );

    if (result.isEmpty) return 0.0;

    final total = result.first['total'];
    if (total == null) return 0.0;
    return (total as num).toDouble();
  }

  // ---------------------------------------------------------------------------
  // Receiving log operations
  // ---------------------------------------------------------------------------

  /// Inserts a row into the `Receiving` log table.
  Future<int> insertReceivingLog(Map<String, dynamic> data) async {
    if (!isAvailable) return 0;
    return await database.insert('Receiving', data);
  }

  /// Returns all rows from the `Receiving` log, ordered newest-first.
  Future<List<Map<String, dynamic>>> getReceivingLogs() async {
    if (!isAvailable) return [];
    return await database.query('Receiving', orderBy: 'id DESC');
  }

  /// Returns `Receiving` rows for a specific date (yyyy-MM-dd), newest-first.
  Future<List<Map<String, dynamic>>> getReceivingLogsByDate(String date) async {
    if (!isAvailable) return [];
    return await database.query(
      'Receiving',
      where: 'Date = ?',
      whereArgs: [date],
      orderBy: 'id DESC',
    );
  }

  // ---------------------------------------------------------------------------
  // RackIn log operations
  // ---------------------------------------------------------------------------

  /// Inserts a row into the `RackIn` log table.
  Future<int> insertRackInLog(Map<String, dynamic> data) async {
    if (!isAvailable) return 0;
    return await database.insert('RackIn', data);
  }

  /// Returns all rows from the `RackIn` log, ordered newest-first.
  Future<List<Map<String, dynamic>>> getRackInLogs() async {
    if (!isAvailable) return [];
    return await database.query('RackIn', orderBy: 'id DESC');
  }

  /// Returns `RackIn` rows for a specific date (yyyy-MM-dd), newest-first.
  Future<List<Map<String, dynamic>>> getRackInLogsByDate(String date) async {
    if (!isAvailable) return [];
    return await database.query(
      'RackIn',
      where: 'Date = ?',
      whereArgs: [date],
      orderBy: 'id DESC',
    );
  }

  // ---------------------------------------------------------------------------
  // ActivityLog operations
  // ---------------------------------------------------------------------------

  /// Inserts a row into the `ActivityLog` table.
  Future<int> insertActivityLog({
    required String action,
    String empNo = '',
    String detail = '',
    String status = 'SUCCESS',
    String errorMsg = '',
  }) async {
    if (!isAvailable) return 0;
    return await database.insert('ActivityLog', {
      'timestamp': DateTime.now().toIso8601String(),
      'empNo': empNo,
      'action': action,
      'detail': detail,
      'status': status,
      'errorMsg': errorMsg,
    });
  }

  /// Returns the most recent [limit] activity log rows, newest first.
  Future<List<Map<String, dynamic>>> getActivityLogs({int limit = 200}) async {
    if (!isAvailable) return [];
    return await database.query(
      'ActivityLog',
      orderBy: 'id DESC',
      limit: limit,
    );
  }

  /// Returns the total number of activity log rows.
  Future<int> getActivityLogCount() async {
    if (!isAvailable) return 0;
    final result = await database.rawQuery(
      'SELECT COUNT(*) AS cnt FROM ActivityLog',
    );
    return (result.first['cnt'] as int?) ?? 0;
  }

  /// Deletes all activity log rows.
  Future<int> clearActivityLog() async {
    if (!isAvailable) return 0;
    return await database.delete('ActivityLog');
  }

  /// Deletes activity log rows older than [days] days.
  Future<int> purgeActivityLog({int days = 30}) async {
    if (!isAvailable) return 0;
    final cutoff =
        DateTime.now().subtract(Duration(days: days)).toIso8601String();
    return await database.delete(
      'ActivityLog',
      where: 'timestamp < ?',
      whereArgs: [cutoff],
    );
  }

  // ---------------------------------------------------------------------------
  // Cleanup
  // ---------------------------------------------------------------------------

  /// Closes the database connection and resets the internal reference so that
  /// [initialize] can be called again if needed.
  Future<void> close() async {
    if (_db != null) {
      await _db!.close();
      _db = null;
    }
  }
}
