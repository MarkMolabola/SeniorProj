using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Game;
using MDRNJfitness.Models.Character;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    /// <summary>Buy special heroes and gear with gold.</summary>
    public partial class ShopViewModel : BaseViewModel
    {
        private readonly IGearService _gear;
        private readonly ICharacterService _character;

        public List<ShopGroup> Groups { get; } = new()
        {
            new ShopGroup("HEROES", "Come dressed in their own outfit and stat boosts.",
                HeroAvatars.All.Where(h => h.IsSpecial).OrderBy(h => h.Price).Select(h => new ShopItemCard(h))),
            new ShopGroup("GEAR", "Worn by the Classic Hero.",
                GearCatalog.ForSale.OrderBy(i => GearCatalog.DisplayOrder.ToList().IndexOf(i.Slot)).ThenBy(i => i.Price)
                    .Select(i => new ShopItemCard(i))),
        };

        private IEnumerable<ShopItemCard> Items => Groups.SelectMany(g => g);

        [ObservableProperty] private int _gold;
        [ObservableProperty] private string _goldText = "0";
        private bool _unlimited;
        public bool HasNothingForSale => !Items.Any();

        public ShopViewModel(IGearService gear, ICharacterService character)
        {
            _gear = gear;
            _character = character;
            Title = "Shop";
        }

        [RelayCommand]
        public async Task LoadAsync() =>
            await RunSafeAsync(async () => Show(await _gear.GetInventoryAsync()));

        private void Show(InventoryDto inventory)
        {
            Gold = inventory.Gold;
            _unlimited = inventory.UnlimitedGold;
            GoldText = _unlimited ? "∞" : Gold.ToString();
            foreach (var card in Items)
            {
                card.Owned = inventory.OwnedItemIds.Contains(card.Id);
                card.CanAfford = !card.Owned && (_unlimited || Gold >= card.Price);
            }
        }

        [RelayCommand]
        public async Task BuyAsync(ShopItemCard card)
        {
            if (card.Owned) return;
            if (!card.CanAfford)
            {
                await Shell.Current.DisplayAlertAsync("Not enough gold",
                    $"The {card.Name} costs {card.Price} gold. You have {Gold}. Train, finish quests and win duels to earn more!", "OK");
                return;
            }

            var what = card.IsHero
                ? $"the {card.Name} ({card.BonusText} from their outfit)"
                : $"the {card.Name}";
            var sure = await Shell.Current.DisplayAlertAsync(card.IsHero ? "Unlock hero?" : "Buy item?",
                _unlimited ? $"Unlock {what}? (Free for admins)"
                           : $"Buy {what} for {card.Price} gold?", "Buy", "Cancel");
            if (!sure) return;

            await RunSafeAsync(async () =>
            {
                Show(await _gear.BuyAsync(card.Id));
                if (card.Hero is not null)
                {
                    if (await Shell.Current.DisplayAlertAsync("Unlocked!",
                            $"The {card.Name} joined your roster. Play as them now?", "Play", "Later"))
                        await _character.SetAvatarAsync(card.Id);
                }
                else if (card.Gear is { } item && await Shell.Current.DisplayAlertAsync("Purchased!",
                             $"The {item.Name} is in your Collection. Wear it now?", "Equip", "Later"))
                {
                    await _gear.EquipAsync(item.Slot, item.Id);
                }
            });
        }
    }
}
