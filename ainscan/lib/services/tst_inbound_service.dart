import 'package:intl/intl.dart';

import '../models/pallet_info.dart';
import '../config/app_constants.dart';
import '../utils/converters.dart';
import 'activity_log_service.dart';
import 'database_service.dart';

/// Extracted business logic for the TST Stock In screen.
///
/// Encapsulates pallet lookup, rack validation, and the combined
/// receiving + rack-in inbound operations. This was previously ~1 000 lines
/// of inline code in _StockInTstScreenState.
class TstInboundService {
  TstInboundService({DatabaseService? db}) : _db = db ?? DatabaseService();

  final DatabaseService _db;

  // ---------------------------------------------------------------------------
  // Pallet lookup
  // ---------------------------------------------------------------------------

  /// Loads pallet info from the database. Returns a [PalletInfo] with
  /// category, batch, run, quantities, QC status, pallet date, and
  /// (for LOOSE) the list of loose entries.
  ///
  /// Throws if the barcode is invalid or the pallet is not found.
  Future<PalletInfo> loadPallet(String barcode) async {
    final bcType = AppConstants.getBarcodeType(barcode);
    if (bcType != BarcodeType.pallet) {
      throw Exception('Nombor pallet tidak sah');
    }

    // Determine NORMAL or LOOSE via TA_PLL001
    final looseCountRows = await _db.query(
      "SELECT COUNT(PltNo) AS Cnt FROM TA_PLL001 WHERE PltNo=@PltNo",
      {'@PltNo': barcode},
    );
    final isLoose = (int.tryParse(
            looseCountRows.firstOrNull?['Cnt']?.toString() ?? '0') ?? 0) > 0;

    // Read pallet date from TA_PLT002
    DateTime palletDate;
    final plt002Rows = await _db.query(
      "SELECT AddDate FROM TA_PLT002 WHERE Pallet=@Pallet",
      {'@Pallet': barcode},
    );
    if (plt002Rows.isNotEmpty) {
      final addDate = plt002Rows.first['AddDate']?.toString() ?? '';
      palletDate = addDate.isNotEmpty
          ? (DateTime.tryParse(addDate) ?? DateTime.now())
          : DateTime.now();
    } else {
      palletDate = DateTime.now();
    }

    return isLoose
        ? await _loadLoosePallet(barcode, palletDate)
        : await _loadNormalPallet(barcode, palletDate);
  }

  Future<PalletInfo> _loadNormalPallet(String barcode, DateTime palletDate) async {
    final rows = await _db.query(
      "SELECT * FROM TA_PLT001 WHERE PltNo=@PltNo",
      {'@PltNo': barcode},
    );
    if (rows.isEmpty) throw Exception('Nombor pallet tidak sah');

    final row = rows.first;
    final batch = (row['Batch'] ?? '').toString().trim();
    final pCode = (row['PCode'] ?? '').toString().trim();
    final cycle = (row['Cycle'] ?? '').toString().trim();

    String qcStatus = '';
    final qcRows = await _db.query(
      "SELECT QS FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
      {'@Batch': batch, '@Run': cycle, '@PCode': pCode},
    );
    if (qcRows.isNotEmpty) {
      qcStatus = (qcRows.first['QS'] ?? '').toString().trim();
    }

    return PalletInfo.fromMap(row, palletDate: palletDate).copyWith(qs: qcStatus);
  }

  Future<PalletInfo> _loadLoosePallet(String barcode, DateTime palletDate) async {
    final pllRows = await _db.query(
      "SELECT * FROM TA_PLL001 WHERE PltNo=@PltNo",
      {'@PltNo': barcode},
    );
    if (pllRows.isEmpty) throw Exception('Nombor pallet tidak sah');

    double looseQty = 0;
    final runs = <String>[];
    for (final entry in pllRows) {
      looseQty += toDouble(entry['Qty']);
      final run = (entry['Run'] ?? '').toString().trim();
      if (run.isNotEmpty && !runs.contains(run)) runs.add(run);
    }

    final pltRows = await _db.query(
      "SELECT * FROM TA_PLT001 WHERE PltNo=@PltNo",
      {'@PltNo': barcode},
    );
    if (pltRows.isEmpty) throw Exception('Nombor pallet tidak sah');
    final pltRow = pltRows.first;

    String qcStatus = '';
    if (runs.isNotEmpty) {
      final qcRows = await _db.query(
        "SELECT QS FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {
          '@Batch': (pltRow['Batch'] ?? '').toString().trim(),
          '@Run': runs.first,
          '@PCode': (pltRow['PCode'] ?? '').toString().trim(),
        },
      );
      if (qcRows.isNotEmpty) {
        qcStatus = (qcRows.first['QS'] ?? '').toString().trim();
      }
    }

    return PalletInfo(
      pltNo: barcode,
      batch: (pltRow['Batch'] ?? '').toString().trim(),
      run: runs.join(','),
      pCode: (pltRow['PCode'] ?? '').toString().trim(),
      pName: (pltRow['PName'] ?? '').toString().trim(),
      pGroup: (pltRow['PGroup'] ?? '').toString().trim(),
      unit: (pltRow['Unit'] ?? '').toString().trim(),
      fullQty: looseQty,
      lsQty: 0,
      qs: qcStatus,
      isLoose: true,
      palletDate: palletDate,
      looseEntries: List.from(pllRows),
    );
  }

  // ---------------------------------------------------------------------------
  // Rack validation
  // ---------------------------------------------------------------------------

  /// Validates that [barcode] is a valid rack location. Throws on failure.
  Future<void> validateRack(String barcode) async {
    final bcType = AppConstants.getBarcodeType(barcode);
    if (bcType != BarcodeType.rack) {
      throw Exception('Nombor lokasi tidak sah');
    }

    final rackRows = await _db.query(
      "SELECT Rack FROM BD_0010 WHERE Rack=@Rack",
      {'@Rack': barcode},
    );
    if (rackRows.isEmpty) {
      throw Exception('Nombor lokasi tidak sah');
    }
  }

  // ---------------------------------------------------------------------------
  // Inbound — combined receiving + rack-in
  // ---------------------------------------------------------------------------

  /// Executes the full inbound operation for a pallet into a rack location.
  ///
  /// Handles both NORMAL and LOOSE pallets, mirroring VB.NET btnInbound_Click.
  Future<void> executeInbound({
    required PalletInfo pallet,
    required String rackNo,
    required double actualQty,
    required String empNo,
  }) async {
    if (!pallet.isLoose) {
      await _inboundNormal(pallet, rackNo, actualQty, empNo);
    } else {
      await _inboundLoose(pallet, rackNo, empNo);
    }

    await ActivityLogService.log(
      action: 'TST_INBOUND',
      empNo: empNo,
      detail: 'Pallet: ${pallet.pltNo}, Rack: $rackNo, Type: ${pallet.category}',
    );
  }

  /// Returns stock location rows for a pallet (for display list).
  Future<List<Map<String, String>>> loadStockLocations(String palletNo) async {
    try {
      final rows = await _db.query(
        "SELECT Loct, OnHand, PCode, Batch, Run FROM IV_0250 WHERE Pallet=@Pallet",
        {'@Pallet': palletNo},
      );
      return rows.map((r) {
        return {
          'Loct': (r['Loct'] ?? '').toString().trim(),
          'OnHand': toDouble(r['OnHand']).toStringAsFixed(0),
          'PCode': (r['PCode'] ?? '').toString().trim(),
          'Batch': (r['Batch'] ?? '').toString().trim(),
          'Run': (r['Run'] ?? '').toString().trim(),
        };
      }).toList();
    } catch (_) {
      return [];
    }
  }

  // ---------------------------------------------------------------------------
  // NORMAL inbound
  // ---------------------------------------------------------------------------

  Future<void> _inboundNormal(
      PalletInfo pallet, String rackNo, double actualQty, String empNo) async {
    final now = DateTime.now();
    final dateStr = DateFormat('yyyy-MM-dd').format(now);
    final timeStr = DateFormat('HH:mm:ss').format(now);
    final tarikhStr = pallet.palletDate != null
        ? DateFormat('yyyy-MM-dd').format(pallet.palletDate!)
        : dateStr;
    final statusQC = pallet.qs ?? '';

    // Step 1: Receiving (TST1)
    final alreadyReceived = await _isAlreadyReceived(pallet.pltNo);

    await _upsertTst1(pallet, actualQty, statusQC, empNo, tarikhStr,
        dateStr, timeStr);
    if (!alreadyReceived) {
      await _markTransfer(pallet.pltNo, empNo, tarikhStr, timeStr);
    }

    // Step 2: Rack-in (TST2 + rack location)
    final alreadyStockIn = await _isAlreadyStockIn(pallet.pltNo);

    if (!alreadyStockIn) {
      // Verify PD_0800 exists
      await _verifyPd0800(pallet.batch, pallet.run, pallet.pCode);

      await _upsertTst2(pallet, actualQty, statusQC, empNo, tarikhStr,
          dateStr, timeStr, deductStock: true);

      await _registerRack(
        rackNo: rackNo, batch: pallet.batch, run: pallet.run,
        pCode: pallet.pCode, pGroup: pallet.pGroup, pName: pallet.pName,
        unit: pallet.unit, status: statusQC, qty: actualQty,
        pallet: pallet.pltNo, empNo: empNo, tarikhStr: tarikhStr,
        dateStr: dateStr, timeStr: timeStr, firstStockIn: true,
      );

      await _closePallet(pallet.pltNo, transferStatus: true);
      await _updateRackBalance(
          pallet.batch, pallet.run, pallet.pCode, actualQty);
      await _upsertLoc0600(
        palletNo: pallet.pltNo, rackNo: rackNo, batch: pallet.batch,
        run: pallet.run, pCode: pallet.pCode, pName: pallet.pName,
        pGroup: pallet.pGroup, qty: actualQty, unit: pallet.unit,
        empNo: empNo, tarikhStr: tarikhStr, timeStr: timeStr,
      );
    } else {
      await _upsertTst2(pallet, actualQty, statusQC, empNo, tarikhStr,
          dateStr, timeStr, deductStock: false);

      await _registerRack(
        rackNo: rackNo, batch: pallet.batch, run: pallet.run,
        pCode: pallet.pCode, pGroup: pallet.pGroup, pName: pallet.pName,
        unit: pallet.unit, status: statusQC, qty: actualQty,
        pallet: pallet.pltNo, empNo: empNo, tarikhStr: tarikhStr,
        dateStr: dateStr, timeStr: timeStr, firstStockIn: false,
      );
    }
  }

  // ---------------------------------------------------------------------------
  // LOOSE inbound
  // ---------------------------------------------------------------------------

  Future<void> _inboundLoose(
      PalletInfo pallet, String rackNo, String empNo) async {
    final now = DateTime.now();
    final dateStr = DateFormat('yyyy-MM-dd').format(now);
    final timeStr = DateFormat('HH:mm:ss').format(now);
    final tarikhStr = pallet.palletDate != null
        ? DateFormat('yyyy-MM-dd').format(pallet.palletDate!)
        : dateStr;

    final pllRows = await _db.query(
      "SELECT * FROM TA_PLL001 WHERE PltNo=@PltNo",
      {'@PltNo': pallet.pltNo},
    );
    if (pllRows.isEmpty) throw Exception('Tiada data LOOSE untuk pallet ini');

    final alreadyReceived = await _isAlreadyReceived(pallet.pltNo);
    final alreadyStockIn = await _isAlreadyStockIn(pallet.pltNo);

    for (final entry in pllRows) {
      final batch = (entry['Batch'] ?? '').toString().trim();
      final run = (entry['Run'] ?? '').toString().trim();
      final entryQty = toDouble(entry['Qty']);

      String statusQC = '';
      final qcRows = await _db.query(
        "SELECT QS FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {'@Batch': batch, '@Run': run, '@PCode': pallet.pCode},
      );
      if (qcRows.isNotEmpty) {
        statusQC = (qcRows.first['QS'] ?? '').toString().trim();
      }

      final entryPallet = PalletInfo(
        pltNo: pallet.pltNo, batch: batch, run: run,
        pCode: pallet.pCode, pName: pallet.pName, pGroup: pallet.pGroup,
        unit: pallet.unit, fullQty: entryQty, lsQty: 0, qs: statusQC,
        isLoose: true, palletDate: pallet.palletDate,
      );

      if (!alreadyReceived) {
        await _upsertTst1(entryPallet, entryQty, statusQC, empNo,
            tarikhStr, dateStr, timeStr);
      }

      if (!alreadyStockIn) {
        await _verifyPd0800(batch, run, pallet.pCode);
        await _upsertTst2(entryPallet, entryQty, statusQC, empNo,
            tarikhStr, dateStr, timeStr, deductStock: false);

        await _registerRack(
          rackNo: rackNo, batch: batch, run: run,
          pCode: pallet.pCode, pGroup: pallet.pGroup, pName: pallet.pName,
          unit: pallet.unit, status: statusQC, qty: entryQty,
          pallet: pallet.pltNo, empNo: empNo, tarikhStr: tarikhStr,
          dateStr: dateStr, timeStr: timeStr, firstStockIn: true,
        );

        await _updateRackBalance(batch, run, pallet.pCode, entryQty);
        await _upsertLoc0600(
          palletNo: pallet.pltNo, rackNo: rackNo, batch: batch,
          run: run, pCode: pallet.pCode, pName: pallet.pName,
          pGroup: pallet.pGroup, qty: entryQty, unit: pallet.unit,
          empNo: empNo, tarikhStr: tarikhStr, timeStr: timeStr,
        );
      } else {
        await _registerRack(
          rackNo: rackNo, batch: batch, run: run,
          pCode: pallet.pCode, pGroup: pallet.pGroup, pName: pallet.pName,
          unit: pallet.unit, status: statusQC, qty: entryQty,
          pallet: pallet.pltNo, empNo: empNo, tarikhStr: tarikhStr,
          dateStr: dateStr, timeStr: timeStr, firstStockIn: false,
        );
      }
    }

    await _closePallet(pallet.pltNo, transferStatus: false);
  }

  // ---------------------------------------------------------------------------
  // Shared helpers
  // ---------------------------------------------------------------------------

  Future<bool> _isAlreadyReceived(String palletNo) async {
    final rows = await _db.query(
      "SELECT COUNT(PltNo) AS Cnt FROM TA_PLT001 WHERE TStatus=@TStatus AND PltNo=@PltNo",
      {'@TStatus': 'Transfer', '@PltNo': palletNo},
    );
    return (int.tryParse(rows.firstOrNull?['Cnt']?.toString() ?? '0') ?? 0) > 0;
  }

  Future<bool> _isAlreadyStockIn(String palletNo) async {
    final rows = await _db.query(
      "SELECT COUNT(Pallet) AS Cnt FROM TA_PLT002 WHERE Status=@Status AND Pallet=@Pallet",
      {'@Status': 'C', '@Pallet': palletNo},
    );
    return (int.tryParse(rows.firstOrNull?['Cnt']?.toString() ?? '0') ?? 0) > 0;
  }

  Future<void> _verifyPd0800(String batch, String run, String pCode) async {
    final rows = await _db.query(
      "SELECT COUNT(Batch) AS Cnt FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
      {'@Batch': batch, '@Run': run, '@PCode': pCode},
    );
    if ((int.tryParse(rows.firstOrNull?['Cnt']?.toString() ?? '0') ?? 0) == 0) {
      throw Exception('Tiada ringkasan stok dalam PD_0800 ($batch/$run/$pCode)');
    }
  }

  Future<void> _upsertTst1(
    PalletInfo pallet,
    double qty, String statusQC, String empNo,
    String tarikhStr, String dateStr, String timeStr,
  ) async {
    final tst1Rows = await _db.query(
      "SELECT * FROM IV_0250 WHERE Loct='TST1' AND Pallet=@Pallet "
      "AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
      {'@Pallet': pallet.pltNo, '@Batch': pallet.batch,
       '@Run': pallet.run, '@PCode': pallet.pCode},
    );

    if (tst1Rows.isEmpty) {
      await _db.execute(
        "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,"
        "OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
        " VALUES ('TST1',@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,"
        "0,@InputQty,@OutputQty,0,@Pallet,@AddUser,@AddDate,@AddTime)",
        {
          '@PCode': pallet.pCode, '@PGroup': pallet.pGroup,
          '@Batch': pallet.batch,
          '@PName': pallet.pName,
          '@Unit': pallet.unit,
          '@Run': pallet.run, '@Status': statusQC,
          '@InputQty': qty, '@OutputQty': qty,
          '@Pallet': pallet.pltNo, '@AddUser': empNo,
          '@AddDate': tarikhStr, '@AddTime': timeStr,
        },
      );
    } else {
      await _db.execute(
        "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=0, "
        "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
        "WHERE Loct='TST1' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {
          '@OutputQty': qty, '@EditUser': empNo,
          '@EditDate': dateStr, '@EditTime': timeStr,
          '@Pallet': pallet.pltNo, '@Batch': pallet.batch,
          '@Run': pallet.run, '@PCode': pallet.pCode,
        },
      );
    }
  }

  Future<void> _upsertTst2(
    PalletInfo pallet,
    double qty, String statusQC, String empNo,
    String tarikhStr, String dateStr, String timeStr,
    {required bool deductStock}
  ) async {
    final tst2Rows = await _db.query(
      "SELECT COUNT(Pallet) AS Cnt FROM IV_0250 WHERE Loct='TST2' "
      "AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
      {'@Pallet': pallet.pltNo, '@Batch': pallet.batch,
       '@Run': pallet.run, '@PCode': pallet.pCode},
    );
    final exists = (int.tryParse(
        tst2Rows.firstOrNull?['Cnt']?.toString() ?? '0') ?? 0) > 0;

    if (!exists) {
      if (deductStock) {
        // First stock-in: InputQty=qty, OutputQty=0, OnHand=qty
        await _db.execute(
          "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,"
          "OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
          " VALUES ('TST2',@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,"
          "0,@InputQty,0,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)",
          {
            '@PCode': pallet.pCode, '@PGroup': pallet.pGroup,
            '@Batch': pallet.batch,
            '@PName': pallet.pName,
            '@Unit': pallet.unit,
            '@Run': pallet.run, '@Status': statusQC,
            '@InputQty': qty, '@OnHand': qty,
            '@Pallet': pallet.pltNo, '@AddUser': empNo,
            '@AddDate': tarikhStr, '@AddTime': timeStr,
          },
        );
      } else {
        // Already stock-in or LOOSE: InputQty=qty, OutputQty=qty, OnHand=0
        await _db.execute(
          "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,"
          "OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
          " VALUES ('TST2',@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,"
          "0,@InputQty,@OutputQty,0,@Pallet,@AddUser,@AddDate,@AddTime)",
          {
            '@PCode': pallet.pCode, '@PGroup': pallet.pGroup,
            '@Batch': pallet.batch,
            '@PName': pallet.pName,
            '@Unit': pallet.unit,
            '@Run': pallet.run, '@Status': statusQC,
            '@InputQty': qty, '@OutputQty': qty,
            '@Pallet': pallet.pltNo, '@AddUser': empNo,
            '@AddDate': tarikhStr, '@AddTime': timeStr,
          },
        );
      }
    } else {
      await _db.execute(
        "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=0, "
        "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
        "WHERE Loct='TST2' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {
          '@OutputQty': qty, '@EditUser': empNo,
          '@EditDate': dateStr, '@EditTime': timeStr,
          '@Pallet': pallet.pltNo, '@Batch': pallet.batch,
          '@Run': pallet.run, '@PCode': pallet.pCode,
        },
      );
    }

    // Deduct from TST2 for first stock-in
    if (deductStock) {
      final tst2StockRows = await _db.query(
        "SELECT * FROM IV_0250 WHERE Loct='TST2' AND Batch=@Batch "
        "AND Run=@Run AND Pallet=@Pallet AND OnHand>0",
        {'@Batch': pallet.batch, '@Run': pallet.run, '@Pallet': pallet.pltNo},
      );
      if (tst2StockRows.isEmpty) throw Exception('Tiada stok di TST2');
      final tst2Row = tst2StockRows.first;
      await _db.execute(
        "UPDATE IV_0250 SET OutputQty=@OutputQty, OnHand=@OnHand, "
        "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
        "WHERE Loct='TST2' AND Pallet=@Pallet AND Batch=@Batch AND Run=@Run AND OnHand>0",
        {
          '@OutputQty': toDouble(tst2Row['OutputQty']) + qty,
          '@OnHand': toDouble(tst2Row['OnHand']) - qty,
          '@EditUser': empNo, '@EditDate': dateStr, '@EditTime': timeStr,
          '@Pallet': pallet.pltNo, '@Batch': pallet.batch, '@Run': pallet.run,
        },
      );
    }
  }

  Future<void> _markTransfer(
      String palletNo, String empNo, String tarikhStr, String timeStr) async {
    await _db.execute(
      "UPDATE TA_PLT001 SET TStatus='Transfer', PickBy=@PickBy, "
      "RecUser=@RecUser, RecDate=@RecDate, RecTime=@RecTime WHERE PltNo=@PltNo",
      {'@PltNo': palletNo, '@PickBy': empNo,
       '@RecUser': empNo, '@RecDate': tarikhStr, '@RecTime': timeStr},
    );
    await _db.execute(
      "UPDATE TA_PLT002 SET TStatus='Transfer', PickBy=@PickBy, "
      "RecUser=@RecUser, RecDate=@RecDate, RecTime=@RecTime WHERE Pallet=@Pallet",
      {'@Pallet': palletNo, '@PickBy': empNo,
       '@RecUser': empNo, '@RecDate': tarikhStr, '@RecTime': timeStr},
    );
  }

  Future<void> _closePallet(String palletNo,
      {required bool transferStatus}) async {
    final tStatus = transferStatus ? ", TStatus='Transfer'" : '';
    await _db.execute(
      "UPDATE TA_PLT001 SET Status='C'$tStatus WHERE PltNo=@PltNo",
      {'@PltNo': palletNo},
    );
    await _db.execute(
      "UPDATE TA_PLT002 SET Status='C'$tStatus WHERE Pallet=@Pallet",
      {'@Pallet': palletNo},
    );
  }

  Future<void> _registerRack({
    required String rackNo, required String batch, required String run,
    required String pCode, required String pGroup, required String pName,
    required String unit, required String status, required double qty,
    required String pallet, required String empNo, required String tarikhStr,
    required String dateStr, required String timeStr,
    required bool firstStockIn,
  }) async {
    final rackRows = await _db.query(
      "SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch "
      "AND Run=@Run AND Pallet=@Pallet",
      {'@Loct': rackNo, '@Batch': batch, '@Run': run, '@Pallet': pallet},
    );

    if (rackRows.isEmpty) {
      await _db.execute(
        "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,Status,"
        "OpenQty,InputQty,OutputQty,OnHand,Pallet,AddUser,AddDate,AddTime)"
        " VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,@Status,"
        "0,@InputQty,0,@OnHand,@Pallet,@AddUser,@AddDate,@AddTime)",
        {
          '@Loct': rackNo, '@PCode': pCode, '@PGroup': pGroup,
          '@Batch': batch, '@PName': pName, '@Unit': unit,
          '@Run': run, '@Status': status, '@InputQty': qty,
          '@OnHand': qty, '@Pallet': pallet, '@AddUser': empNo,
          '@AddDate': tarikhStr, '@AddTime': timeStr,
        },
      );
    } else {
      if (firstStockIn) {
        final existRow = rackRows.first;
        await _db.execute(
          "UPDATE IV_0250 SET OnHand=@OnHand, InputQty=@InputQty, Pallet=@Pallet, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Loct=@Rack AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet AND PCode=@PCode",
          {
            '@OnHand': qty, '@InputQty': toDouble(existRow['InputQty']) + qty,
            '@Pallet': pallet, '@EditUser': empNo,
            '@EditDate': dateStr, '@EditTime': timeStr,
            '@Rack': rackNo, '@Batch': batch, '@Run': run, '@PCode': pCode,
          },
        );
      } else {
        await _db.execute(
          "UPDATE IV_0250 SET OnHand=@OnHand, InputQty=@InputQty, Pallet=@Pallet, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Loct=@Rack AND Batch=@Batch AND Run=@Run AND Pallet=@Pallet",
          {
            '@OnHand': qty, '@InputQty': qty, '@Pallet': pallet,
            '@EditUser': empNo, '@EditDate': dateStr, '@EditTime': timeStr,
            '@Rack': rackNo, '@Batch': batch, '@Run': run,
          },
        );
      }
    }
  }

  Future<void> _updateRackBalance(
      String batch, String run, String pCode, double qty) async {
    try {
      final rackBalRows = await _db.query(
        "SELECT SUM(OnHand) AS Qty FROM IV_0250 "
        "WHERE Batch=@Batch AND Run=@Run AND OnHand>0 GROUP BY Batch, Run",
        {'@Batch': batch, '@Run': run},
      );
      final wRackBal =
          rackBalRows.isNotEmpty ? toDouble(rackBalRows.first['Qty']) : 0.0;

      final pd0800Rows = await _db.query(
        "SELECT Rack_In FROM PD_0800 WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
        {'@Batch': batch, '@Run': run, '@PCode': pCode},
      );
      if (pd0800Rows.isNotEmpty) {
        final existRackIn = toDouble(pd0800Rows.first['Rack_In']);
        await _db.execute(
          "UPDATE PD_0800 SET Rack_In=@Rack_In, SORack=@SORack "
          "WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
          {
            '@Rack_In': existRackIn + qty, '@SORack': wRackBal,
            '@Batch': batch, '@Run': run, '@PCode': pCode,
          },
        );
      }
    } catch (_) {
      // Non-critical for user flow
    }
  }

  Future<void> _upsertLoc0600({
    required String palletNo, required String rackNo, required String batch,
    required String run, required String pCode, required String pName,
    required String pGroup, required double qty, required String unit,
    required String empNo, required String tarikhStr, required String timeStr,
  }) async {
    final todayStr = DateFormat('yyyy-MM-dd').format(DateTime.now());
    try {
      final logRows = await _db.query(
        "SELECT * FROM TA_LOC0600 WHERE Pallet=@Pallet AND Rack=@Rack "
        "AND Batch=@Batch AND Run=@Run",
        {'@Pallet': palletNo, '@Rack': rackNo, '@Batch': batch, '@Run': run},
      );

      if (logRows.isNotEmpty) {
        await _db.execute(
          "UPDATE TA_LOC0600 SET Pallet=@Pallet, Rack=@Rack, Batch=@Batch, Run=@Run, "
          "PCode=@PCode, PName=@PName, PGroup=@PGroup, Qty=@Qty, Unit=@Unit, Ref=@Ref, "
          "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
          "WHERE Pallet=@Pallet AND Rack=@Rack AND Batch=@Batch AND Run=@Run",
          {
            '@Pallet': palletNo, '@Rack': rackNo, '@Batch': batch,
            '@Run': run, '@PCode': pCode, '@PName': pName,
            '@PGroup': pGroup, '@Qty': qty, '@Unit': unit, '@Ref': 1,
            '@EditUser': empNo, '@EditDate': todayStr, '@EditTime': timeStr,
          },
        );
      } else {
        await _db.execute(
          "INSERT INTO TA_LOC0600 (Pallet,Rack,Batch,Run,PCode,PName,PGroup,Qty,Unit,Ref,"
          "AddUser,AddDate,AddTime) VALUES "
          "(@Pallet,@Rack,@Batch,@Run,@PCode,@PName,@PGroup,@Qty,@Unit,@Ref,"
          "@AddUser,@AddDate,@AddTime)",
          {
            '@Pallet': palletNo, '@Rack': rackNo, '@Batch': batch,
            '@Run': run, '@PCode': pCode, '@PName': pName,
            '@PGroup': pGroup, '@Qty': qty, '@Unit': unit, '@Ref': 1,
            '@AddUser': empNo, '@AddDate': tarikhStr, '@AddTime': timeStr,
          },
        );
      }
    } catch (_) {
      // Non-critical
    }
  }
}

// Extension to add copyWith to PalletInfo if not already defined
extension PalletInfoCopyWith on PalletInfo {
  PalletInfo copyWith({String? qs}) {
    return PalletInfo(
      pltNo: pltNo, batch: batch, run: run,
      pCode: pCode, pName: pName, pGroup: pGroup,
      unit: unit, fullQty: fullQty, lsQty: lsQty,
      status: status, tStatus: tStatus,
      qs: qs ?? this.qs,
      isLoose: isLoose, palletDate: palletDate,
      looseEntries: looseEntries,
    );
  }
}
