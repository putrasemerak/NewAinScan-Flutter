import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';

import '../../services/database_service.dart';
import '../../services/keyboard_service.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/data_list_view.dart';

/// Location / Stock screen — converts frmLokasi.vb
///
/// Shows stock on-hand querying IV_0250.
/// Two modes:
///   1. If Batch text field has text → search by Batch + Run
///   2. If Batch text field is empty → loop every Batch/Run in DO_0020
///      for the given [daNo] and show combined results from IV_0250.
class LocationScreen extends StatefulWidget {
  /// Optional batch to pre-fill the text field.
  final String? batch;

  /// DA Number used for DA-based lookup when batch field is empty.
  final String? daNo;

  const LocationScreen({super.key, this.batch, this.daNo});

  @override
  State<LocationScreen> createState() => _LocationScreenState();
}

class _LocationScreenState extends State<LocationScreen> {
  final DatabaseService _db = DatabaseService();
  final TextEditingController _batchController = TextEditingController();
  final TextEditingController _runController = TextEditingController();
  final FocusNode _batchFocus = FocusNode();
  final FocusNode _runFocus = FocusNode();

  List<Map<String, String>> _rows = [];
  bool _loading = false;

  static const _columns = [
    DataColumnConfig(name: 'PCode', flex: 3),
    DataColumnConfig(name: 'Batch', flex: 2),
    DataColumnConfig(name: 'Loct', flex: 2),
    DataColumnConfig(name: 'Pallet', flex: 2),
    DataColumnConfig(name: 'OnHand', flex: 2),
    DataColumnConfig(name: 'Run', flex: 1),
  ];

  @override
  void initState() {
    super.initState();
    if (widget.batch != null && widget.batch!.isNotEmpty) {
      _batchController.text = widget.batch!;
    }
    _batchFocus.addListener(() => _hideKeyboardInScannerMode(_batchFocus));
    _runFocus.addListener(() => _hideKeyboardInScannerMode(_runFocus));
  }

  /// Hide soft keyboard on focus when in scanner mode, keeping input
  /// connection alive for hardware barcode scanners.
  void _hideKeyboardInScannerMode(FocusNode node) {
    if (node.hasFocus) {
      final kbService = context.read<KeyboardService>();
      if (!kbService.keyboardEnabled) {
        Future.delayed(const Duration(milliseconds: 50), () {
          SystemChannels.textInput.invokeMethod('TextInput.hide');
        });
      }
    }
  }

  // ---------------------------------------------------------------------------
  // Batch field Enter — validate against PD_0010, check UKEKN1
  // ---------------------------------------------------------------------------
  Future<void> _onBatchSubmitted(String value) async {
    final batch = value.trim().toUpperCase();
    if (batch.isEmpty) return;

    try {
      final rs = await _db.query(
        "SELECT Batch, UKEKN1 FROM PD_0010 WHERE Batch=@Batch",
        {'@Batch': batch},
      );
      if (rs.isEmpty) {
        _showMsg('No Batch tidak sah');
        return;
      }

      final ukekn1 = int.tryParse(rs.first['UKEKN1']?.toString() ?? '0') ?? 0;
      if (ukekn1 == 0) {
        // Run not needed — set dash and auto-focus Refresh
        _runController.text = '-';
        _search();
      } else {
        _runFocus.requestFocus();
      }
    } on DatabaseException catch (e) {
      _showMsg('Ralat: ${e.message}');
    } catch (e) {
      _showMsg('Ralat: $e');
    }
  }

  // ---------------------------------------------------------------------------
  // Run field Enter — validate batch again, then focus search
  // ---------------------------------------------------------------------------
  Future<void> _onRunSubmitted(String value) async {
    // Re-validate batch then trigger search
    final batch = _batchController.text.trim().toUpperCase();
    if (batch.isEmpty) return;

    try {
      final rs = await _db.query(
        "SELECT Batch, UKEKN1 FROM PD_0010 WHERE Batch=@Batch",
        {'@Batch': batch},
      );
      if (rs.isEmpty) {
        _showMsg('No Batch tidak sah');
        return;
      }
    } on DatabaseException catch (e) {
      _showMsg('Ralat: ${e.message}');
      return;
    } catch (e) {
      _showMsg('Ralat: $e');
      return;
    }

    _search();
  }

  // ---------------------------------------------------------------------------
  // Refresh / Search
  // ---------------------------------------------------------------------------
  Future<void> _search() async {
    if (!mounted) return;
    setState(() {
      _loading = true;
      _rows = [];
    });

    try {
      final batchText = _batchController.text.trim().toUpperCase();

      if (batchText.isNotEmpty) {
        // --- Mode 1: search by Batch + Run ---
        final run = _runController.text.trim().toUpperCase();
        final results = await _db.query(
          "SELECT Loct, Pallet, Batch, Run, OnHand, PCode "
          "FROM IV_0250 "
          "WHERE Batch=@Batch AND Run=@Run AND OnHand > 0",
          {'@Batch': batchText, '@Run': run},
        );

        if (results.isEmpty) {
          _showMsg('Tiada stok untuk Batch ini');
          _batchController.clear();
          _runController.clear();
        } else {
          _setRows(results);
        }
      } else {
        // --- Mode 2: DA-based lookup ---
        final daNo = widget.daNo ?? '';
        if (daNo.isEmpty) {
          _showMsg('Sila masukkan Batch atau DA Number');
          if (!mounted) return;
          setState(() => _loading = false);
          return;
        }

        final daRows = await _db.query(
          "SELECT DANo, Batch, Run FROM DO_0020 WHERE DANo=@DANo",
          {'@DANo': daNo},
        );

        if (daRows.isEmpty) {
          _showMsg('Tiada rekod DA');
        } else {
          final List<Map<String, Object?>> allResults = [];

          for (final daRow in daRows) {
            final batch = daRow['Batch']?.toString() ?? '';
            final run = daRow['Run']?.toString() ?? '';

            final results = await _db.query(
              "SELECT Loct, Pallet, Batch, Run, OnHand, PCode "
              "FROM IV_0250 "
              "WHERE Batch=@Batch AND Run=@Run AND OnHand > 0",
              {'@Batch': batch, '@Run': run},
            );
            allResults.addAll(results);
          }

          if (allResults.isEmpty) {
            _showMsg('Tiada stok untuk Batch ini');
            _batchController.clear();
            _runController.clear();
          } else {
            _setRows(allResults);
          }
        }
      }
    } on DatabaseException catch (e) {
      _showMsg('Ralat: ${e.message}');
    } catch (e) {
      _showMsg('Ralat: $e');
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  void _setRows(List<Map<String, Object?>> results) {
    if (!mounted) return;
    setState(() {
      _rows = results.map((row) {
        return {
          'PCode': row['PCode']?.toString() ?? '',
          'Batch': row['Batch']?.toString() ?? '',
          'Loct': row['Loct']?.toString() ?? '',
          'Pallet': row['Pallet']?.toString() ?? '',
          'OnHand': row['OnHand']?.toString() ?? '',
          'Run': row['Run']?.toString() ?? '',
        };
      }).toList();
    });
  }

  void _showMsg(String msg) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(msg)),
    );
  }

  @override
  void dispose() {
    _batchController.dispose();
    _runController.dispose();
    _batchFocus.dispose();
    _runFocus.dispose();
    super.dispose();
  }

  // ── Compact text styles ────────────────────────────────────────────
  TextStyle _labelStyle(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GoogleFonts.robotoCondensed(
      fontSize: 11,
      fontWeight: FontWeight.w600,
      color: isDark ? const Color(0xFFB0B3B8) : null,
    );
  }

  @override
  Widget build(BuildContext context) {
    // Watch KeyboardService so we rebuild when mode toggles (for UI cues)
    context.watch<KeyboardService>();
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
      title: 'Location / Stock',
      body: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // ─── DA No indicator (when provided) ──────────────────
            if (widget.daNo != null && widget.daNo!.isNotEmpty)
              Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                margin: const EdgeInsets.only(bottom: 4),
                decoration: BoxDecoration(
                  color: Colors.indigo.shade700,
                  borderRadius: BorderRadius.circular(4),
                ),
                child: Row(
                  children: [
                    Icon(Icons.assignment,
                        size: 14, color: Colors.white),
                    const SizedBox(width: 6),
                    Text(
                      'DA : ${widget.daNo}',
                      style: GoogleFonts.robotoCondensed(
                        fontSize: 12,
                        fontWeight: FontWeight.w700,
                        color: Colors.white,
                      ),
                    ),
                  ],
                ),
              ),

            // ─── Batch + Run side by side ─────────────────────────
            Row(
              children: [
                Expanded(
                  flex: 3,
                  child: _buildInputField(
                    label: 'Batch',
                    controller: _batchController,
                    focusNode: _batchFocus,
                    keyboardType: TextInputType.text,
                    onSubmitted: _onBatchSubmitted,
                    cs: cs,
                    isDark: isDark,
                  ),
                ),
                const SizedBox(width: 6),
                Expanded(
                  flex: 2,
                  child: _buildInputField(
                    label: 'Run',
                    controller: _runController,
                    focusNode: _runFocus,
                    keyboardType: TextInputType.text,
                    onSubmitted: _onRunSubmitted,
                    cs: cs,
                    isDark: isDark,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 4),

            // ─── Row count card ───────────────────────────────────
            Card(
              margin: const EdgeInsets.only(bottom: 3),
              color: Colors.teal.shade700,
              child: Padding(
                padding:
                    const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                child: Row(
                  children: [
                    Text(
                      'Results : ${_rows.length}',
                      style: GoogleFonts.robotoCondensed(
                          fontSize: 12,
                          fontWeight: FontWeight.w600,
                          color: Colors.white),
                    ),
                    const Spacer(),
                    if (_loading)
                      const SizedBox(
                        width: 14,
                        height: 14,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: Colors.white,
                        ),
                      )
                    else
                      Icon(Icons.location_on,
                          size: 16,
                          color: Colors.white.withValues(alpha: 0.9)),
                  ],
                ),
              ),
            ),

            // ─── DataListView — fills remaining space ─────────────
            Expanded(
              child: Card(
                margin: const EdgeInsets.only(bottom: 3),
                clipBehavior: Clip.hardEdge,
                child: DataListView(
                  columns: _columns,
                  rows: _rows,
                  headerTextStyle:
                      GoogleFonts.robotoCondensed(fontSize: 11),
                  headerPadding: const EdgeInsets.symmetric(
                      horizontal: 8, vertical: 4),
                ),
              ),
            ),

            // ─── Secondary buttons — Prepare | Close ──────────────
            Padding(
              padding: const EdgeInsets.only(bottom: 3),
              child: Row(
                children: [
                  Expanded(
                    child: ElevatedButton(
                      onPressed: () => Navigator.of(context).pop(),
                      style: btnStyle.copyWith(
                        backgroundColor:
                            WidgetStatePropertyAll(Colors.blue.shade700),
                        foregroundColor:
                            const WidgetStatePropertyAll(Colors.white),
                        minimumSize:
                            const WidgetStatePropertyAll(Size(0, 30)),
                      ),
                      child: const Text('Prepare'),
                    ),
                  ),
                  const SizedBox(width: 4),
                  Expanded(
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
                ],
              ),
            ),

            // ─── Primary REFRESH — large, glove-friendly ──────────
            Padding(
              padding: const EdgeInsets.only(bottom: 4),
              child: SizedBox(
                width: double.infinity,
                height: 52,
                child: MouseRegion(
                  cursor: SystemMouseCursors.click,
                  child: ElevatedButton.icon(
                    onPressed: _loading ? null : _search,
                    icon: _loading
                        ? const SizedBox(
                            width: 20,
                            height: 20,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                              color: Colors.white,
                            ),
                          )
                        : const Icon(Icons.refresh, size: 22),
                    label: Text('REFRESH',
                        style: GoogleFonts.robotoCondensed(
                            fontSize: 16,
                            fontWeight: FontWeight.bold,
                            letterSpacing: 1.5)),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.green.shade700,
                      foregroundColor: Colors.white,
                      disabledBackgroundColor: Colors.grey.shade300,
                      disabledForegroundColor: Colors.grey.shade500,
                      shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8)),
                      elevation: 3,
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  /// Builds a compact input field with label above, matching the design system.
  Widget _buildInputField({
    required String label,
    required TextEditingController controller,
    required FocusNode focusNode,
    required TextInputType keyboardType,
    required ValueChanged<String> onSubmitted,
    required ColorScheme cs,
    required bool isDark,
  }) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 6),
      decoration: BoxDecoration(
        color: isDark ? const Color(0xFF343B47) : Colors.white,
        borderRadius: BorderRadius.circular(4),
        border: Border.all(
          color: isDark
              ? const Color(0xFF546E7A)
              : const Color(0xFF455A64),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(label, style: _labelStyle(context)),
          const SizedBox(height: 2),
          TextField(
            controller: controller,
            focusNode: focusNode,
            keyboardType: keyboardType,
            textCapitalization: TextCapitalization.characters,
            style: GoogleFonts.robotoCondensed(
                fontSize: 13, color: isDark ? const Color(0xFFE0E0E0) : Colors.black),
            decoration: InputDecoration(
              isDense: true,
              contentPadding:
                  const EdgeInsets.symmetric(horizontal: 4, vertical: 4),
              border: InputBorder.none,
            ),
            onSubmitted: onSubmitted,
          ),
        ],
      ),
    );
  }
}
