import 'package:flutter/material.dart';

import '../../services/database_service.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/data_list_view.dart';

/// Stock Take List screen — converts frmListStockTake.vb
///
/// Shows stock take records from TA_STK001, ordered by AddDate descending.
class StockTakeListScreen extends StatefulWidget {
  const StockTakeListScreen({super.key});

  @override
  State<StockTakeListScreen> createState() => _StockTakeListScreenState();
}

class _StockTakeListScreenState extends State<StockTakeListScreen> {
  final DatabaseService _db = DatabaseService();

  List<Map<String, String>> _rows = [];
  bool _loading = false;

  static const _columns = [
    DataColumnConfig(name: 'Pallet', flex: 2),
    DataColumnConfig(name: 'Batch', flex: 2),
    DataColumnConfig(name: 'Run', flex: 1),
    DataColumnConfig(name: 'Rack', flex: 2),
    DataColumnConfig(name: 'Qty', flex: 1),
  ];

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _loadData() async {
    if (!mounted) return;
    setState(() => _loading = true);
    try {
      final results = await _db.query(
        "SELECT Pallet, Batch, Run, Rack, Qty, AddUser, AddDate "
        "FROM TA_STK001 ORDER BY AddDate DESC",
      );

      if (!mounted) return;
      setState(() {
        _rows = results.map((row) {
          return {
            'Pallet': row['Pallet']?.toString() ?? '',
            'Batch': row['Batch']?.toString() ?? '',
            'Run': row['Run']?.toString() ?? '',
            'Rack': row['Rack']?.toString() ?? '',
            'Qty': row['Qty']?.toString() ?? '',
          };
        }).toList();
      });
    } on DatabaseException catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Error: ${e.message}')),
        );
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Stock Take List',
      body: Column(
        children: [
          // Loading indicator
          if (_loading) const LinearProgressIndicator(),

          // Data list
          Expanded(
            child: DataListView(
              columns: _columns,
              rows: _rows,
            ),
          ),

          // Close button
          Padding(
            padding: const EdgeInsets.all(12.0),
            child: SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () => Navigator.of(context).pop(),
                icon: const Icon(Icons.close),
                label: const Text('Close'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.grey.shade600,
                  foregroundColor: Colors.white,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
