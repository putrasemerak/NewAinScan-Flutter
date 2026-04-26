import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';

import '../../services/activity_log_service.dart';
import '../../services/auth_service.dart';
import '../../services/database_service.dart';
import '../../services/keyboard_service.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/scan_field.dart';
import '../../widgets/barcode_scanner_dialog.dart';
import '../../widgets/data_list_view.dart';
import '../../config/app_constants.dart';
import '../../utils/dialog_mixin.dart';

/// Stock Take screen — converts frmStockTake.vb
///
/// Handles stock take / inventory counting for TST1, TST2, FGWH, and TSTP
/// locations. Users scan pallets, verify quantities, and record actual counts.
///
/// Key behavior:
/// - TST1/TST2/TSTP: rack field is pre-filled and read-only with the location
/// - FGWH: rack field is editable (user scans rack barcode, validated against BD_0010)
/// - Pallet lookup cascades: TA_PLT001 -> TA_PLT001K (Kulim) -> TA_PLL001 (loose)
/// - On save: writes to TA_STK001 with PMonth/PYear from SY_0040
class StockTakeScreen extends StatefulWidget {
  /// The warehouse location code (TST1, TST2, FGWH, TSTP).
  final String location;

  /// Whether the rack field is read-only (true for TST1/TST2/TSTP).
  final bool rackReadOnly;

  const StockTakeScreen({
    super.key,
    required this.location,
    required this.rackReadOnly,
  });

  @override
  State<StockTakeScreen> createState() => _StockTakeScreenState();
}

class _StockTakeScreenState extends State<StockTakeScreen> with DialogMixin {
  final _db = DatabaseService();

  // Controllers
  final _rackController = TextEditingController();
  final _palletController = TextEditingController();
  final _actualQtyController = TextEditingController();

  // Focus nodes
  final _rackFocus = FocusNode();
  final _palletFocus = FocusNode();
  final _actualQtyFocus = FocusNode();

  // Key for scrolling actual qty into view when keyboard appears
  final _actualQtyKey = GlobalKey();

  // Pallet info fields
  String _category = ''; // NORMAL, LOOSE, KULIM
  String _batch = '';
  String _run = '';
  String _pCode = '';
  String _unit = '';
  String _palletQty = ''; // qty from pallet card
  String _onHand = ''; // current stock at location from IV_0250

  // Period info from SY_0040
  String _pMonth = '';
  String _pYear = '';

  bool _isLoading = false;
  bool _isDuplicatePallet = false; // tracks if current pallet already exists in TA_STK001

  // Data list for saved stock take items
  final List<Map<String, String>> _listRows = [];
  static const _listColumns = [
    DataColumnConfig(name: 'Pallet', flex: 2),
    DataColumnConfig(name: 'Batch', flex: 2),
    DataColumnConfig(name: 'Qty', flex: 1),
    DataColumnConfig(name: 'Rack', flex: 1),
  ];

  @override
  void initState() {
    super.initState();
    // Pre-fill rack for non-FGWH locations
    if (widget.rackReadOnly) {
      _rackController.text = widget.location;
    }
    _loadPeriod();
    _loadStockTakeList();
    // Listen for keyboard toggle so Actual Qty field responds instantly
    _actualQtyFocus.addListener(_onActualQtyFocusChange);
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        context.read<KeyboardService>().addListener(_onKeyboardToggleForActualQty);
      }
    });
  }

  @override
  void dispose() {
    _actualQtyFocus.removeListener(_onActualQtyFocusChange);
    try {
      context.read<KeyboardService>().removeListener(_onKeyboardToggleForActualQty);
    } catch (_) {}
    _rackController.dispose();
    _palletController.dispose();
    _actualQtyController.dispose();
    _rackFocus.dispose();
    _palletFocus.dispose();
    _actualQtyFocus.dispose();
    super.dispose();
  }

  /// When Actual Qty field gains focus, hide keyboard if in scanner mode.
  void _onActualQtyFocusChange() {
    if (_actualQtyFocus.hasFocus) {
      final kbService = context.read<KeyboardService>();
      if (!kbService.keyboardEnabled) {
        Future.delayed(const Duration(milliseconds: 50), () {
          SystemChannels.textInput.invokeMethod('TextInput.hide');
        });
      }
    }
  }

  /// When keyboard is toggled and Actual Qty has focus, show/hide instantly.
  void _onKeyboardToggleForActualQty() {
    if (!mounted || !_actualQtyFocus.hasFocus) return;
    final kbService = context.read<KeyboardService>();
    if (kbService.keyboardEnabled) {
      _actualQtyFocus.unfocus();
      Future.delayed(const Duration(milliseconds: 50), () {
        if (mounted) {
          _actualQtyFocus.requestFocus();
          // Scroll into view so keyboard doesn't block the field
          WidgetsBinding.instance.addPostFrameCallback((_) {
            final ctx = _actualQtyKey.currentContext;
            if (ctx != null) {
              Scrollable.ensureVisible(ctx,
                  duration: const Duration(milliseconds: 300),
                  curve: Curves.easeInOut);
            }
          });
        }
      });
    } else {
      SystemChannels.textInput.invokeMethod('TextInput.hide');
    }
    if (mounted) setState(() {});
  }

  /// Loads PMonth/PYear from SY_0040.
  /// VB.NET: SELECT MSEQ, MMIN FROM SY_0040 WHERE KEYCD1=110 AND KEYCD2='FGWH'
  /// Always uses KEYCD2='FGWH' regardless of location (same as VB.NET).
  Future<void> _loadPeriod() async {
    try {
      final rows = await _db.query(
        "SELECT MSEQ, MMIN FROM SY_0040 "
        "WHERE KEYCD1=@KeyCD1 AND KEYCD2=@KeyCD2",
        {'@KeyCD1': 110, '@KeyCD2': 'FGWH'},
      );
      if (rows.isNotEmpty) {
        _pMonth = (rows.first['MSEQ'] ?? '').toString().trim();
        _pYear = (rows.first['MMIN'] ?? '').toString().trim();
      }
    } catch (e) {
      debugPrint('Error loading period: $e');
    }
  }

  /// Loads last 3 saved stock take records as visual confirmation.
  Future<void> _loadStockTakeList() async {
    try {
      final rows = await _db.query(
        "SELECT TOP 3 Pallet, Batch, Qty, Rack FROM TA_STK001 "
        "WHERE PMonth=@PMonth AND PYear=@PYear "
        "ORDER BY AddDate DESC, AddTime DESC",
        {'@PMonth': _pMonth, '@PYear': _pYear},
      );
      if (!mounted) return;
      setState(() {
        _listRows.clear();
        for (final row in rows) {
          _listRows.add({
            'Pallet': (row['Pallet'] ?? '').toString().trim(),
            'Batch': (row['Batch'] ?? '').toString().trim(),
            'Qty': (row['Qty'] ?? '').toString().trim(),
            'Rack': (row['Rack'] ?? '').toString().trim(),
          });
        }
      });
    } catch (e) {
      debugPrint('Error loading stock take list: $e');
    }
  }

  // ---------------------------------------------------------------------------
  // Rack validation (FGWH only)
  // ---------------------------------------------------------------------------

  Future<void> _onRackSubmitted(String value) async {
    if (value.isEmpty) return;

    if (!mounted) return;
    setState(() => _isLoading = true);
    try {
      final rows = await _db.query(
        "SELECT Rack FROM BD_0010 WHERE Rack=@Rack",
        {'@Rack': value},
      );
      if (rows.isEmpty) {
        showErrorDialog('Nombor lokasi tidak sah');
        _rackController.clear();
        _rackFocus.requestFocus();
        return;
      }
      // Valid rack - move focus to actual qty with OnHand pre-filled
      _prefillAndFocusActualQty();
    } catch (e) {
      showErrorDialog('Error: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  // ---------------------------------------------------------------------------
  // Pallet scan & lookup
  // ---------------------------------------------------------------------------

  Future<void> _onPalletSubmitted(String value) async {
    if (value.isEmpty) return;

    // Validate period loaded from SY_0040
    final pMonthVal = double.tryParse(_pMonth) ?? 0;
    final pYearVal = double.tryParse(_pYear) ?? 0;
    if (pMonthVal == 0 || pYearVal == 0) {
      showErrorDialog('Info stocktake tidak lengkap (PMonth/PYear = 0)');
      return;
    }

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

    // For FGWH (rack editable): if rack is empty, move focus to rack field
    // so the user can scan it next. For other locations rack is pre-filled.
    if (_rackController.text.trim().isEmpty) {
      if (!widget.rackReadOnly) {
        // FGWH: proceed with pallet lookup, rack will be validated on save
      } else {
        showErrorDialog('Nombor lokasi tidak sah');
        return;
      }
    }

    if (!mounted) return;
    setState(() => _isLoading = true);
    try {
      // Check duplicate FIRST — before loading pallet info
      _isDuplicatePallet = false;
      final dupRows = await _db.query(
        "SELECT Pallet, Rack FROM TA_STK001 "
        "WHERE PMonth=@PMonth AND PYear=@PYear AND Pallet=@Pallet",
        {'@PMonth': _pMonth, '@PYear': _pYear, '@Pallet': palletNo},
      );
      if (dupRows.isNotEmpty) {
        final savedRack = (dupRows.first['Rack'] ?? '').toString().trim();
        if (!mounted) return;
        setState(() => _isLoading = false);
        final confirm = await showDialog<bool>(
          context: context,
          builder: (ctx) => AlertDialog(
            title: const Text('Pallet sudah diimbas'),
            content: Text('Pallet $palletNo sudah wujud.'
                '\nLokasi: $savedRack'
                '\n\nTeruskan tulis ganti?'),
            actions: [
              Row(
                children: [
                  Expanded(
                    flex: 1,
                    child: TextButton(
                      onPressed: () => Navigator.of(ctx).pop(false),
                      child: const Text('Batal'),
                    ),
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    flex: 3,
                    child: ElevatedButton(
                      onPressed: () => Navigator.of(ctx).pop(true),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.orange,
                        foregroundColor: Colors.white,
                      ),
                      child: const Text('Tulis Ganti'),
                    ),
                  ),
                ],
              ),
            ],
          ),
        );
        if (confirm != true) {
          _palletController.clear();
          _clearPalletFields();
          _palletFocus.requestFocus();
          return;
        }
        _isDuplicatePallet = true;
        if (!mounted) return;
        setState(() => _isLoading = true);
      }

      bool found = false;

      // 1. Try TA_PLT001 (normal pallet)
      final normalRows = await _db.query(
        "SELECT Batch, PCode, FullQty, LsQty, Unit, Cycle, Cate FROM TA_PLT001 "
        "WHERE PltNo=@PltNo",
        {'@PltNo': palletNo},
      );
      if (normalRows.isNotEmpty) {
        final row = normalRows.first;
        final fullQty =
            double.tryParse((row['FullQty'] ?? 0).toString()) ?? 0;
        final lsQty =
            double.tryParse((row['LsQty'] ?? 0).toString()) ?? 0;
        final cate = (row['Cate'] ?? '').toString().trim();
        if (!mounted) return;
        setState(() {
          _category = cate.isNotEmpty ? cate : 'NORMAL';
          _batch = (row['Batch'] ?? '').toString().trim();
          _run = (row['Cycle'] ?? '').toString().trim();
          _pCode = (row['PCode'] ?? '').toString().trim();
          _unit = (row['Unit'] ?? '').toString().trim();
          _palletQty = (fullQty + lsQty).toStringAsFixed(0);
        });
        found = true;
      }

      // 2. Try TA_PLT003 (re-print pallet card, e.g. pallets ending with W)
      if (!found) {
        final reprintRows = await _db.query(
          "SELECT Batch, PCode, Cycle, Actual FROM TA_PLT003 "
          "WHERE PltNo=@PltNo",
          {'@PltNo': palletNo},
        );
        if (reprintRows.isNotEmpty) {
          final row = reprintRows.first;
          final actual =
              double.tryParse((row['Actual'] ?? 0).toString()) ?? 0;
          if (!mounted) return;
          setState(() {
            _category = 'NORMAL';
            _batch = (row['Batch'] ?? '').toString().trim();
            _run = (row['Cycle'] ?? '').toString().trim();
            _pCode = (row['PCode'] ?? '').toString().trim();
            _unit = '';
            _palletQty = actual.toStringAsFixed(0);
          });
          found = true;
        }
      }

      // 3. Try TA_PLT001K (Kulim 9-char pallets)
      if (!found && AppConstants.isKulimPallet(palletNo)) {
        final kulimRows = await _db.query(
          "SELECT Batch, PCode, FullQty, LsQty, Unit, Cycle FROM TA_PLT001K "
          "WHERE PltNo=@PltNo",
          {'@PltNo': palletNo},
        );
        if (kulimRows.isNotEmpty) {
          final row = kulimRows.first;
          final fullQty =
              double.tryParse((row['FullQty'] ?? 0).toString()) ?? 0;
          final lsQty =
              double.tryParse((row['LsQty'] ?? 0).toString()) ?? 0;
          if (!mounted) return;
          setState(() {
            _category = 'KULIM';
            _batch = (row['Batch'] ?? '').toString().trim();
            _run = (row['Cycle'] ?? '').toString().trim();
            _pCode = (row['PCode'] ?? '').toString().trim();
            _unit = (row['Unit'] ?? '').toString().trim();
            _palletQty = (fullQty + lsQty).toStringAsFixed(0);
          });
          found = true;
        }
      }

      // 4. Try TA_PLL001 (loose pallet)
      // NOTE: TA_PLL001 has no Unit column; Unit comes from TA_PLT001 master.
      if (!found) {
        final looseRows = await _db.query(
          "SELECT Batch, PCode, Qty, Run FROM TA_PLL001 "
          "WHERE PltNo=@PltNo",
          {'@PltNo': palletNo},
        );
        if (looseRows.isNotEmpty) {
          // Sum qty across runs
          double totalQty = 0;
          for (final row in looseRows) {
            totalQty +=
                double.tryParse((row['Qty'] ?? 0).toString()) ?? 0;
          }
          final firstRow = looseRows.first;
          // Read Unit from TA_PLT001 (pallet master)
          String looseUnit = '';
          final masterRows = await _db.query(
            "SELECT Unit FROM TA_PLT001 WHERE PltNo=@PltNo",
            {'@PltNo': palletNo},
          );
          if (masterRows.isNotEmpty) {
            looseUnit = (masterRows.first['Unit'] ?? '').toString().trim();
          }
          if (!mounted) return;
          setState(() {
            _category = 'LOOSE';
            _batch = (firstRow['Batch'] ?? '').toString().trim();
            _run = (firstRow['Run'] ?? '').toString().trim();
            _pCode = (firstRow['PCode'] ?? '').toString().trim();
            _unit = looseUnit;
            _palletQty = totalQty.toStringAsFixed(0);
          });
          found = true;
        }
      }

      if (!found) {
        showErrorDialog('Nombor pallet tidak sah');
        _clearPalletFields();
        _palletController.clear();
        _palletFocus.requestFocus();
        return;
      }

      // Fetch OnHand from IV_0250 at this location
      await _loadOnHand(palletNo);

      // For FGWH: if rack is still empty, move focus to rack field next.
      // Otherwise move to actual qty with pallet qty pre-filled.
      if (!widget.rackReadOnly && _rackController.text.trim().isEmpty) {
        _rackFocus.requestFocus();
      } else {
        _prefillAndFocusActualQty();
      }
    } catch (e) {
      showErrorDialog('Error: $e');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  /// Pre-fill actual qty with pallet qty, focus the field and select all text
  /// so the user can type over it if the quantity differs.
  void _prefillAndFocusActualQty() {
    _actualQtyController.text = _palletQty;
    _actualQtyFocus.requestFocus();
    // Select all text and scroll the field into view (above virtual keyboard)
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _actualQtyController.selection = TextSelection(
        baseOffset: 0,
        extentOffset: _actualQtyController.text.length,
      );
      final ctx = _actualQtyKey.currentContext;
      if (ctx != null) {
        Scrollable.ensureVisible(ctx,
            duration: const Duration(milliseconds: 300),
            curve: Curves.easeInOut);
      }
    });
  }

  /// Reads the current OnHand quantity from IV_0250 for the scanned pallet.
  /// Matches VB.NET: SELECT SUM(OnHand) ... WHERE Pallet=@Pallet (no Location filter).
  /// If not found in IV_0250, it means the pallet is not inbound yet.
  Future<void> _loadOnHand(String pallet) async {
    try {
      final rows = await _db.query(
        "SELECT SUM(OnHand) as OnHand FROM IV_0250 "
        "WHERE Pallet=@Pallet GROUP BY PCode, Batch, Run",
        {'@Pallet': pallet},
      );
      if (!mounted) return;
      setState(() {
        if (rows.isNotEmpty) {
          final oh =
              double.tryParse((rows.first['OnHand'] ?? 0).toString()) ?? 0;
          _onHand = oh.toStringAsFixed(0);
        } else {
          _onHand = '0';
        }
      });
    } catch (e) {
      debugPrint('Error loading OnHand: $e');
      if (!mounted) return;
      setState(() => _onHand = '0');
    }
  }

  // ---------------------------------------------------------------------------
  // Save
  // ---------------------------------------------------------------------------

  Future<void> _save() async {
    final pallet = _palletController.text.trim();
    final rack = _rackController.text.trim();
    final actualQtyText = _actualQtyController.text.trim();

    // Validation matching VB.NET btnOk_Click
    final pMonthVal = double.tryParse(_pMonth) ?? 0;
    final pYearVal = double.tryParse(_pYear) ?? 0;
    if (pMonthVal == 0 || pYearVal == 0) {
      showErrorDialog('Info stocktake tidak lengkap');
      return;
    }
    if (pallet.isEmpty) {
      showErrorDialog('Tiada Nombor Pallet!');
      _palletFocus.requestFocus();
      return;
    }
    if (_batch.isEmpty) {
      showErrorDialog('Tiada Nombor Batch!');
      _palletFocus.requestFocus();
      return;
    }
    if (_pCode.isEmpty) {
      showErrorDialog('Tiada Product Code!');
      _palletFocus.requestFocus();
      return;
    }
    if (_run.isEmpty) {
      showErrorDialog('Tiada Nombor Run!');
      _palletFocus.requestFocus();
      return;
    }
    if (actualQtyText.isEmpty || (double.tryParse(actualQtyText) ?? 0) == 0) {
      showErrorDialog('Tiada Kuantiti!');
      _actualQtyFocus.requestFocus();
      return;
    }
    if (actualQtyText.length > 5) {
      showErrorDialog('Semak Pallet Kuantiti');
      _actualQtyController.clear();
      _actualQtyFocus.requestFocus();
      return;
    }
    if (rack.isEmpty) {
      showErrorDialog('Tiada Nombor Lokasi!');
      if (!widget.rackReadOnly) _rackFocus.requestFocus();
      return;
    }

    final actualQty = double.tryParse(actualQtyText) ?? 0;

    if (!mounted) return;
    setState(() => _isLoading = true);
    try {
      final authService = Provider.of<AuthService>(context, listen: false);
      final now = DateTime.now();
      final addDate = DateFormat('yyyy-MM-dd').format(now);
      final addTime = DateFormat('HH:mm:ss').format(now);

      if (_category != 'LOOSE') {
        // ---- NORMAL pallet (matches VB.NET btnOk_Click NORMAL path) ----
        // Check if record exists: PMonth + PYear + Pallet + Batch + Run + PCode
        final countRows = await _db.query(
          "SELECT COUNT(Pallet) AS Cnt FROM TA_STK001 "
          "WHERE PMonth=@PMonth AND PYear=@PYear AND Pallet=@Pallet "
          "AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
          {
            '@PMonth': _pMonth,
            '@PYear': _pYear,
            '@Pallet': pallet,
            '@Batch': _batch,
            '@Run': _run,
            '@PCode': _pCode,
          },
        );
        final cnt = int.tryParse(
                (countRows.first['Cnt'] ?? 0).toString()) ??
            0;

        if (cnt == 0) {
          // INSERT new record
          await _db.execute(
            "INSERT INTO TA_STK001 "
            "(PMonth,PYear,Pallet,PCode,Batch,Run,Rack,Qty,AddUser,AddDate,AddTime) "
            "VALUES (@PMonth,@PYear,@Pallet,@PCode,@Batch,@Run,@Rack,@Qty,@AddUser,@AddDate,@AddTime)",
            {
              '@PMonth': _pMonth,
              '@PYear': _pYear,
              '@Pallet': pallet,
              '@PCode': _pCode,
              '@Batch': _batch,
              '@Run': _run,
              '@Rack': rack,
              '@Qty': actualQty,
              '@AddUser': authService.empNo ?? '',
              '@AddDate': addDate,
              '@AddTime': addTime,
            },
          );
        } else {
          // UPDATE existing record (VB.NET uses EditUser/EditDate/EditTime)
          await _db.execute(
            "UPDATE TA_STK001 SET Rack=@Rack, Qty=@Qty, "
            "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
            "WHERE PMonth=@PMonth AND PYear=@PYear AND Pallet=@Pallet "
            "AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
            {
              '@PMonth': _pMonth,
              '@PYear': _pYear,
              '@Pallet': pallet,
              '@Batch': _batch,
              '@Run': _run,
              '@PCode': _pCode,
              '@Rack': rack,
              '@Qty': actualQty,
              '@EditUser': authService.empNo ?? '',
              '@EditDate': addDate,
              '@EditTime': addTime,
            },
          );
        }
      } else {
        // ---- LOOSE pallet (matches VB.NET btnOk_Click LOOSE path) ----
        // If user confirmed overwrite, delete old records first to avoid
        // accumulation on re-scan (VB.NET blocks duplicate at scan time,
        // but Flutter allows overwrite)
        if (_isDuplicatePallet) {
          await _db.execute(
            "DELETE FROM TA_STK001 "
            "WHERE PMonth=@PMonth AND PYear=@PYear AND Pallet=@Pallet",
            {
              '@PMonth': _pMonth,
              '@PYear': _pYear,
              '@Pallet': pallet,
            },
          );
        }

        final onHandVal = double.tryParse(_onHand) ?? 0;

        if (onHandVal == 0) {
          // VB.NET: "Tiada dalam stok semasa - Guna Maklumat TA_PLL001"
          final looseRows = await _db.query(
            "SELECT PltNo, Batch, Run, Qty FROM TA_PLL001 WHERE PltNo=@PltNo",
            {'@PltNo': pallet},
          );
          for (final row in looseRows) {
            final runVal = (row['Run'] ?? '').toString().trim();
            final looseQty =
                double.tryParse((row['Qty'] ?? 0).toString()) ?? 0;
            final looseBatch = (row['Batch'] ?? '').toString().trim();

            // VB.NET: check per-run existence
            final existRows = await _db.query(
              "SELECT Qty FROM TA_STK001 "
              "WHERE PMonth=@PMonth AND PYear=@PYear AND Pallet=@Pallet "
              "AND Batch=@Batch AND Run=@Run",
              {
                '@PMonth': _pMonth,
                '@PYear': _pYear,
                '@Pallet': pallet,
                '@Batch': _batch,
                '@Run': runVal,
              },
            );

            if (existRows.isEmpty) {
              // INSERT per-run record
              await _db.execute(
                "INSERT INTO TA_STK001 "
                "(PMonth,PYear,Pallet,Batch,Run,PCode,Rack,Qty,AddUser,AddDate,AddTime) "
                "VALUES (@PMonth,@PYear,@Pallet,@Batch,@Run,@PCode,@Rack,@Qty,@AddUser,@AddDate,@AddTime)",
                {
                  '@PMonth': _pMonth,
                  '@PYear': _pYear,
                  '@Pallet': pallet,
                  '@Batch': looseBatch,
                  '@Run': runVal,
                  '@PCode': _pCode,
                  '@Rack': rack,
                  '@Qty': looseQty,
                  '@AddUser': authService.empNo ?? '',
                  '@AddDate': addDate,
                  '@AddTime': addTime,
                },
              );
            } else {
              // UPDATE — accumulate qty (VB.NET: existing + TA_PLL001.Qty)
              final existQty = double.tryParse(
                      (existRows.first['Qty'] ?? 0).toString()) ??
                  0;
              await _db.execute(
                "UPDATE TA_STK001 SET PCode=@PCode, Rack=@Rack, Qty=@Qty, "
                "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
                "WHERE PMonth=@PMonth AND PYear=@PYear AND Pallet=@Pallet "
                "AND Batch=@Batch AND Run=@Run",
                {
                  '@PMonth': _pMonth,
                  '@PYear': _pYear,
                  '@Pallet': pallet,
                  '@Batch': looseBatch,
                  '@Run': runVal,
                  '@PCode': _pCode,
                  '@Rack': rack,
                  '@Qty': existQty + looseQty,
                  '@EditUser': authService.empNo ?? '',
                  '@EditDate': addDate,
                  '@EditTime': addTime,
                },
              );
            }
          }
        } else {
          // VB.NET: "Ada dalam Stok - Guna maklumat dalam stok"
          final ivRows = await _db.query(
            "SELECT Pallet, Batch, Run, OnHand FROM IV_0250 "
            "WHERE Pallet=@Pallet AND OnHand > 0",
            {'@Pallet': pallet},
          );
          for (final row in ivRows) {
            final runVal = (row['Run'] ?? '').toString().trim();
            final onHand =
                double.tryParse((row['OnHand'] ?? 0).toString()) ?? 0;
            final ivBatch = (row['Batch'] ?? '').toString().trim();

            final existRows = await _db.query(
              "SELECT Qty FROM TA_STK001 "
              "WHERE PMonth=@PMonth AND PYear=@PYear AND Pallet=@Pallet "
              "AND Batch=@Batch AND Run=@Run",
              {
                '@PMonth': _pMonth,
                '@PYear': _pYear,
                '@Pallet': pallet,
                '@Batch': _batch,
                '@Run': runVal,
              },
            );

            if (existRows.isEmpty) {
              await _db.execute(
                "INSERT INTO TA_STK001 "
                "(PMonth,PYear,Pallet,Batch,Run,PCode,Rack,Qty,AddUser,AddDate,AddTime) "
                "VALUES (@PMonth,@PYear,@Pallet,@Batch,@Run,@PCode,@Rack,@Qty,@AddUser,@AddDate,@AddTime)",
                {
                  '@PMonth': _pMonth,
                  '@PYear': _pYear,
                  '@Pallet': pallet,
                  '@Batch': ivBatch,
                  '@Run': runVal,
                  '@PCode': _pCode,
                  '@Rack': rack,
                  '@Qty': onHand,
                  '@AddUser': authService.empNo ?? '',
                  '@AddDate': addDate,
                  '@AddTime': addTime,
                },
              );
            } else {
              // UPDATE — accumulate qty (VB.NET: existing + IV_0250.OnHand)
              final existQty = double.tryParse(
                      (existRows.first['Qty'] ?? 0).toString()) ??
                  0;
              await _db.execute(
                "UPDATE TA_STK001 SET PCode=@PCode, Rack=@Rack, Qty=@Qty, "
                "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
                "WHERE PMonth=@PMonth AND PYear=@PYear AND Pallet=@Pallet "
                "AND Batch=@Batch AND Run=@Run",
                {
                  '@PMonth': _pMonth,
                  '@PYear': _pYear,
                  '@Pallet': pallet,
                  '@Batch': _batch,
                  '@Run': runVal,
                  '@PCode': _pCode,
                  '@Rack': rack,
                  '@Qty': existQty + onHand,
                  '@EditUser': authService.empNo ?? '',
                  '@EditDate': addDate,
                  '@EditTime': addTime,
                },
              );
            }
          }
        }
      }

      // Capture values for log before clearing
      final savedPallet = pallet;
      final savedRack = rack;
      final savedQty = actualQtyText;

      // Refresh list and clear form
      await _loadStockTakeList();
      _clearForm();

      await ActivityLogService.log(
        action: 'STOCK_TAKE_SAVE',
        empNo: authService.empNo ?? '',
        detail: 'Rack: $savedRack, Pallet: $savedPallet, Actual: $savedQty',
      );

      if (!mounted) return;
      ScaffoldMessenger.of(context)
        ..clearSnackBars()
        ..showSnackBar(
          SnackBar(
            content: Row(
              children: [
                const Icon(Icons.check_circle, color: Colors.white, size: 20),
                const SizedBox(width: 8),
                Text('Stock take saved  [$savedPallet]'),
              ],
            ),
            backgroundColor: Colors.green.shade700,
            duration: const Duration(seconds: 1),
            behavior: SnackBarBehavior.floating,
            margin: const EdgeInsets.only(bottom: 60, left: 16, right: 16),
          ),
        );

      _palletFocus.requestFocus();
    } catch (e) {
      await ActivityLogService.logError(
        action: 'STOCK_TAKE_SAVE',
        detail: 'Rack: ${_rackController.text.trim()}, Pallet: ${_palletController.text.trim()}',
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
      _category = '';
      _batch = '';
      _run = '';
      _pCode = '';
      _unit = '';
      _palletQty = '';
      _onHand = '';
    });
  }

  void _clearForm() {
    _palletController.clear();
    _actualQtyController.clear();
    _clearPalletFields();
    _isDuplicatePallet = false;
    // Keep rack for non-FGWH; clear for FGWH
    if (!widget.rackReadOnly) {
      _rackController.clear();
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

  // Scan input field colors (lighter shade — users type/scan here)
  Color get _scanFieldFill => _isDark
      ? const Color(0xFF2A3140)   // dark: lighter blue-grey
      : const Color(0xFFF5F8FF);  // light: very light blue-white

  // Auto-fill read-only field colors (darker shade — system populated)
  Color get _autoFieldFill => _isDark
      ? const Color(0xFF1E252E)   // dark: darker blue-grey
      : const Color(0xFFE8EAF0);  // light: grey-blue tint

  // Period highlight field colors (ST Month / ST Year — distinct amber tint)
  Color get _periodFieldFill => _isDark
      ? const Color(0xFF2E2A1E)   // dark: warm amber-tinted dark
      : const Color(0xFFFFF8E1);  // light: light amber
  Color get _periodBorder => _isDark
      ? const Color(0xFF8D6E34)   // dark: amber-brown border
      : const Color(0xFFF9A825);  // light: amber border
  TextStyle get _periodLabelStyle => GoogleFonts.robotoCondensed(
      fontSize: 10, color: _isDark ? const Color(0xFFFFD54F) : const Color(0xFFF57F17));
  TextStyle get _periodValueStyle => GoogleFonts.robotoCondensed(
      fontSize: 13, fontWeight: FontWeight.w600,
      color: _isDark ? const Color(0xFFFFE082) : const Color(0xFFE65100));

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
      title: 'Stock Take - ${widget.location}',
      body: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
        child: SingleChildScrollView(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(
              'User: ${auth.empNo ?? ''}@${auth.empName ?? ''}',
              style: GoogleFonts.robotoCondensed(
                  fontSize: 11,
                  color: Theme.of(context).brightness == Brightness.dark
                      ? const Color(0xFF80CBC4) : Colors.blueGrey),
            ),
            const SizedBox(height: 2),

            // Stock Take Period (read-only, from SY_0040)
            Row(
              children: [
                Expanded(child: _buildPeriodField('ST Month', _pMonth)),
                const SizedBox(width: 4),
                Expanded(child: _buildPeriodField('ST Year', _pYear)),
              ],
            ),
            const SizedBox(height: 4),
              // Single card: Scan + Info + Buttons + Confirmation List
              Card(
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 6),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Text('① Scan', style: _cSectionTitle),
                    const SizedBox(height: 3),
                    Row(
                      children: [
                        Expanded(
                          child: ScanField(
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
                            fillColor: _scanFieldFill,
                            style: _cInputStyle,
                            labelStyle: _cInputLabelStyle,
                            contentPadding: const EdgeInsets.symmetric(
                                horizontal: 8, vertical: 8),
                            isDense: true,
                          ),
                        ),
                        const SizedBox(width: 6),
                        SizedBox(
                          width: 120,
                          child: ScanField(
                            controller: _rackController,
                            focusNode: _rackFocus,
                            label: 'Rack No',
                            readOnly: widget.rackReadOnly,
                            enabled: !widget.rackReadOnly,
                            onSubmitted: widget.rackReadOnly
                                ? null
                                : _onRackSubmitted,
                            onScanPressed: widget.rackReadOnly
                                ? null
                                : () => _scanBarcode(
                                      _rackController,
                                      title: 'Scan Rack',
                                      onResult: _onRackSubmitted,
                                    ),
                            filled: true,
                            fillColor: widget.rackReadOnly
                                ? _autoFieldFill
                                : _scanFieldFill,
                            style: widget.rackReadOnly
                                ? _cValueStyle
                                : _cInputStyle,
                            labelStyle: widget.rackReadOnly
                                ? _cLabelStyle
                                : _cInputLabelStyle,
                            contentPadding: const EdgeInsets.symmetric(
                                horizontal: 8, vertical: 8),
                            isDense: true,
                          ),
                        ),
                      ],
                    ),

                      const SizedBox(height: 6),

                      // ── Info Fields ──
                      Text('② Info', style: _cSectionTitle),
                      const SizedBox(height: 3),
                    // Row 1: Category / Batch / Run
                    Row(
                      children: [
                        Expanded(flex: 2, child: _buildReadOnly('Cat', _category)),
                        const SizedBox(width: 4),
                        Expanded(flex: 3, child: _buildReadOnly('Batch', _batch)),
                        const SizedBox(width: 4),
                        Expanded(flex: 1, child: _buildReadOnly('Run', _run)),
                      ],
                    ),
                    const SizedBox(height: 3),
                    // Row 2: PCode / Unit
                    Row(
                      children: [
                        Expanded(flex: 4, child: _buildReadOnly('PCode', _pCode)),
                        const SizedBox(width: 4),
                        Expanded(flex: 1, child: _buildReadOnly('Unit', _unit)),
                      ],
                    ),
                    const SizedBox(height: 3),
                    // Row 3: Pallet Qty / OnHand / Actual Qty
                    Row(
                      children: [
                        Expanded(child: _buildReadOnly('PltQty', _palletQty)),
                        const SizedBox(width: 4),
                        Expanded(child: _buildReadOnly('OnHand', _onHand)),
                        const SizedBox(width: 4),
                        Expanded(
                          child: TextField(
                            key: _actualQtyKey,
                            controller: _actualQtyController,
                            focusNode: _actualQtyFocus,
                            keyboardType: context.watch<KeyboardService>().keyboardEnabled
                                ? TextInputType.number
                                : TextInputType.none,
                            textInputAction: TextInputAction.done,
                            onSubmitted: (_) => _save(),
                            style: _cInputStyle,
                            decoration: InputDecoration(
                              labelText: 'Actual',
                              labelStyle: _cInputLabelStyle,
                              border: const OutlineInputBorder(),
                              filled: true,
                              fillColor: Theme.of(context).brightness == Brightness.dark
                                  ? const Color(0xFF343B47)
                                  : Colors.white,
                              contentPadding: const EdgeInsets.symmetric(
                                  horizontal: 8, vertical: 8),
                            ),
                          ),
                        ),
                      ],
                    ),

                      const SizedBox(height: 6),

                      // ── Loading indicator ──
                      if (_isLoading)
                        const Padding(
                          padding: EdgeInsets.only(bottom: 4),
                          child: LinearProgressIndicator(),
              ),

            // ─── Secondary buttons: Save | Clear | List | Close ─
            // Big SAVE button — glove-friendly, one-hand operation
                      // Big SAVE button — glove-friendly
                      SizedBox(
                        width: double.infinity,
                        height: 46,
                        child: ElevatedButton.icon(
                          onPressed: _isLoading ? null : _save,
                  icon: _isLoading
                      ? const SizedBox(
                          width: 18,
                          height: 18,
                          child: CircularProgressIndicator(
                            strokeWidth: 2,
                            color: Colors.white,
                          ),
                        )
                      : const Icon(Icons.save, size: 20),
                          label: Text('SAVE',
                              style: GoogleFonts.robotoCondensed(
                                  fontSize: 15,
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
                      const SizedBox(height: 4),

                      // Clear | List | Close
                      Row(
                        children: [
                          Expanded(
                            child: SizedBox(
                              height: 36,
                              child: ElevatedButton(
                                onPressed: _isLoading ? null : _clearForm,
                                style: btnStyle.copyWith(
                                  backgroundColor:
                                      WidgetStatePropertyAll(Colors.orange),
                                  foregroundColor:
                                      const WidgetStatePropertyAll(Colors.white),
                                  minimumSize:
                                      const WidgetStatePropertyAll(Size(0, 36)),
                                ),
                                child: const Text('Clear'),
                              ),
                            ),
                          ),
                          const SizedBox(width: 4),
                          Expanded(
                            child: SizedBox(
                              height: 36,
                              child: ElevatedButton(
                                onPressed: _isLoading
                                    ? null
                                    : () {
                                        Navigator.of(context).push(
                                          MaterialPageRoute(
                                            builder: (_) =>
                                                _StockTakeListPlaceholderScreen(
                                              location: widget.location,
                                              pMonth: _pMonth,
                                              pYear: _pYear,
                                            ),
                                          ),
                                        );
                                      },
                                style: btnStyle.copyWith(
                                  backgroundColor:
                                      WidgetStatePropertyAll(Colors.blue.shade700),
                                  foregroundColor:
                                      const WidgetStatePropertyAll(Colors.white),
                                  minimumSize:
                                      const WidgetStatePropertyAll(Size(0, 36)),
                                ),
                                child: const Text('List'),
                              ),
                            ),
                          ),
                          const SizedBox(width: 4),
                          Expanded(
                            child: SizedBox(
                              height: 36,
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
                                      const WidgetStatePropertyAll(Size(0, 36)),
                                ),
                                child: const Text('Close'),
                              ),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 6),

                      // Last 3 saved records (confirmation)
                      SizedBox(
                        height: 130,
                        child: ClipRRect(
                          borderRadius: BorderRadius.circular(4),
                          child: DataListView(
                            columns: _listColumns,
                            rows: _listRows,
                            headerTextStyle: GoogleFonts.robotoCondensed(fontSize: 11),
                            headerPadding: const EdgeInsets.symmetric(
                                horizontal: 8, vertical: 4),
                          ),
                        ),
                      ),
                    ],
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
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 5),
      decoration: BoxDecoration(
        color: _autoFieldFill,
        borderRadius: BorderRadius.circular(4),
        border: Border.all(
          color: _isDark ? const Color(0xFF546E7A) : const Color(0xFF616161),
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

  Widget _buildPeriodField(String label, String value) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 5),
      decoration: BoxDecoration(
        color: _periodFieldFill,
        borderRadius: BorderRadius.circular(4),
        border: Border.all(color: _periodBorder),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: _periodLabelStyle),
          Text(
            value.isEmpty ? '-' : value,
            style: _periodValueStyle,
          ),
        ],
      ),
    );
  }
}

// -----------------------------------------------------------------------------
// Inline placeholder for StockTakeListScreen navigation target.
// This will be replaced when the full stock_take_list_screen.dart is created
// under the utility folder.
// -----------------------------------------------------------------------------

class _StockTakeListPlaceholderScreen extends StatelessWidget {
  final String location;
  final String pMonth;
  final String pYear;

  const _StockTakeListPlaceholderScreen({
    required this.location,
    required this.pMonth,
    required this.pYear,
  });

  @override
  Widget build(BuildContext context) {
    final db = DatabaseService();

    return AppScaffold(
      title: 'Stock Take List - $location',
      body: FutureBuilder<List<Map<String, dynamic>>>(
        future: db.query(
          "SELECT Pallet, Batch, Run, Rack, Qty, AddUser, AddDate, AddTime "
          "FROM TA_STK001 WHERE PMonth=@PMonth AND PYear=@PYear "
          "ORDER BY AddDate DESC, AddTime DESC",
          {'@PMonth': pMonth, '@PYear': pYear},
        ),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return Center(child: Text('Error: ${snapshot.error}',
                style: TextStyle(color: Colors.red.shade300)));
          }

          final rows = (snapshot.data ?? []).map((row) {
            return {
              'Pallet': (row['Pallet'] ?? '').toString().trim(),
              'Batch': (row['Batch'] ?? '').toString().trim(),
              'Rack': (row['Rack'] ?? '').toString().trim(),
              'Qty': (row['Qty'] ?? '').toString().trim(),
              'User': (row['AddUser'] ?? '').toString().trim(),
            };
          }).toList();

          return DataListView(
            columns: const [
              DataColumnConfig(name: 'Pallet', flex: 2),
              DataColumnConfig(name: 'Batch', flex: 2),
              DataColumnConfig(name: 'Rack', flex: 1),
              DataColumnConfig(name: 'Qty', flex: 1),
              DataColumnConfig(name: 'User', flex: 1),
            ],
            rows: rows,
          );
        },
      ),
    );
  }
}
