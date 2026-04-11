import 'package:flutter/material.dart';
import '../model/profile_model.dart';
import '../../../core/Utils/services/profile_service/profile_service.dart';

class ProfileController extends ChangeNotifier {
  ProfileModel? profile;
  bool isLoading = false;
  bool _fetched = false;

  String get profileImageUrl {
    final pic = profile?.picture;
    print(' PROFILE CONTROLLER: picture from model = $pic');

    if (pic == null || pic.isEmpty) {
      print(' PROFILE CONTROLLER: No picture in profile model');
      return '';
    }

    if (pic.startsWith('http://') || pic.startsWith('https://')) {
      print(' PROFILE CONTROLLER: Full URL detected = $pic');
      return pic;
    }

    final fullUrl = 'http://103.123.74.159:5005/upload/$pic';
    print(' PROFILE CONTROLLER: Built URL = $fullUrl');
    return fullUrl;
  }


  Future<void> fetchProfileOnce() async {
    if (_fetched && profile != null) return;

    print(' PROFILE CONTROLLER: Starting profile fetch...');
    isLoading = true;
    notifyListeners();

    final result = await ProfileService.fetchProfile();
    if (result != null) {
      profile = result;
      _fetched = true;
    }

    print(' PROFILE CONTROLLER: Profile fetch complete');
    print(' PROFILE CONTROLLER: Name = ${profile?.name}');
    print(' PROFILE CONTROLLER: Picture = ${profile?.picture}');

    isLoading = false;
    notifyListeners();
  }

  Future<void> refreshProfile() async {
    _fetched = false;
    await fetchProfileOnce();
  }

  void clearData() {
    profile = null;
    _fetched = false;
    isLoading = false;
    notifyListeners();
    print(' PROFILE CONTROLLER: Data cleared');
  }
}
