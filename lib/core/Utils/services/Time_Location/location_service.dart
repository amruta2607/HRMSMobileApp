import 'package:geolocator/geolocator.dart';
import 'package:geocoding/geocoding.dart';

class LocationService {
  static String? _cachedLocation;
  static Position? _cachedPosition;

  static Future<bool> ensurePermissionGranted({bool requestIfDenied = true}) async {
    LocationPermission permission = await Geolocator.checkPermission();

    if (permission == LocationPermission.denied && requestIfDenied) {
      permission = await Geolocator.requestPermission();
    }

    if (permission == LocationPermission.denied ||
        permission == LocationPermission.deniedForever) {
      return false;
    }

    return true;
  }


  static Future<bool> isLocationServiceOn() async {
    return Geolocator.isLocationServiceEnabled();
  }

  static Future<bool> ensureLocationReadyOnAppStart() async {
    final permissionGranted = await ensurePermissionGranted();
    if (!permissionGranted) return false;

    final serviceOn = await isLocationServiceOn();
    if (!serviceOn) {
      await Geolocator.openLocationSettings();
      return false;
    }

    return true;
  }

  static Future<String> getLocation({
    bool forceRefresh = false,
    bool requestPermissionIfDenied = true,
  }) async {
    try {
      if (_cachedLocation != null && !forceRefresh) {
        return _cachedLocation!;
      }

      final permissionGranted = await ensurePermissionGranted(
        requestIfDenied: requestPermissionIfDenied,
      );
      if (!permissionGranted) {
        return 'Permission denied';
      }

      final serviceOn = await isLocationServiceOn();
      if (!serviceOn) {
        return 'Location is turned off';
      }

      final position = await _getPosition(forceRefresh: forceRefresh);

      final placemarks = await placemarkFromCoordinates(
        position.latitude,
        position.longitude,
      ).timeout(const Duration(seconds: 5));

      if (placemarks.isEmpty) {
        return 'Location unavailable';
      }

      final place = placemarks.first;
      final area = place.subLocality ?? place.thoroughfare ?? '';
      final city = place.locality ?? place.subAdministrativeArea ?? '';

      final location = area.isNotEmpty && city.isNotEmpty
          ? '$area, $city'
          : city.isNotEmpty
          ? city
          : 'Location found'; // fallback if names are empty but position was found

      _cachedLocation = location;
      return location;
    } catch (e) {
      print('Location error in getLocation: $e');
      if (_cachedLocation != null) return _cachedLocation!;
      return 'Location unavailable';
    }
  }


  static Future<Position> getLatLng({
    bool forceRefresh = false,
    bool requestPermissionIfDenied = true,
  }) async {
    final permissionGranted = await ensurePermissionGranted(
      requestIfDenied: requestPermissionIfDenied,
    );
    if (!permissionGranted) {
      throw Exception('Permission denied');
    }

    final serviceOn = await isLocationServiceOn();
    if (!serviceOn) {
      throw Exception('Location service off');
    }

    return _getPosition(forceRefresh: forceRefresh);
  }


  static Future<Position> _getPosition({bool forceRefresh = false}) async {
    if (_cachedPosition != null && !forceRefresh) {
      return _cachedPosition!;
    }

    try {
      // 1. Try last known position first for speed
      final lastKnown = await Geolocator.getLastKnownPosition();
      if (lastKnown != null && !forceRefresh) {
        _cachedPosition = lastKnown;
        return lastKnown;
      }

      // 2. Try current position with a timeout
      final position = await Geolocator.getCurrentPosition(
        desiredAccuracy: LocationAccuracy.high,
        timeLimit: const Duration(seconds: 5),
      );

      _cachedPosition = position;
      return position;
    } catch (e) {
      print('Location error in _getPosition: $e');
      // 3. Final fallback to last known if current failed
      final lastKnown = await Geolocator.getLastKnownPosition();
      if (lastKnown != null) {
        return lastKnown;
      }
      rethrow;
    }
  }


  static void clearCache() {
    _cachedLocation = null;
    _cachedPosition = null;
  }
}
