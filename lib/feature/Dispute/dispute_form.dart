import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../../core/Theme/app_colors.dart';
import '../../core/Utils/services/dispute_service/dispute_service.dart';
import 'dispute_category.dart';
import 'package:intl/intl.dart';

/// Which punch time inputs to show for a given category
enum _PunchMode { none, punchInOnly, punchOutOnly, both }

_PunchMode _getPunchMode(String categoryName) {
  final lower = categoryName.toLowerCase();
  if (lower.contains('attendance not marked')) return _PunchMode.both;
  if (lower.contains('missing check-out') || lower.contains('wrong check-out')) return _PunchMode.punchOutOnly;
  if (lower.contains('wrong check-in')) return _PunchMode.punchInOnly;
  return _PunchMode.none;
}

class DisputeForm extends StatefulWidget {
  final String defaultDate;
  final double scale;
  final int? punchId;

  const DisputeForm({
    super.key,
    required this.defaultDate,
    required this.scale,
    this.punchId,
  });

  @override
  State<DisputeForm> createState() => _DisputeFormState();
}

class _DisputeFormState extends State<DisputeForm> {
  late TextEditingController _descController;
  bool _isLoading = false;

  List<DisputeCategory> _categories = [];
  DisputeCategory? _selectedCategory;
  bool _isLoadingCategories = true;

  // Punch In controllers
  final TextEditingController _punchInHourCtrl = TextEditingController();
  final TextEditingController _punchInMinCtrl = TextEditingController();

  // Punch Out controllers
  final TextEditingController _punchOutHourCtrl = TextEditingController();
  final TextEditingController _punchOutMinCtrl = TextEditingController();

  @override
  void initState() {
    super.initState();
    _descController = TextEditingController();
    _loadCategories();
  }

  Future<void> _loadCategories() async {
    final list = await DisputeService.fetchCategories();
    if (!mounted) return;
    setState(() {
      _categories = list;
      if (list.isNotEmpty) {
        _selectedCategory = list.first;
      }
      _isLoadingCategories = false;
    });
  }

  @override
  void dispose() {
    _descController.dispose();
    _punchInHourCtrl.dispose();
    _punchInMinCtrl.dispose();
    _punchOutHourCtrl.dispose();
    _punchOutMinCtrl.dispose();
    super.dispose();
  }

  DateTime? _buildDateTime(DateTime base, TextEditingController hourCtrl, TextEditingController minCtrl) {
    final h = int.tryParse(hourCtrl.text.trim());
    final m = int.tryParse(minCtrl.text.trim());
    if (h == null || m == null) return null;
    if (h < 0 || h > 23 || m < 0 || m > 59) return null;
    return DateTime(base.year, base.month, base.day, h, m);
  }

  Future<void> _submit() async {
    final desc = _descController.text.trim();
    if (desc.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please enter a description')),
      );
      return;
    }

    if (_selectedCategory == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please select a category')),
      );
      return;
    }

    final mode = _getPunchMode(_selectedCategory!.categoryName);

    late DateTime baseDate;
    try {
      baseDate = DateFormat("dd/MM/yyyy").parse(widget.defaultDate);
    } catch (_) {
      baseDate = DateTime.now();
    }

    DateTime? punchIn;
    DateTime? punchOut;

    if (mode == _PunchMode.both || mode == _PunchMode.punchInOnly) {
      punchIn = _buildDateTime(baseDate, _punchInHourCtrl, _punchInMinCtrl);
      if (punchIn == null) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Please enter a valid Punch In time (Hour 0-23, Minute 0-59)')),
        );
        return;
      }
    }

    if (mode == _PunchMode.both || mode == _PunchMode.punchOutOnly) {
      punchOut = _buildDateTime(baseDate, _punchOutHourCtrl, _punchOutMinCtrl);
      if (punchOut == null) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Please enter a valid Punch Out time (Hour 0-23, Minute 0-59)')),
        );
        return;
      }
    }

    setState(() => _isLoading = true);

    try {
      await DisputeService.createDispute(
        disputeDate: baseDate,
        description: desc,
        categoryId: _selectedCategory!.id,
        punchId: widget.punchId,
        requestedPunchInTime: punchIn,
        requestedPunchOutTime: punchOut,
      );

      if (!mounted) return;
      setState(() => _isLoading = false);
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Dispute submitted successfully')),
      );
      Navigator.pop(context);
    } catch (e) {
      if (!mounted) return;
      setState(() => _isLoading = false);
      final msg = e.toString();
      final isOnePerDay = msg.contains('one dispute') || msg.contains('already exists');
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(isOnePerDay
              ? 'Only one dispute can be submitted per day'
              : msg.replaceFirst(RegExp(r'^Exception:\s*'), '')),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final scale = widget.scale;
    final mode = _selectedCategory != null ? _getPunchMode(_selectedCategory!.categoryName) : _PunchMode.none;

    return Container(
      width: double.infinity,
      padding: EdgeInsets.all(16 * scale),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(17 * scale),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.06),
            blurRadius: 14 * scale,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          /// Dispute Category
          _label('Dispute Category'),
          SizedBox(height: 6 * scale),
          _dropdown(scale),

          SizedBox(height: 14 * scale),

          /// Date
          _label('Date'),
          SizedBox(height: 6 * scale),
          _dateField(widget.defaultDate, scale),

          SizedBox(height: 14 * scale),

          /// Punch In time fields
          if (mode == _PunchMode.both || mode == _PunchMode.punchInOnly) ...[
            _timeRow(
              scale: scale,
              hourCtrl: _punchInHourCtrl,
              minCtrl: _punchInMinCtrl,
            ),
            SizedBox(height: 14 * scale),
          ],

          /// Punch Out time fields
          if (mode == _PunchMode.both || mode == _PunchMode.punchOutOnly) ...[
            _timeRow(
              scale: scale,
              hourCtrl: _punchOutHourCtrl,
              minCtrl: _punchOutMinCtrl,
            ),
            SizedBox(height: 14 * scale),
          ],

          /// Description
          _label('Description'),
          SizedBox(height: 6 * scale),
          _description(scale),

          SizedBox(height: 18 * scale),

          /// Submit Button
          Align(
            alignment: Alignment.centerRight,
            child: SizedBox(
              width: 128 * scale,
              height: 27 * scale,
              child: ElevatedButton(
                onPressed: _isLoading ? null : _submit,
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF0F62FE),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(4 * scale),
                  ),
                  padding: EdgeInsets.zero,
                ),
                child: _isLoading
                    ? SizedBox(
                  width: 14 * scale,
                  height: 14 * scale,
                  child: const CircularProgressIndicator(
                    strokeWidth: 2,
                    color: Colors.white,
                  ),
                )
                    : Text(
                  'Submit',
                  style: TextStyle(
                    fontSize: 17 * scale,
                    fontWeight: FontWeight.w600,
                    color: Colors.white,
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  /// ---------- Widgets ----------

  Widget _label(String text) {
    return Text(
      '$text :',
      style: const TextStyle(
        fontWeight: FontWeight.w600,
        color: AppColors.textDark,
      ),
    );
  }

  Widget _dropdown(double scale) {
    return Container(
      height: 36 * scale,
      padding: EdgeInsets.symmetric(horizontal: 10 * scale),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(2 * scale),
        border: Border.all(
          color: const Color(0xFF5D6063),
          width: 0.4,
        ),
      ),
      child: _isLoadingCategories
          ? const Center(child: SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2)))
          : DropdownButtonHideUnderline(
        child: DropdownButton<DisputeCategory>(
          value: _selectedCategory,
          isExpanded: true,
          icon: const Icon(Icons.arrow_drop_down),
          items: _categories.map((cat) {
            return DropdownMenuItem<DisputeCategory>(
              value: cat,
              child: Text(cat.categoryName),
            );
          }).toList(),
          onChanged: (val) {
            if (val != null) {
              setState(() {
                _selectedCategory = val;
                // Reset punch time fields on category change
                _punchInHourCtrl.clear();
                _punchInMinCtrl.clear();
                _punchOutHourCtrl.clear();
                _punchOutMinCtrl.clear();
              });
            }
          },
        ),
      ),
    );
  }

  Widget _dateField(String date, double scale) {
    return Container(
      height: 36 * scale,
      padding: EdgeInsets.symmetric(horizontal: 10 * scale),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(2 * scale),
        border: Border.all(
          color: const Color(0xFF5D6063),
          width: 0.4,
        ),
      ),
      child: Row(
        children: [
          Expanded(
            child: Text(
              date,
              style: const TextStyle(color: AppColors.textDark),
            ),
          ),
          const Icon(Icons.calendar_month, size: 18),
        ],
      ),
    );
  }

  /// Hour + Minute row (no section title — as requested)
  Widget _timeRow({
    required double scale,
    required TextEditingController hourCtrl,
    required TextEditingController minCtrl,
  }) {
    return Row(
      children: [
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Hour (0 - 23)',
                style: TextStyle(
                  fontSize: 12 * scale,
                  color: AppColors.textDark,
                  fontWeight: FontWeight.w500,
                ),
              ),
              SizedBox(height: 4 * scale),
              _numberInput(ctrl: hourCtrl, hint: '0', scale: scale),
            ],
          ),
        ),
        SizedBox(width: 12 * scale),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Minute (0 - 59)',
                style: TextStyle(
                  fontSize: 12 * scale,
                  color: AppColors.textDark,
                  fontWeight: FontWeight.w500,
                ),
              ),
              SizedBox(height: 4 * scale),
              _numberInput(ctrl: minCtrl, hint: '0', scale: scale),
            ],
          ),
        ),
      ],
    );
  }

  Widget _numberInput({
    required TextEditingController ctrl,
    required String hint,
    required double scale,
  }) {
    return Container(
      height: 36 * scale,
      padding: EdgeInsets.symmetric(horizontal: 10 * scale),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(4 * scale),
        border: Border.all(color: const Color(0xFF5D6063), width: 0.4),
      ),
      child: TextField(
        controller: ctrl,
        keyboardType: TextInputType.number,
        inputFormatters: [FilteringTextInputFormatter.digitsOnly],
        decoration: InputDecoration(
          isCollapsed: true,
          border: InputBorder.none,
          hintText: hint,
          contentPadding: EdgeInsets.symmetric(vertical: 9 * scale),
        ),
        style: TextStyle(fontSize: 14 * scale, color: AppColors.textDark),
      ),
    );
  }

  Widget _description(double scale) {
    return Container(
      height: 72 * scale,
      padding: EdgeInsets.all(8 * scale),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(4 * scale),
        border: Border.all(
          color: const Color(0xFF5D6063),
          width: 0.4,
        ),
      ),
      child: TextField(
        controller: _descController,
        maxLines: null,
        decoration: const InputDecoration(
          isCollapsed: true,
          border: InputBorder.none,
          hintText: 'Enter reason...',
        ),
      ),
    );
  }
}

