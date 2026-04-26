import 'package:flutter/material.dart';

import '../../config/app_constants.dart';
import '../../widgets/app_scaffold.dart';
import '../../widgets/menu_button.dart';
import 'stock_take_screen.dart';
import 'change_rack_screen.dart';

/// Stock Control menu screen — converts frmMenu_3.vb
///
/// Sub-menu with: TST1/TST2/FGWH/TSTP Stock Take, Change Rack
class StockControlMenuScreen extends StatelessWidget {
  const StockControlMenuScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: '${AppConstants.appVersion} - Stock Control',
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
            // TST1 Stock Take
            MenuButton(
              label: 'TST1 STOCK TAKE',
              icon: Icons.inventory_2,
              color: Colors.green.shade700,
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute(
                    builder: (_) => const StockTakeScreen(
                      location: AppConstants.locationTST1,
                      rackReadOnly: true,
                    ),
                  ),
                );
              },
            ),
            const SizedBox(height: 10),

            // TST2 Stock Take
            MenuButton(
              label: 'TST2 STOCK TAKE',
              icon: Icons.inventory_2,
              color: Colors.green.shade600,
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute(
                    builder: (_) => const StockTakeScreen(
                      location: AppConstants.locationTST2,
                      rackReadOnly: true,
                    ),
                  ),
                );
              },
            ),
            const SizedBox(height: 10),

            // FGWH Stock Take
            MenuButton(
              label: 'FGWH STOCK TAKE',
              icon: Icons.inventory_2,
              color: Colors.blue.shade700,
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute(
                    builder: (_) => const StockTakeScreen(
                      location: AppConstants.locationFGWH,
                      rackReadOnly: false,
                    ),
                  ),
                );
              },
            ),
            const SizedBox(height: 10),

            // TSTP Stock Take
            MenuButton(
              label: 'TSTP STOCK TAKE',
              icon: Icons.inventory_2,
              color: Colors.teal.shade700,
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute(
                    builder: (_) => const StockTakeScreen(
                      location: AppConstants.locationTSTP,
                      rackReadOnly: true,
                    ),
                  ),
                );
              },
            ),
            const SizedBox(height: 10),

            // Change Rack
            MenuButton(
              label: 'CHANGE RACK',
              icon: Icons.swap_horiz,
              color: Colors.orange.shade700,
              onPressed: () {
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => const ChangeRackScreen()),
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
