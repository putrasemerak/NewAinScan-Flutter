import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../../services/activity_log_service.dart';
import '../../services/auth_service.dart';
import '../../services/database_service.dart';
import '../../services/local_database_service.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/data_list_view.dart';
import '../../widgets/scan_field.dart';
import '../../widgets/barcode_scanner_dialog.dart';
import '../utility/log_list_screen.dart';
import 'rack_in_screen.dart';
import '../../utils/converters.dart';
import '../../utils/dialog_mixin.dart';

/// Receiving screen — converts frmReceiving.vb
///
/// Receives pallets from production (TST1 → TST2 transfer).
///
/// Flow:
/// 1. User scans pallet barcode
/// 2. System validates pallet and checks if already received
/// 3. System reads pallet data (NORMAL or LOOSE)
/// 4. On OK: transfers stock from TST1 to TST2 in IV_0250,
///    updates TA_PLT001/TA_PLT002 TStatus to 'Transfer',
///    logs to local SQLite
class ReceivingScreen extends StatefulWidget {
  const ReceivingScreen({super.key});

  @override
  State<ReceivingScreen> createState() => _ReceivingScreenState();
}

class _ReceivingScreenState extends State<ReceivingScreen> with DialogMixin {
  // ---------------------------------------------------------------------------
  // Controllers & services
  // ---------------------------------------------------------------------------
  final _palletController = TextEditingController();
  final _palletFocus = FocusNode();
  final _db = DatabaseService();
  final _localDb = LocalDatabaseService();

  // ---------------------------------------------------------------------------
  // Screen state
  // ---------------------------------------------------------------------------
  bool _isLoading = false;
  int _receivedCount = 0;

  // Session-only list: pallets received during this screen session
  final List<Map<String, String>> _sessionRows = [];
  final ScrollController _listScrollController = ScrollController();

  // ListView column config — matches DA Confirmation theme
  static const List<DataColumnConfig> _sessionColumns = [
    DataColumnConfig(name: 'No', flex: 1),
    DataColumnConfig(name: 'Pallet', flex: 3),
    DataColumnConfig(name: 'PCode', flex: 3),
    DataColumnConfig(name: 'Batch', flex: 2),
    DataColumnConfig(name: 'Qty', flex: 2),
  ];

  // Pallet data fields
  String _category = ''; // NORMAL or LOOSE
  String _batch = '';
  String _run = '';
  String _pCode = '';
  String _pName = '';
  String _pGroup = '';
  double _qty = 0;
  String _unit = '';
  String _palletNo = ''; // Trimmed pallet number for DB operations

  // For LOOSE pallets: store each run entry for looped processing
  List<Map<String, dynamic>> _looseEntries = [];

  // ---------------------------------------------------------------------------
  // Lifecycle
  // ---------------------------------------------------------------------------
  @override
  void initState() {
    super.initState();
    _loadSessionCount();
    // Auto-focus the pallet field
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _palletFocus.requestFocus();
    });
  }

  @override
  void dispose() {
    _palletController.dispose();
    _palletFocus.dispose();
    _listScrollController.dispose();
    super.dispose();
  }

  // ---------------------------------------------------------------------------
  // Session count from local DB
  // ---------------------------------------------------------------------------
  Future<void> _loadSessionCount() async {
    try {
      final logs = await _localDb.getReceivingLogs();
      if (mounted) {
        setState(() {
          _receivedCount = logs.length;
        });
      }
    } catch (_) {
      // Non-critical
    }
  }

  // ---------------------------------------------------------------------------
  // Barcode processing
  // ---------------------------------------------------------------------------

  /// Trims check digit from scanned barcode.
  ///
  /// Scanner appends a check digit: 8-char scan → 7-char pallet,
  /// 10-char scan → 9-char pallet, 13-char scan → 12-char pallet.
  /// Keyboard input used as-is.
  String _trimBarcode(String raw) {
    final upper = raw.toUpperCase().trim();
    final len = upper.length;
    if (len == 13) return upper.substring(0, 12); // new 12-char pallet
    if (len == 8 || len == 10) {
      // Trim last character (check digit)
      return upper.substring(0, len - 1);
    }
    return upper;
  }

  /// Main entry point after a barcode is scanned or typed.
  Future<void> _processPallet(String rawBarcode) async {
    if (rawBarcode.isEmpty) return;

    final trimmed = _trimBarcode(rawBarcode);

    // Validate length: must be 7, 9, or 12 chars after trimming
    if (trimmed.length != 7 && trimmed.length != 9 && trimmed.length != 12) {
      showMessageDialog('Nombor pallet tidak sah');
      _clearFields();
      return;
    }

    if (!mounted) return;
    setState(() {
      _isLoading = true;
      _palletNo = trimmed;
    });

    try {
      // ------------------------------------------------------------------
      // Step 1: Check if pallet already received (TStatus='Transfer')
      // ------------------------------------------------------------------
      final plt001Check = await _db.query(
        "SELECT COUNT(PltNo) AS Cnt FROM TA_PLT001 WHERE PltNo=@PltNo AND TStatus=@TStatus",
        {'@PltNo': trimmed, '@TStatus': 'Transfer'},
      );
      if (plt001Check.isNotEmpty) {
        final cnt = int.tryParse(plt001Check.first['Cnt']?.toString() ?? '0') ?? 0;
        if (cnt > 0) {
          showMessageDialog('Pallet sudah diterima');
          _clearFields();
          return;
        }
      }

      final plt002Check = await _db.query(
        "SELECT COUNT(Pallet) AS Cnt FROM TA_PLT002 WHERE Pallet=@Pallet AND TStatus=@TStatus",
        {'@Pallet': trimmed, '@TStatus': 'Transfer'},
      );
      if (plt002Check.isNotEmpty) {
        final cnt = int.tryParse(plt002Check.first['Cnt']?.toString() ?? '0') ?? 0;
        if (cnt > 0) {
          showMessageDialog('Pallet sudah diterima');
          _clearFields();
          return;
        }
      }

      // ------------------------------------------------------------------
      // Step 2: Determine if NORMAL or LOOSE pallet
      // ------------------------------------------------------------------
      final looseCheck = await _db.query(
        "SELECT * FROM TA_PLL001 WHERE PltNo=@PltNo",
        {'@PltNo': trimmed},
      );

      if (looseCheck.isNotEmpty) {
        // --- LOOSE pallet ---
        await _processLoosePallet(trimmed, looseCheck);
      } else {
        // --- NORMAL pallet ---
        await _processNormalPallet(trimmed);
      }
    } on DatabaseException catch (e) {
      showMessageDialog('Ralat pangkalan data: ${e.message}');
      _clearFields();
    } catch (e) {
      showMessageDialog('Ralat: $e');
      _clearFields();
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  /// Reads pallet data for a NORMAL pallet from TA_PLT001.
  Future<void> _processNormalPallet(String palletNo) async {
    final rows = await _db.query(
      "SELECT Batch, PCode, FullQty, LsQty, Unit, Cycle FROM TA_PLT001 "
      "WHERE PltNo=@PltNo",
      {'@PltNo': palletNo},
    );

    if (rows.isEmpty) {
      showMessageDialog('Pallet Tidak Sah');
      _clearFields();
      return;
    }

    final row = rows.first;
    final batch = (row['Batch'] ?? '').toString().trim();
    final pCode = (row['PCode'] ?? '').toString().trim();
    final fullQty = toDouble(row['FullQty']);
    final lsQty = toDouble(row['LsQty']);
    final unit = (row['Unit'] ?? '').toString().trim();
    final run = (row['Cycle'] ?? '').toString().trim(); // Cycle = Run

    final totalQty = fullQty + lsQty;

    // Check stock at TST1 — VB.NET: WHERE Loct='TST1' AND Batch=@Batch AND Pallet=@Pallet
    final tst1Rows = await _db.query(
      "SELECT COUNT(Loct) AS Cnt FROM IV_0250 WHERE Loct='TST1' AND Batch=@Batch AND Pallet=@Pallet",
      {'@Batch': batch, '@Pallet': palletNo},
    );
    final tst1Cnt = int.tryParse(tst1Rows.firstOrNull?['Cnt']?.toString() ?? '0') ?? 0;
    if (tst1Cnt == 0) {
      showMessageDialog('Tiada stok di TST1.\nTiada transfer sheet');
      _clearFields();
      return;
    }

    // Look up product name/group for display and logging
    final productInfo = await _getProductInfo(pCode);

    if (mounted) {
      setState(() {
        _category = 'NORMAL';
        _batch = batch;
        _run = run;
        _pCode = pCode;
        _pName = productInfo['PName'] ?? '';
        _pGroup = productInfo['PGroup'] ?? '';
        _qty = totalQty;
        _unit = unit;
        _looseEntries = [];
      });
    }
  }

  /// Reads pallet data for a LOOSE pallet from TA_PLL001 + TA_PLT001.
  Future<void> _processLoosePallet(
    String palletNo,
    List<Map<String, dynamic>> pllEntries,
  ) async {
    double totalQty = 0;
    final runs = <String>[];

    // TA_PLL001 has: Batch, Run, Qty
    final batch = (pllEntries.first['Batch'] ?? '').toString().trim();

    for (final entry in pllEntries) {
      final run = (entry['Run'] ?? '').toString().trim();
      final qty = toDouble(entry['Qty']);
      totalQty += qty;
      if (run.isNotEmpty && !runs.contains(run)) runs.add(run);
    }

    // Read PCode, Unit from TA_PLT001 (VB.NET does this separately)
    final plt001 = await _db.query(
      "SELECT PCode, Unit FROM TA_PLT001 WHERE PltNo=@PltNo",
      {'@PltNo': palletNo},
    );
    if (plt001.isEmpty) {
      showMessageDialog('Pallet Tidak Sah');
      _clearFields();
      return;
    }
    final pCode = (plt001.first['PCode'] ?? '').toString().trim();
    final unit = (plt001.first['Unit'] ?? '').toString().trim();

    // Check stock at TST1 for each entry
    // VB.NET checks: WHERE Loct='TST1' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND OnHand>0
    // (checked later during transfer, not during scan in VB.NET)

    // Look up product name/group for display
    final productInfo = await _getProductInfo(pCode);

    if (mounted) {
      setState(() {
        _category = 'LOOSE';
        _batch = batch;
        _run = runs.join(', ');
        _pCode = pCode;
        _pName = productInfo['PName'] ?? '';
        _pGroup = productInfo['PGroup'] ?? '';
        _qty = totalQty;
        _unit = unit;
        _looseEntries = pllEntries;
      });
    }
  }

  // ---------------------------------------------------------------------------\n  // Stock validation\n  // ---------------------------------------------------------------------------\n\n  /// Retrieves product name and group from IC_0100.
  Future<Map<String, String>> _getProductInfo(String pCode) async {
    try {
      final rows = await _db.query(
        "SELECT PName, PGroup FROM IC_0100 WHERE PCode=@PCode",
        {'@PCode': pCode},
      );
      if (rows.isNotEmpty) {
        return {
          'PName': (rows.first['PName'] ?? '').toString().trim(),
          'PGroup': (rows.first['PGroup'] ?? '').toString().trim(),
        };
      }
    } catch (_) {
      // Non-critical — display will just be empty
    }
    return {'PName': '', 'PGroup': ''};
  }

  // ---------------------------------------------------------------------------
  // OK — perform transfer
  // ---------------------------------------------------------------------------

  /// Executes the TST1 → TST2 stock transfer and updates pallet status.
  Future<void> _confirmReceiving() async {
    if (_palletNo.isEmpty || _category.isEmpty) {
      showMessageDialog('Sila imbas pallet terlebih dahulu');
      return;
    }

    if (!mounted) return;
    setState(() => _isLoading = true);

    try {
      final authService = Provider.of<AuthService>(context, listen: false);
      final user = authService.empNo ?? '';
      final now = DateTime.now();
      final dateStr = DateFormat('yyyy-MM-dd').format(now);
      final timeStr = DateFormat('HH:mm:ss').format(now);

      if (_category == 'NORMAL') {
        await _transferNormal(user, dateStr, timeStr);
      } else {
        await _transferLoose(user, dateStr, timeStr);
      }

      // ------------------------------------------------------------------
      // Update TA_PLT001: TStatus, PickBy, RecUser, RecDate, RecTime
      // ------------------------------------------------------------------
      await _db.execute(
        "UPDATE TA_PLT001 SET TStatus='Transfer', PickBy=@User, "
        "RecUser=@User, RecDate=@RecDate, RecTime=@RecTime "
        "WHERE PltNo=@PltNo",
        {
          '@User': user,
          '@RecDate': dateStr,
          '@RecTime': timeStr,
          '@PltNo': _palletNo,
        },
      );

      // ------------------------------------------------------------------
      // Update TA_PLT002: TStatus, PickBy, RecUser, RecDate, RecTime
      // VB.NET: WHERE Pallet=@Pallet
      // ------------------------------------------------------------------
      await _db.execute(
        "UPDATE TA_PLT002 SET TStatus='Transfer', PickBy=@User, "
        "RecUser=@User, RecDate=@RecDate, RecTime=@RecTime "
        "WHERE Pallet=@Pallet",
        {
          '@User': user,
          '@RecDate': dateStr,
          '@RecTime': timeStr,
          '@Pallet': _palletNo,
        },
      );

      // ------------------------------------------------------------------
      // Log to local SQLite
      // ------------------------------------------------------------------
      await _localDb.insertReceivingLog({
        'PalletNo': _palletNo,
        'PCode': _pCode,
        'PName': _pName,
        'Batch': _batch,
        'Run': _run,
        'PGroup': _pGroup,
        'Qty': _qty,
        'Unit': _unit,
        'User': user,
        'Date': dateStr,
        'Time': timeStr,
      });

      // Update count
      _receivedCount++;

      // Add to session ListView
      _sessionRows.add({
        'No': _receivedCount.toString(),
        'Pallet': _palletNo,
        'PCode': _pCode,
        'Batch': _batch,
        'Qty': _qty.toStringAsFixed(_qty == _qty.roundToDouble() ? 0 : 2),
      });

      await ActivityLogService.log(
        action: 'RECEIVING_CONFIRM',
        empNo: user,
        detail: 'Pallet: $_palletNo, PCode: $_pCode, Batch: $_batch, Qty: $_qty',
      );

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Pallet $_palletNo berjaya diterima'),
            backgroundColor: Colors.green,
            duration: const Duration(seconds: 2),
          ),
        );
      }

      _clearFields();
      _palletFocus.requestFocus();

      // Auto-scroll to bottom after frame renders
      WidgetsBinding.instance.addPostFrameCallback((_) {
        if (_listScrollController.hasClients) {
          _listScrollController.animateTo(
            _listScrollController.position.maxScrollExtent,
            duration: const Duration(milliseconds: 300),
            curve: Curves.easeOut,
          );
        }
      });
    } on DatabaseException catch (e) {
      await ActivityLogService.logError(
        action: 'RECEIVING_CONFIRM',
        detail: 'Pallet: $_palletNo', errorMsg: e.message,
      );
      showMessageDialog('Ralat pangkalan data: ${e.message}');
    } catch (e) {
      await ActivityLogService.logError(
        action: 'RECEIVING_CONFIRM',
        detail: 'Pallet: $_palletNo', errorMsg: '$e',
      );
      showMessageDialog('Ralat: $e');
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  /// Transfers a NORMAL pallet: deduct TST1, add TST2 in IV_0250.
  /// Mirrors VB.NET Normal_Pallet() exactly.
  Future<void> _transferNormal(
    String user,
    String dateStr,
    String timeStr,
  ) async {
    // Re-read pallet info from TA_PLT001 (VB.NET does this)
    final rows = await _db.query(
      "SELECT PCode, Batch, Cycle, FullQty, LsQty, PltNo FROM TA_PLT001 "
      "WHERE PltNo=@PltNo",
      {'@PltNo': _palletNo},
    );
    if (rows.isEmpty) throw Exception('Invalid Pallet No');

    final row = rows.first;
    final batch = (row['Batch'] ?? '').toString().trim();
    final pCode = (row['PCode'] ?? '').toString().trim();
    final run = (row['Cycle'] ?? '').toString().trim();
    final fullQty = toDouble(row['FullQty']);
    final lsQty = toDouble(row['LsQty']);
    final qty = fullQty + lsQty;

    // --- Read TST1 row (VB.NET: WHERE Loct='TST1' AND Pallet=@Pallet AND OnHand>0) ---
    final tst1Rows = await _db.query(
      "SELECT * FROM IV_0250 WHERE Loct='TST1' AND Pallet=@Pallet AND OnHand>0",
      {'@Pallet': _palletNo},
    );
    if (tst1Rows.isEmpty) {
      throw Exception('Tiada stok di TST1');
    }
    final tst1 = tst1Rows.first;

    // --- Deduct TST1 ---
    final tst1OutputQty = toDouble(tst1['OutputQty']) + qty;
    final tst1OnHand = toDouble(tst1['OnHand']) - qty;
    await _db.execute(
      "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=@OnHand, "
      "Pallet=@Pallet, EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
      "WHERE Loct='TST1' AND Pallet=@Pallet AND OnHand>0",
      {
        '@OutputQty': tst1OutputQty,
        '@OnHand': tst1OnHand,
        '@Pallet': _palletNo,
        '@EditUser': user,
        '@EditDate': dateStr,
        '@EditTime': timeStr,
      },
    );

    // --- Add to TST2 (VB.NET: WHERE Loct='TST2' AND Pallet=@Pallet) ---
    final tst2Rows = await _db.query(
      "SELECT * FROM IV_0250 WHERE Loct='TST2' AND Pallet=@Pallet",
      {'@Pallet': _palletNo},
    );

    if (tst2Rows.isNotEmpty) {
      // Update existing TST2 row
      final tst2 = tst2Rows.first;
      final tst2OnHand = toDouble(tst2['OnHand']) + qty;
      final tst2InputQty = toDouble(tst2['InputQty']) + qty;
      await _db.execute(
        "UPDATE IV_0250 SET OnHand=@OnHand, InputQty=@InputQty, "
        "Pallet=@Pallet, EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
        "WHERE Loct='TST2' AND Pallet=@Pallet",
        {
          '@OnHand': tst2OnHand,
          '@InputQty': tst2InputQty,
          '@Pallet': _palletNo,
          '@EditUser': user,
          '@EditDate': dateStr,
          '@EditTime': timeStr,
        },
      );
    } else {
      // Insert new TST2 row with all columns from TST1 row (VB.NET pattern)
      await _db.execute(
        "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,"
        "OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime) "
        "VALUES ('TST2',@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,"
        "0,@InputQty,0,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)",
        {
          '@PCode': (tst1['PCode'] ?? pCode).toString().trim(),
          '@PGroup': (tst1['PGroup'] ?? '').toString().trim(),
          '@Batch': batch,
          '@PName': (tst1['PName'] ?? '').toString().trim(),
          '@Unit': (tst1['Unit'] ?? '').toString().trim(),
          '@Run': (tst1['Run'] ?? run).toString().trim(),
          '@Status': (tst1['Status'] ?? '').toString().trim(),
          '@InputQty': qty,
          '@OnHand': qty,
          '@Pallet': _palletNo,
          '@AddUser': user,
          '@AddDate': dateStr,
          '@AddTime': timeStr,
        },
      );
    }
  }

  /// Transfers a LOOSE pallet: loops through each TA_PLL001 entry.
  /// Mirrors VB.NET Loose_Pallet() exactly.
  Future<void> _transferLoose(
    String user,
    String dateStr,
    String timeStr,
  ) async {
    for (final entry in _looseEntries) {
      final batch = (entry['Batch'] ?? _batch).toString().trim();
      final run = (entry['Run'] ?? '').toString().trim();
      final qty = toDouble(entry['Qty']);

      // --- Read TST1 row for this batch/run ---
      // VB.NET: WHERE Loct='TST1' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND OnHand>0
      final tst1Rows = await _db.query(
        "SELECT * FROM IV_0250 WHERE Loct='TST1' AND Pallet=@Pallet "
        "AND Batch=@Batch AND Run=@Run AND OnHand>0",
        {'@Pallet': _palletNo, '@Batch': batch, '@Run': run},
      );
      if (tst1Rows.isEmpty) {
        throw Exception('Tiada stok di TST1.\nTiada transfer sheet');
      }
      final tst1 = tst1Rows.first;

      // --- Deduct TST1 ---
      final tst1OutputQty = toDouble(tst1['OutputQty']) + qty;
      final tst1OnHand = toDouble(tst1['OnHand']) - qty;
      await _db.execute(
        "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=@OnHand, "
        "Pallet=@Pallet, EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
        "WHERE Loct='TST1' AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND OnHand>0",
        {
          '@OutputQty': tst1OutputQty,
          '@OnHand': tst1OnHand,
          '@Pallet': _palletNo,
          '@EditUser': user,
          '@EditDate': dateStr,
          '@EditTime': timeStr,
          '@Batch': batch,
          '@Run': run,
        },
      );

      // --- Check TST2 for this batch/run ---
      // VB.NET: WHERE Loct='TST2' AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet
      final tst2Rows = await _db.query(
        "SELECT * FROM IV_0250 WHERE Loct='TST2' AND Pallet=@Pallet "
        "AND Batch=@Batch AND Run=@Run",
        {'@Pallet': _palletNo, '@Batch': batch, '@Run': run},
      );

      if (tst2Rows.isNotEmpty) {
        // Update existing TST2 row
        final tst2 = tst2Rows.first;
        final tst2OnHand = toDouble(tst2['OnHand']) + qty;
        final tst2InputQty = toDouble(tst2['InputQty']) + qty;
        await _db.execute(
          "UPDATE IV_0250 SET OnHand=@OnHand, InputQty=@InputQty, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Loct='TST2' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run",
          {
            '@OnHand': tst2OnHand,
            '@InputQty': tst2InputQty,
            '@EditUser': user,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Pallet': _palletNo,
            '@Batch': batch,
            '@Run': run,
          },
        );
      } else {
        // Insert new TST2 row with columns from TST1 row
        await _db.execute(
          "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,"
          "OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime) "
          "VALUES ('TST2',@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,"
          "0,@InputQty,0,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)",
          {
            '@PCode': (tst1['PCode'] ?? _pCode).toString().trim(),
            '@PGroup': (tst1['PGroup'] ?? '').toString().trim(),
            '@Batch': batch,
            '@PName': (tst1['PName'] ?? '').toString().trim(),
            '@Unit': (tst1['Unit'] ?? '').toString().trim(),
            '@Run': run,
            '@Status': (tst1['Status'] ?? '').toString().trim(),
            '@InputQty': qty,
            '@OnHand': qty,
            '@Pallet': _palletNo,
            '@AddUser': user,
            '@AddDate': dateStr,
            '@AddTime': timeStr,
          },
        );
      }
    }
  }

  // ---------------------------------------------------------------------------
  // Clear / helpers
  // ---------------------------------------------------------------------------

  void _clearFields() {
    if (mounted) {
      setState(() {
        _palletController.clear();
        _palletNo = '';
        _category = '';
        _batch = '';
        _run = '';
        _pCode = '';
        _pName = '';
        _pGroup = '';
        _qty = 0;
        _unit = '';
        _looseEntries = [];
      });
    }
    _palletFocus.requestFocus();
  }



  // ---------------------------------------------------------------------------
  // Camera scan
  // ---------------------------------------------------------------------------
  Future<void> _openScanner() async {
    final barcode = await BarcodeScannerDialog.show(
      context,
      title: 'Imbas Pallet',
    );
    if (barcode != null && barcode.isNotEmpty) {
      _palletController.text = barcode;
      await _processPallet(barcode);
    }
  }

  // ---------------------------------------------------------------------------
  // Navigation
  // ---------------------------------------------------------------------------
  void _openList() {
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => const LogListScreen(formType: 'R1'),
      ),
    );
  }

  // ── Compact text styles (matching DA Confirmation) ─────────────────
  static final _labelStyle = GoogleFonts.robotoCondensed(
    fontSize: 11,
    fontWeight: FontWeight.w600,
  );
  static final _fieldStyle = GoogleFonts.robotoCondensed(fontSize: 12);
  static final _fieldDecoration = const InputDecoration(
    isDense: true,
    contentPadding: EdgeInsets.symmetric(horizontal: 6, vertical: 6),
  );

  // ---------------------------------------------------------------------------
  // Build
  // ---------------------------------------------------------------------------
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
      title: 'Receiving',
      body: GestureDetector(
        onTap: () => FocusScope.of(context).unfocus(),
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
          child: SingleChildScrollView(
            child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // ─── 1. Date / User card ────────────────────────────
              Card(
                margin: const EdgeInsets.only(bottom: 3),
                child: Padding(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 10, vertical: 4),
                  child: Text(
                    'Date/User : $currentDate  /  ${auth.empNo ?? ''}@${auth.empName ?? ''}',
                    style: GoogleFonts.robotoCondensed(fontSize: 11),
                  ),
                ),
              ),

              // ─── 2. Pallet scan card ──────────────────────────
              Card(
                margin: const EdgeInsets.only(bottom: 3),
                child: Padding(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 10, vertical: 4),
                  child: Row(
                    children: [
                      Text('Pallet:', style: _labelStyle),
                      const SizedBox(width: 4),
                      SizedBox(
                        width: 120,
                        child: ScanField(
                          controller: _palletController,
                          focusNode: _palletFocus,
                          label: '',
                          autofocus: true,
                          enabled: !_isLoading,
                          onSubmitted: _processPallet,
                          onScanPressed:
                              _isLoading ? null : _openScanner,
                          filled: true,
                          fillColor: Theme.of(context).brightness == Brightness.dark
                              ? const Color(0xFF343B47) : Colors.white,
                          style: _fieldStyle,
                          contentPadding: const EdgeInsets.symmetric(
                              horizontal: 6, vertical: 6),
                          isDense: true,
                        ),
                      ),
                      const SizedBox(width: 8),
                      Text('Cat:', style: _labelStyle),
                      const SizedBox(width: 4),
                      Expanded(
                        child: TextField(
                          readOnly: true,
                          controller:
                              TextEditingController(text: _category),
                          style: _fieldStyle,
                          decoration: _fieldDecoration,
                        ),
                      ),
                    ],
                  ),
                ),
              ),

              // ─── 3. Info fields card ──────────────────────────
              Card(
                margin: const EdgeInsets.only(bottom: 3),
                child: Padding(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 10, vertical: 4),
                  child: Column(
                    children: [
                      // Batch / Run
                      Row(
                        children: [
                          SizedBox(
                              width: 40,
                              child:
                                  Text('Batch:', style: _labelStyle)),
                          const SizedBox(width: 4),
                          Expanded(
                            flex: 3,
                            child: TextField(
                              readOnly: true,
                              controller: TextEditingController(
                                  text: _batch),
                              style: _fieldStyle,
                              decoration: _fieldDecoration,
                            ),
                          ),
                          const SizedBox(width: 8),
                          SizedBox(
                              width: 28,
                              child:
                                  Text('Run:', style: _labelStyle)),
                          const SizedBox(width: 4),
                          Expanded(
                            flex: 1,
                            child: TextField(
                              readOnly: true,
                              controller: TextEditingController(
                                  text: _run),
                              style: _fieldStyle,
                              decoration: _fieldDecoration,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 4),
                      // PCode / Qty / Unit
                      Row(
                        children: [
                          SizedBox(
                              width: 40,
                              child:
                                  Text('PCode:', style: _labelStyle)),
                          const SizedBox(width: 4),
                          Expanded(
                            flex: 3,
                            child: TextField(
                              readOnly: true,
                              controller: TextEditingController(
                                  text: _pCode),
                              style: _fieldStyle,
                              decoration: _fieldDecoration,
                            ),
                          ),
                          const SizedBox(width: 8),
                          SizedBox(
                              width: 24,
                              child:
                                  Text('Qty:', style: _labelStyle)),
                          const SizedBox(width: 4),
                          Expanded(
                            flex: 1,
                            child: TextField(
                              readOnly: true,
                              controller: TextEditingController(
                                text: _qty > 0
                                    ? _qty.toStringAsFixed(
                                        _qty == _qty.roundToDouble()
                                            ? 0
                                            : 2)
                                    : '',
                              ),
                              style: _fieldStyle,
                              decoration: _fieldDecoration,
                            ),
                          ),
                          const SizedBox(width: 4),
                          Text(_unit,
                              style: GoogleFonts.robotoCondensed(
                                  fontSize: 11)),
                        ],
                      ),
                    ],
                  ),
                ),
              ),

              // ─── 4. Session ListView (fills remaining space) ──
              Expanded(
                child: Card(
                  margin: const EdgeInsets.only(bottom: 3),
                  clipBehavior: Clip.hardEdge,
                  child: Column(
                    children: [
                      Container(
                        width: double.infinity,
                        padding: const EdgeInsets.symmetric(
                            horizontal: 6, vertical: 3),
                        color: Theme.of(context)
                            .colorScheme
                            .primaryContainer,
                        child: Row(
                          children: [
                            Text(
                              'Received (session)',
                              style: GoogleFonts.robotoCondensed(
                                  fontSize: 10,
                                  fontWeight: FontWeight.w600),
                            ),
                            const Spacer(),
                            if (_isLoading)
                              const Padding(
                                padding: EdgeInsets.only(right: 6),
                                child: SizedBox(
                                  width: 12,
                                  height: 12,
                                  child: CircularProgressIndicator(
                                      strokeWidth: 1.5),
                                ),
                              ),
                            Text(
                              '$_receivedCount pallet${_receivedCount == 1 ? '' : 's'}',
                              style: GoogleFonts.robotoCondensed(
                                  fontSize: 10,
                                  fontWeight: FontWeight.w600,
                                  color: Colors.green.shade800),
                            ),
                          ],
                        ),
                      ),
              Expanded(
                child: DataListView(
                          columns: _sessionColumns,
                          rows: _sessionRows,
                          headerTextStyle:
                              GoogleFonts.robotoCondensed(fontSize: 11),
                          headerPadding: const EdgeInsets.symmetric(
                              horizontal: 8, vertical: 4),
                          scrollController: _listScrollController,
                        ),
                      ),
                    ],
                  ),
                ),
              ),

              // ─── 5. Di Terima summary ─────────────────────────
              Card(
                margin: const EdgeInsets.only(bottom: 3),
                color: Colors.green.shade700,
                child: Padding(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 10, vertical: 5),
                  child: Row(
                    children: [
                      Text(
                        'Di Terima : $_receivedCount',
                        style: GoogleFonts.robotoCondensed(
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                            color: Colors.white),
                      ),
                      const Spacer(),
                      Icon(Icons.check_circle,
                          size: 16, color: Colors.white.withValues(alpha: 0.9)),
                    ],
                  ),
                ),
              ),

              // ─── 6. Bottom buttons ────────────────────────────
              // Row 1: Secondary — Clear | List | Stock IN | Close
              Padding(
                padding: const EdgeInsets.only(bottom: 3),
                child: Row(
                  children: [
                    Expanded(
                      child: ElevatedButton(
                        onPressed: _isLoading ? null : _clearFields,
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
                        onPressed: _isLoading ? null : _openList,
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
                      child: ElevatedButton.icon(
                        onPressed: _isLoading
                            ? null
                            : () {
                                Navigator.of(context).push(
                                  MaterialPageRoute(
                                    builder: (_) => const RackInScreen(),
                                  ),
                                );
                              },
                        icon: const Icon(Icons.warehouse, size: 14),
                        label: Text('Stock IN',
                            style: GoogleFonts.robotoCondensed(
                                fontSize: 10,
                                fontWeight: FontWeight.w600)),
                        style: btnStyle.copyWith(
                          backgroundColor:
                              WidgetStatePropertyAll(Colors.indigo),
                          foregroundColor:
                              WidgetStatePropertyAll(Colors.white),
                          minimumSize:
                              const WidgetStatePropertyAll(Size(0, 30)),
                          padding: const WidgetStatePropertyAll(
                              EdgeInsets.symmetric(horizontal: 4)),
                        ),
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
              // Row 2: Primary OK — large, full width, glove-friendly
              Padding(
                padding: const EdgeInsets.only(bottom: 4),
                child: SizedBox(
                  width: double.infinity,
                  height: 52,
                  child: MouseRegion(
                    cursor: SystemMouseCursors.click,
                    child: ElevatedButton.icon(
                      onPressed: _isLoading || _category.isEmpty
                          ? null
                          : _confirmReceiving,
                      icon: const Icon(Icons.check_circle_outline, size: 22),
                      label: Text('TERIMA',
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
      ),
    );
  }
}
