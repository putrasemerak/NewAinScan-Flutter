class PalletInfo {
  final String pltNo;
  final String batch;
  final String run;
  final String pCode;
  final String pName;
  final String pGroup;
  final String unit;
  final double fullQty;
  final double lsQty;
  final String? status;
  final String? tStatus;
  final String? qs;
  final bool isLoose;

  /// TA_PLT002 AddDate — used as AddDate for IV_0250 inserts (VB.NET: Tarikh)
  final DateTime? palletDate;

  /// LOOSE entries from TA_PLL001 (only populated for loose pallets)
  final List<Map<String, dynamic>> looseEntries;

  double get totalQty => fullQty + lsQty;
  bool get isEmpty => pltNo.isEmpty;
  String get category => isLoose ? 'LOOSE' : 'NORMAL';

  PalletInfo({
    required this.pltNo,
    required this.batch,
    required this.run,
    required this.pCode,
    required this.pName,
    required this.pGroup,
    required this.unit,
    required this.fullQty,
    required this.lsQty,
    this.status,
    this.tStatus,
    this.qs,
    this.isLoose = false,
    this.palletDate,
    this.looseEntries = const [],
  });

  /// Empty singleton used as initial/cleared state.
  static final empty = PalletInfo(
    pltNo: '', batch: '', run: '', pCode: '', pName: '',
    pGroup: '', unit: '', fullQty: 0, lsQty: 0,
  );

  factory PalletInfo.fromMap(Map<String, dynamic> map,
      {bool isLoose = false,
      DateTime? palletDate,
      List<Map<String, dynamic>> looseEntries = const []}) {
    return PalletInfo(
      pltNo: (map['PltNo'] as String? ?? '').trim(),
      batch: (map['Batch'] as String? ?? '').trim(),
      run: (map['Run'] ?? map['Cycle'] ?? '').toString().trim(),
      pCode: (map['PCode'] as String? ?? '').trim(),
      pName: (map['PName'] as String? ?? '').trim(),
      pGroup: (map['PGroup'] as String? ?? '').trim(),
      unit: (map['Unit'] as String? ?? '').trim(),
      fullQty: (map['FullQty'] as num?)?.toDouble() ?? 0.0,
      lsQty: (map['LsQty'] as num?)?.toDouble() ?? 0.0,
      status: map['Status'] as String?,
      tStatus: map['TStatus'] as String?,
      qs: map['QS'] as String?,
      isLoose: isLoose,
      palletDate: palletDate,
      looseEntries: looseEntries,
    );
  }
}
