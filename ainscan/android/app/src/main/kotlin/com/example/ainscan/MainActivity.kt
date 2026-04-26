package com.example.ainscan

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.content.IntentFilter
import android.os.Build
import android.util.Log

import io.flutter.embedding.android.FlutterActivity
import io.flutter.embedding.engine.FlutterEngine
import io.flutter.plugin.common.EventChannel

/**
 * MainActivity with hardware barcode scanner support via broadcast intents.
 *
 * Seuic AutoID (and many other industrial scanner brands) broadcast
 * scanned barcode data as Android intents. We register a BroadcastReceiver
 * for known scanner actions and forward the barcode string to Flutter
 * through an EventChannel.
 *
 * Physical keyboard input (numeric keypad) is NOT intercepted - it flows
 * through to Flutter TextFields normally via the standard input connection.
 */
class MainActivity : FlutterActivity() {

    companion object {
        private const val TAG = "AINScanScanner"

        val SCANNER_INTENT_ACTIONS = listOf(
            // Seuic AutoID
            "com.android.server.scannerservice.broadcast",
            "com.seuic.scanner.ResultBroadcast",
            // Honeywell / Intermec
            "com.honeywell.sample.action.BARCODE_DATA",
            "com.honeywell.decode.intent.action.EDIT_DATA",
            // Zebra / Symbol DataWedge
            "com.symbol.datawedge.api.RESULT_ACTION",
            // Generic / multi-brand
            "android.intent.ACTION_DECODE_DATA",
            "android.intent.action.DECODE_DATA",
            "android.intent.action.SCANRESULT",
            "scan.rcv.message",
            // Chainway
            "com.scanner.broadcast",
            // Sunmi
            "com.sunmi.scanner.ACTION_DATA_CODE_RECEIVED",
            // Newland
            "nlscan.action.SCANNER_RESULT",
            // Point Mobile
            "device.scanner.USERMSG",
        )

        val DATA_EXTRAS = listOf(
            // Seuic AutoID
            "scannerdata",
            "Scan_context",
            "barcode_string",
            // Honeywell
            "com.honeywell.sample.action.data",
            "com.honeywell.decode.intent.extra.EDIT_DATA",
            // Zebra
            "com.symbol.datawedge.data_string",
            // Generic
            "data",
            "value",
            "scan_result",
            "code",
            "SCAN_BARCODE1",
            "nlscan.action.SCANNER_RESULT",
        )
    }

    private var scannerEventSink: EventChannel.EventSink? = null
    private var scannerReceiver: BroadcastReceiver? = null

    override fun configureFlutterEngine(flutterEngine: FlutterEngine) {
        super.configureFlutterEngine(flutterEngine)

        EventChannel(
            flutterEngine.dartExecutor.binaryMessenger,
            "com.example.ainscan/scanner"
        ).setStreamHandler(object : EventChannel.StreamHandler {
            override fun onListen(arguments: Any?, events: EventChannel.EventSink?) {
                scannerEventSink = events
                registerScannerReceiver()
                Log.d(TAG, "EventChannel listening - broadcast receiver active")
            }

            override fun onCancel(arguments: Any?) {
                unregisterScannerReceiver()
                scannerEventSink = null
                Log.d(TAG, "EventChannel cancelled")
            }
        })
    }

    private fun registerScannerReceiver() {
        if (scannerReceiver != null) return

        scannerReceiver = object : BroadcastReceiver() {
            override fun onReceive(context: Context?, intent: Intent?) {
                intent ?: return
                Log.d(TAG, "Broadcast received - action: ${'$'}{intent.action}")
                val barcode = extractBarcodeData(intent)
                if (!barcode.isNullOrEmpty()) {
                    Log.d(TAG, "Broadcast barcode: ${'$'}barcode")
                    scannerEventSink?.success(barcode)
                }
            }
        }

        val filter = IntentFilter()
        for (action in SCANNER_INTENT_ACTIONS) {
            filter.addAction(action)
        }

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
            registerReceiver(scannerReceiver, filter, Context.RECEIVER_EXPORTED)
        } else {
            registerReceiver(scannerReceiver, filter)
        }
        Log.d(TAG, "Broadcast receiver registered for ${'$'}{SCANNER_INTENT_ACTIONS.size} actions")
    }

    private fun unregisterScannerReceiver() {
        scannerReceiver?.let {
            try { unregisterReceiver(it) } catch (_: Exception) {}
        }
        scannerReceiver = null
    }

    private fun extractBarcodeData(intent: Intent): String? {
        for (key in DATA_EXTRAS) {
            val value = intent.getStringExtra(key)
            if (!value.isNullOrEmpty()) {
                Log.d(TAG, "  Found data in extra '${'$'}key'")
                return value.trim()
            }
        }
        intent.extras?.let { bundle ->
            for (key in bundle.keySet()) {
                val value = bundle.get(key)
                if (value is String && value.isNotEmpty()) {
                    Log.d(TAG, "  Found data in fallback extra '${'$'}key': ${'$'}value")
                    return value.trim()
                }
            }
        }
        Log.d(TAG, "  No barcode found. All extras: ${'$'}{intent.extras?.keySet()?.joinToString()}")
        return null
    }

    override fun onDestroy() {
        unregisterScannerReceiver()
        super.onDestroy()
    }
}