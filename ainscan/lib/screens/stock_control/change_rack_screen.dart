import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';

import '../../services/activity_log_service.dart';
import '../../services/auth_service.dart';
import '../../services/database_service.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/scan_field.dart';
import '../../widgets/barcode_scanner_dialog.dart';
import '../../config/app_constants.dart';
import '../../utils/dialog_mixin.dart';

/// Change Rack screen — converts frmChangeRack.vb
///
/// Moves stock between rack locations. User scans a pallet barcode,
/// the system shows the current location from IV_0250, then the user
/// scans a new rack barcode. On confirm, inventory is deducted from
/// the old location and added to the new location, with a log entry
/// written to TA_LOC0400.
class ChangeRackScreen extends StatefulWidget {
  const ChangeRackScreen({super.key});

  @override
  State<ChangeRackScreen> createState() => _ChangeRackScreenState();
}

class _ChangeRackScreenState extends State<ChangeRackScreen> with DialogMixin {
  final _db = DatabaseService();

  // Controllers
  final _palletController = TextEditingController();
  final _newRackController = TextEditingController();

  // Focus nodes
  final _palletFocus = FocusNode();
  final _newRackFocus = FocusNode();

  // Pallet info (read-only display)
  String _currentLocation = '';
  String _pCode = '';
  String _batch = '';
  String _run = '';
  String _qty = '';
  String _unit = '';

  // Internal state from IV_0250 row for the move operation
  double _onHandQty = 0;
  // Extra IV_0250 fields needed for INSERT at new location
  String _pGroup = '';
  String _pName = '';
  String _status = '';

  // Transaction number (SNo) for TA_LOC0400
  String _trxNo = '';

  bool _isLoading = false;

  @override
  void dispose() {
    _palletController.dispose();
    _newRackController.dispose();
    _palletFocus.dispose();
    _newRackFocus.dispose();
    super.dispose();
  }

  // ---------------------------------------------------------------------------
  // Pallet scan & lookup
  // ---------------------------------------------------------------------------

  Future<void> _onPalletSubmitted(String value) async {
    if (value.isEmpty) return;

    final len = value.length;
    if (len < AppConstants.palletMinLength ||
        len > AppConstants.palletMaxLength) {
      showErrorDialog('Nombor pallet tidak sah');
      _palletController.clear();
      _palletFocus.requestFocus();
      return;
    }

    final palletNo = len == 13
        ? value.substring(0, 12).toUpperCase()
        : value.toUpperCase();
    _palletController.text = palletNo;

    if (!mounted) return;
    setState(() => _isLoading = true);
    try {
      // Look up current location from IV_0250 where OnHand > 0
      final ivRows = await _db.query(
        "SELECT Loct, PCode, PGroup, PName, Batch, Run, OnHand, Unit, Status FROM IV_0250 "
        "WHERE Pallet=@Pallet AND OnHand>0",
        {'@Pallet': palletNo},
      );

      if (ivRows.isEmpty) {
        showErrorDialog('Tiada stok untuk pallet ini');
        _clearPalletFields();
        _palletController.clear();
        _palletFocus.requestFocus();
        return;
      }

      final row = ivRows.first;
      final onHand =
          double.tryParse((row['OnHand'] ?? 0).toString()) ?? 0;

      if (!mounted) return;
      setState(() {
        _currentLocation = (row['Loct'] ?? '').toString().trim();
        _pCode = (row['PCode'] ?? '').toString().trim();
        _batch = (row['Batch'] ?? '').toString().trim();
        _run = (row['Run'] ?? '').toString().trim();
        _qty = onHand.toStringAsFixed(0);
        _unit = (row['Unit'] ?? '').toString().trim();
        _onHandQty = onHand;
        _pGroup = (row['PGroup'] ?? '').toString().trim();
        _pName = (row['PName'] ?? '').toString().trim();
        _status = (row['Status'] ?? '').toString().trim();
      });

      // Generate transaction number if not yet set
      if (_trxNo.isEmpty) {
        await _generateTrxNo();
      }

      // Move focus to new rack field
      _newRackFocus.requestFocus();
    } catch (e) {
      showErrorDialog('Error: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  // ---------------------------------------------------------------------------
  // New rack validation & move
  // ---------------------------------------------------------------------------

  Future<void> _onNewRackSubmitted(String value) async {
    if (value.isEmpty) return;

    if (!mounted) return;
    setState(() => _isLoading = true);
    try {
      // Validate against BD_0010
      final rackRows = await _db.query(
        "SELECT Rack FROM BD_0010 WHERE Rack=@Rack",
        {'@Rack': value},
      );
      if (rackRows.isEmpty) {
        showErrorDialog('Nombor lokasi tidak sah');
        _newRackController.clear();
        _newRackFocus.requestFocus();
        return;
      }
      // New rack valid — user proceeds to OK button
    } catch (e) {
      showErrorDialog('Error: $e');
      _newRackFocus.requestFocus();
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  // ---------------------------------------------------------------------------
  // OK — execute the rack change
  // ---------------------------------------------------------------------------

  Future<void> _executeChange() async {
    final pallet = _palletController.text.trim();
    final newRack = _newRackController.text.trim();

    if (pallet.isEmpty) {
      showErrorDialog('Nombor pallet tidak sah');
      _palletFocus.requestFocus();
      return;
    }
    if (_currentLocation.isEmpty) {
      showErrorDialog('Tiada stok untuk pallet ini');
      _palletFocus.requestFocus();
      return;
    }
    if (newRack.isEmpty) {
      showErrorDialog('Nombor lokasi tidak sah');
      _newRackFocus.requestFocus();
      return;
    }
    if (newRack == _currentLocation) {
      showErrorDialog('Lokasi baru sama dengan lokasi semasa');
      _newRackController.clear();
      _newRackFocus.requestFocus();
      return;
    }

    if (!mounted) return;
    setState(() => _isLoading = true);
    try {
      // Validate new rack against BD_0010
      final rackRows = await _db.query(
        "SELECT Rack FROM BD_0010 WHERE Rack=@Rack",
        {'@Rack': newRack},
      );
      if (rackRows.isEmpty) {
        showErrorDialog('Nombor lokasi tidak sah');
        _newRackController.clear();
        _newRackFocus.requestFocus();
        return;
      }

      if (!mounted) return;
      final authService = Provider.of<AuthService>(context, listen: false);
      final now = DateTime.now();
      final dateStr = DateFormat('yyyy-MM-dd').format(now);
      final timeStr = DateFormat('HH:mm:ss').format(now);

      // 1. Deduct from old location (matches VB.NET WHERE with Batch, Run)
      // Read current row first to get OutputQty
      final oldRows = await _db.query(
        "SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run "
        "AND Pallet=@Pallet AND OnHand>0",
        {'@Loct': _currentLocation, '@Batch': _batch, '@Run': _run, '@Pallet': pallet},
      );
      if (oldRows.isNotEmpty) {
        final oldRow = oldRows.first;
        final oldOnHand = double.tryParse((oldRow['OnHand'] ?? 0).toString()) ?? 0;
        final oldOutputQty = double.tryParse((oldRow['OutputQty'] ?? 0).toString()) ?? 0;
        await _db.execute(
          "UPDATE IV_0250 SET OnHand=@OnHand, OutputQty=@OutputQty, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND OnHand>0",
          {
            '@OnHand': oldOnHand - _onHandQty,
            '@OutputQty': oldOutputQty + _onHandQty,
            '@EditUser': authService.empNo ?? '',
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Loct': _currentLocation,
            '@Batch': _batch,
            '@Run': _run,
            '@Pallet': pallet,
          },
        );
      }

      // 2. Add/update new location
      final existingNew = await _db.query(
        "SELECT * FROM IV_0250 WHERE Loct=@Loct AND Pallet=@Pallet",
        {'@Loct': newRack, '@Pallet': pallet},
      );

      if (existingNew.isNotEmpty) {
        // Update existing row (matches VB.NET)
        await _db.execute(
          "UPDATE IV_0250 SET OnHand=@OnHand, OutputQty=@OutputQty, Pallet=@Pallet, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet",
          {
            '@OnHand': _onHandQty,
            '@OutputQty': 0,
            '@Pallet': pallet,
            '@EditUser': authService.empNo ?? '',
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Loct': newRack,
            '@Batch': _batch,
            '@Run': _run,
          },
        );
      } else {
        // Insert new row at new location (matches VB.NET full column list)
        await _db.execute(
          "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,"
          "OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime) "
          "VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,"
          "0,@InputQty,0,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)",
          {
            '@Loct': newRack,
            '@PCode': _pCode,
            '@PGroup': _pGroup,
            '@Batch': _batch,
            '@PName': _pName,
            '@Unit': _unit,
            '@Run': _run,
            '@Status': _status,
            '@InputQty': _onHandQty,
            '@OnHand': _onHandQty,
            '@Pallet': pallet,
            '@AddUser': authService.empNo ?? '',
            '@AddDate': dateStr,
            '@AddTime': timeStr,
          },
        );
      }

      // 3. Log to TA_LOC0400 (matches VB.NET column names exactly)
      final existingLog = await _db.query(
        "SELECT COUNT(SNo) as cnt FROM TA_LOC0400 WHERE SNo=@SNo AND Rack=@Rack AND NRack=@NRack",
        {'@SNo': _trxNo, '@Rack': _currentLocation, '@NRack': newRack},
      );
      final logExists = existingLog.isNotEmpty &&
          (int.tryParse((existingLog.first['cnt'] ?? 0).toString()) ?? 0) > 0;

      if (!logExists) {
        await _db.execute(
          "INSERT INTO TA_LOC0400 "
          "(SNo,Rack,NRack,BN,Run,PCode,PName,PGroup,PltNo,Qty,Unit,Remark,AddUser,AddDate,AddTime) "
          "VALUES (@SNo,@Rack,@NRack,@BN,@Run,@PCode,@PName,@PGroup,@PltNo,@Qty,@Unit,@Remark,@AddUser,@AddDate,@AddTime)",
          {
            '@SNo': _trxNo,
            '@Rack': _currentLocation,
            '@NRack': newRack,
            '@BN': _batch,
            '@Run': _run,
            '@PCode': _pCode,
            '@PName': _pName,
            '@PGroup': _pGroup,
            '@PltNo': pallet,
            '@Qty': _onHandQty,
            '@Unit': _unit,
            '@Remark': 'Mobile Scanner',
            '@AddUser': authService.empNo ?? '',
            '@AddDate': dateStr,
            '@AddTime': timeStr,
          },
        );
      } else {
        await _db.execute(
          "UPDATE TA_LOC0400 SET Rack=@Rack, NRack=@NRack, Qty=@Qty, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE SNo=@SNo AND Rack=@Rack AND NRack=@NRack",
          {
            '@Qty': _onHandQty,
            '@SNo': _trxNo,
            '@Rack': _currentLocation,
            '@NRack': newRack,
            '@EditUser': authService.empNo ?? '',
            '@EditDate': dateStr,
            '@EditTime': timeStr,
          },
        );
      }

      // Success
      _clearForm();

      await ActivityLogService.log(
        action: 'CHANGE_RACK',
        empNo: authService.empNo ?? '',
        detail: 'Pallet: $pallet, $_currentLocation -> $newRack',
      );

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Lokasi berjaya ditukar'),
          duration: Duration(seconds: 2),
        ),
      );

      _palletFocus.requestFocus();
    } catch (e) {
      await ActivityLogService.logError(
        action: 'CHANGE_RACK',
        detail: 'Pallet: $pallet',
        errorMsg: '$e',
      );
      showErrorDialog('Error: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  // ---------------------------------------------------------------------------
  // Clear / helpers
  // ---------------------------------------------------------------------------

  void _clearPalletFields() {
    if (!mounted) return;
    setState(() {
      _currentLocation = '';
      _pCode = '';
      _batch = '';
      _run = '';
      _qty = '';
      _unit = '';
      _onHandQty = 0;
      _pGroup = '';
      _pName = '';
      _status = '';
    });
  }

  void _clearForm() {
    _palletController.clear();
    _newRackController.clear();
    _clearPalletFields();
    setState(() => _trxNo = '');
  }

  /// Generates a transaction number from SY_0040 (KeyCD2='52').
  /// Format: MG + 2-digit year + 4-digit sequence, e.g. MG260001
  Future<void> _generateTrxNo() async {
    try {
      final yearStr = DateTime.now().year.toString();
      final kCd1 = yearStr.substring(yearStr.length - 2);

      final rows = await _db.query(
        "SELECT MSEQ FROM SY_0040 WHERE KeyCD1=@KeyCD1 AND KeyCD2=@KeyCD2",
        {'@KeyCD1': kCd1, '@KeyCD2': '52'},
      );
      if (rows.isEmpty) {
        debugPrint('No sequence number for SY_0040 KeyCD2=52');
        return;
      }

      final currentSeq = int.tryParse((rows.first['MSEQ'] ?? 0).toString()) ?? 0;
      final newSeq = currentSeq + 1;

      await _db.execute(
        "UPDATE SY_0040 SET MSEQ=@MSEQ WHERE KeyCD1=@KeyCD1 AND KeyCD2=@KeyCD2",
        {'@MSEQ': newSeq, '@KeyCD1': kCd1, '@KeyCD2': '52'},
      );

      final seqPadded = currentSeq.toString().padLeft(4, '0');
      if (!mounted) return;
      setState(() {
        _trxNo = 'MG$kCd1$seqPadded';
      });
    } catch (e) {
      debugPrint('Error generating TrxNo: $e');
    }
  }



  Future<void> _scanBarcode(TextEditingController controller,
      {String title = 'Scan Barcode',
      Function(String)? onResult}) async {
    final result = await BarcodeScannerDialog.show(context, title: title);
    if (result != null && result.isNotEmpty) {
      controller.text = result;
      onResult?.call(result);
    }
  }

  // ---------------------------------------------------------------------------
  // Build
  // ---------------------------------------------------------------------------

  // ── Condensed text styles ──────────────────────────────────────────
  bool get _isDark => Theme.of(context).brightness == Brightness.dark;
  TextStyle get _cLabelStyle => GoogleFonts.robotoCondensed(
      fontSize: 10, color: _isDark ? const Color(0xFF80CBC4) : const Color(0xFF616161));
  TextStyle get _cValueStyle => GoogleFonts.robotoCondensed(
      fontSize: 13, color: _isDark ? const Color(0xFFE4E8EE) : const Color(0xFF424242));
  TextStyle get _cInputStyle => GoogleFonts.robotoCondensed(
      fontSize: 13, color: _isDark ? const Color(0xFFE4E8EE) : Colors.black);
  TextStyle get _cInputLabelStyle => GoogleFonts.robotoCondensed(
      fontSize: 12, color: _isDark ? const Color(0xFF80CBC4) : const Color(0xFF616161));

  TextStyle get _cSectionTitle => GoogleFonts.robotoCondensed(
    fontSize: 11,
    fontWeight: FontWeight.w600,
    color: Theme.of(context).brightness == Brightness.dark
        ? const Color(0xFF4DB6AC)
        : Theme.of(context).colorScheme.onSurface.withValues(alpha: 0.65),
  );

  @override
  Widget build(BuildContext context) {
    final auth = Provider.of<AuthService>(context, listen: false);

    final btnStyle = ElevatedButton.styleFrom(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 8),
      textStyle: GoogleFonts.robotoCondensed(
        fontSize: 11,
        fontWeight: FontWeight.w600,
      ),
    );

    return AppScaffold(
      title: 'Change Rack',
      body: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
        child: SingleChildScrollView(
          child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // ─── User info ──────────────────────────────────────
            Text(
              'User: ${auth.empNo ?? ''}@${auth.empName ?? ''}',
              style: GoogleFonts.robotoCondensed(
                  fontSize: 11,
                  color: Theme.of(context).brightness == Brightness.dark
                      ? const Color(0xFF80CBC4) : Colors.blueGrey),
            ),
            const SizedBox(height: 4),

            // ─── Card 1: Pallet Scan ────────────────────────────
            Card(
              child: Padding(
                padding: const EdgeInsets.all(8),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text('① Pallet', style: _cSectionTitle),
                    const SizedBox(height: 4),
                    ScanField(
                      controller: _palletController,
                      focusNode: _palletFocus,
                      label: 'Pallet No',
                      autofocus: true,
                      onSubmitted: _onPalletSubmitted,
                      onScanPressed: () => _scanBarcode(
                        _palletController,
                        title: 'Scan Pallet',
                        onResult: _onPalletSubmitted,
                      ),
                      filled: true,
                      fillColor: Theme.of(context).brightness == Brightness.dark
                          ? const Color(0xFF343B47)
                          : Colors.white,
                      style: _cInputStyle,
                      labelStyle: _cInputLabelStyle,
                      contentPadding: const EdgeInsets.symmetric(
                          horizontal: 8, vertical: 8),
                      isDense: true,
                    ),
                    const SizedBox(height: 4),
                    // Row 1: Current Location / PCode
                    Row(
                      children: [
                        Expanded(flex: 2, child: _buildReadOnly('Cur. Loct', _currentLocation)),
                        const SizedBox(width: 4),
                        Expanded(flex: 3, child: _buildReadOnly('PCode', _pCode)),
                      ],
                    ),
                    const SizedBox(height: 4),
                    // Row 2: Batch / Run / Qty
                    Row(
                      children: [
                        Expanded(flex: 3, child: _buildReadOnly('Batch', _batch)),
                        const SizedBox(width: 4),
                        Expanded(flex: 1, child: _buildReadOnly('Run', _run)),
                        const SizedBox(width: 4),
                        Expanded(flex: 1, child: _buildReadOnly('Qty', _qty)),
                      ],
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 4),

            // ─── Card 2: New Rack ───────────────────────────────
            Card(
              child: Padding(
                padding: const EdgeInsets.all(8),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text('② New Rack', style: _cSectionTitle),
                    const SizedBox(height: 4),
                    ScanField(
                      controller: _newRackController,
                      focusNode: _newRackFocus,
                      label: 'New Rack No',
                      onSubmitted: _onNewRackSubmitted,
                      onScanPressed: () => _scanBarcode(
                        _newRackController,
                        title: 'Scan New Rack',
                        onResult: _onNewRackSubmitted,
                      ),
                      filled: true,
                      fillColor: Theme.of(context).brightness == Brightness.dark
                          ? const Color(0xFF343B47)
                          : Colors.white,
                      style: _cInputStyle,
                      labelStyle: _cInputLabelStyle,
                      contentPadding: const EdgeInsets.symmetric(
                          horizontal: 8, vertical: 8),
                      isDense: true,
                    ),
                  ],
                ),
              ),
            ),

            const SizedBox(height: 16),

            // Loading indicator
            if (_isLoading)
              const Padding(
                padding: EdgeInsets.only(bottom: 2),
                child: LinearProgressIndicator(),
              ),

            // ─── Secondary buttons: Clear | Close ───────────────
            Padding(
              padding: const EdgeInsets.only(bottom: 3),
              child: Row(
                children: [
                  Expanded(
                    child: ElevatedButton(
                      onPressed: _isLoading ? null : _clearForm,
                      style: btnStyle.copyWith(
                        backgroundColor:
                            WidgetStatePropertyAll(Colors.orange),
                        foregroundColor:
                            const WidgetStatePropertyAll(Colors.white),
                        minimumSize:
                            const WidgetStatePropertyAll(Size(0, 30)),
                      ),
                      child: const Text('Clear'),
                    ),
                  ),
                  const SizedBox(width: 4),
                  Expanded(
                    child: ElevatedButton(
                      onPressed: _isLoading
                          ? null
                          : () => Navigator.of(context).pop(),
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

            // ─── Primary OK — large, glove-friendly ─────────────
            Padding(
              padding: const EdgeInsets.only(bottom: 4),
              child: SizedBox(
                width: double.infinity,
                height: 52,
                child: MouseRegion(
                  cursor: SystemMouseCursors.click,
                  child: ElevatedButton.icon(
                    onPressed: _isLoading ? null : _executeChange,
                    icon: _isLoading
                        ? const SizedBox(
                            width: 20,
                            height: 20,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                              color: Colors.white,
                            ),
                          )
                        : const Icon(Icons.swap_horiz, size: 22),
                    label: Text('CHANGE RACK',
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
      ),
    );
  }

  /// Read-only field with grey bg, near-white text (matches outbound DA).
  Widget _buildReadOnly(String label, String value) {
    final cs = Theme.of(context).colorScheme;
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 5),
      decoration: BoxDecoration(
        color: cs.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(4),
        border: Border.all(
          color: isDark ? const Color(0xFF546E7A) : const Color(0xFF616161),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: _cLabelStyle),
          Text(
            value.isEmpty ? '-' : value,
            style: _cValueStyle,
          ),
        ],
      ),
    );
  }
}
