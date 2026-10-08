using CommunityToolkit.Mvvm.ComponentModel;
using MDRNJ.Shared.Game;

namespace MDRNJfitness.Models.Character
{
    /// <summary>One item in the Shop grid.</summary>
    public partial class ShopItemCard : ObservableObject
    {
        public ShopItemCard(GearItem item) => Item = item;

        public GearItem Item { get; }
        public string SlotName => Item.Slot.ToString().ToUpperInvariant();

        [ObservableProperty] private bool _owned;
        [ObservableProperty] private bool _canAfford;
    }

    /// <summary>One owned item in the Collection grid.</summary>
    public partial class CollectionCard : ObservableObject
    {
        public CollectionCard(GearItem item) => Item = item;

        public GearItem Item { get; }
        public string SlotName => Item.Slot.ToString().ToUpperInvariant();

        [ObservableProperty] private bool _equipped;
    }

    /// <summary>One of the five gear slots on the Hero tab.</summary>
    public partial class GearSlotCard : ObservableObject
    {
        public GearSlotCard(GearSlot slot) => Slot = slot;

        public GearSlot Slot { get; }
        public string Label => Slot.ToString().ToUpperInvariant();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ItemName), nameof(IsEmpty))]
        private string? _itemId;

        public string ItemName => GearCatalog.Find(ItemId)?.Name ?? "Empty";
        public bool IsEmpty => ItemId is null;
    }
}
