import 'package:flutter/material.dart';

import '../../services/database_service.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/data_list_view.dart';

/// DA Detail screen — converts frmOut_DA_3.vb
///
/// Shows the product lines for a specific Delivery Advice (DA).
/// Receives the [daNo] as a parameter from [DaListScreen].
class DaDetailScreen extends StatefulWidget {
  /// The DA number to display details for.
  final String daNo;

  const DaDetailScreen({super.key, required this.daNo});

  @override
  State<DaDetailScreen> createState() => _DaDetailScreenState();
}

class _DaDetailScreenState extends State<DaDetailScreen> {
  final DatabaseService _db = DatabaseService();

  List<Map<String, String>> _rows = [];
  bool _loading = false;

  static const _columns = [
    DataColumnConfig(name: 'PCode', label: 'Product', width: 140),
    DataColumnConfig(name: 'PName', label: 'Name', width: 200),
    DataColumnConfig(name: 'Batch', width: 110),
    DataColumnConfig(name: 'Run', width: 60),
    DataColumnConfig(name: 'Rack', width: 80),
    DataColumnConfig(name: 'DAQty', label: 'DA Qty', width: 80),
    DataColumnConfig(name: 'OrderQty', label: 'Order', width: 80),
    DataColumnConfig(name: 'RackQty', label: 'Rack Qty', width: 80),
    DataColumnConfig(name: 'Unit', width: 60),
    DataColumnConfig(name: 'Status', width: 80),
  ];

  @override
  void initState() {
    super.initState();
    _loadDetails();
  }

  Future<void> _loadDetails() async {
    setState(() => _loading = true);
    try {
      final results = await _db.query(
        "SELECT PCode, PName, Batch, Run, Rack, DAQty, OrderQty, RackQty, Unit, Status "
        "FROM DO_0070 "
        "WHERE DANo=@DANo",
        {'@DANo': widget.daNo},
      );

      if (!mounted) return;
      setState(() {
        _rows = results.map((row) {
          return {
            'PCode': row['PCode']?.toString() ?? '',
            'PName': row['PName']?.toString() ?? '',
            'Batch': row['Batch']?.toString() ?? '',
            'Run': row['Run']?.toString() ?? '',
            'Rack': row['Rack']?.toString() ?? '',
            'DAQty': row['DAQty']?.toString() ?? '',
            'OrderQty': row['OrderQty']?.toString() ?? '',
            'RackQty': row['RackQty']?.toString() ?? '',
            'Unit': row['Unit']?.toString() ?? '',
            'Status': row['Status']?.toString() ?? '',
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
      title: 'DA Detail - ${widget.daNo}',
      body: Column(
        children: [
          // Loading indicator
          if (_loading) const LinearProgressIndicator(),

          // Data list card
          Expanded(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 12.0),
              child: Card(
                clipBehavior: Clip.hardEdge,
                elevation: 2,
                child: DataListView(
                  columns: _columns,
                  rows: _rows,
                  scrollable: true,
                ),
              ),
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
