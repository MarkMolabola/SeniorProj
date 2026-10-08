namespace MDRNJ.Shared.Game
{
    /// <summary>The five gear slots a hero wears (item ids, null when empty).</summary>
    public interface IEquippedGear
    {
        string? HeadGear { get; set; }
        string? TopGear { get; set; }
        string? BottomGear { get; set; }
        string? ShoesGear { get; set; }
        string? AccessoryGear { get; set; }
    }

    public static class EquippedGear
    {
        public static string? Get(this IEquippedGear gear, GearSlot slot) => slot switch
        {
            GearSlot.Head => gear.HeadGear,
            GearSlot.Top => gear.TopGear,
            GearSlot.Bottom => gear.BottomGear,
            GearSlot.Shoes => gear.ShoesGear,
            _ => gear.AccessoryGear
        };

        public static void Set(this IEquippedGear gear, GearSlot slot, string? itemId)
        {
            switch (slot)
            {
                case GearSlot.Head: gear.HeadGear = itemId; break;
                case GearSlot.Top: gear.TopGear = itemId; break;
                case GearSlot.Bottom: gear.BottomGear = itemId; break;
                case GearSlot.Shoes: gear.ShoesGear = itemId; break;
                default: gear.AccessoryGear = itemId; break;
            }
        }

        /// <summary>Equipped item ids in draw order (bottom first, head last).</summary>
        public static IReadOnlyList<string> InDrawOrder(this IEquippedGear gear) =>
            Enum.GetValues<GearSlot>()
                .OrderBy(s => (int)s)
                .Select(gear.Get)
                .OfType<string>()
                .ToList();

        /// <summary>Copies all five slots.</summary>
        public static void CopyGearFrom(this IEquippedGear target, IEquippedGear source)
        {
            foreach (var slot in Enum.GetValues<GearSlot>())
                target.Set(slot, source.Get(slot));
        }
    }
}
