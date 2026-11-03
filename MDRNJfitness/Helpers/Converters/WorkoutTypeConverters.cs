using MDRNJ.Shared.Game;
using System.Globalization;

namespace MDRNJfitness.Helpers.Converters
{
    /// <summary>API workout type name → display name ("JumpRope" → "Jump Rope").</summary>
    public class WorkoutTypeNameConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            WorkoutTypes.DisplayName(value as string);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Workout type or category name → its category's accent color.</summary>
    public class WorkoutCategoryColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            WorkoutCategoryStyle.Color(value as string);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Category colors match the stat each one mostly trains.</summary>
    public static class WorkoutCategoryStyle
    {
        public static string ColorKey(string? categoryName) => categoryName switch
        {
            WorkoutTypes.Cardio => "PxTeal",
            WorkoutTypes.Flexibility => "PxFlx",
            WorkoutTypes.Strength => "PxStr",
            WorkoutTypes.Sports => "PxOrange",
            _ => "PxTextMuted"
        };

        /// <summary>Accepts a category name or a workout type name.</summary>
        public static Color Color(string? name)
        {
            var category = WorkoutTypes.Categories.Any(c => c.Name == name)
                ? name
                : WorkoutTypes.CategoryOf(name)?.Name;
            return Application.Current?.Resources.TryGetValue(ColorKey(category), out var value) == true
                && value is Color color ? color : Colors.White;
        }
    }
}
