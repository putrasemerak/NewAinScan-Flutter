import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:flutter/material.dart';
import 'package:mobile_scanner/mobile_scanner.dart';

/// Full-screen barcode scanner dialog using the device camera.
///
/// Replaces the Intermec hardware barcode reader from the VB.NET app.
/// Returns the scanned barcode string via Navigator.pop, or null if cancelled.
///
/// On web, a manual text-entry option is always available alongside the camera
/// because laptop webcams/browser permissions may not always work reliably.
class BarcodeScannerDialog extends StatefulWidget {
  /// Optional title shown at the top of the scanner overlay.
  final String title;

  const BarcodeScannerDialog({
    super.key,
    this.title = 'Scan Barcode',
  });

  /// Convenience method to show the scanner and return the result.
  ///
  /// On web, shows a dialog with both camera scanner AND a manual text input
  /// so the user can type the barcode if the camera is unavailable.
  static Future<String?> show(BuildContext context, {String title = 'Scan Barcode'}) {
    if (kIsWeb) {
      return _showWebScannerDialog(context, title: title);
    }
    return showDialog<String>(
      context: context,
      barrierDismissible: false,
      builder: (_) => BarcodeScannerDialog(title: title),
    );
  }

  /// Web-specific scanner dialog with manual input fallback.
  static Future<String?> _showWebScannerDialog(
    BuildContext context, {
    String title = 'Scan Barcode',
  }) {
    return showDialog<String>(
      context: context,
      barrierDismissible: false,
      builder: (_) => _WebBarcodeScannerDialog(title: title),
    );
  }

  @override
  State<BarcodeScannerDialog> createState() => _BarcodeScannerDialogState();
}

class _BarcodeScannerDialogState extends State<BarcodeScannerDialog> {
  final MobileScannerController _controller = MobileScannerController(
    detectionSpeed: DetectionSpeed.normal,
    facing: CameraFacing.back,
  );
  bool _scanned = false;

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _onDetect(BarcodeCapture capture) {
    if (_scanned) return;
    final barcodes = capture.barcodes;
    if (barcodes.isNotEmpty && barcodes.first.rawValue != null) {
      _scanned = true;
      final value = barcodes.first.rawValue!.trim().toUpperCase();
      Navigator.of(context).pop(value);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Dialog.fullscreen(
      child: Scaffold(
        appBar: AppBar(
          title: Text(widget.title),
          leading: IconButton(
            icon: const Icon(Icons.close),
            onPressed: () => Navigator.of(context).pop(null),
          ),
          actions: [
            IconButton(
              icon: ValueListenableBuilder(
                valueListenable: _controller,
                builder: (context, state, child) {
                  return Icon(
                    state.torchState == TorchState.on
                        ? Icons.flash_on
                        : Icons.flash_off,
                  );
                },
              ),
              onPressed: () => _controller.toggleTorch(),
            ),
          ],
        ),
        body: Stack(
          children: [
            MobileScanner(
              controller: _controller,
              onDetect: _onDetect,
            ),
            // Crosshair overlay
            Center(
              child: Container(
                width: 250,
                height: 250,
                decoration: BoxDecoration(
                  border: Border.all(color: Colors.green, width: 3),
                  borderRadius: BorderRadius.circular(12),
                ),
              ),
            ),
            // Instruction text
            Positioned(
              bottom: 80,
              left: 0,
              right: 0,
              child: Text(
                'Arahkan kamera ke barcode',
                textAlign: TextAlign.center,
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 16,
                  backgroundColor: Colors.black54,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

// =============================================================================
// Web-specific scanner dialog with manual input + optional camera
// =============================================================================

class _WebBarcodeScannerDialog extends StatefulWidget {
  final String title;
  const _WebBarcodeScannerDialog({required this.title});

  @override
  State<_WebBarcodeScannerDialog> createState() =>
      _WebBarcodeScannerDialogState();
}

class _WebBarcodeScannerDialogState extends State<_WebBarcodeScannerDialog> {
  final TextEditingController _textController = TextEditingController();
  bool _showCamera = false;
  MobileScannerController? _cameraController;
  bool _scanned = false;
  String? _cameraError;

  @override
  void dispose() {
    _textController.dispose();
    _cameraController?.dispose();
    super.dispose();
  }

  void _submitManual() {
    final value = _textController.text.trim().toUpperCase();
    if (value.isNotEmpty) {
      Navigator.of(context).pop(value);
    }
  }

  void _toggleCamera() {
    setState(() {
      _showCamera = !_showCamera;
      if (_showCamera && _cameraController == null) {
        _cameraController = MobileScannerController(
          detectionSpeed: DetectionSpeed.normal,
          facing: CameraFacing.back,
        );
      }
    });
  }

  void _onDetect(BarcodeCapture capture) {
    if (_scanned) return;
    final barcodes = capture.barcodes;
    if (barcodes.isNotEmpty && barcodes.first.rawValue != null) {
      _scanned = true;
      final value = barcodes.first.rawValue!.trim().toUpperCase();
      Navigator.of(context).pop(value);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Dialog(
      insetPadding: const EdgeInsets.all(16),
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 500, maxHeight: 600),
        child: Scaffold(
          appBar: AppBar(
            title: Text(widget.title),
            leading: IconButton(
              icon: const Icon(Icons.close),
              onPressed: () => Navigator.of(context).pop(null),
            ),
          ),
          body: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                // Manual text entry (always visible on web)
                Text(
                  'Enter barcode manually:',
                  style: Theme.of(context).textTheme.titleSmall,
                ),
                const SizedBox(height: 8),
                Row(
                  children: [
                    Expanded(
                      child: TextField(
                        controller: _textController,
                        autofocus: true,
                        textCapitalization: TextCapitalization.characters,
                        decoration: const InputDecoration(
                          hintText: 'Type or paste barcode...',
                          border: OutlineInputBorder(),
                          contentPadding: EdgeInsets.symmetric(
                            horizontal: 12,
                            vertical: 14,
                          ),
                        ),
                        onSubmitted: (_) => _submitManual(),
                      ),
                    ),
                    const SizedBox(width: 8),
                    ElevatedButton(
                      onPressed: _submitManual,
                      child: const Text('OK'),
                    ),
                  ],
                ),

                const SizedBox(height: 16),
                const Divider(),
                const SizedBox(height: 8),

                // Camera toggle
                TextButton.icon(
                  onPressed: _toggleCamera,
                  icon: Icon(_showCamera ? Icons.videocam_off : Icons.videocam),
                  label: Text(_showCamera ? 'Hide Camera' : 'Use Camera Scanner'),
                ),

                // Camera view (optional on web)
                if (_showCamera) ...[
                  const SizedBox(height: 8),
                  if (_cameraError != null)
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: Colors.orange.withValues(alpha: 0.15),
                        borderRadius: BorderRadius.circular(4),
                      ),
                      child: Text(
                        'Camera error: $_cameraError\n'
                        'Use manual entry above instead.',
                        style: TextStyle(color: Colors.orange.shade300),
                      ),
                    )
                  else
                    Expanded(
                      child: ClipRRect(
                        borderRadius: BorderRadius.circular(8),
                        child: MobileScanner(
                          controller: _cameraController!,
                          onDetect: _onDetect,
                          errorBuilder: (context, error) {
                            // Schedule error state update after build
                            WidgetsBinding.instance.addPostFrameCallback((_) {
                              if (mounted) {
                                setState(() {
                                  _cameraError = error.errorDetails?.message ??
                                      'Camera not available';
                                  _showCamera = false;
                                });
                              }
                            });
                            return Center(
                              child: Text(
                                'Camera not available.\nUse manual entry above.',
                                textAlign: TextAlign.center,
                              ),
                            );
                          },
                        ),
                      ),
                    ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }
}
