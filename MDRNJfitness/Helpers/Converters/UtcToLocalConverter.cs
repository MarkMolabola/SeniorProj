using System.Globalization;

namespace MDRNJfitness.Helpers.Converters
{
    /// <summary>
    /// Converts a UTC DateTime to the device's local time for display in XAML bindings.
    /// </summary>
    public class UtcToLocalConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
                return dt.Kind == DateTimeKind.Utc ? dt.ToLocalTime() : dt;
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
