# Flutter specific rules
-keep class io.flutter.app.** { *; }
-keep class io.flutter.plugin.**  { *; }
-keep class io.flutter.util.**  { *; }
-keep class io.flutter.view.**  { *; }
-keep class io.flutter.**  { *; }
-keep class io.flutter.plugins.**  { *; }

# Geolocator plugin
-keep class com.baseflow.geolocator.** { *; }
-dontwarn com.baseflow.geolocator.**

# TransistorSoft Background Geolocation plugin (Paid / Premium)
-keep class com.transistorsoft.** { *; }
-keep class com.transistorsoft.locationmanager.** { *; }
-dontwarn com.transistorsoft.locationmanager.**
-keep class com.transistorsoft.locationmanager.adapter.BootReceiver { *; }
-keep class * extends com.transistorsoft.locationmanager.adapter.TSBackgroundReceiver

# Location services (Google Play Services)
-keep class com.google.android.gms.location.** { *; }
-dontwarn com.google.android.gms.location.**

# Keep Altroz classes
-keep class com.example.altroz.** { *; }

# Preserve native methods
-keepclasseswithmembernames class * {
    native <methods>;
}

# Keep enum fields
-keepclassmembers enum * {
    public static **[] values();
    public static ** valueOf(java.lang.String);
}

# Keep Parcelable classes
-keep class * implements android.os.Parcelable {
  public static final android.os.Parcelable$Creator *;
}

# Remove debug logs in release
-assumenosideeffects class android.util.Log {
    public static *** d(...);
    public static *** v(...);
    public static *** i(...);
}

# Keep SQLite classes (for offline database caching)
-keep class android.database.sqlite.** { *; }
-keep class org.sqlite.** { *; }

# Keep HTTP classes for API calls
-keep class okhttp3.** { *; }
-keep class okio.** { *; }
-dontwarn okhttp3.**
-dontwarn okio.**

# Keep JSON serialization classes
-keepattributes Signature
-keepattributes *Annotation*
-keep class com.google.gson.** { *; }
-keep class * implements com.google.gson.TypeAdapterFactory
-keep class * implements com.google.gson.JsonSerializer
-keep class * implements com.google.gson.JsonDeserializer

# Prevent obfuscation of background tasks & workers
-keep class androidx.work.** { *; }
-keep class * extends androidx.work.Worker
-keep class * extends androidx.work.InputMerger
-keep class * extends androidx.work.ListenableWorker

# Keep notification classes
-keep class androidx.core.app.NotificationCompat** { *; }
-keep class androidx.core.app.NotificationManagerCompat** { *; }

# Keep Play Core classes
-keep class com.google.android.play.core.** { *; }
-keep class com.google.android.play.core.splitcompat.** { *; }
-keep class com.google.android.play.core.splitinstall.** { *; }
-keep class com.google.android.play.core.tasks.** { *; }

# Keep Flutter deferred components classes
-keep class io.flutter.embedding.engine.deferredcomponents.** { *; }

# Suppress warnings
-dontwarn com.google.android.play.core.**
-dontwarn com.google.android.play.core.splitcompat.**
-dontwarn com.google.android.play.core.splitinstall.**
-dontwarn com.google.android.play.core.tasks.**
-dontwarn io.flutter.embedding.engine.deferredcomponents.**
