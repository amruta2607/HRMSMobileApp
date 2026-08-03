package com.example.altroz

import io.flutter.embedding.android.FlutterActivity
import io.flutter.embedding.engine.FlutterEngine
import io.flutter.plugin.common.MethodChannel
import android.content.Intent
import android.net.Uri
import android.os.Build
import android.os.Bundle
import android.os.PowerManager
import android.provider.Settings
import androidx.annotation.RequiresApi

class MainActivity: FlutterActivity() {
    private val BATTERY_CHANNEL = "battery_optimization"
    private val DEVICE_STATE_CHANNEL = "device_state"

    override fun onCreate(savedInstanceState: Bundle?) {
        destroyBackgroundEngines()
        super.onCreate(savedInstanceState)
    }

    /**
     * Aggressively scan all plugin classes for stale background FlutterEngine
     * instances and destroy them before the main engine attaches.
     * This prevents the black-screen-on-relaunch bug in flutter_background_geolocation v4.
     */
    private fun destroyBackgroundEngines() {
        val classNames = listOf(
            "com.transistorsoft.flutter.backgroundgeolocation.HeadlessTask",
            "com.transistorsoft.flutter.backgroundfetch.HeadlessTask",
            "com.transistorsoft.flutter.backgroundgeolocation.FLTBackgroundGeolocationPlugin",
            "com.transistorsoft.flutter.backgroundgeolocation.BackgroundGeolocationModule"
        )

        for (className in classNames) {
            try {
                val clazz = Class.forName(className)

                // 1) Scan ALL declared fields for any static FlutterEngine reference
                for (field in clazz.declaredFields) {
                    try {
                        if (java.lang.reflect.Modifier.isStatic(field.modifiers)) {
                            field.isAccessible = true
                            val value = field.get(null) ?: continue
                            if (isFlutterEngine(value)) {
                                android.util.Log.w("MainActivity",
                                    "Destroying stale background FlutterEngine: $className.${field.name}")
                                try {
                                    value.javaClass.getMethod("destroy").invoke(value)
                                } catch (ex: Exception) {
                                    android.util.Log.w("MainActivity",
                                        "destroy() failed on ${field.name}: ${ex.message}")
                                }
                                field.set(null, null)
                            }
                        }
                    } catch (ex: Exception) { /* skip field */ }
                }

                // 2) Try calling destroyBackgroundIsolate() if it exists (v5 back-compat)
                try {
                    val method = clazz.getDeclaredMethod("destroyBackgroundIsolate")
                    method.isAccessible = true
                    method.invoke(null)
                    android.util.Log.d("MainActivity",
                        "Called destroyBackgroundIsolate() on $className")
                } catch (_: NoSuchMethodException) { /* not available in v4 */ }
                  catch (ex: Exception) {
                    android.util.Log.w("MainActivity",
                        "destroyBackgroundIsolate() error: ${ex.message}")
                }

            } catch (_: ClassNotFoundException) { /* class not present */ }
              catch (ex: Exception) {
                android.util.Log.e("MainActivity",
                    "Error cleaning up $className: ${ex.message}", ex)
            }
        }
    }

    /** Walk the class hierarchy to check whether [obj] is a FlutterEngine */
    private fun isFlutterEngine(obj: Any): Boolean {
        var c: Class<*>? = obj.javaClass
        while (c != null) {
            if (c.name == "io.flutter.embedding.engine.FlutterEngine") return true
            c = c.superclass
        }
        return false
    }

    override fun configureFlutterEngine(flutterEngine: FlutterEngine) {
        super.configureFlutterEngine(flutterEngine)
        
        MethodChannel(flutterEngine.dartExecutor.binaryMessenger, BATTERY_CHANNEL).setMethodCallHandler { call, result ->
            when (call.method) {
                "isBatteryOptimizationDisabled" -> {
                    result.success(isBatteryOptimizationDisabled())
                }
                "requestDisableBatteryOptimization" -> {
                    requestDisableBatteryOptimization()
                    result.success(null)
                }
                else -> {
                    result.notImplemented()
                }
            }
        }

        MethodChannel(flutterEngine.dartExecutor.binaryMessenger, DEVICE_STATE_CHANNEL)
            .setMethodCallHandler { call, result ->
                when (call.method) {
                    "isAirplaneModeOn" -> result.success(isAirplaneModeOn())
                    else -> result.notImplemented()
                }
            }
    }

    private fun isAirplaneModeOn(): Boolean {
        return try {
            Settings.Global.getInt(contentResolver, Settings.Global.AIRPLANE_MODE_ON, 0) != 0
        } catch (e: Exception) {
            false
        }
    }

    private fun isBatteryOptimizationDisabled(): Boolean {
        return if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            val powerManager = getSystemService(POWER_SERVICE) as PowerManager
            powerManager.isIgnoringBatteryOptimizations(packageName)
        } else {
            true // Battery optimization not available on older Android versions
        }
    }

    @RequiresApi(Build.VERSION_CODES.M)
    private fun requestDisableBatteryOptimization() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            val intent = Intent(Settings.ACTION_REQUEST_IGNORE_BATTERY_OPTIMIZATIONS).apply {
                data = Uri.parse("package:$packageName")
            }
            
            try {
                startActivity(intent)
            } catch (e: Exception) {
                // Fallback to general battery optimization settings
                val fallbackIntent = Intent(Settings.ACTION_IGNORE_BATTERY_OPTIMIZATION_SETTINGS)
                try {
                    startActivity(fallbackIntent)
                } catch (e2: Exception) {
                    // Last fallback to general settings
                    val generalIntent = Intent(Settings.ACTION_APPLICATION_DETAILS_SETTINGS).apply {
                        data = Uri.parse("package:$packageName")
                    }
                    startActivity(generalIntent)
                }
            }
        }
    }
}
