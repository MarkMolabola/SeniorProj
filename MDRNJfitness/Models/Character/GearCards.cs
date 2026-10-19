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

    /// <summary>One item in the Shop grid: a piece of gear or a special hero.</summary>
    public partial class ShopItemCard : ObservableObject
    {
        public ShopItemCard(GearItem item)
        {
            Gear = item;
            (Id, Name, Price, Rarity, BonusText) = (item.Id, item.Name, item.Price, item.Rarity, item.BonusText);
        }

        public ShopItemCard(HeroAvatar hero)
        {
            Hero = hero;
            (Id, Name, Price, Rarity, BonusText) = (hero.Id, hero.Name, hero.Price, hero.Rarity, hero.BonusText);
        }

        public GearItem? Gear { get; }
        public HeroAvatar? Hero { get; }
        public bool IsHero => Hero is not null;
        public bool IsGear => Gear is not null;

        public string Id { get; }
        public string Name { get; }
        public int Price { get; }
        public GearRarity Rarity { get; }
        public string BonusText { get; }

        public Brush RarityBrush => RarityColors.BrushFor(Rarity);
        public Color RarityColor => RarityColors.For(Rarity);
        public string RarityName => Rarity.ToString().ToUpperInvariant();

        [ObservableProperty] private bool _owned;
        [ObservableProperty] private bool _canAfford;
    }

    /// <summary>A titled section of the Shop (heroes, gear).</summary>
    public class ShopGroup : List<ShopItemCard>
    {
        public ShopGroup(string name, string hint, IEnumerable<ShopItemCard> items) : base(items)
        {
            Name = name;
            Hint = hint;
        }

        public string Name { get; }
        public string Hint { get; }
    }

    /// <summary>One owned item in the Collection grid.</summary>
    public partial class CollectionCard : ObservableObject
    {
        public CollectionCard(GearItem item) => Item = item;

        public GearItem Item { get; }
        public bool IsDungeonDrop => Item.DropOnly;
        public string SlotName => Item.Slot.ToString().ToUpperInvariant();
        public Brush RarityBrush => RarityColors.BrushFor(Item.Rarity);
        public Color RarityColor => RarityColors.For(Item.Rarity);
        public string RarityName => Item.Rarity.ToString().ToUpperInvariant();

        [ObservableProperty] private bool _equipped;
    }

    /// <summary>
    /// One of the five gear slots on the Hero tab: the worn gear item, or, while a
    /// special hero is selected, that hero's built-in piece for the slot.
    /// </summary>
    public partial class GearSlotCard : ObservableObject
    {
        public GearSlotCard(GearSlot slot) => Slot = slot;

        public GearSlot Slot { get; }
        public string Label => Slot.ToString().ToUpperInvariant();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ItemName), nameof(IsEmpty), nameof(RarityBrush))]
        private string? _itemId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ItemName), nameof(IsEmpty), nameof(RarityBrush), nameof(HeroId), nameof(Piece))]
        private HeroAvatar? _hero;

        /// <summary>The special hero whose piece fills this slot (for the icon), if any.</summary>
        public string? HeroId => Piece is null ? null : Hero?.Id;
        public HeroPiece? Piece => Hero?.PieceFor(Slot);

        public string ItemName => Hero is not null ? Piece?.Name ?? "—" : GearCatalog.Find(ItemId)?.Name ?? "Empty";

        /// <summary>Shows the "+" (add gear) marker: only for an empty slot on the Classic Hero.</summary>
        public bool IsEmpty => Hero is null && ItemId is null;

        /// <summary>The worn item's (or hero's) rarity colour; the plain slot outline when empty.</summary>
        public Brush RarityBrush => Hero is not null
            ? Piece is null ? new SolidColorBrush(Color.FromArgb("#0B0918")) : RarityColors.BrushFor(Hero.Rarity)
            : GearCatalog.Find(ItemId) is { } item
                ? RarityColors.BrushFor(item.Rarity)
                : new SolidColorBrush(Color.FromArgb("#0B0918"));
    }
}
