import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

/// A compact labeled text field used throughout the app.
///
/// Two layout modes:
/// - **horizontal** (default): label on the left, text field on the right
/// - **vertical**: label above the text field (read-only display style)
class LabeledTextField extends StatelessWidget {
  const LabeledTextField({
    super.key,
    required this.label,
    this.controller,
    this.readOnly = false,
    this.enabled = true,
    this.keyboardType,
    this.onSubmitted,
    this.focusNode,
    this.labelWidth = 60,
    this.vertical = false,
    this.value,
    this.style,
    this.labelStyle,
    this.decoration,
    this.contentPadding,
  });

  final String label;
  final TextEditingController? controller;
  final bool readOnly;
  final bool enabled;
  final TextInputType? keyboardType;
  final ValueChanged<String>? onSubmitted;
  final FocusNode? focusNode;
  final double labelWidth;
  final bool vertical;

  /// For read-only display without a controller.
  final String? value;

  final TextStyle? style;
  final TextStyle? labelStyle;
  final InputDecoration? decoration;
  final EdgeInsetsGeometry? contentPadding;

  static TextStyle _defaultLabelStyle(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GoogleFonts.robotoCondensed(
      fontSize: 11,
      fontWeight: FontWeight.w600,
      color: isDark ? const Color(0xFFB0B3B8) : Colors.black54,
    );
  }

  static TextStyle _defaultValueStyle(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GoogleFonts.robotoCondensed(
      fontSize: 12,
      color: isDark ? const Color(0xFFE0E0E0) : null,
    );
  }

  @override
  Widget build(BuildContext context) {
    if (vertical) return _buildVertical(context);
    return _buildHorizontal(context);
  }

  Widget _buildHorizontal(BuildContext context) {
    return Row(
      children: [
        SizedBox(
          width: labelWidth,
          child: Text(label, style: labelStyle ?? _defaultLabelStyle(context)),
        ),
        const SizedBox(width: 4),
        Expanded(
          child: TextField(
            controller: controller,
            readOnly: readOnly,
            enabled: enabled,
            focusNode: focusNode,
            keyboardType: readOnly ? null : keyboardType,
            onSubmitted: onSubmitted,
            style: style ?? _defaultValueStyle(context),
            decoration: decoration ??
                InputDecoration(
                  isDense: true,
                  contentPadding: contentPadding ??
                      const EdgeInsets.symmetric(horizontal: 6, vertical: 6),
                ),
          ),
        ),
      ],
    );
  }

  Widget _buildVertical(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final isEditable = controller != null && !readOnly;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 5),
      decoration: BoxDecoration(
        color: isEditable
            ? (isDark ? const Color(0xFF343B47) : Colors.white)
            : cs.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(4),
        border: Border.all(
          color: isDark
              ? const Color(0xFF546E7A)
              : const Color(0xFF455A64),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: labelStyle ?? _defaultLabelStyle(context)),
          if (controller != null)
            SizedBox(
              height: 22,
              child: TextField(
                controller: controller,
                readOnly: readOnly,
                enabled: enabled,
                focusNode: focusNode,
                keyboardType: readOnly ? null : keyboardType,
                onSubmitted: onSubmitted,
                style: style ?? _defaultValueStyle(context),
                decoration: const InputDecoration(
                  isDense: true,
                  contentPadding: EdgeInsets.zero,
                  border: InputBorder.none,
                  enabledBorder: InputBorder.none,
                  focusedBorder: InputBorder.none,
                  disabledBorder: InputBorder.none,
                  filled: false,
                ),
              ),
            )
          else
            Text(
              (value?.isEmpty ?? true) ? '\u2013' : value!,
              style: style ?? _defaultValueStyle(context),
            ),
        ],
      ),
    );
  }
}

/// Pairs two [LabeledTextField] widgets side by side.
///
/// Replaces the common `_fieldRow()` pattern found in DA screens.
class LabeledFieldRow extends StatelessWidget {
  const LabeledFieldRow({
    super.key,
    required this.label1,
    required this.label2,
    this.controller1,
    this.controller2,
    this.readOnly1 = false,
    this.readOnly2 = false,
    this.labelWidth1 = 72,
    this.labelWidth2 = 62,
    this.keyboardType,
    this.style,
    this.labelStyle,
    this.decoration,
  });

  final String label1;
  final String label2;
  final TextEditingController? controller1;
  final TextEditingController? controller2;
  final bool readOnly1;
  final bool readOnly2;
  final double labelWidth1;
  final double labelWidth2;
  final TextInputType? keyboardType;
  final TextStyle? style;
  final TextStyle? labelStyle;
  final InputDecoration? decoration;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: LabeledTextField(
            label: label1,
            controller: controller1,
            readOnly: readOnly1,
            labelWidth: labelWidth1,
            keyboardType: keyboardType,
            style: style,
            labelStyle: labelStyle,
            decoration: decoration,
          ),
        ),
        const SizedBox(width: 8),
        Expanded(
          child: LabeledTextField(
            label: label2,
            controller: controller2,
            readOnly: readOnly2,
            labelWidth: labelWidth2,
            keyboardType: keyboardType,
            style: style,
            labelStyle: labelStyle,
            decoration: decoration,
          ),
        ),
      ],
    );
  }
}
