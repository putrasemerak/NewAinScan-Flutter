/// Safely converts a dynamic value to [double].
///
/// Handles null, num, and String representations.
double toDouble(dynamic value) {
  if (value == null) return 0;
  if (value is num) return value.toDouble();
  return double.tryParse(value.toString()) ?? 0;
}
