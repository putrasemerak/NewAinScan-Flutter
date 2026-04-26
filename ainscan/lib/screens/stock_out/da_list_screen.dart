import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';

import '../../services/database_service.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/data_list_view.dart';

/// DA List screen — converts frmOut_DA_1.vb
///
/// Shows a list of Delivery Advices (DAs) filtered by date.
/// Tapping a row shows detail lines inline below the DA list.
class DaListScreen extends StatefulWidget {
  const DaListScreen({super.key});

  @override
  State<DaListScreen> createState() => _DaListScreenState();
}

class _DaListScreenState extends State<DaListScreen> {
  final DatabaseService _db = DatabaseService();
  final DateFormat _displayFormat = DateFormat('dd/MM/yyyy');
  final DateFormat _queryFormat = DateFormat('yyyyMMdd');

  DateTime _selectedDate = DateTime.now();
  List<Map<String, String>> _rows = [];
  bool _loading = false;

  // Detail panel
  String _selectedDA = '';
  List<Map<String, String>> _detailRows = [];
  bool _loadingDetail = false;

  static const _columns = [
    DataColumnConfig(name: 'DANo', label: 'DA No', flex: 3),
    DataColumnConfig(name: 'PCode', label: 'PCode', flex: 2),
    DataColumnConfig(name: 'Batch', flex: 2),
    DataColumnConfig(name: 'Qty', flex: 1),
    DataColumnConfig(name: 'Status', flex: 1),
  ];

  static const _detailColumns = [
    DataColumnConfig(name: 'PCode', label: 'PCode', flex: 2),
    DataColumnConfig(name: 'Batch', flex: 2),
    DataColumnConfig(name: 'Run', flex: 1),
    DataColumnConfig(name: 'Rack', flex: 1),
    DataColumnConfig(name: 'DAQty', label: 'DA', flex: 1),
    DataColumnConfig(name: 'RackQty', label: 'Rack', flex: 1),
  ];

  @override
  void initState() {
    super.initState();
    _search();
  }

  Future<void> _pickDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate,
      firstDate: DateTime(2000),
      lastDate: DateTime(2099),
    );
    if (picked != null) {
      if (!mounted) return;
      setState(() => _selectedDate = picked);
    }
  }

  Future<void> _search() async {
    if (!mounted) return;
    setState(() {
      _loading = true;
      _selectedDA = '';
      _detailRows = [];
    });
    try {
      final results = await _db.query(
        "SELECT DANo, PCode, PName, Batch, CustName, Quantity, OrderQty, Unit, Status, ETD "
        "FROM DO_0020 "
        "WHERE CONVERT(varchar,ETD,112) = @ETD "
        "ORDER BY DANo",
        {'@ETD': _queryFormat.format(_selectedDate)},
      );

      if (!mounted) return;
      setState(() {
        _rows = results.map((row) {
          return {
            'DANo': row['DANo']?.toString().trim() ?? '',
            'PCode': row['PCode']?.toString().trim() ?? '',
            'PName': row['PName']?.toString().trim() ?? '',
            'Batch': row['Batch']?.toString().trim() ?? '',
            'CustName': row['CustName']?.toString().trim() ?? '',
            'Qty': row['Quantity']?.toString().trim() ?? '',
            'OrderQty': row['OrderQty']?.toString().trim() ?? '',
            'Unit': row['Unit']?.toString().trim() ?? '',
            'Status': row['Status']?.toString().trim() ?? '',
            'ETD': row['ETD'] != null
                ? _formatDate(row['ETD'].toString())
                : '',
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

  Future<void> _loadDetail(String daNo) async {
    if (daNo == _selectedDA) {
      // Toggle off
      if (!mounted) return;
      setState(() {
        _selectedDA = '';
        _detailRows = [];
      });
      return;
    }
    if (!mounted) return;
    setState(() {
      _selectedDA = daNo;
      _loadingDetail = true;
      _detailRows = [];
    });
    try {
      final results = await _db.query(
        "SELECT PCode, PName, Batch, Run, Rack, DAQty, OrderQty, RackQty, Unit, Status "
        "FROM DO_0070 "
        "WHERE DANo=@DANo",
        {'@DANo': daNo},
      );
      if (!mounted) return;
      setState(() {
        _detailRows = results.map((row) {
          return {
            'PCode': row['PCode']?.toString().trim() ?? '',
            'PName': row['PName']?.toString().trim() ?? '',
            'Batch': row['Batch']?.toString().trim() ?? '',
            'Run': row['Run']?.toString().trim() ?? '',
            'Rack': row['Rack']?.toString().trim() ?? '',
            'DAQty': row['DAQty']?.toString().trim() ?? '',
            'OrderQty': row['OrderQty']?.toString().trim() ?? '',
            'RackQty': row['RackQty']?.toString().trim() ?? '',
            'Unit': row['Unit']?.toString().trim() ?? '',
            'Status': row['Status']?.toString().trim() ?? '',
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
      if (mounted) setState(() => _loadingDetail = false);
    }
  }

  String _formatDate(String raw) {
    try {
      final dt = DateTime.parse(raw);
      return _displayFormat.format(dt);
    } catch (_) {
      return raw;
    }
  }

  // ── Condensed text styles ──────────────────────────────────────────
  TextStyle get _cSectionTitle => GoogleFonts.robotoCondensed(
    fontSize: 11,
    fontWeight: FontWeight.w600,
    color: Theme.of(context).brightness == Brightness.dark
        ? const Color(0xFFE6E6E6)
        : Theme.of(context).colorScheme.onSurface.withValues(alpha: 0.65),
  );

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    final isDark = Theme.of(context).brightness == Brightness.dark;

    final btnStyle = ElevatedButton.styleFrom(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 8),
      textStyle: GoogleFonts.robotoCondensed(
        fontSize: 11,
        fontWeight: FontWeight.w600,
      ),
    );

    return AppScaffold(
      title: 'DA List',
      body: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            const SizedBox(height: 4),

            // ─── ETD Date + Search — side by side ───────────────
            Row(
              children: [
                // Date field
                Expanded(
                  child: InkWell(
                    onTap: _pickDate,
                    child: Container(
                      height: 36,
                      padding: const EdgeInsets.symmetric(horizontal: 8),
                      decoration: BoxDecoration(
                        color: isDark ? const Color(0xFF343B47) : Colors.white,
                        border: Border.all(color: isDark ? const Color(0xFF546E7A) : const Color(0xFF616161)),
                        borderRadius: BorderRadius.circular(4),
                      ),
                      child: Row(
                        children: [
                          Text('ETD:', style: GoogleFonts.robotoCondensed(
                              fontSize: 11, color: isDark ? const Color(0xFF80CBC4) : const Color(0xFF616161))),
                          const SizedBox(width: 6),
                          Expanded(
                            child: Text(
                              _displayFormat.format(_selectedDate),
                              style: GoogleFonts.robotoCondensed(
                                  fontSize: 13, color: isDark ? const Color(0xFFE0E0E0) : Colors.black),
                            ),
                          ),
                          Icon(Icons.calendar_today,
                              size: 16, color: isDark ? const Color(0xFFB0B3B8) : const Color(0xFF616161)),
                        ],
                      ),
                    ),
                  ),
                ),
                const SizedBox(width: 6),
                // Search button
                SizedBox(
                  height: 36,
                  child: ElevatedButton.icon(
                    onPressed: _loading ? null : _search,
                    icon: const Icon(Icons.search, size: 16),
                    label: Text('Search',
                        style: GoogleFonts.robotoCondensed(fontSize: 12)),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: cs.primary,
                      foregroundColor: cs.onPrimary,
                      minimumSize: const Size(0, 36),
                      padding: const EdgeInsets.symmetric(horizontal: 12),
                      shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(4)),
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 4),

            // Loading indicator
            if (_loading)
              const Padding(
                padding: EdgeInsets.only(bottom: 2),
                child: LinearProgressIndicator(),
              ),

            // ─── DA List ────────────────────────────────────────
            Text('DA List (${_rows.length})', style: _cSectionTitle),
            const SizedBox(height: 2),
            Expanded(
              flex: _selectedDA.isEmpty ? 1 : 1,
              child: Card(
                margin: const EdgeInsets.only(bottom: 3),
                clipBehavior: Clip.hardEdge,
                child: DataListView(
                  columns: _columns,
                  rows: _rows,
                  headerTextStyle: GoogleFonts.robotoCondensed(fontSize: 11),
                  headerPadding: const EdgeInsets.symmetric(
                      horizontal: 8, vertical: 4),
                  onRowTap: (index, row) {
                    final daNo = row['DANo'] ?? '';
                    if (daNo.isNotEmpty) {
                      _loadDetail(daNo);
                    }
                  },
                ),
              ),
            ),

            // ─── DA Detail (inline) ─────────────────────────────
            if (_selectedDA.isNotEmpty) ...[
              Text('Detail: $_selectedDA', style: _cSectionTitle),
              const SizedBox(height: 2),
              if (_loadingDetail)
                const Padding(
                  padding: EdgeInsets.only(bottom: 2),
                  child: LinearProgressIndicator(),
                ),
              Expanded(
                flex: 1,
                child: Card(
                  margin: const EdgeInsets.only(bottom: 3),
                  clipBehavior: Clip.hardEdge,
                  child: DataListView(
                    columns: _detailColumns,
                    rows: _detailRows,
                    headerTextStyle:
                        GoogleFonts.robotoCondensed(fontSize: 11),
                    headerPadding: const EdgeInsets.symmetric(
                        horizontal: 8, vertical: 4),
                  ),
                ),
              ),
            ],

            // ─── Close button ───────────────────────────────────
            Padding(
              padding: const EdgeInsets.only(bottom: 4),
              child: SizedBox(
                height: 30,
                child: ElevatedButton(
                  onPressed: () => Navigator.of(context).pop(),
                  style: btnStyle.copyWith(
                    backgroundColor:
                        WidgetStatePropertyAll(Colors.grey.shade600),
                    foregroundColor:
                        const WidgetStatePropertyAll(Colors.white),
                    minimumSize:
                        const WidgetStatePropertyAll(Size(0, 30)),
                  ),
                  child: const Text('Close'),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
