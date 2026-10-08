using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Game;
using MDRNJfitness.Models.Character;
using MDRNJfitness.Services.Interfaces;
using System.Collections.ObjectModel;

namespace MDRNJfitness.ViewModels
{
    /// <summary>Buy gear with gold.</summary>
    public partial class ShopViewModel : BaseViewModel
    {
        private readonly IGearService _gear;

        public ObservableCollection<ShopItemCard> Items { get; } =
            new(GearCatalog.All.OrderBy(i => GearCatalog.DisplayOrder.ToList().IndexOf(i.Slot)).ThenBy(i => i.Price)
                .Select(i => new ShopItemCard(i)));

        [ObservableProperty] private int _gold;
        [ObservableProperty] private string _goldText = "0";
        private bool _unlimited;
        public bool HasNothingForSale => Items.Count == 0;

        public ShopViewModel(IGearService gear)
        {
            _gear = gear;
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
                card.Owned = inventory.OwnedItemIds.Contains(card.Item.Id);
                card.CanAfford = !card.Owned && (_unlimited || Gold >= card.Item.Price);
            }
        }

        [RelayCommand]
        public async Task BuyAsync(ShopItemCard card)
        {
            if (card.Owned) return;
            if (!card.CanAfford)
            {
                await Shell.Current.DisplayAlertAsync("Not enough gold",
                    $"The {card.Item.Name} costs {card.Item.Price} gold. You have {Gold}. Train, finish quests and win duels to earn more!", "OK");
                return;
            }

            var sure = await Shell.Current.DisplayAlertAsync("Buy item?",
                _unlimited ? $"Add the {card.Item.Name} to your Collection? (Free for admins)"
                           : $"Buy the {card.Item.Name} for {card.Item.Price} gold?", "Buy", "Cancel");
            if (!sure) return;

            await RunSafeAsync(async () =>
            {
                Show(await _gear.BuyAsync(card.Item.Id));
                var equip = await Shell.Current.DisplayAlertAsync("Purchased!",
                    $"The {card.Item.Name} is in your Collection. Wear it now?", "Equip", "Later");
                if (equip)
                    await _gear.EquipAsync(card.Item.Slot, card.Item.Id);
            });
        }
    }
}
