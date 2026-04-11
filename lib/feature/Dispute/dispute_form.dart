import 'package:flutter/material.dart';
import '../../core/Theme/app_colors.dart';
import '../../core/Utils/services/dispute_service/dispute_service.dart';
import 'dispute_category.dart';
import 'package:intl/intl.dart';

class DisputeForm extends StatefulWidget {
  final String defaultDate;
  final double scale;

  const DisputeForm({
    super.key,
    required this.defaultDate,
    required this.scale,
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
    super.dispose();
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

    setState(() => _isLoading = true);

    late DateTime date;
    try {
      date = DateFormat("dd/MM/yyyy").parse(widget.defaultDate);
    } catch (_) {
      date = DateTime.now();
    }

    try {
      await DisputeService.createDispute(
        disputeDate: date,
        description: desc,
        categoryId: _selectedCategory!.id,
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
          content: Text(isOnePerDay ? 'Only one dispute can be submitted per day' : msg.replaceFirst(RegExp(r'^Exception:\s*'), '')),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final scale = widget.scale;

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
                  'Submit Dispute',
                  style: TextStyle(
                    fontSize: 13 * scale,
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
            if (val != null) setState(() => _selectedCategory = val);
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
