import 'package:flutter/material.dart';

import '../../config/app_constants.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/menu_button.dart';
import 'outbound_da_screen.dart';
import 'da_list_screen.dart';


/// Stock Out menu screen — converts frmMenu_2.vb
///
/// Sub-menu with: Outbound (DA Scan), DA List
class StockOutMenuScreen extends StatelessWidget {
  const StockOutMenuScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: '${AppConstants.appVersion} - Stock Out',
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
            // Outbound (DA Scan)
            MenuButton(
              label: 'OUTBOUND (DA SCAN)',
              icon: Icons.local_shipping,
              color: Colors.orange.shade700,
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => const OutboundDaScreen()),
                );
              },
            ),
            const SizedBox(height: 10),

            // DA List
            MenuButton(
              label: 'DA LIST',
              icon: Icons.list_alt,
              color: Colors.blue.shade700,
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => const DaListScreen()),
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
