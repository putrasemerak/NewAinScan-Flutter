import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

/// A full-width navigation button used in menu screens.
///
/// Mirrors the large navigation buttons from frmMainMenu, frmMenu_2,
/// frmMenu_3, and frmMenu_4 in the VB.NET application.
class MenuButton extends StatelessWidget {
  /// The text label displayed on the button.
  final String label;

  /// Callback invoked when the button is pressed.
  final VoidCallback onPressed;

  /// Background color of the button. Defaults to blue.
  final Color? color;

  /// Optional icon displayed to the left of the label.
  final IconData? icon;

  const MenuButton({
    super.key,
    required this.label,
    required this.onPressed,
    this.color,
    this.icon,
  });

  @override
  Widget build(BuildContext context) {
    final buttonColor = color ?? Colors.blue;
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final borderColor = isDark ? Colors.yellow : Colors.black;

    return ConstrainedBox(
      constraints: const BoxConstraints(maxWidth: 220),
      child: SizedBox(
        width: double.infinity,
        height: 56,
        child: ElevatedButton(
          onPressed: onPressed,
          style: ElevatedButton.styleFrom(
            backgroundColor: buttonColor,
            foregroundColor: Colors.white,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(12),
              side: BorderSide(
                color: borderColor,
                width: 1.5,
              ),
            ),
            elevation: 1,
            shadowColor: buttonColor.withValues(alpha: 0.3),
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 0),
          ),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.center,
            mainAxisSize: MainAxisSize.min,
            children: [
              if (icon != null) ...[
                Icon(icon, size: 22),
                const SizedBox(width: 10),
              ],
              Flexible(
                child: Text(
                  label,
                  style: GoogleFonts.robotoCondensed(
                    fontSize: 14,
                    fontWeight: FontWeight.bold,
                    letterSpacing: 0.5,
                  ),
                  overflow: TextOverflow.ellipsis,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
