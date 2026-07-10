/**
 * Log Levels and Configuration
 * 
 * This file controls which log messages are displayed in the console.
 * You can easily enable/disable different types of logs from here.
 * 
 * Usage: Import this file and use LogConfig.shouldLog() before print statements
 * Benefits: Easy to control logging verbosity, better performance in production
 */

enum LogLevel {
  /// No logging
  none,

  /// Only critical errors that affect app functionality
  error,

  /// Important events that need attention
  warning,

  /// General information about app flow
  info,

  /// Detailed debugging information
  debug,

  /// Very detailed debugging information
  verbose,
}

class LogConfig {
  // =================== CURRENT LOG LEVEL ===================
  // Change this to control overall logging verbosity
  static const LogLevel currentLogLevel = LogLevel.info;

  // =================== FEATURE-SPECIFIC LOGGING ===================
  // Enable/disable logging for specific features

  /// Show location tracking logs (GPS coordinates, location updates)
  static const bool showLocationLogs = true;

  /// Show API call logs (requests, responses, errors)
  static const bool showApiLogs = true;

  /// Show database operation logs (inserts, updates, queries)
  static const bool showDatabaseLogs = false;

  /// Show punch out logs (automatic punch out events)
  static const bool showPunchOutLogs = true;

  /// Show background service logs (heartbeat, provider changes)
  static const bool showBackgroundLogs = true;

  /// Show caching logs (location caching, user data caching)
  static const bool showCachingLogs = false;

  /// Show timer and periodic task logs
  static const bool showTimerLogs = false;

  /// Show duplicate detection logs
  static const bool showDuplicateLogs = false;

  /// Show connectivity and network logs
  static const bool showNetworkLogs = true;

  /// Show headless mode logs (when app is killed)
  static const bool showHeadlessLogs = true;

  /// Show error logs (always recommended to keep enabled)
  static const bool showErrorLogs = true;

  /// Show success logs (successful API calls, operations)
  static const bool showSuccessLogs = true;

  // =================== HELPER METHODS ===================

  /// Check if a log level should be displayed
  static bool shouldLog(LogLevel level) {
    return level.index <= currentLogLevel.index;
  }

  /// Log error messages (always important)
  static void logError(String message, [dynamic error]) {
    if (showErrorLogs && shouldLog(LogLevel.error)) {
      print('❌ $message${error != null ? ': $error' : ''}');
    }
  }

  /// Log warning messages
  static void logWarning(String message) {
    if (shouldLog(LogLevel.warning)) {
      print('⚠️ $message');
    }
  }

  /// Log info messages
  static void logInfo(String message) {
    if (shouldLog(LogLevel.info)) {
      print('ℹ️ $message');
    }
  }

  /// Log debug messages
  static void logDebug(String message) {
    if (shouldLog(LogLevel.debug)) {
      print('🐛 $message');
    }
  }

  /// Log location-related messages
  static void logLocation(String message) {
    if (showLocationLogs && shouldLog(LogLevel.info)) {
      print('📍 $message');
    }
  }

  /// Log API-related messages
  static void logApi(String message) {
    if (showApiLogs && shouldLog(LogLevel.info)) {
      print('🌐 $message');
    }
  }

  /// Log database-related messages
  static void logDatabase(String message) {
    if (showDatabaseLogs && shouldLog(LogLevel.debug)) {
      print('💾 $message');
    }
  }

  /// Log punch out related messages
  static void logPunchOut(String message) {
    if (showPunchOutLogs && shouldLog(LogLevel.info)) {
      print('👤 $message');
    }
  }

  /// Log background service messages
  static void logBackground(String message) {
    if (showBackgroundLogs && shouldLog(LogLevel.info)) {
      print('🔄 $message');
    }
  }

  /// Log caching messages
  static void logCaching(String message) {
    if (showCachingLogs && shouldLog(LogLevel.debug)) {
      print('💾 $message');
    }
  }

  /// Log timer and periodic task messages
  static void logTimer(String message) {
    if (showTimerLogs && shouldLog(LogLevel.debug)) {
      print('⏰ $message');
    }
  }

  /// Log duplicate detection messages
  static void logDuplicate(String message) {
    if (showDuplicateLogs && shouldLog(LogLevel.debug)) {
      print('🔄 $message');
    }
  }

  /// Log network and connectivity messages
  static void logNetwork(String message) {
    if (showNetworkLogs && shouldLog(LogLevel.info)) {
      print('📡 $message');
    }
  }

  /// Log headless mode messages
  static void logHeadless(String message) {
    if (showHeadlessLogs && shouldLog(LogLevel.info)) {
      print('🤖 $message');
    }
  }

  /// Log success messages
  static void logSuccess(String message) {
    if (showSuccessLogs && shouldLog(LogLevel.info)) {
      print('✅ $message');
    }
  }

  /// Log heartbeat messages
  static void logHeartbeat(String message) {
    if (showBackgroundLogs && shouldLog(LogLevel.debug)) {
      print('💓 $message');
    }
  }

  /// Log initialization messages
  static void logInit(String message) {
    if (shouldLog(LogLevel.info)) {
      print('🚀 $message');
    }
  }

  /// Log cleanup messages
  static void logCleanup(String message) {
    if (shouldLog(LogLevel.debug)) {
      print('🧹 $message');
    }
  }
}
