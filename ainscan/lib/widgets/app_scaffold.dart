import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart' show kIsWeb;
import 'dart:async';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../config/database_config.dart';
import '../services/keyboard_service.dart';
import '../services/theme_service.dart';

/// Shared scaffold widget used by all screens in AINScan.
///
/// Mirrors the VB.NET form pattern where every form had a title bar with
/// the app version and a Timer1_Tick updating txtDateTime with the current
/// date/time. Provides a consistent layout across the entire application.
class AppScaffold extends StatefulWidget {
  /// The title displayed in the AppBar.
  final String title;

  /// The main body content of the screen.
  final Widget body;

  /// Optional action widgets displayed on the right side of the AppBar.
  final List<Widget>? actions;

  /// Whether to show a back button in the AppBar. Defaults to true.
  final bool showBackButton;

  /// Whether to show the real-time date/time display. Defaults to true.
  final bool showDateTime;

  /// Optional custom title widget (overrides title text if provided).
  final Widget? titleWidget;

  /// Whether to show the DB info footer. Defaults to true.
  final bool showDbFooter;

  /// Whether to show the floating keyboard toggle. Defaults to true.
  /// Set to false on non-scanning screens like menus and login.
  final bool showKeyboardToggle;

  const AppScaffold({
    super.key,
    required this.title,
    required this.body,
    this.actions,
    this.showBackButton = true,
    this.showDateTime = true,
    this.showDbFooter = true,
    this.showKeyboardToggle = true,
    this.titleWidget,
  });

  @override
  State<AppScaffold> createState() => _AppScaffoldState();
}

class _AppScaffoldState extends State<AppScaffold> {
  Timer? _clockTimer;
  String _currentDateTime = '';
  final DateFormat _dateFormat = DateFormat('dd/MM/yyyy HH:mm:ss');

  @override
  void initState() {
    super.initState();
    _updateDateTime();
    // Tick every second, matching VB.NET Timer1_Tick behavior
    _clockTimer = Timer.periodic(
      const Duration(seconds: 1),
      (_) => _updateDateTime(),
    );
  }

  void _updateDateTime() {
    if (!mounted) return;
    setState(() {
      _currentDateTime = _dateFormat.format(DateTime.now());
    });
  }

  @override
  void dispose() {
    _clockTimer?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final themeService = Provider.of<ThemeService>(context);

    // Build combined actions: screen-specific + theme toggle
    final allActions = <Widget>[
      if (widget.actions != null) ...widget.actions!,
      IconButton(
        icon: Icon(themeService.isDark ? Icons.light_mode : Icons.dark_mode),
        tooltip: themeService.isDark ? 'Light Mode' : 'Dark Mode',
        onPressed: () => themeService.toggleTheme(),
        iconSize: 20,
      ),
    ];

    return Scaffold(
      appBar: AppBar(
        automaticallyImplyLeading: widget.showBackButton,
        toolbarHeight: 52,
        centerTitle: widget.titleWidget != null,
        title: widget.titleWidget ?? Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              widget.title,
              style: const TextStyle(
                fontSize: 15,
                fontWeight: FontWeight.bold,
              ),
              overflow: TextOverflow.ellipsis,
            ),
            if (widget.showDateTime)
              Text(
                _currentDateTime,
                style: const TextStyle(
                  fontSize: 10,
                  fontWeight: FontWeight.normal,
                ),
              ),
          ],
        ),
        actions: allActions,
        backgroundColor: Theme.of(context).appBarTheme.backgroundColor ??
            Theme.of(context).colorScheme.primary,
        foregroundColor: Theme.of(context).appBarTheme.foregroundColor ??
            Theme.of(context).colorScheme.onPrimary,
      ),
      body: SafeArea(
        child: Stack(
          children: [
            // Main content
            Column(
              children: [
                Expanded(
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Flexible(
                        child: ConstrainedBox(
                          constraints: const BoxConstraints(maxWidth: 480),
                          child: widget.body,
                        ),
                      ),
                    ],
                  ),
                ),
                // DB info footer
                if (widget.showDbFooter)
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                    decoration: BoxDecoration(
                      color: isDark
                          ? const Color(0xFF172030)
                          : Colors.grey.shade100,
                      border: Border(
                        top: BorderSide(
                          color: isDark ? const Color(0xFF37474F) : Colors.grey.shade300,
                          width: 0.5,
                        ),
                      ),
                    ),
                    child: Text(
                      'Database Connected : ${DatabaseConfig.databaseName}',
                      style: TextStyle(
                        fontSize: 9,
                        fontWeight: isDark ? FontWeight.normal : FontWeight.w600,
                        color: isDark ? const Color(0xFF546E7A) : Colors.grey.shade700,
                      ),
                      textAlign: TextAlign.center,
                    ),
                  ),
              ],
            ),
            // Floating keyboard toggle — mobile only, when enabled
            if (!kIsWeb && widget.showKeyboardToggle)
              Positioned(
                top: 4,
                right: 4,
                child: Consumer<KeyboardService>(
                  builder: (context, kbService, _) {
                    return Material(
                      elevation: 4,
                      shape: const CircleBorder(),
                      color: kbService.keyboardEnabled
                          ? Colors.green.shade600
                          : Colors.blue.shade600,
                      child: InkWell(
                        customBorder: const CircleBorder(),
                        onTap: kbService.toggle,
                        child: Padding(
                          padding: const EdgeInsets.all(8),
                          child: Icon(
                            kbService.keyboardEnabled
                                ? Icons.keyboard
                                : Icons.keyboard_hide,
                            size: 20,
                            color: Colors.white,
                          ),
                        ),
                      ),
                    );
                  },
                ),
              ),
          ],
        ),
      ),
    );
  }
}
