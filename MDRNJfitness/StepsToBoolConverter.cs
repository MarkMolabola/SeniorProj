using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FitnessApp;

public class StepsToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int steps)
        {
            return steps > 0; // Show only if steps > 0
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
