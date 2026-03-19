using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FitnessApp;

public class HeightMultiplierConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double normalized && double.TryParse(parameter?.ToString(), out double maxHeight))
        {
            return normalized * maxHeight;
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
