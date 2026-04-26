class StockItem {
  final String loct;
  final String pCode;
  final String pGroup;
  final String batch;
  final String pName;
  final String unit;
  final String run;
  final String? status;
  final double openQty;
  final double inputQty;
  final double outputQty;
  final double onHand;
  final String pallet;

  StockItem({
    required this.loct,
    required this.pCode,
    required this.pGroup,
    required this.batch,
    required this.pName,
    required this.unit,
    required this.run,
    this.status,
    required this.openQty,
    required this.inputQty,
    required this.outputQty,
    required this.onHand,
    required this.pallet,
  });

  factory StockItem.fromMap(Map<String, dynamic> map) {
    return StockItem(
      loct: map['Loct'] as String? ?? '',
      pCode: map['PCode'] as String? ?? '',
      pGroup: map['PGroup'] as String? ?? '',
      batch: map['Batch'] as String? ?? '',
      pName: map['PName'] as String? ?? '',
      unit: map['Unit'] as String? ?? '',
      run: map['Run'] as String? ?? '',
      status: map['Status'] as String?,
      openQty: (map['OpenQty'] as num?)?.toDouble() ?? 0.0,
      inputQty: (map['InputQty'] as num?)?.toDouble() ?? 0.0,
      outputQty: (map['OutputQty'] as num?)?.toDouble() ?? 0.0,
      onHand: (map['OnHand'] as num?)?.toDouble() ?? 0.0,
      pallet: map['Pallet'] as String? ?? '',
    );
  }
}
