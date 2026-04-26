import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';

import '../../services/activity_log_service.dart';
import '../../services/auth_service.dart';
import '../../services/database_service.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/data_list_view.dart';
import '../../widgets/scan_field.dart';
import '../../widgets/barcode_scanner_dialog.dart';
import '../../utils/converters.dart';

/// Bincard screen — converts frmBincard.vb
///
/// Bincard-based stock-in for reprint pallets. Scans a bincard number,
/// validates the rack location, then processes inbound entries by
/// updating IV_0250, PD_0800, and logging to TA_LOC0600.
class BincardScreen extends StatefulWidget {
  const BincardScreen({super.key});

  @override
  State<BincardScreen> createState() => _BincardScreenState();
}

class _BincardScreenState extends State<BincardScreen> {
  final DatabaseService _db = DatabaseService();
  final TextEditingController _bincardController = TextEditingController();
  final TextEditingController _rackController = TextEditingController();
  final TextEditingController _monthController = TextEditingController();
  final TextEditingController _yearController = TextEditingController();

  // Focus nodes for scanner workflow
  final FocusNode _bincardFocus = FocusNode();
  final FocusNode _rackFocus = FocusNode();

  bool _stockTakeEnabled = false;
  bool _loading = false;

  // Stock take month/year read from SY_0040
  double _stockTakeMonth = 0;
  double _stockTakeYear = 0;

  List<Map<String, String>> _bincardRows = [];
  List<Map<String, String>> _resultRows = [];

  // Full bincard data for processing (includes PGroup, PName, Unit, Cycle)
  List<Map<String, dynamic>> _bincardData = [];

  static const _bincardColumns = [
    DataColumnConfig(name: 'BinCardNo', flex: 2),
    DataColumnConfig(name: 'Pallet', flex: 2),
    DataColumnConfig(name: 'Actual', flex: 1),
    DataColumnConfig(name: 'Batch', flex: 2),
    DataColumnConfig(name: 'PCode', flex: 2),
  ];

  static const _resultColumns = [
    DataColumnConfig(name: 'Pallet', flex: 2),
    DataColumnConfig(name: 'Actual', flex: 1),
    DataColumnConfig(name: 'Batch', flex: 2),
    DataColumnConfig(name: 'Status', flex: 2),
  ];

  @override
  void initState() {
    super.initState();
    _loadMonthYear();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _bincardFocus.requestFocus();
    });
  }

  @override
  void dispose() {
    _bincardController.dispose();
    _rackController.dispose();
    _monthController.dispose();
    _yearController.dispose();
    _bincardFocus.dispose();
    _rackFocus.dispose();
    super.dispose();
  }

  // ---------------------------------------------------------------------------
  // Load stock take month/year from SY_0040
  // VB.NET: SELECT MSEQ, MMIN FROM SY_0040 WHERE KEYCD1=110 AND KEYCD2='FGWH'
  // ---------------------------------------------------------------------------
  Future<void> _loadMonthYear() async {
    try {
      final results = await _db.query(
        "SELECT MSEQ, MMIN FROM SY_0040 "
        "WHERE KEYCD1=110 AND KEYCD2='FGWH'",
      );
      if (results.isNotEmpty) {
        final row = results.first;
        _stockTakeMonth = toDouble(row['MSEQ']);
        _stockTakeYear = toDouble(row['MMIN']);
        _monthController.text = _stockTakeMonth.toStringAsFixed(0);
        _yearController.text = _stockTakeYear.toStringAsFixed(0);
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Error loading month/year: $e'),
          ),
        );
      }
    }
  }

  // ---------------------------------------------------------------------------
  // Bincard scan — mirrors VB.NET txtBincard_KeyPress
  // VB.NET: SELECT * FROM TA_PLT003 WHERE BinCardNo=@BinCardNo OR PltNo=@PltNo
  // ---------------------------------------------------------------------------
  Future<void> _searchBincard(String value) async {
    final bincard = value.trim().toUpperCase();
    if (bincard.isEmpty) return;

    if (!mounted) return;
    setState(() => _loading = true);
    try {
      final results = await _db.query(
        "SELECT * FROM TA_PLT003 "
        "WHERE BinCardNo=@BinCardNo OR PltNo=@PltNo",
        {'@BinCardNo': bincard, '@PltNo': bincard},
      );

      if (results.isEmpty) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Nombor Bincard tidak sah'),
              backgroundColor: Colors.red,
            ),
          );
        }
        return;
      }

      if (!mounted) return;
      setState(() {
        _bincardData = List.from(results);
        _bincardRows = results.map((row) {
          return {
            'BinCardNo': (row['BinCardNo'] ?? '').toString().trim(),
            'Pallet': (row['PltNo'] ?? '').toString().trim(),
            'Actual': (row['Actual'] ?? '').toString().trim(),
            'Batch': (row['Batch'] ?? '').toString().trim(),
            'PCode': (row['PCode'] ?? '').toString().trim(),
          };
        }).toList();
      });
      _rackFocus.requestFocus();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Error: $e')),
        );
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  // ---------------------------------------------------------------------------
  // Rack validation
  // VB.NET: SELECT Rack FROM BD_0010 WHERE Rack=@Rack
  // ---------------------------------------------------------------------------
  Future<bool> _validateRack(String rackNo) async {
    final rack = rackNo.trim().toUpperCase();
    if (rack.isEmpty) return false;

    try {
      final results = await _db.query(
        "SELECT Rack FROM BD_0010 WHERE Rack=@Rack",
        {'@Rack': rack},
      );
      if (results.isEmpty) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Nombor lokasi tidak sah'),
              backgroundColor: Colors.red,
            ),
          );
        }
        return false;
      }
      return true;
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Error: $e')),
        );
      }
      return false;
    }
  }

  // ---------------------------------------------------------------------------
  // Main inbound processing — mirrors VB.NET btnInbound_Click
  // Loops through TA_PLT003 entries and updates IV_0250, PD_0800, TA_LOC0600
  // ---------------------------------------------------------------------------
  Future<void> _processInbound() async {
    if (_bincardData.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Nombor Bincard tidak sah')),
      );
      return;
    }

    final rackNo = _rackController.text.trim().toUpperCase();
    if (!await _validateRack(rackNo)) return;
    if (!mounted) return;

    final auth = context.read<AuthService>();
    final user = auth.empNo ?? '';
    final now = DateTime.now();
    final dateStr = DateFormat('yyyy-MM-dd').format(now);
    final timeStr = DateFormat('HH:mm:ss').format(now);

    if (!mounted) return;
    setState(() {
      _loading = true;
      _resultRows = [];
    });

    final results = <Map<String, String>>[];

    try {
      for (final entry in _bincardData) {
        final pallet = (entry['PltNo'] ?? '').toString().trim();
        final actual = toDouble(entry['Actual']);
        final batch = (entry['Batch'] ?? '').toString().trim();
        final cycle = (entry['Cycle'] ?? '').toString().trim(); // Run
        final pCode0 = (entry['PCode'] ?? '').toString().trim();
        final pGroup = (entry['PGroup'] ?? '').toString().trim();
        final pName = (entry['PName'] ?? '').toString().trim();
        final unit = (entry['Unit'] ?? '').toString().trim();

        String status = 'Success';

        try {
          // -----------------------------------------------------------------
          // Step 1: Check PD_0800
          // VB.NET: SELECT Batch, QS, PCode FROM PD_0800
          //   WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode
          // -----------------------------------------------------------------
          String qcStatus = '';
          String pCode = pCode0;
          final pdRows = await _db.query(
            "SELECT Batch, QS, PCode FROM PD_0800 "
            "WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
            {'@Batch': batch, '@Run': cycle, '@PCode': pCode0},
          );

          if (pdRows.isEmpty) {
            results.add({
              'Pallet': pallet,
              'Actual': actual.toStringAsFixed(0),
              'Batch': batch,
              'Status': 'Tiada ringkasan stok dalam PD_0800',
            });
            continue;
          }
          qcStatus = (pdRows.first['QS'] ?? '').toString().trim();
          pCode = (pdRows.first['PCode'] ?? pCode0).toString().trim();

          // -----------------------------------------------------------------
          // Step 2: Update Stock-In IV_0250
          // VB.NET: SELECT * FROM IV_0250
          //   WHERE Loct=@Loct AND Batch=@Batch AND Run=@Run
          //   AND Pallet=@Pallet AND PCode=@PCode
          // -----------------------------------------------------------------
          final iv0250Rows = await _db.query(
            "SELECT * FROM IV_0250 WHERE Loct=@Loct AND Batch=@Batch "
            "AND Run=@Run AND Pallet=@Pallet AND PCode=@PCode",
            {
              '@Loct': rackNo,
              '@Batch': batch,
              '@Run': cycle,
              '@Pallet': pallet,
              '@PCode': pCode,
            },
          );

          if (iv0250Rows.isEmpty) {
            // INSERT new rack location
            // VB.NET: INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,
            //   Run,Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,
            //   AddUser,AddDate,AddTime)
            await _db.execute(
              "INSERT INTO IV_0250 (Loct,PCode,PGroup,Batch,PName,Unit,Run,"
              "Status,OpenQty,InputQty,OutputQty,OnHand,Pallet,"
              "AddUser,AddDate,AddTime) "
              "VALUES (@Loct,@PCode,@PGroup,@Batch,@PName,@Unit,@Run,"
              "@Status,0,@InputQty,0,@OnHand,@Pallet,"
              "@AddUser,@AddDate,@AddTime)",
              {
                '@Loct': rackNo,
                '@PCode': pCode,
                '@PGroup': pGroup,
                '@Batch': batch,
                '@PName': pName,
                '@Unit': unit,
                '@Run': cycle,
                '@Status': qcStatus,
                '@InputQty': actual,
                '@OnHand': actual,
                '@Pallet': pallet,
                '@AddUser': user,
                '@AddDate': dateStr,
                '@AddTime': timeStr,
              },
            );
          } else {
            // UPDATE existing: OnHand = existing + Actual, InputQty = existing + Actual
            // VB.NET: WHERE Loct=@Rack AND Batch=@Batch AND Run=@Run
            //   AND Pallet=@Pallet AND PCode=@PCode
            final existOnHand = toDouble(iv0250Rows.first['OnHand']);
            final existInputQty = toDouble(iv0250Rows.first['InputQty']);

            await _db.execute(
              "UPDATE IV_0250 SET OnHand=@OnHand, InputQty=@InputQty, "
              "Pallet=@Pallet, EditUser=@EditUser, EditDate=@EditDate, "
              "EditTime=@EditTime "
              "WHERE Loct=@Rack AND Batch=@Batch AND Run=@Run "
              "AND Pallet=@Pallet AND PCode=@PCode",
              {
                '@OnHand': existOnHand + actual,
                '@InputQty': existInputQty + actual,
                '@Pallet': pallet,
                '@EditUser': user,
                '@EditDate': dateStr,
                '@EditTime': timeStr,
                '@Rack': rackNo,
                '@Batch': batch,
                '@Run': cycle,
                '@PCode': pCode,
              },
            );
          }

          // -----------------------------------------------------------------
          // Step 3: Calculate rack balance and update PD_0800
          // VB.NET: SELECT SUM(OnHand) AS Qty FROM IV_0250
          //   WHERE Batch=@Batch AND Run=@Run AND OnHand>0
          //   GROUP BY Batch, Run
          // -----------------------------------------------------------------
          double wRackBal = 0;
          try {
            final rackBalRows = await _db.query(
              "SELECT SUM(OnHand) AS Qty FROM IV_0250 "
              "WHERE Batch=@Batch AND Run=@Run AND OnHand>0 "
              "GROUP BY Batch, Run",
              {'@Batch': batch, '@Run': cycle},
            );
            if (rackBalRows.isNotEmpty) {
              wRackBal = toDouble(rackBalRows.first['Qty']);
            }

            // VB.NET: SELECT Rack_In FROM PD_0800
            //   WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode
            final pd0800Rows = await _db.query(
              "SELECT Rack_In FROM PD_0800 "
              "WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
              {'@Batch': batch, '@Run': cycle, '@PCode': pCode},
            );

            if (pd0800Rows.isNotEmpty) {
              final existRackIn = toDouble(pd0800Rows.first['Rack_In']);

              // VB.NET: UPDATE PD_0800
              //   SET Rack_In=existing+Actual, SORack=W_RackBal, Balance=W_RackBal
              //   WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode
              await _db.execute(
                "UPDATE PD_0800 SET Rack_In=@Rack_In, SORack=@SORack, "
                "Balance=@Balance "
                "WHERE Batch=@Batch AND Run=@Run AND PCode=@PCode",
                {
                  '@Rack_In': existRackIn + actual,
                  '@SORack': wRackBal,
                  '@Balance': wRackBal,
                  '@Batch': batch,
                  '@Run': cycle,
                  '@PCode': pCode,
                },
              );
            }
          } catch (_) {
            // Non-critical — continue
          }

          // -----------------------------------------------------------------
          // Step 4: Log to TA_LOC0600 (upsert)
          // VB.NET: SELECT * FROM TA_LOC0600
          //   WHERE Pallet=@Pallet AND Rack=@Rack AND Batch=@Batch AND Run=@Run
          // -----------------------------------------------------------------
          try {
            final logRows = await _db.query(
              "SELECT * FROM TA_LOC0600 WHERE Pallet=@Pallet AND Rack=@Rack "
              "AND Batch=@Batch AND Run=@Run",
              {
                '@Pallet': pallet,
                '@Rack': rackNo,
                '@Batch': batch,
                '@Run': cycle,
              },
            );

            if (logRows.isNotEmpty) {
              // UPDATE existing log
              await _db.execute(
                "UPDATE TA_LOC0600 SET Pallet=@Pallet, Rack=@Rack, "
                "Batch=@Batch, Run=@Run, PCode=@PCode, PName=@PName, "
                "PGroup=@PGroup, Qty=@Qty, Unit=@Unit, Ref=@Ref, "
                "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
                "WHERE Pallet=@Pallet AND Rack=@Rack "
                "AND Batch=@Batch AND Run=@Run",
                {
                  '@Pallet': pallet,
                  '@Rack': rackNo,
                  '@Batch': batch,
                  '@Run': cycle,
                  '@PCode': pCode,
                  '@PName': pName,
                  '@PGroup': pGroup,
                  '@Qty': actual,
                  '@Unit': unit,
                  '@Ref': 1,
                  '@EditUser': user,
                  '@EditDate': dateStr,
                  '@EditTime': timeStr,
                },
              );
            } else {
              // INSERT new log
              await _db.execute(
                "INSERT INTO TA_LOC0600 (Pallet,Rack,Batch,Run,PCode,PName,"
                "PGroup,Qty,Unit,Ref,AddUser,AddDate,AddTime) VALUES "
                "(@Pallet,@Rack,@Batch,@Run,@PCode,@PName,"
                "@PGroup,@Qty,@Unit,@Ref,@AddUser,@AddDate,@AddTime)",
                {
                  '@Pallet': pallet,
                  '@Rack': rackNo,
                  '@Batch': batch,
                  '@Run': cycle,
                  '@PCode': pCode,
                  '@PName': pName,
                  '@PGroup': pGroup,
                  '@Qty': actual,
                  '@Unit': unit,
                  '@Ref': 1,
                  '@AddUser': user,
                  '@AddDate': dateStr,
                  '@AddTime': timeStr,
                },
              );
            }
          } catch (_) {
            // Non-critical
          }

          // -----------------------------------------------------------------
          // Step 5: Stock take — TA_STK001 (upsert)
          // VB.NET: If OptStockTake.Checked = True Then ...
          // -----------------------------------------------------------------
          if (_stockTakeEnabled) {
            try {
              final stkCountRows = await _db.query(
                "SELECT COUNT(Pallet) AS Cnt FROM TA_STK001 "
                "WHERE PMonth=@PMonth AND PYear=@PYear AND Pallet=@Pallet "
                "AND Batch=@Batch AND Run=@Run",
                {
                  '@PMonth': _stockTakeMonth,
                  '@PYear': _stockTakeYear,
                  '@Pallet': pallet,
                  '@Batch': batch,
                  '@Run': cycle,
                },
              );
              final stkExists = (int.tryParse(
                          stkCountRows.firstOrNull?['Cnt']?.toString() ??
                              '0') ??
                      0) >
                  0;

              if (!stkExists) {
                // INSERT new stock take record
                await _db.execute(
                  "INSERT INTO TA_STK001 (PMonth,PYear,Pallet,Batch,Run,"
                  "Rack,Qty,AddUser,AddDate,AddTime) VALUES "
                  "(@PMonth,@PYear,@Pallet,@Batch,@Run,"
                  "@Rack,@Qty,@AddUser,@AddDate,@AddTime)",
                  {
                    '@PMonth': _stockTakeMonth,
                    '@PYear': _stockTakeYear,
                    '@Pallet': pallet,
                    '@Batch': batch,
                    '@Run': cycle,
                    '@Rack': rackNo,
                    '@Qty': actual,
                    '@AddUser': user,
                    '@AddDate': dateStr,
                    '@AddTime': timeStr,
                  },
                );
              } else {
                // UPDATE existing stock take record
                await _db.execute(
                  "UPDATE TA_STK001 SET PMonth=@PMonth, PYear=@PYear, "
                  "Pallet=@Pallet, Batch=@Batch, Run=@Run, "
                  "Rack=@Rack, Qty=@Qty, "
                  "EditUser=@EditUser, EditDate=@EditDate, EditTime=@EditTime "
                  "WHERE PMonth=@PMonth AND PYear=@PYear AND Pallet=@Pallet "
                  "AND Batch=@Batch AND Run=@Run",
                  {
                    '@PMonth': _stockTakeMonth,
                    '@PYear': _stockTakeYear,
                    '@Pallet': pallet,
                    '@Batch': batch,
                    '@Run': cycle,
                    '@Rack': rackNo,
                    '@Qty': actual,
                    '@EditUser': user,
                    '@EditDate': dateStr,
                    '@EditTime': timeStr,
                  },
                );
              }
            } catch (_) {
              // Non-critical
            }
          }
        } catch (e) {
          status = 'Failed';
        }

        results.add({
          'Pallet': pallet,
          'Actual': actual.toStringAsFixed(0),
          'Batch': batch,
          'Status': status,
        });
      }

      if (!mounted) return;
      setState(() => _resultRows = results);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Processed ${results.length} entries'),
            backgroundColor: Colors.green,
          ),
        );
      }

      await ActivityLogService.log(
        action: 'BINCARD_INBOUND',
        empNo: auth.empNo ?? '',
        detail: 'Entries: ${results.length}',
      );

      // VB.NET: clear fields after processing
      _bincardController.clear();
      _rackController.clear();
      _bincardFocus.requestFocus();
    } catch (e) {
      await ActivityLogService.logError(
        action: 'BINCARD_INBOUND',
        empNo: auth.empNo ?? '',
        errorMsg: '$e',
      );
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Error: $e')),
        );
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  // ---------------------------------------------------------------------------
  // Utility
  // ---------------------------------------------------------------------------

  // ---------------------------------------------------------------------------
  // Build
  // ---------------------------------------------------------------------------
  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthService>();
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Bincard',
      body: Column(
        children: [
          // User info
          Padding(
            padding: const EdgeInsets.fromLTRB(12, 12, 12, 0),
            child: Align(
              alignment: Alignment.centerLeft,
              child: Text(
                'User: ${auth.empNo ?? ''}@${auth.empName ?? ''}',
                style: TextStyle(fontSize: 12, color: isDark ? const Color(0xFF80CBC4) : Colors.grey),
              ),
            ),
          ),

          // Input fields
          Padding(
            padding: const EdgeInsets.all(12.0),
            child: Column(
              children: [
                // Bincard No scan field
                ScanField(
                  controller: _bincardController,
                  focusNode: _bincardFocus,
                  label: 'Bincard No',
                  filled: true,
                  fillColor: isDark ? const Color(0xFF343B47) : Colors.white,
                  style: GoogleFonts.robotoCondensed(
                      fontSize: 13, color: isDark ? const Color(0xFFE4E8EE) : Colors.black),
                  labelStyle: GoogleFonts.robotoCondensed(
                      fontSize: 12, color: isDark ? const Color(0xFF80CBC4) : null),
                  onSubmitted: _searchBincard,
                  onScanPressed: () async {
                    final result = await BarcodeScannerDialog.show(
                      context,
                      title: 'Scan Bincard',
                    );
                    if (result != null) {
                      _bincardController.text = result;
                      _searchBincard(result);
                    }
                  },
                ),
                const SizedBox(height: 10),

                // Rack No scan field
                ScanField(
                  controller: _rackController,
                  focusNode: _rackFocus,
                  label: 'Rack No',
                  filled: true,
                  fillColor: isDark ? const Color(0xFF343B47) : Colors.white,
                  style: GoogleFonts.robotoCondensed(
                      fontSize: 13, color: isDark ? const Color(0xFFE4E8EE) : Colors.black),
                  labelStyle: GoogleFonts.robotoCondensed(
                      fontSize: 12, color: isDark ? const Color(0xFF80CBC4) : null),
                  onScanPressed: () async {
                    final result = await BarcodeScannerDialog.show(
                      context,
                      title: 'Scan Rack',
                    );
                    if (result != null) {
                      _rackController.text = result;
                    }
                  },
                ),
                const SizedBox(height: 10),

                // Stock Take toggle and Month/Year
                Row(
                  children: [
                    // Stock Take toggle
                    Expanded(
                      child: SwitchListTile(
                        title: const Text(
                          'Stock Take',
                          style: TextStyle(fontSize: 13),
                        ),
                        value: _stockTakeEnabled,
                        dense: true,
                        contentPadding: EdgeInsets.zero,
                        onChanged: (val) {
                          if (!mounted) return;
                          setState(() => _stockTakeEnabled = val);
                        },
                      ),
                    ),

                    // Month field
                    SizedBox(
                      width: 60,
                      child: TextField(
                        controller: _monthController,
                        readOnly: true,
                        decoration: const InputDecoration(
                          labelText: 'Month',
                          border: OutlineInputBorder(),
                          contentPadding: EdgeInsets.symmetric(
                            horizontal: 8,
                            vertical: 10,
                          ),
                          isDense: true,
                        ),
                        style: const TextStyle(fontSize: 13),
                      ),
                    ),
                    const SizedBox(width: 8),

                    // Year field
                    SizedBox(
                      width: 70,
                      child: TextField(
                        controller: _yearController,
                        readOnly: true,
                        decoration: const InputDecoration(
                          labelText: 'Year',
                          border: OutlineInputBorder(),
                          contentPadding: EdgeInsets.symmetric(
                            horizontal: 8,
                            vertical: 10,
                          ),
                          isDense: true,
                        ),
                        style: const TextStyle(fontSize: 13),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 10),

                // Inbound button
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton.icon(
                    onPressed: _loading ? null : _processInbound,
                    icon: const Icon(Icons.move_to_inbox),
                    label: const Text('Inbound'),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.green.shade700,
                      foregroundColor: Colors.white,
                    ),
                  ),
                ),
              ],
            ),
          ),

          // Loading indicator
          if (_loading) const LinearProgressIndicator(),

          // Bincard entries list
          Expanded(
            child: DataListView(
              columns: _bincardColumns,
              rows: _bincardRows,
            ),
          ),

          // Divider between the two lists
          if (_resultRows.isNotEmpty) ...[
            const Divider(height: 1, thickness: 2),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
              child: Align(
                alignment: Alignment.centerLeft,
                child: Text(
                  'Inbound Results',
                  style: TextStyle(
                    fontSize: 13,
                    fontWeight: FontWeight.bold,
                    color: Colors.grey.shade700,
                  ),
                ),
              ),
            ),
            Expanded(
              child: DataListView(
                columns: _resultColumns,
                rows: _resultRows,
              ),
            ),
          ],

          // Close button
          Padding(
            padding: const EdgeInsets.all(12.0),
            child: SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () => Navigator.of(context).pop(),
                icon: const Icon(Icons.close),
                label: const Text('Close'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.grey.shade600,
                  foregroundColor: Colors.white,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
