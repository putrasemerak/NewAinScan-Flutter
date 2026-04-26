import 'package:flutter/material.dart';

import '../../config/database_config.dart';
import '../../services/database_service.dart';
import '../../widgets/app_scaffold.dart';

/// Database info screen — converts frmDB.vb
///
/// Admin-only screen showing DB connection details and status.
class DbScreen extends StatefulWidget {
  const DbScreen({super.key});

  @override
  State<DbScreen> createState() => _DbScreenState();
}

class _DbScreenState extends State<DbScreen> {
  final _db = DatabaseService();
  String _serverDate = '';
  String _connectionStatus = 'Checking...';

  @override
  void initState() {
    super.initState();
    _checkConnection();
  }

  Future<void> _checkConnection() async {
    try {
      final connected = await _db.connectedToNetwork();
      if (connected) {
        final dateRows = await _db.query('SELECT GetDate() AS CurrentDate');
        if (dateRows.isNotEmpty) {
          _serverDate = dateRows.first['CurrentDate'].toString();
        }
        if (!mounted) return;
        setState(() {
          _connectionStatus = 'Connected';
        });
      } else {
        if (!mounted) return;
        setState(() {
          _connectionStatus = 'Disconnected';
        });
      }
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _connectionStatus = 'Error: $e';
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Database Info',
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _infoRow('Profile', DatabaseConfig.currentProfile),
            _infoRow('Database', DatabaseConfig.databaseName),
            const Divider(height: 24),
            _infoRow('Status', _connectionStatus),
            if (_serverDate.isNotEmpty)
              _infoRow('Server Date', _serverDate),
            const Spacer(),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: _checkConnection,
                child: const Text('Test Connection'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _infoRow(String label, String value) {
    final cs = Theme.of(context).colorScheme;
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 100,
            child: Text(
              label,
              style: TextStyle(
                fontWeight: FontWeight.bold,
                color: cs.onSurface.withValues(alpha: 0.6),
              ),
            ),
          ),
          Expanded(
            child: Text(
              value,
              style: const TextStyle(fontSize: 14),
            ),
          ),
        ],
      ),
    );
  }
}
