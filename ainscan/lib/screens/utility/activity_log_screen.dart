import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../services/local_database_service.dart';
import '../../widgets/app_scaffold.dart';

/// Activity Log viewer screen — shows logged user actions from the local
/// SQLite `ActivityLog` table.
///
/// Provides filtering by status (All / Success / Fail), pull-to-refresh,
/// and a clear button to purge old entries.
class ActivityLogScreen extends StatefulWidget {
  const ActivityLogScreen({super.key});

  @override
  State<ActivityLogScreen> createState() => _ActivityLogScreenState();
}

class _ActivityLogScreenState extends State<ActivityLogScreen> {
  final LocalDatabaseService _localDb = LocalDatabaseService();
  final DateFormat _displayFmt = DateFormat('dd/MM HH:mm:ss');

  List<Map<String, dynamic>> _allLogs = [];
  List<Map<String, dynamic>> _filteredLogs = [];
  bool _loading = false;
  String _filter = 'ALL'; // ALL, SUCCESS, FAIL
  int _totalCount = 0;

  @override
  void initState() {
    super.initState();
    _loadLogs();
  }

  Future<void> _loadLogs() async {
    if (!mounted) return;
    setState(() => _loading = true);
    try {
      final logs = await _localDb.getActivityLogs(limit: 500);
      final count = await _localDb.getActivityLogCount();
      if (!mounted) return;
      setState(() {
        _allLogs = logs;
        _totalCount = count;
        _applyFilter();
      });
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Error loading logs: $e')),
        );
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  void _applyFilter() {
    if (_filter == 'ALL') {
      _filteredLogs = _allLogs;
    } else {
      _filteredLogs =
          _allLogs.where((r) => r['status'] == _filter).toList();
    }
  }

  void _setFilter(String filter) {
    if (!mounted) return;
    setState(() {
      _filter = filter;
      _applyFilter();
    });
  }

  Future<void> _clearLogs() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Clear Activity Log?'),
        content: Text('Delete all $_totalCount log entries?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Cancel'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Clear', style: TextStyle(color: Colors.red)),
          ),
        ],
      ),
    );
    if (confirmed == true) {
      await _localDb.clearActivityLog();
      await _loadLogs();
    }
  }

  String _formatTimestamp(String? iso) {
    if (iso == null || iso.isEmpty) return '';
    try {
      return _displayFmt.format(DateTime.parse(iso));
    } catch (_) {
      return iso;
    }
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Activity Log',
      actions: [
        IconButton(
          icon: const Icon(Icons.delete_outline),
          tooltip: 'Clear Logs',
          onPressed: _clearLogs,
        ),
      ],
      body: Column(
        children: [
          // Filter chips + count
          Padding(
            padding:
                const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
            child: Row(
              children: [
                _filterChip('ALL', _allLogs.length),
                const SizedBox(width: 8),
                _filterChip(
                  'SUCCESS',
                  _allLogs.where((r) => r['status'] == 'SUCCESS').length,
                ),
                const SizedBox(width: 8),
                _filterChip(
                  'FAIL',
                  _allLogs.where((r) => r['status'] == 'FAIL').length,
                ),
                const Spacer(),
                Text(
                  'Total: $_totalCount',
                  style: TextStyle(
                    fontSize: 11,
                    color: isDark ? Colors.white54 : Colors.grey,
                  ),
                ),
              ],
            ),
          ),

          if (_loading) const LinearProgressIndicator(),

          // Log list
          Expanded(
            child: _filteredLogs.isEmpty
                ? Center(
                    child: Text(
                      _loading ? '' : 'No log entries',
                      style: TextStyle(
                        color: isDark ? Colors.white38 : Colors.grey,
                      ),
                    ),
                  )
                : RefreshIndicator(
                    onRefresh: _loadLogs,
                    child: ListView.builder(
                      itemCount: _filteredLogs.length,
                      itemBuilder: (context, index) {
                        final row = _filteredLogs[index];
                        return _logTile(row, isDark);
                      },
                    ),
                  ),
          ),
        ],
      ),
    );
  }

  Widget _filterChip(String label, int count) {
    final isSelected = _filter == label;
    return ChoiceChip(
      label: Text('$label ($count)', style: const TextStyle(fontSize: 11)),
      selected: isSelected,
      onSelected: (_) => _setFilter(label),
      visualDensity: VisualDensity.compact,
      padding: const EdgeInsets.symmetric(horizontal: 4),
    );
  }

  Widget _logTile(Map<String, dynamic> row, bool isDark) {
    final isFail = row['status'] == 'FAIL';
    final action = row['action']?.toString() ?? '';
    final detail = row['detail']?.toString() ?? '';
    final empNo = row['empNo']?.toString() ?? '';
    final errorMsg = row['errorMsg']?.toString() ?? '';
    final timestamp = _formatTimestamp(row['timestamp']?.toString());

    return Container(
      decoration: BoxDecoration(
        border: Border(
          bottom: BorderSide(
            color: isDark ? Colors.white10 : Colors.grey.shade200,
            width: 0.5,
          ),
        ),
      ),
      child: ListTile(
        dense: true,
        visualDensity: VisualDensity.compact,
        leading: Icon(
          isFail ? Icons.error_outline : Icons.check_circle_outline,
          color: isFail ? Colors.red : Colors.green,
          size: 20,
        ),
        title: Row(
          children: [
            Expanded(
              child: Text(
                action,
                style: TextStyle(
                  fontSize: 13,
                  fontWeight: FontWeight.w600,
                  color: isFail ? Colors.red.shade300 : null,
                ),
              ),
            ),
            Text(
              timestamp,
              style: TextStyle(
                fontSize: 10,
                color: isDark ? Colors.white38 : Colors.grey,
              ),
            ),
          ],
        ),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (detail.isNotEmpty)
              Text(
                detail,
                style: const TextStyle(fontSize: 11),
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
            if (isFail && errorMsg.isNotEmpty)
              Text(
                errorMsg,
                style: TextStyle(
                  fontSize: 10,
                  color: Colors.red.shade300,
                ),
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
            if (empNo.isNotEmpty)
              Text(
                'User: $empNo',
                style: TextStyle(
                  fontSize: 10,
                  color: isDark ? Colors.white30 : Colors.grey.shade500,
                ),
              ),
          ],
        ),
      ),
    );
  }
}
