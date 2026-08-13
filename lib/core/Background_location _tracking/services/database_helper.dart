/**
 * Database Helper for Location Tracking
 * 
 * This class handles all SQLite database operations for location tracking data.
 * It provides methods to store, retrieve, update, and delete location data.
 * 
 * Features:
 * - Singleton pattern for database instance management
 * - Location data CRUD operations
 * - Batch operations for better performance
 * - Database size and count monitoring
 * - Pagination support for large datasets
 * - Sync status tracking for API integration
 */

import 'dart:async';
import 'dart:io';
import 'package:sqflite/sqflite.dart';
import 'package:path/path.dart';
import '../../constants/log_levels.dart';
import '../../constants/location_config.dart';
import '../models/location_model.dart';

class DatabaseHelper {
  // Singleton instance
  static final DatabaseHelper _instance = DatabaseHelper._internal();
  static DatabaseHelper get instance => _instance;

  // Database instance
  static Database? _database;

  // Database configuration
  static const String _databaseName = 'location_tracking.db';
  static const int _databaseVersion = 2;
  static const String _tableName = 'locations';

  // Private constructor for singleton
  DatabaseHelper._internal();

  /**
   * Get database instance
   * Creates database if it doesn't exist
   */
  Future<Database> get database async {
    if (_database != null) return _database!;
    _database = await _initDatabase();
    return _database!;
  }

  /**
   * Initialize database
   * Creates database file and tables
   */
  Future<Database> _initDatabase() async {
    String path = join(await getDatabasesPath(), _databaseName);
    return await openDatabase(
      path,
      version: _databaseVersion,
      onCreate: _onCreate,
      onUpgrade: _onUpgrade,
    );
  }

  /**
   * Create database tables
   * Called when database is first created
   */
  Future<void> _onCreate(Database db, int version) async {
    await db.execute('''
      CREATE TABLE $_tableName(
        id INTEGER PRIMARY KEY,
        latitude TEXT NOT NULL,
        longitude TEXT NOT NULL,
        timestamp TEXT NOT NULL,
        isSynced INTEGER NOT NULL DEFAULT 0,
        errorMessage TEXT,
        retryCount INTEGER NOT NULL DEFAULT 0,
        locationFrom TEXT NOT NULL DEFAULT 'online'
      )
    ''');
  }

  /**
   * Database upgrade handler
   * For future schema migrations
   */
  Future<void> _onUpgrade(Database db, int oldVersion, int newVersion) async {
    LogConfig.logInfo(
        'Upgrading database from version $oldVersion to $newVersion');

    if (oldVersion < 2) {
      // Add locationFrom column in version 2
      try {
        await db.execute(
            'ALTER TABLE $_tableName ADD COLUMN locationFrom TEXT NOT NULL DEFAULT "online"');
        LogConfig.logInfo('Successfully added locationFrom column');
      } catch (e) {
        LogConfig.logError('Error adding locationFrom column: $e', e);
        // Column might already exist, that's okay
      }
    }
  }

  /**
   * Insert a new location record
   * Enforces offlineStorageLimit from dashboard config (drop oldest first).
   * @param location - LocationData object to insert
   * @returns Future<int> - Row ID of inserted record
   */
  Future<int> insertLocation(LocationData location) async {
    final Database db = await database;
    await _enforceOfflineStorageLimit(db);
    return await db.insert(
      _tableName,
      _locationToMap(location),
      conflictAlgorithm: ConflictAlgorithm.replace,
    );
  }

  /// Drop oldest rows when count would exceed [LocationConfig.MAX_LOCATIONS_IN_DATABASE].
  /// Prefers deleting synced rows first, then oldest unsent.
  Future<void> _enforceOfflineStorageLimit(Database db) async {
    try {
      final max = LocationConfig.MAX_LOCATIONS_IN_DATABASE;
      if (max <= 0) return;

      final count = Sqflite.firstIntValue(
              await db.rawQuery('SELECT COUNT(*) FROM $_tableName')) ??
          0;
      if (count < max) return;

      final toDelete = count - max + 1;

      // Prefer synced oldest
      final synced = await db.query(
        _tableName,
        columns: ['id'],
        where: 'isSynced = ?',
        whereArgs: [1],
        orderBy: 'timestamp ASC',
        limit: toDelete,
      );

      var deleted = 0;
      if (synced.isNotEmpty) {
        final ids = synced.map((r) => r['id'] as int).toList();
        deleted = await db.delete(
          _tableName,
          where: 'id IN (${List.filled(ids.length, '?').join(',')})',
          whereArgs: ids,
        );
      }

      final stillNeeded = toDelete - deleted;
      if (stillNeeded > 0) {
        final oldest = await db.query(
          _tableName,
          columns: ['id'],
          orderBy: 'timestamp ASC',
          limit: stillNeeded,
        );
        if (oldest.isNotEmpty) {
          final ids = oldest.map((r) => r['id'] as int).toList();
          await db.delete(
            _tableName,
            where: 'id IN (${List.filled(ids.length, '?').join(',')})',
            whereArgs: ids,
          );
        }
      }

      LogConfig.logCleanup(
          'Offline storage limit ($max): trimmed ~$toDelete oldest location(s)');
    } catch (e) {
      LogConfig.logError('Error enforcing offline storage limit', e);
    }
  }

  /**
   * Update an existing location record
   * @param location - LocationData object to update
   * @returns Future<int> - Number of rows affected
   */
  Future<int> updateLocation(LocationData location) async {
    final Database db = await database;
    return await db.update(
      _tableName,
      _locationToMap(location),
      where: 'id = ?',
      whereArgs: [location.id],
    );
  }

  /**
   * Get all locations ordered by timestamp
   * @returns Future<List<LocationData>> - List of all locations
   */
  Future<List<LocationData>> getLocations() async {
    final Database db = await database;
    final List<Map<String, dynamic>> maps =
        await db.query(_tableName, orderBy: 'timestamp ASC');

    return _mapListToLocationList(maps);
  }

  /**
   * Get all locations ordered by timestamp (alias for consistency)
   * @returns Future<List<LocationData>> - List of all locations
   */
  Future<List<LocationData>> getAllLocations() async {
    return await getLocations();
  }

  /**
   * Get unsynchronized locations (not sent to API)
   * @returns Future<List<LocationData>> - List of unsent locations
   */
  Future<List<LocationData>> getUnsentLocations() async {
    final Database db = await database;
    final List<Map<String, dynamic>> maps = await db.query(
      _tableName,
      where: 'isSynced = ?',
      whereArgs: [0],
      orderBy: 'timestamp ASC',
    );

    return _mapListToLocationList(maps);
  }

  /**
   * Get recent locations with limit
   * @param limit - Maximum number of locations to return (default: 100)
   * @returns Future<List<LocationData>> - List of recent locations
   */
  Future<List<LocationData>> getRecentLocations({int limit = 100}) async {
    final Database db = await database;
    final List<Map<String, dynamic>> maps = await db.query(
      _tableName,
      orderBy: 'timestamp DESC',
      limit: limit,
    );

    final results = _mapListToLocationList(maps);

    // Sort by timestamp ascending (oldest first) for consistent ordering
    results.sort((a, b) => a.timestamp.compareTo(b.timestamp));
    return results;
  }

  /**
   * Get paginated locations for UI display
   * @param page - Page number (0-based)
   * @param pageSize - Number of items per page
   * @returns Future<List<LocationData>> - Page of locations
   */
  Future<List<LocationData>> getLocationsPaginated({
    int page = 0,
    int pageSize = 20,
  }) async {
    final Database db = await database;
    final offset = page * pageSize;

    final List<Map<String, dynamic>> maps = await db.query(
      _tableName,
      orderBy: 'timestamp DESC',
      limit: pageSize,
      offset: offset,
    );

    return _mapListToLocationList(maps);
  }

  /**
   * Get the most recent location
   * @returns Future<LocationData?> - Latest location or null if none exists
   */
  Future<LocationData?> getLastLocation() async {
    try {
      final db = await database;
      final List<Map<String, dynamic>> maps = await db.query(
        _tableName,
        orderBy: 'timestamp DESC',
        limit: 1,
      );

      if (maps.isNotEmpty) {
        return _mapToLocation(maps.first);
      }
      return null;
    } catch (e) {
      LogConfig.logError('Error getting last location from database', e);
      return null;
    }
  }

  /**
   * Get last location only if it's stale (older than threshold)
   * @param threshold - Duration to consider location stale (default: 30 minutes)
   * @returns Future<LocationData?> - Stale location or null if fresh/none exists
   */
  Future<LocationData?> getLastStaleLocation(
      {Duration threshold = const Duration(minutes: 30)}) async {
    try {
      final lastLocation = await getLastLocation();
      if (lastLocation == null) return null;

      final difference = DateTime.now().difference(lastLocation.timestamp);
      if (difference >= threshold) {
        return lastLocation;
      }
      return null;
    } catch (e) {
      LogConfig.logError('Error determining stale location', e);
      return null;
    }
  }

  /**
   * Mark multiple locations as synchronized
   * @param ids - List of location IDs to mark as synced
   * @returns Future<int> - Number of rows affected
   */
  Future<int> markLocationsAsSynced(List<int> ids) async {
    if (ids.isEmpty) return 0;

    final Database db = await database;
    return await db.update(
      _tableName,
      {'isSynced': 1},
      where: 'id IN (${List.filled(ids.length, '?').join(',')})',
      whereArgs: ids,
    );
  }

  /**
   * Delete specific locations by IDs
   * @param ids - List of location IDs to delete
   * @returns Future<int> - Number of rows deleted
   */
  Future<int> deleteLocations(List<int> ids) async {
    if (ids.isEmpty) return 0;

    final Database db = await database;
    return await db.delete(
      _tableName,
      where: 'id IN (${List.filled(ids.length, '?').join(',')})',
      whereArgs: ids,
    );
  }

  /**
   * Delete all locations from database
   * @returns Future<int> - Number of rows deleted
   */
  Future<int> deleteAllLocations() async {
    final Database db = await database;
    return await db.delete(_tableName);
  }

  /**
   * Get database file size in kilobytes
   * @returns Future<double> - Database size in KB
   */
  Future<double> getDatabaseSize() async {
    try {
      final Database db = await database;
      final databaseFile = File(db.path);

      if (await databaseFile.exists()) {
        final int bytes = await databaseFile.length();
        return bytes / 1024; // Convert to KB
      }
      return 0;
    } catch (e) {
      LogConfig.logError('Error getting database size', e);
      return 0;
    }
  }

  /**
   * Get total count of stored locations
   * @returns Future<int> - Total number of locations
   */
  Future<int> getLocationCount() async {
    try {
      final Database db = await database;
      final result =
          await db.rawQuery('SELECT COUNT(*) as count FROM $_tableName');
      return Sqflite.firstIntValue(result) ?? 0;
    } catch (e) {
      LogConfig.logError('Error getting location count', e);
      return 0;
    }
  }

  /**
   * Close database connection
   * Should be called when app is terminating
   */
  Future<void> close() async {
    if (_database != null) {
      await _database!.close();
      _database = null;
    }
  }

  // =================== PRIVATE HELPER METHODS ===================

  /**
   * Convert LocationData object to Map for database storage
   */
  Map<String, dynamic> _locationToMap(LocationData location) {
    return {
      'id': location.id,
      'latitude': location.latitude.toString(),
      'longitude': location.longitude.toString(),
      'timestamp': location.timestamp.toIso8601String(),
      'isSynced': location.isSynced ? 1 : 0,
      'errorMessage': location.errorMessage,
      'retryCount': location.retryCount,
      'locationFrom': location.locationFrom,
    };
  }

  /**
   * Convert database Map to LocationData object
   */
  LocationData _mapToLocation(Map<String, dynamic> map) {
    return LocationData(
      id: map['id'] as int,
      latitude: double.parse(map['latitude'] as String),
      longitude: double.parse(map['longitude'] as String),
      timestamp: DateTime.parse(map['timestamp'] as String),
      isSynced: map['isSynced'] == 1,
      errorMessage: map['errorMessage'] as String?,
      retryCount: map['retryCount'] as int,
      locationFrom: map['locationFrom'] as String? ?? 'online',
    );
  }

  /**
   * Convert list of database Maps to list of LocationData objects
   */
  List<LocationData> _mapListToLocationList(
      List<Map<String, dynamic>> mapList) {
    return mapList.map((map) => _mapToLocation(map)).toList();
  }
}
