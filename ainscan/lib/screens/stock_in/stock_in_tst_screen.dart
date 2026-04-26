import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';

import '../../models/pallet_info.dart';
import '../../services/auth_service.dart';
import '../../services/tst_inbound_service.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/scan_field.dart';
import '../../widgets/barcode_scanner_dialog.dart';
import '../../widgets/data_list_view.dart';
import '../../widgets/labeled_text_field.dart';
import '../../utils/dialog_mixin.dart';

/// TST Stock In screen — converts frmStockIN_TST.vb
///
/// Combines receiving + rack-in in one step. Handles both NORMAL and LOOSE
/// pallets. Flow: scan pallet -> review info -> edit actual qty -> scan rack
/// -> press Inbound to execute receiving + rack-in together.
///
/// Business logic is in [TstInboundService]; this file is UI only.
class StockInTstScreen extends StatefulWidget {
  const StockInTstScreen({super.key});

  @override
  State<StockInTstScreen> createState() => _StockInTstScreenState();
}

class _StockInTstScreenState extends State<StockInTstScreen> with DialogMixin {
  final _service = TstInboundService();

  // Controllers
  final _palletController = TextEditingController();
  final _rackController = TextEditingController();
  final _actualQtyController = TextEditingController();

  // Focus nodes
  final _palletFocus = FocusNode();
  final _rackFocus = FocusNode();
  final _actualQtyFocus = FocusNode();

  // UI state
  bool _isLoading = false;
  bool _palletScanned = false;
  bool _rackScanned = false;

  // Domain object — replaces 12+ loose variables
  PalletInfo _pallet = PalletInfo.empty;

  // Stock location list displayed at the bottom
  List<Map<String, String>> _stockRows = [];

  // DataListView column config
  final List<DataColumnConfig> _stockColumns = const [
    DataColumnConfig(name: 'Loct', flex: 2),
    DataColumnConfig(name: 'OnHand', flex: 2),
    DataColumnConfig(name: 'PCode', flex: 2),
    DataColumnConfig(name: 'Batch', flex: 2),
    DataColumnConfig(name: 'Run', flex: 1),
  ];

  @override
  void dispose() {
    _palletController.dispose();
    _rackController.dispose();
    _actualQtyController.dispose();
    _palletFocus.dispose();
    _rackFocus.dispose();
    _actualQtyFocus.dispose();
    super.dispose();
  }

  // ---------------------------------------------------------------------------
  // Pallet scan
  // ---------------------------------------------------------------------------

  Future<void> _onPalletScanned(String value) async {
    final barcode = value.trim().toUpperCase();
    if (barcode.isEmpty) return;

    if (!mounted) return;
    setState(() => _isLoading = true);

    try {
      final info = await _service.loadPallet(barcode);

      if (!mounted) return;
      setState(() {
        _pallet = info;
        _palletScanned = true;
        _isLoading = false;
        _actualQtyController.text = info.totalQty.toStringAsFixed(0);
      });
      _actualQtyFocus.requestFocus();
    } catch (e) {
      if (!mounted) return;
      setState(() => _isLoading = false);
      showErrorDialog('Ralat membaca pallet: $e');
      _palletController.clear();
      _palletFocus.requestFocus();
    }
  }

  // ---------------------------------------------------------------------------
  // Rack scan
  // ---------------------------------------------------------------------------

  Future<void> _onRackScanned(String value) async {
    final barcode = value.trim().toUpperCase();
    if (barcode.isEmpty) return;

    if (!mounted) return;
    setState(() => _isLoading = true);

    try {
      await _service.validateRack(barcode);

      if (!mounted) return;
      setState(() {
        _rackScanned = true;
        _isLoading = false;
      });
      _actualQtyFocus.requestFocus();
    } catch (e) {
      if (!mounted) return;
      setState(() => _isLoading = false);
      showErrorDialog('$e');
      _rackController.clear();
      _rackFocus.requestFocus();
    }
  }

  // ---------------------------------------------------------------------------
  // Inbound
  // ---------------------------------------------------------------------------

  Future<void> _onInbound() async {
    if (!_palletScanned) {
      showErrorDialog('Sila imbas nombor pallet terlebih dahulu');
      return;
    }
    final rackNo = _rackController.text.trim().toUpperCase();
    if (rackNo.isEmpty || !_rackScanned) {
      showErrorDialog('Sila imbas nombor lokasi terlebih dahulu');
      return;
    }
    final actualQty = double.tryParse(_actualQtyController.text.trim()) ?? 0;
    if (actualQty <= 0) {
      showErrorDialog('Kuantiti tidak sah');
      return;
    }

    if (!mounted) return;
    setState(() => _isLoading = true);
    final empNo = Provider.of<AuthService>(context, listen: false).empNo ?? '';

    try {
      await _service.executeInbound(
        pallet: _pallet,
        rackNo: rackNo,
        actualQty: actualQty,
        empNo: empNo,
      );

      final stockRows =
          await _service.loadStockLocations(_pallet.pltNo);

      if (mounted) {
        setState(() => _stockRows = stockRows);
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Stok masuk berjaya'),
            backgroundColor: Colors.green,
          ),
        );
      }
      _onClear();
    } catch (e) {
      showErrorDialog('Ralat semasa stok masuk: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  // ---------------------------------------------------------------------------
  // Clear / Close
  // ---------------------------------------------------------------------------

  void _onClear() {
    if (!mounted) return;
    setState(() {
      _palletController.clear();
      _rackController.clear();
      _actualQtyController.clear();
      _palletScanned = false;
      _rackScanned = false;
      _pallet = PalletInfo.empty;
    });
    _palletFocus.requestFocus();
  }

  void _onClose() => Navigator.of(context).pop();

  // ---------------------------------------------------------------------------
  // Barcode scanner helpers
  // ---------------------------------------------------------------------------

  Future<void> _scanPallet() async {
    final result = await BarcodeScannerDialog.show(context, title: 'Imbas Pallet');
    if (result != null && result.isNotEmpty) {
      _palletController.text = result;
      await _onPalletScanned(result);
    }
  }

  Future<void> _scanRack() async {
    final result = await BarcodeScannerDialog.show(context, title: 'Imbas Lokasi');
    if (result != null && result.isNotEmpty) {
      _rackController.text = result;
      await _onRackScanned(result);
    }
  }

  // ---------------------------------------------------------------------------
  // QC field with colour-coded background (like Outbound DA PQS)
  // ---------------------------------------------------------------------------

  Widget _buildQcField() {
    final qs = (_pallet.qs ?? '').toUpperCase();
    final hasValue = qs.isNotEmpty;
    final isGood = qs == 'WHP';
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final cs = Theme.of(context).colorScheme;
    final bgColor = !hasValue
        ? cs.surfaceContainerHighest
        : isGood
            ? Colors.green
            : Colors.red;
    final borderColor = !hasValue
        ? (isDark ? const Color(0xFF546E7A) : const Color(0xFF616161))
        : bgColor;

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 5),
      decoration: BoxDecoration(
        color: bgColor,
        borderRadius: BorderRadius.circular(4),
        border: Border.all(color: borderColor, width: hasValue ? 2 : 1),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Text('QC',
              style: hasValue
                  ? _labelStyle.copyWith(color: Colors.white70)
                  : _labelStyle),
          Text(
            hasValue ? qs : '\u2013',
            style: hasValue
                ? GoogleFonts.robotoCondensed(
                    fontSize: 13,
                    fontWeight: FontWeight.bold,
                    color: Colors.white)
                : GoogleFonts.robotoCondensed(
                    fontSize: 12, color: const Color(0xFF424242)),
          ),
        ],
      ),
    );
  }

  // ---------------------------------------------------------------------------
  // Build
  // ---------------------------------------------------------------------------

  static final _labelStyle = GoogleFonts.robotoCondensed(
      fontSize: 11, fontWeight: FontWeight.w600);

  @override
  Widget build(BuildContext context) {
    final auth = Provider.of<AuthService>(context, listen: false);
    final btnStyle = ElevatedButton.styleFrom(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 8),
      textStyle: GoogleFonts.robotoCondensed(
        fontSize: 11, fontWeight: FontWeight.w600,
      ),
    );

    return AppScaffold(
      title: 'TST Stock In',
      body: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
        child: SingleChildScrollView(
          child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // --- User info ---
            Text(
              'User: ${auth.empNo ?? ''}@${auth.empName ?? ''}',
              style: GoogleFonts.robotoCondensed(fontSize: 11,
                  color: Theme.of(context).brightness == Brightness.dark
                      ? const Color(0xFF80CBC4) : Colors.blueGrey),
            ),
            const SizedBox(height: 4),

            // --- Pallet & Rack scan ---
            Row(children: [
              SizedBox(
                width: 140,
                child: ScanField(
                  controller: _palletController, focusNode: _palletFocus,
                  label: 'Pallet No', autofocus: true,
                  onSubmitted: _onPalletScanned, onScanPressed: _scanPallet,
                  filled: true, fillColor: Theme.of(context).brightness == Brightness.dark
                      ? const Color(0xFF343B47) : Colors.white,
                  style: GoogleFonts.robotoCondensed(fontSize: 13,
                      color: Theme.of(context).brightness == Brightness.dark
                          ? const Color(0xFFE4E8EE) : Colors.black),
                  labelStyle: GoogleFonts.robotoCondensed(fontSize: 12),
                  contentPadding: const EdgeInsets.symmetric(horizontal: 8, vertical: 8),
                  isDense: true,
                ),
              ),
              const SizedBox(width: 6),
              SizedBox(
                width: 90,
                child: ScanField(
                  controller: _rackController, focusNode: _rackFocus,
                  label: 'Rack No', enabled: _palletScanned,
                  onSubmitted: _onRackScanned,
                  onScanPressed: _palletScanned ? _scanRack : null,
                  filled: true, fillColor: Theme.of(context).brightness == Brightness.dark
                      ? const Color(0xFF343B47) : Colors.white,
                  style: GoogleFonts.robotoCondensed(fontSize: 13,
                      color: Theme.of(context).brightness == Brightness.dark
                          ? const Color(0xFFE4E8EE) : Colors.black),
                  labelStyle: GoogleFonts.robotoCondensed(fontSize: 12),
                  contentPadding: const EdgeInsets.symmetric(horizontal: 8, vertical: 8),
                  isDense: true,
                ),
              ),
            ]),
            const SizedBox(height: 4),

            // --- Info fields ---
            Row(children: [
              Expanded(flex: 2, child: LabeledTextField(
                vertical: true, label: 'Category', value: _pallet.category,
                labelStyle: _labelStyle,
              )),
              const SizedBox(width: 4),
              Expanded(flex: 3, child: LabeledTextField(
                vertical: true, label: 'Batch', value: _pallet.batch,
                labelStyle: _labelStyle,
              )),
            ]),
            const SizedBox(height: 4),
            Row(children: [
              Expanded(flex: 1, child: LabeledTextField(
                vertical: true, label: 'Run', value: _pallet.run,
                labelStyle: _labelStyle,
              )),
              const SizedBox(width: 4),
              Expanded(flex: 4, child: LabeledTextField(
                vertical: true, label: 'PCode', value: _pallet.pCode,
                labelStyle: _labelStyle,
              )),
            ]),
            const SizedBox(height: 4),
            Row(children: [
              Expanded(flex: 2, child: LabeledTextField(
                vertical: true, label: 'Qty',
                value: _palletScanned ? _pallet.totalQty.toStringAsFixed(0) : '',
                labelStyle: _labelStyle,
              )),
              const SizedBox(width: 4),
              Expanded(flex: 2, child: LabeledTextField(
                vertical: true, label: 'Actual',
                controller: _actualQtyController, focusNode: _actualQtyFocus,
                enabled: _palletScanned, keyboardType: TextInputType.number,
                labelStyle: _labelStyle,
              )),
              const SizedBox(width: 4),
              Expanded(flex: 1, child: LabeledTextField(
                vertical: true, label: 'Unit', value: _pallet.unit,
                labelStyle: _labelStyle,
              )),
            ]),
            const SizedBox(height: 4),
            Row(children: [
              Expanded(flex: 2, child: _buildQcField()),
              const SizedBox(width: 4),
              Expanded(flex: 3, child: LabeledTextField(
                vertical: true, label: 'PGroup', value: _pallet.pGroup,
                labelStyle: _labelStyle,
              )),
            ]),
            const SizedBox(height: 4),

            // --- Stock locations ListView ---
            SizedBox(
              height: 180,
              child: Card(
                margin: const EdgeInsets.only(bottom: 3),
                clipBehavior: Clip.hardEdge,
                child: DataListView(
                  columns: _stockColumns, rows: _stockRows,
                  headerTextStyle: GoogleFonts.robotoCondensed(fontSize: 11),
                  headerPadding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                ),
              ),
            ),

            // --- Clear | Close ---
            Padding(
              padding: const EdgeInsets.only(bottom: 3),
              child: Row(children: [
                Expanded(child: ElevatedButton(
                  onPressed: _isLoading ? null : _onClear,
                  style: btnStyle.copyWith(
                    backgroundColor: WidgetStatePropertyAll(Colors.orange),
                    foregroundColor: const WidgetStatePropertyAll(Colors.white),
                    minimumSize: const WidgetStatePropertyAll(Size(0, 30)),
                  ),
                  child: const Text('Clear'),
                )),
                const SizedBox(width: 4),
                Expanded(child: ElevatedButton(
                  onPressed: _isLoading ? null : _onClose,
                  style: btnStyle.copyWith(
                    backgroundColor: WidgetStatePropertyAll(Colors.grey.shade600),
                    foregroundColor: const WidgetStatePropertyAll(Colors.white),
                    minimumSize: const WidgetStatePropertyAll(Size(0, 30)),
                  ),
                  child: const Text('Close'),
                )),
              ]),
            ),

            // --- Primary INBOUND ---
            Padding(
              padding: const EdgeInsets.only(bottom: 4),
              child: SizedBox(
                width: double.infinity, height: 52,
                child: MouseRegion(
                  cursor: SystemMouseCursors.click,
                  child: ElevatedButton.icon(
                    onPressed: _isLoading ? null : _onInbound,
                    icon: _isLoading
                        ? const SizedBox(width: 20, height: 20,
                            child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                        : const Icon(Icons.move_to_inbox, size: 22),
                    label: Text('INBOUND',
                      style: GoogleFonts.robotoCondensed(
                        fontSize: 16, fontWeight: FontWeight.bold, letterSpacing: 1.5)),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.green.shade700,
                      foregroundColor: Colors.white,
                      disabledBackgroundColor: Colors.grey.shade300,
                      disabledForegroundColor: Colors.grey.shade500,
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                      elevation: 3,
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
        ),
      ),
    );
  }
}
