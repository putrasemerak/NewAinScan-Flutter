import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../config/app_constants.dart';
import '../../services/activity_log_service.dart';
import '../../services/auth_service.dart';
import '../../services/database_service.dart';
import '../../services/local_database_service.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/barcode_scanner_dialog.dart';
import '../../widgets/data_list_view.dart';
import '../../widgets/scan_field.dart';
import '../utility/log_list_screen.dart';
import '../../utils/dialog_mixin.dart';

/// Rack In screen — converts frmStockIn_1.vb
///
/// Stocks items into rack locations (TST2→Rack or TST4→Rack for Kulim).
/// Flow:
/// 1. Scan pallet barcode → validate & load pallet details
/// 2. Scan rack barcode → validate rack location
/// 3. OK → deduct from transit, add to rack, log, close pallet card
class RackInScreen extends StatefulWidget {
  const RackInScreen({super.key});

  @override
  State<RackInScreen> createState() => _RackInScreenState();
}

class _RackInScreenState extends State<RackInScreen> with DialogMixin {
  final _db = DatabaseService();
  final _localDb = LocalDatabaseService();

  // Controllers
  final _palletController = TextEditingController();
  final _categoryController = TextEditingController();
  final _batchController = TextEditingController();
  final _runController = TextEditingController();
  final _pCodeController = TextEditingController();
  final _qtyController = TextEditingController();
  final _unitController = TextEditingController();
  final _rackController = TextEditingController();

  // Scroll controller
  final _scrollController = ScrollController();

  // Focus nodes
  final _palletFocus = FocusNode();
  final _rackFocus = FocusNode();

  // State
  bool _isLoading = false;
  bool _kulim = false;
  int _count = 0;
  List<Map<String, String>> _listRows = [];

  // ListView column config — mirrors VB.NET lvList columns
  static const List<DataColumnConfig> _columns = [
    DataColumnConfig(name: 'Loct', flex: 2),
    DataColumnConfig(name: 'OnHand', flex: 2),
    DataColumnConfig(name: 'PCode', flex: 3),
    DataColumnConfig(name: 'Batch', flex: 2),
    DataColumnConfig(name: 'Run', flex: 2),
  ];

  @override
  void initState() {
    super.initState();
    _palletFocus.requestFocus();
  }

  @override
  void dispose() {
    _palletController.dispose();
    _categoryController.dispose();
    _batchController.dispose();
    _runController.dispose();
    _pCodeController.dispose();
    _qtyController.dispose();
    _unitController.dispose();
    _rackController.dispose();
    _scrollController.dispose();
    _palletFocus.dispose();
    _rackFocus.dispose();
    super.dispose();
  }

  // ---------------------------------------------------------------------------
  // Barcode trimming — mirrors VB.NET Mid() logic
  // ---------------------------------------------------------------------------

  /// Trims scanned pallet barcode: 13→12, 9→8, 10→9, 8→7 chars.
  String _trimPalletBarcode(String bc) {
    final len = bc.length;
    if (len == 13) return bc.substring(0, 12).toUpperCase();
    if (len == 9) return bc.substring(0, 8).toUpperCase();
    if (len == 10) return bc.substring(0, 9).toUpperCase();
    if (len == 8) return bc.substring(0, 7).toUpperCase();
    return bc.toUpperCase();
  }

  /// Trims scanned rack barcode: 5→4, 6→5 chars.
  String _trimRackBarcode(String bc) {
    final len = bc.length;
    if (len == 5) return bc.substring(0, 4).toUpperCase();
    if (len == 6) return bc.substring(0, 5).toUpperCase();
    return bc.toUpperCase();
  }

  // ---------------------------------------------------------------------------
  // Clear fields — mirrors Body_Null()
  // ---------------------------------------------------------------------------

  void _bodyNull() {
    _palletController.clear();
    _categoryController.clear();
    _batchController.clear();
    _runController.clear();
    _pCodeController.clear();
    _qtyController.clear();
    _unitController.clear();
    _rackController.clear();
    _kulim = false;
  }

  // ---------------------------------------------------------------------------
  // Pallet scan handler
  // ---------------------------------------------------------------------------

  Future<void> _onPalletScanned(String value) async {
    final bc = value.trim();
    if (bc.isEmpty) return;

    // Accept pallet-length barcodes (7-10 chars: typed=7-9, scanned=8-10)
    if (bc.length < AppConstants.palletMinLength ||
        bc.length > AppConstants.palletMaxLength) {
      showErrorDialog('Nombor pallet tidak sah');
      return;
    }

    final palletNo = _trimPalletBarcode(bc);
    _palletController.text = palletNo;

    if (!mounted) return;
    setState(() => _isLoading = true);

    try {
      _kulim = false;

      // Check pallet exists in TA_PLT001
      final pltRows = await _db.query(
        "SELECT PCode FROM TA_PLT001 WHERE PltNo=@PltNo",
        {'@PltNo': palletNo.trim()},
      );

      String batch = '';
      String run = '';
      String pCode = '';

      if (pltRows.isEmpty) {
        // Check if Kulim product (IV_0250 at TST4)
        final kulimRows = await _db.query(
          "SELECT * FROM IV_0250 WHERE Pallet=@Pallet AND Loct='TST4' AND OnHand > 0",
          {'@Pallet': palletNo.trim()},
        );

        if (kulimRows.isEmpty) {
          showErrorDialog('Nombor pallet tidak sah atau sudah dimasukkan ke lokasi');
          _palletController.clear();
          _palletFocus.requestFocus();
          return;
        }

        // It is a Kulim product
        _kulim = true;
        batch = (kulimRows.first['Batch'] ?? '').toString().trim();
        run = (kulimRows.first['Run'] ?? '').toString().trim();
        pCode = (kulimRows.first['PCode'] ?? '').toString().trim();
        _batchController.text = batch;
        _runController.text = run;
        _pCodeController.text = pCode;
        _qtyController.text = (kulimRows.first['OnHand'] ?? 0).toString();
      } else {
        pCode = (pltRows.first['PCode'] ?? '').toString().trim();

        // Read Batch and Cycle from TA_PLT001
        final batchRows = await _db.query(
          "SELECT Batch, Cycle FROM TA_PLT001 WHERE PltNo=@PltNo",
          {'@PltNo': palletNo},
        );
        if (batchRows.isNotEmpty) {
          batch = (batchRows.first['Batch'] ?? '').toString().trim();
          run = (batchRows.first['Cycle'] ?? '').toString().trim();
        }
      }

      // Check pallet received (not for Kulim)
      if (!_kulim) {
        final transferCount1 = await _db.executeScalar(
          "SELECT COUNT(PltNo) FROM TA_PLT001 WHERE PltNo=@PltNo AND TStatus='Transfer'",
          {'@PltNo': palletNo},
        );
        if ((transferCount1 ?? 0) == 0) {
          final transferCount2 = await _db.executeScalar(
            "SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Pallet=@Pallet AND TStatus='Transfer'",
            {'@Pallet': palletNo},
          );
          if ((transferCount2 ?? 0) == 0) {
            showErrorDialog('Pallet belum diterima');
            _bodyNull();
            _palletFocus.requestFocus();
            return;
          }
        }

        // Check if pallet already closed (Status='C')
        final closedCount1 = await _db.executeScalar(
          "SELECT COUNT(PltNo) FROM TA_PLT001 WHERE PltNo=@PltNo AND TStatus='Transfer' AND Status='C'",
          {'@PltNo': palletNo},
        );
        if ((closedCount1 ?? 0) != 0) {
          final closedCount2 = await _db.executeScalar(
            "SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Pallet=@Pallet AND TStatus='Transfer' AND Status='C'",
            {'@Pallet': palletNo},
          );
          if ((closedCount2 ?? 0) != 0) {
            showErrorDialog('Pallet sudah dimasukkan ke lokasi');
            _bodyNull();
            _palletFocus.requestFocus();
            return;
          }
        }
      }

      // Check stock summary in PD_0800
      final pdCount = await _db.executeScalar(
        "SELECT COUNT(Batch) FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {'@Batch': batch, '@Run': run, '@PCode': pCode},
      );
      if ((pdCount ?? 0) == 0) {
        showErrorDialog('Tiada Ringkasan Stok - PD_0800');
        _bodyNull();
        _palletFocus.requestFocus();
        return;
      }

      // Check loose pallet (TA_PLL001)
      final looseRows = await _db.query(
        "SELECT * FROM TA_PLL001 WHERE PltNo=@PltNo",
        {'@PltNo': palletNo},
      );

      if (looseRows.isEmpty) {
        // NORMAL pallet
        _categoryController.text = 'NORMAL';

        if (!_kulim) {
          final normalRows = await _db.query(
            "SELECT Batch, PCode, FullQty, Unit, lsQty, Cycle FROM TA_PLT001 WHERE PltNo=@PltNo",
            {'@PltNo': palletNo.trim()},
          );
          if (normalRows.isNotEmpty) {
            final row = normalRows.first;
            _batchController.text = (row['Batch'] ?? '').toString().trim();
            _runController.text = (row['Cycle'] ?? '').toString().trim();
            _pCodeController.text = (row['PCode'] ?? '').toString().trim();
            final fullQty = toDouble(row['FullQty']);
            final lsQty = toDouble(row['lsQty']);
            _qtyController.text = (fullQty + lsQty).toString();
            _unitController.text = (row['Unit'] ?? '').toString().trim();
          }
        }
        // For Kulim normal pallets, fields were already set above
      } else {
        // LOOSE pallet
        _categoryController.text = 'LOOSE';

        // Read PCode/Unit from TA_PLT001
        final plt001Rows = await _db.query(
          "SELECT PCode, Unit FROM TA_PLT001 WHERE PltNo=@PltNo",
          {'@PltNo': palletNo},
        );
        if (plt001Rows.isNotEmpty) {
          _pCodeController.text =
              (plt001Rows.first['PCode'] ?? '').toString().trim();
          _unitController.text =
              (plt001Rows.first['Unit'] ?? '').toString().trim();
        }

        _batchController.text =
            (looseRows.first['Batch'] ?? '').toString().trim();

        // Build run string and sum qty from all loose entries
        double looseQty = 0;
        final runParts = <String>[];
        for (final looseRow in looseRows) {
          runParts.add((looseRow['Run'] ?? '').toString().trim());
          looseQty += toDouble(looseRow['Qty']);
        }
        _runController.text = runParts.join(',');
        _qtyController.text = looseQty.toString();
      }

      _rackFocus.requestFocus();
    } catch (e) {
      showErrorDialog('Error: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  // ---------------------------------------------------------------------------
  // Rack scan handler
  // ---------------------------------------------------------------------------

  Future<void> _onRackScanned(String value) async {
    final bc = value.trim();
    if (bc.isEmpty) return;

    if (_palletController.text.isEmpty) {
      showErrorDialog('Sila Imbas no Pallet dahulu');
      _palletFocus.requestFocus();
      return;
    }

    if (!mounted) return;
    setState(() => _isLoading = true);

    try {
      // Try exact value first (manual entry / camera scan without check digit)
      String rackNo = bc.toUpperCase();
      var rackRows = await _db.query(
        "SELECT Rack FROM BD_0010 WHERE Rack=@Rack",
        {'@Rack': rackNo},
      );

      // If not found, try trimmed value (hardware scanner with trailing check digit)
      if (rackRows.isEmpty) {
        final trimmed = _trimRackBarcode(bc);
        if (trimmed != rackNo) {
          rackRows = await _db.query(
            "SELECT Rack FROM BD_0010 WHERE Rack=@Rack",
            {'@Rack': trimmed},
          );
          if (rackRows.isNotEmpty) {
            rackNo = trimmed;
          }
        }
      }

      _rackController.text = rackNo;

      if (rackRows.isEmpty) {
        showErrorDialog('Nombor lokasi tidak sah');
        _rackController.clear();
        _rackFocus.requestFocus();
        return;
      }
      // Rack valid — no further scan needed, user presses OK
    } catch (e) {
      showErrorDialog('Error: $e');
      _rackFocus.requestFocus();
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  // ---------------------------------------------------------------------------
  // OK button — dispatch to Normal or Loose
  // ---------------------------------------------------------------------------

  Future<void> _onOkPressed() async {
    if (_palletController.text.isEmpty) {
      showErrorDialog('Sila Imbas no Pallet dahulu');
      _palletFocus.requestFocus();
      return;
    }
    if (_rackController.text.isEmpty) {
      showErrorDialog('Sila imbas no lokasi');
      _rackFocus.requestFocus();
      return;
    }

    if (!mounted) return;
    setState(() => _isLoading = true);

    final empNo = Provider.of<AuthService>(context, listen: false).empNo ?? '';

    try {
      if (_categoryController.text == 'LOOSE') {
        await _loosePallet();
      } else {
        await _normalPallet();
      }
      await ActivityLogService.log(
        action: 'RACKIN_CONFIRM',
        empNo: empNo,
        detail: 'Pallet: ${_palletController.text.trim()}, '
            'Rack: ${_rackController.text.trim()}, Type: ${_categoryController.text}',
      );
      _palletFocus.requestFocus();
    } catch (e) {
      await ActivityLogService.logError(
        action: 'RACKIN_CONFIRM',
        empNo: empNo,
        detail: 'Pallet: ${_palletController.text.trim()}',
        errorMsg: '$e',
      );
      showErrorDialog('Error: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  // ---------------------------------------------------------------------------
  // Normal pallet logic — mirrors Normal_Pallet()
  // ---------------------------------------------------------------------------

  Future<void> _normalPallet() async {
    final palletNo = _palletController.text.trim();
    final rackNo = _rackController.text.trim().toUpperCase();
    final batchNo = _batchController.text.trim();
    final runNo = _runController.text.trim();
    final pCodeNo = _pCodeController.text.trim();
    final qty = double.tryParse(_qtyController.text) ?? 0;
    final auth = Provider.of<AuthService>(context, listen: false);
    final userStr = 'User : ${auth.empNo}@${auth.empName}';
    final now = DateTime.now();
    final dateStr = DateFormat('yyyy-MM-dd').format(now);
    final timeStr = DateFormat('yyyy-MM-dd HH:mm:ss').format(now);

    // Check rack already has stock
    final rackStockCount = await _db.executeScalar(
      "SELECT COUNT(Loct) FROM IV_0250 WHERE Loct=@Loct AND OnHand > 0",
      {'@Loct': rackNo},
    );
    if ((_toInt(rackStockCount)) > 0) {
      final proceed = await _showConfirmDialog(
        'Lokasi ini sudah ada Produk',
        'Teruskan rack-in di lokasi ini?',
      );
      if (!proceed) {
        _rackController.clear();
        return;
      }
    }

    // Check PD_0800
    final pdCount = await _db.executeScalar(
      "SELECT COUNT(Batch) FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
      {'@Batch': batchNo, '@Run': runNo, '@PCode': pCodeNo},
    );
    if ((_toInt(pdCount)) == 0) {
      showErrorDialog('Tiada ringkasan stok dalam PD_0800');
      _bodyNull();
      _palletFocus.requestFocus();
      return;
    }

    // Deduct from transit location (TST4 for Kulim, TST2 otherwise)
    final transitLoct = _kulim
        ? AppConstants.locationTST4
        : AppConstants.locationTST2;

    final transitRows = await _db.query(
      "SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND OnHand > 0",
      {
        '@Loct': transitLoct,
        '@Batch': batchNo,
        '@Run': runNo,
        '@Pallet': palletNo,
      },
    );

    if (transitRows.isEmpty) {
      showErrorDialog(_kulim ? 'Tiada stok di TST4' : 'Tiada stok di TST2');
      return;
    }

    final transitRow = transitRows.first;
    final oriOutputQty = toDouble(transitRow['OutputQty']);
    final oriOnHand = toDouble(transitRow['OnHand']);

    await _db.execute(
      "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=@OnHand, EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime"
      " WHERE Loct=@Loct AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND OnHand > 0",
      {
        '@OutputQty': oriOutputQty + qty,
        '@OnHand': oriOnHand - qty,
        '@EditUser': userStr,
        '@EditDate': dateStr,
        '@EditTime': timeStr,
        '@Loct': transitLoct,
        '@Pallet': palletNo,
        '@Batch': batchNo,
        '@Run': runNo,
      },
    );

    // Register rack location in IV_0250
    final rackExisting = await _db.query(
      "SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet",
      {
        '@Loct': rackNo,
        '@Batch': batchNo,
        '@Run': runNo,
        '@Pallet': palletNo,
      },
    );

    if (rackExisting.isEmpty) {
      // Insert new rack location
      await _db.execute(
        "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
        " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)",
        {
          '@Loct': rackNo,
          '@PCode': (transitRow['PCode'] ?? '').toString(),
          '@PGroup': (transitRow['PGroup'] ?? '').toString(),
          '@Batch': batchNo,
          '@PName': (transitRow['PName'] ?? '').toString(),
          '@Unit': (transitRow['Unit'] ?? '').toString(),
          '@Run': (transitRow['Run'] ?? '').toString(),
          '@Status': (transitRow['Status'] ?? '').toString(),
          '@OpenQty': 0,
          '@InputQty': qty,
          '@OutputQty': 0,
          '@OnHand': qty,
          '@Pallet': palletNo,
          '@AddUser': userStr,
          '@AddDate': dateStr,
          '@AddTime': timeStr,
        },
      );
    } else {
      // Update existing rack location
      final existOnHand = toDouble(rackExisting.first['OnHand']);
      final existInputQty = toDouble(rackExisting.first['InputQty']);
      await _db.execute(
        "UPDATE IV_0250 SET OnHand=@OnHand, InputQty=@InputQty, Pallet=@Pallet, EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime"
        " WHERE Loct=@Rack AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet",
        {
          '@OnHand': existOnHand + qty,
          '@InputQty': existInputQty + qty,
          '@Pallet': palletNo,
          '@EditUser': userStr,
          '@EditDate': dateStr,
          '@EditTime': timeStr,
          '@Rack': rackNo,
          '@Batch': batchNo,
          '@Run': runNo,
        },
      );
    }

    // Close pallet card (not for Kulim)
    if (!_kulim) {
      final plt001Count = await _db.executeScalar(
        "SELECT COUNT(Status) FROM TA_PLT001 WHERE PltNo=@PltNo",
        {'@PltNo': palletNo},
      );
      if ((_toInt(plt001Count)) > 0) {
        await _db.execute(
          "UPDATE TA_PLT001 SET Status='C' WHERE PltNo=@PltNo",
          {'@PltNo': palletNo},
        );
      }

      final plt002Count = await _db.executeScalar(
        "SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Pallet=@Pallet",
        {'@Pallet': palletNo},
      );
      if ((_toInt(plt002Count)) > 0) {
        await _db.execute(
          "UPDATE TA_PLT002 SET Status='C' WHERE Pallet=@Pallet",
          {'@Pallet': palletNo},
        );
      }
    }

    // Calculate rack balance and update PD_0800
    try {
      final rackBalRows = await _db.query(
        "SELECT SUM(OnHand) AS Qty FROM IV_0250 WHERE Batch=@Batch AND Run=@Run AND OnHand > 0 GROUP BY Batch, Run",
        {'@Batch': batchNo, '@Run': runNo},
      );
      final wRackBal =
          rackBalRows.isNotEmpty ? toDouble(rackBalRows.first['Qty']) : 0.0;

      final pd0800Rows = await _db.query(
        "SELECT Rack_In FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {'@Batch': batchNo, '@Run': runNo, '@PCode': pCodeNo},
      );

      if (pd0800Rows.isNotEmpty) {
        final existRackIn = toDouble(pd0800Rows.first['Rack_In']);
        if (_kulim) {
          await _db.execute(
            "UPDATE PD_0800 SET Rack_In=@Rack_In, SORack=@SORack, Balance=@Balance WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
            {
              '@Rack_In': existRackIn + qty,
              '@SORack': wRackBal,
              '@Balance': wRackBal,
              '@Batch': batchNo,
              '@Run': runNo,
              '@PCode': pCodeNo,
            },
          );
        } else {
          await _db.execute(
            "UPDATE PD_0800 SET Rack_In=@Rack_In, SORack=@SORack WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
            {
              '@Rack_In': existRackIn + qty,
              '@SORack': wRackBal,
              '@Batch': batchNo,
              '@Run': runNo,
              '@PCode': pCodeNo,
            },
          );
        }
      }
    } catch (e) {
      showErrorDialog('Error while updating PD_0800: $e');
    }

    // Log to TA_LOC0600
    await _upsertLoc0600(
      palletNo: palletNo,
      rackNo: rackNo,
      batchNo: batchNo,
      runNo: runNo,
      pCodeNo: pCodeNo,
      pName: (transitRow['PName'] ?? '').toString(),
      pGroup: (transitRow['PGroup'] ?? '').toString(),
      qty: qty,
      unit: (transitRow['Unit'] ?? '').toString(),
      ref: 1,
      userStr: userStr,
    );

    // Log to local SQLite
    await _insertLocalLog(
      palletNo: palletNo,
      rackNo: rackNo,
      batchNo: batchNo,
      runNo: runNo,
      pCodeNo: pCodeNo,
      qty: qty,
      userStr: userStr,
    );

    // Refresh ListView with stock locations for this pallet
    await _loadStockLocations(palletNo);

    _bodyNull();
  }

  // ---------------------------------------------------------------------------
  // Loose pallet logic — mirrors Loose_Pallet()
  // ---------------------------------------------------------------------------

  Future<void> _loosePallet() async {
    final palletNo = _palletController.text.trim();
    final rackNo = _rackController.text.trim().toUpperCase();
    final batchNo = _batchController.text.trim();
    final pCodeNo = _pCodeController.text.trim();
    final auth = Provider.of<AuthService>(context, listen: false);
    final userStr = 'User : ${auth.empNo}@${auth.empName}';
    final now = DateTime.now();
    final dateStr = DateFormat('yyyy-MM-dd').format(now);
    final timeStr = DateFormat('yyyy-MM-dd HH:mm:ss').format(now);

    // Clear list
    if (!mounted) return;
    setState(() => _listRows = []);

    // Read loose pallet entries
    final looseRows = await _db.query(
      "SELECT * FROM TA_PLL001 WHERE PltNo=@PltNo",
      {'@PltNo': palletNo},
    );

    for (int i = 0; i < looseRows.length; i++) {
      final looseRow = looseRows[i];
      final looseRun = (looseRow['Run'] ?? '').toString().trim();
      final looseQty = toDouble(looseRow['Qty']);

      // Check PD_0800 for each run
      final pdCount = await _db.executeScalar(
        "SELECT COUNT(Batch) FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {'@Batch': batchNo, '@Run': looseRun, '@PCode': pCodeNo},
      );
      if ((_toInt(pdCount)) == 0) {
        showErrorDialog('Tiada ringkasan stok dalam PD_0800');
        _bodyNull();
        _palletFocus.requestFocus();
        return;
      }

      // Minus TST2
      final tst2Rows = await _db.query(
        "SELECT * FROM IV_0250 WHERE Loct='TST2' AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND OnHand > 0",
        {'@Batch': batchNo, '@Run': looseRun, '@Pallet': palletNo},
      );

      if (tst2Rows.isEmpty) {
        showErrorDialog('Tiada stok di TST2');
        return;
      }

      final tst2Row = tst2Rows.first;
      final oriOutputQty = toDouble(tst2Row['OutputQty']);
      final oriOnHand = toDouble(tst2Row['OnHand']);

      await _db.execute(
        "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=@OnHand, EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime"
        " WHERE Loct='TST2' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND OnHand > 0",
        {
          '@OutputQty': oriOutputQty + looseQty,
          '@OnHand': oriOnHand - looseQty,
          '@EditUser': userStr,
          '@EditDate': dateStr,
          '@EditTime': timeStr,
          '@Pallet': palletNo,
          '@Batch': batchNo,
          '@Run': looseRun,
        },
      );

      // Register rack location
      final rackExisting = await _db.query(
        "SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet",
        {
          '@Loct': rackNo,
          '@Batch': batchNo,
          '@Run': looseRun,
          '@Pallet': palletNo,
        },
      );

      if (rackExisting.isEmpty) {
        await _db.execute(
          "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
          " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,@OpenQty,@InputQty,@OutputQty,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)",
          {
            '@Loct': rackNo,
            '@PCode': (tst2Row['PCode'] ?? '').toString(),
            '@PGroup': (tst2Row['PGroup'] ?? '').toString(),
            '@Batch': batchNo,
            '@PName': (tst2Row['PName'] ?? '').toString(),
            '@Unit': (tst2Row['Unit'] ?? '').toString(),
            '@Run': looseRun,
            '@Status': (tst2Row['Status'] ?? '').toString(),
            '@OpenQty': 0,
            '@InputQty': looseQty,
            '@OutputQty': 0,
            '@OnHand': looseQty,
            '@Pallet': palletNo,
            '@AddUser': userStr,
            '@AddDate': dateStr,
            '@AddTime': timeStr,
          },
        );
      } else {
        final existOnHand = toDouble(rackExisting.first['OnHand']);
        final existInputQty = toDouble(rackExisting.first['InputQty']);
        await _db.execute(
          "UPDATE IV_0250 SET OnHand=@OnHand, InputQty=@InputQty, Pallet=@Pallet, EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime"
          " WHERE Loct=@Rack AND PCode=@PCode AND Batch=@Batch AND Run=@Run AND OnHand > 0 AND Pallet=@Pallet",
          {
            '@OnHand': existOnHand + looseQty,
            '@InputQty': existInputQty + looseQty,
            '@Pallet': palletNo,
            '@EditUser': userStr,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Rack': rackNo,
            '@PCode': pCodeNo,
            '@Batch': batchNo,
            '@Run': looseRun,
          },
        );
      }

      // Log to TA_LOC0600
      await _upsertLoc0600(
        palletNo: palletNo,
        rackNo: rackNo,
        batchNo: batchNo,
        runNo: looseRun,
        pCodeNo: pCodeNo,
        pName: (tst2Row['PName'] ?? '').toString(),
        pGroup: (tst2Row['PGroup'] ?? '').toString(),
        qty: looseQty,
        unit: (tst2Row['Unit'] ?? '').toString(),
        ref: 2,
        userStr: userStr,
      );

      // Calculate rack balance and update PD_0800
      try {
        final rackBalRows = await _db.query(
          "SELECT SUM(OnHand) AS Qty FROM IV_0250 WHERE Batch=@Batch AND Run=@Run AND OnHand > 0 GROUP BY OnHand",
          {'@Batch': batchNo, '@Run': looseRun},
        );
        final wRackBal =
            rackBalRows.isNotEmpty ? toDouble(rackBalRows.first['Qty']) : 0.0;

        final pd0800Rows = await _db.query(
          "SELECT Rack_In FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
          {'@Batch': batchNo, '@Run': looseRun, '@PCode': pCodeNo},
        );

        if (pd0800Rows.isNotEmpty) {
          final existRackIn = toDouble(pd0800Rows.first['Rack_In']);
          await _db.execute(
            "UPDATE PD_0800 SET Rack_In=@Rack_In, SORack=@SORack WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
            {
              '@Rack_In': existRackIn + looseQty,
              '@SORack': wRackBal,
              '@Batch': batchNo,
              '@Run': looseRun,
              '@PCode': pCodeNo,
            },
          );
        }
      } catch (e) {
        showErrorDialog('Error while updating PD_0800: $e');
      }

      // Log to local SQLite
      await _insertLocalLog(
        palletNo: palletNo,
        rackNo: rackNo,
        batchNo: batchNo,
        runNo: looseRun,
        pCodeNo: pCodeNo,
        qty: looseQty,
        userStr: userStr,
      );
    }

    // Refresh ListView
    await _loadStockLocations(palletNo);

    // Close pallet card
    try {
      final plt001Count = await _db.executeScalar(
        "SELECT COUNT(PltNo) FROM TA_PLT001 WHERE PltNo=@PltNo",
        {'@PltNo': palletNo},
      );
      if ((_toInt(plt001Count)) > 0) {
        await _db.execute(
          "UPDATE TA_PLT001 SET Status='C' WHERE PltNo=@PltNo",
          {'@PltNo': palletNo},
        );
      }

      final plt002Count = await _db.executeScalar(
        "SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Pallet=@Pallet",
        {'@Pallet': palletNo},
      );
      if ((_toInt(plt002Count)) > 0) {
        await _db.execute(
          "UPDATE TA_PLT002 SET Status='C' WHERE Pallet=@Pallet",
          {'@Pallet': palletNo},
        );
      }
    } catch (e) {
      showErrorDialog('Error while closing pallet card: $e');
    }

    _bodyNull();
  }

  // ---------------------------------------------------------------------------
  // Shared helpers
  // ---------------------------------------------------------------------------

  /// Upsert inbound log to TA_LOC0600
  Future<void> _upsertLoc0600({
    required String palletNo,
    required String rackNo,
    required String batchNo,
    required String runNo,
    required String pCodeNo,
    required String pName,
    required String pGroup,
    required double qty,
    required String unit,
    required int ref,
    required String userStr,
  }) async {
    final now = DateTime.now();
    final dateStr = DateFormat('yyyy-MM-dd').format(now);
    final timeStr = DateFormat('yyyy-MM-dd HH:mm:ss').format(now);

    try {
      final logRows = await _db.query(
        "SELECT * FROM TA_LOC0600 WHERE Pallet=@Pallet AND Rack=@Rack AND Batch=@Batch AND Run=@Run",
        {
          '@Pallet': palletNo,
          '@Rack': rackNo,
          '@Batch': batchNo,
          '@Run': runNo,
        },
      );

      if (logRows.isNotEmpty) {
        await _db.execute(
          "UPDATE TA_LOC0600 SET Pallet=@Pallet, Rack=@Rack, Batch=@Batch, Run=@Run, PCode=@PCode, PName=@PName, "
          "PGroup=@PGroup, Qty=@Qty, Unit=@Unit, Ref=@Ref, EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime"
          " WHERE Pallet=@Pallet AND Rack=@Rack AND Batch=@Batch AND Run=@Run",
          {
            '@Pallet': palletNo,
            '@Rack': rackNo,
            '@Batch': batchNo,
            '@Run': runNo,
            '@PCode': pCodeNo,
            '@PName': pName,
            '@PGroup': pGroup,
            '@Qty': qty,
            '@Unit': unit,
            '@Ref': ref,
            '@EditUser': userStr,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
          },
        );
      } else {
        await _db.execute(
          "INSERT INTO TA_LOC0600 (Pallet,Rack,Batch,Run,PCode,PName,PGroup,Qty,Unit,Ref,AddUser,AddDate,AddTime)"
          " VALUES (@Pallet,@Rack,@Batch,@Run,@PCode,@PName,@PGroup,@Qty,@Unit,@Ref,@AddUser,@AddDate,@AddTime)",
          {
            '@Pallet': palletNo,
            '@Rack': rackNo,
            '@Batch': batchNo,
            '@Run': runNo,
            '@PCode': pCodeNo,
            '@PName': pName,
            '@PGroup': pGroup,
            '@Qty': qty,
            '@Unit': unit,
            '@Ref': ref,
            '@AddUser': userStr,
            '@AddDate': dateStr,
            '@AddTime': timeStr,
          },
        );
      }
    } catch (e) {
      showErrorDialog('Error while updating TA_LOC0600: $e');
    }
  }

  /// Insert local SQLite log
  Future<void> _insertLocalLog({
    required String palletNo,
    required String rackNo,
    required String batchNo,
    required String runNo,
    required String pCodeNo,
    required double qty,
    required String userStr,
  }) async {
    try {
      final now = DateTime.now();
      await _localDb.insertRackInLog({
        'PalletNo': palletNo,
        'RackNo': rackNo,
        'PCode': pCodeNo,
        'PName': '',
        'Batch': batchNo,
        'Run': runNo,
        'PGroup': '',
        'Qty': qty,
        'Unit': _unitController.text,
        'User': userStr,
        'Date': DateFormat('yyyy-MM-dd').format(now),
        'Time': DateFormat('HH:mm:ss').format(now),
      });
      _count++;
    } catch (e) {
      showErrorDialog('Error while updating scanner log: $e');
    }
  }

  /// Load stock locations for a pallet into the DataListView.
  Future<void> _loadStockLocations(String palletNo) async {
    try {
      final rows = await _db.query(
        "SELECT Loct, OnHand, PCode, Batch, Run FROM IV_0250 WHERE Pallet=@Pallet",
        {'@Pallet': palletNo},
      );
      if (!mounted) return;
      setState(() {
        _listRows = rows.map((row) {
          return {
            'Loct': (row['Loct'] ?? '').toString(),
            'OnHand': (row['OnHand'] ?? '').toString(),
            'PCode': (row['PCode'] ?? '').toString(),
            'Batch': (row['Batch'] ?? '').toString(),
            'Run': (row['Run'] ?? '').toString(),
          };
        }).toList();
      });
    } catch (e) {
      showErrorDialog('Error while loading stock locations: $e');
    }
  }

  // ---------------------------------------------------------------------------
  // UI helpers
  // ---------------------------------------------------------------------------

  double toDouble(dynamic val) {
    if (val == null) return 0.0;
    if (val is num) return val.toDouble();
    return double.tryParse(val.toString()) ?? 0.0;
  }

  int _toInt(dynamic val) {
    if (val == null) return 0;
    if (val is int) return val;
    if (val is num) return val.toInt();
    return int.tryParse(val.toString()) ?? 0;
  }

  /// Shows a two-step confirm dialog (product exists → continue?).
  /// Returns true if user confirms, false if cancelled.
  Future<bool> _showConfirmDialog(String message, String confirmMessage) async {
    if (!mounted) return false;

    // First dialog: inform about existing product
    final firstResult = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(false),
            child: const Text('Cancel'),
          ),
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(true),
            child: const Text('OK'),
          ),
        ],
      ),
    );

    if (firstResult != true) {
      if (mounted) {
        showDialog(
          context: context,
          builder: (ctx) => AlertDialog(
            content: const Text('Sila imbas lokasi lain'),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(ctx).pop(),
                child: const Text('OK'),
              ),
            ],
          ),
        );
      }
      return false;
    }

    if (!mounted) return false;

    // Second dialog: confirm continuation
    final secondResult = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        content: Text(confirmMessage),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(false),
            child: const Text('Cancel'),
          ),
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(true),
            child: const Text('OK'),
          ),
        ],
      ),
    );

    return secondResult == true;
  }

  Future<void> _scanPalletBarcode() async {
    final result = await BarcodeScannerDialog.show(
      context,
      title: 'Scan Pallet',
    );
    if (result != null && result.isNotEmpty) {
      await _onPalletScanned(result);
    }
  }

  Future<void> _scanRackBarcode() async {
    final result = await BarcodeScannerDialog.show(
      context,
      title: 'Scan Rack',
    );
    if (result != null && result.isNotEmpty) {
      await _onRackScanned(result);
    }
  }

  void _onClearPressed() {
    _bodyNull();
    if (!mounted) return;
    setState(() => _listRows = []);
    _palletFocus.requestFocus();
  }

  void _onListPressed() {
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => const LogListScreen(formType: 'S1'),
      ),
    );
  }

  // ── Condensed text styles (matching DA Confirmation page) ─────────
  static final _cValue = GoogleFonts.robotoCondensed(fontSize: 13);

  TextStyle get _cLabel => GoogleFonts.robotoCondensed(
    fontSize: 10,
    color: Theme.of(context).brightness == Brightness.dark
        ? const Color(0xFF80CBC4)
        : Theme.of(context).colorScheme.onSurface.withValues(alpha: 0.55),
  );

  // ---------------------------------------------------------------------------
  // Build
  // ---------------------------------------------------------------------------

  @override
  Widget build(BuildContext context) {
    final auth = Provider.of<AuthService>(context, listen: false);
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
      title: 'Rack In',
      body: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        child: SingleChildScrollView(
          child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // User info
            Text(
              'User: ${auth.empNo ?? ''}@${auth.empName ?? ''}',
              style: GoogleFonts.robotoCondensed(
                  fontSize: 11,
                  color: Theme.of(context).brightness == Brightness.dark
                      ? const Color(0xFF80CBC4) : Colors.blueGrey),
            ),
            const SizedBox(height: 4),

            // Pallet & Rack scan — side by side
            Row(
              children: [
                SizedBox(
                  width: 140,
                  child: ScanField(
                    controller: _palletController,
                    label: 'Pallet No',
                    autofocus: true,
                    onSubmitted: _onPalletScanned,
                    onScanPressed: _scanPalletBarcode,
                    filled: true,
                    fillColor: Theme.of(context).brightness == Brightness.dark
                        ? const Color(0xFF343B47) : Colors.white,
                    style: GoogleFonts.robotoCondensed(
                        fontSize: 13,
                        color: Theme.of(context).brightness == Brightness.dark
                            ? const Color(0xFFE4E8EE) : Colors.black),
                    labelStyle: GoogleFonts.robotoCondensed(fontSize: 12),
                    contentPadding: const EdgeInsets.symmetric(
                        horizontal: 8, vertical: 8),
                    isDense: true,
                  ),
                ),
                const SizedBox(width: 6),
                SizedBox(
                  width: 90,
                  child: ScanField(
                    controller: _rackController,
                    label: 'Rack No',
                    onSubmitted: _onRackScanned,
                    onScanPressed: _scanRackBarcode,
                    filled: true,
                    fillColor: Theme.of(context).brightness == Brightness.dark
                        ? const Color(0xFF343B47) : Colors.white,
                    style: GoogleFonts.robotoCondensed(
                        fontSize: 13,
                        color: Theme.of(context).brightness == Brightness.dark
                            ? const Color(0xFFE4E8EE) : Colors.black),
                    labelStyle: GoogleFonts.robotoCondensed(fontSize: 12),
                    contentPadding: const EdgeInsets.symmetric(
                        horizontal: 8, vertical: 8),
                    isDense: true,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 4),

            // Read-only fields — condensed rows
            Row(
              children: [
                Expanded(
                  flex: 2,
                  child: _buildReadOnlyRow('Category',
                      _categoryController.text, cs, isDark),
                ),
                const SizedBox(width: 4),
                Expanded(
                  flex: 3,
                  child: _buildReadOnlyRow('Batch',
                      _batchController.text, cs, isDark),
                ),
              ],
            ),
            const SizedBox(height: 4),
            Row(
              children: [
                Expanded(
                  flex: 1,
                  child: _buildReadOnlyRow('Run',
                      _runController.text, cs, isDark),
                ),
                const SizedBox(width: 4),
                Expanded(
                  flex: 4,
                  child: _buildReadOnlyRow('PCode',
                      _pCodeController.text, cs, isDark),
                ),
              ],
            ),
            const SizedBox(height: 4),
            Row(
              children: [
                Expanded(
                  flex: 2,
                  child: _buildReadOnlyRow('Qty',
                      _qtyController.text, cs, isDark),
                ),
                const SizedBox(width: 4),
                Expanded(
                  flex: 1,
                  child: _buildReadOnlyRow('Unit',
                      _unitController.text, cs, isDark),
                ),
              ],
            ),
            const SizedBox(height: 4),

            // Count display
            Card(
              margin: const EdgeInsets.only(bottom: 3),
              color: Colors.indigo.shade700,
              child: Padding(
                padding: const EdgeInsets.symmetric(
                    horizontal: 10, vertical: 5),
                child: Row(
                  children: [
                    Text(
                      'Rack In : $_count',
                      style: GoogleFonts.robotoCondensed(
                          fontSize: 12,
                          fontWeight: FontWeight.w600,
                          color: Colors.white),
                    ),
                    const Spacer(),
                    Icon(Icons.inventory_2,
                        size: 16,
                        color: Colors.white.withValues(alpha: 0.9)),
                  ],
                ),
              ),
            ),

            // DataListView — fills remaining space
            SizedBox(
              height: 180,
              child: Card(
                margin: const EdgeInsets.only(bottom: 3),
                clipBehavior: Clip.hardEdge,
                child: DataListView(
                  columns: _columns,
                  rows: _listRows,
                  headerTextStyle:
                      GoogleFonts.robotoCondensed(fontSize: 11),
                  headerPadding: const EdgeInsets.symmetric(
                      horizontal: 8, vertical: 4),
                ),
              ),
            ),

            // Secondary buttons — Clear | List | Close
            Padding(
              padding: const EdgeInsets.only(bottom: 3),
              child: Row(
                children: [
                  Expanded(
                    child: ElevatedButton(
                      onPressed: _isLoading ? null : _onClearPressed,
                      style: btnStyle.copyWith(
                        backgroundColor:
                            WidgetStatePropertyAll(Colors.orange),
                        foregroundColor:
                            WidgetStatePropertyAll(Colors.white),
                        minimumSize:
                            const WidgetStatePropertyAll(Size(0, 30)),
                      ),
                      child: const Text('Clear'),
                    ),
                  ),
                  const SizedBox(width: 4),
                  Expanded(
                    child: ElevatedButton(
                      onPressed: _isLoading ? null : _onListPressed,
                      style: btnStyle.copyWith(
                        backgroundColor:
                            WidgetStatePropertyAll(Colors.blue),
                        foregroundColor:
                            WidgetStatePropertyAll(Colors.white),
                        minimumSize:
                            const WidgetStatePropertyAll(Size(0, 30)),
                      ),
                      child: const Text('List'),
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
                            WidgetStatePropertyAll(Colors.white),
                        minimumSize:
                            const WidgetStatePropertyAll(Size(0, 30)),
                      ),
                      child: const Text('Close'),
                    ),
                  ),
                ],
              ),
            ),
            // Primary OK — large, full width, glove-friendly
            Padding(
              padding: const EdgeInsets.only(bottom: 4),
              child: SizedBox(
                width: double.infinity,
                height: 52,
                child: MouseRegion(
                  cursor: SystemMouseCursors.click,
                  child: ElevatedButton.icon(
                    onPressed: _isLoading ? null : _onOkPressed,
                    icon: _isLoading
                        ? const SizedBox(
                            width: 20,
                            height: 20,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                              color: Colors.white,
                            ),
                          )
                        : const Icon(Icons.check_circle_outline, size: 22),
                    label: Text('MASUK RACK',
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

  /// Builds a DA-style read-only label/value row (condensed).
  Widget _buildReadOnlyRow(
      String label, String value, ColorScheme cs, bool isDark) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 5),
      decoration: BoxDecoration(
        color: cs.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(4),
        border: Border.all(
          color: isDark
              ? const Color(0xFF546E7A)
              : const Color(0xFF455A64),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: _cLabel),
          Text(
            value.isEmpty ? '-' : value,
            style: _cValue,
          ),
        ],
      ),
    );
  }
}
