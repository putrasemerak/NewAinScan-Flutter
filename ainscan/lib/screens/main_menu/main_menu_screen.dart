import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../config/app_constants.dart';
import '../../services/activity_log_service.dart';
import '../../services/auth_service.dart';
import '../../services/database_service.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/menu_button.dart';
import '../login/login_screen.dart';
import '../stock_in/stock_in_menu_screen.dart';
import '../stock_out/stock_out_menu_screen.dart';
import '../stock_control/stock_control_menu_screen.dart';
import '../utility/db_screen.dart';
import '../utility/activity_log_screen.dart';

/// Main menu screen — converts frmMainMenu.vb
///
/// Shows 3 navigation buttons: Stock In, Stock Out, Stock Control.
/// Has Logout button (records logout time) and DB connection button (admin only).
class MainMenuScreen extends StatelessWidget {
  const MainMenuScreen({super.key});

  Future<void> _logout(BuildContext context) async {
    // Capture navigator before any async gap to avoid context issues
    final navigator = Navigator.of(context);
    final authService = Provider.of<AuthService>(context, listen: false);
    final db = DatabaseService();

    // Record logout time in SY_ScanLOG (mirrors frmMainMenu.Button1_Click)
    // Wrap in timeout so logout never hangs
    try {
      if (db.isConnected) {
        final loginTime = authService.loginTime?.toString() ?? '';
        const scannerName = 'Flutter_App';

        final logRows = await db.query(
          "SELECT * FROM SY_ScanLOG WHERE LoginTime=@LoginTime AND ScannerName=@ScannerName",
          {'@LoginTime': loginTime, '@ScannerName': scannerName},
        ).timeout(const Duration(seconds: 5));
        if (logRows.isNotEmpty) {
          await db.execute(
            "UPDATE SY_ScanLOG SET LogOutTime=@LogOutTime, LogOutDate=@LogOutDate "
            "WHERE LoginTime=@LoginTime AND ScannerName=@ScannerName",
            {
              '@LogOutTime': DateTime.now().toString(),
              '@LogOutDate': DateTime.now().toIso8601String().substring(0, 10),
              '@LoginTime': loginTime,
              '@ScannerName': scannerName,
            },
          ).timeout(const Duration(seconds: 5));
        }
      }
    } catch (_) {
      // Non-critical: proceed with logout even if logging fails
    }

    try {
      await ActivityLogService.log(
        action: 'LOGOUT',
        empNo: authService.empNo ?? '',
      );
    } catch (_) {}

    authService.logout();

    // Use the pre-captured navigator to avoid using a stale context
    navigator.pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const LoginScreen()),
      (_) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    final authService = Provider.of<AuthService>(context);
    final userDisplay = '${authService.empNo ?? ''}@${authService.empName ?? ''}';

    return AppScaffold(
      title: AppConstants.appVersion,
      showBackButton: false,
      showKeyboardToggle: false,
      actions: [
        // Activity log viewer
        IconButton(
          icon: const Icon(Icons.history),
          tooltip: 'Activity Log',
          onPressed: () {
            Navigator.of(context).push(
              MaterialPageRoute(builder: (_) => const ActivityLogScreen()),
            );
          },
        ),
        // DB connection button — admin only (EmpNo 10097)
        if (authService.isAdmin)
          IconButton(
            icon: const Icon(Icons.storage),
            tooltip: 'Database Info',
            onPressed: () {
              Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => const DbScreen()),
              );
            },
          ),
      ],
      body: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        child: Column(
          children: [
            // User info bar
            Container(
              width: double.infinity,
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              decoration: BoxDecoration(
                color: Theme.of(context).colorScheme.surfaceContainerHighest,
                borderRadius: BorderRadius.circular(8),
              ),
              child: Text(
                userDisplay,
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  color: Theme.of(context).colorScheme.onSurface,
                ),
              ),
            ),

            // Centered menu buttons
            Expanded(
              child: Center(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    // Stock In
                    MenuButton(
              label: 'STOCK IN',
              icon: Icons.input,
              color: Colors.green.shade700,
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => const StockInMenuScreen()),
                );
              },
            ),
            const SizedBox(height: 10),

            // Stock Out
            MenuButton(
              label: 'STOCK OUT',
              icon: Icons.output,
              color: Colors.orange.shade700,
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => const StockOutMenuScreen()),
                );
              },
            ),
            const SizedBox(height: 10),

            // Stock Control
            MenuButton(
              label: 'STOCK CONTROL',
              icon: Icons.inventory,
              color: Colors.blue.shade700,
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => const StockControlMenuScreen()),
                );
              },
            ),
                  ],
                ),
              ),
            ),

            // Logout button
            MenuButton(
              label: 'LOGOUT',
              icon: Icons.logout,
              color: Colors.red.shade700,
              onPressed: () => _logout(context),
            ),
            const SizedBox(height: 8),
          ],
        ),
      ),
    );
  }
}
