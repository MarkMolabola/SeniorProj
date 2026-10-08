using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using System.Globalization;

namespace MDRNJfitness.Helpers.Converters
{
    /// <summary>
    /// Converts a 0–100 score (e.g. ActivityScore.StrengthScore) to a 0–1 progress value.
    /// </summary>
    public class PercentToProgressConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var percent = value is null ? 0 : System.Convert.ToDouble(value, culture);
            return Math.Clamp(percent / 100d, 0, 1);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Stat points → stat level (1, 2, 3…).</summary>
    public class StatLevelConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => StatProgression.LevelFor(value is null ? 0 : System.Convert.ToSingle(value, culture));

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Stat points → 0–1 progress through the current stat level.</summary>
    public class StatLevelProgressConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => StatProgression.ProgressInLevel(value is null ? 0 : System.Convert.ToSingle(value, culture));

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>CharacterStateDto.BodyType (int) → its name, e.g. "Athletic".</summary>
    public class BodyTypeNameConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is int i && Enum.IsDefined(typeof(BodyType), i) ? ((BodyType)i).ToString() : string.Empty;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// MultiBinding converter: [current, target] → 0–1 progress (e.g. quest Progress / TargetValue).
    /// </summary>
    public class RatioToProgressConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] is null || values[1] is null)
                return 0d;

            var current = System.Convert.ToDouble(values[0], culture);
            var target = System.Convert.ToDouble(values[1], culture);
            return target <= 0 ? 0d : Math.Clamp(current / target, 0, 1);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
