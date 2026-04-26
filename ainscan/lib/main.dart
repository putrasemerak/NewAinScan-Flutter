import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:sqflite_common_ffi_web/sqflite_ffi_web.dart';
import 'package:sqflite/sqflite.dart';

import 'package:google_fonts/google_fonts.dart';

import 'services/auth_service.dart';
import 'services/keyboard_service.dart';
import 'services/theme_service.dart';
import 'services/hardware_scanner_service.dart';
import 'screens/login/login_screen.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();

  // Use bundled fonts only – never fetch from the network at runtime.
  GoogleFonts.config.allowRuntimeFetching = false;

  // Start listening for hardware barcode scanner broadcasts (Android intent).
  HardwareScannerService.init();

  // On Web, use the FFI-web database factory (SQLite compiled to WASM).
  // On Android/iOS, sqflite uses the native factory automatically.
  if (kIsWeb) {
    databaseFactory = databaseFactoryFfiWeb;
  }

  runApp(const AINScanApp());
}

class AINScanApp extends StatelessWidget {
  const AINScanApp({super.key});

  static ThemeData _buildTheme(Brightness brightness) {
    final isDark = brightness == Brightness.dark;
    final seed = Colors.blue;
    final colorScheme = ColorScheme.fromSeed(
      seedColor: seed,
      brightness: brightness,
    );

    // Dark mode: blue-tinted dark (professional with subtle color)
    const darkSurface    = Color(0xFF1E2228); // main background (blue-tinted)
    const darkCard       = Color(0xFF262B33); // cards / containers (blue-tinted)
    const darkAppBar     = Color(0xFF172030); // app bar (dark navy)
    const darkInputFill  = Color(0xFF343B47); // text field fill (blue-tinted)

    return ThemeData(
      colorScheme: isDark
          ? colorScheme.copyWith(
              surface: darkSurface,
              surfaceContainerHighest: const Color(0xFF2F3541),
              primary: const Color(0xFF5CB8FF),
              onPrimary: Colors.white,
              primaryContainer: const Color(0xFF1A3A5C),
              onPrimaryContainer: const Color(0xFFBBDEFF),
              secondary: const Color(0xFF4DB6AC),
              onSecondary: Colors.white,
              tertiary: const Color(0xFFFFB74D),
              onTertiary: Colors.black,
              onSurface: const Color(0xFFE4E8EE),
              outline: const Color(0xFF546E7A),
              outlineVariant: const Color(0xFF37474F),
            )
          : colorScheme,
      useMaterial3: true,
      brightness: brightness,
      scaffoldBackgroundColor:
          isDark ? darkSurface : null,
      appBarTheme: AppBarTheme(
        centerTitle: false,
        elevation: 2,
        backgroundColor:
            isDark ? darkAppBar : null,
        foregroundColor: isDark ? const Color(0xFFE4E8EE) : null,
      ),
      cardTheme: CardThemeData(
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(4),
        ),
        margin: EdgeInsets.zero,
        color: isDark ? darkCard : null,
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          minimumSize: const Size(double.infinity, 48),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(4),
            side: isDark
                ? const BorderSide(color: Colors.yellow, width: 1)
                : BorderSide.none,
          ),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        border: const OutlineInputBorder(),
        contentPadding:
            const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
        filled: isDark,
        fillColor: isDark ? darkInputFill : null,
        labelStyle: isDark
            ? const TextStyle(color: Color(0xFFB0BEC5))
            : null,
        floatingLabelStyle: isDark
            ? const TextStyle(color: Color(0xFF80CBC4))
            : null,
        enabledBorder: OutlineInputBorder(
          borderSide: BorderSide(
            color: isDark
                ? const Color(0xFF546E7A)  // blue-grey for dark mode
                : const Color(0xFF455A64), // dark blue-grey for light mode
          ),
        ),
        focusedBorder: OutlineInputBorder(
          borderSide: BorderSide(
            color: isDark ? const Color(0xFF5CB8FF) : colorScheme.primary,
            width: 2,
          ),
        ),
      ),
      dividerColor:
          isDark ? const Color(0xFF37474F) : null,
      dialogTheme: isDark
          ? DialogThemeData(
              backgroundColor: darkCard,
              titleTextStyle: const TextStyle(
                color: Color(0xFFE4E8EE),
                fontSize: 18,
                fontWeight: FontWeight.w600,
              ),
              contentTextStyle: const TextStyle(
                color: Color(0xFFDCE2EA),
                fontSize: 14,
              ),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(8),
              ),
            )
          : null,
      snackBarTheme: isDark
          ? const SnackBarThemeData(
              backgroundColor: Color(0xFF2F3541),
              contentTextStyle: TextStyle(color: Color(0xFFE4E8EE)),
              actionTextColor: Color(0xFF5CB8FF),
            )
          : null,
      textTheme: _applyFont(isDark
          ? const TextTheme(
              bodyLarge: TextStyle(color: Color(0xFFE4E8EE)),
              bodyMedium: TextStyle(color: Color(0xFFDCE2EA)),
              bodySmall: TextStyle(color: Color(0xFFB0BEC5)),
              titleMedium: TextStyle(color: Colors.white),
              titleSmall: TextStyle(color: Color(0xFFDCE2EA)),
              labelLarge: TextStyle(color: Colors.white),
              labelMedium: TextStyle(color: Color(0xFFB0BEC5)),
            )
          : null),
    );
  }

  /// Apply Roboto Condensed as the app-wide default font.
  static TextTheme _applyFont(TextTheme? overrides) {
    final base = GoogleFonts.robotoCondensedTextTheme();
    if (overrides == null) return base;
    return base.merge(overrides);
  }

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AuthService()),
        ChangeNotifierProvider(create: (_) => KeyboardService()),
        ChangeNotifierProvider(create: (_) => ThemeService()),
      ],
      child: Consumer<ThemeService>(
        builder: (context, themeService, _) {
          return MaterialApp(
            title: 'AINScan',
            debugShowCheckedModeBanner: false,
            theme: _buildTheme(Brightness.light),
            darkTheme: _buildTheme(Brightness.dark),
            themeMode: themeService.themeMode,
            home: const LoginScreen(),
          );
        },
      ),
    );
  }
}
