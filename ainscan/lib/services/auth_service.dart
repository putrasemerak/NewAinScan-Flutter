import 'package:flutter/foundation.dart';

class AuthService extends ChangeNotifier {
  String? empNo;
  String? empName;
  String? accessLevel;
  DateTime? loginTime;

  bool get isLoggedIn => empNo != null;

  bool get isAdmin => empNo == '10097';

  void setUser(String empNo, String empName, String accessLevel) {
    this.empNo = empNo;
    this.empName = empName;
    this.accessLevel = accessLevel;
    loginTime = DateTime.now();
    notifyListeners();
  }

  void logout() {
    empNo = null;
    empName = null;
    accessLevel = null;
    loginTime = null;
    notifyListeners();
  }
}
