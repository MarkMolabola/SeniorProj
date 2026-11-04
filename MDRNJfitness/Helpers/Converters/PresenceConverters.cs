using MDRNJ.Shared.DTOs.Friend;
using MDRNJ.Shared.Game;
using System.Globalization;

namespace MDRNJfitness.Helpers.Converters
{
    /// <summary>Presence name → status light: green working out, yellow online, red offline.</summary>
    public class PresenceColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var key = (value as string) switch
            {
                "WorkingOut" => "PxGreen",
                "Online" => "PxGold",
                _ => "PxStr"
            };
            var color = Application.Current?.Resources.TryGetValue(key, out var resource) == true && resource is Color c
                ? c : Colors.Gray;
            return typeof(Brush).IsAssignableFrom(targetType) ? new SolidColorBrush(color) : color;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>A friend → "Working out · Running" / "Online" / "Offline".</summary>
    public class PresenceTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is not FriendDto friend ? null
            : friend.Presence switch
            {
                "WorkingOut" => friend.PresenceWorkoutType is { } type
                    ? $"Working out · {WorkoutTypes.DisplayName(type)}"
                    : "Working out",
                "Online" => "Online",
                _ => "Offline"
            };

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
