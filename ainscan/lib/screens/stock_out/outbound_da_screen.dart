import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';

import '../../services/activity_log_service.dart';
import '../../services/auth_service.dart';
import '../../services/database_service.dart';
import '../../services/local_database_service.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/scan_field.dart';
import '../../widgets/barcode_scanner_dialog.dart';
import '../stock_control/change_rack_screen.dart';
import '../utility/location_screen.dart';

/// Outbound DA screen — converts frmOut_DA_2.vb (2716 lines)
///
/// DA preparation workflow:
/// 1. Scan DA number (13 chars) -> loads DA info, batch list, method
/// 2. Scan pallet (7-9 chars) -> validates pallet, checks booking/inbound,
///    resolves NORMAL vs LOOSE, looks up batch/run/qty
/// 3. Scan rack (4-6 chars) -> validates rack, checks stock at location
/// 4. Press Prepare -> rack-out, update DO_0070, TA_LOC0300, PD_0800,
///    handle balance -> TST3 transit, update DA_Confirm local, confirm when done
class OutboundDaScreen extends StatefulWidget {
  const OutboundDaScreen({super.key});

  @override
  State<OutboundDaScreen> createState() => _OutboundDaScreenState();
}

class _OutboundDaScreenState extends State<OutboundDaScreen> {
  // ---------------------------------------------------------------------------
  // Controllers & focus nodes
  // ---------------------------------------------------------------------------
  final _daNoController = TextEditingController();
  final _palletController = TextEditingController();
  final _rackController = TextEditingController();
  final _preparedQtyController = TextEditingController();

  final _daNoFocus = FocusNode();
  final _palletFocus = FocusNode();
  final _rackFocus = FocusNode();
  final _preparedQtyFocus = FocusNode();

  // Scroll controllers
  final _mainScrollController = ScrollController();
  final _batchScrollController = ScrollController();
  final _locationScrollController = ScrollController();

  // ---------------------------------------------------------------------------
  // Services
  // ---------------------------------------------------------------------------
  final _db = DatabaseService();
  final _localDb = LocalDatabaseService();

  // ---------------------------------------------------------------------------
  // State variables mirroring VB.NET form fields
  // ---------------------------------------------------------------------------
  bool _isLoading = false;

  String _method = ''; // BOOKING or NORMAL
  String _batchNo = '';
  String _run = '';
  String _daQty = '';
  String _palletQty = '';
  String _totalQty = '';
  String _outstandingDA = '';
  String _bookingPalletQty = '0';
  String _qsStatus = '';
  Color _qsColor = Colors.transparent;
  Color _outstandingColor = Colors.transparent;

  // Internal state
  String _palletType = ''; // NORMAL or LOOSE
  String _batch = ''; // working batch (may differ from display)
  String _runInternal = ''; // working run
  String _pCode = '';
  double _palletQtyNum = 0;
  bool _kulim = false;

  // Batch list for ListView
  List<Map<String, String>> _batchList = [];
  int _selectedBatchIndex = -1;
  // Location/Pallet list for ListView
  List<Map<String, String>> _locationList = [];
  int _selectedLocationIndex = -1;

  // ---------------------------------------------------------------------------
  // Lifecycle
  // ---------------------------------------------------------------------------
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _daNoFocus.requestFocus();
    });
  }

  @override
  void dispose() {
    _daNoController.dispose();
    _palletController.dispose();
    _rackController.dispose();
    _preparedQtyController.dispose();
    _daNoFocus.dispose();
    _palletFocus.dispose();
    _rackFocus.dispose();
    _preparedQtyFocus.dispose();
    _mainScrollController.dispose();
    _batchScrollController.dispose();
    _locationScrollController.dispose();
    super.dispose();
  }

  // ---------------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------------
  double _val(String? s) {
    if (s == null || s.isEmpty) return 0;
    if (s == 'Auto') return 0;
    return double.tryParse(s) ?? 0;
  }

  double _dbDouble(dynamic v) {
    if (v == null) return 0;
    if (v is num) return v.toDouble();
    return double.tryParse(v.toString()) ?? 0;
  }

  String _dbStr(dynamic v) {
    if (v == null) return '';
    return v.toString().trim();
  }

  String _user() {
    final auth = Provider.of<AuthService>(context, listen: false);
    return auth.empNo ?? '';
  }

  void _showMsg(String msg, {bool isInfo = false}) {
    if (!mounted) return;
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(isInfo ? 'Maklumat' : 'Ralat'),
        content: Text(msg),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }

  // ---------------------------------------------------------------------------
  // Barcode dispatch — unified handler for scanned/typed barcodes
  // ---------------------------------------------------------------------------
  Future<void> _processBarcode(String raw) async {
    if (raw.isEmpty) return;
    final bc = raw.toUpperCase().trim();
    final len = bc.length;

    if (len == 13) {
      if (_daNoController.text.trim().isNotEmpty) {
        // DA already loaded — treat as scanned new 12-char pallet (13→12)
        final palletNo = bc.substring(0, 12);
        _kulim = false;
        _palletController.text = palletNo;
        await _processPallet(palletNo);
      } else {
        // No DA yet — treat as DA Number
        _daNoController.text = bc.substring(0, 12);
        await _loadDA(_daNoController.text);
      }
    } else if (len == 8 || len == 9) {
      // Pallet
      _kulim = (len == 9);
      final trimmed = _kulim ? bc.substring(0, 8) : bc.substring(0, 7);
      _palletController.text = trimmed;
      await _processPallet(trimmed);
    } else if (len == 10) {
      // 10-char pallet -> trim to 9 (Kulim)
      _kulim = true;
      final trimmed = bc.substring(0, 9);
      _palletController.text = trimmed;
      // Actually re-check: 9 chars after trim -> Kulim=true, substring(0,8)
      _palletController.text = trimmed.substring(0, 8);
      await _processPallet(_palletController.text);
    } else if (len == 12) {
      // New 12-char pallet (typed, no trim)
      _kulim = false;
      _palletController.text = bc;
      await _processPallet(bc);
    } else if (len == 6) {
      // Rack (6 chars -> trim to 5)
      _rackController.text = bc.substring(0, 5);
      await _processRack(_rackController.text);
    } else if (len == 5) {
      // Rack (5 chars -> trim to 4)
      _rackController.text = bc.substring(0, 4);
      await _processRack(_rackController.text);
    } else if (len == 7) {
      // Pallet (7 chars, no trim needed)
      _kulim = false;
      _palletController.text = bc;
      await _processPallet(bc);
    } else if (len == 4) {
      // Rack (4 chars, no trim)
      _rackController.text = bc;
      await _processRack(bc);
    }
  }

  // ---------------------------------------------------------------------------
  // 1. LOAD DA — scanned or typed DA number
  // ---------------------------------------------------------------------------
  Future<void> _loadDA(String daNo) async {
    if (daNo.isEmpty) return;
    debugPrint('[_loadDA] START daNo=$daNo');
    if (!mounted) return;
    setState(() => _isLoading = true);

    try {
      debugPrint('[_loadDA] connecting...');
      await _db.connect();
      debugPrint('[_loadDA] connected, querying DO_0020...');

      // Read open DA orders
      final rows = await _db.query(
        "SELECT DANo, Batch, Run, Quantity, Status, PCode "
        "FROM DO_0020 WHERE DANo=@DANo AND Status=@Status",
        {'@DANo': daNo.trim(), '@Status': 'Open'},
      );
      debugPrint('[_loadDA] DO_0020 rows=${rows.length}');

      if (rows.isEmpty) {
        _showMsg('DA Ini Telah Siap');
        _daNoController.clear();
        if (!mounted) return;
        setState(() => _method = '');
        _daNoFocus.requestFocus();
        return;
      }

      // Copy DA lines to local DA_Confirm
      debugPrint('[_loadDA] initializing local DB...');
      await _localDb.initialize();
      debugPrint('[_loadDA] local DB ready (avail=${_localDb.isAvailable}), copying ${rows.length} rows...');
      for (final row in rows) {
        final batch = _dbStr(row['Batch']);
        final run = _dbStr(row['Run']);
        final pCode = _dbStr(row['PCode']);

        final existing = await _localDb.getDAConfirm(daNo, batch, run, pCode);
        if (existing.isEmpty) {
          await _localDb.insertDAConfirm({
            'DANo': daNo.trim(),
            'Batch': batch,
            'Run': run,
            'Qty': _dbDouble(row['Quantity']),
            'PCode': pCode,
            'Prepared': 0,
            'Status': _dbStr(row['Status']),
          });
        }
      }
      debugPrint('[_loadDA] local copy done, loading batch list...');

      // Load batch list and detect method
      final batchRows = await _db.query(
        "SELECT Method, Batch, Run FROM DO_0020 "
        "WHERE DANo=@DANo GROUP BY Method, Batch, Run",
        {'@DANo': daNo},
      );
      debugPrint('[_loadDA] batchRows=${batchRows.length}');

      String method = 'NORMAL';
      final batches = <Map<String, String>>[];
      for (final r in batchRows) {
        final m = _dbStr(r['Method']);
        if (m == 'BOOKING') method = 'BOOKING';
        batches.add({
          'Batch': _dbStr(r['Batch']),
          'Run': _dbStr(r['Run']),
        });
      }

      debugPrint('[_loadDA] method=$method batches=${batches.length}, calling setState...');
      if (!mounted) return;
      setState(() {
        _method = method;
        _batchList = batches;
        _locationList = [];
        _selectedBatchIndex = -1;
        _selectedLocationIndex = -1;
      });
      debugPrint('[_loadDA] setState done');

      await ActivityLogService.log(
        action: 'DA_LOAD',
        empNo: _user(),
        detail: 'DA: ${daNo.trim()}, Method: $method, Lines: ${rows.length}',
      );
      debugPrint('[_loadDA] SUCCESS');

      _palletFocus.requestFocus();
    } on DatabaseException catch (e) {
      debugPrint('[_loadDA] DatabaseException: ${e.message}');
      await ActivityLogService.logError(
        action: 'DA_LOAD', empNo: _user(), detail: 'DA: $daNo', errorMsg: e.message,
      );
      _showMsg('Ralat pangkalan data: ${e.message}');
    } catch (e, st) {
      debugPrint('[_loadDA] ERROR: $e');
      debugPrint('[_loadDA] STACK: $st');
      await ActivityLogService.logError(
        action: 'DA_LOAD', empNo: _user(), detail: 'DA: $daNo', errorMsg: '$e',
      );
      _showMsg('Ralat: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  // ---------------------------------------------------------------------------
  // 2. PROCESS PALLET
  // ---------------------------------------------------------------------------
  Future<void> _processPallet(String palletNo) async {
    if (palletNo.isEmpty) return;

    // Trim scanner check digit — mirrors _trimPalletBarcode in rack_in_screen
    final bc = palletNo.toUpperCase().trim();
    final trimmed = _trimPalletInput(bc);
    if (trimmed != bc) {
      _palletController.text = trimmed;
    }
    palletNo = trimmed;

    // Must have DA number first
    final daNo = _daNoController.text.trim();
    if (daNo.isEmpty) {
      _showMsg('Sila imbas no DA dahulu');
      return;
    }

    if (!mounted) return;
    setState(() => _isLoading = true);

    try {
      await _db.connect();
      await _localDb.initialize();

      // --- BOOKING method check: TA_LOC0700 ---
      if (!mounted) return;
      setState(() => _bookingPalletQty = '0');
      if (_method == 'BOOKING') {
        final bookingRows = await _db.query(
          "SELECT * FROM TA_LOC0700 WHERE Pallet=@Pallet AND DANo=@DANo",
          {'@Pallet': palletNo.trim(), '@DANo': daNo.trim()},
        );
        if (bookingRows.isNotEmpty) {
          if (!mounted) return;
          setState(() {
            _bookingPalletQty =
                _dbDouble(bookingRows.first['Qty']).toString();
          });
        } else {
          _showMsg(
              'Pallet Ini Tiada Dalam Senarai Booking Untuk DA No Ini.');
          _clearPalletFields();
          return;
        }
      }

      // --- Check inbound record: TA_LOC0600 ---
      final inboundRows = await _db.query(
        "SELECT * FROM TA_LOC0600 WHERE Pallet=@Pallet",
        {'@Pallet': palletNo.trim()},
      );
      if (inboundRows.isEmpty) {
        _showMsg(
            'Tiada Rekod Inbound (Stock In). Sila Inbound Terlebih Dahulu.');
        _clearPalletFields();
        return;
      }

      // --- Determine pallet type ---
      await _checkPalletType(palletNo);

      if (_palletType == 'NORMAL') {
        await _processNormalPallet(palletNo, daNo);
      } else {
        // LOOSE
        _showMsg(
          'Ini Pallet Loose. Masukkan Nombor lokasi. Sistem akan proses secara Auto',
          isInfo: true,
        );
        if (!mounted) return;
        setState(() {
          _batchNo = 'Auto';
          _daQty = 'Auto';
          _palletQty = 'Auto';
          _preparedQtyController.text = 'Auto';
          _totalQty = 'Auto';
          _outstandingDA = 'Auto';
          _rackController.clear();
        });
        _rackFocus.requestFocus();
      }
    } on DatabaseException catch (e) {
      _showMsg('Ralat pangkalan data: ${e.message}');
    } catch (e) {
      _showMsg('Ralat: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  /// Trims scanner check digit from pallet barcode:
  /// 13→12, 9→8, 10→9→8, 8→7 chars.
  String _trimPalletInput(String bc) {
    final len = bc.length;
    if (len == 13) return bc.substring(0, 12);
    if (len == 10) return bc.substring(0, 8); // 10→9→8 (Kulim)
    if (len == 9) return bc.substring(0, 8);  // Kulim scanned
    if (len == 8) return bc.substring(0, 7);  // normal scanned
    return bc;
  }

  /// Determines if pallet is NORMAL or LOOSE (TA_PLL001 check)
  Future<void> _checkPalletType(String palletNo) async {
    if (_kulim) {
      _palletType = 'NORMAL';
      return;
    }
    final count = await _db.executeScalar(
      "SELECT COUNT(PltNo) FROM TA_PLL001 WHERE PltNo=@PltNo",
      {'@PltNo': palletNo},
    );
    _palletType = (_dbDouble(count) == 0) ? 'NORMAL' : 'LOOSE';
  }

  /// Process NORMAL pallet: lookup in TA_PLT001/003/IV_0250, validate batch in DA
  Future<void> _processNormalPallet(String palletNo, String daNo) async {
    String batch = '';
    String run = '';
    String pCode = '';
    double palletQty = 0;

    try {
      if (_kulim) {
        // Kulim product: read from IV_0250
        final iv = await _db.query(
          "SELECT * FROM IV_0250 WHERE Pallet=@Pallet AND OnHand > 0",
          {'@Pallet': palletNo.trim()},
        );
        if (iv.isEmpty) {
          _showMsg(
              'Nombor pallet tidak sah atau sudah dimasukkan ke lokasi');
          return;
        }
        batch = _dbStr(iv.first['Batch']);
        run = _dbStr(iv.first['Run']);
        pCode = _dbStr(iv.first['PCode']);
        palletQty = _dbDouble(iv.first['OnHand']);
      } else {
        // Check TA_PLT001
        final plt001 = await _db.query(
          "SELECT PltNo, Batch, PCode, QS, Cycle, FullQty, lsQty "
          "FROM TA_PLT001 WHERE PltNo=@PltNo",
          {'@PltNo': palletNo.trim()},
        );

        if (plt001.isEmpty) {
          // Fallback to TA_PLT003 (reprint pallet card)
          final plt003 = await _db.query(
            "SELECT PltNo, PCode, Batch, Cycle, Actual "
            "FROM TA_PLT003 WHERE PltNo=@PltNo",
            {'@PltNo': palletNo.trim()},
          );
          if (plt003.isEmpty) {
            _showMsg('Nombor Pallet tidak sah');
            _palletController.clear();
            _palletFocus.requestFocus();
            return;
          }
          batch = _dbStr(plt003.first['Batch']);
          run = _dbStr(plt003.first['Cycle']);
          pCode = _dbStr(plt003.first['PCode']);
        } else {
          batch = _dbStr(plt001.first['Batch']);
          run = _dbStr(plt001.first['Cycle']);
          pCode = _dbStr(plt001.first['PCode']);
          palletQty = _dbDouble(plt001.first['FullQty']) +
              _dbDouble(plt001.first['lsQty']);

          // Check QS status in pallet
          final qs = _dbStr(plt001.first['QS']);
          if (qs.isNotEmpty) {
            if (qs != 'WHP' && qs != 'WPT' && qs != 'QQP') {
              _showMsg('Pallet bermasalah - Rujuk QC, Status: $qs');
              return;
            }
          }
        }

        // Check QS in PD_0800
        await _checkQS(batch, run, pCode);
      }

      // --- Verify batch exists in DA (DO_0020) ---
      final daCheck = await _db.query(
        "SELECT Status FROM DO_0020 WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run",
        {'@DANo': daNo.trim(), '@Batch': batch.trim(), '@Run': run.trim()},
      );

      if (daCheck.isEmpty) {
        _showMsg('Batch ini tiada dalam DA');
        _clearPalletFields();
        _palletFocus.requestFocus();
        return;
      }

      if (_dbStr(daCheck.first['Status']) == 'Confirmed') {
        _showMsg('Batch ini telah disiapkan');
        _clearPalletFields();
        _palletFocus.requestFocus();
        return;
      }

      // --- Check stock in IV_0250 ---
      final stockRows = await _db.query(
        "SELECT OnHand FROM IV_0250 WHERE Pallet=@Pallet AND OnHand > 0",
        {'@Pallet': palletNo},
      );
      if (stockRows.isEmpty) {
        _showMsg('Pallet tiada di lokasi');
        _clearPalletFields();
        _palletFocus.requestFocus();
        return;
      }
      final onHand = _dbDouble(stockRows.first['OnHand']);
      palletQty = onHand;

      // --- Get DA qty ---
      final daSumRows = await _db.query(
        "SELECT DANo, Batch, Run, SUM(Quantity) AS SumOfQty "
        "FROM DO_0020 WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run "
        "AND Status='Open' GROUP BY DANo, Batch, Run",
        {'@DANo': daNo, '@Batch': batch, '@Run': run},
      );
      double daQty = 0;
      if (daSumRows.isNotEmpty) {
        daQty = _dbDouble(daSumRows.first['SumOfQty']);
      }

      // --- Check existing preparation in local DA_Confirm ---
      double totalPrepared = 0;
      final localConfirm =
          await _localDb.getDAConfirm(daNo, batch, run, pCode);
      if (localConfirm.isNotEmpty) {
        final prep = _dbDouble(localConfirm.first['Prepared']);
        if (prep > 0) {
          totalPrepared = prep;
        }
      }

      // --- Check preparation in DO_0070 ---
      final do0070Sum = await _db.query(
        "SELECT SUM(SelQty) AS SumOfPrepared FROM DO_0070 "
        "WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {'@DANo': daNo, '@Batch': batch, '@Run': run, '@PCode': pCode},
      );
      if (do0070Sum.isNotEmpty) {
        final sumVal = do0070Sum.first['SumOfPrepared'];
        if (sumVal != null) {
          totalPrepared = _dbDouble(sumVal);
          final outstanding = daQty - totalPrepared;
          if (outstanding <= 0) {
            // Already done — update status
            await _db.execute(
              "UPDATE DO_0020 SET Status='Confirmed' "
              "WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
              {
                '@DANo': daNo,
                '@Batch': batch,
                '@Run': run,
                '@PCode': pCode,
              },
            );
            _showMsg('Product & Batch ini telah disiapkan', isInfo: true);
            _bodyNull();
            return;
          }
        }
      }

      double outstanding = daQty - totalPrepared;

      // --- Calculate prepared qty ---
      double preparedQty;
      if (daQty >= palletQty) {
        preparedQty =
            (outstanding >= palletQty) ? palletQty : outstanding;
      } else {
        preparedQty =
            (palletQty >= outstanding) ? outstanding : palletQty;
      }

      // Store state
      _batch = batch;
      _runInternal = run;
      _pCode = pCode;
      _palletQtyNum = palletQty;

      if (!mounted) return;
      setState(() {
        _batchNo = batch;
        _run = run;
        _palletQty = palletQty.toStringAsFixed(0);
        _daQty = daQty.toStringAsFixed(0);
        _totalQty = totalPrepared.toStringAsFixed(0);
        _outstandingDA = outstanding.toStringAsFixed(0);
        _preparedQtyController.text = preparedQty.toStringAsFixed(0);
      });

      _rackFocus.requestFocus();
    } catch (e) {
      _showMsg('Error: $e');
    }
  }

  /// Check QS status in PD_0800
  Future<void> _checkQS(String batch, String run, String pCode) async {
    try {
      final rows = await _db.query(
        "SELECT QS FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {'@Batch': batch, '@Run': run, '@PCode': pCode},
      );
      if (rows.isEmpty) {
        _showMsg('Tiada Ringkasan Produk PD_0800');
        return;
      }
      final qs = _dbStr(rows.first['QS']);
      if (!mounted) return;
      setState(() {
        _qsStatus = qs;
        if (qs == 'WHP' || qs == 'WPT') {
          _qsColor = Colors.green;
        } else {
          _qsColor = Colors.red;
        }
      });
    } catch (_) {
      // Non-critical
    }
  }

  // ---------------------------------------------------------------------------
  // 3. PROCESS RACK
  // ---------------------------------------------------------------------------
  Future<void> _processRack(String rackNo) async {
    if (rackNo.isEmpty) return;

    // Must have pallet scanned first
    if (_palletController.text.isEmpty) {
      _showMsg('Sila Imbas no Pallet');
      return;
    }

    if (!mounted) return;
    setState(() => _isLoading = true);

    try {
      await _db.connect();
      final rack = rackNo.toUpperCase();

      // Validate rack in BD_0010
      final rackCheck = await _db.query(
        "SELECT Rack FROM BD_0010 WHERE Rack=@Rack",
        {'@Rack': rack},
      );
      if (rackCheck.isEmpty) {
        _showMsg('Nombor lokasi tidak sah');
        _rackController.clear();
        _rackFocus.requestFocus();
        return;
      }

      if (_palletType == 'NORMAL' || _palletType == 'Normal') {
        // Check stock at this rack
        final stockCount = await _db.executeScalar(
          "SELECT COUNT(loct) FROM IV_0250 "
          "WHERE loct=@loct AND Batch=@Batch AND Run=@Run AND OnHand>0",
          {'@loct': rack, '@Batch': _batch, '@Run': _runInternal},
        );
        if (_dbDouble(stockCount) == 0) {
          _showMsg('Produk tiada dilokasi ini');
          _rackController.clear();
          _palletController.clear();
          if (!mounted) return;
          setState(() {
            _batchNo = '';
            _run = '';
          });
          _palletFocus.requestFocus();
          return;
        }

        // Adjust prepared qty if > outstanding
        if (_val(_preparedQtyController.text) > _val(_outstandingDA)) {
          _preparedQtyController.text =
              _val(_outstandingDA).toStringAsFixed(0);
        }

        // Check QS in PD_0800
        await _checkQS(_batch, _runInternal, _pCode);

        _preparedQtyFocus.requestFocus();
        // Select all text in prepared qty field
        _preparedQtyController.selection = TextSelection(
          baseOffset: 0,
          extentOffset: _preparedQtyController.text.length,
        );
      } else {
        // LOOSE — focus on Prepare button (in Flutter, focus preparedQty)
        _preparedQtyFocus.requestFocus();
      }
    } on DatabaseException catch (e) {
      _showMsg('Ralat pangkalan data: ${e.message}');
    } catch (e) {
      _showMsg('Ralat: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  // ---------------------------------------------------------------------------
  // 4. PREPARE button — the main business logic
  // ---------------------------------------------------------------------------
  Future<void> _onPrepare() async {
    // Validate defaults
    if (_daQty.isEmpty) setState(() => _daQty = '0');
    if (_palletQty.isEmpty) setState(() => _palletQty = '0');
    if (_outstandingDA.isEmpty) setState(() => _outstandingDA = '0');
    if (_preparedQtyController.text.isEmpty) {
      _preparedQtyController.text = '0';
    }
    if (_totalQty.isEmpty) setState(() => _totalQty = '0');

    // BOOKING validation
    if (_method == 'BOOKING') {
      if (_val(_totalQty) != _val(_bookingPalletQty)) {
        _showMsg(
            'Total Prpd Qty Mestilah Sama Dengan Quantiti DA Booking : $_bookingPalletQty');
        return;
      }
    }

    if (!mounted) return;
    setState(() => _isLoading = true);

    try {
      if (_palletType == 'NORMAL') {
        await _prepareNormal();
      } else {
        await _prepareLoose();
      }
      await ActivityLogService.log(
        action: 'DA_PREPARE',
        empNo: _user(),
        detail: 'DA: ${_daNoController.text.trim()}, Pallet: ${_palletController.text.trim()}, '
            'Type: $_palletType, PrepQty: ${_preparedQtyController.text}',
      );
    } on DatabaseException catch (e) {
      await ActivityLogService.logError(
        action: 'DA_PREPARE', empNo: _user(),
        detail: 'DA: ${_daNoController.text.trim()}, Pallet: ${_palletController.text.trim()}',
        errorMsg: e.message,
      );
      _showMsg('Ralat pangkalan data: ${e.message}');
    } catch (e) {
      await ActivityLogService.logError(
        action: 'DA_PREPARE', empNo: _user(),
        detail: 'DA: ${_daNoController.text.trim()}, Pallet: ${_palletController.text.trim()}',
        errorMsg: '$e',
      );
      _showMsg('Ralat: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  // ---------------------------------------------------------------------------
  // 4a. NORMAL pallet preparation
  // ---------------------------------------------------------------------------
  Future<void> _prepareNormal() async {
    final daNo = _daNoController.text.trim();
    final palletNo = _palletController.text.trim();
    final rackNo = _rackController.text.trim().toUpperCase();
    final user = _user();
    final now = DateTime.now();
    final dateStr = DateFormat('yyyy-MM-dd').format(now);
    final timeStr = DateFormat('HH:mm:ss').format(now);

    // Validations
    if (rackNo.isEmpty) {
      _showMsg('Please scan Rack Number');
      _rackFocus.requestFocus();
      return;
    }

    final preparedQty = _val(_preparedQtyController.text);
    final palletQty = _val(_palletQty);

    if (preparedQty > palletQty) {
      _showMsg('Kuantiti prepare besar dari kuantiti produk yang ada');
      _preparedQtyController.clear();
      _preparedQtyFocus.requestFocus();
      return;
    }

    if (preparedQty > _palletQtyNum) {
      _showMsg('Kuantiti prepare besar dari kuantiti produk yang ada');
      _preparedQtyController.clear();
      _preparedQtyFocus.requestFocus();
      return;
    }

    if (preparedQty == 0) {
      _showMsg('Kuantiti prepare adalah 0');
      _preparedQtyController.clear();
      _preparedQtyFocus.requestFocus();
      return;
    }

    await _db.connect();
    await _localDb.initialize();

    // --- Rack-Out: read current IV_0250 then update ---
    final ivRows = await _db.query(
      "SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch "
      "AND Run=@Run AND Pallet=@Pallet AND PCode=@PCode AND OnHand > 0",
      {
        '@Loct': rackNo,
        '@Batch': _batch,
        '@Run': _runInternal,
        '@Pallet': palletNo,
        '@PCode': _pCode,
      },
    );
    if (ivRows.isEmpty) {
      _showMsg('Tiada stok di lokasi');
      return;
    }

    final currentOutputQty = _dbDouble(ivRows.first['OutputQty']);
    final currentOnHand = _dbDouble(ivRows.first['OnHand']);

    try {
      await _db.execute(
        "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=@OnHand, "
        "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
        "WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run "
        "AND Pallet=@Pallet AND PCode=@PCode AND OnHand > 0",
        {
          '@OutputQty': currentOutputQty + preparedQty,
          '@OnHand': currentOnHand - preparedQty,
          '@EditUser': user,
          '@EditDate': dateStr,
          '@EditTime': timeStr,
          '@Loct': rackNo,
          '@Batch': _batch,
          '@Run': _runInternal,
          '@Pallet': palletNo,
          '@PCode': _pCode,
        },
      );
    } catch (e) {
      _showMsg('Error while Rack Out: $e');
    }

    // --- Update local DA_Confirm ---
    try {
      final localRows =
          await _localDb.getDAConfirm(daNo, _batchNo, _run, _pCode);
      if (localRows.isNotEmpty) {
        final currentPrepared = _dbDouble(localRows.first['Prepared']);
        await _localDb.updateDAConfirmPrepared(
          daNo,
          _batchNo,
          _run,
          _pCode,
          currentPrepared + preparedQty,
          user,
        );
      }
    } catch (e) {
      _showMsg('Error Update Local DB: $e');
    }

    // --- Handle balance: move remainder to TST3 ---
    if (palletQty > preparedQty) {
      await _handleBalanceToTST3(
          daNo, palletNo, rackNo, palletQty, preparedQty, user, dateStr, timeStr);
    }

    // --- Update DO_0070 ---
    await _updateDO0070(daNo, palletNo, rackNo, preparedQty, user, dateStr, timeStr);

    // --- Update TA_LOC0300 (outbound log) ---
    await _updateTaLoc0300(daNo, palletNo, rackNo, preparedQty, user, dateStr, timeStr);

    // --- Update PD_0800 ---
    await _updatePD0800(preparedQty);

    // --- Check total prepared (local) ---
    await _checkTotalPreparedLocal(daNo);

    // --- Check total prepared (DO_0070) ---
    await _checkTotalPreparedDO0070(daNo);

    // --- Check if all DA is done ---
    await _checkDAComplete(daNo);
  }

  /// Handles the balance when palletQty > preparedQty:
  /// racks out remaining from original location and creates/updates TST3 record
  Future<void> _handleBalanceToTST3(
    String daNo,
    String palletNo,
    String rackNo,
    double palletQty,
    double preparedQty,
    String user,
    String dateStr,
    String timeStr,
  ) async {
    final balance = palletQty - preparedQty;

    // Read pallet info for TST3 insert
    final palletInfo = await _db.query(
      "SELECT * FROM IV_0250 WHERE Batch=@Batch AND Run=@Run "
      "AND Loct=@Loct AND Pallet=@Pallet",
      {
        '@Batch': _batch,
        '@Run': _runInternal,
        '@Loct': rackNo,
        '@Pallet': palletNo,
      },
    );

    // Rack out remaining from original location
    try {
      final ivRemaining = await _db.query(
        "SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch "
        "AND Run=@Run AND Pallet=@Pallet AND OnHand > 0",
        {
          '@Loct': rackNo,
          '@Batch': _batch,
          '@Run': _runInternal,
          '@Pallet': palletNo,
        },
      );
      if (ivRemaining.isNotEmpty) {
        final curOutput = _dbDouble(ivRemaining.first['OutputQty']);
        final curOnHand = _dbDouble(ivRemaining.first['OnHand']);
        await _db.execute(
          "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=@OnHand, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run "
          "AND Pallet=@Pallet AND OnHand > 0",
          {
            '@OutputQty': curOutput + balance,
            '@OnHand': curOnHand - balance,
            '@EditUser': user,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Loct': rackNo,
            '@Batch': _batch,
            '@Run': _runInternal,
            '@Pallet': palletNo,
          },
        );
      }
    } catch (e) {
      _showMsg('Error while outbound: $e');
    }

    // Create/update TST3
    try {
      final tst3Rows = await _db.query(
        "SELECT * FROM IV_0250 WHERE Batch=@Batch AND Run=@Run "
        "AND Loct='TST3' AND Pallet=@Pallet",
        {'@Batch': _batch, '@Run': _runInternal, '@Pallet': palletNo},
      );

      if (tst3Rows.isEmpty && palletInfo.isNotEmpty) {
        final pi = palletInfo.first;
        await _db.execute(
          "INSERT INTO IV_0250 (Loct, PCode, PGroup, Batch, PName, Unit, Run, "
          "Status, OpenQty, InputQty, OutputQty, OnHand, Pallet, MFGDate, "
          "EXPDate, AddUser, AddDate, AddTime) "
          "VALUES (@Loct, @PCode, @PGroup, @Batch, @PName, @Unit, @Run, "
          "@Status, @OpenQty, @InputQty, @OutputQty, @OnHand, @Pallet, "
          "@MFGDate, @EXPDate, @AddUser, @AddDate, @AddTime)",
          {
            '@Loct': 'TST3',
            '@PCode': _dbStr(pi['PCode']),
            '@PGroup': _dbStr(pi['PGroup']),
            '@Batch': _batchNo,
            '@PName': _dbStr(pi['PName']),
            '@Unit': _dbStr(pi['Unit']),
            '@Run': _dbStr(pi['Run']),
            '@Status': _dbStr(pi['Status']),
            '@OpenQty': 0,
            '@InputQty': balance,
            '@OutputQty': 0,
            '@OnHand': balance,
            '@Pallet': palletNo,
            '@MFGDate': _dbStr(pi['MFGDate']),
            '@EXPDate': _dbStr(pi['EXPDate']),
            '@AddUser': user,
            '@AddDate': dateStr,
            '@AddTime': timeStr,
          },
        );
      } else if (tst3Rows.isNotEmpty) {
        final curOnHand = _dbDouble(tst3Rows.first['OnHand']);
        final curInput = _dbDouble(tst3Rows.first['InputQty']);
        await _db.execute(
          "UPDATE IV_0250 SET OnHand=@OnHand, InputQty=@InputQty, "
          "Pallet=@Pallet, EditUser=@EditUser, EditDate=@EditDate, "
          "EditTime=@EditTime "
          "WHERE Loct='TST3' AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet",
          {
            '@OnHand': curOnHand + balance,
            '@InputQty': curInput + balance,
            '@Pallet': palletNo,
            '@EditUser': user,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Batch': _batchNo,
            '@Run': _runInternal,
          },
        );
      }
    } catch (e) {
      _showMsg('Error while adding TST3: $e');
    }

    // Generate sequence number and log to TA_LOC0400
    final serialNumber = await _seqCheck();
    if (serialNumber.isNotEmpty && palletInfo.isNotEmpty) {
      final pi = palletInfo.first;
      try {
        final countSno = await _db.executeScalar(
          "SELECT COUNT(SNo) FROM TA_LOC0400 WHERE SNo=@SNo AND Rack=@Rack AND NRack=@NRack",
          {'@SNo': serialNumber, '@Rack': rackNo, '@NRack': 'TST3'},
        );
        if (_dbDouble(countSno) == 0) {
          await _db.execute(
            "INSERT INTO TA_LOC0400 (SNo, Rack, NRack, BN, Run, PCode, PName, "
            "PGroup, PltNo, Qty, Unit, Remark, AddUser, AddDate, AddTime) "
            "VALUES (@SNo, @Rack, @NRack, @BN, @Run, @PCode, @PName, "
            "@PGroup, @PltNo, @Qty, @Unit, @Remark, @AddUser, @AddDate, @AddTime)",
            {
              '@SNo': serialNumber,
              '@Rack': rackNo,
              '@NRack': 'TST3',
              '@BN': _dbStr(pi['Batch']),
              '@Run': _dbStr(pi['Run']),
              '@PCode': _dbStr(pi['PCode']),
              '@PName': _dbStr(pi['PName']),
              '@PGroup': _dbStr(pi['PGroup']),
              '@PltNo': palletNo,
              '@Qty': balance,
              '@Unit': _dbStr(pi['Unit']),
              '@Remark': 'Mobile Scanner Outbound',
              '@AddUser': user,
              '@AddDate': dateStr,
              '@AddTime': timeStr,
            },
          );
        } else {
          // Update existing log
          await _db.execute(
            "UPDATE TA_LOC0400 SET Rack=@Rack, NRack=@NRack, Qty=@Qty, "
            "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
            "WHERE SNo=@SNo AND Rack=@Rack AND NRack=@NRack",
            {
              '@Qty': balance,
              '@SNo': serialNumber,
              '@Rack': rackNo,
              '@NRack': 'TST3',
              '@EditUser': user,
              '@EditDate': dateStr,
              '@EditTime': timeStr,
            },
          );
        }
      } catch (e) {
        _showMsg('Error while updating TA_LOC0400: $e');
      }
    }
  }

  /// Update DO_0070 (preparation details) - insert or update
  Future<void> _updateDO0070(
    String daNo,
    String palletNo,
    String rackNo,
    double preparedQty,
    String user,
    String dateStr,
    String timeStr,
  ) async {
    try {
      await _db.connect();

      // Read DA data from DO_0020
      final daData = await _db.query(
        "SELECT * FROM DO_0020 WHERE DANo=@DANo AND Batch=@Batch "
        "AND Run=@Run AND PCode=@PCode",
        {
          '@DANo': daNo,
          '@Batch': _batch,
          '@Run': _runInternal,
          '@PCode': _pCode,
        },
      );

      if (daData.isEmpty) return;
      final da = daData.first;

      // Check existing DO_0070 row
      final existing = await _db.query(
        "SELECT SelQty FROM DO_0070 WHERE DANo=@DANo AND Batch=@Batch "
        "AND Run=@Run AND Rack=@Rack",
        {
          '@DANo': daNo,
          '@Batch': _batch,
          '@Run': _runInternal,
          '@Rack': rackNo,
        },
      );

      if (existing.isEmpty) {
        // Insert new
        await _db.execute(
          "INSERT INTO DO_0070 (DANo, OrderNo, PCode, Batch, Run, Rack, "
          "CustNo, CustName, Address, PName, PGroup, DAQty, OrderQty, "
          "SelQty, Unit, Status, etd, Pallet, AddUser, AddDate, AddTime) "
          "VALUES (@DANo, @OrderNo, @PCode, @Batch, @Run, @Rack, "
          "@CustNo, @CustName, @Address, @PName, @PGroup, @DAQty, "
          "@OrderQty, @SelQty, @Unit, @Status, @etd, @Pallet, "
          "@AddUser, @AddDate, @AddTime)",
          {
            '@DANo': daNo.toUpperCase(),
            '@OrderNo': _dbStr(da['OrderNo']),
            '@PCode': _dbStr(da['PCode']),
            '@Batch': _dbStr(da['Batch']),
            '@Run': _dbStr(da['Run']),
            '@Rack': rackNo,
            '@CustNo': _dbStr(da['CustNo']),
            '@CustName': _dbStr(da['CustName']),
            '@Address': _dbStr(da['Address']),
            '@PName': _dbStr(da['PName']),
            '@PGroup': _dbStr(da['PGroup']),
            '@DAQty': _dbDouble(da['Quantity']),
            '@OrderQty': _dbDouble(da['OrderQty']),
            '@SelQty': preparedQty,
            '@Unit': _dbStr(da['Unit']),
            '@Status': _dbStr(da['Status']),
            '@etd': _dbStr(da['ETD']),
            '@Pallet': palletNo.toUpperCase(),
            '@AddUser': user,
            '@AddDate': dateStr,
            '@AddTime': timeStr,
          },
        );
      } else {
        // Update existing — full update matching VB.NET
        final currentSelQty = _dbDouble(existing.first['SelQty']);
        await _db.execute(
          "UPDATE DO_0070 SET DANo=@DANo, OrderNo=@OrderNo, PCode=@PCode, "
          "Batch=@Batch, Run=@Run, Rack=@Rack, CustNo=@CustNo, "
          "CustName=@CustName, Address=@Address, PName=@PName, "
          "PGroup=@PGroup, DAQty=@DAQty, OrderQty=@OrderQty, "
          "SelQty=@SelQty, Unit=@Unit, Status=@Status, etd=@etd, "
          "Pallet=@Pallet, EditUser=@EditUser, EditDate=@EditDate, "
          "EditTime=@EditTime "
          "WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND Rack=@Rack",
          {
            '@DANo': daNo.toUpperCase(),
            '@OrderNo': _dbStr(da['OrderNo']),
            '@PCode': _dbStr(da['PCode']),
            '@Batch': _dbStr(da['Batch']),
            '@Run': _dbStr(da['Run']),
            '@Rack': rackNo,
            '@CustNo': _dbStr(da['CustNo']),
            '@CustName': _dbStr(da['CustName']),
            '@Address': _dbStr(da['Address']),
            '@PName': _dbStr(da['PName']),
            '@PGroup': _dbStr(da['PGroup']),
            '@DAQty': _dbDouble(da['Quantity']),
            '@OrderQty': _dbDouble(da['OrderQty']),
            '@SelQty': currentSelQty + preparedQty,
            '@Unit': _dbStr(da['Unit']),
            '@Status': _dbStr(da['Status']),
            '@etd': _dbStr(da['ETD']),
            '@Pallet': palletNo.toUpperCase(),
            '@EditUser': user,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
          },
        );
      }
    } catch (e) {
      _showMsg('Error while updating DO_0070: $e');
    }
  }

  /// Update TA_LOC0300 (outbound log) - insert or update
  Future<void> _updateTaLoc0300(
    String daNo,
    String palletNo,
    String rackNo,
    double preparedQty,
    String user,
    String dateStr,
    String timeStr,
  ) async {
    try {
      await _db.connect();

      // Read PGroup from PD_0010
      String pGroup = '';
      final pgRows = await _db.query(
        "SELECT PGroup FROM PD_0010 WHERE Batch=@Batch",
        {'@Batch': _batch},
      );
      if (pgRows.isNotEmpty) {
        pGroup = _dbStr(pgRows.first['PGroup']);
      }

      // Read DA data
      final daData = await _db.query(
        "SELECT * FROM DO_0020 WHERE DANo=@DANo AND Batch=@Batch "
        "AND Run=@Run AND PCode=@PCode",
        {
          '@DANo': daNo,
          '@Batch': _batch,
          '@Run': _runInternal,
          '@PCode': _pCode,
        },
      );
      if (daData.isEmpty) return;
      final da = daData.first;

      // Check existing
      final existingLog = await _db.query(
        "SELECT * FROM TA_LOC0300 WHERE DANo=@DANo AND Batch=@Batch "
        "AND Run=@Run AND Pallet=@Pallet",
        {
          '@DANo': daNo.trim(),
          '@Batch': _batch,
          '@Run': _runInternal,
          '@Pallet': palletNo,
        },
      );

      if (existingLog.isEmpty) {
        await _db.execute(
          "INSERT INTO TA_LOC0300 (PCode, PName, PGroup, DANo, Cust, Batch, "
          "Run, Pallet, Rack, Qty, Unit, AddUser, AddDate, AddTime) "
          "VALUES (@PCode, @PName, @PGroup, @DANo, @Cust, @Batch, @Run, "
          "@Pallet, @Rack, @Qty, @Unit, @AddUser, @AddDate, @AddTime)",
          {
            '@PCode': _dbStr(da['PCode']),
            '@PName': _dbStr(da['PName']),
            '@PGroup': pGroup,
            '@DANo': _dbStr(da['DANo']),
            '@Cust': _dbStr(da['CustNo']),
            '@Batch': _dbStr(da['Batch']),
            '@Run': _dbStr(da['Run']),
            '@Pallet': palletNo.trim(),
            '@Rack': rackNo,
            '@Qty': preparedQty,
            '@Unit': _dbStr(da['Unit']),
            '@AddUser': user,
            '@AddDate': dateStr,
            '@AddTime': timeStr,
          },
        );
      } else {
        // Update log — full update matching VB.NET
        await _db.execute(
          "UPDATE TA_LOC0300 SET PCode=@PCode, PName=@PName, PGroup=@PGroup, "
          "DANo=@DANo, Cust=@Cust, Batch=@Batch, Run=@Run, "
          "Pallet=@Pallet, Rack=@Rack, Qty=@Qty, Unit=@Unit, "
          "EditUser=@EditUser, EditDate=@EditDate, AddTime=@EditTime "
          "WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run "
          "AND PCode=@PCode AND Pallet=@Pallet",
          {
            '@PCode': _dbStr(da['PCode']),
            '@PName': _dbStr(da['PName']),
            '@PGroup': pGroup,
            '@DANo': _dbStr(da['DANo']),
            '@Cust': _dbStr(da['CustNo']),
            '@Batch': _dbStr(da['Batch']),
            '@Run': _dbStr(da['Run']),
            '@Pallet': palletNo.trim(),
            '@Rack': rackNo,
            '@Qty': preparedQty,
            '@Unit': _dbStr(da['Unit']),
            '@EditUser': user,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
          },
        );
      }
    } catch (e) {
      _showMsg('Error while updating TA_LOC0300: $e');
    }
  }

  /// Update PD_0800: Rack_Out and SORack
  Future<void> _updatePD0800(double preparedQty) async {
    try {
      await _db.connect();
      final rows = await _db.query(
        "SELECT Rack_Out, SORack FROM PD_0800 "
        "WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {'@Batch': _batch, '@Run': _runInternal, '@PCode': _pCode},
      );
      if (rows.isEmpty) return;

      double rackOut = _dbDouble(rows.first['Rack_Out']);
      double soRack = _dbDouble(rows.first['SORack']);

      await _db.execute(
        "UPDATE PD_0800 SET Rack_Out=@Rack_Out, SORack=@SORack "
        "WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {
          '@Rack_Out': rackOut + preparedQty,
          '@SORack': soRack - preparedQty,
          '@Batch': _batch,
          '@Run': _runInternal,
          '@PCode': _pCode,
        },
      );
    } catch (e) {
      _showMsg('Error while updating PD_0800: $e');
    }
  }

  /// Check total prepared from local DA_Confirm
  Future<void> _checkTotalPreparedLocal(String daNo) async {
    try {
      final sumPrepared =
          await _localDb.getSumPrepared(daNo, _batch, _runInternal, _pCode);
      if (!mounted) return;
      setState(() {
        _totalQty = sumPrepared.toStringAsFixed(0);
        _outstandingDA =
            (_val(_daQty) - sumPrepared).toStringAsFixed(0);
      });

      if (_val(_outstandingDA) <= 0) {
        if (!mounted) return;
        setState(() => _outstandingColor = Colors.green);
        await _db.execute(
          "UPDATE DO_0020 SET Status='Confirmed' "
          "WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run",
          {'@DANo': daNo, '@Batch': _batch, '@Run': _runInternal},
        );
      } else {
        _clearPalletFieldsKeepDA();
        _palletFocus.requestFocus();
      }
    } catch (e) {
      _showMsg('Error while checking prprd qty local: $e');
    }
  }

  /// Check total prepared from DO_0070
  Future<void> _checkTotalPreparedDO0070(String daNo) async {
    try {
      await _db.connect();
      final rows = await _db.query(
        "SELECT SUM(SelQty) AS SumOfPrepared FROM DO_0070 "
        "WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {
          '@DANo': daNo,
          '@Batch': _batch,
          '@Run': _runInternal,
          '@PCode': _pCode,
        },
      );

      if (rows.isNotEmpty && rows.first['SumOfPrepared'] != null) {
        final total = _dbDouble(rows.first['SumOfPrepared']);
        final outstanding = _val(_daQty) - total;
        if (!mounted) return;
        setState(() {
          _totalQty = total.toStringAsFixed(0);
          _outstandingDA = outstanding.toStringAsFixed(0);
        });

        if (outstanding <= 0) {
          await _db.execute(
            "UPDATE DO_0020 SET Status='Confirmed' "
            "WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
            {
              '@DANo': daNo,
              '@Batch': _batch,
              '@Run': _runInternal,
              '@PCode': _pCode,
            },
          );
          _showMsg('Product & Batch ini telah disiapkan', isInfo: true);
          _bodyNull();
          _palletFocus.requestFocus();
        } else {
          _clearPalletFieldsKeepDA();
          _palletFocus.requestFocus();
        }
      }
    } catch (e) {
      _showMsg('Error while checking prprd qty DO_0070: $e');
    }
  }

  /// Check if all DA lines are done
  Future<void> _checkDAComplete(String daNo) async {
    try {
      await _db.connect();
      final count = await _db.executeScalar(
        "SELECT COUNT(DANo) FROM DO_0020 WHERE DANo=@DANo AND Status=@Status",
        {'@DANo': daNo.trim(), '@Status': 'Open'},
      );
      if (_dbDouble(count) == 0) {
        _showMsg('DA sudah habis Prepare', isInfo: true);
        _bodyNullAll();
        _daNoFocus.requestFocus();
      } else {
        _clearPalletFieldsKeepDA();
        _palletFocus.requestFocus();
      }
    } catch (e) {
      _showMsg('Error while checking preparation status: $e');
    }
  }

  // ---------------------------------------------------------------------------
  // 4b. LOOSE pallet preparation
  // ---------------------------------------------------------------------------
  Future<void> _prepareLoose() async {
    final daNo = _daNoController.text.trim();
    final palletNo = _palletController.text.trim();
    final rackNo = _rackController.text.trim().toUpperCase();
    final user = _user();
    final now = DateTime.now();
    final dateStr = DateFormat('yyyy-MM-dd').format(now);
    final timeStr = DateFormat('HH:mm:ss').format(now);

    await _db.connect();
    await _localDb.initialize();

    // Read all entries from TA_PLL001
    final looseEntries = await _db.query(
      "SELECT * FROM TA_PLL001 WHERE PltNo=@PltNo",
      {'@PltNo': palletNo},
    );

    for (int i = 0; i < looseEntries.length; i++) {
      final entry = looseEntries[i];
      _batch = _dbStr(entry['Batch']);
      _runInternal = _dbStr(entry['Run']);

      // Get PCode from TA_PLT001 or TA_PLT003
      try {
        final plt001 = await _db.query(
          "SELECT PCode FROM TA_PLT001 WHERE PltNo=@PltNo",
          {'@PltNo': palletNo},
        );
        if (plt001.isNotEmpty) {
          _pCode = _dbStr(plt001.first['PCode']);
        } else {
          final plt003 = await _db.query(
            "SELECT PCode FROM TA_PLT003 WHERE PltNo=@PltNo",
            {'@PltNo': palletNo},
          );
          if (plt003.isNotEmpty) {
            _pCode = _dbStr(plt003.first['PCode']);
          }
        }
      } catch (e) {
        _showMsg('Error while checking PCode: $e');
      }

      // Check DA qty
      await _db.connect();
      final daSumRows = await _db.query(
        "SELECT DANo, Batch, Run, SUM(Quantity) AS SumOfQty "
        "FROM DO_0020 WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run "
        "AND Status='Open' GROUP BY DANo, Batch, Run",
        {'@DANo': daNo, '@Batch': _batch, '@Run': _runInternal},
      );

      if (daSumRows.isEmpty) continue; // Batch not in DA, skip

      double daQty = _dbDouble(daSumRows.first['SumOfQty']);
      if (!mounted) return;
      setState(() {
        _daQty = daQty.toStringAsFixed(0);
        _outstandingDA = daQty.toStringAsFixed(0);
      });

      // Check local preparation
      double totalPrepared = 0;
      try {
        final localRows =
            await _localDb.getDAConfirm(daNo, _batch, _runInternal, _pCode);
        if (localRows.isNotEmpty) {
          final prep = _dbDouble(localRows.first['Prepared']);
          if (prep > 0) {
            totalPrepared = prep;
            if (!mounted) return;
            setState(() {
              _totalQty = totalPrepared.toStringAsFixed(0);
              _outstandingDA =
                  (daQty - totalPrepared).toStringAsFixed(0);
            });
          } else {
            if (!mounted) return;
            setState(() {
              _totalQty = '0';
              _preparedQtyController.text = '0';
            });
          }
        }
      } catch (e) {
        _showMsg('Error while Checking Preparation Local: $e');
      }

      // Check DO_0070 preparation
      await _db.connect();
      try {
        final do0070 = await _db.query(
          "SELECT SUM(SelQty) AS SumOfPrepared FROM DO_0070 "
          "WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
          {
            '@DANo': daNo,
            '@Batch': _batch,
            '@Run': _runInternal,
            '@PCode': _pCode,
          },
        );
        if (do0070.isNotEmpty && do0070.first['SumOfPrepared'] != null) {
          totalPrepared = _dbDouble(do0070.first['SumOfPrepared']);
          final outstanding = daQty - totalPrepared;
          if (!mounted) return;
          setState(() {
            _totalQty = totalPrepared.toStringAsFixed(0);
            _outstandingDA = outstanding.toStringAsFixed(0);
          });
          if (outstanding <= 0) {
            await _db.execute(
              "UPDATE DO_0020 SET Status='Confirmed' "
              "WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
              {
                '@DANo': daNo,
                '@Batch': _batch,
                '@Run': _runInternal,
                '@PCode': _pCode,
              },
            );
            if (!mounted) return;
            setState(() => _outstandingColor = Colors.green);
            _showMsg('Product & Batch ini telah disiapkan', isInfo: true);
            _bodyNull();
            continue; // Next loop entry
          }
        }
      } catch (e) {
        _showMsg('Error while checking prprd qty DO_0070: $e');
      }

      // Check stock at rack
      await _db.connect();
      final stockRows = await _db.query(
        "SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch "
        "AND Run=@Run AND PCode=@PCode AND Pallet=@Pallet AND OnHand > 0",
        {
          '@Loct': rackNo,
          '@Batch': _batch,
          '@Run': _runInternal,
          '@PCode': _pCode,
          '@Pallet': palletNo,
        },
      );

      if (stockRows.isEmpty) {
        // No stock at location — skip (GoTo STEP1 in VB.NET)
        continue;
      }

      double stockOnHand = _dbDouble(stockRows.first['OnHand']);
      if (!mounted) return;
      setState(() {
        _palletQty = stockOnHand.toStringAsFixed(0);
        _preparedQtyController.text = stockOnHand.toStringAsFixed(0);
      });
      double palletQty = stockOnHand;
      double preparedQty = stockOnHand;

      // Rack out
      try {
        final curOutput = _dbDouble(stockRows.first['OutputQty']);
        await _db.execute(
          "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=@OnHand, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run "
          "AND Pallet=@Pallet AND OnHand > 0",
          {
            '@OutputQty': curOutput + preparedQty,
            '@OnHand': stockOnHand - preparedQty,
            '@EditUser': user,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Loct': rackNo,
            '@Batch': _batch,
            '@Run': _runInternal,
            '@Pallet': palletNo,
          },
        );
      } catch (e) {
        _showMsg('Error while Racking out: $e');
      }

      // Update local DA_Confirm
      try {
        final localRows =
            await _localDb.getDAConfirm(daNo, _batch, _runInternal, _pCode);
        if (localRows.isNotEmpty) {
          final curPrepared = _dbDouble(localRows.first['Prepared']);
          await _localDb.updateDAConfirmPrepared(
            daNo,
            _batch,
            _runInternal,
            _pCode,
            curPrepared + preparedQty,
            user,
          );
        }
      } catch (e) {
        _showMsg('Error while Updating local log: $e');
      }

      // Handle balance -> TST3
      if (palletQty > preparedQty) {
        await _handleLooseBalanceToTST3(
            palletNo, rackNo, palletQty, preparedQty, user, dateStr, timeStr);
      }

      // Update DO_0070 for loose
      await _updateDO0070(
          daNo, palletNo, rackNo, preparedQty, user, dateStr, timeStr);

      // Update TA_LOC0300
      await _updateTaLoc0300(
          daNo, palletNo, rackNo, preparedQty, user, dateStr, timeStr);

      // Update PD_0800
      await _updatePD0800(preparedQty);

      // Check total prepared local
      try {
        final sumPrepared = await _localDb.getSumPrepared(
            daNo, _batch, _runInternal, _pCode);
        if (!mounted) return;
        setState(() {
          _totalQty = sumPrepared.toStringAsFixed(0);
          _outstandingDA =
              (daQty - sumPrepared).toStringAsFixed(0);
        });
        if (_val(_outstandingDA) <= 0) {
          await _db.execute(
            "UPDATE DO_0020 SET Status='Confirmed' "
            "WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
            {
              '@DANo': daNo,
              '@Batch': _batch,
              '@Run': _runInternal,
              '@PCode': _pCode,
            },
          );
        }
      } catch (e) {
        _showMsg('Error while Checking total prepared local: $e');
      }

      // Check DO_0070 total
      await _db.connect();
      try {
        final do0070 = await _db.query(
          "SELECT SUM(SelQty) AS SumOfPrepared FROM DO_0070 "
          "WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
          {
            '@DANo': daNo,
            '@Batch': _batch,
            '@Run': _runInternal,
            '@PCode': _pCode,
          },
        );
        if (do0070.isNotEmpty && do0070.first['SumOfPrepared'] != null) {
          final total = _dbDouble(do0070.first['SumOfPrepared']);
          final outstanding = daQty - total;
          if (!mounted) return;
          setState(() {
            _totalQty = total.toStringAsFixed(0);
            _outstandingDA = outstanding.toStringAsFixed(0);
          });
          if (outstanding <= 0) {
            await _db.execute(
              "UPDATE DO_0020 SET Status='Confirmed' "
              "WHERE DANo=@DANo AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
              {
                '@DANo': daNo,
                '@Batch': _batch,
                '@Run': _runInternal,
                '@PCode': _pCode,
              },
            );
            _showMsg('Product & Batch ini telah disiapkan', isInfo: true);
          }
        }
      } catch (e) {
        _showMsg('Error while checking prprd qty DO_0070: $e');
      }

      // Check all DA
      await _db.connect();
      try {
        final count = await _db.executeScalar(
          "SELECT COUNT(DANo) FROM DO_0020 WHERE DANo=@DANo AND Status=@Status",
          {'@DANo': daNo.trim(), '@Status': 'Open'},
        );
        if (_dbDouble(count) == 0) {
          _showMsg('DA sudah habis Prepare', isInfo: true);
          _bodyNullAll();
          _daNoFocus.requestFocus();
          return; // Exit loop
        } else {
          if (!mounted) return;
          setState(() => _outstandingColor = Colors.red);
        }
      } catch (e) {
        _showMsg('Error while Checking order status DO_0020: $e');
      }

      // Clear for last iteration
      if (i == looseEntries.length - 1) {
        if (!mounted) return;
        setState(() {
          _palletController.clear();
          _rackController.clear();
          _totalQty = '';
          _outstandingDA = '';
          _outstandingColor = Colors.transparent;
        });
      }
    }

    _showMsg('Outbound Pallet Loose Selesai', isInfo: true);
    _bodyNull();
    _palletFocus.requestFocus();
  }

  /// Handle loose pallet balance -> TST3 (same logic as normal but with PCode)
  Future<void> _handleLooseBalanceToTST3(
    String palletNo,
    String rackNo,
    double palletQty,
    double preparedQty,
    String user,
    String dateStr,
    String timeStr,
  ) async {
    final balance = palletQty - preparedQty;

    try {
      // Read pallet info
      final palletInfo = await _db.query(
        "SELECT * FROM IV_0250 WHERE Batch=@Batch AND Run=@Run "
        "AND Loct=@Loct AND PCode=@PCode AND Pallet=@Pallet",
        {
          '@Batch': _batch,
          '@Run': _runInternal,
          '@PCode': _pCode,
          '@Loct': rackNo,
          '@Pallet': palletNo,
        },
      );

      // Rack out remaining
      final ivRows = await _db.query(
        "SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch "
        "AND Run=@Run AND Pallet=@Pallet AND PCode=@PCode AND OnHand > 0",
        {
          '@Loct': rackNo,
          '@Batch': _batch,
          '@Run': _runInternal,
          '@PCode': _pCode,
          '@Pallet': palletNo,
        },
      );
      if (ivRows.isNotEmpty) {
        final curOutput = _dbDouble(ivRows.first['OutputQty']);
        final curOnHand = _dbDouble(ivRows.first['OnHand']);
        await _db.execute(
          "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=@OnHand, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run "
          "AND Pallet=@Pallet AND PCode=@PCode AND OnHand > 0",
          {
            '@OutputQty': curOutput + balance,
            '@OnHand': curOnHand - balance,
            '@EditUser': user,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Loct': rackNo,
            '@Batch': _batch,
            '@Run': _runInternal,
            '@PCode': _pCode,
            '@Pallet': palletNo,
          },
        );
      }

      // TST3
      final tst3Rows = await _db.query(
        "SELECT * FROM IV_0250 WHERE Batch=@Batch AND Run=@Run "
        "AND Loct='TST3' AND Pallet=@Pallet AND PCode=@PCode",
        {
          '@Batch': _batch,
          '@Run': _runInternal,
          '@PCode': _pCode,
          '@Pallet': palletNo,
        },
      );

      if (tst3Rows.isEmpty && palletInfo.isNotEmpty) {
        final pi = palletInfo.first;
        await _db.execute(
          "INSERT INTO IV_0250 (Loct, PCode, PGroup, Batch, PName, Unit, Run, "
          "Status, OpenQty, InputQty, OutputQty, OnHand, Pallet, MFGDate, "
          "EXPDate, AddUser, AddDate, AddTime) "
          "VALUES (@Loct, @PCode, @PGroup, @Batch, @PName, @Unit, @Run, "
          "@Status, @OpenQty, @InputQty, @OutputQty, @OnHand, @Pallet, "
          "@MFGDate, @EXPDate, @AddUser, @AddDate, @AddTime)",
          {
            '@Loct': 'TST3',
            '@PCode': _dbStr(pi['PCode']),
            '@PGroup': _dbStr(pi['PGroup']),
            '@Batch': _batch,
            '@PName': _dbStr(pi['PName']),
            '@Unit': _dbStr(pi['Unit']),
            '@Run': _dbStr(pi['Run']),
            '@Status': _dbStr(pi['Status']),
            '@OpenQty': 0,
            '@InputQty': balance,
            '@OutputQty': 0,
            '@OnHand': balance,
            '@Pallet': palletNo,
            '@MFGDate': _dbStr(pi['MFGDate']),
            '@EXPDate': _dbStr(pi['EXPDate']),
            '@AddUser': user,
            '@AddDate': dateStr,
            '@AddTime': timeStr,
          },
        );
      } else if (tst3Rows.isNotEmpty) {
        final curOnHand = _dbDouble(tst3Rows.first['OnHand']);
        final curInput = _dbDouble(tst3Rows.first['InputQty']);
        await _db.execute(
          "UPDATE IV_0250 SET OnHand=@OnHand, InputQty=@InputQty, "
          "Pallet=@Pallet, EditUser=@EditUser, EditDate=@EditDate, "
          "EditTime=@EditTime "
          "WHERE Loct='TST3' AND Batch=@Batch AND Run=@Run "
          "AND Pallet=@Pallet AND PCode=@PCode",
          {
            '@OnHand': curOnHand + balance,
            '@InputQty': curInput + balance,
            '@Pallet': palletNo,
            '@EditUser': user,
            '@EditDate': dateStr,
            '@EditTime': timeStr,
            '@Batch': _batch,
            '@Run': _runInternal,
            '@PCode': _pCode,
          },
        );
      }
    } catch (e) {
      _showMsg('Error while Outputing to TST3: $e');
    }
  }

  // ---------------------------------------------------------------------------
  // Sequence number generator (Seq_Check from VB.NET)
  // ---------------------------------------------------------------------------
  Future<String> _seqCheck() async {
    try {
      await _db.connect();
      final yearSuffix = DateTime.now().year.toString().substring(2);

      final rows = await _db.query(
        "SELECT * FROM SY_0040 WHERE KeyCD1=@KeyCD1 AND KeyCD2=@KeyCD2",
        {'@KeyCD1': int.parse(yearSuffix), '@KeyCD2': '52'},
      );

      if (rows.isEmpty) {
        _showMsg('No sequence Number !');
        return '';
      }

      final currentSeq = _dbDouble(rows.first['MSEQ']).toInt();
      final newSeq = currentSeq + 1;

      await _db.execute(
        "UPDATE SY_0040 SET MSEQ=@MSEQ WHERE KeyCD1=@KeyCD1 AND KeyCD2=@KeyCD2",
        {'@MSEQ': newSeq, '@KeyCD1': int.parse(yearSuffix), '@KeyCD2': '52'},
      );

      // Format: MG + yearSuffix + 4-digit sequence (e.g., MG260001)
      final formatted = 'MG$yearSuffix${currentSeq.toString().padLeft(4, '0')}';
      return formatted;
    } catch (e) {
      _showMsg('Error generating sequence: $e');
      return '';
    }
  }

  // ---------------------------------------------------------------------------
  // Clear helpers
  // ---------------------------------------------------------------------------
  void _bodyNullAll() {
    if (!mounted) return;
    setState(() {
      _daNoController.clear();
      _method = '';
      _palletController.clear();
      _batchNo = '';
      _run = '';
      _daQty = '';
      _palletQty = '';
      _rackController.clear();
      _preparedQtyController.clear();
      _totalQty = '';
      _outstandingDA = '';
      _batchList = [];
      _locationList = [];
      _selectedBatchIndex = -1;
      _selectedLocationIndex = -1;
      _qsStatus = '';
      _qsColor = Colors.transparent;
      _outstandingColor = Colors.transparent;
    });
  }

  void _bodyNull() {
    if (_palletType == 'NORMAL') {
      if (!mounted) return;
      setState(() {
        _palletController.clear();
        _batchNo = '';
        _run = '';
        _daQty = '';
        _palletQty = '';
        _rackController.clear();
        _preparedQtyController.clear();
        _totalQty = '';
        _outstandingDA = '';
        _batchList = [];
        _locationList = [];
        _selectedBatchIndex = -1;
        _selectedLocationIndex = -1;
        _qsStatus = '';
        _qsColor = Colors.transparent;
      });
    } else {
      if (!mounted) return;
      setState(() {
        _palletController.clear();
        _batchNo = '';
        _run = '';
        _daQty = '';
        _palletQty = '';
      });
    }
  }

  void _clearPalletFields() {
    if (!mounted) return;
    setState(() {
      _palletController.clear();
      _batchNo = '';
      _run = '';
      _palletQty = '';
      _rackController.clear();
      _preparedQtyController.clear();
      _qsStatus = '';
      _qsColor = Colors.transparent;
    });
    _palletFocus.requestFocus();
  }

  void _clearPalletFieldsKeepDA() {
    if (!mounted) return;
    setState(() {
      _palletController.clear();
      _batchNo = '';
      _run = '';
      _daQty = '';
      _palletQty = '';
      _rackController.clear();
      _qsStatus = '';
      _qsColor = Colors.transparent;
    });
  }

  void _onCancel() {
    _bodyNullAll();
    _daNoFocus.requestFocus();
  }

  /// Clear all data on form and reset focus to DA No field
  void _onClearAll() {
    _bodyNullAll();
    _daNoFocus.requestFocus();
  }

  // ---------------------------------------------------------------------------
  // Clear local log (btnClearLog)
  // ---------------------------------------------------------------------------
  Future<void> _clearLog() async {
    try {
      await _localDb.initialize();
      if (_localDb.isAvailable) {
        final db = _localDb.database;
        await db.delete('DA_Confirm');
      }
      await ActivityLogService.log(action: 'DA_CLEAR_LOG', empNo: _user());
      _showMsg('Log cleared', isInfo: true);
      _bodyNullAll();
      _daNoFocus.requestFocus();
    } catch (e) {
      _showMsg('Ralat: $e');
    }
  }

  // ---------------------------------------------------------------------------
  // Batch list tap -> load locations
  // ---------------------------------------------------------------------------
  Future<void> _onBatchTap(String batch, String run) async {
    final tappedIndex = _batchList.indexWhere(
        (b) => b['Batch'] == batch && b['Run'] == run);
    try {
      await _db.connect();
      final rows = await _db.query(
        "SELECT Loct, Pallet, OnHand FROM IV_0250 "
        "WHERE Batch=@Batch AND Run=@Run AND OnHand > 0",
        {'@Batch': batch, '@Run': run},
      );
      if (!mounted) return;
      setState(() {
        _selectedBatchIndex = tappedIndex;
        _selectedLocationIndex = -1;
        _locationList = rows
            .map((r) => {
                  'Loct': _dbStr(r['Loct']),
                  'Pallet': _dbStr(r['Pallet']),
                  'Qty': _dbDouble(r['OnHand']).toStringAsFixed(0),
                })
            .toList();
      });
    } catch (e) {
      _showMsg('Ralat: $e');
    }
    _palletFocus.requestFocus();
  }

  // ---------------------------------------------------------------------------
  // Location list tap -> auto-fill pallet details
  // ---------------------------------------------------------------------------
  Future<void> _onLocationTap(String pallet, String loct) async {
    if (pallet.isEmpty) return;

    final tappedIndex = _locationList.indexWhere(
        (l) => l['Pallet'] == pallet && l['Loct'] == loct);
    if (!mounted) return;
    setState(() {
      _selectedLocationIndex = tappedIndex;
    });

    // Determine Kulim based on pallet length (normal=7, Kulim=8+)
    _kulim = pallet.length >= 8;

    _palletController.text = pallet;
    await _processPallet(pallet);

    // If pallet was valid (batch populated) and rack provided, process rack
    if (loct.isNotEmpty && _batchNo.isNotEmpty) {
      _rackController.text = loct;
      await _processRack(loct);
    }
  }

  // ---------------------------------------------------------------------------
  // Camera scanner
  // ---------------------------------------------------------------------------
  Future<void> _openScanner() async {
    final barcode = await BarcodeScannerDialog.show(
      context,
      title: 'Imbas Barcode',
    );
    if (barcode != null && barcode.isNotEmpty) {
      await _processBarcode(barcode);
    }
  }

  // ---------------------------------------------------------------------------
  // Navigation to other screens
  // ---------------------------------------------------------------------------
  void _openLocation() {
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => LocationScreen(
          batch: _batchNo,
          daNo: _daNoController.text.trim(),
        ),
      ),
    );
  }

  void _openChangeRack() {
    Navigator.of(context).push(
      MaterialPageRoute(builder: (_) => const ChangeRackScreen()),
    );
  }

  void _setRackTST3() {
    _rackController.text = 'TST3';
    _preparedQtyFocus.requestFocus();
    _preparedQtyController.selection = TextSelection(
      baseOffset: 0,
      extentOffset: _preparedQtyController.text.length,
    );
  }

  // ---------------------------------------------------------------------------
  // PreparedQty Enter key handler
  // ---------------------------------------------------------------------------
  void _onPreparedQtySubmit(String value) {
    final daQty = _val(_daQty);
    final totalQty = _val(_totalQty);
    final preparedQty = _val(value);

    if (daQty - (totalQty + preparedQty) >= 0) {
      // OK — user can press Prepare
    } else {
      _showMsg('Jumlah DA Melebihi Preparation');
      _preparedQtyController.selection = TextSelection(
        baseOffset: 0,
        extentOffset: _preparedQtyController.text.length,
      );
      _preparedQtyFocus.requestFocus();
    }
  }

  // ---------------------------------------------------------------------------
  // BUILD
  // ---------------------------------------------------------------------------
 
  static final _cSmall = GoogleFonts.robotoCondensed(fontSize: 11);

  // Theme-aware helper styles (need BuildContext)
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
    final cs = Theme.of(context).colorScheme;
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Outbound DA',
      actions: [
        IconButton(
          icon: const Icon(Icons.qr_code_scanner),
          onPressed: _isLoading ? null : _openScanner,
          tooltip: 'Scan',
        ),
      ],
      body: GestureDetector(
        onTap: () => FocusScope.of(context).unfocus(),
        child: Padding(
          padding: const EdgeInsets.all(6.0),
          child: ScrollbarTheme(
            data: ScrollbarThemeData(
              thickness: WidgetStateProperty.all(6),
              radius: const Radius.circular(3),
              thumbColor: WidgetStateProperty.all(
                Theme.of(context).colorScheme.primary.withValues(alpha: 0.5),
              ),
              minThumbLength: 36,
            ),
            child: Scrollbar(
              controller: _mainScrollController,
              thumbVisibility: true,
              interactive: true,
              child: SingleChildScrollView(
                controller: _mainScrollController,
                padding: const EdgeInsets.only(right: 10),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                      // -- User info ---------------------------------
                      Text(
                        'User: ${auth.empNo ?? ""}@${auth.empName ?? ""}',
                        style: GoogleFonts.robotoCondensed(
                            fontSize: 11,
                            color: isDark
                                ? const Color(0xFF80CBC4) : Colors.blueGrey),
                      ),
                      const SizedBox(height: 4),

                      // ================================================
                      // CARD 1: DA Info
                      // ================================================
                      Card(
                        child: Padding(
                          padding: const EdgeInsets.all(8),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              Text('\u2460 DA Info', style: _cSectionTitle),
                              const SizedBox(height: 4),
                              Row(
                                children: [
                                  Expanded(
                                    child: ScanField(
                                      controller: _daNoController,
                                      focusNode: _daNoFocus,
                                      label: 'DA No',
                                      enabled: !_isLoading,
                                      onSubmitted: (v) => _loadDA(v),
                                      onScanPressed: _isLoading ? null : _openScanner,
                                      filled: true,
                                      fillColor: isDark ? const Color(0xFF343B47) : Colors.white,
                                      style: GoogleFonts.robotoCondensed(fontSize: 13, color: isDark ? const Color(0xFFE4E8EE) : Colors.black),
                                      labelStyle: GoogleFonts.robotoCondensed(fontSize: 12, color: isDark ? const Color(0xFF80CBC4) : const Color(0xFF616161)),
                                    ),
                                  ),
                                  const SizedBox(width: 4),
                                  SizedBox(
                                    width: 42,
                                    height: 42,
                                    child: IconButton(
                                      onPressed: _isLoading ? null : _onClearAll,
                                      icon: const Icon(Icons.clear, size: 20),
                                      tooltip: 'Clear All',
                                      style: IconButton.styleFrom(
                                        backgroundColor: Colors.red.shade400,
                                        foregroundColor: Colors.white,
                                        shape: RoundedRectangleBorder(
                                          borderRadius: BorderRadius.circular(4),
                                        ),
                                      ),
                                    ),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 4),
                              _buildReadOnlyRow('Method', _method),
                            ],
                          ),
                        ),
                      ),
                      const SizedBox(height: 4),
                      // Batch List & Location List  side by side
                      Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          // Batch List
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.stretch,
                              children: [
                                Text('Batch', style: _cSectionTitle),
                                const SizedBox(height: 2),
                                Container(
                                  height: 110,
                                  decoration: BoxDecoration(
                                    border: Border.all(
                                      color: isDark
                                          ? const Color(0xFF546E7A)
                                          : const Color(0xFF455A64),
                                    ),
                                    borderRadius: BorderRadius.circular(4),
                                  ),
                                  child: Column(
                                    children: [
                                      Container(
                                        padding: const EdgeInsets.symmetric(
                                            horizontal: 6, vertical: 3),
                                        decoration: BoxDecoration(
                                          color: cs.primary,
                                          borderRadius: const BorderRadius.vertical(
                                              top: Radius.circular(3)),
                                        ),
                                        child: Row(
                                          children: [
                                            Expanded(
                                                child: Text('Batch',
                                                    style: GoogleFonts.robotoCondensed(
                                                        fontSize: 10,
                                                        fontWeight: FontWeight.bold,
                                                        color: cs.onPrimary))),
                                            SizedBox(
                                                width: 30,
                                                child: Text('Run',
                                                    style: GoogleFonts.robotoCondensed(
                                                        fontSize: 10,
                                                        fontWeight: FontWeight.bold,
                                                        color: cs.onPrimary))),
                                          ],
                                        ),
                                      ),
                                      Expanded(
                                        child: _batchList.isEmpty
                                            ? Center(
                                                child: Text('No data',
                                                    style: GoogleFonts.robotoCondensed(
                                                        fontSize: 10,
                                                        color: Theme.of(context).colorScheme.onSurface.withValues(alpha: 0.5))))
                                            : Scrollbar(
                                                controller: _batchScrollController,
                                                thumbVisibility: true,
                                                interactive: true,
                                                thickness: 4,
                                                radius: const Radius.circular(2),
                                                child: ListView.builder(
                                                  controller: _batchScrollController,
                                                  itemCount: _batchList.length,
                                                  itemBuilder: (ctx, idx) {
                                                    final b = _batchList[idx];
                                                    final isBatchSelected = idx == _selectedBatchIndex;
                                                    return InkWell(
                                                      onTap: () => _onBatchTap(
                                                          b['Batch']!, b['Run']!),
                                                      child: Container(
                                                        color: isBatchSelected
                                                            ? cs.primaryContainer
                                                            : idx.isEven
                                                                ? cs.surface
                                                                : cs.surfaceContainerHighest,
                                                        padding: const EdgeInsets.symmetric(
                                                            horizontal: 6, vertical: 3),
                                                        child: Row(
                                                          children: [
                                                            Expanded(
                                                                child: Text(
                                                                    b['Batch'] ?? '',
                                                                    style: _cSmall)),
                                                            SizedBox(
                                                                width: 30,
                                                                child: Text(
                                                                    b['Run'] ?? '',
                                                                    style: _cSmall)),
                                                          ],
                                                        ),
                                                      ),
                                                    );
                                                  },
                                                ),
                                              ),
                                      ),
                                    ],
                                  ),
                                ),
                              ],
                            ),
                          ),
                          const SizedBox(width: 6),
                          // Location List
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.stretch,
                              children: [
                                Text('Locations', style: _cSectionTitle),
                                const SizedBox(height: 2),
                                Container(
                                  height: 110,
                                  decoration: BoxDecoration(
                                    border: Border.all(
                                      color: isDark
                                          ? const Color(0xFF546E7A)
                                          : const Color(0xFF455A64),
                                    ),
                                    borderRadius: BorderRadius.circular(4),
                                  ),
                                  child: Column(
                                    children: [
                                      Container(
                                        padding: const EdgeInsets.symmetric(
                                            horizontal: 6, vertical: 3),
                                        decoration: BoxDecoration(
                                          color: cs.primary,
                                          borderRadius: const BorderRadius.vertical(
                                              top: Radius.circular(3)),
                                        ),
                                        child: Row(
                                          children: [
                                            SizedBox(
                                                width: 40,
                                                child: Text('Loct',
                                                    style: GoogleFonts.robotoCondensed(
                                                        fontSize: 10,
                                                        fontWeight: FontWeight.bold,
                                                        color: cs.onPrimary))),
                                            Expanded(
                                                child: Text('Pallet',
                                                    style: GoogleFonts.robotoCondensed(
                                                        fontSize: 10,
                                                        fontWeight: FontWeight.bold,
                                                        color: cs.onPrimary))),
                                          ],
                                        ),
                                      ),
                                      Expanded(
                                        child: _locationList.isEmpty
                                            ? Center(
                                                child: Text('No data',
                                                    style: GoogleFonts.robotoCondensed(
                                                        fontSize: 10,
                                                        color: Theme.of(context).colorScheme.onSurface.withValues(alpha: 0.5))))
                                            : Scrollbar(
                                                controller: _locationScrollController,
                                                thumbVisibility: true,
                                                interactive: true,
                                                thickness: 4,
                                                radius: const Radius.circular(2),
                                                child: ListView.builder(
                                                  controller: _locationScrollController,
                                                  itemCount: _locationList.length,
                                                  itemBuilder: (ctx, idx) {
                                                    final l = _locationList[idx];
                                                    final isLocSelected = idx == _selectedLocationIndex;
                                                    return InkWell(
                                                      onTap: () => _onLocationTap(
                                                          l['Pallet'] ?? '',
                                                          l['Loct'] ?? ''),
                                                      child: Container(
                                                        color: isLocSelected
                                                            ? cs.primaryContainer
                                                            : idx.isEven
                                                                ? cs.surface
                                                                : cs.surfaceContainerHighest,
                                                        padding: const EdgeInsets.symmetric(
                                                            horizontal: 6, vertical: 3),
                                                        child: Row(
                                                          children: [
                                                            SizedBox(
                                                                width: 40,
                                                                child: Text(
                                                                    l['Loct'] ?? '',
                                                                    style: _cSmall)),
                                                            Expanded(
                                                                child: Text(
                                                                    l['Pallet'] ?? '',
                                                                    style: _cSmall)),
                                                          ],
                                                        ),
                                                      ),
                                                    );
                                                  },
                                                ),
                                              ),
                                      ),
                                    ],
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 4),

                      // ================================================
                      // CARD 2: Pallet / Batch Info
                      // ================================================
                      Card(
                        child: Padding(
                          padding: const EdgeInsets.all(8),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              Text('\u2461 Pallet / Batch', style: _cSectionTitle),
                              const SizedBox(height: 4),
                              ScanField(
                                controller: _palletController,
                                focusNode: _palletFocus,
                                label: 'Pallet No',
                                enabled: !_isLoading,
                                onSubmitted: (v) {
                                  if (_daNoController.text.isEmpty) {
                                    _showMsg('Tiada nombor DA');
                                    _daNoFocus.requestFocus();
                                    return;
                                  }
                                  _processPallet(v);
                                },
                                onScanPressed: _isLoading ? null : _openScanner,
                                filled: true,
                                fillColor: isDark ? const Color(0xFF343B47) : Colors.white,
                                style: GoogleFonts.robotoCondensed(fontSize: 13, color: isDark ? const Color(0xFFE4E8EE) : Colors.black),
                                labelStyle: GoogleFonts.robotoCondensed(fontSize: 12, color: isDark ? const Color(0xFF80CBC4) : const Color(0xFF616161)),
                              ),
                              const SizedBox(height: 4),
                              Row(
                                children: [
                                  Expanded(
                                      flex: 3,
                                      child: _buildReadOnlyRow('Batch', _batchNo)),
                                  const SizedBox(width: 4),
                                  Expanded(
                                      flex: 2,
                                      child: _buildReadOnlyRow('Run', _run)),
                                  const SizedBox(width: 4),
                                  Expanded(
                                    flex: 2,
                                    child: Container(
                                      padding: const EdgeInsets.symmetric(
                                          horizontal: 6, vertical: 5),
                                      decoration: BoxDecoration(
                                        color: _qsStatus.isEmpty
                                            ? cs.surfaceContainerHighest
                                            : _qsColor,
                                        borderRadius: BorderRadius.circular(4),
                                        border: Border.all(
                                          color: _qsStatus.isEmpty
                                              ? (isDark ? const Color(0xFF546E7A) : const Color(0xFF616161))
                                              : _qsColor,
                                          width: _qsStatus.isEmpty ? 1 : 2,
                                        ),
                                      ),
                                      child: Column(
                                        crossAxisAlignment:
                                            CrossAxisAlignment.center,
                                        children: [
                                          Text('PQS',
                                            style: _qsStatus.isEmpty
                                                ? GoogleFonts.robotoCondensed(
                                                    fontSize: 10,
                                                    color: const Color(0xFF616161))
                                                : GoogleFonts.robotoCondensed(
                                                    fontSize: 10,
                                                    color: Colors.white70)),
                                          Text(
                                            _qsStatus.isEmpty
                                                ? '—'
                                                : _qsStatus,
                                            style: _qsStatus.isEmpty
                                                ? GoogleFonts.robotoCondensed(
                                                    fontSize: 13,
                                                    color: const Color(0xFF424242))
                                                : GoogleFonts.robotoCondensed(
                                                    fontSize: 13,
                                                    fontWeight: FontWeight.bold,
                                                    color: Colors.white),
                                          ),
                                        ],
                                      ),
                                    ),
                                  ),
                                ],
                              ),
                            ],
                          ),
                        ),
                      ),
                      const SizedBox(height: 4),

                      // ================================================
                      // CARD 3: Quantity & Rack
                      // ================================================
                      Card(
                        child: Padding(
                          padding: const EdgeInsets.all(8),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              Text('\u2462 Rack / Qty', style: _cSectionTitle),
                              const SizedBox(height: 4),
                              Row(
                                children: [
                                  Expanded(
                                      child: _buildReadOnlyRow('DA Qty', _daQty)),
                                  const SizedBox(width: 6),
                                  Expanded(
                                      child: _buildReadOnlyRow(
                                          'Pallet Qty', _palletQty)),
                                ],
                              ),
                              const SizedBox(height: 4),
                              Row(
                                children: [
                                  Expanded(
                                    child: ScanField(
                                      controller: _rackController,
                                      focusNode: _rackFocus,
                                      label: 'Rack No',
                                      enabled: !_isLoading,
                                      onSubmitted: (v) => _processRack(v),
                                      onScanPressed:
                                          _isLoading ? null : _openScanner,
                                      filled: true,
                                      fillColor: isDark ? const Color(0xFF343B47) : Colors.white,
                                      style: GoogleFonts.robotoCondensed(fontSize: 13, color: isDark ? const Color(0xFFE4E8EE) : Colors.black),
                                      labelStyle: GoogleFonts.robotoCondensed(fontSize: 12, color: isDark ? const Color(0xFF80CBC4) : const Color(0xFF616161)),
                                    ),
                                  ),
                                  const SizedBox(width: 4),
                                  SizedBox(
                                    width: 56,
                                    child: ElevatedButton(
                                      onPressed:
                                          _isLoading ? null : _setRackTST3,
                                      style: ElevatedButton.styleFrom(
                                        padding: const EdgeInsets.symmetric(
                                            vertical: 14),
                                        backgroundColor: Colors.teal,
                                        foregroundColor: Colors.white,
                                      ),
                                      child: Text('TST3', style: _cSmall),
                                    ),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 4),
                              Row(
                                children: [
                                  Expanded(
                                    flex: 1,
                                    child: TextField(
                                      controller: _preparedQtyController,
                                      focusNode: _preparedQtyFocus,
                                      keyboardType: TextInputType.number,
                                      style: GoogleFonts.robotoCondensed(fontSize: 13, color: isDark ? const Color(0xFFE4E8EE) : Colors.black),
                                      inputFormatters: [
                                        FilteringTextInputFormatter.allow(
                                            RegExp(r'[0-9.]')),
                                      ],
                                      enabled: !_isLoading,
                                      textInputAction: TextInputAction.done,
                                      onSubmitted: _onPreparedQtySubmit,
                                      decoration: InputDecoration(
                                        labelText: 'Prep. Qty',
                                        labelStyle: GoogleFonts.robotoCondensed(fontSize: 12, color: isDark ? const Color(0xFF80CBC4) : const Color(0xFF616161)),
                                        border: const OutlineInputBorder(),
                                        filled: true,
                                        fillColor: isDark ? const Color(0xFF343B47) : Colors.white,
                                        contentPadding: const EdgeInsets.symmetric(
                                            horizontal: 8, vertical: 8),
                                      ),
                                    ),
                                  ),
                                  const SizedBox(width: 6),
                                  Expanded(
                                    flex: 1,
                                    child: _buildReadOnlyRow(
                                        'Total', _totalQty),
                                  ),
                                  const SizedBox(width: 6),
                                  Expanded(
                                    flex: 1,
                                    child: Container(
                                      padding: const EdgeInsets.symmetric(
                                          horizontal: 6, vertical: 6),
                                      decoration: BoxDecoration(
                                        color: _outstandingColor == Colors.green
                                            ? (isDark ? Colors.green.shade900 : Colors.green.shade50)
                                            : _outstandingColor == Colors.red
                                                ? (isDark ? Colors.red.shade900 : Colors.red.shade50)
                                                : cs.surfaceContainerHighest,
                                        borderRadius: BorderRadius.circular(4),
                                        border: Border.all(
                                          color:
                                              _outstandingColor == Colors.transparent
                                                  ? (isDark ? const Color(0xFF546E7A) : const Color(0xFF616161))
                                                  : _outstandingColor,
                                        ),
                                      ),
                                      child: Column(
                                        crossAxisAlignment:
                                            CrossAxisAlignment.start,
                                        children: [
                                          Text('Outstd', style: GoogleFonts.robotoCondensed(
                                              fontSize: 10, color: isDark ? const Color(0xFF80CBC4) : const Color(0xFF616161))),
                                          Text(
                                            _outstandingDA.isEmpty
                                                ? '—'
                                                : _outstandingDA,
                                            style: GoogleFonts.robotoCondensed(
                                              fontSize: 13,
                                              fontWeight: FontWeight.bold,
                                              color: _outstandingColor ==
                                                      Colors.transparent
                                                  ? const Color(0xFF424242)
                                                  : _outstandingColor,
                                            ),
                                          ),
                                        ],
                                      ),
                                    ),
                                  ),
                                ],
                              ),
                              if (_method == 'BOOKING') ...[
                                const SizedBox(height: 4),
                                _buildReadOnlyRow(
                                    'Booking Qty', _bookingPalletQty),
                              ],
                            ],
                          ),
                        ),
                      ),

                      // Loading indicator
                    if (_isLoading)
                      const Padding(
                        padding: EdgeInsets.only(bottom: 4),
                        child: LinearProgressIndicator(),
                      ),

                    // Bottom buttons
                    _buildButtonBar(),
                    const SizedBox(height: 8),
                ],
              ),
            ),
          ),
          ),
        ),
      ),
    );
  }

  /// Builds the action button bar at the bottom
  Widget _buildButtonBar() {
    final btnText = GoogleFonts.robotoCondensed(
        fontSize: 11, fontWeight: FontWeight.w600);
    final btnStyle = ElevatedButton.styleFrom(
      padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 6),
      textStyle: btnText,
    );
    return Column(
      children: [
        // Row 1: Secondary — Cancel | Clr Log | Location | C.Rack | Close
        Row(
          children: [
            Expanded(
              child: ElevatedButton(
                onPressed: _isLoading ? null : _onCancel,
                style: btnStyle.copyWith(
                  backgroundColor:
                      WidgetStatePropertyAll(Colors.orange),
                  foregroundColor:
                      const WidgetStatePropertyAll(Colors.white),
                  minimumSize:
                      const WidgetStatePropertyAll(Size(0, 30)),
                ),
                child: const Text('Cancel'),
              ),
            ),
            const SizedBox(width: 3),
            Expanded(
              child: ElevatedButton(
                onPressed: _isLoading ? null : _clearLog,
                style: btnStyle.copyWith(
                  backgroundColor:
                      WidgetStatePropertyAll(Colors.red.shade400),
                  foregroundColor:
                      const WidgetStatePropertyAll(Colors.white),
                  minimumSize:
                      const WidgetStatePropertyAll(Size(0, 30)),
                ),
                child: const Text('Clr Log'),
              ),
            ),
            const SizedBox(width: 3),
            Expanded(
              child: ElevatedButton(
                onPressed: _isLoading ? null : _openLocation,
                style: btnStyle.copyWith(
                  backgroundColor:
                      WidgetStatePropertyAll(Colors.blue),
                  foregroundColor:
                      const WidgetStatePropertyAll(Colors.white),
                  minimumSize:
                      const WidgetStatePropertyAll(Size(0, 30)),
                ),
                child: const Text('Loct'),
              ),
            ),
            const SizedBox(width: 3),
            Expanded(
              child: ElevatedButton(
                onPressed: _isLoading ? null : _openChangeRack,
                style: btnStyle.copyWith(
                  backgroundColor:
                      WidgetStatePropertyAll(Colors.purple.shade400),
                  foregroundColor:
                      const WidgetStatePropertyAll(Colors.white),
                  minimumSize:
                      const WidgetStatePropertyAll(Size(0, 30)),
                ),
                child: const Text('C.Rack'),
              ),
            ),
            const SizedBox(width: 3),
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
        const SizedBox(height: 4),
        // Row 2: Primary PREPARE — large, glove-friendly
        SizedBox(
          width: double.infinity,
          height: 52,
          child: MouseRegion(
            cursor: SystemMouseCursors.click,
            child: ElevatedButton.icon(
              onPressed: _isLoading ? null : _onPrepare,
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
              label: Text('PREPARE',
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
      ],
    );
  }

  /// Builds a read-only label/value row (condensed)
  Widget _buildReadOnlyRow(String label, String value) {
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
          Text(label, style: GoogleFonts.robotoCondensed(
              fontSize: 10, color: isDark ? const Color(0xFF80CBC4) : const Color(0xFF616161))),
          Text(
            value.isEmpty ? '—' : value,
            style: GoogleFonts.robotoCondensed(
                fontSize: 13, color: isDark ? const Color(0xFFE4E8EE) : const Color(0xFF424242)),
          ),
        ],
      ),
    );
  }
}
