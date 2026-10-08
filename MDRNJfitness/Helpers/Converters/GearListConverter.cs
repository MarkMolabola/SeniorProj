using MDRNJ.Shared.Game;
using System.Globalization;

namespace MDRNJfitness.Helpers.Converters
{
    /// <summary>
    /// A character / profile DTO (anything with the five gear slots) → worn
    /// item ids in draw order, for PixelAvatarView.Gear.
    /// </summary>
    public class GearListConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is IEquippedGear gear ? gear.InDrawOrder() : Array.Empty<string>();

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>Gear slot → its column on the Hero tab (Head, Top, Bottom, Shoes, Accessory).</summary>
    public class GearSlotColumnConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is GearSlot slot ? GearCatalog.DisplayOrder.ToList().IndexOf(slot) : 0;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
