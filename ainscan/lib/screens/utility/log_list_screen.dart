import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../services/auth_service.dart';
import '../../services/local_database_service.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/data_list_view.dart';

/// Log List screen — converts frmList.vb
///
/// Shows local Receiving or RackIn log records from SQLite.
/// The [formType] parameter controls the initial tab:
/// "R1" for Receiving, "S1" for RackIn.
///
/// By default displays records for today. User can pick any date to view
/// pallets received/racked-in on that date.
class LogListScreen extends StatefulWidget {
  final String formType;
  const LogListScreen({super.key, this.formType = 'R1'});

  @override
  State<LogListScreen> createState() => _LogListScreenState();
}

class _LogListScreenState extends State<LogListScreen> {
  final LocalDatabaseService _localDb = LocalDatabaseService();

  List<Map<String, String>> _rows = [];
  bool _loading = false;
  late bool _showReceiving;
  late DateTime _selectedDate;

  // ── Compact text styles (matching DA Confirmation) ─────────────────
  static final _labelStyle = GoogleFonts.robotoCondensed(
    fontSize: 11,
    fontWeight: FontWeight.w600,
  );

  // ── Column configs ─────────────────────────────────────────────────
  static const _receivingColumns = [
    DataColumnConfig(name: 'No', label: '#', flex: 1),
    DataColumnConfig(name: 'PalletNo', label: 'Pallet', flex: 3),
    DataColumnConfig(name: 'PCode', flex: 3),
    DataColumnConfig(name: 'Batch', flex: 2),
    DataColumnConfig(name: 'Qty', flex: 2),
    DataColumnConfig(name: 'Time', flex: 2),
  ];

  static const _rackInColumns = [
    DataColumnConfig(name: 'No', label: '#', flex: 1),
    DataColumnConfig(name: 'PalletNo', label: 'Pallet', flex: 3),
    DataColumnConfig(name: 'RackNo', label: 'Rack', flex: 2),
    DataColumnConfig(name: 'PCode', flex: 3),
    DataColumnConfig(name: 'Qty', flex: 2),
    DataColumnConfig(name: 'Time', flex: 2),
  ];

  @override
  void initState() {
    super.initState();
    _showReceiving = widget.formType != 'S1';
    _selectedDate = DateTime.now();
    _loadLogs();
  }

  String get _dateStr => DateFormat('yyyy-MM-dd').format(_selectedDate);
  String get _displayDate => DateFormat('dd/MM/yyyy').format(_selectedDate);

  Future<void> _loadLogs() async {
    if (!mounted) return;
    setState(() => _loading = true);
    try {
      final dateFilter = _dateStr;
      if (_showReceiving) {
        final results = await _localDb.getReceivingLogsByDate(dateFilter);
        if (!mounted) return;
        setState(() {
          _rows = List.generate(results.length, (i) {
            final row = results[i];
            return {
              'No': '${i + 1}',
              'PalletNo': row['PalletNo']?.toString() ?? '',
              'PCode': row['PCode']?.toString() ?? '',
              'Batch': row['Batch']?.toString() ?? '',
              'Qty': row['Qty']?.toString() ?? '',
              'Time': row['Time']?.toString() ?? '',
            };
          });
        });
      } else {
        final results = await _localDb.getRackInLogsByDate(dateFilter);
        if (!mounted) return;
        setState(() {
          _rows = List.generate(results.length, (i) {
            final row = results[i];
            return {
              'No': '${i + 1}',
              'PalletNo': row['PalletNo']?.toString() ?? '',
              'RackNo': row['RackNo']?.toString() ?? '',
              'PCode': row['PCode']?.toString() ?? '',
              'Qty': row['Qty']?.toString() ?? '',
              'Time': row['Time']?.toString() ?? '',
            };
          });
        });
      }
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

  void _switchLogType(bool showReceiving) {
    if (_showReceiving == showReceiving) return;
    if (!mounted) return;
    setState(() {
      _showReceiving = showReceiving;
      _rows = [];
    });
    _loadLogs();
  }

  Future<void> _pickDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate,
      firstDate: DateTime(2020),
      lastDate: DateTime.now(),
    );
    if (picked != null && picked != _selectedDate) {
      if (!mounted) return;
      setState(() => _selectedDate = picked);
      _loadLogs();
    }
  }

  @override
  Widget build(BuildContext context) {
    final auth = Provider.of<AuthService>(context, listen: false);
    final currentDate =
        DateFormat('dd/MM/yyyy HH:mm:ss').format(DateTime.now());
    final btnStyle = ElevatedButton.styleFrom(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 8),
      textStyle: GoogleFonts.robotoCondensed(
        fontSize: 11,
        fontWeight: FontWeight.w600,
      ),
    );

    return AppScaffold(
      title: _showReceiving ? 'Receiving Log' : 'Rack In Log',
      body: Column(
        children: [
          // ── Scrollable top section ──────────────────────────────────
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 6),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                // ─── 1. Date / User card ────────────────────────────
                Card(
                  child: Padding(
                    padding: const EdgeInsets.symmetric(
                        horizontal: 10, vertical: 5),
                    child: Text(
                      'Date/User : $currentDate  /  ${auth.empNo ?? ''}@${auth.empName ?? ''}',
                      style: GoogleFonts.robotoCondensed(fontSize: 11),
                    ),
                  ),
                ),
                const SizedBox(height: 4),

                // ─── 2. Toggle + Date selector card ─────────────────
                Card(
                  child: Padding(
                    padding: const EdgeInsets.symmetric(
                        horizontal: 10, vertical: 6),
                    child: Column(
                      children: [
                        // Receiving / RackIn toggle
                        Row(
                          children: [
                            Text('Type:', style: _labelStyle),
                            const SizedBox(width: 8),
                            Expanded(
                              child: SegmentedButton<bool>(
                                segments: [
                                  ButtonSegment(
                                    value: true,
                                    label: Text('Receiving',
                                        style: GoogleFonts.robotoCondensed(
                                            fontSize: 11)),
                                  ),
                                  ButtonSegment(
                                    value: false,
                                    label: Text('Rack In',
                                        style: GoogleFonts.robotoCondensed(
                                            fontSize: 11)),
                                  ),
                                ],
                                selected: {_showReceiving},
                                onSelectionChanged: (selected) {
                                  _switchLogType(selected.first);
                                },
                                style: ButtonStyle(
                                  visualDensity: VisualDensity.compact,
                                  tapTargetSize:
                                      MaterialTapTargetSize.shrinkWrap,
                                ),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 6),
                        // Date selector
                        Row(
                          children: [
                            Text('Date:', style: _labelStyle),
                            const SizedBox(width: 8),
                            InkWell(
                              onTap: _pickDate,
                              child: Container(
                                padding: const EdgeInsets.symmetric(
                                    horizontal: 10, vertical: 6),
                                decoration: BoxDecoration(
                                  border: Border.all(
                                      color: Colors.grey.shade400),
                                  borderRadius: BorderRadius.circular(4),
                                ),
                                child: Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    Icon(Icons.calendar_today,
                                        size: 14,
                                        color: Theme.of(context)
                                            .colorScheme
                                            .primary),
                                    const SizedBox(width: 6),
                                    Text(
                                      _displayDate,
                                      style: GoogleFonts.robotoCondensed(
                                          fontSize: 12,
                                          fontWeight: FontWeight.w600),
                                    ),
                                  ],
                                ),
                              ),
                            ),
                            const SizedBox(width: 8),
                            Text(
                              '${_rows.length} record${_rows.length == 1 ? '' : 's'}',
                              style: GoogleFonts.robotoCondensed(
                                  fontSize: 11,
                                  color: Colors.green.shade700,
                                  fontWeight: FontWeight.w600),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),

          // Loading indicator
          if (_loading) const LinearProgressIndicator(),

          // ── Data ListView ──────────────────────────────────────────
          Expanded(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 8),
              child: Card(
                clipBehavior: Clip.hardEdge,
                child: Column(
                  children: [
                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.symmetric(
                          horizontal: 6, vertical: 4),
                      color: Theme.of(context)
                          .colorScheme
                          .primaryContainer,
                      child: Text(
                        _showReceiving
                            ? 'Pallet Received - $_displayDate'
                            : 'Pallet Racked In - $_displayDate',
                        style: GoogleFonts.robotoCondensed(
                            fontSize: 10,
                            fontWeight: FontWeight.w600),
                      ),
                    ),
                    Expanded(
                      child: DataListView(
                        columns: _showReceiving
                            ? _receivingColumns
                            : _rackInColumns,
                        rows: _rows,
                        headerTextStyle:
                            GoogleFonts.robotoCondensed(fontSize: 11),
                        headerPadding: const EdgeInsets.symmetric(
                            horizontal: 8, vertical: 4),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
          const SizedBox(height: 4),

          // ── Bottom button ──────────────────────────────────────────
          Padding(
            padding: const EdgeInsets.fromLTRB(8, 0, 8, 6),
            child: SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: () => Navigator.of(context).pop(),
                style: btnStyle.copyWith(
                  backgroundColor:
                      WidgetStatePropertyAll(Colors.grey.shade600),
                  foregroundColor:
                      const WidgetStatePropertyAll(Colors.white),
                ),
                child: const Text('CLOSE'),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
