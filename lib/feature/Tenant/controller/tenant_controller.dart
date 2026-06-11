import 'package:flutter/material.dart';
import '../../../core/Utils/services/tenant_service/tenant_service.dart';

class TenantController extends ChangeNotifier {
  String? _companyLogoUrl;
  bool _isLoading = false;

  String? get companyLogoUrl => _companyLogoUrl;
  bool get isLoading => _isLoading;

  Future<void> fetchCompanyLogo() async {
    if (_companyLogoUrl != null) return; // Only fetch once

    _isLoading = true;
    notifyListeners();

    try {
      _companyLogoUrl = await TenantService.getCompanyLogo();
    } catch (e) {
      print('🔴 TENANT CONTROLLER ERROR => $e');
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  void clearData() {
    _companyLogoUrl = null;
    _isLoading = false;
    notifyListeners();
  }
}
