/// Database configuration - mirrors Data_Con.vb connection settings
///
/// Supports runtime switching between profiles:
///   - **Live**: Production database (AINData @ 194.100.1.249)
///   - **Demo**: Local development database (DevData @ 192.168.68.123)
///
/// Call [useLive] or [useDemo] before connecting.
/// Override defaults at build time with --dart-define:
///   flutter run -d chrome --dart-define=DB_HOST=194.100.1.249
class DatabaseConfig {
  // ── Current runtime values (mutable) ─────────────────────────────────────
  static String _serverHost =
      const String.fromEnvironment('DB_HOST', defaultValue: '194.100.1.249');
  static String _serverPort =
      const String.fromEnvironment('DB_PORT', defaultValue: '1433');
  static String _databaseName =
      const String.fromEnvironment('DB_NAME', defaultValue: 'AINData');
  static String _username =
      const String.fromEnvironment('DB_USER', defaultValue: 'sa');
  static String _password =
      const String.fromEnvironment('DB_PASS', defaultValue: 'ain06@sql');
  static final String _apiUrl =
      const String.fromEnvironment('API_URL', defaultValue: 'http://localhost:8085');

  static String _currentProfile = 'Live';

  // ── Public getters ───────────────────────────────────────────────────────
  static String get serverHost => _serverHost;
  static String get serverPort => _serverPort;
  static String get databaseName => _databaseName;
  static String get username => _username;
  static String get password => _password;
  static String get apiUrl => _apiUrl;
  static String get currentProfile => _currentProfile;

  static String get connectionString =>
      'Server=$serverHost,$serverPort;Database=$databaseName;User Id=$username;Password=$password;';

  // ── Profile switching ────────────────────────────────────────────────────

  /// Switch to the production database.
  static void useLive() {
    _currentProfile = 'Live';
    _serverHost = '194.100.1.249';
    _serverPort = '1433';
    _databaseName = 'AINData';
    _username = 'sa';
    _password = 'ain06@sql';
  }

  /// Switch to the local development database.
  static void useDemo() {
    _currentProfile = 'Demo';
    _serverHost = '194.100.1.222';
    _serverPort = '1433';
    _databaseName = 'PQData';
    _username = 'sa';
    _password = 'P@ssw0rd';
  }

  // ---------------------------------------------------------------------------
  // API proxy server (used by Flutter Web – runs on your PC/server)
  // Start it with: dart run bin/api_server.dart
  //
  // Override at build time:
  //   flutter run -d chrome --dart-define=API_URL=http://192.168.1.100:8085
  // ---------------------------------------------------------------------------

  // Local database name (replaces SQL Server CE appDatabase.sdf)
  static const String localDatabaseName = 'appDatabase.db';
}
