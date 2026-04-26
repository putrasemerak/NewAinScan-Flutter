/// Application constants - mirrors values from VB.NET project
class AppConstants {
  static const String appVersion = 'V14JAN2018';
  static const String programId = 'WH04';
  static const String appTitle = 'AINScan';

  // Admin employee number (for DB access screen)
  static const String adminEmpNo = '10097';

  // Barcode length rules (from VB.NET barcode reader logic)
  static const int palletMinLength = 7;
  static const int palletMaxLength = 13;
  static const int rackMinLength = 4;
  static const int rackMaxLength = 6;
  static const int daNumberLength = 13;

  // Transit locations
  static const String locationTST1 = 'TST1';
  static const String locationTST2 = 'TST2';
  static const String locationTST3 = 'TST3';
  static const String locationTST4 = 'TST4';
  static const String locationFGWH = 'FGWH';
  static const String locationTSTP = 'TSTP';

  /// Determine barcode type from its length
  static BarcodeType getBarcodeType(String barcode) {
    final len = barcode.length;
    if (len == daNumberLength) return BarcodeType.daNumber;
    if (len >= palletMinLength && len <= palletMaxLength) {
      return BarcodeType.pallet;
    }
    if (len >= rackMinLength && len <= rackMaxLength) return BarcodeType.rack;
    return BarcodeType.unknown;
  }

  /// Check if barcode indicates Kulim product (9-char pallet)
  static bool isKulimPallet(String barcode) => barcode.length == 9;
}

enum BarcodeType { pallet, rack, daNumber, unknown }
