using CommunityToolkit.Mvvm.ComponentModel;
using MDRNJ.Shared.Game;

namespace MDRNJfitness.Models.Character
{
    /// <summary>Gear rarity outline colours: grey, blue, purple, gold.</summary>
    public static class RarityColors
    {
        public static Color For(GearRarity rarity) => rarity switch
        {
            GearRarity.Common => Color.FromArgb("#9A96B0"),
            GearRarity.Rare => Color.FromArgb("#4A9BFF"),
            GearRarity.Epic => Color.FromArgb("#B266FF"),
            _ => Color.FromArgb("#FFC94A")
        };

        public static Brush BrushFor(GearRarity rarity) => new SolidColorBrush(For(rarity));
    }

    /// <summary>One item in the Shop grid.</summary>
    public partial class ShopItemCard : ObservableObject
    {
        public ShopItemCard(GearItem item) => Item = item;

        public GearItem Item { get; }
        public string SlotName => Item.Slot.ToString().ToUpperInvariant();
        public Brush RarityBrush => RarityColors.BrushFor(Item.Rarity);
        public Color RarityColor => RarityColors.For(Item.Rarity);
        public string RarityName => Item.Rarity.ToString().ToUpperInvariant();

        [ObservableProperty] private bool _owned;
        [ObservableProperty] private bool _canAfford;
    }

    /// <summary>One owned item in the Collection grid.</summary>
    public partial class CollectionCard : ObservableObject
    {
        public CollectionCard(GearItem item) => Item = item;

        public GearItem Item { get; }
        public string SlotName => Item.Slot.ToString().ToUpperInvariant();
        public Brush RarityBrush => RarityColors.BrushFor(Item.Rarity);
        public Color RarityColor => RarityColors.For(Item.Rarity);
        public string RarityName => Item.Rarity.ToString().ToUpperInvariant();

        [ObservableProperty] private bool _equipped;
    }

    /// <summary>One of the five gear slots on the Hero tab.</summary>
    public partial class GearSlotCard : ObservableObject
    {
        public GearSlotCard(GearSlot slot) => Slot = slot;

        public GearSlot Slot { get; }
        public string Label => Slot.ToString().ToUpperInvariant();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ItemName), nameof(IsEmpty), nameof(RarityBrush))]
        private string? _itemId;

        public string ItemName => GearCatalog.Find(ItemId)?.Name ?? "Empty";
        public bool IsEmpty => ItemId is null;

        /// <summary>The worn item's rarity colour; the plain slot outline when empty.</summary>
        public Brush RarityBrush => GearCatalog.Find(ItemId) is { } item
            ? RarityColors.BrushFor(item.Rarity)
            : new SolidColorBrush(Color.FromArgb("#0B0918"));
    }
}
