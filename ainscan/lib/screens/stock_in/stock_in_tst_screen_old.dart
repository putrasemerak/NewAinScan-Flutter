import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../services/activity_log_service.dart';
import '../../services/database_service.dart';
import '../../services/auth_service.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/scan_field.dart';
import '../../widgets/barcode_scanner_dialog.dart';
import '../../widgets/data_list_view.dart';
import '../../config/app_constants.dart';
import '../../utils/converters.dart';
import '../../utils/dialog_mixin.dart';

/// TST Stock In screen — converts frmStockIN_TST.vb
///
/// Combines receiving + rack-in in one step. Handles both NORMAL and LOOSE
/// pallets. Flow: scan pallet -> review info -> edit actual qty -> scan rack
/// -> press Inbound to execute receiving + rack-in together.
class StockInTstScreen extends StatefulWidget {
  const StockInTstScreen({super.key});

  @override
  State<StockInTstScreen> createState() => _StockInTstScreenState();
}

class _StockInTstScreenState extends State<StockInTstScreen> with DialogMixin {
  final _db = DatabaseService();

  // Controllers
  final _palletController = TextEditingController();
  final _rackController = TextEditingController();
  final _actualQtyController = TextEditingController();

  // Focus nodes
  final _palletFocus = FocusNode();
  final _rackFocus = FocusNode();
  final _actualQtyFocus = FocusNode();

  // State
  bool _isLoading = false;
  bool _palletScanned = false;
  bool _rackScanned = false;

  // Pallet info
  String _category = ''; // NORMAL or LOOSE
  String _batch = '';
  String _run = ''; // Cycle
  String _pCode = '';
  String _pGroup = '';
  // ignore: unused_field — mirrors VB.NET PName; may be displayed later
  String _pName = '';
  String _unit = '';
  double _fullQty = 0;
  double _lsQty = 0;
  String _qcStatus = '';
  String _palletNo = '';

  // Pallet date from TA_PLT002 — used as AddDate for IV_0250 inserts (VB.NET: Tarikh)
  DateTime? _palletDate;

  // LOOSE entries (multiple runs from TA_PLL001)
  // ignore: unused_field
  List<Map<String, dynamic>> _looseEntries = [];

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
  void initState() {
    super.initState();
  }

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
  // Pallet scan — mirrors VB.NET txtPalletNo_KeyPress
  // ---------------------------------------------------------------------------

  Future<void> _onPalletScanned(String value) async {
    final barcode = value.trim().toUpperCase();
    if (barcode.isEmpty) return;

    final bcType = AppConstants.getBarcodeType(barcode);
    if (bcType != BarcodeType.pallet) {
      showErrorDialog('Nombor pallet tidak sah');
      _palletController.clear();
      _palletFocus.requestFocus();
      return;
    }

    if (!mounted) return;
    setState(() => _isLoading = true);

    try {
      // Determine NORMAL or LOOSE via TA_PLL001
      // VB.NET Pallet_Type(): SELECT COUNT(PltNo) FROM TA_PLL001 WHERE PltNo=@PltNo
      final looseCountRows = await _db.query(
        "SELECT COUNT(PltNo) AS Cnt FROM TA_PLL001 WHERE PltNo=@PltNo",
        {'@PltNo': barcode},
      );
      final isLoose = (int.tryParse(
              looseCountRows.firstOrNull?['Cnt']?.toString() ?? '0') ??
          0) > 0;

      if (isLoose) {
        await _loadLoosePallet(barcode);
      } else {
        await _loadNormalPallet(barcode);
      }

      // Read pallet date from TA_PLT002 (VB.NET: Tarikh = AddDate)
      final plt002Rows = await _db.query(
        "SELECT AddDate FROM TA_PLT002 WHERE Pallet=@Pallet",
        {'@Pallet': barcode},
      );
      if (plt002Rows.isNotEmpty) {
        final addDate = plt002Rows.first['AddDate']?.toString() ?? '';
        if (addDate.isNotEmpty) {
          try {
            _palletDate = DateTime.parse(addDate);
          } catch (_) {
            _palletDate = DateTime.now();
          }
        }
      }
      _palletDate ??= DateTime.now();

      if (!mounted) return;
      setState(() {
        _palletScanned = true;
        _isLoading = false;
      });

      // VB.NET: txtActualQty.Focus(); txtActualQty.SelectAll()
      _actualQtyFocus.requestFocus();
    } catch (e) {
      if (!mounted) return;
      setState(() => _isLoading = false);
      showErrorDialog('Ralat membaca pallet: $e');
    }
  }

  /// Load NORMAL pallet info from TA_PLT001
  /// VB.NET: SELECT * FROM TA_PLT001 WHERE PltNo=@PltNo
  Future<void> _loadNormalPallet(String barcode) async {
    final rows = await _db.query(
      "SELECT * FROM TA_PLT001 WHERE PltNo=@PltNo",
      {'@PltNo': barcode},
    );
    if (rows.isEmpty) {
      throw Exception('Nombor pallet tidak sah');
    }

    final row = rows.first;
    final batch = (row['Batch'] ?? '').toString().trim();
    final pCode = (row['PCode'] ?? '').toString().trim();
    final pGroup = (row['PGroup'] ?? '').toString().trim();
    final fullQty = toDouble(row['FullQty']);
    final lsQty = toDouble(row['LsQty']);
    final unit = (row['Unit'] ?? '').toString().trim();
    final cycle = (row['Cycle'] ?? '').toString().trim();
    final pName = (row['PName'] ?? '').toString().trim();

    // Read QC status from PD_0800
    String qcStatus = '';
    final qcRows = await _db.query(
      "SELECT QS FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
      {'@Batch': batch, '@Run': cycle, '@PCode': pCode},
    );
    if (qcRows.isNotEmpty) {
      qcStatus = (qcRows.first['QS'] ?? '').toString().trim();
    }

    final totalQty = fullQty + lsQty;

    if (!mounted) return;
    setState(() {
      _category = 'NORMAL';
      _palletNo = barcode;
      _batch = batch;
      _run = cycle;
      _pCode = pCode;
      _pGroup = pGroup;
      _pName = pName;
      _unit = unit;
      _fullQty = fullQty;
      _lsQty = lsQty;
      _qcStatus = qcStatus;
      _actualQtyController.text = totalQty.toStringAsFixed(0);
    });
  }

  /// Load LOOSE pallet info from TA_PLL001 + TA_PLT001
  /// VB.NET: reads Run/Qty from TA_PLL001, PCode/PGroup/Batch/Cycle from TA_PLT001
  Future<void> _loadLoosePallet(String barcode) async {
    // Read loose entries from TA_PLL001 (has: PltNo, Batch, Run, Qty)
    final pllRows = await _db.query(
      "SELECT * FROM TA_PLL001 WHERE PltNo=@PltNo",
      {'@PltNo': barcode},
    );
    if (pllRows.isEmpty) {
      throw Exception('Nombor pallet tidak sah');
    }

    // Sum quantities and collect runs from TA_PLL001
    double looseQty = 0;
    final runs = <String>[];
    for (final entry in pllRows) {
      looseQty += toDouble(entry['Qty']);
      final run = (entry['Run'] ?? '').toString().trim();
      if (run.isNotEmpty && !runs.contains(run)) runs.add(run);
    }

    // Read pallet master info from TA_PLT001
    // VB.NET: SELECT * FROM TA_PLT001 WHERE PltNo=@PltNo
    final pltRows = await _db.query(
      "SELECT * FROM TA_PLT001 WHERE PltNo=@PltNo",
      {'@PltNo': barcode},
    );
    if (pltRows.isEmpty) {
      throw Exception('Nombor pallet tidak sah');
    }

    final pltRow = pltRows.first;
    final batch = (pltRow['Batch'] ?? '').toString().trim();
    final pCode = (pltRow['PCode'] ?? '').toString().trim();
    final pGroup = (pltRow['PGroup'] ?? '').toString().trim();
    final unit = (pltRow['Unit'] ?? '').toString().trim();
    final pName = (pltRow['PName'] ?? '').toString().trim();

    // Read QC status from PD_0800 (use first run)
    String qcStatus = '';
    if (runs.isNotEmpty) {
      final qcRows = await _db.query(
        "SELECT QS FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {'@Batch': batch, '@Run': runs.first, '@PCode': pCode},
      );
      if (qcRows.isNotEmpty) {
        qcStatus = (qcRows.first['QS'] ?? '').toString().trim();
      }
    }

    if (!mounted) return;
    setState(() {
      _category = 'LOOSE';
      _palletNo = barcode;
      _batch = batch;
      _run = runs.join(',');
      _pCode = pCode;
      _pGroup = pGroup;
      _pName = pName;
      _unit = unit;
      _fullQty = looseQty;
      _lsQty = 0;
      _qcStatus = qcStatus;
      _actualQtyController.text = looseQty.toStringAsFixed(0);
      _looseEntries = List.from(pllRows);
    });
  }

  // ---------------------------------------------------------------------------
  // Rack scan — mirrors VB.NET txtRackNo_KeyPress
  // ---------------------------------------------------------------------------

  Future<void> _onRackScanned(String value) async {
    final barcode = value.trim().toUpperCase();
    if (barcode.isEmpty) return;

    final bcType = AppConstants.getBarcodeType(barcode);
    if (bcType != BarcodeType.rack) {
      showErrorDialog('Nombor lokasi tidak sah');
      _rackController.clear();
      _rackFocus.requestFocus();
      return;
    }

    if (!mounted) return;
    setState(() => _isLoading = true);

    try {
      // VB.NET: SELECT Rack FROM BD_0010 WHERE Rack=@Rack
      final rackRows = await _db.query(
        "SELECT Rack FROM BD_0010 WHERE Rack=@Rack",
        {'@Rack': barcode},
      );

      if (rackRows.isEmpty) {
        showErrorDialog('Nombor lokasi tidak sah');
        _rackController.clear();
        _rackFocus.requestFocus();
        if (!mounted) return;
        setState(() => _isLoading = false);
        return;
      }

      if (!mounted) return;
      setState(() {
        _rackScanned = true;
        _isLoading = false;
      });

      // VB.NET: btnInbound.Focus()
    } catch (e) {
      if (!mounted) return;
      setState(() => _isLoading = false);
      showErrorDialog('Ralat membaca lokasi: $e');
    }
  }

  // ---------------------------------------------------------------------------
  // Inbound — mirrors VB.NET btnInbound_Click
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

    // Re-validate rack
    // VB.NET: SELECT Rack FROM BD_0010 WHERE Rack=@Rack
    final rackRows = await _db.query(
      "SELECT Rack FROM BD_0010 WHERE Rack=@Rack",
      {'@Rack': rackNo},
    );
    if (rackRows.isEmpty) {
      showErrorDialog('Nombor lokasi tidak sah');
      return;
    }

    if (!mounted) return;
    setState(() => _isLoading = true);

    if (!mounted) return;
    final empNo = Provider.of<AuthService>(context, listen: false).empNo ?? '';

    try {
      if (_category == 'NORMAL') {
        await _inboundNormal(rackNo, actualQty, empNo);
      } else if (_category == 'LOOSE') {
        await _inboundLoose(rackNo, empNo);
      }

      // Load stock locations for display
      // VB.NET: SELECT Loct,OnHand,PCode,Batch,Run FROM IV_0250 WHERE Pallet=@Pallet
      await _loadStockLocations();

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Stok masuk berjaya'),
            backgroundColor: Colors.green,
          ),
        );
      }

      await ActivityLogService.log(
        action: 'TST_INBOUND',
        empNo: empNo,
        detail: 'Pallet: $_palletNo, Rack: $rackNo, Type: $_category',
      );

      // VB.NET: Body_Null() then txtPalletNo.Focus()
      _onClear();
    } catch (e) {
      await ActivityLogService.logError(
        action: 'TST_INBOUND',
        empNo: empNo,
        detail: 'Pallet: $_palletNo',
        errorMsg: '$e',
      );
      showErrorDialog('Ralat semasa stok masuk: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  // ---------------------------------------------------------------------------
  // NORMAL pallet inbound — mirrors VB.NET btnInbound_Click NORMAL path
  // ---------------------------------------------------------------------------

  Future<void> _inboundNormal(
      String rackNo, double actualQty, String empNo) async {
    final now = DateTime.now();
    final dateStr = DateFormat('yyyy-MM-dd').format(now);
    final timeStr = DateFormat('yyyy-MM-dd HH:mm:ss').format(now);
    final tarikhStr = _palletDate != null
        ? DateFormat('yyyy-MM-dd').format(_palletDate!)
        : dateStr;

    // Read QS from PD_0800
    // VB.NET: SELECT QS FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode
    String statusQC = _qcStatus;
    final qcRows = await _db.query(
      "SELECT QS FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
      {'@Batch': _batch, '@Run': _run, '@PCode': _pCode},
    );
    if (qcRows.isNotEmpty) {
      statusQC = (qcRows.first['QS'] ?? '').toString().trim();
    }

    // Re-read pallet info from TA_PLT001 (VB.NET does this in btnInbound)
    final pltRows = await _db.query(
      "SELECT * FROM TA_PLT001 WHERE PltNo=@PltNo",
      {'@PltNo': _palletNo},
    );
    if (pltRows.isEmpty) throw Exception('Nombor pallet tidak sah');
    final pltRow = pltRows.first;

    // -----------------------------------------------------------------------
    // Step 1: Check if already received (TStatus='Transfer')
    // VB.NET: SELECT COUNT(PltNo) FROM TA_PLT001 WHERE TStatus='Transfer' AND PltNo=@PltNo
    // -----------------------------------------------------------------------
    final recCountRows = await _db.query(
      "SELECT COUNT(PltNo) AS Cnt FROM TA_PLT001 WHERE TStatus=@TStatus AND PltNo=@PltNo",
      {'@TStatus': 'Transfer', '@PltNo': _palletNo},
    );
    final alreadyReceived =
        (int.tryParse(recCountRows.firstOrNull?['Cnt']?.toString() ?? '0') ??
                0) >
            0;

    if (!alreadyReceived) {
      // --- Not received: Create/Update TST1, then update TStatus ---

      // Check TST1 in IV_0250
      // VB.NET: SELECT * FROM IV_0250 WHERE Loct='TST1' AND Pallet=@Pallet
      final tst1Rows = await _db.query(
        "SELECT * FROM IV_0250 WHERE Loct='TST1' AND Pallet=@Pallet",
        {'@Pallet': _palletNo},
      );

      if (tst1Rows.isEmpty) {
        // INSERT TST1 (InputQty=ActualQty, OutputQty=ActualQty, OnHand=0)
        await _db.execute(
          "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,"
          "OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
          " VALUES ('TST1',@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,"
          "0,@InputQty,@OutputQty,0,@Pallet,@AddUser,@AddDate,@AddTime)",
          {
            '@PCode': _pCode,
            '@PGroup': _pGroup,
            '@Batch': _batch,
            '@PName': (pltRow['PName'] ?? '').toString().trim(),
            '@Unit': (pltRow['Unit'] ?? '').toString().trim(),
            '@Run': (pltRow['Cycle'] ?? '').toString().trim(),
            '@Status': statusQC,
            '@InputQty': actualQty,
            '@OutputQty': actualQty,
            '@Pallet': _palletNo,
            '@AddUser': empNo,
            '@AddDate': tarikhStr,
            '@AddTime': timeStr,
          },
        );
      } else {
        // UPDATE TST1: SET OutputQty=ActualQty, OnHand=0
        // VB.NET: WHERE Loct='TST1' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode
        await _db.execute(
          "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=0, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Loct='TST1' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
          {
            '@OutputQty': actualQty,
            '@EditUser': empNo,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Pallet': _palletNo,
            '@Batch': _batch,
            '@Run': _run,
            '@PCode': _pCode,
          },
        );
      }

      // Update TA_PLT001 TStatus='Transfer' with PickBy, RecUser, RecDate, RecTime
      await _db.execute(
        "UPDATE TA_PLT001 SET TStatus='Transfer', PickBy=@PickBy, "
        "RecUser=@RecUser, RecDate=@RecDate, RecTime=@RecTime WHERE PltNo=@PltNo",
        {
          '@PltNo': _palletNo,
          '@PickBy': empNo,
          '@RecUser': empNo,
          '@RecDate': tarikhStr,
          '@RecTime': timeStr,
        },
      );

      // Update TA_PLT002 TStatus='Transfer'
      await _db.execute(
        "UPDATE TA_PLT002 SET TStatus='Transfer', PickBy=@PickBy, "
        "RecUser=@RecUser, RecDate=@RecDate, RecTime=@RecTime WHERE Pallet=@Pallet",
        {
          '@Pallet': _palletNo,
          '@PickBy': empNo,
          '@RecUser': empNo,
          '@RecDate': tarikhStr,
          '@RecTime': timeStr,
        },
      );
    } else {
      // --- Already received: check/create/update TST1 ---
      // VB.NET: SELECT OnHand,OutputQty FROM IV_0250 WHERE Batch=@Batch AND Run=@Run
      //   AND Loct='TST1' AND Pallet=@Pallet AND PCode=@PCode
      final tst1Rows = await _db.query(
        "SELECT * FROM IV_0250 WHERE Batch=@Batch AND Run=@Run "
        "AND Loct='TST1' AND Pallet=@Pallet AND PCode=@PCode",
        {
          '@Batch': _batch,
          '@Run': _run,
          '@Pallet': _palletNo,
          '@PCode': _pCode,
        },
      );

      if (tst1Rows.isEmpty) {
        // INSERT TST1
        await _db.execute(
          "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,"
          "OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
          " VALUES ('TST1',@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,"
          "0,@InputQty,@OutputQty,0,@Pallet,@AddUser,@AddDate,@AddTime)",
          {
            '@PCode': _pCode,
            '@PGroup': _pGroup,
            '@Batch': _batch,
            '@PName': (pltRow['PName'] ?? '').toString().trim(),
            '@Unit': (pltRow['Unit'] ?? '').toString().trim(),
            '@Run': (pltRow['Cycle'] ?? '').toString().trim(),
            '@Status': statusQC,
            '@InputQty': actualQty,
            '@OutputQty': actualQty,
            '@Pallet': _palletNo,
            '@AddUser': empNo,
            '@AddDate': tarikhStr,
            '@AddTime': timeStr,
          },
        );
      } else {
        // UPDATE TST1: SET OutputQty=ActualQty, OnHand=0
        await _db.execute(
          "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=0, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Loct='TST1' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
          {
            '@OutputQty': actualQty,
            '@EditUser': empNo,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Pallet': _palletNo,
            '@Batch': _batch,
            '@Run': _run,
            '@PCode': _pCode,
          },
        );
      }
    }

    // -----------------------------------------------------------------------
    // Step 2: Check if already stock-in (TA_PLT002 Status='C')
    // VB.NET: SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Status='C' AND Pallet=@Pallet
    // -----------------------------------------------------------------------
    final siCountRows = await _db.query(
      "SELECT COUNT(Pallet) AS Cnt FROM TA_PLT002 WHERE Status=@Status AND Pallet=@Pallet",
      {'@Status': 'C', '@Pallet': _palletNo},
    );
    final alreadyStockIn =
        (int.tryParse(siCountRows.firstOrNull?['Cnt']?.toString() ?? '0') ??
                0) >
            0;

    if (!alreadyStockIn) {
      // --- First stock-in ---

      // Check PD_0800 exists
      final pd0800Count = await _db.query(
        "SELECT COUNT(Batch) AS Cnt FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {'@Batch': _batch, '@Run': _run, '@PCode': _pCode},
      );
      if ((int.tryParse(
                  pd0800Count.firstOrNull?['Cnt']?.toString() ?? '0') ??
              0) ==
          0) {
        throw Exception('Tiada ringkasan stok dalam PD_0800');
      }

      // Check/Create/Update TST2
      final tst2CountRows = await _db.query(
        "SELECT COUNT(Pallet) AS Cnt FROM IV_0250 WHERE Loct='TST2' "
        "AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {
          '@Pallet': _palletNo,
          '@Batch': _batch,
          '@Run': _run,
          '@PCode': _pCode,
        },
      );
      final tst2Exists =
          (int.tryParse(
                      tst2CountRows.firstOrNull?['Cnt']?.toString() ?? '0') ??
                  0) >
              0;

      if (!tst2Exists) {
        // INSERT TST2: InputQty=ActualQty, OutputQty=0, OnHand=ActualQty
        await _db.execute(
          "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,"
          "OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
          " VALUES ('TST2',@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,"
          "0,@InputQty,0,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)",
          {
            '@PCode': _pCode,
            '@PGroup': _pGroup,
            '@Batch': _batch,
            '@PName': (pltRow['PName'] ?? '').toString().trim(),
            '@Unit': (pltRow['Unit'] ?? '').toString().trim(),
            '@Run': (pltRow['Cycle'] ?? '').toString().trim(),
            '@Status': statusQC,
            '@InputQty': actualQty,
            '@OnHand': actualQty,
            '@Pallet': _palletNo,
            '@AddUser': empNo,
            '@AddDate': tarikhStr,
            '@AddTime': timeStr,
          },
        );
      } else {
        // UPDATE TST2: SET OutputQty=ActualQty, OnHand=0
        await _db.execute(
          "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=0, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Loct='TST2' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
          {
            '@OutputQty': actualQty,
            '@EditUser': empNo,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Pallet': _palletNo,
            '@Batch': _batch,
            '@Run': _run,
            '@PCode': _pCode,
          },
        );
      }

      // Minus TST2 (deduct stock from TST2 to transfer to rack)
      // VB.NET: SELECT * FROM IV_0250 WHERE Loct='TST2' AND Batch=@Batch
      //         AND Run=@Run AND Pallet=@Pallet AND OnHand>0
      final tst2StockRows = await _db.query(
        "SELECT * FROM IV_0250 WHERE Loct='TST2' AND Batch=@Batch "
        "AND Run=@Run AND Pallet=@Pallet AND OnHand>0",
        {'@Batch': _batch, '@Run': _run, '@Pallet': _palletNo},
      );
      if (tst2StockRows.isEmpty) {
        throw Exception('Tiada stok di TST2');
      }
      final tst2Row = tst2StockRows.first;
      await _db.execute(
        "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=@OnHand, "
        "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
        "WHERE Loct='TST2' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND OnHand>0",
        {
          '@OutputQty': toDouble(tst2Row['OutputQty']) + actualQty,
          '@OnHand': toDouble(tst2Row['OnHand']) - actualQty,
          '@EditUser': empNo,
          '@EditDate': dateStr,
          '@EditTime': timeStr,
          '@Pallet': _palletNo,
          '@Batch': _batch,
          '@Run': _run,
        },
      );

      // Register rack location
      await _registerRack(
        rackNo: rackNo,
        batch: _batch,
        run: _run,
        pCode: _pCode,
        pGroup: _pGroup,
        pName: (pltRow['PName'] ?? '').toString().trim(),
        unit: (pltRow['Unit'] ?? '').toString().trim(),
        status: statusQC,
        qty: actualQty,
        pallet: _palletNo,
        empNo: empNo,
        tarikhStr: tarikhStr,
        dateStr: dateStr,
        timeStr: timeStr,
        firstStockIn: true,
      );

      // Close pallet card
      // VB.NET: UPDATE TA_PLT001 SET Status='C', TStatus='Transfer' WHERE PltNo=@PltNo
      final plt001Count = await _db.query(
        "SELECT COUNT(Status) AS Cnt FROM TA_PLT001 WHERE PltNo=@PltNo",
        {'@PltNo': _palletNo},
      );
      if ((int.tryParse(
                  plt001Count.firstOrNull?['Cnt']?.toString() ?? '0') ??
              0) >
          0) {
        await _db.execute(
          "UPDATE TA_PLT001 SET Status='C', TStatus='Transfer' WHERE PltNo=@PltNo",
          {'@PltNo': _palletNo},
        );
      }

      // Close transfer sheet
      // VB.NET: UPDATE TA_PLT002 SET Status='C', TStatus='Transfer' WHERE Pallet=@Pallet
      final plt002Count = await _db.query(
        "SELECT COUNT(Pallet) AS Cnt FROM TA_PLT002 WHERE Pallet=@Pallet",
        {'@Pallet': _palletNo},
      );
      if ((int.tryParse(
                  plt002Count.firstOrNull?['Cnt']?.toString() ?? '0') ??
              0) >
          0) {
        await _db.execute(
          "UPDATE TA_PLT002 SET Status='C', TStatus='Transfer' WHERE Pallet=@Pallet",
          {'@Pallet': _palletNo},
        );
      }

      // Calculate rack balance and update PD_0800
      await _updateRackBalance(
        batch: _batch,
        run: _run,
        pCode: _pCode,
        qty: actualQty,
      );

      // Log to TA_LOC0600
      await _upsertLoc0600(
        palletNo: _palletNo,
        rackNo: rackNo,
        batch: _batch,
        run: _run,
        pCode: _pCode,
        pName: (pltRow['PName'] ?? '').toString().trim(),
        pGroup: _pGroup,
        qty: actualQty,
        unit: (pltRow['Unit'] ?? '').toString().trim(),
        empNo: empNo,
        tarikhStr: tarikhStr,
        timeStr: timeStr,
      );
    } else {
      // --- Already stock-in: update TST2 and register rack ---

      // Check TST2
      final tst2CountRows = await _db.query(
        "SELECT COUNT(Pallet) AS Cnt FROM IV_0250 WHERE Loct='TST2' "
        "AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet",
        {'@Batch': _batch, '@Run': _run, '@Pallet': _palletNo},
      );
      final tst2Exists =
          (int.tryParse(
                      tst2CountRows.firstOrNull?['Cnt']?.toString() ?? '0') ??
                  0) >
              0;

      if (!tst2Exists) {
        // INSERT TST2 with OutputQty=ActualQty, OnHand=0
        await _db.execute(
          "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,"
          "OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
          " VALUES ('TST2',@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,"
          "0,@InputQty,@OutputQty,0,@Pallet,@AddUser,@AddDate,@AddTime)",
          {
            '@PCode': _pCode,
            '@PGroup': _pGroup,
            '@Batch': _batch,
            '@PName': (pltRow['PName'] ?? '').toString().trim(),
            '@Unit': (pltRow['Unit'] ?? '').toString().trim(),
            '@Run': (pltRow['Cycle'] ?? '').toString().trim(),
            '@Status': statusQC,
            '@InputQty': actualQty,
            '@OutputQty': actualQty,
            '@Pallet': _palletNo,
            '@AddUser': empNo,
            '@AddDate': tarikhStr,
            '@AddTime': timeStr,
          },
        );
      } else {
        // UPDATE TST2: SET OutputQty=ActualQty, OnHand=0
        await _db.execute(
          "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=0, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Loct='TST2' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
          {
            '@OutputQty': actualQty,
            '@EditUser': empNo,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Pallet': _palletNo,
            '@Batch': _batch,
            '@Run': _run,
            '@PCode': _pCode,
          },
        );
      }

      // Register rack (already stock-in path)
      await _registerRack(
        rackNo: rackNo,
        batch: _batch,
        run: _run,
        pCode: _pCode,
        pGroup: _pGroup,
        pName: (pltRow['PName'] ?? '').toString().trim(),
        unit: (pltRow['Unit'] ?? '').toString().trim(),
        status: statusQC,
        qty: actualQty,
        pallet: _palletNo,
        empNo: empNo,
        tarikhStr: tarikhStr,
        dateStr: dateStr,
        timeStr: timeStr,
        firstStockIn: false,
      );
    }
  }

  // ---------------------------------------------------------------------------
  // LOOSE pallet inbound — mirrors VB.NET btnInbound_Click LOOSE path
  // ---------------------------------------------------------------------------

  Future<void> _inboundLoose(String rackNo, String empNo) async {
    final now = DateTime.now();
    final dateStr = DateFormat('yyyy-MM-dd').format(now);
    final timeStr = DateFormat('yyyy-MM-dd HH:mm:ss').format(now);
    final tarikhStr = _palletDate != null
        ? DateFormat('yyyy-MM-dd').format(_palletDate!)
        : dateStr;

    // Re-read TA_PLL001 for loop entries (VB.NET does this in btnInbound_Click)
    final pllRows = await _db.query(
      "SELECT * FROM TA_PLL001 WHERE PltNo=@PltNo",
      {'@PltNo': _palletNo},
    );
    if (pllRows.isEmpty) {
      throw Exception('Tiada data LOOSE untuk pallet ini');
    }

    // Read TA_PLT001 for PName, Unit (VB.NET: Rs4)
    final pltRows = await _db.query(
      "SELECT * FROM TA_PLT001 WHERE PltNo=@PltNo",
      {'@PltNo': _palletNo},
    );
    if (pltRows.isEmpty) throw Exception('Nombor pallet tidak sah');
    final pltRow = pltRows.first;

    // Loop each loose entry
    for (final entry in pllRows) {
      final batch = (entry['Batch'] ?? '').toString().trim();
      final run = (entry['Run'] ?? '').toString().trim();
      // VB.NET: txtActualQty.Text = entry.Qty (per-entry qty)
      final entryQty = toDouble(entry['Qty']);

      // Read QS from PD_0800
      String statusQC = '';
      final qcRows = await _db.query(
        "SELECT QS FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {'@Batch': batch, '@Run': run, '@PCode': _pCode},
      );
      if (qcRows.isNotEmpty) {
        statusQC = (qcRows.first['QS'] ?? '').toString().trim();
      }

      // Check if already received
      // VB.NET: SELECT ... FROM TA_PLT001 WHERE TStatus='Transfer' AND PltNo=@PltNo
      final recRows = await _db.query(
        "SELECT PltNo FROM TA_PLT001 WHERE TStatus=@TStatus AND PltNo=@PltNo",
        {'@TStatus': 'Transfer', '@PltNo': _palletNo},
      );
      final alreadyReceived = recRows.isNotEmpty;

      if (!alreadyReceived) {
        // Check TST1 for this entry
        // VB.NET: WHERE Loct='TST1' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode
        final tst1Rows = await _db.query(
          "SELECT * FROM IV_0250 WHERE Loct='TST1' AND Pallet=@Pallet "
          "AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
          {
            '@Pallet': _palletNo,
            '@Batch': batch,
            '@Run': run,
            '@PCode': _pCode,
          },
        );

        if (tst1Rows.isEmpty) {
          // INSERT TST1
          await _db.execute(
            "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,"
            "OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
            " VALUES ('TST1',@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,"
            "0,@InputQty,0,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)",
            {
              '@PCode': _pCode,
              '@PGroup': _pGroup,
              '@Batch': batch,
              '@PName': (pltRow['PName'] ?? '').toString().trim(),
              '@Unit': (pltRow['Unit'] ?? '').toString().trim(),
              '@Run': run,
              '@Status': statusQC,
              '@InputQty': entryQty,
              '@OnHand': entryQty,
              '@Pallet': _palletNo,
              '@AddUser': empNo,
              '@AddDate': tarikhStr,
              '@AddTime': timeStr,
            },
          );
        } else {
          // UPDATE TST1: SET OutputQty=0, OnHand=entryQty
          await _db.execute(
            "UPDATE IV_0250 SET OutputQty=0, OnHand=@OnHand, "
            "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
            "WHERE Loct='TST1' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
            {
              '@OnHand': entryQty,
              '@EditUser': empNo,
              '@EditDate': dateStr,
              '@EditTime': timeStr,
              '@Pallet': _palletNo,
              '@Batch': batch,
              '@Run': run,
              '@PCode': _pCode,
            },
          );
        }
      }

      // Check if already stock-in
      // VB.NET: SELECT COUNT(Pallet) FROM TA_PLT002 WHERE Status='C' AND Pallet=@Pallet
      final siCountRows = await _db.query(
        "SELECT COUNT(Pallet) AS Cnt FROM TA_PLT002 WHERE Status=@Status AND Pallet=@Pallet",
        {'@Status': 'C', '@Pallet': _palletNo},
      );
      final alreadyStockIn =
          (int.tryParse(siCountRows.firstOrNull?['Cnt']?.toString() ?? '0') ??
                  0) >
              0;

      if (!alreadyStockIn) {
        // Check PD_0800
        final pd0800Count = await _db.query(
          "SELECT COUNT(Batch) AS Cnt FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
          {'@Batch': batch, '@Run': run, '@PCode': _pCode},
        );
        if ((int.tryParse(
                    pd0800Count.firstOrNull?['Cnt']?.toString() ?? '0') ??
                0) ==
            0) {
          throw Exception(
              'Tiada ringkasan stok dalam PD_0800 ($batch/$run/$_pCode)');
        }

        // Check/Create/Update TST2
        final tst2CountRows = await _db.query(
          "SELECT COUNT(Pallet) AS Cnt FROM IV_0250 WHERE Loct='TST2' "
          "AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
          {
            '@Pallet': _palletNo,
            '@Batch': batch,
            '@Run': run,
            '@PCode': _pCode,
          },
        );
        final tst2Exists =
            (int.tryParse(
                        tst2CountRows.firstOrNull?['Cnt']?.toString() ??
                            '0') ??
                    0) >
                0;

        if (!tst2Exists) {
          // INSERT TST2: InputQty=entryQty, OutputQty=entryQty, OnHand=0
          await _db.execute(
            "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,"
            "OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
            " VALUES ('TST2',@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,"
            "0,@InputQty,@OutputQty,0,@Pallet,@AddUser,@AddDate,@AddTime)",
            {
              '@PCode': _pCode,
              '@PGroup': _pGroup,
              '@Batch': batch,
              '@PName': (pltRow['PName'] ?? '').toString().trim(),
              '@Unit': (pltRow['Unit'] ?? '').toString().trim(),
              '@Run': run,
              '@Status': statusQC,
              '@InputQty': entryQty,
              '@OutputQty': entryQty,
              '@Pallet': _palletNo,
              '@AddUser': empNo,
              '@AddDate': tarikhStr,
              '@AddTime': timeStr,
            },
          );
        } else {
          // UPDATE TST2: SET OutputQty=entryQty, OnHand=0
          await _db.execute(
            "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=0, "
            "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
            "WHERE Loct='TST2' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
            {
              '@OutputQty': entryQty,
              '@EditUser': empNo,
              '@EditDate': dateStr,
              '@EditTime': timeStr,
              '@Pallet': _palletNo,
              '@Batch': batch,
              '@Run': run,
              '@PCode': _pCode,
            },
          );
        }

        // Register rack for this entry
        await _registerRack(
          rackNo: rackNo,
          batch: batch,
          run: run,
          pCode: _pCode,
          pGroup: _pGroup,
          pName: (pltRow['PName'] ?? '').toString().trim(),
          unit: (pltRow['Unit'] ?? '').toString().trim(),
          status: statusQC,
          qty: entryQty,
          pallet: _palletNo,
          empNo: empNo,
          tarikhStr: tarikhStr,
          dateStr: dateStr,
          timeStr: timeStr,
          firstStockIn: true,
        );

        // Calculate rack balance
        await _updateRackBalance(
          batch: batch,
          run: run,
          pCode: _pCode,
          qty: entryQty,
        );

        // Log to TA_LOC0600
        await _upsertLoc0600(
          palletNo: _palletNo,
          rackNo: rackNo,
          batch: batch,
          run: run,
          pCode: _pCode,
          pName: (pltRow['PName'] ?? '').toString().trim(),
          pGroup: _pGroup,
          qty: entryQty,
          unit: (pltRow['Unit'] ?? '').toString().trim(),
          empNo: empNo,
          tarikhStr: tarikhStr,
          timeStr: timeStr,
        );
      } else {
        // Already stock-in: register rack
        await _registerRack(
          rackNo: rackNo,
          batch: batch,
          run: run,
          pCode: _pCode,
          pGroup: _pGroup,
          pName: (pltRow['PName'] ?? '').toString().trim(),
          unit: (pltRow['Unit'] ?? '').toString().trim(),
          status: statusQC,
          qty: entryQty,
          pallet: _palletNo,
          empNo: empNo,
          tarikhStr: tarikhStr,
          dateStr: dateStr,
          timeStr: timeStr,
          firstStockIn: false,
        );
      }
    } // end loop

    // Close pallet card (after loop)
    // VB.NET: UPDATE TA_PLT001 SET Status='C' WHERE PltNo=@PltNo
    final plt001Count = await _db.query(
      "SELECT COUNT(Status) AS Cnt FROM TA_PLT001 WHERE PltNo=@PltNo",
      {'@PltNo': _palletNo},
    );
    if ((int.tryParse(plt001Count.firstOrNull?['Cnt']?.toString() ?? '0') ??
            0) >
        0) {
      await _db.execute(
        "UPDATE TA_PLT001 SET Status='C' WHERE PltNo=@PltNo",
        {'@PltNo': _palletNo},
      );
    }

    // Close transfer sheet
    // VB.NET: UPDATE TA_PLT002 SET Status='C' WHERE Pallet=@Pallet
    final plt002Count = await _db.query(
      "SELECT COUNT(Pallet) AS Cnt FROM TA_PLT002 WHERE Pallet=@Pallet",
      {'@Pallet': _palletNo},
    );
    if ((int.tryParse(plt002Count.firstOrNull?['Cnt']?.toString() ?? '0') ??
            0) >
        0) {
      await _db.execute(
        "UPDATE TA_PLT002 SET Status='C' WHERE Pallet=@Pallet",
        {'@Pallet': _palletNo},
      );
    }
  }

  // ---------------------------------------------------------------------------
  // Register rack location — mirrors VB.NET "Register racking"
  // ---------------------------------------------------------------------------

  Future<void> _registerRack({
    required String rackNo,
    required String batch,
    required String run,
    required String pCode,
    required String pGroup,
    required String pName,
    required String unit,
    required String status,
    required double qty,
    required String pallet,
    required String empNo,
    required String tarikhStr,
    required String dateStr,
    required String timeStr,
    required bool firstStockIn,
  }) async {
    // VB.NET: SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch
    //         AND Run=@Run AND Pallet=@Pallet
    final rackRows = await _db.query(
      "SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch "
      "AND Run=@Run AND Pallet=@Pallet",
      {'@Loct': rackNo, '@Batch': batch, '@Run': run, '@Pallet': pallet},
    );

    if (rackRows.isEmpty) {
      // INSERT new rack location
      await _db.execute(
        "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,"
        "OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
        " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,"
        "0,@InputQty,0,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)",
        {
          '@Loct': rackNo,
          '@PCode': pCode,
          '@PGroup': pGroup,
          '@Batch': batch,
          '@PName': pName,
          '@Unit': unit,
          '@Run': run,
          '@Status': status,
          '@InputQty': qty,
          '@OnHand': qty,
          '@Pallet': pallet,
          '@AddUser': empNo,
          '@AddDate': tarikhStr,
          '@AddTime': timeStr,
        },
      );
    } else {
      if (firstStockIn) {
        // VB.NET (first stock-in): OnHand = Qty (SET), InputQty = existing+Qty
        // WHERE Loct=@Rack AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND PCode=@PCode
        final existRow = rackRows.first;
        await _db.execute(
          "UPDATE IV_0250 SET OnHand=@OnHand, InputQty=@InputQty, Pallet=@Pallet, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Loct=@Rack AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND PCode=@PCode",
          {
            '@OnHand': qty,
            '@InputQty': toDouble(existRow['InputQty']) + qty,
            '@Pallet': pallet,
            '@EditUser': empNo,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Rack': rackNo,
            '@Batch': batch,
            '@Run': run,
            '@PCode': pCode,
          },
        );
      } else {
        // VB.NET (already stock-in): OnHand = Qty, InputQty = Qty
        // WHERE Loct=@Rack AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet
        await _db.execute(
          "UPDATE IV_0250 SET OnHand=@OnHand, InputQty=@InputQty, Pallet=@Pallet, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Loct=@Rack AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet",
          {
            '@OnHand': qty,
            '@InputQty': qty,
            '@Pallet': pallet,
            '@EditUser': empNo,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Rack': rackNo,
            '@Batch': batch,
            '@Run': run,
          },
        );
      }
    }
  }

  // ---------------------------------------------------------------------------
  // PD_0800 rack balance — mirrors VB.NET "Calculate rack Balance"
  // ---------------------------------------------------------------------------

  Future<void> _updateRackBalance({
    required String batch,
    required String run,
    required String pCode,
    required double qty,
  }) async {
    try {
      // VB.NET: SELECT SUM(OnHand) AS Qty FROM IV_0250
      //   WHERE Batch=@Batch AND Run=@Run AND OnHand>0 GROUP BY Batch,Run
      final rackBalRows = await _db.query(
        "SELECT SUM(OnHand) AS Qty FROM IV_0250 "
        "WHERE Batch=@Batch AND Run=@Run AND OnHand>0 GROUP BY Batch, Run",
        {'@Batch': batch, '@Run': run},
      );
      final wRackBal =
          rackBalRows.isNotEmpty ? toDouble(rackBalRows.first['Qty']) : 0.0;

      // VB.NET: SELECT Rack_In FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode
      final pd0800Rows = await _db.query(
        "SELECT Rack_In FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {'@Batch': batch, '@Run': run, '@PCode': pCode},
      );
      if (pd0800Rows.isNotEmpty) {
        final existRackIn = toDouble(pd0800Rows.first['Rack_In']);
        // VB.NET: UPDATE PD_0800 SET Rack_In=Rack_In+Qty, SORack=SUM(OnHand)
        await _db.execute(
          "UPDATE PD_0800 SET Rack_In=@Rack_In, SORack=@SORack "
          "WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
          {
            '@Rack_In': existRackIn + qty,
            '@SORack': wRackBal,
            '@Batch': batch,
            '@Run': run,
            '@PCode': pCode,
          },
        );
      }
    } catch (_) {
      // Non-critical for user flow
    }
  }

  // ---------------------------------------------------------------------------
  // TA_LOC0600 logging — mirrors VB.NET "save log table TA_LOC0600"
  // ---------------------------------------------------------------------------

  Future<void> _upsertLoc0600({
    required String palletNo,
    required String rackNo,
    required String batch,
    required String run,
    required String pCode,
    required String pName,
    required String pGroup,
    required double qty,
    required String unit,
    required String empNo,
    required String tarikhStr,
    required String timeStr,
  }) async {
    final now = DateTime.now();
    final todayStr = DateFormat('yyyy-MM-dd').format(now);

    try {
      // VB.NET: SELECT * FROM TA_LOC0600 WHERE Pallet=@Pallet AND Rack=@Rack
      //   AND Batch=@Batch AND Run=@Run
      final logRows = await _db.query(
        "SELECT * FROM TA_LOC0600 WHERE Pallet=@Pallet AND Rack=@Rack "
        "AND Batch=@Batch AND Run=@Run",
        {
          '@Pallet': palletNo,
          '@Rack': rackNo,
          '@Batch': batch,
          '@Run': run,
        },
      );

      if (logRows.isNotEmpty) {
        // UPDATE existing log
        await _db.execute(
          "UPDATE TA_LOC0600 SET Pallet=@Pallet, Rack=@Rack, Batch=@Batch, Run=@Run, "
          "PCode=@PCode, PName=@PName, PGroup=@PGroup, Qty=@Qty, Unit=@Unit, Ref=@Ref, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Pallet=@Pallet AND Rack=@Rack AND Batch=@Batch AND Run=@Run",
          {
            '@Pallet': palletNo,
            '@Rack': rackNo,
            '@Batch': batch,
            '@Run': run,
            '@PCode': pCode,
            '@PName': pName,
            '@PGroup': pGroup,
            '@Qty': qty,
            '@Unit': unit,
            '@Ref': 1,
            '@EditUser': empNo,
            '@EditDate': todayStr,
            '@EditTime': timeStr,
          },
        );
      } else {
        // INSERT new log
        await _db.execute(
          "INSERT INTO TA_LOC0600 (Pallet,Rack,Batch,Run,PCode,PName,PGroup,Qty,Unit,Ref,"
          "AddUser,AddDate,AddTime) VALUES "
          "(@Pallet,@Rack,@Batch,@Run,@PCode,@PName,@PGroup,@Qty,@Unit,@Ref,"
          "@AddUser,@AddDate,@AddTime)",
          {
            '@Pallet': palletNo,
            '@Rack': rackNo,
            '@Batch': batch,
            '@Run': run,
            '@PCode': pCode,
            '@PName': pName,
            '@PGroup': pGroup,
            '@Qty': qty,
            '@Unit': unit,
            '@Ref': 1,
            '@AddUser': empNo,
            '@AddDate': tarikhStr,
            '@AddTime': timeStr,
          },
        );
      }
    } catch (_) {
      // Non-critical
    }
  }

  // ---------------------------------------------------------------------------
  // Load stock locations for display
  // VB.NET: SELECT Loct,OnHand,PCode,Batch,Run FROM IV_0250 WHERE Pallet=@Pallet
  // ---------------------------------------------------------------------------

  Future<void> _loadStockLocations() async {
    try {
      final rows = await _db.query(
        "SELECT Loct, OnHand, PCode, Batch, Run FROM IV_0250 WHERE Pallet=@Pallet",
        {'@Pallet': _palletNo},
      );

      if (!mounted) return;
      setState(() {
        _stockRows = rows.map((r) {
          return {
            'Loct': (r['Loct'] ?? '').toString().trim(),
            'OnHand': toDouble(r['OnHand']).toStringAsFixed(0),
            'PCode': (r['PCode'] ?? '').toString().trim(),
            'Batch': (r['Batch'] ?? '').toString().trim(),
            'Run': (r['Run'] ?? '').toString().trim(),
          };
        }).toList();
      });
    } catch (_) {
      if (!mounted) return;
      setState(() => _stockRows = []);
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
      _category = '';
      _batch = '';
      _run = '';
      _pCode = '';
      _pGroup = '';
      _pName = '';
      _unit = '';
      _fullQty = 0;
      _lsQty = 0;
      _qcStatus = '';
      _palletNo = '';
      _palletDate = null;
      _looseEntries = [];
    });
    _palletFocus.requestFocus();
  }

  void _onClose() {
    Navigator.of(context).pop();
  }

  // ---------------------------------------------------------------------------
  // Barcode scanner helpers
  // ---------------------------------------------------------------------------

  Future<void> _scanPallet() async {
    final result = await BarcodeScannerDialog.show(
      context,
      title: 'Imbas Pallet',
    );
    if (result != null && result.isNotEmpty) {
      _palletController.text = result;
      await _onPalletScanned(result);
    }
  }

  Future<void> _scanRack() async {
    final result = await BarcodeScannerDialog.show(
      context,
      title: 'Imbas Lokasi',
    );
    if (result != null && result.isNotEmpty) {
      _rackController.text = result;
      await _onRackScanned(result);
    }
  }

  // ---------------------------------------------------------------------------
  // Utility
  // ---------------------------------------------------------------------------



  // ---------------------------------------------------------------------------
  // Build
  // ---------------------------------------------------------------------------

  // Shared styles
  static final _labelStyle = GoogleFonts.robotoCondensed(
      fontSize: 11, fontWeight: FontWeight.w600, color: Colors.black54);
  static final _valueStyle = GoogleFonts.robotoCondensed(fontSize: 12);

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
      title: 'TST Stock In',
      body: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // ─── User info ──────────────────────────────────────
            Text(
              'User: ${auth.empNo ?? ''}@${auth.empName ?? ''}',
              style: GoogleFonts.robotoCondensed(
                  fontSize: 11, color: Colors.blueGrey),
            ),
            const SizedBox(height: 4),

            // ─── Pallet & Rack scan — side by side ──────────────
            Row(
              children: [
                SizedBox(
                  width: 140,
                  child: ScanField(
                    controller: _palletController,
                    focusNode: _palletFocus,
                    label: 'Pallet No',
                    autofocus: true,
                    onSubmitted: _onPalletScanned,
                    onScanPressed: _scanPallet,
                    filled: true,
                    fillColor: Colors.white,
                    style: GoogleFonts.robotoCondensed(
                        fontSize: 13, color: Colors.black),
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
                    focusNode: _rackFocus,
                    label: 'Rack No',
                    enabled: _palletScanned,
                    onSubmitted: _onRackScanned,
                    onScanPressed: _palletScanned ? _scanRack : null,
                    filled: true,
                    fillColor: Colors.white,
                    style: GoogleFonts.robotoCondensed(
                        fontSize: 13, color: Colors.black),
                    labelStyle: GoogleFonts.robotoCondensed(fontSize: 12),
                    contentPadding: const EdgeInsets.symmetric(
                        horizontal: 8, vertical: 8),
                    isDense: true,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 4),

            // ─── Info fields — condensed cards ──────────────────
            // Row 1: Category / Batch
            Row(
              children: [
                Expanded(
                  flex: 2,
                  child: _buildReadOnlyField('Category', _category, cs, isDark),
                ),
                const SizedBox(width: 4),
                Expanded(
                  flex: 3,
                  child: _buildReadOnlyField('Batch', _batch, cs, isDark),
                ),
              ],
            ),
            const SizedBox(height: 4),
            // Row 2: Run / PCode
            Row(
              children: [
                Expanded(
                  flex: 1,
                  child: _buildReadOnlyField('Run', _run, cs, isDark),
                ),
                const SizedBox(width: 4),
                Expanded(
                  flex: 4,
                  child: _buildReadOnlyField('PCode', _pCode, cs, isDark),
                ),
              ],
            ),
            const SizedBox(height: 4),
            // Row 3: Qty / Actual Qty / Unit
            Row(
              children: [
                Expanded(
                  flex: 2,
                  child: _buildReadOnlyField(
                    'Qty',
                    _palletScanned
                        ? (_fullQty + _lsQty).toStringAsFixed(0)
                        : '',
                    cs,
                    isDark,
                  ),
                ),
                const SizedBox(width: 4),
                Expanded(
                  flex: 2,
                  child: Container(
                    padding: const EdgeInsets.symmetric(
                        horizontal: 6, vertical: 2),
                    decoration: BoxDecoration(
                      color: cs.surfaceContainerHighest,
                      borderRadius: BorderRadius.circular(4),
                      border: Border.all(
                        color: isDark
                            ? const Color(0xFF8A8E94)
                            : const Color(0xFF455A64),
                      ),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text('Actual', style: _labelStyle),
                        SizedBox(
                          height: 22,
                          child: TextField(
                            controller: _actualQtyController,
                            focusNode: _actualQtyFocus,
                            enabled: _palletScanned,
                            keyboardType: TextInputType.number,
                            style: GoogleFonts.robotoCondensed(
                                fontSize: 12, color: Colors.black),
                            decoration: const InputDecoration(
                              isDense: true,
                              contentPadding: EdgeInsets.zero,
                              border: InputBorder.none,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(width: 4),
                Expanded(
                  flex: 1,
                  child: _buildReadOnlyField('Unit', _unit, cs, isDark),
                ),
              ],
            ),
            const SizedBox(height: 4),
            // Row 4: QC Status / PGroup
            Row(
              children: [
                Expanded(
                  flex: 2,
                  child: _buildReadOnlyField('QC', _qcStatus, cs, isDark),
                ),
                const SizedBox(width: 4),
                Expanded(
                  flex: 3,
                  child: _buildReadOnlyField('PGroup', _pGroup, cs, isDark),
                ),
              ],
            ),
            const SizedBox(height: 4),

            // ─── Stock locations ListView — fills remaining ─────
            Expanded(
              child: Card(
                margin: const EdgeInsets.only(bottom: 3),
                clipBehavior: Clip.hardEdge,
                child: DataListView(
                  columns: _stockColumns,
                  rows: _stockRows,
                  headerTextStyle:
                      GoogleFonts.robotoCondensed(fontSize: 11),
                  headerPadding: const EdgeInsets.symmetric(
                      horizontal: 8, vertical: 4),
                ),
              ),
            ),

            // ─── Secondary buttons — Clear | Close ──────────────
            Padding(
              padding: const EdgeInsets.only(bottom: 3),
              child: Row(
                children: [
                  Expanded(
                    child: ElevatedButton(
                      onPressed: _isLoading ? null : _onClear,
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
                      onPressed: _isLoading ? null : _onClose,
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
            // ─── Primary INBOUND — large, glove-friendly ────────
            Padding(
              padding: const EdgeInsets.only(bottom: 4),
              child: SizedBox(
                width: double.infinity,
                height: 52,
                child: MouseRegion(
                  cursor: SystemMouseCursors.click,
                  child: ElevatedButton.icon(
                    onPressed: _isLoading ? null : _onInbound,
                    icon: _isLoading
                        ? const SizedBox(
                            width: 20,
                            height: 20,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                              color: Colors.white,
                            ),
                          )
                        : const Icon(Icons.move_to_inbox, size: 22),
                    label: Text('INBOUND',
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

  /// Builds a DA-style read-only label/value field (condensed).
  Widget _buildReadOnlyField(
      String label, String value, ColorScheme cs, bool isDark) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 5),
      decoration: BoxDecoration(
        color: cs.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(4),
        border: Border.all(
          color: isDark
              ? const Color(0xFF8A8E94)
              : const Color(0xFF455A64),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: _labelStyle),
          Text(
            value.isEmpty ? '-' : value,
            style: _valueStyle,
          ),
        ],
      ),
    );
  }
}
