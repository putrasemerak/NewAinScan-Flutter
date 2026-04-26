import 'package:flutter/material.dart';

/// Configuration for a single column in [DataListView].
class DataColumnConfig {
  /// The display name shown in the column header.
  final String name;

  /// Optional display label for the header (defaults to [name]).
  final String? label;

  /// The flex factor controlling how much horizontal space this column
  /// occupies relative to other columns. Defaults to 1.
  /// Used when [DataListView.scrollable] is false.
  final int flex;

  /// Fixed width in pixels. Used when [DataListView.scrollable] is true.
  final double width;

  const DataColumnConfig({
    required this.name,
    this.label,
    this.flex = 1,
    this.width = 100,
  });
}

/// A reusable data table/list widget for displaying inventory data.
///
/// Replaces the VB.NET ListView control used across many screens to display
/// scan results, inventory lists, and other tabular data. Supports
/// configurable columns, row tap handling, and alternating row colors
/// for readability.
///
/// When [scrollable] is true, columns use fixed widths and the entire
/// table (header + rows) scrolls horizontally together.
class DataListView extends StatefulWidget {
  /// Column definitions specifying the header name and flex width.
  final List<DataColumnConfig> columns;

  /// Row data where each row is a map of column name to cell value.
  final List<Map<String, String>> rows;

  /// Optional callback invoked when a row is tapped, receiving the row
  /// index and the row data.
  final Function(int index, Map<String, String> row)? onRowTap;

  /// When true, columns use fixed [DataColumnConfig.width] and the table
  /// scrolls horizontally. When false, columns use flex-based layout.
  final bool scrollable;

  /// Optional text style for column header text (e.g. condensed font).
  final TextStyle? headerTextStyle;

  /// Optional padding for the header row. Defaults to
  /// `EdgeInsets.symmetric(horizontal: 8, vertical: 10)`.
  final EdgeInsetsGeometry? headerPadding;

  /// Optional scroll controller for the vertical list of rows.
  /// Use this to programmatically scroll (e.g. auto-scroll on new item).
  final ScrollController? scrollController;

  const DataListView({
    super.key,
    required this.columns,
    required this.rows,
    this.onRowTap,
    this.scrollable = false,
    this.headerTextStyle,
    this.headerPadding,
    this.scrollController,
  });

  @override
  State<DataListView> createState() => _DataListViewState();
}

class _DataListViewState extends State<DataListView> {
  final ScrollController _horizontalController = ScrollController();
  int _selectedIndex = -1;

  @override
  void didUpdateWidget(covariant DataListView oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.rows != oldWidget.rows) {
      _selectedIndex = -1;
    }
  }

  @override
  void dispose() {
    _horizontalController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    if (widget.columns.isEmpty) {
      return const SizedBox.shrink();
    }

    if (widget.scrollable) {
      return _buildScrollable(theme);
    }
    return _buildFixed(theme);
  }

  // ── Scrollable mode: fixed-width columns, horizontal scroll ──────────────

  Widget _buildScrollable(ThemeData theme) {
    final totalWidth =
        widget.columns.fold<double>(0, (sum, c) => sum + c.width) + 16;

    Widget buildRowCells(Map<String, String>? row) {
      return Row(
        children: widget.columns.map((col) {
          return SizedBox(
            width: col.width,
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 4),
              child: Text(
                row != null ? (row[col.name] ?? '') : (col.label ?? col.name),
                style: row != null
                    ? TextStyle(fontSize: 13, color: theme.colorScheme.onSurface)
                    : (widget.headerTextStyle ?? const TextStyle(fontSize: 13)).copyWith(
                        fontWeight: FontWeight.bold,
                        color: theme.colorScheme.onPrimary,
                      ),
                overflow: TextOverflow.ellipsis,
              ),
            ),
          );
        }).toList(),
      );
    }

    return Column(
      children: [
        // Horizontally scrollable header + data (scroll together)
        Expanded(
          child: Scrollbar(
            controller: _horizontalController,
            thumbVisibility: true,
            interactive: true,
            notificationPredicate: (notification) =>
                notification.metrics.axis == Axis.horizontal,
            child: SingleChildScrollView(
              controller: _horizontalController,
              scrollDirection: Axis.horizontal,
              child: SizedBox(
                width: totalWidth,
                child: Column(
                  children: [
                    // Header
                    Container(
                      color: theme.colorScheme.primary,
                      padding: widget.headerPadding ??
                          const EdgeInsets.symmetric(
                              horizontal: 8, vertical: 10),
                      child: buildRowCells(null),
                    ),
                    // Data rows (vertical scroll inside)
                    Expanded(
                      child: widget.rows.isEmpty
                          ? Center(
                              child: Text(
                                'No data',
                                style: TextStyle(
                                    color: theme.colorScheme.onSurface.withValues(alpha: 0.5),
                                    fontSize: 14),
                              ),
                            )
                          : ListView.builder(
                              controller: widget.scrollController,
                              itemCount: widget.rows.length,
                              itemBuilder: (context, index) {
                                final row = widget.rows[index];
                                final isSelected = index == _selectedIndex;
                                final isDark = theme.brightness == Brightness.dark;
                                final bg = isSelected
                                    ? theme.colorScheme.primaryContainer
                                    : index.isEven
                                        ? (isDark ? const Color(0xFF1E2228) : Colors.white)
                                        : (isDark ? const Color(0xFF262B33) : Colors.grey.shade100);

                                return InkWell(
                                  onTap: () {
                                    setState(() {
                                      _selectedIndex = index;
                                    });
                                    if (widget.onRowTap != null) {
                                      widget.onRowTap!(index, row);
                                    }
                                  },
                                  child: Container(
                                    color: bg,
                                    padding: const EdgeInsets.symmetric(
                                        horizontal: 8, vertical: 10),
                                    child: buildRowCells(row),
                                  ),
                                );
                              },
                            ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
        // Row count footer (stays full-width, not scrolled)
        Container(
          color: theme.brightness == Brightness.dark
              ? const Color(0xFF262B33)
              : Colors.grey.shade200,
          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 6),
          width: double.infinity,
          child: Text(
            '${widget.rows.length} row${widget.rows.length == 1 ? '' : 's'}',
            style: TextStyle(
              fontSize: 12,
              color: theme.colorScheme.onSurface.withValues(alpha: 0.6),
            ),
            textAlign: TextAlign.right,
          ),
        ),
      ],
    );
  }

  // ── Fixed mode: flex-based columns, no horizontal scroll ─────────────────

  Widget _buildFixed(ThemeData theme) {
    return Column(
      children: [
        // Header row
        Container(
          color: theme.colorScheme.primary,
          padding: widget.headerPadding ??
              const EdgeInsets.symmetric(horizontal: 8, vertical: 10),
          child: Row(
            children: widget.columns.map((col) {
              return Expanded(
                flex: col.flex,
                child: Text(
                  col.label ?? col.name,
                  style: (widget.headerTextStyle ?? const TextStyle(fontSize: 13)).copyWith(
                    fontWeight: FontWeight.bold,
                    color: theme.colorScheme.onPrimary,
                  ),
                  overflow: TextOverflow.ellipsis,
                ),
              );
            }).toList(),
          ),
        ),
        // Data rows
        Expanded(
          child: widget.rows.isEmpty
              ? Center(
                  child: Text(
                    'No data',
                    style: TextStyle(
                        color: theme.colorScheme.onSurface.withValues(alpha: 0.5),
                        fontSize: 14),
                  ),
                )
              : ListView.builder(
                  controller: widget.scrollController,
                  itemCount: widget.rows.length,
                  itemBuilder: (context, index) {
                    final row = widget.rows[index];
                    final isSelected = index == _selectedIndex;
                    final isDark = theme.brightness == Brightness.dark;
                    final bg = isSelected
                        ? theme.colorScheme.primaryContainer
                        : index.isEven
                            ? (isDark ? const Color(0xFF1E2228) : Colors.white)
                            : (isDark ? const Color(0xFF262B33) : Colors.grey.shade100);

                    return InkWell(
                      onTap: () {
                        setState(() {
                          _selectedIndex = index;
                        });
                        if (widget.onRowTap != null) {
                          widget.onRowTap!(index, row);
                        }
                      },
                      child: Container(
                        color: bg,
                        padding: const EdgeInsets.symmetric(
                            horizontal: 8, vertical: 10),
                        child: Row(
                          children: widget.columns.map((col) {
                            return Expanded(
                              flex: col.flex,
                              child: Text(
                                row[col.name] ?? '',
                                style: TextStyle(fontSize: 13, color: theme.colorScheme.onSurface),
                                overflow: TextOverflow.ellipsis,
                              ),
                            );
                          }).toList(),
                        ),
                      ),
                    );
                  },
                ),
        ),
        // Row count footer
        Container(
          color: theme.brightness == Brightness.dark
              ? const Color(0xFF262B33)
              : Colors.grey.shade200,
          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 6),
          width: double.infinity,
          child: Text(
            '${widget.rows.length} row${widget.rows.length == 1 ? '' : 's'}',
            style: TextStyle(
              fontSize: 12,
              color: theme.colorScheme.onSurface.withValues(alpha: 0.6),
            ),
            textAlign: TextAlign.right,
          ),
        ),
      ],
    );
  }
}
