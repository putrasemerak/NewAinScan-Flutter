// Basic smoke test for the AINScan app.
//
// This verifies that the app starts without crashing.

import 'package:flutter_test/flutter_test.dart';

import 'package:ainscan/main.dart';

void main() {
  testWidgets('App starts without crashing', (WidgetTester tester) async {
    await tester.pumpWidget(const AINScanApp());
    // The app should render the login screen
    expect(find.text('AINScan'), findsWidgets);
  });
}
