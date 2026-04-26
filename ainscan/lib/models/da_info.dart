class DAInfo {
  final String daNo;
  final String? orderNo;
  final String batch;
  final String run;
  final double quantity;
  final String status;
  final String pCode;
  final String? pName;
  final String? pGroup;
  final String? custNo;
  final String? custName;
  final String? address;
  final String? unit;
  final double? orderQty;
  final String? etd;

  DAInfo({
    required this.daNo,
    this.orderNo,
    required this.batch,
    required this.run,
    required this.quantity,
    required this.status,
    required this.pCode,
    this.pName,
    this.pGroup,
    this.custNo,
    this.custName,
    this.address,
    this.unit,
    this.orderQty,
    this.etd,
  });

  factory DAInfo.fromMap(Map<String, dynamic> map) {
    return DAInfo(
      daNo: map['DANo'] as String? ?? '',
      orderNo: map['OrderNo'] as String?,
      batch: map['Batch'] as String? ?? '',
      run: map['Run'] as String? ?? '',
      quantity: (map['Quantity'] as num?)?.toDouble() ?? 0.0,
      status: map['Status'] as String? ?? '',
      pCode: map['PCode'] as String? ?? '',
      pName: map['PName'] as String?,
      pGroup: map['PGroup'] as String?,
      custNo: map['CustNo'] as String?,
      custName: map['CustName'] as String?,
      address: map['Address'] as String?,
      unit: map['Unit'] as String?,
      orderQty: (map['OrderQty'] as num?)?.toDouble(),
      etd: map['ETD'] as String?,
    );
  }
}
