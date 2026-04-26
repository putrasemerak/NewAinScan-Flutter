import 'package:flutter/material.dart';

/// Shows a [CircularProgressIndicator] overlay when [isLoading] is true,
/// otherwise shows [child].
///
/// Replaces the repeated `_isLoading ? CircularProgressIndicator() : content`
/// pattern across screens.
class LoadingOverlay extends StatelessWidget {
  const LoadingOverlay({
    super.key,
    required this.isLoading,
    required this.child,
    this.color,
  });

  final bool isLoading;
  final Widget child;
  final Color? color;

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        child,
        if (isLoading)
          Positioned.fill(
            child: Container(
              color: Colors.black26,
              alignment: Alignment.center,
              child: CircularProgressIndicator(
                color: color ?? Theme.of(context).colorScheme.primary,
              ),
            ),
          ),
      ],
    );
  }
}
