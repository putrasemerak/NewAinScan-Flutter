import 'package:flutter/material.dart';

import '../../config/app_constants.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/menu_button.dart';
import 'receiving_screen.dart';
import 'rack_in_screen.dart';
import 'bincard_screen.dart';
import 'stock_in_tst_screen.dart';

/// Stock In menu screen — converts frmMenu_4.vb
///
/// Sub-menu with: Receiving, Rack In, Bincard, TST
class StockInMenuScreen extends StatelessWidget {
  const StockInMenuScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: '${AppConstants.appVersion} - Stock In',
      showKeyboardToggle: false,
      body: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        child: Column(
          children: [
            Expanded(
              child: Center(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
            // Receiving
            MenuButton(
              label: 'RECEIVING',
              icon: Icons.move_to_inbox,
              color: Colors.green.shade700,
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => const ReceivingScreen()),
                );
              },
            ),
            const SizedBox(height: 10),

            // Rack In
            MenuButton(
              label: 'RACK IN',
              icon: Icons.shelves,
              color: Colors.blue.shade700,
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => const RackInScreen()),
                );
              },
            ),
            const SizedBox(height: 10),

            // Bincard
            MenuButton(
              label: 'BINCARD',
              icon: Icons.credit_card,
              color: Colors.purple.shade700,
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => const BincardScreen()),
                );
              },
            ),
            const SizedBox(height: 10),

            // TST
            MenuButton(
              label: 'TST',
              icon: Icons.swap_horiz,
              color: Colors.teal.shade700,
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => const StockInTstScreen()),
                );
              },
            ),
                  ],
                ),
              ),
            ),

            // Back button
            MenuButton(
              label: 'BACK',
              icon: Icons.arrow_back,
              color: Colors.grey.shade600,
              onPressed: () => Navigator.of(context).pop(),
            ),
            const SizedBox(height: 8),
          ],
        ),
      ),
    );
  }
}
